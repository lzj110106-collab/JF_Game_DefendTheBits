using UnityEngine;
using System.Collections;

public class PanelSwitchNotifier : MonoBehaviour
{
	[SerializeField] PanelID	PanelToSwitchTo;
	PanelID						PanelToSwitchFrom;

	void Awake()
	{
		Panel parentPanel = GetComponentInParent<Panel>();
		if (parentPanel == null)
			throw new UnityException("Can't find Panel component anywhere above '" + gameObject.name + "' in the hierarchy");

		PanelToSwitchFrom = parentPanel.id;
	}

	public void Switch()
	{
		PanelManager.Instance.SwitchToScreen(PanelToSwitchFrom, PanelToSwitchTo);
	}

	public void BackButtonPressed()
	{
		Switch();
	}

	public void BackButtonPressed(GameObject _panelToDismiss)
	{
		_panelToDismiss.SetActive(false);
	}


}
