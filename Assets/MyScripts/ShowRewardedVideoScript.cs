using UnityEngine;
using System;
using System.Collections;

public class ShowRewardedVideoScript : MonoBehaviour
{
	//GameObject InitText;
	//GameObject ShowButton;
	//GameObject ShowText;
	//GameObject AmountText;
	//int userTotalCredits = 0;
	
	public static String REWARDED_INSTANCE_ID = "0";

	// Use this for initialization
	void Awake ()
	{	
		//Debug.Log ("unity-script: ShowRewardedVideoScript Start called");

		//ShowButton = GameObject.Find ("ShowRewardedVideo");
		//ShowText = GameObject.Find ("ShowRewardedVideoText"); 
		//ShowText.GetComponent<UnityEngine.UI.Text> ().color = UnityEngine.Color.red;

		//AmountText = GameObject.Find ("RVAmount");
		
		////Add Rewarded Video Events
		//IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
		//IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent; 
		//IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
		//IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
		//IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
		//IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent; 
		//IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent; 
		//IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent; 

		////Add Rewarded Video DemandOnly Events
		//IronSourceEvents.onRewardedVideoAdOpenedDemandOnlyEvent += RewardedVideoAdOpenedDemandOnlyEvent;
		//IronSourceEvents.onRewardedVideoAdClosedDemandOnlyEvent += RewardedVideoAdClosedDemandOnlyEvent; 
		//IronSourceEvents.onRewardedVideoAvailabilityChangedDemandOnlyEvent += RewardedVideoAvailabilityChangedDemandOnlyEvent;
		//IronSourceEvents.onRewardedVideoAdRewardedDemandOnlyEvent += RewardedVideoAdRewardedDemandOnlyEvent; 
		//IronSourceEvents.onRewardedVideoAdShowFailedDemandOnlyEvent += RewardedVideoAdShowFailedDemandOnlyEvent; 
		//IronSourceEvents.onRewardedVideoAdClickedDemandOnlyEvent += RewardedVideoAdClickedDemandOnlyEvent; 
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

    private bool rewardTypeValue;
    /************* RewardedVideo API *************/
    public void ShowRewardedVideoButtonClicked (bool rewardType)
	{
        rewardTypeValue = rewardType;

        //if (IronSource.Agent.isRewardedVideoAvailable())
        //{
        //    IronSource.Agent.showRewardedVideo();
        //}
        //else
        //{
        //    Debug.Log("unity-script: IronSource.Agent.isRewardedVideoAvailable - False");
        //}
        // DemandOnly
        // ShowDemandOnlyRewardedVideo ();
    }

	void ShowDemandOnlyRewardedVideo ()
	{
		//Debug.Log ("unity-script: ShowDemandOnlyRewardedVideoButtonClicked");
		//if (IronSource.Agent.isISDemandOnlyRewardedVideoAvailable (REWARDED_INSTANCE_ID)) {
		//	IronSource.Agent.showISDemandOnlyRewardedVideo (REWARDED_INSTANCE_ID);
		//} else {
		//	Debug.Log ("unity-script: IronSource.Agent.isISDemandOnlyRewardedVideoAvailable - False");
		//}
	}

    /************* RewardedVideo Delegates *************/ 
	void RewardedVideoAvailabilityChangedEvent (bool canShowAd)
	{
		Debug.Log ("unity-script: I got RewardedVideoAvailabilityChangedEvent, value = " + canShowAd);
		//if (canShowAd) {
		//	ShowText.GetComponent<UnityEngine.UI.Text> ().color = UnityEngine.Color.blue;
		//} else {
		//	ShowText.GetComponent<UnityEngine.UI.Text> ().color = UnityEngine.Color.red;
		//}
	}

	void RewardedVideoAdOpenedEvent ()
	{
		Debug.Log ("unity-script: I got RewardedVideoAdOpenedEvent");
	}

	//void RewardedVideoAdRewardedEvent (IronSourcePlacement ssp)
	//{
 //       //Debug.Log ("unity-script: I got RewardedVideoAdRewardedEvent, amount = " + ssp.getRewardAmount () + " name = " + ssp.getRewardName ());
 //       //userTotalCredits = userTotalCredits + ssp.getRewardAmount ();
 //       //AmountText.GetComponent<UnityEngine.UI.Text> ().text = "" + userTotalCredits;

 //       //print("关闭广告回调函数");
 //       //print(rewardTypeValue);
 //       if (rewardTypeValue.Equals("回复10心"))
 //       {
 //           //Debug.Log("回复10心");
 //           hud.RewardAD();
 //       }
 //       else if (rewardTypeValue.Equals("奖励翻倍"))
 //       {
 //           //Debug.Log("奖励翻倍");
 //           eor.WatchADVideo();
 //       }
 //   }

    public HUD hud;
    public EOR eor;

	void RewardedVideoAdClosedEvent ()
	{
        //print("关闭广告回调函数122");
        //print(rewardTypeValue);

        if (rewardTypeValue)
        {
            //Debug.Log("回复10心");
            hud.RewardAD();
        }
        else
        {
            //Debug.Log("奖励翻倍");
            eor.WatchADVideo();
        }
    }

	void RewardedVideoAdStartedEvent ()
	{
		Debug.Log ("unity-script: I got RewardedVideoAdStartedEvent");
	}

	void RewardedVideoAdEndedEvent ()
	{
		Debug.Log ("unity-script: I got RewardedVideoAdEndedEvent");
	}
	
	//void RewardedVideoAdShowFailedEvent (IronSourceError error)
	//{
	//	Debug.Log ("unity-script: I got RewardedVideoAdShowFailedEvent, code :  " + error.getCode () + ", description : " + error.getDescription ());
	//}

	//void RewardedVideoAdClickedEvent (IronSourcePlacement ssp)
	//{
	//	Debug.Log ("unity-script: I got RewardedVideoAdClickedEvent, name = " + ssp.getRewardName ());
	//}

	/************* RewardedVideo DemandOnly Delegates *************/ 

	void RewardedVideoAvailabilityChangedDemandOnlyEvent (string instanceId, bool canShowAd)
	{
		Debug.Log ("unity-script: I got RewardedVideoAvailabilityChangedDemandOnlyEvent for instance: " + instanceId + ", value = " + canShowAd);
		//if (canShowAd) {
		//	ShowText.GetComponent<UnityEngine.UI.Text> ().color = UnityEngine.Color.blue;
		//} else {
		//	ShowText.GetComponent<UnityEngine.UI.Text> ().color = UnityEngine.Color.red;
		//}
	}

	void RewardedVideoAdOpenedDemandOnlyEvent (string instanceId)
	{
		Debug.Log ("unity-script: I got RewardedVideoAdOpenedDemandOnlyEvent for instance: " + instanceId);
	}

	//void RewardedVideoAdRewardedDemandOnlyEvent (string instanceId, IronSourcePlacement ssp)
	//{
	//	Debug.Log ("unity-script: I got RewardedVideoAdRewardedDemandOnlyEvent for instance: " + instanceId + ", amount = " + ssp.getRewardAmount () + " name = " + ssp.getRewardName ());
	//	//userTotalCredits = userTotalCredits + ssp.getRewardAmount ();
	//	//AmountText.GetComponent<UnityEngine.UI.Text> ().text = "" + userTotalCredits;
	//}
	
	void RewardedVideoAdClosedDemandOnlyEvent (string instanceId)
	{
		Debug.Log ("unity-script: I got RewardedVideoAdClosedDemandOnlyEvent for instance: " + instanceId);
	}

	void RewardedVideoAdStartedDemandOnlyEvent (string instanceId)
	{
		Debug.Log ("unity-script: I got RewardedVideoAdStartedDemandOnlyEvent for instance: " + instanceId);
	}

	void RewardedVideoAdEndedDemandOnlyEvent (string instanceId)
	{
		Debug.Log ("unity-script: I got RewardedVideoAdEndedDemandOnlyEvent for instance: " + instanceId);
	}
	
	//void RewardedVideoAdShowFailedDemandOnlyEvent (string instanceId, IronSourceError error)
	//{
	//	Debug.Log ("unity-script: I got RewardedVideoAdShowFailedDemandOnlyEvent for instance: " + instanceId + ", code :  " + error.getCode () + ", description : " + error.getDescription ());
	//}

	//void RewardedVideoAdClickedDemandOnlyEvent (string instanceId, IronSourcePlacement ssp)
	//{
	//	Debug.Log ("unity-script: I got RewardedVideoAdClickedDemandOnlyEvent for instance: " + instanceId + ", name = " + ssp.getRewardName ());
	//}

}
