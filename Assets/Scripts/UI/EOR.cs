using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Runtime.InteropServices;
using DG.Tweening;

public class EOR : MonoBehaviour 
{
	public Animator panelAnimator;
	public Animator rewardAnimatorCash;
	public Animator rewardAnimatorTrinket;

	public GameObject tokenHierarchy;
	public GameObject cashHierarchy;
	public GameObject unlockTowerHierarchy;
	public GameObject rewardHierarchy;

	public Image rewardImgToken;
	public Text rewardTextToken;
	public Text rewardTextCash;

	public GameObject playOnButton;
	public GameObject retryButton;
	public GameObject returnToMainButton;
	public GameObject continueButton;
	public GameObject unlockTowerProgressButton;
	public GameObject unlockTowerRewardButton;
	public GameObject LeaderboardButton;

	public GameObject bestWaveHierarchy;
	public Text waveProgressDisplay;
	public Text waveHighscoreDisplay;
	public Text livesRemainingDisplay;
	public Text unlockProgressText;
	public Image unlockProgressTrinketIcon;
	public Image unlockProgressTowerIcon;
	public Slider unlockProgressSlider;
	public GameObject[] stars;
	public Animator starAnimator;
	public float[] starGemRewards;
	public Animator victoryTextAnimator;
	public Animator gameoverTextAnimator;

	List<Tower> towersToUnlock = new List<Tower>();
	int nextTowerToUnlock = 0;

	LevelData levelData;
	bool premiumRewardAvailable;
	bool tokenRewardAvailable;
	int premiumReward;

	int wavesCompleted;
	int prevWavesCompleted;

	int starsAwarded;
	int starsAwardedNormalMode;
	int prevStarsAwarded;

	bool canPlayOn;
	bool endlessMode;
	bool rewardsAvailable;
	bool continueRewards;

	bool isTowerUnlockSequence;
	bool canUnlockTrinketTower;

