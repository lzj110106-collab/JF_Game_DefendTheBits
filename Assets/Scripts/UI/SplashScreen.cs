using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreen : MonoBehaviour 
{
	public GameObject tapToPlayButton;

	public void OnTapToPlay()
	{

        //GameObject.Find("_ThirdParty").GetComponent<TimeManager>().ShowClamPanel();

        if (FTUE.ShouldTriggerFTUE())
		{
			GameState.TriggerGame(LevelDatabase.GetLevelData("FTUE"));

			PanelManager.Instance.SwitchToScreen(PanelID.MainMenu, PanelID.HUD);
			UISceneManager.instance.SetScene(UISceneIDs.None);

			GetComponent<Panel>().PerformDefaultOffTransition();
		}
		else
		{
			tapToPlayButton.GetComponent<PanelSwitchNotifier>().Switch();
		}

		tapToPlayButton.GetComponent<PlayAudio>().PlayClip("UI_TapToPlay");


	}

    void OnEnable()
    {
            //GameObject.Find("_ThirdParty").GetComponent<TimeManager>().GetDateTime();
    }
}
