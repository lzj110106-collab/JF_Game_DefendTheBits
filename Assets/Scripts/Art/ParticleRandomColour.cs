using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleRandomColour : MonoBehaviour {

	public Color[] colors;
	public ParticleSystem[] particleSystems;

	// Tint system random colour on awake
	void Awake()
	{
		if(particleSystems.Length==0)
			particleSystems = transform.GetComponentsInChildren<ParticleSystem>();

		Color newColor = colors[(int)Random.Range(0, colors.Length-1)];

		for(int i=0; i<particleSystems.Length; i++)
		{
			particleSystems[i].startColor = newColor;
		}
	}
}
