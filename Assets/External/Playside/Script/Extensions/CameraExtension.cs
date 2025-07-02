using UnityEngine;
using System.Collections;

public static class CameraExtension {

	// Cached objects
	public static Camera uiCamera;

	// Returns first camera tagged 'UICamera' in the scene
	public static Camera GetUICamera(this Camera camera)
	{
		// Return previously cached camera if it exists
		if(uiCamera != null)
			return uiCamera;

		// Return UI Camera if it exists
		GameObject cameraObject = GameObject.FindWithTag("UICamera");
		if(cameraObject != null)
		{
			uiCamera = cameraObject.GetComponent<Camera>();
			if(uiCamera != null)
				return uiCamera;
		}

		Debug.LogWarning("Camera tagged 'UICamera' does not exist, tag UI Camera correctly. Returning Main Camera instead.");
		return Camera.main;
	}
}
