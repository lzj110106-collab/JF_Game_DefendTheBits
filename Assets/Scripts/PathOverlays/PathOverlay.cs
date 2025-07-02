using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathOverlayTileData
{
	public GameObject[] objects;
	public Animator[] objectAnimators;

	public float[] timers;

	public int locationX;
	public int locationY;

	public GameObject slopeObject;
};

public class PathOverlay
{
	public static PathOverlay instance;

	PathOverlayTileData[] tileData; 
	int[] tileIndexToOverlayIndex;

	public int minX { get; private set; }
	public int maxX { get; private set; }

	public int minY { get; private set; }
	public int maxY { get; private set; }

	bool doesRequireLandscapeRefresh = false;

	public PathOverlay()
	{
		instance = this;

		var landscape = Landscape.instance;
		var overlayCount = System.Enum.GetValues(typeof(PathOverlayData.Type)).Length;

		minX = int.MaxValue;
		minY = int.MaxValue;

		maxX = int.MinValue;
		maxY = int.MinValue;

		int pathTileCount = 0;
		for (var j = 0; j < landscape.h; ++j)
		{
			for (var i = 0; i < landscape.w; ++i)
			{
				if (landscape.HasFlag(i, j, TileFlag.HasPath_RuntimeAssigned))
				{
//					minX = Mathf.Min(i, minX);
//					maxX = Mathf.Max(i, maxX);
//
//					minY = Mathf.Min(j, minY);
//					maxY = Mathf.Max(j, maxY);

					pathTileCount += 1;
				}

				//calculating the centre of the map based on the buildable
				//tile area. feels like using the paths doesnt work because
				//they stretch offscreen and skew the results
				if (landscape.HasFlag(i, j, TileFlag.Buildable))
				{
					minX = Mathf.Min(i, minX);
					maxX = Mathf.Max(i, maxX);

					minY = Mathf.Min(j, minY);
					maxY = Mathf.Max(j, maxY);
				}
			}
		}

		tileData = new PathOverlayTileData[pathTileCount];
		tileIndexToOverlayIndex = new int[landscape.w * landscape.h];

		int writeIndex = 0;

		for (int j = 0; j < landscape.h; ++j)
		{
			for (int i = 0; i < landscape.w; ++i)
			{
				//only create overlay data where path exists, should make updates faster.
				if (Landscape.instance.HasFlag(i, j, TileFlag.HasPath_RuntimeAssigned))
				{
					tileData[writeIndex] = new PathOverlayTileData();
					tileData[writeIndex].objects = new GameObject[overlayCount];
					tileData[writeIndex].objectAnimators = new Animator[overlayCount];
					tileData[writeIndex].timers = new float[overlayCount];
					tileData[writeIndex].locationX = i;
					tileData[writeIndex].locationY = j;

					for (var k = 0; k < tileData[writeIndex].timers.Length; ++k)
						tileData[writeIndex].timers[k] = 0.0f;

					tileIndexToOverlayIndex[i + j*landscape.w] = writeIndex++;
				}
				else
				{
					tileIndexToOverlayIndex[i + j*landscape.w] = -1; //invalid
				}
			}
		}

//		Debug.Log("[PathOverlay] size: " + pathTileCount + " (" + minX + ", " + minY + " => " + maxX + ", " + maxY + ")");

		//auto-detect where path slope objects are and cache them in the tileData
		var propsContainer = UnityUtil.FindChild(World.instance.transform, "Props");
		if (propsContainer != null)
		{
			var pathSlopesContainer = UnityUtil.FindChild(propsContainer.transform, "PathSlopes");
			if (pathSlopesContainer != null)
			{
				foreach (Transform child in pathSlopesContainer.transform)
				{
					int tileX = 0;
					int tileY = 0;
					Landscape.instance.GetTileIndexFromPosition(child.position.x,
																child.position.z, 
																ref tileX, 
																ref tileY);

					var mappingIndex = tileX + tileY * Landscape.instance.w;
					var tileIndex = instance.tileIndexToOverlayIndex[mappingIndex];

					if (tileIndex != -1)
					{
						var data = instance.tileData[tileIndex];
						data.slopeObject = child.gameObject;

						//Debug.Log("added slope object to: " + tileX + " " + tileY + " | " + tileIndex);
					}
				}
			}
		}
	}

	~PathOverlay()
	{
		//return objects to pool
		Clear(false); 
	}

