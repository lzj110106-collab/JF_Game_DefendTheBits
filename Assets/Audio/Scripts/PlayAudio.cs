using UnityEngine;
using System.Collections;

public class PlayAudio : MonoBehaviour {

	private AudioController audioController;

	AudioObject toesLaserAudioObject;

	public void PlayClip(string clipName)
	{
		if (!string.IsNullOrEmpty(clipName))
			AudioController.Play(clipName);
	}

	public void ToesLaser (string clipName)
	{

		if (!string.IsNullOrEmpty(clipName) && !AudioController.IsPlaying(clipName)) {

			if (toesLaserAudioObject == null)
				toesLaserAudioObject = AudioController.Play ("Tower_Toes_Laser_Lp");

			else if (!toesLaserAudioObject.IsPlaying())
				toesLaserAudioObject.Play();
		}
	}

	public void ToesLaserStop ()
	{
			toesLaserAudioObject.Stop(0.1f);
	}

	public void StopClip (string clipName)
	{
		AudioController.Stop(clipName);
	}
}
