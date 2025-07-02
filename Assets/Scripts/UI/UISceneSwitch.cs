using UnityEngine;
using System.Collections;

public class UISceneSwitch : MonoBehaviour {

	public UISceneIDs targetID;

	public void SetScene ()
	{
		UISceneManager.instance.SetScene(targetID);
	}
	
}
