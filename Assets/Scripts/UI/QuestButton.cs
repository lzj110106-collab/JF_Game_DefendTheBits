using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestButton : MonoBehaviour 
{
	public Achievement data { get; private set; }
	QuestScreen questScreen;

	public GameObject questRewardTowerPrefab;
	public GameObject questRewardResourcePrefab;
	public Transform questRewardContainer;
	public QuestTypes questType;
	public Animator questAnim;
	public Text txtHeading;
	public Text txtHeadingComplete;
	public Text txtDescription;
	public Text txtProgress;
	public Image questTypeIcon;
	public Slider progressSlider;
	public bool questComplete;

	[Header("Rewards")]
	public GameObject rewardTowerHierarchy;
	public GameObject rewardPremiumHierarchy;
	public Text rewardPremiumAmount;
	public Image rewardTowerIcon;
	public Image rewardPremiumIcon;

	public void Initialise(Achievement achievement, QuestScreen parent)
	{
		data = achievement;
		name = achievement.guid;
		questScreen = parent;

		LocManager.Assign(txtHeading, data.headerID);
		LocManager.Assign(txtHeadingComplete, data.headerID);

		data.AssignDescription(txtDescription);

		Refresh();
	}

	void OnEnable()
	{
		if (data != null)
			Refresh ();
	}

	void Refresh()
	{
		progressSlider.value = AchievementDatabase.AchievementProgress(data);
		txtProgress.text = AchievementDatabase.AchievementProgressText(data);

		questAnim.Play(data.isCompleted ? (data.isCollected ? "Claimed" : "Complete") : "InProgress", 0, 0.0f);


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
		}

		var iconName = data.type == Achievement.Type.Collect ? "quest_collect" :
					   data.type == Achievement.Type.Defeat ? "quest_defeat" :
					   data.type == Achievement.Type.Upgrade ? "quest_upgrade" :
					   "quest_complete";
		
		questTypeIcon.sprite = TowerIconDatabase.GetIcon(iconName);
	}

	public void QuestButtonPressed()
	{
		if (data.isCompleted && !data.isCollected)
		{
			questScreen.PlayClaimPFX(transform.position);
            GetComponent<PlayAudio>().PlayClip("UI_EOR_Star");
            if (data.rewardType == Achievement.RewardType.Tower)
			{
				questScreen.UnlockTower(this);
			}
			else
			{
				PlayUnlockAnimation();
			}
		}
	}

	public void PlayUnlockAnimation()
	{
		AchievementDatabase.CollectAchievement(data);
		Refresh();
		questAnim.Play("Pressed", 0, 0.0f);
		questScreen.RefreshCategoryAlerts();
	}
}
