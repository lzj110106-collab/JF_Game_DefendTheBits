using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CreditsController : MonoBehaviour
{
	public ScrollRect scroll;
	public float speed = 12.0f;

	float startTime;
	float endTime;
	bool onDrag = false;

	void Start()
	{
		ScrollInitializer();
	}

	public void ScrollInitializer()
	{
		scroll.verticalNormalizedPosition = 1.0f;
		startTime = Time.time;
		endTime = startTime + speed;
	}

    void Update()
    {

    }

	void FixedUpdate()
	{
		if (!onDrag)
		{
			scroll.verticalNormalizedPosition = CarculateScrollValue();

			if (scroll.verticalNormalizedPosition <= 0.01f)
			{
				ScrollInitializer();
			}
		}
	}

	float CarculateScrollValue()
	{
		return 1.0f - Mathf.InverseLerp(startTime, endTime, Time.time);
	}

	float CarculateScrollTime()
	{
		return speed - Mathf.Lerp(0.0f, speed, scroll.verticalNormalizedPosition);
	}

	public void OnDragBegin()
	{
		onDrag = true;
        //print("拖拽开始");
	}

	public void OnDragEnd()
	{
		float scrollTime = CarculateScrollTime();
		startTime = Time.time - scrollTime;
		endTime = startTime + speed;
		onDrag = false;
        //print("拖拽结束");
    }
}