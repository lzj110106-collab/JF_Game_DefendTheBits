using UnityEngine;
using System.Collections;

public class EnableRandomGameobject : MonoBehaviour {

	[SerializeField] bool			enableOnAwake;
	[SerializeField] GameObject[] 	targets;


	void Start ()
	{
		if(enableOnAwake)
			Randomize();
	}

	// Randomly enables a single object from a list and disables the others
	public void Randomize()
	{
		int randomId = Random.Range(0, targets.Length);
		for(int i=0; i<targets.Length; i++)
		{
			targets[i].SetActive( i==randomId );
		}
	}
}
