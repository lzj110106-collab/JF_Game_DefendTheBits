using UnityEngine;
using System.Collections;

public class AnimMaterialTint : MonoBehaviour {

	public Renderer[] targetRenderers;
	private Material[] targetMaterials;
	private Animator anim;
	private Color targetColor; 


	void Start ()
	{
		anim = GetComponent<Animator>();
		targetColor = Color.white;

		targetMaterials = new Material[targetRenderers.Length];

		for(int i=0; i< targetRenderers.Length; i++)
		{
			targetMaterials[i] = targetRenderers[i].material;
		}
	}
	

	void Update () 
	{
		if(anim != null)
		{
			targetColor.r = anim.GetFloat("R");
			targetColor.g = anim.GetFloat("G");
			targetColor.b = anim.GetFloat("B");

			for (int i=0; i<targetMaterials.Length; i++)
			{
				targetMaterials[i].color = targetColor;
			}
		}
	}

	public void EnableTinting() { this.enabled = true; }
	public void DisableTinting() { this.enabled = false; }
}
