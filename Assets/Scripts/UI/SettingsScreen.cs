using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour 
{
	public Text languageButtonText;

	public void ResetSaveData()
	{
		UserInterface.ShowYesNoDialog(LocManager.Translate("ui_warning"), 
									  LocManager.Translate("ui_loseallprogress"), 
									  ResetDataConfirm, 
									  null);
	}

	void ResetDataConfirm()
	{
		SaveData.ResetSaveData();
	}

	public void BackButtonPressed()
	{
		if (GameState.instance.currentState == GameState.State.MainMenu)
			PanelManager.Instance.SwitchToScreen(PanelID.Settings, PanelID.MainMenu);
		else
			PanelManager.Instance.SwitchToScreen(PanelID.Settings, PanelID.Pause);
	}

	public void OnLanguageNextPressed()
	{
		var notifier = GetComponent<LocNotifier>();
		if (notifier != null)
		{
			notifier.ChangeToNextLanguage();

			if (languageButtonText != null)
				languageButtonText.text = LocManager.CurrentLanguageName();
		}
	}

	public void OnLanguagePreviousPressed()
	{
		var notifier = GetComponent<LocNotifier>();
		if (notifier != null)
		{
			notifier.ChangeToPreviousLanguage();

			if (languageButtonText != null)
				languageButtonText.text = LocManager.CurrentLanguageName();
		}
	}



}
