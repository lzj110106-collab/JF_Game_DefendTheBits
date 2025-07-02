using UnityEngine;
using System.Collections;

public class ShareWaterMaterial : MonoBehaviour {

	public Renderer[] renderers;

	void Awake ()
	{
		foreach(Renderer s in renderers)
		{
			s.sharedMaterial = MaterialCache.instance.waterMaterial;
		}
	}
}
