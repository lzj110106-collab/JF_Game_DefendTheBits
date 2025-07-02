using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Audio;

public class SpeedButtonControl : MonoBehaviour {

	public static SpeedButtonControl Instance;

	public Toggle buttonToggle;
	public Animator buttonAnimator;
	
	public enum SpeedTypes {Normal, Fast, Fastest}

	[System.Serializable]
	public class SpeedInfo
	{
		public SpeedTypes speed = SpeedTypes.Normal;
		public float speedMultiplier = 1.0f;
	}
	public SpeedInfo[] speedInfos;
	public SpeedTypes currentSpeed { get; private set;}

	public bool singleUsePerWave = false;

	public AudioMixerSnapshot normalAudioSnapshot;
	public AudioMixerSnapshot fastForwardAudioSnapshot;

	void Awake()
	{
		Instance = this;
	}

	public void SetSpeed (int id)
	{
		SpeedTypes newSpeed = (SpeedTypes)id;
		if (buttonAnimator.gameObject.activeSelf)
			buttonAnimator.SetTrigger( (id > 0) ? "FastForward" : "Normal");
		
		currentSpeed = newSpeed;
		if (World.instance != null)
			World.instance.SetTimescale(speedInfos[id].speedMultiplier);
	}

	public void ToggleSpeed(bool enabled)
	{
		SetSpeed(enabled?1:0);
        

		if (enabled)
		{
            if (Instance != null && fastForwardAudioSnapshot != null)
            {
                Instance.fastForwardAudioSnapshot.TransitionTo(0.3f);
                AudioController.Play("UI_SpeedUp");
            }

            if (singleUsePerWave)
                Instance.buttonToggle.interactable = false;
        }

		if (!enabled) {
			Instance.normalAudioSnapshot.TransitionTo (0.3f);
			AudioController.Play ("UI_SlowDown");
		}
	}

	public static void ResetSpeed()
	{
		if (World.instance != null)
			World.instance.SetTimescale(1f);
		


		if (Instance != null)
		{
			Instance.currentSpeed = SpeedTypes.Normal;
			Instance.buttonToggle.isOn = false;
			Instance.buttonToggle.interactable = true;
			Instance.buttonAnimator.SetTrigger("Normal");
			Instance.normalAudioSnapshot.TransitionTo (0.3f);
		}
	}
}



