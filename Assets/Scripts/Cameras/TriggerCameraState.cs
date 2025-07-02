using UnityEngine;
using System.Collections;

public class TriggerCameraState : MonoBehaviour
{
	public CameraStates targetState;

	public void TriggerCamera()
	{
		UISceneManager.instance.uiSceneCamera.SetCameraAnimation(targetState);
	}

	public void NavigateCameraBack()
	{
		UISceneManager.instance.uiSceneCamera.NavigateCameraBack();
	}
}
