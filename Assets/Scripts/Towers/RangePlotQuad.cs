using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangePlotQuad : MonoBehaviour 
{
	public enum MaterialType { Normal, WillPlaceValid, WillPlaceInvalid };
		
	public Material towerHasBeenPlaced;

	public Material placementPositionValid;
	public Material placementPositionInvalid;

	public void UpdateMaterial(MaterialType type)
	{
		var renderer = GetComponent<MeshRenderer>();
		if (renderer != null)
		{
			switch (type)
			{
			case MaterialType.Normal:
				renderer.sharedMaterial = towerHasBeenPlaced;
				break;

			case MaterialType.WillPlaceValid:
				renderer.sharedMaterial = placementPositionValid;
				break;

			case MaterialType.WillPlaceInvalid:
				renderer.sharedMaterial = placementPositionInvalid;
				break;
			}
		}
	}
}
