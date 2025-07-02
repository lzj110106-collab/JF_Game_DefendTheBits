using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LevelDetails : MonoBehaviour 
{
	public static LevelDetails instance;

	public GameObject bossCollectInfoPrefab;
	public Transform bossInfoContainer;

	public Text displayText;
	public Image levelPortraitImage;
	public GameObject[] stars;
	public Text[] starHealthTexts;
	public Text startCoinText;
	public Text startLifeText;
	public GameObject bossHierarchy;
	public Text bestWaveText;
	public Text cashText;
	public Text trinketText;
	public Image trinketIcon;

	public GameObject playButton;

	[Header("UI")]
	public Color[] difficultyTextColor;
	public Sprite[] difficultyImages;
	public Image difficultyIcon;	
	public GameObject popupStartInfo;

	[Header("Rewards")]
	public GameObject cashRewardNode;
	public Text cashRewardText;

	public GameObject trinketRewardNode;
	public Text trinketRewardText;
	public Image trinketRewardIcon;

	public LevelData data { get; private set; }

	UIBossCollectInfo[] bossInfos;

	public void Awake()
	{
		instance = this;
	}

	public void SetLevelData(LevelData levelData)
	{
		data = levelData;
		RefreshInfo();
	}

	void RefreshInfo()
	{
		LocManager.Assign(displayText, data.identifier);

		startCoinText.text = data.gold.ToString();
		startLifeText.text = data.lives.ToString();
		cashText.text = data.cashReward.ToString();

		// TODO: add boss type and reward types/chances to CSV and hook up

		RefreshBossInfo();

		if (data != null)
		{
			LocManager.Assign(displayText, data.identifier);

			// Set stars comlete
			int rating = SaveData.StarRating(data);
			levelPortraitImage.sprite = data.prefab.portraitImage;

			for (int i = 0; i < stars.Length; ++i)
				stars[i].SetActive(rating > i);
			
			starHealthTexts[0].text = data.lives.ToString();
			starHealthTexts[1].text = Mathf.RoundToInt(data.lives * 0.5f).ToString();
			starHealthTexts[2].text = "1";

			int wavesComplete = SaveData.WavesComplete(data);
			int totalWaves = WaveLoader.GetWave(data.waves).Count;
			bestWaveText.text = wavesComplete.ToString() + "/" + totalWaves.ToString();

			int difficulty = data.difficulty;
			//displayText.color = difficultyTextColor[difficulty];
			difficultyIcon.sprite = difficultyImages[difficulty];

			//rewards set up
			{
				cashRewardNode.SetActive(data.cashReward > 0);
				cashRewardText.text = data.cashReward.ToString();

				trinketRewardNode.SetActive(false); //TODO
			}
		}
	}

	void RefreshBossInfo()
	{
		trinketIcon.sprite = TrinketDatabase.GetIcon(data.trinketIds[0]);
		LocManager.Assign(trinketText, data.trinketIds[0]);

		//trinketIcon.sprite = ;

		// Clear out existing boss isnfos
		/*if(bossInfos != null)
		{
			for(int i = 0; i<bossInfos.Length; i++)
			{
				Destroy(bossInfos[i].gameObject);
			}
		}

		// Hardcoding a boss value for now, eventually store in CSV
		int bossCount = 1;

		if (bossCount < 1)
			bossHierarchy.SetActive(false);
		else
		{
			bossHierarchy.SetActive(true);

			// Spawn boss info objects
			bossInfos = new UIBossCollectInfo[bossCount];
			for (int i = 0; i < bossCount; i++)
			{
				if (bossInfos[i] == null)
				{
					GameObject boss = Instantiate(bossCollectInfoPrefab);
					boss.transform.SetParent(bossInfoContainer, false);
					bossInfos[i] = boss.GetComponent<UIBossCollectInfo>();
					bossInfos[i].SetInfo();
				}
			}
		}*/
	}
		
	public void TriggerGame()
	{
		/*if (SaveData.WasGameInProgress())
		{
			var header = SaveData.GetSaveStateHeader();

			//TODO: localise
			var levelName = LocManager.Translate(header[(int)SaveStateHeader.LevelName]);
			var waveNumber = int.Parse(header[(int)SaveStateHeader.WaveNumber]);
			var gold = int.Parse(header[(int)SaveStateHeader.GoldRemaining]);
			var lives = int.Parse(header[(int)SaveStateHeader.LivesRemaining]);

			var headerText = "Game In Progress";
			var bodyText = "Would you like to resume " + levelName + 
						   " at wave " + (waveNumber + 1) +  //convert to 1-indexing for display
				       	   " with " + gold + 
					   	   " gold and " + lives + 
					   	   " lives?";

			UserInterface.ShowYesNoDialog(headerText, bodyText, OnResumeGameYes, OnResumeGameNo);
		}
		else
		*/
		//{
			TransitionUI();
			GameState.TriggerGame(data);
		//}
	}

	public void OnResumeGameYes()
	{
		TransitionUI();
		GameState.TriggerResumeGame();
	}

	public void OnResumeGameNo()
	{
		TransitionUI();
		SaveData.ClearSaveState();
		GameState.TriggerGame(data);
	}

	void TransitionUI()
	{
		PanelManager.Instance.SwitchToScreen(PanelID.LevelDetails, PanelID.HUD);
		UISceneManager.instance.SetScene(UISceneIDs.None);
	}

	public void DisablePopupStarInfo()
	{
		popupStartInfo.SetActive(false);
	}

	public void OnLeaderboardButtonPressed()
	{
        //print(data.waves);

		//GameCentreWrapper.ShowLeaderboardUI(data);
	}
}
