using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

public class FTUE : MonoBehaviour
{
	static FTUE instance;

	public int archerLocationX;
	public int archerLocationY;

	public int axeLocationX;
	public int axeLocationY;

	public int soldierLocationX;
	public int soldierLocationY;

	public enum State
	{
		PlaceArcher,
		PlaceAxe,
		PlaceSoldier,
		UpgradeTower,

		WillShowEnemyTutorial,
		WaitForPugCharacter,

		PugShowHintButton,
		PugShowPlayButton,

		Idle
	}

	//enum only exists so that it is clearer what is happening
	//when ShowTutorial() is called.
	public enum TutorialType
	{
		//wave 0
		Intro_0,
		Intro_1,
		Enemy_0,

		//wave 1
		Viking,
		Enemy_1,

		//wave 3
		Soldier,
		Enemy_2,

		//wave 5
		Upgrade,

		//wave 7
		Enemy_3,

		//wave 9
		Enemy_4,

		None
	}

	State currentState = State.PlaceArcher;
	bool preventTowerPlacement = true;

	TutorialType nextTutorial = TutorialType.None;

	bool canUpgradeTower = false;
	bool canSellTower = false;

	public GameObject placementPrefab;
	GameObject placementInstance;

	void Awake() { instance = this; }
	void OnDestroy() { instance = null; }

	void Start()
	{
		HUD.LockStartWaveButton();

		ShowPugCharacter("ftue_pug_0", TutorialType.Intro_0, instance.OnWelcomePugDismissed);

		for (var i = 0; i <= 4; ++i)
		{
			var data = HintsDatabase.GetFTUEHintData("FTUE_Enemy_" + i.ToString());
			if (data != null)
			{
				data.forceDisplay = false;
//				data.hintType = HintTypes.NewEnemy;
			}
		}

		placementInstance = GameObject.Instantiate(placementPrefab);
		placementInstance.transform.SetParent(transform, false);
		placementInstance.gameObject.SetActive(false);
	}

	public static void UpdateTick()
	{
	}

	public static bool ShouldTriggerFTUE()
	{
		//return PlayerPrefs.GetInt("show_ftue", 1) == 1;

        return ObscuredPrefs.GetInt("show_ftue", 1) == 1;
    }

	public static bool IsActive()
	{
		return instance != null;
	}

	public static bool AllowPlacement(string towerName)
	{
		if (instance != null)
		{
			switch (instance.currentState)
			{
			case State.PlaceArcher:		return towerName == "archer";
			case State.PlaceAxe:		return towerName == "dwarf";
			case State.PlaceSoldier:	return towerName == "soldier";

			default:					return !instance.preventTowerPlacement;
			}
		}

		return true;
	}

	public static void RefreshPlacementMarker()
	{
		if (instance != null)
		{
			switch (instance.currentState)
			{
			case State.PlaceArcher:		AddPlacementMarker(instance.archerLocationX, instance.archerLocationY);		break;
			case State.PlaceAxe:		AddPlacementMarker(instance.axeLocationX, instance.axeLocationY);			break;
			case State.PlaceSoldier:	AddPlacementMarker(instance.soldierLocationX, instance.soldierLocationY);	break;

			default:
				break;
			}
		}
	}

	static void AddPlacementMarker(int tileX, int tileY)
	{
//		RangeObjectPool.PlaceAt(tileX, tileY, RangePlotQuad.MaterialType.WillPlaceValid); //TODO: alternate material?

		var position = Landscape.instance.GetTileCentre(tileX, tileY);
		instance.placementInstance.SetActive(true);
		instance.placementInstance.transform.position = position;
	}

	public static bool ShowAlertHierarchy(string towerName)
	{
		if (instance != null)
		{
			switch (instance.currentState)
			{
			case State.PlaceArcher:		return towerName == "archer";
			case State.PlaceAxe:		return towerName == "dwarf";
			case State.PlaceSoldier:	return towerName == "soldier";
			}
		}

		return false;
	}

	public static bool CanPlaceTowerAtLocation(int tileX, int tileY)
	{
		if (instance != null)
		{
			switch (instance.currentState)
			{
			case State.PlaceArcher:		return tileX == instance.archerLocationX && tileY == instance.archerLocationY;
			case State.PlaceAxe:		return tileX == instance.axeLocationX && tileY == instance.axeLocationY;
			case State.PlaceSoldier:	return tileX == instance.soldierLocationX && tileY == instance.soldierLocationY;
			}
		}

		return true;
	}

	public static void OnTowerPlaced(Tower tower)
	{
		if (instance != null)
		{
			if (instance.currentState == State.PlaceArcher ||
				instance.currentState == State.PlaceAxe ||
				instance.currentState == State.PlaceSoldier)
			{
				if (instance.currentState == State.PlaceArcher)
				{
					ShowTutorial(TutorialType.Enemy_0);
					ShowPugCharacter("ftue_pug_16", TutorialType.None, null);
					instance.currentState = State.PugShowHintButton;
					HUD.instance.hintButtonTrigger.ftuePing = true;
				}
				else
				{
					instance.currentState = State.Idle;
					UserInterface.HideFTUEGuide();
					HUD.UnlockStartWaveButton(false);
				}

				instance.placementInstance.SetActive(false);
			}
		}
	}

