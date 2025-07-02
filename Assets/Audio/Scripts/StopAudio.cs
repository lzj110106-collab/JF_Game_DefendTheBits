using UnityEngine;
using System.Collections;

public class StopAudio : MonoBehaviour {

	private AudioController audioController;

	public void StopClip(string clipName)
	{
		AudioController.Stop(clipName);
	}

}
