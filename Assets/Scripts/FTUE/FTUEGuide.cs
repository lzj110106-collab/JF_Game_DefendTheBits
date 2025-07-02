using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FTUEGuide : MonoBehaviour 
{
	public enum GuidePositions
	{
		TopLeft,
		TopRight,
		BotLeft,
		BotRight,
		TopCenter,
		BotCenter,
		BotLeftCorner,
		BotRightCorner
	}

	public Animator guideAnimator;
	public Animator anchorAnimator;
	public Text dialogue;

	public delegate void OnGuideDismissed(string previousID);
	OnGuideDismissed onGuideDismissed;

	public float minDuration = 1.0f;
	public float duration = 3.0f;
	float countdown;
	bool isVisible;
	bool wasVisible;

	bool autoDismissEnabled;
	GuidePositions guidePos;
	bool leftSide;

	string currentStringID;

	public void Update()
	{
		if (isVisible && autoDismissEnabled)
		{
			countdown -= Time.deltaTime;
			if (countdown <= 0.0f || (InputUtil.MousePressed() && countdown < duration - minDuration))
			{
				Hide();

				if (onGuideDismissed != null)
				{
					onGuideDismissed(currentStringID);
					onGuideDismissed = null;
				}
			}
		}
	}

	//if the OnGuideDimissed delegate is null, then its up to the
	//caller to dismiss the guide class manually.
	public void Show(string stringID, OnGuideDismissed cb, bool autoDismiss)
	{
		gameObject.SetActive(true);
		LocManager.Assign(dialogue, stringID);

		isVisible = true;
		autoDismissEnabled = autoDismiss;

		countdown = duration;
		onGuideDismissed = cb;
		currentStringID = stringID;


		// TODO load in as tag from CSV
		PositionGuide(GuidePositions.BotLeft);

//		Debug.Log("Show: " + stringID);
	}

	void PositionGuide(GuidePositions _guidePos)
	{
		leftSide = false;
		guidePos = _guidePos;

		if(guidePos == GuidePositions.TopRight || guidePos == GuidePositions.BotRight )
			leftSide = true;

		guideAnimator.Play((leftSide) ? "On_Left" : "On_Right", 0, 0.0f);
		anchorAnimator.Play(guidePos.ToString(), 0, 0.0f);
	}

	public void Hide()
	{
		if (isVisible)
		{
			guideAnimator.Play((leftSide) ? "Off_Left" : "Off_Right", 0, 0.0f);
			isVisible = false;

//			Debug.Log("hide: " + currentStringID);
		}
	}

	public void OnPause()
	{
		if (isVisible)
		{
			guideAnimator.Play((leftSide) ? "Off_Left" : "Off_Right", 0, 0.0f);
			isVisible = false;
			wasVisible = true;
		}
	}

	public void OnResume()
	{
		if (wasVisible)
		{
			gameObject.SetActive(true);
			guideAnimator.Play((leftSide) ? "On_Left" : "On_Right", 0, 0.0f);
			// TODO load in as tag from CSV
			PositionGuide(GuidePositions.BotLeft);

			isVisible = true;			
		}
	}

	public bool IsVisible()
	{
		return isVisible;
	}
}
