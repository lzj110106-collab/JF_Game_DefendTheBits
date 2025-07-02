using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogYesNo : MonoBehaviour 
{
	public Text headerDisplay;
	public Text bodyDisplay;

	public delegate void Callback();
	Callback onYes;
	Callback onNo;

	public GameObject yesButton;
	public GameObject noButton;

	public Text yesButtonText;
	public Text noButtonText;

    public bool isEnteredShop;

	//NB: pass in translated text
	public void Show(string header, string body, Callback onYes, Callback onNo)
	{
		headerDisplay.text = header;
		bodyDisplay.text = body;

		this.onYes = onYes;
		this.onNo = onNo;

		gameObject.SetActive(true);
		AudioController.Play ("UI_Enter");

		yesButton.SetActive(true);
		noButton.SetActive(true);

		LocManager.Assign(yesButtonText, "ui_yes");
		LocManager.Assign(noButtonText, "ui_no");
	}

	//NB: pass in translated text
	public void Show(string header, string body, Callback cb)
	{
		headerDisplay.text = header;
		bodyDisplay.text = body;
		onYes = cb;

		gameObject.SetActive(true);
		AudioController.Play ("UI_Enter");

		noButton.SetActive(false);
		yesButton.SetActive(true);
		LocManager.Assign(yesButtonText, "ui_ok");
	}

	public void OnYesButtonPressed()
	{
		gameObject.SetActive(false);

		if (onYes != null)
			onYes();
	}

	public void OnNoButtonPressed()
	{
		gameObject.SetActive(false);

		if (onNo != null)
			onNo();
	}
}