    string rewardTokenID2;
    // EOR Flow is as follows
    // -------------------
    // Init, then enable GameOver panel to start sequence
    // Show Title text
    // Show Stars gained
    // Show Round stats
    // Show (if any) Rewards, otherwise stay on previous round stat screen
    // --------------------
    public void Show(LevelData level)
	{
		levelData = level;

		prevWavesCompleted = SaveData.WavesComplete(level);
		wavesCompleted = EnemyWaveController.WavesCompleted();

		if (wavesCompleted > prevWavesCompleted)
			SaveData.SetWavesComplete(level, wavesCompleted);

		prevStarsAwarded = SaveData.StarRating(level);
		starsAwarded = level.CalcStarRatingByRemainingLives(HUD.instance.livesRemaining);

		endlessMode = EnemyWaveController.IsEndlessMode();
		isTowerUnlockSequence = false;
		canPlayOn = false;

		if (endlessMode)
		{
			//EOR from endless mode means that we lost all our lives, so 
			//grab the stars awarded from the first time the EOR
			//screen was shown
			starsAwarded = starsAwardedNormalMode; 
		}
		else
		{
			if (starsAwarded > prevStarsAwarded)
			{
				SaveData.SetStarRating(level, starsAwarded);
				LevelDatabase.RefreshTotalStarRating();

				if (!levelData.IsFTUE())
					AchievementDatabase.CollectStars(starsAwarded - prevStarsAwarded);
			}

			//can trigger endless mode as long as the player has lives remaining
			if (starsAwarded > 0 && !FTUE.IsActive())
				canPlayOn = true; 

			//save this for endless
			starsAwardedNormalMode = starsAwarded;
		}

		UnlockPanel.inst.Show ();
        if (LocManager.isInChina())
            PanelManager.Instance.EnableScreen(PanelID.GameOver);
        else
            PanelManager.Instance.EnableScreen(PanelID.GameOver_en);

        //LEGACY
        {
//			//make sure that gems are only ever awarded once
//			premiumRewardAvailable = starsAwarded >= 3 && !SaveData.HasReceivedReward(level);
//			rewardTextCash.text = levelData.cashReward.ToString();
//
//			if (premiumRewardAvailable)
//			{
//				SaveData.AddCash(levelData.cashReward);
//				SaveData.ReceivedReward(level);
//			}
		}

		int gemModifier = Mathf.Clamp(starsAwarded - 1, 0, starGemRewards.Length - 1);

		premiumRewardAvailable = starsAwarded > 0;  //是否提供额外奖励
        // print(gemModifier + ","+level.cashReward+","+ starGemRewards[gemModifier]);
        if (PlayerPrefs.GetString("BuyMonthlyCard") == "Yes")
        {
            Debug.LogError("此处是获取奖励的位置");
            premiumReward = ((int)(level.cashReward * starGemRewards[gemModifier])) * 2; //此处是获取奖励的位置，设置每日奖励翻倍在这修改
        }
        else
        {
            premiumReward = (int)(level.cashReward * starGemRewards[gemModifier]); //此处是获取奖励的位置，设置每日奖励翻倍在这修改
        }
        rewardTextCash.text = premiumReward.ToString();

        if (premiumRewardAvailable)
			SaveData.AddCash(premiumReward);

		if (HUD.instance.trinketsThisRound.Count != 0)
		{
			string rewardTokenID = "";
			foreach (string key in HUD.instance.trinketsThisRound.Keys)
			{
				rewardTokenID = key;
                rewardTokenID2 = key;
            }

			if (rewardTokenID != null)
			{
				rewardImgToken.sprite = TrinketDatabase.GetIcon(rewardTokenID);
				rewardTextToken.text = HUD.instance.trinketsThisRound[rewardTokenID].ToString();
			}
			tokenRewardAvailable = true;
		}

		//build unlock list up front. if this is the second time we won the level, we need
		//to know if there are trinket unlocks possible, otherwise the reward animation
		//wont play (gems are only awarded once), and you will never be able to tower
		//unlock from the EOR for this level again
		BuildUnlockList();
		rewardsAvailable = false;

		//dont reward rewards in endless mode.
		if (!endlessMode)
		{
			rewardsAvailable = premiumRewardAvailable ||
								tokenRewardAvailable ||
								towersToUnlock.Count > 0;
		}

		cashHierarchy.SetActive(premiumRewardAvailable);
		tokenHierarchy.SetActive(tokenRewardAvailable);

		playOnButton.SetActive(false);
		retryButton.SetActive(false);
		returnToMainButton.SetActive(false);
		continueButton.SetActive(false);
		//LeaderboardButton.SetActive(false);
		unlockTowerHierarchy.SetActive(false);
		unlockTowerRewardButton.SetActive(false);

		//achievement tracking
		{
			if (starsAwarded >= 3)
				AchievementDatabase.LevelComplete(levelData.identifier);

//			if (starsAwarded > prevStarsAwarded)
//				AchievementDatabase.CollectStars(starsAwarded - prevStarsAwarded);
		}

		//gamecentre leaderboards
		if (FTUE.IsActive())
		{
            //PlayerPrefs.SetInt("show_ftue", 0);
            //PlayerPrefs.SetInt("show_ftue_mm", 1);

            ObscuredPrefs.SetInt("show_ftue", 0);
            ObscuredPrefs.SetInt("show_ftue_mm", 1);

            //make sure all these show up in the codex
            for (int i = 1; i <= 5; ++i)
				SaveData.ClearEnemyHint("enemy_" + i.ToString());

			//unlock these towers now. this wont stop the unlock sequence from playing out,
			//but it will ensure the user is awarded the towers correctly in the case
			//that they quit the game before the sequence has completed (since FTUE is
			//only accessible a single time)
			for (int i = 0; i < level.towerRewards.Length; ++i)
				if (!string.IsNullOrEmpty(level.towerRewards[i]))
					SaveData.UnlockTower(level.towerRewards[i]);

			//clear anything that wasnt interacted with by the player. if not,
			//then the ftue hints will show up in the next played level
			HintsPanel.ClearHints();
		}
		else
		{
			//GameCentreWrapper.OnEndOfRound(level, wavesCompleted);
		}
	}

    public void WatchADVideo()
    {
        adButton.SetActive(false);

        if (tokenHierarchy.activeSelf)
        {
            DOTween.To(delegate (float value) {
                var temp = Mathf.Floor(value);
                rewardTextToken.text = string.Format("{0}", temp);
            }, HUD.instance.trinketsThisRound[rewardTokenID2], HUD.instance.trinketsThisRound[rewardTokenID2] * 2, 1.0f);
        }
        
        if (cashHierarchy.activeSelf)
        {
            DOTween.To(delegate (float value) {
                var temp = Mathf.Floor(value);
                rewardTextCash.text = string.Format("{0}", temp);
            }, premiumReward, premiumReward * 2, 1.0f);
        }
    }

    public Image shareImage;
    public Text shareText;
    public Image shareIcon;

