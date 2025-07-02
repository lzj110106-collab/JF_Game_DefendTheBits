using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PanelNotifier : MonoBehaviour 
{
	[System.Serializable]
	public class PanelNotifyTarget
	{
		public PanelID id;
		public string transitionName;
	}
	public PanelNotifyTarget[] panelNotifyTargets;


	// Transitions all panels
	public void TransitionAll ()
	{
		for(int i=0; i<panelNotifyTargets.Length; i++)
		{
			PanelManager.Instance.Transition(panelNotifyTargets[i].id, panelNotifyTargets[i].transitionName);
		}
	}
	// Transitions specific panel ID
	public void Transition (int panelID)
	{
		PanelManager.Instance.Transition(panelNotifyTargets[panelID].id, panelNotifyTargets[panelID].transitionName);
	}

	public void TransitionOnly ()
	{
		List<PanelID> panels = new List<PanelID>(panelNotifyTargets.Length);
		for (int i = 0; i < panelNotifyTargets.Length; ++i)
			panels.Add(panelNotifyTargets[i].id);

		PanelManager.Instance.EnableOnlyScreens(panels);
	}

	public void FadeOutThenTransition()
	{
		ScreenCover.Instances[(int)ScreenCoverIDs.Main].FadeCoverOn(ScreenCover.defaultFadeTime, Color.black, TransitionAll);
	}

	public void TransitionToLastScreen()
	{
		PanelID lastPanel = PanelManager.Instance.PeekLastScreen();
		PanelManager.Instance.Transition(lastPanel, "On");
		AudioController.Play ("UI_Back");
	}
}
