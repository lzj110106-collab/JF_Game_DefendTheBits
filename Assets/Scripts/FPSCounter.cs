using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {

	static private int totalFrameCount;
	static private int frameAccumulation;
	static private float timeAccumulation;
	static private float totalTime;
	static private float averageFPS;

	Text myTextObject;

	void Start()
	{
		myTextObject = GetComponent<Text>();
	}

	// Update is called once per frame
	void Update () {
		totalFrameCount++;
		frameAccumulation++;
		timeAccumulation += Time.unscaledDeltaTime;
		totalTime += Time.unscaledDeltaTime;

		if (timeAccumulation > 0.5f)
		{
			if (averageFPS == 0)
			{
				averageFPS = (float)frameAccumulation / timeAccumulation;
			} else
			{
				averageFPS += (float)frameAccumulation / timeAccumulation;
				averageFPS /= 2;
			}
			myTextObject.text = "Avg: " + Mathf.RoundToInt(averageFPS).ToString() + "\nCurrent: " + Mathf.RoundToInt(((float)frameAccumulation / timeAccumulation)).ToString();

			frameAccumulation = 0;
			timeAccumulation = 0;
		}
	}
}
