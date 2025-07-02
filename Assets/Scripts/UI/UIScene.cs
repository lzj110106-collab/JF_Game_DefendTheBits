using UnityEngine;
using System.Collections;

[RequireComponent (typeof (FogControl))]
public class UIScene : MonoBehaviour {

	public UISceneIDs id;
	public bool blur;
	FogControl fogControl;


	void Awake()
	{
		fogControl = GetComponent<FogControl>();
	}


	public void Enable()
	{
		gameObject.SetActive(true);

		if (fogControl != null)
			fogControl.SetFog();
	}

	public void Disable()
	{
		gameObject.SetActive(false);
	}

}
