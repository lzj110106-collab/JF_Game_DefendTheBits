using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RangeObjectPool : MonoBehaviour 
{
	static RangeObjectPool instance = null;

	public GenericPrefabPool source;

	public GameObject prefabQuad;
	public GameObject prefabQuadAngled;
	public GameObject prefabRotationArrow;

	GameObject activeQuads;
	GameObject rotationArrowInstance;

	void Awake()
	{
		instance = this;

		activeQuads = new GameObject("range_quads");
		source.Initialise("RangePlots");

		rotationArrowInstance = GameObject.Instantiate(prefabRotationArrow);
	}

	void OnDestroy()
	{
		instance = null;
		Destroy(activeQuads);
	}

	public static void PlaceAt(int tileX, int tileY, RangePlotQuad.MaterialType materialType)
	{
		GameObject quad = null;
		float zFightOffset = 0.01f;

		var position = Landscape.instance.GetAdjustedTileCentre(tileX, tileY);
		var angledOffset = Landscape.instance.tileHeight * 2.0f + zFightOffset;
		int rotationX = 0;
		int rotationY = 0;


		if (Landscape.instance.HasFlag(tileX, tileY, TileFlag.PathSlopeLeft_RuntimeAssigned))
		{
			quad = instance.source.Get(instance.prefabQuadAngled);

			position.y += angledOffset;
			rotationX = 117;
			rotationY = 90;
		}
		else if (Landscape.instance.HasFlag(tileX, tileY, TileFlag.PathSlopeRight_RuntimeAssigned))
		{
			quad = instance.source.Get(instance.prefabQuadAngled);

			position.y += angledOffset;
			rotationX = 117;
			rotationY = 270;
		}
		else if (Landscape.instance.HasFlag(tileX, tileY, TileFlag.PathSlopeUp_RuntimeAssigned))
		{
			quad = instance.source.Get(instance.prefabQuadAngled);

			position.y += angledOffset;
			rotationX = 117;
			rotationY = 180;
		}
		else if (Landscape.instance.HasFlag(tileX, tileY, TileFlag.PathSlopeDown_RuntimeAssigned))
		{
			quad = instance.source.Get(instance.prefabQuadAngled);

			position.y += angledOffset;
			rotationX = 117;
			rotationY = 0;
		}
		else
		{
			quad = instance.source.Get(instance.prefabQuad);

			position.y += zFightOffset;
			rotationX = 90;
		}

		var component = quad.GetComponent<RangePlotQuad>();
		if (component != null)
			component.UpdateMaterial(materialType);

		quad.transform.SetParent(instance.activeQuads.transform, false);
		quad.transform.position = position;
		quad.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0.0f);
		quad.SetActive(true);
	}

	public static void Reset()
	{
		if (instance != null)
			instance.source.ResetPool();
	}

	public static void ShowPlacementArrow(Tower parentTower)
	{
		if (instance != null)
		{
			instance.rotationArrowInstance.transform.SetParent(parentTower.towerArtInfo.swivelPoint, false);
			instance.rotationArrowInstance.SetActive(true);
		}
	}

	public static void ClearPlacementArrow()
	{
		if (instance != null)
		{
			instance.rotationArrowInstance.transform.SetParent(null, false);
			instance.rotationArrowInstance.SetActive(false);
		}
	}
}
