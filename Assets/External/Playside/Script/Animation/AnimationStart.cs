using UnityEngine;
using System.Collections;

public class AnimationStart : MonoBehaviour {

	public Animator targetAnimator;
	public string defaultAnimName;
	public float minNormStart = 0;
	public float maxNormStart = 0.5f;
	public float minSpeed = 1;
	public float maxSpeed = 1;
	public string[] startBools;
	
	[System.Serializable]
	public class AnimFloat {
		public string name;
		public float minValue;
		public float maxValue;
	}
	public AnimFloat[] startFloats;


	void OnEnable ()
	{
		targetAnimator.Play(defaultAnimName, 0, Random.Range(minNormStart, maxNormStart));
		targetAnimator.speed = Random.Range(minSpeed, maxSpeed);

		for (int i=0; i< startBools.Length; i++)
		{
			targetAnimator.SetBool(startBools[i], true);
		}

		for (int i=0; i< startFloats.Length; i++)
		{
			targetAnimator.SetFloat(startFloats[i].name, Random.Range(startFloats[i].minValue, startFloats[i].maxValue) );
		}
	}
}
