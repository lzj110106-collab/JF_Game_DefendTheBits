using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Last minute
public class DialogNewVersionMore : MonoBehaviour 
{
	bool isVisible = false;
	public void Trigger()
	{
		gameObject.SetActive(true);
		isVisible = true;
	}

	public void Hide()
	{
		gameObject.SetActive(false);
		isVisible = false;
	}

	public bool IsVisible()
	{
		return isVisible;
	}
}
