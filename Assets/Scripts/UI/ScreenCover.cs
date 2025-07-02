using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ScreenCover : MonoBehaviour
{
	public const float defaultFadeTime = 0.5f;
	public ScreenCoverIDs id;
	public Animator fadeAnim;
	public Image colorImage;
	public Action callbackWhenFaded;

	public static ScreenCover[] Instances;

	void Awake()
	{
		if(Instances == null)
			Instances = new ScreenCover[(int)ScreenCoverIDs.Length];
//		if(Instances[(int)id] != null)
//			throw new UnityException("ScreenCover singleton already created!");
		Instances[(int)id] = this;
		CoverFadeOffComplete();
	}

	public void FadeCoverOff(float duration, Color? fadeColor=null )
	{
		if(colorImage != null)
		{
			colorImage.enabled = true;
			colorImage.color = fadeColor ?? Color.black;
		}
		fadeAnim.Play("FadeOff", 0, 0f);
		fadeAnim.speed = 1/duration;
		
	}

	public void CoverFadeOffComplete()
	{
		colorImage.enabled = false;
	}

	public void FadeCoverOn(float duration, Color fadeColor, Action endOfFadeCallback = null)
	{
		if(colorImage != null)
		{
			colorImage.enabled = true;
			colorImage.color = fadeColor;
		}

		callbackWhenFaded = endOfFadeCallback;

		fadeAnim.Play("FadeOn", 0, 0f);
		fadeAnim.speed = 1/duration;
	}

	public void CoverFadeOnComplete()
	{
		if (callbackWhenFaded != null)
		{
			callbackWhenFaded();
			callbackWhenFaded = null;
		}
	}

	public void Flash(float duration, Color? fadeColor=null, int variant=0 )
	{
		if(colorImage != null)
		{
			colorImage.enabled = true;
			colorImage.color = fadeColor ?? Color.black;
		}
		fadeAnim.Play("Flash", 0, 0f);
		fadeAnim.speed = 1/duration;
	}
}
