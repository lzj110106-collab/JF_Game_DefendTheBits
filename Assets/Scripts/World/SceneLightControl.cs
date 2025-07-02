using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SceneLightControl : MonoBehaviour {

	public static SceneLightControl instance;
	[SerializeField] Color ambientLightColor;

	void Awake()
	{
		instance = this;
		SetSceneLight(ambientLightColor);
	}

	public void SetSceneLight(Color newLightColor)
	{
		// Fix FOG for 4:3 devices
		RenderSettings.ambientLight = newLightColor; 

	}

	#if UNITY_EDITOR
	void Update ()
	{
		if(RenderSettings.ambientLight != ambientLightColor)
			SetSceneLight(ambientLightColor);
	}
	#endif
}
