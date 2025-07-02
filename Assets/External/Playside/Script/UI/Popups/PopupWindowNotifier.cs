using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupWindowNotifier : MonoBehaviour {

	public PopupWindowContainer.PopupIDs				popupId;					// Which popup this is

	public void ShowPopup()
	{
		PopupMessageBase.instance.PopupOrQueue(popupId);
	}	
}
