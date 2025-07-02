using UnityEngine;
using System.Collections;

public class PopupWindowUI : PopupMessageBase
{
	const PanelID PopupPanelId = PanelID.Popups;

	PopupWindowContainer[] popups;

	#region PopupMessageBase Overrides

	/// <summary> Shows & returns the specified popup container </summary>
	/// <param name="_popupId"> Popup's name </param>
	/// <returns> The PopupWindowContainer </returns>
	protected override PopupWindowContainer ShowContainer(PopupWindowContainer.PopupIDs _popupId)
	{
		PopupWindowContainer container = null;

		if (popups == null)
			popups = GetComponentsInChildren<PopupWindowContainer>(true);

		for (int i = 0; i < popups.Length; ++i)
		{
			PopupWindowContainer popup = popups[i];
			if (popup.popupId == _popupId)
			{
				popup.gameObject.SetActive(true);
				container = popup;
			}
			else
				popup.gameObject.SetActive(false);
		}

		return container;
	}

	/// <summary> Shows the Popup </summary>
	protected override void Show()
	{
		PanelManager.Instance.EnableScreen(PopupPanelId);
		AudioController.Play ("UI_Popup");
	}

	/// <summary> Hides the Popup </summary>
	protected override void Hide()
	{
		PanelManager.Instance.DisableScreen(PopupPanelId);
		AudioController.Play ("UI_Back");
	}

	/// <summary> Is the popup currently visible? </summary>
	/// <returns> True if showing, false if hidden </returns>
	protected override bool IsShowing()
	{
		return PanelManager.Instance.IsScreenEnabled(PopupPanelId);
	}

	#endregion	// PopupMessageBase Overrides
/*
	/// <summary> Popups up a specific screen with its specific image </summary>
	/// <param name="character"> Character unlocked </param>
	public void PopupOrQueueCharacterUnlock(Player.Characters character)
	{
		Sprite sprite = Player.Instance.GetPlayerConfig(character).LargeSprite;

		PopupOrQueue(new PopupMessageBase.PopupInfo("NEW CHARACTER UNLOCKED!", sprite, Player.Instance.GetCharacterName(character, false) + " IS NOW AVAILABLE ON THE CHARACTER SELECT SCREEN!", "OK", PopupMessageBase.Containers.CharacterUnlock));
	}
*/
}
