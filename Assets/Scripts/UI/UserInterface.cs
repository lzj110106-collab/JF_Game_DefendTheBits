using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//parent class for static access to various UI things

public class UserInterface : MonoBehaviour
{
    static UserInterface instance;

    public Canvas canvas;

    [Header("Cameras")]
    public Camera camera2D;
    public Camera camera3D;
    public Camera cameraUIScene;

    [Header("Screens")]
    public DialogNewVersion dialogNewVersion;
    public DialogNewVersionMore dialogNewVersionMore;
    public DialogRateApp dialogRateApp;
    public DialogYesNo dialogYesNo;
    public TowerUnlock towerUnlock;
    public HintsPanel hintsPanel;
    public FTUEGuide ftueGuide;
    public QuestPopup questPopup;

    [Header("Effects")]
    public UnityStandardAssets.ImageEffects.BlurOptimized uiSceneBlur;

    bool splashShown;

    void Awake() { instance = this; splashShown = false; }
    void OnDestroy() { instance = null; }

    public static Camera Camera2D()
    {
        return instance ? instance.camera2D : null;
    }

    public static Camera Camera3D()
    {
        return instance ? instance.camera3D : null;
    }

    public static Camera CameraUIScene()
    {
        return instance ? instance.cameraUIScene : null;
    }

    public static Canvas GetCanvas()
    {
        return instance ? instance.canvas : null;
    }

    public static void ShowYesNoDialog(string header,
                                       string body,
                                       DialogYesNo.Callback onYes,
                                       DialogYesNo.Callback onNo)
    {
        if (instance != null)
            instance.dialogYesNo.Show(header, body, onYes, onNo);
    }

    public static void ShowOKDialog(string translatedHeader,
                                    string translatedBody,
                                    DialogYesNo.Callback cb)
    {
        if (instance != null)
            instance.dialogYesNo.Show(translatedHeader, translatedBody, cb);
    }

    public static bool CanTriggerHintsPanel()
    {
        if (instance == null || instance.hintsPanel == null)
            return false;

        return !instance.hintsPanel.gameObject.activeSelf && HintsPanel.hintsToDisplay.Count > 0;
    }

    public static void TriggerHintsPanel()
    {
        instance.hintsPanel.Trigger();
    }

    public static DialogNewVersion GetNewVersionDialog()
    {
        return instance.dialogNewVersion;
    }

    public static DialogNewVersionMore GetNewVersionMoreDialog()
    {
        return instance.dialogNewVersionMore;
    }

    public static DialogRateApp GetRateAppDialog()
    {
        return instance.dialogRateApp;
    }

    public static TowerUnlock GetTowerUnlockScreen()
    {
        return instance != null ? instance.towerUnlock : null;
    }

    public static void ShowFTUEGuide(string stringID, FTUEGuide.OnGuideDismissed cb, bool autoDismiss)
    {
        if (instance != null && instance.ftueGuide != null)
            instance.ftueGuide.Show(stringID, cb, autoDismiss);
    }

    public static void HideFTUEGuide()
    {
        if (instance != null && instance.ftueGuide != null)
            instance.ftueGuide.Hide();
    }

    public static FTUEGuide GetFTUEGuide()
    {
        return instance != null ? instance.ftueGuide : null;
    }

    public static void ShowAchievementUnlocked(Achievement data)
    {
        if (instance != null && instance.questPopup != null)
            instance.questPopup.Initialise(data);
    }

    public static UnityStandardAssets.ImageEffects.BlurOptimized SceneBlur()
    {
        return instance ? instance.uiSceneBlur : null;
    }

    void Start()
    {
        //make sure all the text is pointing at the correct fonts. this fixes stuff
        //like starting the game in korean which wont render with bm_heavitas
        LocManager.RefreshFonts(gameObject);
    }

    // Delay showing Splash screen until all the startup hitches are finished
    void Update()
    {
        if (!splashShown)
        {
            splashShown = true;
            if (LocManager.isInChina())
                PanelManager.Instance.EnableScreen(PanelID.Splash);
           else
               PanelManager.Instance.EnableScreen(PanelID.Splash_en);
            this.enabled = false;
        }
    }

}
