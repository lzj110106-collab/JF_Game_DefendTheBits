using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class HUD : MonoBehaviour
{
    public static HUD instance;

    public GameObject shareBtn;

    public GameObject towers;

    public GameObject Shop;


    LevelData level;

    [Header("Behaviour")]
    public bool addLockedTowersToBuildMenu = false;
    public bool showInfoPanelOnTowerPlacement = false;
    public bool showPlacementConfirmationDialog = true;
    public bool showFPS = false;

    [Header("UI")]
    public float hitFlashDuration;
    public Color hitFlashColor;
    public CameraShake.Shake takeDamageShake;

    public ShrinkScrollEdgeItems shrinkScrollEdgeItems;
    public TowerBuildMenu buildMenu;

    public Animator goldAnimator;
    public Animator livesAnimator;
    public Text goldDisplay;
    public Text livesDisplay;

    public GameObject btnSpeed;
    public GameObject btnStartWave;
    public GameObject btnHints;

    public GameObject ADPanel;

    public HintsPanelTriggerButton hintButtonTrigger;
    public Animator btnHintAnimator;

    public int goldRemaining { get; private set; }
    public int livesRemaining { get; private set; }

    /// <summary>
    /// 是否弹出过广告了
    /// 第一次死亡会弹出广告，广告增加了10颗心后继续游戏
    /// 再次死亡后不会弹出广告
    /// </summary>
    private bool showedAD;

    public GameObject healthBarPrefab;
    Dictionary<Character, HealthBar> healthBars;
    GameObject healthBarsParent;

    bool resumeFastForward = false;
    //bool lockPlayButton = false;

    PlayerAbility[] abilities = null;

    public Dictionary<string, int> trinketsThisRound { get; private set; }

    void Awake()
    {
        healthBarsParent = new GameObject("health_bar");
        healthBarsParent.transform.SetParent(transform, false);
        healthBars = new Dictionary<Character, HealthBar>();

        instance = this;
        btnStartWave.SetActive(true);
        btnSpeed.SetActive(false);

        abilities = GetComponentsInChildren<PlayerAbility>(true);
        trinketsThisRound = new Dictionary<string, int>();
    }

    void OnDestroy()
    {
        instance = null;
    }

    private void OnEnable()
    {
        Shop.SetActive(true);

    }

    void Update()
    {
        if (UserInterface.CanTriggerHintsPanel())
        {
            if (HintsPanel.hintsToDisplay.Last.Value.forceDisplay)
            {
                UserInterface.TriggerHintsPanel();
                btnHints.SetActive(false);
            }
            else if (!btnHints.activeSelf)
            {
                hintButtonTrigger.Show();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("返回主界面");
            PauseMenu.instance.ReturnToMainMenu();

        }
    }

    public void Initialise(LevelData data)
    {
        //if (LocManager.isInChina())
        //{
        //    shareBtn.SetActive(true);
        //}
        //else
        //{
            shareBtn.SetActive(false);
        //}

        level = data;

        showedAD = false;

        goldRemaining = data.gold;
        livesRemaining = data.lives;
        trinketsThisRound.Clear();

        SetLives(livesRemaining);
        SetGold(goldRemaining);

        for (int i = 0; i < abilities.Length; ++i)
            abilities[i].Restart();

        //show the first available trinket for this level. this will
        //switch to the other trinket types as they get collected
        if (level.trinketIds != null && level.trinketIds.Length > 0)
            CurrencyDisplay.ShowTrinketDisplay(level.trinketIds[0]);

        buildMenu.Initialise(data, addLockedTowersToBuildMenu);
        shrinkScrollEdgeItems.Initialize();

        CurrencyDisplay.HideStarDisplay();
        CurrencyDisplay.HideStoreButton();

        resumeFastForward = false;
        btnHints.SetActive(false);
        LockStartWaveButton();

        //make sure the play button is visible, the HUD is
        //not created from scratch each time the game is run
        NewWaveReady();

        Show();
    }

    public void StartWave()
    {
        //make sure the speed button is visible, dont modify the speed scale
        btnStartWave.SetActive(false);
        btnSpeed.SetActive(true);
        AudioController.Play("Start");
    }

    public void NewWaveReady()
    {
        // Hide Speed button show Start button
        btnStartWave.SetActive(true);
        btnSpeed.SetActive(false);
    }

    public static void OnWaveLaunched()
    {
        instance.StartWave();

        if (instance.resumeFastForward)
        {
            instance.btnStartWave.SetActive(false);
            instance.btnSpeed.SetActive(true);

            SpeedButtonControl.Instance.ToggleSpeed(true);
        }
    }

    public static void OnWaveComplete(int waveNumber, int waveReward)
    {
        //this is a bit gross, but i wanted to keep all the different
        //options for the end of wave behaviour in the same script
        var waves = instance.GetComponent<WavesHUD>();
        if (waves != null)
        {
            instance.resumeFastForward = false;
            if (waves.resumeFastForwardOnWaveLaunch)
                instance.resumeFastForward = World.instance.timeScale > 1.0f;

            if (!waves.enableWaveAutoProgression)
            {
                instance.NewWaveReady();
                World.instance.SetTimescale(1.0f);
            }
            else if (!waves.enableFastForwardBetweenWaves)
            {
                SpeedButtonControl.ResetSpeed();
                World.instance.SetTimescale(1.0f);

                //ensure the play button is showing if FF is
                //being killed between waves
                instance.btnStartWave.SetActive(true);
                instance.btnSpeed.SetActive(false);
            }
        }
    }


    public void DisplayAddLives(int lives)
    {
        livesAnimator.Play("Add", 0, 0f);
        livesDisplay.text = lives.ToString();
    }

    public void DisplaySubtractLives(int lives)
    {
        livesAnimator.Play("Subtract", 0, 0f);
        livesDisplay.text = lives.ToString();
    }

    public void DisplayAddGold(int amount)
    {
        goldAnimator.Play("Add", 0, 0f);
        goldDisplay.text = amount.ToString();
    }

    public void DisplaySubtractGold(int amount)
    {
        goldAnimator.Play("Subtract", 0, 0f);
        goldDisplay.text = amount.ToString();
    }

    public void Show()
    {
        PanelManager.Instance.EnableScreen(PanelID.HUD);
    }

    public void Hide()
    {
        PanelManager.Instance.DisableScreen(PanelID.HUD);
    }

    public void AddHealthBar(Character character)
    {
        if (!healthBars.ContainsKey(character))
        {
            var instance = GameObject.Instantiate(healthBarPrefab).GetComponent<HealthBar>();
            instance.Initialise(UserInterface.GetCanvas(), character);
            instance.transform.SetParent(healthBarsParent.transform, false);

            healthBars.Add(character, instance);
        }
    }

    public void RemoveHealthBar(Character character)
    {
        if (healthBars.ContainsKey(character))
        {
            var instance = healthBars[character];
            Destroy(instance.gameObject);

            healthBars.Remove(character);
        }
    }

    public void DealDamageToPlayer(int amount)
    {
        if (livesRemaining <= amount)
        {
            livesRemaining = 0; //make sure to zero this out so the star calcs work
            //print("生命值为0");
            if (!showedAD /*&& IronSource.Agent.isRewardedVideoAvailable()*/)
            {
                //print("广告可用，暂停游戏");
                //暂停游戏
                HUD.instance.Hide();
                World.instance.TogglePause();
                UserInterface.GetFTUEGuide().OnPause();
                AudioController.PauseCategory("Music_Game");
                //弹出广告窗口
                //ADPanel.GetComponent<ADPanel>().ShowADPanel();
                ADPanel.SetActive(true);
            }
            else
            {
                NoAD();
            }

        }
        else
        {
            livesRemaining -= amount;
            DisplaySubtractLives(livesRemaining);
            ScreenCover.Instances[(int)ScreenCoverIDs.Hit].Flash(hitFlashDuration, hitFlashColor);
            CameraShake.instance.TriggerShake(takeDamageShake);
        }
    }

    /// <summary>
    /// 拒绝广告或不再弹出广告
    /// </summary>
    public void NoAD()
    {
        World.OnDefeat();
        GameState.TriggerEOR();
    }

    /// <summary>
    /// 看完激励广告后的回调函数
    /// 1元获得20点生命
    /// </summary>
    public void RewardAD()
    {

        ////速度要降到最慢
        btnSpeed.GetComponent<SpeedButtonControl>().ToggleSpeed(false);
        showedAD = true;
        AddLives(20);
        HUD.instance.Show();
        World.instance.TogglePause();
        FTUE.OnPauseResume();
        UserInterface.GetFTUEGuide().OnResume();
        AudioController.UnpauseCategory("Music_Game");
        //弹出广告窗口
        //ADPanel.GetComponent<ADPanel>().HideADPanel();
        ADPanel.SetActive(false);


    }

    public bool CanAfford(int price)
    {
        return price <= goldRemaining;
    }

    public bool MakePurchase(int price)
    {
        if (CanAfford(price))
        {
            goldRemaining -= price;
            HUD.instance.DisplaySubtractGold(goldRemaining);

            return true;
        }


        return false;
    }

    public void MakeSale(int purchasePrice)
    {
        goldRemaining += purchasePrice;
        HUD.instance.DisplayAddGold(goldRemaining);
    }

    public void AddGold(int amount)
    {
        //dont animate for 0 rewards
        if (amount > 0)
        {
            goldRemaining += amount;
            HUD.instance.DisplayAddGold(goldRemaining);
        }
    }

    public void AddLives(int amount)
    {
        livesRemaining += amount;
        HUD.instance.DisplayAddLives(livesRemaining);
    }

    public void SetGold(int amount)
    {
        goldRemaining = amount;
        HUD.instance.DisplayAddGold(goldRemaining);
    }

    public void SetLives(int amount)
    {
        livesRemaining = amount;
        HUD.instance.DisplayAddLives(livesRemaining);
    }

    public static void AddTapReward(InteractReward.RewardType type, int value, string trinketId)
    {
        if (type == InteractReward.RewardType.CoinSmall ||
            type == InteractReward.RewardType.CoinLarge)
        {
            instance.AddGold(value);
        }
        else if (type == InteractReward.RewardType.Cash)
        {
            //TODO: might need to reflect this in the UI somehow.
            SaveData.AddCash(value);
        }
        else if (type == InteractReward.RewardType.Trinket)
        {
            SaveData.AddTrinket(trinketId);
            CurrencyDisplay.OnTrinketAdded();

            IncreaseTrinketTally(trinketId, 1);
        }
    }

    public static void IncreaseTrinketTally(string trinketID, int amount)
    {
        if (instance != null)
        {
            int current = 0;
            if (instance.trinketsThisRound.TryGetValue(trinketID, out current))
                instance.trinketsThisRound[trinketID] = current + amount;
            else
                instance.trinketsThisRound.Add(trinketID, amount);
        }
    }

    public static GameObject GetTapRewardDestination(InteractReward.RewardType type)
    {

        if (type == InteractReward.RewardType.CoinSmall ||
            type == InteractReward.RewardType.CoinLarge)
        {
            return instance.goldAnimator.gameObject;
        }
        else if (type == InteractReward.RewardType.Trinket)
        {
            return CurrencyDisplay.GetTrinketCollectionDestination();
        }
        else if (type == InteractReward.RewardType.Cash)
        {
            return CurrencyDisplay.GetCashCollectionDestination();
        }

        return null;
    }

    public static void ShowHint(int waveNumber)
    {
        HintsPanel.AddHint(HintsDatabase.GetLevelHintData(instance.level.identifier, waveNumber));
    }

    public static void UpdateAbilities()
    {
        for (int i = 0; i < instance.abilities.Length; ++i)
            if (instance.abilities[i].inProgress)
                instance.abilities[i].UpdateTick();
    }

    public static void LockStartWaveButton()
    {
        if (instance != null)
            instance.btnStartWave.GetComponent<StartWaveButton>().Lock();
    }

    public static void UnlockStartWaveButton(bool _highlighted)
    {
        if (instance != null)
            instance.btnStartWave.GetComponent<StartWaveButton>().Unlock(_highlighted);
    }
}
