using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICameraController : MonoBehaviour 
{
	static UICameraController instance;

	Camera cachedCamera;

	void Awake() 
	{
		instance = this;
		cachedCamera = GetComponent<Camera>();
	}

	void OnDestroy()
	{
		instance = null;
	}

	public static Rect CalcScreenSpaceBounds(GameObject go)
	{
		var rectTransform = go.GetComponent<RectTransform>();
		if (rectTransform)
		{
			var corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);

			corners[0] = RectTransformUtility.WorldToScreenPoint(instance.cachedCamera, corners[0]);
			corners[2] = RectTransformUtility.WorldToScreenPoint(instance.cachedCamera, corners[2]);

			return new Rect(corners[0].x, 
							corners[0].y, 
							corners[2].x - corners[0].x,
							corners[2].y - corners[0].y);
		}

		return new Rect(0, 0, 0, 0);
	}

	public static Vector3 CalcScreenSpacePosition(GameObject go)
	{
		var rectTransform = go.GetComponent<RectTransform>();
		if (rectTransform)
		{
			var corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);

			var centre = (0.5f * (corners[0] + corners[2]));
			return RectTransformUtility.WorldToScreenPoint(instance.cachedCamera, centre);
		}

		return Vector3.zero;
	}
		
	public static Vector3 WorldToScreen(Vector3 worldPosition)
	{
		return instance.cachedCamera.WorldToScreenPoint(worldPosition);
	}

	public static Vector3 ScreenToWorld(Vector3 screenPosition)
	{
		return instance.cachedCamera.ScreenToWorldPoint(screenPosition);
	}
}
