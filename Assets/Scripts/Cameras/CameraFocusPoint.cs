using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraFocusPoint : MonoBehaviour 
{
	public float distanceFromFocus = 35.0f;
	public float distanceFromFocusWide = 45.0f;

	public void Update()
	{
		var controller = GameObject.Find("Main Camera");
		if (controller != null)
			controller.GetComponent<MainCameraController>().SetFocus(this);
	}
}
