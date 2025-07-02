using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Text;
using UnityEngine.Networking;

public class MainMenu : MonoBehaviour
{
    public Button playButton;
    public GameObject resumeGameLabel;
    public Animator quest3DAnimator;

    public Animator questButtonAnimator;
    public Animator upgradesButtonAnimator;

    public GameObject playAlert;
    public GameObject leaderboardAlert;
    public GameObject questsdAlert;
    public GameObject codexAlert;
    public GameObject upgradesAlert;

    //public GameObject clamAlert;
    private float checkTime = 10.0f;

    public Text dalyTime;
    public Text dalyTime_video;

    string[] puggingtonStrings;
    float puggingtonCountdown;

    public GameObject exitGame;

    void RefreshQuestAlert()
    {
        // TODO: Track new unclaimed quest rewards
        bool newQuestReward = AchievementDatabase.UncollectedAchievementCount() > 0;
        if (quest3DAnimator)
            quest3DAnimator.Play((newQuestReward) ? "Idle_Alert" : "Idle", 0, 0.0f);
    }

    void Awake()
    {
        puggingtonStrings = new string[4]
        {
            "ftue_pug_6", //achievements
//			"ftue_pug_10", 
			"ftue_pug_11",
            "ftue_pug_12",
            "ftue_pug_13"
        };
    }

    public HUD hud;

    void Start()
    {
        resumeGameLabel.SetActive(SaveData.WasGameInProgress());

#if   UNITY_EDITOR
        Debug.Log("编译器中的补单");
#elif UNITY_ANDROID 
        Debug.Log("android中的补单"); 
        PXY_AndroidBuy.Instance.PayCheck();
#endif
    }

    public int appointRewardCount;
    public GameObject appointRewardBtn;
    
    void OnEnable()
    {
        if (LocManager.isInChina()&&TimeManager.getAppointReward)     //如果是中国区玩家,并且获得了预约奖励的等级
        {
            //判断可领取的预约奖励是否已经都领取了
            if (SaveData.GetAppointReward() < appointRewardCount)
            {
                //显示按钮
                appointRewardBtn.SetActive(true);
            }else
            {
                //隐藏按钮
                appointRewardBtn.SetActive(false);
            }
        }
        else
        {
            appointRewardBtn.SetActive(false);
        }

        //delay the puggington display a tiny bit as it highlights
        //one of the buttons, which immediately gets overwritten
        //by the animation that brings the main menu on screen
        puggingtonCountdown = 2.0f;

        leaderboardAlert.SetActive(false); //never show this
        codexAlert.SetActive(SaveData.ShouldShowCodexAlert());
        playAlert.SetActive(false); //TODO
        questsdAlert.SetActive(AchievementDatabase.UncollectedAchievementCount() > 0);
        upgradesAlert.SetActive(false); //TODO
    }

