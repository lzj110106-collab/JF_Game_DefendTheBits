using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

public class PanelManager : MonoBehaviour
{
	#region Editor variables

	public PanelID[] initialPanels;
	public PanelID FallbackReturnToPanel;

	#endregion	// Editor variables

	#region Non-editor variables

	public Panel[] panels { get; private set; }
	Stack<PanelID> navHistory = new Stack<PanelID>();
	public static PanelManager Instance;

	#endregion	// Non-editor variables

	#region Debug
	#if !FINAL
	public bool OnScreenNavigationDebugging;
	StringBuilder OnGUISB = new StringBuilder();
	Rect OnGUIRect = new Rect(0.0f, 200.0f, 200.0f, 400.0f);
	void OnGUI()
	{
		if (!OnScreenNavigationDebugging)
			return;

		PanelID[] history = navHistory.ToArray();
		OnGUISB.Length = 0;
		for (int i = history.Length - 1; i >= 0; --i)
		{
			if (i < history.Length - 1)
				OnGUISB.Append(" -> ");
			OnGUISB.Append(history[i].ToString());
			OnGUISB.Append("\n");
		}

		GUI.Label(OnGUIRect, OnGUISB.ToString());
	}
	#endif
	#endregion	// Debug

    void Awake()
    {
		if (Instance != null)
			throw new UnityException("Singleton Instance trying to be set multiple times");
        Instance = this;

		Initialize();
    }

    // Index panels by enum int for later access
    public void Initialize()
    {
        Panel[] unsortedPanels = GetComponentsInChildren<Panel>(true);
        panels = new Panel[(int) Enum.GetValues( typeof( PanelID ) ).Length];

        for(int i=0; i<unsortedPanels.Length; i++)
        {
            int newIndex = (int)unsortedPanels[i].id;
            //if(panels[newIndex] != null)
            //    Debug.LogError("Unable to initialize panels. Multiple panels share same ID: " + panels[newIndex].name + ", " + unsortedPanels[i].name);
            panels[(int)unsortedPanels[i].id] = unsortedPanels[i];
        }
    }

    void Start()
    {
        for (int i = 0; i < panels.Length; i++)
        {
			Panel panel = panels[i];
            if (panel != null)
            {
				panel.Init();
				//panel.RefreshAnimator();

				// Check if initial panel to turn on
				for (int initialPanelNo = 0; initialPanelNo < initialPanels.Length; ++initialPanelNo)
                {
					if (panel.id == initialPanels[initialPanelNo])
					{
						panel.gameObject.SetActive(true);
						panel.Transition(panel.defaultOnTransition);
						break;
					}
                }
            }
        }
    }

    // Play a Transition (In/Out/State) on a Panel
    public void Transition(PanelID panelId, string transitionName)
    {
        //if(panels[(int)panelId]==null)
        //    Debug.LogWarning("Unable to transition Panel "+panelId);
        panels[(int)panelId].Transition(transitionName);
    }

    public void Transition(PanelID panelId)
    {
        panels[(int)panelId].Transition(panels[(int)panelId].defaultOnTransition);
    }

	public void SwitchToScreen(PanelID fromScreen, PanelID toScreen)
	{
		DisableScreen(fromScreen);
        EnableScreen(toScreen);
    }

	public void EnableScreen(int _p)
	{
		panels[_p].Transition(panels[_p].defaultOnTransition);
	}

	// Instantly Enable a Panel
    public void EnableScreen(PanelID panelId)
    {
        panels[(int)panelId].Transition(panels[(int)panelId].defaultOnTransition);
    }

    // Instantly Disable a Panel
    public void DisableScreen(PanelID panelId)
    {
		if (panels [(int)panelId] == null)
			throw new UnityException ("Panel '" + panelId + "' not found in the hierarchy");

		panels[(int)panelId].Transition(panels[(int)panelId].defaultOffTransition);
    }

	public void TurnOffScreenImmediately(PanelID panelId)
	{
		panels[(int)panelId].gameObject.SetActive(false);
	}

	// Checks if a panel is enabled
	public bool IsScreenEnabled(PanelID panelId)
	{
		return (panels[(int)panelId].gameObject.activeSelf);
	}

	// Instantly enable the specified panel, and disable all others
	public void EnableOnlyScreen(PanelID panelToLeaveOn)
	{
		List<PanelID> panelList = new List<PanelID>();
		panelList.Add(panelToLeaveOn);
		EnableOnlyScreens(panelList);
	}

	public void EnableOnlyScreens(List<PanelID> panelIDs)
	{
		for (int i = 0; i < Enum.GetValues(typeof(PanelID)).Length; ++i)
		{
			Panel panel = panels[i];
			if (panel != null)
			{
				PanelID panelID = (PanelID)i;
				if (panelIDs.Contains(panelID))
					panel.Transition(panel.defaultOnTransition);
				else
				{
					if (panel.gameObject.activeSelf)
						panel.Transition(panel.defaultOffTransition);
				}
			}
		}
	}

	public void DisableAllScreens(bool turnOffImmediately)
	{
		for (int i = 0; i < Enum.GetValues(typeof(PanelID)).Length; ++i)
		{
			Panel panel = panels[i];
			if ((panel != null) && (panel.gameObject.activeSelf))
			{
				if (turnOffImmediately)
					panel.gameObject.SetActive(false);
				else
					panel.Transition(panel.defaultOffTransition);
			}
		}
	}

	#region Navigation history + back button functionality

	public void AddScreenToNavHistory(PanelID panelID)
	{
		if ((navHistory.Count == 0) || (navHistory.Peek() != panelID))
		{
			navHistory.Push(panelID);
		}
	}

	public void NavigateBack()
	{
		PanelID returningFrom = navHistory.Pop();
		PanelID returningTo = navHistory.Pop();

		SwitchToScreen(returningFrom, returningTo);
	}

	public PanelID PeekLastScreen()
	{
		return navHistory.Peek();
	}

	/// <summary> Special case functionality that goes back in history as necessary </summary>
	/// <param name="panelID"> Screen to return to </param>
	public void ReturnToScreen(PanelID panelID)
	{
		PanelID panelToShow = panelID;

		if (!navHistory.Contains (panelToShow))
			panelToShow = FallbackReturnToPanel;

		while (navHistory.Count > 0 && navHistory.Peek () != panelToShow) {
			navHistory.Pop ();
		}

		EnableScreen(panelToShow);
	}

	#endregion	// Navigation history + back button functionality
}