    // Called via Animation Trigger
    public void BeginTitle()
	{
		if (starsAwarded == 0 || endlessMode) {
            if (shareImage)
            {
                shareImage.enabled = false;
                shareText.enabled = false;
                shareIcon.enabled = false;
            }
            
            gameoverTextAnimator.Play("On", 0, 0.0f);
			AudioController.Play ("Music_EOR_Fail");
			AudioController.Play ("UI_EOR_Lose");
		}
		else {
            if (shareImage)
            {
                shareImage.enabled = true;
                shareText.enabled = true;
                shareIcon.enabled = true;
            }
            
            victoryTextAnimator.Play("On", 0, 0.0f);
			AudioController.Play ("Music_EOR_Win");
			AudioController.Play ("UI_EOR_Win");
		}

	}


	// Called via Animation Trigger
	public void TitleComplete()
	{
		if (!isTowerUnlockSequence)
		{
			if (FTUE.IsActive())
			{
				//no stars awarded in FTUE
				BeginStats();
			}
			else
			{
				BeginStars();
			}
		}
	}

	public void BeginStars()
	{
		starAnimator.Play(starsAwarded.ToString() + "Star", 0, 0.0f);
	}

	// Called via Animation Trigger
	public void StarsComplete()
	{
		if (!isTowerUnlockSequence)
			BeginStats();
	}

	public void BeginStats()
	{
		if (isTowerUnlockSequence)
		{
		}
		else
		{
			//convert to 1-indexing for display
			waveProgressDisplay.text = wavesCompleted.ToString() + "/" + EnemyWaveController.TotalWaves().ToString();
			livesRemainingDisplay.text = HUD.instance.livesRemaining.ToString();
			bestWaveHierarchy.SetActive(prevWavesCompleted <= wavesCompleted);

			// Enable Navigation
			playOnButton.SetActive(canPlayOn && !rewardsAvailable);
			retryButton.SetActive(!canPlayOn && !rewardsAvailable && !FTUE.IsActive());
			returnToMainButton.SetActive(!rewardsAvailable);
			continueButton.SetActive(rewardsAvailable);
			//LeaderboardButton.SetActive(!FTUE.IsActive());

			//we lost. clear the save state.
			if (!canPlayOn)
				SaveData.ClearSaveState();

			WorldAnalytics.EndOfRound(wavesCompleted, endlessMode);
		}

		panelAnimator.Play("Stats", 0, 0.0f);
		continueRewards = true;
	}

    public GameObject adButton;

	// Called via Button
	public void BeginRewards()
	{
        //print("按下继续按钮");
        if (adButton)
        {
            //if ((premiumRewardAvailable || tokenRewardAvailable) && IronSource.Agent.isRewardedVideoAvailable())
            //    adButton.SetActive(true);
            //else
                adButton.SetActive(false);
        }
        //if (adButton)
        //adButton.SetActive(false);

        panelAnimator.Play("Rewards", 0, 0.0f);
		continueRewards = false;

		rewardHierarchy.SetActive(premiumRewardAvailable || tokenRewardAvailable);

		// Enable Navigation
		playOnButton.SetActive(canPlayOn);
		returnToMainButton.SetActive(true);
		retryButton.SetActive(!canPlayOn && !FTUE.IsActive());
		continueButton.SetActive(false);
		//LeaderboardButton.SetActive(false);
		AudioController.Play ("UI_EOR_Reward");
		AudioController.Stop ("UI_EOR_Win");

		//only FTUE awards towers for free
		if (FTUE.IsActive()) 
		{
//			unlockTowerRewardButton.SetActive(true); //turn on non-progression unlock button
//			returnToMainButton.SetActive(false);
		}
		//else if (towersToUnlock.Count > 0)        //如果有未解锁的防御塔，把进度显示出来
		//{
		//	string trinketID = "";
		//	int trinketRequirement = 0;

		//	if (TowerLoader.UnlockRequirements(towersToUnlock[0], out trinketID, out trinketRequirement))
		//	{
		//		int trinketCount = SaveData.TrinketCount(trinketID);
		//		trinketCount = Mathf.Min(trinketCount, trinketRequirement);

		//		unlockTowerHierarchy.SetActive(true);
		//		unlockProgressText.text = trinketCount.ToString() + "/" + trinketRequirement.ToString();
		//		unlockProgressTrinketIcon.sprite = TrinketDatabase.GetIcon(trinketID);
		//		unlockProgressTowerIcon.sprite = TowerInfoPanel.GetIcon(towersToUnlock[0], 0);
		//		unlockProgressSlider.value = (float)trinketCount/(float)trinketRequirement;

		//		canUnlockTrinketTower = (trinketCount >= trinketRequirement);
		//		unlockTowerProgressButton.GetComponent<Button>().interactable = canUnlockTrinketTower;
		//	}
		//}
	}