    void Update()
    {
        //NB: need to change this ID each update, otherwise people who saw the 2nd
        //version update won't see the 3rd or 4th etc.
        //2018 8 15  zbs 屏蔽版本更新提示界面
        //if (PlayerPrefs.GetInt("show_version_2", 1) == 1)
        //{
        //	UserInterface.GetNewVersionDialog().Trigger();
        //	PlayerPrefs.SetInt("show_version_2", 0);
        //}

        //if (UserInterface.GetFTUEGuide().IsVisible() ||
        //	UserInterface.GetNewVersionDialog().IsVisible() ||
        //	UserInterface.GetNewVersionMoreDialog().IsVisible())
        //{
        //	return;
        //}

        //checkTime -= Time.deltaTime;
        //if(checkTime<=0.0f){
        //    print("检查一次");
        // GameObject.Find("_ThirdParty").GetComponent<TimeManager>().GetDateTime();
        //    checkTime += 10.0f;
        //}

        if (/*PlayerPrefs.GetInt("show_ftue_mm", 0) == 1*/ObscuredPrefs.GetInt("show_ftue_mm", 0) == 1)
        {
            puggingtonCountdown -= Time.deltaTime;
            if (puggingtonCountdown > 0.0f)
                return;

            for (int i = 0; i < puggingtonStrings.Length; ++i)
            {
                if (/*PlayerPrefs.GetInt(puggingtonStrings[i], 0) == 0*/ObscuredPrefs.GetInt(puggingtonStrings[i], 0) == 0)
                {
                    UserInterface.ShowFTUEGuide(puggingtonStrings[i], OnPuggingtonDismissed, true);
                    //PlayerPrefs.SetInt(puggingtonStrings[i], 1);

                    ObscuredPrefs.SetInt(puggingtonStrings[i], 1);

                    if (i == 0)
                        questButtonAnimator.Play("Idle_Highlighted", 0, 0.0f);
                    else
                        upgradesButtonAnimator.Play("Idle_Highlighted", 0, 0.0f);
                    break;
                }
            }
        }


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



    void OnPuggingtonDismissed(string previousID)
    {
        questButtonAnimator.Play("Idle", 0, 0.0f);
        upgradesButtonAnimator.Play("Idle", 0, 0.0f);

        //hardcoded hacks to chain pug pop-ups.
        for (int i = 1; i < puggingtonStrings.Length; ++i)
        {
            if (previousID == puggingtonStrings[i] &&/*
				PlayerPrefs.GetInt(puggingtonStrings[i], 0) == 0*/ObscuredPrefs.GetInt(puggingtonStrings[i], 0) == 0)
            {
                UserInterface.ShowFTUEGuide(puggingtonStrings[i], OnPuggingtonDismissed, true);
                //PlayerPrefs.SetInt(puggingtonStrings[i], 1);

                ObscuredPrefs.SetInt(puggingtonStrings[i], 1);

                questButtonAnimator.Play("Idle_Highlighted", 0, 0.0f);

                break;
            }
        }

        //turn these checks off once the last pug has been shown
        if (previousID == puggingtonStrings[puggingtonStrings.Length - 1])
            //PlayerPrefs.SetInt("show_ftue_mm", 0);
            ObscuredPrefs.SetInt("show_ftue_mm", 0);
    }

    public void Show()
    {
        PanelManager.Instance.ReturnToScreen(PanelID.MainMenu);
        resumeGameLabel.SetActive(SaveData.WasGameInProgress());
    }

    public void Hide()
    {
        PanelManager.Instance.DisableScreen(PanelID.MainMenu);
    }

    public void OnPlayButtonPressed()
    {
        if (FTUE.ShouldTriggerFTUE())
        {
            GameState.TriggerGame(LevelDatabase.GetLevelData("FTUE"));

            PanelManager.Instance.SwitchToScreen(PanelID.MainMenu, PanelID.HUD);
            UISceneManager.instance.SetScene(UISceneIDs.None);
        }
        else if (SaveData.WasGameInProgress())
        {
            var header = SaveData.GetSaveStateHeader();

            //TODO: this is the same code thats in LevelDetails. 
            var levelName = LocManager.Translate(header[(int)SaveStateHeader.LevelName]);
            var waveNumber = int.Parse(header[(int)SaveStateHeader.WaveNumber]);
            var gold = int.Parse(header[(int)SaveStateHeader.GoldRemaining]);
            var lives = int.Parse(header[(int)SaveStateHeader.LivesRemaining]);

            var headerText = LocManager.Translate("ui_resume");
            var bodyText = LocManager.BuildString("ui_resume_body", levelName, waveNumber + 1, gold, lives);

            UserInterface.ShowYesNoDialog(headerText, bodyText, OnResumeGameYes, OnResumeGameNo);
            PanelManager.Instance.DisableScreen(PanelID.MainMenu);
        }
        else
        {
            PanelManager.Instance.SwitchToScreen(PanelID.MainMenu, PanelID.LevelSelect);
            UISceneManager.instance.uiSceneCamera.SetCameraAnimation(CameraStates.LevelSelect);
        }
    }

    public void OnResumeGameYes()
    {
        GameState.TriggerResumeGame();
        PanelManager.Instance.EnableScreen(PanelID.HUD);
        UISceneManager.instance.SetScene(UISceneIDs.None);
    }

    public void OnResumeGameNo()
    {
        PanelManager.Instance.EnableScreen(PanelID.LevelSelect);
        UISceneManager.instance.uiSceneCamera.SetCameraAnimation(CameraStates.LevelSelect);
    }

    public void OnLeaderboardButtonPressed()
    {
        //GameCentreWrapper.ShowLeaderboardUI(null);
    }
}
