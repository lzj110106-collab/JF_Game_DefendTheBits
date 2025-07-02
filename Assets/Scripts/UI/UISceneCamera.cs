using UnityEngine;
using System.Collections;


public enum CameraStates
{
	Invalid = -1,
	MainMenu,
	Upgrades,
	LevelSelect,
	LevelDetails,
	Settings,
	Codex,
	Quests,
	Shop
}


public class UISceneCamera : MonoBehaviour
{
	public Camera camera;
	public Animator cameraAnimator;

	CameraStates currentState;
	CameraStates previousState;

	void Awake()
	{
		currentState = previousState = CameraStates.MainMenu;
	}

	public void Enable()
	{
		gameObject.SetActive(true);
	}

	public void Disable()
	{
		gameObject.SetActive(false);
	}

	public void SetCameraAnimation(CameraStates _cameraState)
	{
		previousState = currentState;
		currentState = _cameraState;

		cameraAnimator.SetTrigger(currentState.ToString());
	}

	public void NavigateCameraBack()
	{
		if (previousState != CameraStates.Invalid)
		{
			SetCameraAnimation(previousState);
			previousState = CameraStates.Invalid;
		}
		else
			Debug.LogWarning("Not going back, previous state is Invalid");
	}
}
