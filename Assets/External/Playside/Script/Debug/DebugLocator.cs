using UnityEngine;
using System.Collections;

public class DebugLocator : MonoBehaviour {


	void OnDrawGizmos ()
	{
		if(!Application.isPlaying)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(transform.position, Vector3.one);
		}
	}
}
