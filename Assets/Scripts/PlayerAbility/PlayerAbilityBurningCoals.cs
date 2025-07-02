using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityBurningCoals : PlayerAbility
{
	public GameObject pathObjectPrefab;

	public int cashRequired = 10;
	public override int CashRequired() { return cashRequired; }

	public string rangePlotName = "RNG2";
	public override string RangePlotName() { return rangePlotName; }

	public float duration = 5.0f;
	public float offAnimationLength = 0.5f;

	public float damageFrequency = 0.25f;
	float elapsed = 0.0f;
	float damageElapsed = 0.0f;

	public float burnDamage = 4.0f;
	public float burnDuration = 4.0f;

	int locationX;
	int locationY;

	GameObject pathObjectContainer;
	List<GameObject> pathObjects = new List<GameObject>();
	List<Animator> pathObjectAnimators = new List<Animator>();
	int pathObjectsVisible = 0;

	void Start()
	{
		var plotData = RangePlots.GetPlotData(rangePlotName, 0);
		pathObjectContainer = new GameObject("path_objects");

		//range plots dont include the tower centre, so +1 to account for that
		for (var i = 0; i < plotData.Count + 1; ++i)
		{
			pathObjects.Add(GameObject.Instantiate(pathObjectPrefab));
			pathObjects[i].transform.SetParent(pathObjectContainer.transform, false);
			pathObjects[i].SetActive(false);

			pathObjectAnimators.Add(pathObjects[i].GetComponentsInChildren<Animator>(true)[0]);
			pathObjectAnimators[i].enabled = false;
		}

		pathObjectsVisible = 0;
		inProgress = false;
	}

	public override bool Trigger(int tileX, int tileY)
	{
		var plotData = RangePlots.GetPlotData(rangePlotName, 0);
		if (plotData != null)
		{
			//remember to account for the range plot centre tile
			{
				AddPathTilePrefab(tileX, tileY);

				for (int i = 0; i < plotData.Count; ++i)
					AddPathTilePrefab((int)(tileX + plotData[i].x), 
						              (int)(tileY + plotData[i].y));
			}

			//rebuild the mesh and fix up path object positioning
			Landscape.instance.RebuildTowerMesh();
			RepositionPathObjects();

			inProgress = true;
			elapsed = 0.0f;
			damageElapsed = 0.0f;

			locationX = tileX;
			locationY = tileY;
		}

		Landscape.instance.RebuildTowerMesh();
		return true;
	}

	public override void UpdateTick()
	{
		var plotData = RangePlots.GetPlotData(rangePlotName, 0);

		float start = elapsed;
		elapsed += World.frameTime;
		damageElapsed += World.frameTime;

		float animTrigger = duration - offAnimationLength;
		if (start < animTrigger && elapsed >= animTrigger)
		{
			for (int i = 0; i < pathObjectsVisible; ++i)
				pathObjectAnimators[i].SetTrigger("Off");
		}

		if (elapsed >= duration)
		{
			Restart();
		}
		else if (damageElapsed >= damageFrequency)
		{
			var query = World.instance.FindCharactersInArea(plotData, locationX, locationY);
			for (var i = 0; i < query.found; ++i)
				query.Get(i).AddStatusEffect(WeaponStatusEffectType.Burn, burnDamage, burnDuration);

			damageElapsed -= damageFrequency;
		}

		for (var i = 0; i < pathObjectsVisible; ++i)
			pathObjectAnimators[i].Update(World.frameTime);
	}

	public override void Restart()
	{
		if (inProgress)
		{
			inProgress = false;

			var plotData = RangePlots.GetPlotData(rangePlotName, 0);
			for (int i = 0; i < plotData.Count; ++i)
			{
				int x = (int)(locationX + plotData[i].x);
				int y = (int)(locationY + plotData[i].y);

				Landscape.instance.RemoveFlag(x, y, TileFlag.HasLoweredPath_RuntimeAssigned);
			}

			Landscape.instance.RemoveFlag(locationX, locationY, TileFlag.HasLoweredPath_RuntimeAssigned);
			Landscape.instance.RebuildTowerMesh();

			ClearPathObjects();
		}
	}

	void AddPathTilePrefab(int tileX, int tileY)
	{
		if (Landscape.instance.HasFlag(tileX, tileY, TileFlag.HasPath_RuntimeAssigned))
		{
			//this is incorrect, because the landscape is about to be refreshed and
			//this will lower the tiles that this object sits on. so basically
			//all thats happening here is that we are caching x/y for later
			pathObjects[pathObjectsVisible].transform.position = new Vector3(tileX, tileY, 0.0f);
			pathObjects[pathObjectsVisible].SetActive(true);
			pathObjectsVisible += 1;

			Landscape.instance.AddFlag(tileX, tileY, TileFlag.HasLoweredPath_RuntimeAssigned);
		}
	}

	void RepositionPathObjects()
	{
		//proper placement of visible path objects
		for (var i = 0; i < pathObjectsVisible; ++i)
		{
			var x = (int)pathObjects[i].transform.position.x;
			var y = (int)pathObjects[i].transform.position.y;

			var world = Landscape.instance.GetTileCentre(x, y);
			world.y -= Landscape.instance.towerDepth * Landscape.instance.tileHeight;
			pathObjects[i].transform.position = world;
		}
	}

	void ClearPathObjects()
	{
		for (var i = 0; i < pathObjects.Count; ++i)
			pathObjects[i].SetActive(false);

		pathObjectsVisible = 0;
	}
}
