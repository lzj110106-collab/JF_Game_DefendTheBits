using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour {

	[SerializeField] Transform[] targets;
	private Transform cameraTransform;

	public enum CameraType { Default, UI_2D, UI_3D, UI_Scene, Unlock_Scene };
	public CameraType cameraType = CameraType.Default;

	void OnEnable ()
	{
		// Project specific
		SetCameraType(cameraType);
	}

	public void SetCameraType(CameraType type)
	{
		cameraType = type;

		switch (cameraType)
		{
		case CameraType.Default:
			cameraTransform = Camera.main.transform;
			break;

		case CameraType.UI_2D:
			cameraTransform = UserInterface.Camera2D().transform;
			break;

		case CameraType.UI_3D:
			cameraTransform = UserInterface.Camera3D().transform;
			break;

		case CameraType.UI_Scene:
			cameraTransform = UserInterface.CameraUIScene().transform;
			break;

		case CameraType.Unlock_Scene:
			cameraTransform = UserInterface.GetTowerUnlockScreen().cameraTransform;
			break;
		}
	}
	
	void LateUpdate ()
	{
		for (int i=0; i< targets.Length; i++)
			targets[i].LookAt(cameraTransform.position, targets[i].up);
	}
}
