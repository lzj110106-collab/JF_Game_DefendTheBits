using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {

	public static CameraShake instance;



	// Store these in a dictionary for easy reference by string? Or maybe just enums
	// It's nice having the freedom to create a new shake for special cases on the fly

	[System.Serializable]
	public class Shake
	{
		public CameraShakeIds id;
		public Vector3 direction;
		public float randomJitter = 0.0f;
		public float intensity = 1.0f;
		public float frequency = 1.0f;
		public float duration = 0.5f;
	}


	public Transform shakeTransform;
	public float smoothness = 1.0f;

	private bool shaking;
	private bool directional;
	private Vector3 shakeDirection;
	private Vector3 shakeOffset;
	private Vector2 noiseOffset;

	private float intensity;
	private float frequency;
	private float duration;
	private float randomJitter;
	private float count;
	private float progress;

	void Awake()
	{
		instance = this;
		shakeOffset = Vector3.zero;
	}

	void Update ()
	{
		if(shaking)
		{
			if (count >= duration)
			{
				shaking = false;
				shakeOffset = Vector3.zero;
			}
			else
			{
				count += Time.deltaTime;
				progress = count/duration;

				if(directional)
				{
					shakeOffset = shakeDirection * (Mathf.PerlinNoise(noiseOffset.x, progress*frequency) - 0.5f)*(1-progress)*intensity;
				}
				else
				{
					shakeOffset.x = (Mathf.PerlinNoise(noiseOffset.x, progress*frequency) - 0.5f)*(1-progress)*intensity;
					shakeOffset.y = (Mathf.PerlinNoise(noiseOffset.y, progress*frequency) - 0.5f)*(1-progress)*intensity;
				}
			}
		}

		// Apply shake transform offset
		shakeTransform.localPosition = Vector3.Lerp(shakeTransform.localPosition, shakeOffset, Time.deltaTime/smoothness);

		// Disable updates if shake offset is zero
		if(shakeOffset == Vector3.zero)
			this.enabled = false;
	}
	

	// Using CameraShake.Shake class
	public void TriggerShake (CameraShake.Shake shake)
	{
		TriggerShake(shake.direction, shake.intensity, shake.frequency, shake.duration, shake.randomJitter);
	}


	// Manually set values as parameters
	public void TriggerShake (Vector3 newDirection, float newIntensity = 1.0f, float newFrequency = 1.0f, float newDuration = 0.25f,  float newRandomJitter = 1.0f)
	{
		if (newIntensity > 0.0f)
		{
			shaking = true;
			if(!this.enabled)
				this.enabled = true;

			noiseOffset = new Vector2(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f));

			shakeDirection = newDirection.normalized;
			duration = newDuration;
			intensity = newIntensity;
			frequency = newFrequency;
			randomJitter = newRandomJitter;
		
			directional = (shakeDirection != Vector3.zero);
			count = 0f;
		}
	}
}
