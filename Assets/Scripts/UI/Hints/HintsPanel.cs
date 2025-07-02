using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintsPanel : MonoBehaviour 
{
	public static HintsPanel instance;
	public static LinkedList<HintData> hintsToDisplay = new LinkedList<HintData>();

	public Text headerText;
	public Text subHeaderText;
	public Text dot1Text;
	public Text dot2Text;
	public Text dot3Text;
	public Text bodyText;
	public Text bodyTextBasicEnemy;
	public Text bodyTextBigEnemy;

	public Image imageMain;
	public Image imageMainEnemy;
	public Image[] weaknessImages;

	public GameObject dot1Hierarchy;
	public GameObject dot2Hierarchy;
	public GameObject dot3Hierarchy;

	public Image dropIcon;
	public Text dropText;

	[Header("Toggleable Hierarchies")]
	public GameObject dropsHierarchy;
	public GameObject headerHintHierarchy;
	public GameObject headerNewEnemyHierarchy;
	public GameObject okayButtonHierarchy;
	public GameObject nextButtonHierarchy;
	public GameObject infoEnemyHierarchy;
	public GameObject infoBasicEnemyHierarchy;
	public GameObject infoBigEnemyHierarchy;
	public GameObject infoNormalHierarchy;

	[Header("Circle Image Hierarchies")]
	public GameObject circleStandard;
	public GameObject circleEnemy;
	public GameObject circleDragTower;
	public GameObject circleStartWave;
	public GameObject circleUpgradeTower;
	public GameObject circleTrinkets;

	HintData currentHint;
	HintTypes hintType;
	HintCircleTypes circleType;

	bool isVisible = false;
		
	void Awake(){ instance = this; }

	public void Trigger()
	{
		currentHint = hintsToDisplay.Last.Value;
		hintsToDisplay.RemoveLast();

		SetText(currentHint.headerLocID, headerText);
		SetText(currentHint.textFooterLocID, subHeaderText); // TODO add subheader to csv
		SetDotText(currentHint.textDot1LocID, dot1Text, dot1Hierarchy);
		SetDotText(currentHint.textDot2LocID, dot2Text, dot2Hierarchy);
		SetDotText(currentHint.textDot3LocID, dot3Text, dot3Hierarchy);
		SetText(currentHint.textDot1LocID, bodyText);
		SetText(currentHint.textDot1LocID, bodyTextBasicEnemy);
		SetText(currentHint.textDot1LocID, bodyTextBigEnemy);

		SetHintType(currentHint.hintType, currentHint.circleType);

		imageMain.gameObject.SetActive(currentHint.imgPortrait != null);
		imageMainEnemy.sprite = imageMain.sprite = currentHint.imgPortrait;

		// TODO: get trinket id from CSV
		SetTrinket(currentHint.rewards);

		SetWeaknesses(currentHint.weaknesses);
	
		var panel = GetComponent<Panel>();
		panel.SetTransitionCompleteCallback(OnPanelTransitionComplete);
		panel.PerformDefaultOnTransition();

		// TODO: switch between "Next" and "Okay" depending on whether more hints remain
		bool moreHintsToShow = false;
		okayButtonHierarchy.SetActive(!moreHintsToShow);
		nextButtonHierarchy.SetActive(moreHintsToShow);

		if (World.instance != null)
			World.instance.TogglePause();
	}

	void OnPanelTransitionComplete(Panel targetPanel, string transitionName)
	{
		if (transitionName == targetPanel.defaultOffTransition)
		{
			targetPanel.SetTransitionCompleteCallback(null);
			isVisible = false;
		}
	}

    static bool hitShow;

	static void SetHintType(HintTypes _hintType, HintCircleTypes _circleType)
	{
        hitShow = (_hintType == HintTypes.Hint || _circleType == HintCircleTypes.BasicEnemy || _circleType == HintCircleTypes.BigEnemy);
        instance.headerHintHierarchy.SetActive(_hintType == HintTypes.Hint || _circleType == HintCircleTypes.BasicEnemy || _circleType == HintCircleTypes.BigEnemy);
		instance.headerNewEnemyHierarchy.SetActive(_hintType == HintTypes.NewEnemy);

        //print(hitShow);
        if (hitShow)
            instance.headerNewEnemyHierarchy.SetActive(false);

        instance.infoNormalHierarchy.SetActive(_hintType == HintTypes.Hint && !( _circleType == HintCircleTypes.BasicEnemy || _circleType == HintCircleTypes.BigEnemy));
		instance.infoEnemyHierarchy.SetActive(_hintType == HintTypes.NewEnemy && !( _circleType == HintCircleTypes.BasicEnemy || _circleType == HintCircleTypes.BigEnemy));
		instance.infoBasicEnemyHierarchy.SetActive(_circleType == HintCircleTypes.BasicEnemy);
		instance.infoBigEnemyHierarchy.SetActive(_circleType == HintCircleTypes.BigEnemy);

		instance.circleStandard.SetActive(_circleType == HintCircleTypes.Standard);
		instance.circleEnemy.SetActive(_circleType == HintCircleTypes.Enemy || _circleType == HintCircleTypes.BasicEnemy || _circleType == HintCircleTypes.BigEnemy);
		instance.circleDragTower.SetActive(_circleType == HintCircleTypes.DragTower);
		instance.circleStartWave.SetActive(_circleType == HintCircleTypes.StartWave);
		instance.circleUpgradeTower.SetActive(_circleType == HintCircleTypes.UpgradeTower);
		instance.circleTrinkets.SetActive(_circleType == HintCircleTypes.Trinkets);
	}

	static void SetWeaknesses(string[] _weaknessIDs)
	{
		for (int i = 0; i < instance.weaknessImages.Length; ++i)
		{
			if (_weaknessIDs == null || i >= _weaknessIDs.Length || string.IsNullOrEmpty(_weaknessIDs[i]))
			{
				instance.weaknessImages[i].gameObject.SetActive(false);
			}
			else if (TowerIconDatabase.IsValidIcon(_weaknessIDs[i]) && i < instance.weaknessImages.Length - 1)
			{
				instance.weaknessImages[i].sprite = TowerIconDatabase.GetIcon(_weaknessIDs[i]);

				var text = instance.weaknessImages[i].GetComponentInChildren<Text>(true);
				if (text != null)
					LocManager.Assign(text, TowerIconDatabase.GetNameID(_weaknessIDs[i]));

				if (!instance.weaknessImages[i].gameObject.activeSelf)
					instance.weaknessImages[i].gameObject.SetActive(true);
			}
		}
	}

	static void SetTrinket(string _trinketID)
	{
		var trinketPrefab = TrinketDatabase.GetPrefab("trinket_"+_trinketID);
		if (trinketPrefab != null)
		{
			instance.dropText.text = _trinketID+" trinkets";
			instance.dropIcon.sprite = trinketPrefab.GetComponent<InteractReward>().trinketIcon;
			instance.dropsHierarchy.SetActive(true);
		}
		else
			instance.dropsHierarchy.SetActive(false);
	}

	public void OnPanelDismissed()
	{
		if (World.instance != null)
			World.instance.TogglePause();

		if (currentHint != null)
		{
			if (currentHint.onHintDismissed != null)
			{
				currentHint.onHintDismissed();
				currentHint.onHintDismissed = null;
			}

			currentHint = null;
		}
	}

	public static void AddHint(HintData hintData)
	{
		if (hintData != null)
			hintsToDisplay.AddLast(hintData);
	}

	public static void ClearHints()
	{
		hintsToDisplay.Clear();
	}

	static void SetText(string stringID, Text location)
	{
		if (!string.IsNullOrEmpty(stringID))
		{
			var translation = LocManager.Translate(stringID);
			if (translation != stringID && !string.IsNullOrEmpty(translation))
			{
				location.gameObject.SetActive(true);
				LocManager.Assign(location, stringID);

				return;
			}
		}

		location.gameObject.SetActive(false);
	}

	static void SetDotText(string stringID, Text location, GameObject dotHierarchy)
	{
		if (!string.IsNullOrEmpty(stringID))
		{
			var translation = LocManager.Translate(stringID);
			if (translation != stringID && !string.IsNullOrEmpty(translation))
			{
				dotHierarchy.SetActive(true);
				LocManager.Assign(location, stringID);

				return;
			}
		}

		dotHierarchy.SetActive(false);
	}
}
