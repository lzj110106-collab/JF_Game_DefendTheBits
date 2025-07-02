using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public abstract class PopupMessageBase : MonoBehaviour
{
	public static PopupMessageBase instance;

	[System.Serializable]
	public class PopupInfo
	{
		public string							title;				// Title text
		public Sprite							imageSprite;		// Sprite to use for large image (optional)
		public string							message;			// Main message text
		public string							confirmButtonText;	// Label on button
		public PopupWindowContainer.PopupIDs	popupId;			// Which container to display
		public Action							confirmCallback;	// Callback for when button is pressed
		public Action							cancelCallback;		// Callback for when button is pressed

		public PopupInfo(string _title, Sprite _imageSprite, string _message, string _confirmButtonText = "", 
			PopupWindowContainer.PopupIDs _popupId = PopupWindowContainer.PopupIDs.Default, 
			Action _confirmCallBack = null, Action _cancelCallback = null) 
		{
			title 				= _title; 
			imageSprite 		= _imageSprite; 
			message 			= _message; 
			confirmButtonText 	= _confirmButtonText; 
			popupId 			= _popupId; 
			confirmCallback 	= _confirmCallBack;
			cancelCallback 		= _cancelCallback;
		}
	}

	#region Non-editor variables

	protected Queue<PopupInfo>		PopupQueue = new Queue<PopupInfo>();
	Action							ConfirmCallback;
	Action							CancelCallback;

	#endregion	// Non-editor variables

	#region Implement these

	protected abstract void			Show();						// Implement this
	protected abstract void			Hide();						// Implement this
	protected abstract bool			IsShowing();				// Implement this

	#endregion	// Implement these

	void Awake()
	{
		instance = this;
	}


	/// <summary> Shows & returns the specified popup container </summary>
	/// <param name="_popupId"> Popup's name </param>
	/// <returns> The PopupWindowContainer </returns>
	protected virtual PopupWindowContainer ShowContainer(PopupWindowContainer.PopupIDs _popupId)
	{
		throw new NotImplementedException();
	}

	/// <summary> Pops up, showing the specified options/layout </summary>
	/// <param name="info"> Popup contents </param>
	public void PopupOrQueue(PopupInfo _info)
	{
		PopupQueue.Enqueue(_info);

		if (!IsShowing())
			TryShowNext();

		AudioController.Play ("UI_Popup");
	}

	/// <summary> Pops up, showing the specified options/layout </summary>
	/// <param name="_popupId"> Popup container's ID </param>
	/// <param name="_dismissCallback"> Dismiss callback, else uses the default one </param>
	public void PopupOrQueue(PopupWindowContainer.PopupIDs _popupId)
	{
		PopupOrQueue(new PopupInfo(string.Empty, null, string.Empty, string.Empty, _popupId));
	}

	/// <summary> Shows the next popup, if there is one </summary>
	/// <returns> True if there was a popup to show in the queue </returns>
	bool TryShowNext()
	{
		if (PopupQueue.Count > 0)
		{
			PopupInfo info = PopupQueue.Dequeue();

			PopupWindowContainer container = ShowContainer(info.popupId);

			if ((container.titleText != null) && !string.IsNullOrEmpty(info.title))
				container.titleText.text = info.title;

			if ((container.messageText != null) && !string.IsNullOrEmpty(info.message))
				container.messageText.text = info.message;
	
			if (info.imageSprite != null)
				container.largeImage.sprite = info.imageSprite;

			if (!string.IsNullOrEmpty(info.confirmButtonText))
				container.dismissButtonText.text = info.confirmButtonText;

			ConfirmCallback = (info.confirmCallback == null) ? DefaultDismissCallback : info.confirmCallback;
			CancelCallback 	= (info.cancelCallback == null) ? DefaultDismissCallback : info.cancelCallback;

			container.cancelButton.SetActive(info.confirmCallback != null);

			AudioController.Play ("UI_Popup");

			Show();

			return true;
		}

		else
			return false;
	}

	/// <summary> Called when popup is finished </summary>
	public void PopupDismissed()
	{
		if (!TryShowNext())
		{
			Hide();
			if (ConfirmCallback != null)
			{
				ConfirmCallback();
				ConfirmCallback = null;
			}
			CancelCallback = null;
		}
	}

	/// <summary> Dismisses the window without triggering the callback </summary>
	public void PopupCancelled()
	{
		if (!TryShowNext())
		{
			Hide();
			if (CancelCallback != null)
			{
				CancelCallback();
				CancelCallback = null;
			}
			ConfirmCallback = null;
		}
	}

	/// <summary> Default callback for just dismissing the screen </summary>
	public void DefaultDismissCallback()
	{
		PanelManager.Instance.DisableScreen(PanelID.Popups);
	}
}
