using UnityEngine;
using UnityEngine.UI;
using System.Collections;
//using UnityEditor;

[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class Panel : MonoBehaviour
{
    #region Editor variables
    public PanelID id;
    public Animator anim;
    public string defaultOnTransition = "On";
    public string defaultOffTransition = "Off";
    public GameObject[] enableObjectsOnEnable;
    public bool addToNavigationHistory = true;
    public PanelID[] otherPanelsToShow;
    public PanelID[] otherPanelsToNotShow;
    public PanelID[] otherPanelsToDisableWithThis;
    #endregion    // Editor variables

    #region Non-editor variables

    private CanvasGroup canvasGroup;
    private bool transitioning;
    private bool startupComplete;
    private string transitionID;

    public delegate void TransitionCompleteCallback(Panel panel, string transitionName);
    TransitionCompleteCallback transitionCompleteCallback;

    #endregion    // Non-editor variables

    public void Init()
    {
        CheckCacheAnim();

        startupComplete = false;

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                throw new UnityException("Could not find Canvas Group component on '" + gameObject.name + "'");
        }

        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transitioning = false;

        // Let's screens play their Awake early
        // TODO maybe make this a bool to be set per panel in case other behaviour is desired
        gameObject.SetActive(true);
        gameObject.SetActive(false);

        // Assign default transition if none has been set
        if (defaultOnTransition == "")
            defaultOnTransition = "On";

        if (defaultOffTransition == "")
            defaultOffTransition = "Off";
    }

    void CheckCacheAnim()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
            if (anim == null)
                throw new UnityException("Could not find Animator component on '" + gameObject.name + "'");
        }
    }

    void OnEnable()
    {
        for (int i = 0; i < enableObjectsOnEnable.Length; i++)
            enableObjectsOnEnable[i].SetActive(true);
    }

    void OnDisable()
    {
        transitioning = false;
    }

    /*public void Init()
	{
		Transition(defaultOnTransition);
	}*/

    public void RefreshAnimator()
    {
        CheckCacheAnim();
        // Deprecated in Unity 5.5, but they appear to have fixed the "anim starting state" glitch anyway...
        // anim.Update(Time.deltaTime);
    }

    public void Transition(string transitionName)
    {
        CheckCacheAnim();

        startupComplete = true;
        
        // Turning on?
        if (transitionName != defaultOffTransition)
        {
            // Enable GameObject?
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            // Add to "Back" button history?
            if (addToNavigationHistory)
                PanelManager.Instance.AddScreenToNavHistory(id);

            // Paired panels to show
            for (int i = 0; i < otherPanelsToShow.Length; ++i)
            {
                if (PanelManager.Instance.panels[(int)otherPanelsToShow[i]].gameObject.activeSelf != true)
                    PanelManager.Instance.EnableScreen(otherPanelsToShow[i]);
            }

            //Debug.Log("隐藏其他不需要的panel");
            // Paired panels to not show
            for (int i = 0; i < otherPanelsToNotShow.Length; ++i)
                PanelManager.Instance.DisableScreen(otherPanelsToNotShow[i]);
        }
        else
        {
            for (int i = 0; i < otherPanelsToDisableWithThis.Length; i++)
            {
                PanelManager.Instance.DisableScreen(otherPanelsToDisableWithThis[i]);
            }
        }

        // Start transition if active
        if (gameObject.activeSelf)
        {
            anim.Play(transitionName, 0, 0f);
            transitioning = true;
            transitionID = transitionName;
        }
        //		}
    }

    // Called from Start animation
    public void OnStartupComplete()
    {
        if (!startupComplete)
        {
            startupComplete = true;
            transitioning = false;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            //this catches events that TransitionComplete doesn't
            if (transitioning && transitionCompleteCallback != null)
                transitionCompleteCallback(this, transitionID);

            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            transitioning = false;

            gameObject.SetActive(false);
        }
    }


    public void TransitionComplete()
    {
        transitioning = false;

        if (transitionCompleteCallback != null)
            transitionCompleteCallback(this, transitionID);
    }

    public void DisablePanel()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        //this catches events that TransitionComplete doesn't
        if (transitioning && transitionCompleteCallback != null)
            transitionCompleteCallback(this, transitionID);

        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transitioning = false;

        gameObject.SetActive(false);
    }

    public void PerformDefaultOnTransition()
    {
        Transition(defaultOnTransition);
    }

    public void PerformDefaultOffTransition()
    {
        Transition(defaultOffTransition);
    }

    #region Navigation history + back button functionality

    public void NavigateBack()
    {
        PanelManager.Instance.NavigateBack();
        AudioController.Play("UI_Back");
    }

    public void ReturnToScreen(PanelID panelID)
    {
        PanelManager.Instance.ReturnToScreen(panelID);
        AudioController.Play("UI_Back");
    }

    #endregion // Navigation history + back button functionality

    public void SetTransitionCompleteCallback(TransitionCompleteCallback callback)
    {
        transitionCompleteCallback = callback;
    }
}
