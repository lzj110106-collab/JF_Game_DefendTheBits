using UnityEngine;
using System.Collections;

public class PlayAnimation : MonoBehaviour {

	public string inString;
	public string outString;
	public Animator[] animators;

	void PlayAnimIn(int id=0)
	{
		if (animators[id].gameObject.activeSelf)
			animators[id].Play(inString, 0, 0f);
	}

	void PlayAnimOut(int id=0)
	{
		if (animators[id].gameObject.activeSelf)
			animators[id].Play(outString, 0, 0f);
	}

	void PlayAllAnimIn()
	{
		for (int i = 0; i < animators.Length; i++)
		{
			if (animators[i] != null && animators[i].gameObject.activeSelf) 
				animators[i].Play(inString, 0, 0f);
		}
	}

	void PlayAllAnimOut()
	{
		for (int i = 0; i < animators.Length; i++)
		{
			if (animators[i] != null && animators[i].gameObject.activeSelf)
				animators[i].Play(outString, 0, 0f);
		}
	}
}
