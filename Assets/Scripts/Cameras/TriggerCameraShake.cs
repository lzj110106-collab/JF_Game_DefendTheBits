using UnityEngine;
using System.Collections;

public class TriggerCameraShake : MonoBehaviour {


	public CameraShake.Shake[] shakes;

	// Trigger Shake by array ID
	public void TriggerShake (CameraShakeIds id)
	{
		for (int i=0; i<shakes.Length; i++)
		{
			if(shakes[i].id == id)
				CameraShake.instance.TriggerShake(shakes[i]);
		}
	}

	// Trigger Shake by id string (for anim triggers)
	public void TriggerShakeID (string id)
	{
		TriggerShake ( (CameraShakeIds)System.Enum.Parse(typeof(CameraShakeIds), id) );
	}
}