	public static void PlaceOverlay(PathOverlayData.Type type, int tileX, int tileY, float duration)
	{
		if (instance == null)
			return;

		var mappingIndex = tileX + tileY * Landscape.instance.w;
		var tileIndex = instance.tileIndexToOverlayIndex[mappingIndex];

		if (tileIndex != -1)
		{
			var data = instance.tileData[tileIndex];
			var sourceData = PathOverlayDatabase.Get(type);

			if (data.timers[(int)type] <= 0.0f)
			{
				GameObject obj = null;

				if (data.slopeObject == null)
				{
					obj = PathOverlayPool.Get(sourceData.prefab);
					obj.SetActive(true);
					obj.transform.SetParent(null, false);
					obj.transform.position = Landscape.instance.GetTileCentre(tileX, tileY);
					obj.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 4) * 90, Vector3.up);

					//dont adjust the landscape for slope tiles. slope tiles just replace their slope object
					if (sourceData.doesAdjustLandscape)
					{
						Landscape.instance.AddFlag(tileX, tileY, TileFlag.HasLoweredPath_RuntimeAssigned);
						instance.doesRequireLandscapeRefresh = true;

						//cant add the lowered offset to GetAdjustedTileCentre because then the
						//range plots end up rendering below the overlay prefab.
						var position = obj.transform.position;
						position.y -= Landscape.instance.tileHeight;
						obj.transform.position = position;
					}
				}
				else
				{
					data.slopeObject.SetActive(false);

					obj = PathOverlayPool.Get(sourceData.prefabSlope);
					obj.SetActive(true);
					obj.transform.SetParent(null, false);
					obj.transform.rotation = data.slopeObject.transform.rotation;
					obj.transform.localScale = data.slopeObject.transform.localScale;

					//as above. no need to alter the landscape though
					var position = data.slopeObject.transform.position;
					position.y -= 1.5f*Landscape.instance.tileHeight; //TODO: magic number
					obj.transform.position = position;
				}

				data.objects[(int)type] = obj;
				data.objectAnimators[(int)type] = obj.GetComponentInChildren<Animator>(true);
			}
			else
			{
				//already exists, reset the animation only if its currently fading out
				if (data.timers[(int)type] <= sourceData.fadeOutDuration)
					data.objectAnimators[(int)type].Play(sourceData.entryAnimation);
			}

			//reset the timer
			data.timers[(int)type] = duration;
		}
	}

	public static void GetCentreTile(out int tileX, out int tileY)
	{
		if (instance != null)
		{
			tileX = Mathf.RoundToInt((instance.minX + instance.maxX) * 0.5f);
			tileY = Mathf.RoundToInt((instance.minY + instance.maxY) * 0.5f);
		}
		else
		{
			tileX = 0;
			tileY = 0;
		}
	}

	public static PathOverlayTileData[] AllTileData()
	{
		return instance != null ? instance.tileData : null; 
	}

	public void UpdateTick()
	{
		for (int i = 0; i < tileData.Length; ++i)
		{
			var data = tileData[i];
			for (int j = 0; j < data.timers.Length; ++j)
			{
				if (data.timers[j] <= 0.0f)
					continue;

				var sourceData = PathOverlayDatabase.Get(j);

				float start = data.timers[j];
				data.timers[j] -= World.frameTime;

				if (data.timers[j] <= 0.0f)
				{
					if (data.slopeObject == null)
					{
						PathOverlayPool.Return(sourceData.prefab, data.objects[j]);

						if (sourceData.doesAdjustLandscape)
						{
							Landscape.instance.RemoveFlag(data.locationX, data.locationY, TileFlag.HasLoweredPath_RuntimeAssigned);
							doesRequireLandscapeRefresh = true;
						}
					}
					else
					{
						PathOverlayPool.Return(sourceData.prefabSlope, data.objects[j]);
						data.slopeObject.SetActive(true);
					}

					data.objects[j] = null;
					data.objectAnimators[j] = null;
					data.timers[j] = 0.0f;

					continue;
				}
				else
				{
					//attempt to trigger fade out animation
					float fadeOut = sourceData.fadeOutDuration;

					if (start > fadeOut && data.timers[j] <= fadeOut)
						data.objectAnimators[j].SetTrigger(sourceData.exitTrigger);
				}
			}
		}
	}

	public void LateUpdate()
	{
		if (doesRequireLandscapeRefresh)
		{
			Landscape.instance.RebuildTowerMesh();
			doesRequireLandscapeRefresh = false;
		}
	}
		
	public void Clear(bool rebuildMesh = true)
	{
		for (int i = 0; i < tileData.Length; ++i)
		{
			var data = tileData[i];
			for (int j = 0; j < data.timers.Length; ++j)
			{
				if (data.timers[j] < 0.0f)
					continue;

				//return overlay data to pool
				var sourceData = PathOverlayDatabase.Get(j);
				var sourcePrefab = data.slopeObject == null ? sourceData.prefab : sourceData.prefabSlope;
				PathOverlayPool.Return(sourcePrefab, data.objects[j]);
					
				data.objects[j] = null;
				data.objectAnimators[j] = null;
				data.timers[j] = 0.0f;
			}
		}

		//make sure to return the landscape to its normal condition
		if (rebuildMesh)
		{
			for (int i = 0; i < tileData.Length; ++i)
			{
				Landscape.instance.RemoveFlag(tileData[i].locationX, 
											  tileData[i].locationY,
											  TileFlag.HasLoweredPath_RuntimeAssigned);
			}

			Landscape.instance.RebuildTowerMesh();
			doesRequireLandscapeRefresh = false;
		}

		//and turn slope objects back on as well
		{
			for (int i = 0; i < tileData.Length; ++i)
				if (tileData[i].slopeObject != null)
					tileData[i].slopeObject.SetActive(true);
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(Landscape.instance.GetTileCentre(minX, minY), 0.25f);

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(Landscape.instance.GetTileCentre(maxX, maxY), 0.25f);

		int centreX;
		int centreY;
		GetCentreTile(out centreX, out centreY);

		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(Landscape.instance.GetTileCentre(centreX, centreY), 0.25f);
	}
}