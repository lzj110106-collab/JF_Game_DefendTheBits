using UnityEngine;
using System.Collections;

public class InteractReward : InteractObject
{
    public enum RewardType
    {
        CoinSmall,
        CoinLarge,
        Cash,
        Crown,
        Trinket
    };

    public enum State
    {
        Launch,
        WaitForInteraction,
        Collection,
        WaitForFPX
    };

    GameObject sourcePrefab;

    State currentState;
    float currentStateTime;

    public RewardType type;
    public GameObject meshNode;

    [Header("Trinkets")]
    public string trinketID;
    public Sprite trinketIcon;

    [Header("Coins, etc")]
    public int defaultValue;
    public int value { get; private set; }
    public GameObject subRewardPrefab;
    public int subRewardCount = 0;

    [Header("Collection")]
    public GameObject pfxPrefab;
    public float timeOut = 30.0f;
    public float timeOutFlashDuration = 2.0f;
    public float timeOutFlashInterval = 0.5f;
    public float collectionSpeed = 1000.0f;
    public bool autoCollect = false;

    [Header("Spawn Animation")]
    public float gravity = 10.0f;
    public float launchAngle = 45.0f;
    public float speedMultiplier = 2.0f;
    ParabolaHelper parabolaHelper = new ParabolaHelper();

    bool finishedUpdating;


    public void Update()
    {
        //all reward stuff happens in HUD time, so as not
        //to be affected by fast forward stuff

        if (!World.instance.paused && !finishedUpdating)
        {
            currentStateTime += Time.deltaTime;

            switch (currentState)
            {
                case State.Launch: UpdateStateLaunch(); break;
                case State.WaitForInteraction: UpdateStateWaitForInteraction(); break;
                case State.Collection: UpdateStateCollection(); break;
            }

            OnTouch();
        }
    }

    public override bool UpdateTick()
    {
        if (finishedUpdating)
        {
            InteractObjectPool.Return(sourcePrefab, gameObject);
            return false;
        }

        return true;
    }

    public override void OnSwiped(Vector3 swipeDirection)
    {
    }

    public override void OnTouch()
    {
        //dont cause this state to get triggered over and over
        if (currentState == State.WaitForInteraction && (InputUtil.MousePressed() || InputUtil.MouseDrag()))
        {
            Ray ray = Camera.main.ScreenPointToRay(InputUtil.MousePosition());
            RaycastHit info;

            if (interactCollider.Raycast(ray, out info, float.MaxValue))
            {
                Collect(defaultValue);

                if (InputUtil.MousePressed())
                    World.enableTowerSelection = false; //eat the input event
            }
        }
    }

    public void Collect(int collectionValue)
    {
        SetState(State.Collection);

        if (subRewardCount > 0)
        {
            for (int i = 0; i < subRewardCount; ++i)
                World.AddInteractReward(subRewardPrefab, transform.position);

            finishedUpdating = true;
        }
        else
        {
            SetState(State.Collection);
            value = collectionValue;
        }

        //make sure we switch to the correct trinket type for collection as
        //levels can have multiple trinket reward types
        switch (type)
        {
            case RewardType.Trinket:
                CurrencyDisplay.SetTrinketID(trinketID);
                AchievementDatabase.CollectTrinket(trinketID);
                break;
            case RewardType.CoinLarge:
            case RewardType.CoinSmall:
                AchievementDatabase.CollectCoin();
                AudioController.Play("Tower_Duck_Collect");
                break;
            case RewardType.Cash:
                AudioController.Play("Tower_Duck_Collect");
                break;
            default:
                break;
        }

        PFXPool.Play(pfxPrefab, transform.position);
    }

    public override void OnSpawned(GameObject prefab, Vector3 worldLocation)
    {
        gameObject.SetActive(true);
        meshNode.SetActive(true);

        sourcePrefab = prefab;
        finishedUpdating = false;

        transform.position = worldLocation;
        var launchPosition = worldLocation;
        var launchTarget = worldLocation;

        value = defaultValue;

        //attempt to launch the reward to an empty tile.
        if (!Landscape.FindRandomTileOfType(worldLocation, TileFlag.HasPath_RuntimeAssigned, ref launchTarget, true))
        {
            //couldnt find a valid destination, so pick anywhere.
            launchTarget = Landscape.FindRandomTile(worldLocation, true);
        }

        parabolaHelper.Init(launchPosition, launchTarget, launchAngle, gravity);

        SetState(State.Launch);
    }

    void SetState(State newState)
    {
        currentState = newState;
        currentStateTime = 0.0f;

        //make sure to make the mesh visible during collection
        if (newState == State.Collection && meshNode != null)
            meshNode.SetActive(true);
    }

    void UpdateStateLaunch()
    {
        if (parabolaHelper.Update(World.frameTime * speedMultiplier))
        {
            SetState(State.WaitForInteraction);
            transform.position = parabolaHelper.targetPosition;
        }
        else
        {
            transform.position = parabolaHelper.currentPosition;
        }
    }

    void UpdateStateWaitForInteraction()
    {
        if (currentStateTime >= timeOut)
        {
            if (autoCollect)
            {
                SetState(State.Collection);
            }
            else
            {
                //time outs just kill the reward now, no auto collect
                finishedUpdating = true;
            }
        }
        else if (currentStateTime >= timeOut - timeOutFlashDuration)
        {
            //flash the mesh node on and off if there is one
            if (meshNode)
            {
                float remaining = currentStateTime - (timeOut - timeOutFlashDuration);
                meshNode.SetActive((int)(remaining / timeOutFlashInterval) % 2 == 0);
            }
        }
    }

    void UpdateStateCollection()
    {
        var destObject = HUD.GetTapRewardDestination(type);
        if (destObject == null)
        {
            //nowhere to animate to, just auto-collect to prevent null pointer issues
            HUD.AddTapReward(type, value, trinketID);
            finishedUpdating = true;
            return;
        }

        //project the coin world position into screen space. if its close enough
        //to the destination, then we are done.
        var screenSpace = MainCameraController.WorldToScreen(transform.position);
        var screenSpaceDest = UICameraController.CalcScreenSpacePosition(destObject);

        float distanceFromCamera = screenSpace.z;
        screenSpace.z = 0.0f;

        float movement = collectionSpeed * Time.deltaTime; //use HUD timing
        if (movement >= Vector3.Magnitude(screenSpaceDest - screenSpace))
        {
            HUD.AddTapReward(type, value, trinketID);
            finishedUpdating = true;
        }
        else
        {
            //adjust position in screen space 
            screenSpace += Vector3.Normalize(screenSpaceDest - screenSpace) * movement;
            screenSpace.z = distanceFromCamera;

            //and reproject back into the world
            transform.position = MainCameraController.ScreenToWorld(screenSpace);
        }
    }
}
