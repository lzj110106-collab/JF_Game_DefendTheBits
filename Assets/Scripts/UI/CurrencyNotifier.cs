using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyNotifier : MonoBehaviour
{

	// Used for tracking the correct back to use for Shop back nav
	Panel thisPanel;

	void Awake()
	{
		thisPanel = GetComponent<Panel>();
	}

	public void EnableStoreButton ()
	{
		CurrencyDisplay.EnableStoreButton();
	}

	public void DisableStoreButton ()
	{
		CurrencyDisplay.DisableStoreButton();
	}

	public void SetShopBackPanel()
	{
		CurrencyDisplay.SetCurrencyBackPanel(thisPanel.id);
	}


}
