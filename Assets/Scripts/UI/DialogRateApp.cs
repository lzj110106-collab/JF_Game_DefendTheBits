using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;

public class DialogRateApp : MonoBehaviour 
{
	public string enjoyAppHeaderStringID = "enjoy_app";
	public string enjoyAppBodyStringID = "enjoy_app_body";
	public string rateAppHeaderStringID = "rate_app";
	public string rateAppBodyStringID = "rate_app_body";

	public string appStoreLink = "https://itunes.apple.com/us/app/defend-the-bits/id1239297660?ls=1&mt=8";
	public int minutesBetweenDisplays = 1440;

	bool isVisible = false;

	public void Show()
	{
		if (/*PlayerPrefs.GetInt("show_rate_app", 1) == 1*/ ObscuredPrefs .GetInt("show_rate_app", 1) == 1 && !isVisible)
		{
			//first time this function is triggered, we always show the app rating thing.
			if (/*PlayerPrefs.GetInt("show_rate_app_first_time", 1) == 1*/ObscuredPrefs.GetInt("show_rate_app_first_time", 1) == 1)
			{
				ShowEnjoyGameDialog();
				//PlayerPrefs.SetInt("show_rate_app_first_time", 0);
				//PlayerPrefs.SetString("last_rate_app_showing", DateTime.Now.ToString());

                ObscuredPrefs.SetInt("show_rate_app_first_time", 0);
                ObscuredPrefs.SetString("last_rate_app_showing", DateTime.Now.ToString());
            }
			else
			{
				//on subsequent tries, we need to check timing since the last time 
				//the rate app pop-up was triggered
				//var lastTime = DateTime.Parse(/*PlayerPrefs.GetString("last_rate_app_showing", "")*/ObscuredPrefs.GetString("last_rate_app_showing", ""));
				//var now = DateTime.Now;

				//if ((now - lastTime).TotalSeconds >= minutesBetweenDisplays * 60)
				//{
				//	ShowEnjoyGameDialog();
				//	//PlayerPrefs.SetString("last_rate_app_showing", now.ToString());
    //                ObscuredPrefs.SetString("last_rate_app_showing", now.ToString());
    //            }
			}
		}
	}
		
	public void OnResultYes()
	{
		Application.OpenURL(appStoreLink);
		//PlayerPrefs.SetInt("show_rate_app", 0);
        ObscuredPrefs.SetInt("show_rate_app", 0);

        gameObject.SetActive(false);
		isVisible = false;
	}

	public void OnResultNo()
	{
        //PlayerPrefs.SetInt("show_rate_app", 0); //disable future pop-ups

        ObscuredPrefs.SetInt("show_rate_app", 0);
        gameObject.SetActive(false);
		isVisible = false;
	}

	public void OnResultLater()
	{
		gameObject.SetActive(false);
		isVisible = false;
	}


	void ShowEnjoyGameDialog()
	{
		UserInterface.ShowYesNoDialog(LocManager.Translate(enjoyAppHeaderStringID),
									  LocManager.Translate(enjoyAppBodyStringID),
									  OnEnjoyGameResultYes,
									  OnEnjoyGameResultNo);
	}

	void OnEnjoyGameResultYes()
	{
		//user is enjoying the game, show the rate pop-up
		gameObject.SetActive(true);
		gameObject.GetComponent<Animator>().Play("On", 0, 0.0f);
	}

	void OnEnjoyGameResultNo()
	{
		//nothing. the timer has already been set up.
	}
}
