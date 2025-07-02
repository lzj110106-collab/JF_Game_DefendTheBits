using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAspectRatioScaler : MonoBehaviour
{
	public float scale169 = 1.0f;
	public float scale43 = 1.0f;

	// Scales between 16:9 and 4:3 ratios on device, quick and dirty alternative to complex anchoring
	void Awake ()
	{
		float currentRatio = 	Camera.main.aspect;
		float aspectLerp = (currentRatio - 1.333f) / (1.777f - 1.333f);
		float ratioScale = Mathf.Lerp(scale43, scale169, aspectLerp);

		Vector3 newScale = new Vector3(ratioScale, ratioScale, 1.0f);
		GetComponent<RectTransform>().localScale = newScale;
	}
}
