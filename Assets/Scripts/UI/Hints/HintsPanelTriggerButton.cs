using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintsPanelTriggerButton : MonoBehaviour 
{
	public Image icon;

	public bool ftuePing;

	public void Show()
	{
		gameObject.SetActive(true);
		GetComponent<Animator>().Play("On", 0, 0.0f);
		HUD.instance.btnHintAnimator.SetBool("Ping", ftuePing);
	}

	public void TriggerHintsPanel()
	{
		ftuePing = false;
		UserInterface.TriggerHintsPanel();
		UserInterface.HideFTUEGuide();
		gameObject.SetActive(false);
	}

	public void LateUpdate()
	{
		if (HintsPanel.hintsToDisplay.Count > 0)
			icon.sprite = HintsPanel.hintsToDisplay.Last.Value.imageIcon;
	}
}
