using UnityEngine;
using System.Collections;

public class ShareMaterial : MonoBehaviour {

	public Renderer[] renderers;
	public Renderer masterRenderer;
	[HideInInspector] public Material sharedMat;


	void Awake ()
	{
		CreateSharedMaterial();
	}

	private void CreateSharedMaterial()
	{
		sharedMat = new Material(masterRenderer.material);
		sharedMat.name = masterRenderer.sharedMaterial.name + "_master";
		masterRenderer.material = sharedMat;

		foreach(Renderer s in renderers)
		{
			s.sharedMaterial = sharedMat;
		}
	}
}