	// Called via Animation Trigger
	public void RevealRewards()
	{
		if (!isTowerUnlockSequence)
		{
			if (premiumRewardAvailable)
			{
				rewardAnimatorCash.Play("On", 0, 0.0f);
				premiumRewardAvailable = false;
			}
			else if (tokenRewardAvailable)
			{
				rewardAnimatorTrinket.Play("On", 0, 0.0f);
				tokenRewardAvailable = false;
			}
			else
				panelAnimator.Play("RewardsComplete", 0, 0.0f);
		}
	}



	public void Hide()
	{
		gameObject.SetActive(false);
	}

	// Used to progress through EOR screens, ie Stats to Rewards
	public void ContinueEORButtonPressed()
	{
		if (continueRewards)
			BeginRewards();
		else
			ReturnToMainMenu();

//#if UNITY_IOS && !UNITY_EDITOR
//        print("隐藏互推。");
//        HidePromote();
//#endif
    }

    //[DllImport("__Internal")]
    //private static extern void HidePromote();

    public void ReturnToMainMenu()
	{
		Hide();

		SaveData.ClearSaveState();
		GameState.instance.TriggerMainMenu();

//#if UNITY_IOS && !UNITY_EDITOR
//        print("隐藏互推。");
//        HidePromote();
//#endif
        //only show the rate app thing if the user just got three
        //stars for a level. the theory goes that they are more
        //likely to rate if something good just happened.
        //zbs 20180817 去除询问玩家是否喜欢游戏的功能
        //if (starsAwarded == 3 && !levelData.IsFTUE())
        //	UserInterface.GetRateAppDialog().Show();
    }

    public void ContinuePlay()
	{
		Hide();
		GameState.TriggerEndlessMode();
	}

	public void ReplayLevel()
	{
		Hide();

        SaveData.ClearSaveState();
		GameState.instance.RestartWorld();

//#if UNITY_IOS && !UNITY_EDITOR
//        print("隐藏互推。");
//        HidePromote();
//#endif
    }

    #region TOWER UNLOCK

    public void UnlockTower()
	{
		if (FTUE.IsActive())
		{
			BeginUnlockSequence();
		}
		else
		{
			/* Edit: Stefan, 15/08/17 */
			// Commented out due to design change. Leaving here incase the flow reverts back.
			/* 
			//UI flow isnt set up to return to the EOR, so show a warning before
			//initiating the unlock animation sequence
			UserInterface.ShowYesNoDialog(LocManager.Translate("ui_warning"), 
										  LocManager.Translate("ui_lose_progress"), 
										  BeginUnlockSequence,
										  null);
			*/

			BeginUnlockSequence();
		}
	}
		
	public void BeginUnlockSequence()
	{
		var unlock = UserInterface.GetTowerUnlockScreen();

		for (int i = 0; i < towersToUnlock.Count; ++i)
			unlock.Add(towersToUnlock[i]);

		unlock.Show(UnlockSequenceComplete);
		towersToUnlock.Clear();

		//dont render the FTUE while the unlock sequence is happening.
		//cant just call ReturnToMainMenu() at this point as it
		//will conflict with the unlock sequence UI
		World.instance.gameObject.SetActive(false);

		Hide();
	}

	public void UnlockSequenceComplete()
	{
		ReturnToMainMenu();
	}

	void BuildUnlockList()
	{
		towersToUnlock.Clear();
		nextTowerToUnlock = 0;

		//list of awards given by this stage.
		for (int i = 0; i < levelData.towerRewards.Length; ++i)
		{
			var prefab = TowerLoader.GetTowerPrefab(levelData.towerRewards[i]);
			if (prefab != null)
			{
				//don't play the unlock sequence every time the
				//user completes the current level
				if (!SaveData.IsTowerUnlocked(levelData.towerRewards[i]) || FTUE.IsActive())
					towersToUnlock.Add(prefab);
			}
		}

		if (!FTUE.IsActive())
		{
			//search for an unlockable tower that uses the trinkets on this level
			foreach (var kv in TowerLoader.instance.towerInfoByPrefab)
			{
				if (SaveData.IsTowerUnlocked(kv.Value[0].name))
					continue;
				
				string trinketID;
				int trinketCount;

				if (TowerLoader.UnlockRequirements(kv.Key, out trinketID, out trinketCount))
				{
					//all levelData.trinketIDs are the same. 
					if (trinketID == levelData.trinketIds[0])
					{
						//only force unlock a single tower at a time with this method
						towersToUnlock.Add(kv.Key);
						return;
					}
				}
			}
		}
	}

#endregion
}
