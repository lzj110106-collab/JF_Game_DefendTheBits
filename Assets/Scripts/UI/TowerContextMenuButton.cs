using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TowerContextMenuButton : MonoBehaviour 
{
	public Text text; //TODO: replace with icon
	public Text cost;

	System.Action buttonAction;

	public void SetButtonInteraction(System.Action buttonEvent)
	{
		buttonAction = buttonEvent;
	}

	public void OnContextMenuButtonPressed()
	{
		buttonAction();
	}
}
