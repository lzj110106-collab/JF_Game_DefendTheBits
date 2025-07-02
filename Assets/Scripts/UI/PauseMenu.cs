using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour 
{
	public static PauseMenu instance;

	public GameObject restartButton;
	public GameObject returnToMainMenuButton;

	void Awake()
	{
		instance = this;
	}

	void Update()
	{
		restartButton.SetActive(!FTUE.IsActive());
		returnToMainMenuButton.SetActive(!FTUE.IsActive());
	}

	public void Show()
	{
		PanelManager.Instance.SwitchToScreen(PanelID.HUD, PanelID.Pause);

		//HUD.instance.Hide();
		World.instance.TogglePause();
		UserInterface.GetFTUEGuide().OnPause();
		AudioController.PauseCategory ("Music_Game");
	}

	public void Hide()
	{
		PanelManager.Instance.SwitchToScreen(PanelID.Pause, PanelID.HUD);

		//HUD.instance.Show();
		World.instance.TogglePause();

		FTUE.OnPauseResume();
		UserInterface.GetFTUEGuide().OnResume();
		AudioController.UnpauseCategory ("Music_Game");
	}

	public void RestartLevel()
	{
		UserInterface.ShowYesNoDialog(LocManager.Translate("ui_warning"), 
									  LocManager.Translate("ui_lose_progress"), 
									  RestartLevelConfirm,
									  RestartLevelCancel);
	
	}

	void RestartLevelConfirm()
	{
		Hide();

		SaveData.ClearSaveState();
		GameState.instance.RestartWorld();
		AudioController.UnpauseCategory ("Music_Game");

        GameObject Shop_parent = GameObject.Find("Menu_HUD");
        GameObject Shop = Shop_parent.transform.Find("shop").gameObject;
        Shop.SetActive(true);
        try
        {
            GameObject temp = GameObject.Find("enemy_sort");
            if (temp != null)
            {
                Debug.Log("temp.name : " + temp.name);
                for (int i = 0; i < temp.transform.childCount; i++)
                {
                    Destroy(temp.transform.GetChild(i).gameObject);
                }
            }

        }
        catch (System.Exception)
        {

            Debug.Log("暂未找到此游戏物体");
        }

	}

	void RestartLevelCancel()
	{
	}

	public void ReturnToMainMenu()
	{
		UserInterface.ShowYesNoDialog(LocManager.Translate("ui_warning"), 
									  LocManager.Translate("ui_lose_progress"), 
									  ReturnToMainMenuConfirm,
									  ReturnToMainMenuCancel);

	}

	void ReturnToMainMenuConfirm()
	{
		//need to make sure we reset all the object pooling before we go back to the main menu, 
		//otherwise things will break the next time the game is loaded
		AudioController.UnpauseAll();
		AudioController.StopAll();

		World.instance.Restart();

		SaveData.ClearSaveState();
		GameState.instance.TriggerMainMenu();

		TransitionUI(returnToMainMenuButton);

    }

	void ReturnToMainMenuCancel()
	{
	}

	void TransitionUI(GameObject targetButton)
	{
		PanelManager.Instance.DisableScreen(PanelID.Pause);
		UISceneManager.instance.SetScene(UISceneIDs.MainMenu);
	}
}
