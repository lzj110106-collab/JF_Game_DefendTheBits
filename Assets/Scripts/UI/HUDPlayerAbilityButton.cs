using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDPlayerAbilityButton : MonoBehaviour 
{
	public Text costDisplay;

	PlayerAbility ability;

	Animator animator;
	RectTransform rectTransform;

	bool isDragging = false;
	bool isValidDragLocation = false;
	int dragLocationX = -1;
	int dragLocationY = -1;

	bool isAvailable = true;

	void Start()
	{
		ability = GetComponent<PlayerAbility>();
		rectTransform = GetComponent<RectTransform>();

		animator = GetComponent<Animator>();
		animator.SetTrigger("Normal");
		isAvailable = true;

		costDisplay.text = ability.CashRequired().ToString();
	}

	void Update()
	{
		bool availableNow = ability.CashRequired() <= SaveData.GetCash() && !ability.inProgress;
		if (availableNow != isAvailable)
		{
			animator.SetTrigger(availableNow ? "Normal" : "Disabled");
			isAvailable = availableNow;
		}

		if (InputUtil.MousePressed() && isAvailable)
		{
			if (InputUtil.IsHovered(rectTransform, UserInterface.Camera2D()))
			{
				SaveData.AddCash(-ability.CashRequired());

				animator.SetTrigger("Pressed");
				ability.Trigger(0, 0); //tile indices are ignored now
				isDragging = true;
			}
		}
		else if (isDragging)
		{
			if (InputUtil.MouseReleased())
			{
				animator.SetTrigger("Release");
				isDragging = false;
			}
		}
	}

	void ShowRangePlots()
	{
		var mat = isValidDragLocation ? RangePlotQuad.MaterialType.WillPlaceValid :
										RangePlotQuad.MaterialType.WillPlaceInvalid;
		
		var plotData = RangePlots.GetPlotData(ability.RangePlotName(), 0);
		if (plotData != null)
		{
			
			for (int i = 0; i < plotData.Count; ++i)
			{
				RangeObjectPool.PlaceAt((int)(dragLocationX + plotData[i].x),
										(int)(dragLocationY + plotData[i].y),
										mat);
			}
		}
			
		RangeObjectPool.PlaceAt(dragLocationX, dragLocationY, mat);
	}

	//legacy
	void UpdateDragAndDrop()
	{
		if (InputUtil.MousePressed() && isAvailable)
		{
			if (InputUtil.IsHovered(rectTransform, UserInterface.Camera2D()))
			{
				animator.SetTrigger("Pressed");

				isDragging = true;
				isValidDragLocation = false;
				dragLocationX = -1;
				dragLocationY = -1;
			}
		}
		else if (isDragging)
		{
			if (InputUtil.MouseReleased())
			{
				if (isValidDragLocation)
				{
					ability.Trigger(dragLocationX, dragLocationY);
					SaveData.AddCash(-ability.CashRequired());
				}

				RangeObjectPool.Reset();
				animator.SetTrigger("Release");
				isDragging = false;
			}
			else if (InputUtil.MouseDrag())
			{
				if (InputUtil.IsWorldHovered())
				{
					var ray = MainCameraController.instance.cachedCamera.ScreenPointToRay(InputUtil.MousePosition());

					int tileX = -1;
					int tileY = -1;

					if (Landscape.instance.PickTile(ray, ref tileX, ref tileY))
					{
						if (tileX != dragLocationX || tileY != dragLocationY)
						{
							RangeObjectPool.Reset();

							dragLocationX = tileX;
							dragLocationY = tileY;

							if (dragLocationX != -1 && dragLocationY != -1)
							{
								isValidDragLocation = ability.IsValidTarget(dragLocationX, dragLocationY);
								ShowRangePlots();
							}
						}

						return;
					}
				}

				//placement failed somehow.
				isValidDragLocation = false;
				dragLocationX = -1;
				dragLocationY = -1;

				RangeObjectPool.Reset();
			}
		}
	}
}
