using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FogControl : MonoBehaviour {

	public static FogControl instance;
	public Color fogColor;
	public float wideStartDistance = 37.5f;
	public float wideEndDistance = 50.0f;
	public float narrowStartDistance = 37.5f;
	public float narrowEndDistance = 50.0f;

	private float currentRatio;


	void Awake()
	{
		instance = this;
		SetFog();
	}

	public void SetFog()
	{
		// Fix FOG for 4:3 devices
		currentRatio = 	Camera.main.aspect;
		float aspectLerp = (currentRatio - 1.333f) / (1.777f - 1.333f);
		RenderSettings.fogStartDistance = Mathf.Lerp(narrowStartDistance, wideStartDistance, aspectLerp);
		RenderSettings.fogEndDistance = Mathf.Lerp(narrowEndDistance, wideEndDistance, aspectLerp);
		RenderSettings.fogColor = fogColor;
	}

	#if UNITY_EDITOR
	void Update ()
	{
		if(currentRatio != Camera.main.aspect || RenderSettings.fogColor != fogColor)
			SetFog();
	}
	#endif
}
