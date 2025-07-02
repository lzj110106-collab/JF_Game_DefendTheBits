using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public enum QuestTypes
{
	Defeat,
	Collect
}

public class QuestScreen : MonoBehaviour 
{
	public GameObject questButtonPrefab;
	public GameObject buttonContainer;

	[Header("UI")]
	public ParticleSystem claimPFX;
	public ScrollRect scrollRect;
	public ShrinkScrollEdgeItems shrinkScroll;
	public Text completionText;

	public GameObject defeatAlert;
	public GameObject collectAlert;
	public GameObject beatAlert;
	public GameObject upgradeAlert;

	public List<QuestButton> questItems { get; private set; }
	QuestButton unlockSequenceSourceButton = null;

	Achievement.Type currentCategory;

	void Awake()
	{
		questItems = new List<QuestButton>();
	}

	void Start()
	{
		Populate(Achievement.Type.Defeat, true);
		RefreshQuestCounter();
	}

    private void Update()
    {
#if PXY_YUNCE
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("测试退出按钮");
            //Application.Quit();
            PXY_AndroidBuy.Instance.ExitGame();
            //ExitGame.Instance.SetSelfTrue();
        }
#endif
    }

    void OnEnable()
	{
		//ignore the sort and stuff during the unlock sequence.
		if (unlockSequenceSourceButton != null)
			return;

		//make sure freshly completed achievements are first in the list
		SortAchievements();
		RefreshQuestCounter();
	}

	public void CategoryButtonPressed_Defeat()
	{
		Populate(Achievement.Type.Defeat);
	}

	public void CategoryButtonPressed_Collect()
	{
		Populate(Achievement.Type.Collect);
	}

	public void CategoryButtonPressed_CompleteLevel()
	{
		Populate(Achievement.Type.CompleteLevel);
	}

	public void CategoryButtonPressed_Upgrade()
	{
		Populate(Achievement.Type.Upgrade);
	}

	void Populate(Achievement.Type category, bool force = false)
	{
		if (currentCategory != category || force)
		{
			questItems.Clear();
			UnityUtil.DestroyAllChildren(buttonContainer);

			for (var i = 0; i < AchievementDatabase.orderedAchievements.Count; ++i)
			{
				var data = AchievementDatabase.orderedAchievements[i];
				if (data.type == category)
				{
					var item = GameObject.Instantiate(questButtonPrefab);
					item.transform.SetParent(buttonContainer.transform, false);

					var questButton = item.GetComponent<QuestButton>();
					questButton.Initialise(data, this);

					questItems.Add(questButton);
				}
			}

			SortAchievements();
			RefreshCategoryAlerts();

			currentCategory = category;
		}

		//reset the scroll bar
		scrollRect.horizontalNormalizedPosition = 0.0f;
	}

	void RefreshQuestCounter()
	{
		if (questItems != null)
		{
			var completed = 0;
			for (var i = 0; i < questItems.Count; ++i)
				completed += questItems[i].data.isCompleted ? 1 : 0;

			completionText.text = completed.ToString() + "/" + questItems.Count.ToString();
		}
	}

	void SortAchievements()
	{
		for (var i = 0; i < questItems.Count; ++i)
			questItems[i].transform.SetParent(null, false);
		
		questItems.Sort((item0, item1) => {

			var ach0 = item0.data;
			var ach1 = item1.data;

			//sort the objects so that quests awaiting collection appear first
			bool canCollect0 = ach0.isCompleted && !ach0.isCollected;
			bool canCollect1 = ach1.isCompleted && !ach1.isCollected;

			if (canCollect0)
			{
				if (canCollect1)
					return ach0.guid.CompareTo(ach1.guid);

				return -1;
			}
			else if (canCollect1)
			{
				return 1;
			}

			//now sort by achievement type. this needs to match the button
			//order in the UI. the enum has been reordered to match in 
			//so that this sort function doesnt get too insane.
			if (ach0.type == ach1.type)
				return ach0.guid.CompareTo(ach1.guid);

			return ach0.type.CompareTo(ach1.type);
		});
			
		//reparent in the correct order
		for (var i = 0; i < questItems.Count; ++i)
			questItems[i].transform.SetParent(buttonContainer.transform, false);

		shrinkScroll.Initialize();
	}



	public void UnlockTower(QuestButton questButton)
	{
		unlockSequenceSourceButton = questButton;

		var unlock = UserInterface.GetTowerUnlockScreen();
		unlock.Add(TowerLoader.GetTowerPrefab(questButton.data.rewardTypeName));
		unlock.Show(OnUnlockSequenceComplete);

		gameObject.SetActive(false);
	}

	public void OnUnlockSequenceComplete()
	{
		//bring back the foreground
		gameObject.SetActive(true);
		GetComponent<Panel>().PerformDefaultOnTransition();	

		//restoring the background. crazy double set because the navigation stack will
		//end up being quest/quest and the navigation back to the main screen will break
		UISceneManager.instance.SetScene(UISceneIDs.MainMenu);
		UISceneManager.instance.uiSceneCamera.SetCameraAnimation(CameraStates.MainMenu);
		UISceneManager.instance.uiSceneCamera.SetCameraAnimation(CameraStates.Quests);

		unlockSequenceSourceButton.PlayUnlockAnimation();
		unlockSequenceSourceButton = null;

		RefreshCategoryAlerts();
	}

	public void PlayClaimPFX(Vector3 _worldPosition)
	{
		claimPFX.transform.position = _worldPosition;
		claimPFX.Play();
	}

	public void RefreshCategoryAlerts()
	{
		collectAlert.SetActive(false);
		defeatAlert.SetActive(false);
		upgradeAlert.SetActive(false);
		beatAlert.SetActive(false);

		for (var i = 0; i < AchievementDatabase.orderedAchievements.Count; ++i)
		{
			var data = AchievementDatabase.orderedAchievements[i];

			// TODO display Alert !s for the category buttons
			if (data.isCompleted && !data.isCollected)
			{
				if (data.type == Achievement.Type.Collect)
					collectAlert.SetActive(true);
				else if (data.type == Achievement.Type.Defeat)
					defeatAlert.SetActive(true);
				else if (data.type == Achievement.Type.Upgrade)
					upgradeAlert.SetActive(true);
				else
					beatAlert.SetActive(true);
			}
		}
	}
}
