using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

//helper functions for ensuring device and editor input work without having
//to duplicate code all over the place. assumes left mouse button for editor gameplay.
public class InputUtil
{
	public static bool MousePressed()
	{
	#if UNITY_EDITOR
		return Input.GetMouseButtonDown(0);
	#else
		return Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began;
	#endif
	}

	public static bool MouseReleased()
	{
	#if UNITY_EDITOR
		return Input.GetMouseButtonUp(0);
	#else
		if (Input.touchCount > 0)
			return Input.touches[0].phase == TouchPhase.Ended ||
				   Input.touches[0].phase == TouchPhase.Canceled;

		return false;
	#endif
	}

	public static bool MouseDrag()
	{
	#if UNITY_EDITOR
		return Input.GetMouseButton(0);
	#else
		return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved;
	#endif
	}

	public static Vector2 MousePosition()
	{
	#if UNITY_EDITOR
		return Input.mousePosition;
	#else
		if (Input.touchCount > 0)
			return Input.touches[0].position;

		return Vector2.zero;
	#endif
	}

	public static bool IsHovered(RectTransform transform, Camera camera)
	{
		return RectTransformUtility.RectangleContainsScreenPoint(transform, MousePosition(), camera);
	}

	public static bool IsWorldHovered()
	{
		if (EventSystem.current == null)
			return true;

		//checking both of these methods because editor and device behave differently.
		return !EventSystem.current.IsPointerOverGameObject() && 
			   !EventSystem.current.IsPointerOverGameObject(0);
	}
}
