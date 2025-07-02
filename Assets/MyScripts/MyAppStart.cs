using UnityEngine;
using System.Collections;

public class MyAppStart : MonoBehaviour
{
	public static string appKey = "7ae01c25";

	// Use this for initialization
	void Start ()
	{
#if IOS
        //Debug.Log ("unity-script: MyAppStart Start called");

        //IronSource tracking sdk
        IronSource.Agent.reportAppStarted ();

		//Dynamic config example
		IronSourceConfig.Instance.setClientSideCallbacks (true);

		//string id = IronSource.Agent.getAdvertiserId ();
		//Debug.Log ("unity-script: IronSource.Agent.getAdvertiserId : " + id);
		
		//Debug.Log ("unity-script: IronSource.Agent.validateIntegration");
		IronSource.Agent.validateIntegration ();

		//Debug.Log ("unity-script: unity version" + IronSource.unityVersion ());

		// SDK init
		//Debug.Log ("unity-script: IronSource.Agent.init");
		IronSource.Agent.setUserId ("uniqueUserId");
		IronSource.Agent.init (appKey);
#endif
	}

	void OnApplicationPause (bool isPaused)
	{
#if IOS
        //Debug.Log ("unity-script: OnApplicationPause = " + isPaused);
        IronSource.Agent.onApplicationPause (isPaused);
#endif
	}
}