	public static void OnTowerUpgraded(Tower tower)
	{
		if (instance != null && instance.currentState == State.UpgradeTower)
		{
			//all forced placements and stuff have occured now. 
			//unlock all the towers for the rest of FTUE
			instance.preventTowerPlacement = false;
			instance.canSellTower = true;
			instance.currentState = State.Idle;

			HUD.UnlockStartWaveButton(false);
			UserInterface.HideFTUEGuide();
		}
	}

	public static bool CanUpgradeTower()
	{
		if (IsActive())
			return instance.canUpgradeTower;

		return true;
	}

	public static bool CanSellTower()
	{
		if (IsActive())
			return instance.canSellTower;

		return true;
	}

	public static void OnWaveLaunched()
	{
		if (instance != null && instance.currentState == State.PugShowPlayButton)
		{
			UserInterface.HideFTUEGuide();
			instance.currentState = State.Idle;
		}
	}

	public static void OnWaveComplete(int waveNumber)
	{
		if (instance != null)
		{
			//need to do this immediately, otherwise there are a couple of frames
			//where it is possible to press the play button. this happens when
			//the completion UI fanfare is occuring.
			if (waveNumber == 0 || //viking
				waveNumber == 2 || //soldier
				waveNumber == 3) //upgrade
			{
				HUD.LockStartWaveButton();
			}
		}
	}

	//wait for wave complete UI to complete before triggering more tutorial prompts
	public static void OnWaveCompleteMessageComplete(int waveNumber)
	{
		if (instance != null)
		{
			//push enemy pop-ups first so the forced pop-ups render on top
//			switch (waveNumber)
//			{
//			case 0:		ShowTutorial(TutorialType.Enemy_1);		break;
//			case 4:		ShowTutorial(TutorialType.Enemy_2);		break;
//			case 6:		ShowTutorial(TutorialType.Enemy_3);		break;
//			case 8:		ShowTutorial(TutorialType.Enemy_4);		break;
//			}

			//forced pop-ups
			switch (waveNumber)
			{
			case 0:		
				AddPlacementMarker(instance.axeLocationX, instance.axeLocationY);
				ShowPugCharacter("ftue_pug_2", TutorialType.None, null);

				instance.currentState = State.PlaceAxe;
				break;

			case 2:		
				AddPlacementMarker(instance.soldierLocationX, instance.soldierLocationY);
				ShowPugCharacter("ftue_pug_3", TutorialType.None, null);

				instance.currentState = State.PlaceSoldier;
				break;

			case 3:
				ShowTutorial(TutorialType.Upgrade);
				instance.currentState = State.UpgradeTower;
				instance.canUpgradeTower = true;
				break;
			}
		}
	}

	public static void OnWaveSpawningComplete()
	{
	}

	public static void OnPauseResume()
	{
		if (instance != null)
		{
			//make sure to relock the play button, as the pause menu animating the
			//HUD on and off while reset its current state.
			if (instance.currentState != State.Idle && instance.currentState != State.PugShowPlayButton)
				HUD.LockStartWaveButton();
		}	
	}

	static void ShowTutorial(TutorialType type)
	{
		var hintData = HintsDatabase.GetFTUEHintData("FTUE_" + type.ToString());
		if (hintData != null)
		{
			hintData.onHintDismissed = instance.OnTutorialDismissed;
			HintsPanel.AddHint(hintData);
		}
	}

	void OnTutorialDismissed()
	{
		//if the pop-up was the archer placement one, show the archer placement pug
		if (currentState == State.PlaceArcher)
		{
			ShowPugCharacter("ftue_pug_1", TutorialType.None, null);
			AddPlacementMarker(archerLocationX, archerLocationY);
			currentState = State.PlaceArcher; //show pug crushes this.
		}
		else if (currentState == State.UpgradeTower)
		{
			ShowPugCharacter("ftue_pug_4", TutorialType.None, null);
			currentState = State.UpgradeTower;
		}
		else if (currentState == State.PugShowHintButton)
		{
			ShowPugCharacter("ftue_pug_18", TutorialType.None, null);
			currentState = State.PugShowPlayButton;
			HUD.UnlockStartWaveButton(true);
		}
	}

	static void ShowPugCharacter(string dialogueID, TutorialType nextTutorial, FTUEGuide.OnGuideDismissed cb)
	{
		if (instance != null)
		{
			UserInterface.ShowFTUEGuide(dialogueID, cb, cb != null);

			instance.currentState = State.WaitForPugCharacter;
			instance.nextTutorial = nextTutorial;
		}
	}

	void OnWelcomePugDismissed(string previousID)
	{
		ShowTutorial(nextTutorial);
		currentState = State.PlaceArcher;	
	}

	public void OnDrawGizmos()
	{
		var landscape = Landscape.instance;

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(landscape.GetTileCentre(archerLocationX, archerLocationY), 0.25f*landscape.tileWidth);

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(landscape.GetTileCentre(axeLocationX, axeLocationY), 0.25f*landscape.tileWidth);

		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(landscape.GetTileCentre(soldierLocationX, soldierLocationY), 0.25f*landscape.tileWidth);
	}
}