using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class QuestPopup : MonoBehaviour 
{
	public QuestTypes questType;
	public Text txtHeading;
	public Text txtHeadingComplete;
	public Text txtDescription;
	public Image questTypeIcon;
	public bool questComplete;

	[Header("Rewards")]
	public GameObject rewardTowerHierarchy;
	public GameObject rewardPremiumHierarchy;
	public Text rewardPremiumAmount;
	public Image rewardTowerIcon;
	public Image rewardPremiumIcon;

	Sprite premiumCashSprite;

	public float displayDuration = 5.0f;
	float displayRemaining;
	bool isVisible = false;

	public void Awake()
	{
		//cache this. the panel gets reused over and over
		premiumCashSprite = rewardPremiumIcon.sprite;
	}

	public void Initialise(Achievement data)
	{
		//trigger the panel animation and set up a time-out value
		{
			gameObject.SetActive(true);
			// Enable screen and force transition complete since panel can be immediately dismissed
			PanelManager.Instance.EnableScreen(PanelID.QuestPopup);
			PanelManager.Instance.panels[(int)PanelID.QuestPopup].TransitionComplete();
			AudioController.Play ("UI_Reward");

			displayRemaining = displayDuration;
			isVisible = true;
		}

		LocManager.Assign(txtHeading, data.headerID);
		data.AssignDescription(txtDescription);

		rewardPremiumHierarchy.SetActive(false);
		rewardTowerHierarchy.SetActive(false);

		if (data.rewardType == Achievement.RewardType.Tower)
		{
			var info = TowerLoader.GetTowerInfo(data.rewardTypeName);
			var icon = info[0].prefab.GetComponent<Tower>().icon;

			rewardTowerHierarchy.SetActive(true);
			rewardTowerIcon.sprite = icon;
		}
		else
		{
			rewardPremiumHierarchy.SetActive(true);
			rewardPremiumAmount.text = data.rewardCount.ToString();			

			if (data.rewardType == Achievement.RewardType.Trinket)
			{
				var trinketPrefab = TrinketDatabase.GetPrefab(data.rewardTypeName);
				var trinketIcon = trinketPrefab.GetComponent<InteractReward>().trinketIcon;

				rewardPremiumIcon.sprite = trinketIcon;
			}
			else
			{
				rewardPremiumIcon.sprite = premiumCashSprite;
			}
		}

		var iconName = data.type == Achievement.Type.Collect ? "quest_collect" :
					   data.type == Achievement.Type.Defeat ? "quest_defeat" :
					   data.type == Achievement.Type.Upgrade ? "quest_upgrade" :
					   "quest_defeat";

		questTypeIcon.sprite = TowerIconDatabase.GetIcon(iconName);
	}

	void Update()
	{
		displayRemaining -= Time.deltaTime;

		// Hide popup if display timer reaches 0
		// Or Hide popup if user clicks after forced 1 second of display time
		if ((displayRemaining <= 0.0f || ( InputUtil.MousePressed() && displayRemaining <= displayDuration - 1.0f)) && isVisible)
		{
			PanelManager.Instance.DisableScreen(PanelID.QuestPopup);
			PanelManager.Instance.panels[(int)PanelID.QuestPopup].TransitionComplete();
			isVisible = false;

            //任务完成后，定义一个自定义事件，将当前任务的描述传到后台
            Dictionary<string, object> missionCompleted = new Dictionary<string, object>();
            missionCompleted.Add("Mission", txtDescription.text);
        }
	}
}
