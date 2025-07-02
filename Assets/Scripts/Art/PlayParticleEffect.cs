using UnityEngine;
using System.Collections;

public class PlayParticleEffect : MonoBehaviour {

	public ParticleSystem[] targets;

	public void PlayEffect(string pfxName)
	{
		for(int i=0; i<targets.Length; i++)
		{
			if(targets[i] == null)
				continue;
			if(targets[i].gameObject.name == pfxName)
				targets[i].Play();
		}
	}

	public void PlayAll()
	{
		for(int i=0; i<targets.Length; i++)
		{
			targets[i].Play();
		}
	}

	public void StopEffect(string pfxName)
	{
		for(int i=0; i<targets.Length; i++)
		{
			if(targets[i] == null)
				continue;
			if(targets[i].gameObject.name == pfxName)
				targets[i].Stop();
		}
	}

	public void StopAll()
	{
		for(int i=0; i<targets.Length; i++)
		{
			targets[i].Stop();
		}
	}

	public void StopAllImmediate()
	{
		for(int i=0; i<targets.Length; i++)
		{
			targets[i].Stop();
			targets[i].Clear();
		}
	}
}
