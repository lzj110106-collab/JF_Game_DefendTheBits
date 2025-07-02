using UnityEngine;
using System.Collections;

public class MusicOnStart : MonoBehaviour {

	// Use this for initialization
	void Awake () {

		AudioController.Play ("Music");
	
	}

}
