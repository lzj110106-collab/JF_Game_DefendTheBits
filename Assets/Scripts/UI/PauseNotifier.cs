using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseNotifier : MonoBehaviour
{

	public void ShowPauseMenu ()
	{
		PauseMenu.instance.Show();
	}

}
