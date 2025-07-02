using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestoreScrollPosition : MonoBehaviour 
{
	public float movementSpeed = 1.0f;

	bool isAnimating = false;
	Vector2 restorePosition;

	ScrollRect scrollRect;
	bool isHorizontal; // store this locally so that scrollRect.horizontal can be disabled.

	void Start()
	{
		scrollRect = GetComponent<ScrollRect>();
		isHorizontal = scrollRect.horizontal;
	}

	void Update() 
	{
		if (isAnimating)
		{
			float movement = World.frameTime * movementSpeed;

			var diff = restorePosition - scrollRect.normalizedPosition;
			var distance = Vector3.Magnitude(diff);

			if (movement >= distance)
			{
				scrollRect.normalizedPosition = restorePosition;
				isAnimating = false;
			}
			else
			{
				scrollRect.normalizedPosition += movement * diff/distance;
			}
		}
	}

	public void MarkScrollPosition()
	{
		restorePosition = scrollRect.normalizedPosition;
	}

	public void Trigger()
	{
		isAnimating = true;
	}

	public void Stop()
	{
		isAnimating = false;
	}
}
