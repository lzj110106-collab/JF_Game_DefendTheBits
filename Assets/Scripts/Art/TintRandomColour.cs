using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TintRandomColour : MonoBehaviour {

	public Color[] colors;
	public Renderer[] renderers;

	// Tint system random colour on awake
	void Awake()
	{
		if(renderers.Length==0)
			renderers = transform.GetComponentsInChildren<Renderer>();

		Color newColor = colors[(int)Random.Range(0, colors.Length)];

		for(int i=0; i<renderers.Length; i++)
		{
			renderers[i].material.color = newColor;
		}
	}
}
