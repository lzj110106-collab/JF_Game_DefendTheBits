using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: legacy class
//TODO: pool sub-towers if this ever gets re-implemented

public class TowerSpawnSubTowers : Tower
{
	public GameObject subTowerPrefab;
	public List<Tower> subTowers { get; private set; }

	int maxSubTowers = 3;
	int maxBlockRadius = 1;

	public float minTimeBetweenSubTowerSpawn = 5.0f;
	float spawnTimer = 0.0f;

	//fix the order that things subtowers get spawned in 
	System.Random rng = new System.Random(0);

	public void OnDestroy()
	{
		//destroy all our subtowers
		for (var i = 0; i < subTowers.Count; ++i)
		{
			World.instance.RemoveTower(subTowers[i]);
			GameObject.Destroy(subTowers[i]);
		}
	}
		
	public override bool UpdateTick()
	{
		base.UpdateTick();

		//trivial cases
		if (subTowerPrefab == null || subTowers.Count >= maxSubTowers)
			return true;
		
		spawnTimer += World.frameTime;
		if (spawnTimer >= minTimeBetweenSubTowerSpawn)
		{
			//rng.Next is an exclusive max value, so +1
			int subTileX = rng.Next(tileX - maxBlockRadius, tileX + maxBlockRadius + 1);
			int subTileY = rng.Next(tileX - maxBlockRadius, tileX + maxBlockRadius + 1);

			if (!Landscape.instance.IsValidTile(subTileX, subTileY))
				return true;

			if (!Landscape.instance.HasFlag(subTileX, subTileY, TileFlag.Buildable))
				return true;

			var subTowerObject = GameObject.Instantiate(subTowerPrefab);
			var subTowerComponent = subTowerObject.GetComponent<Tower>();

			if (subTowerComponent == null)
			{
				Debug.Log("[TowerSpawnSubTowers] invalid sub tower prefab: " + subTowerPrefab.name + " for tower type: " + name);
				maxSubTowers = 0; //dont flood the console with error messages
			}
			else
			{
				//tower set up
				subTowerComponent.SetTile(subTileX, subTileY);

				//add to the world
				subTowers.Add(subTowerComponent);
				World.instance.AddTower(subTowerComponent);

				spawnTimer = 0.0f;
			}
		}

		return true;
	}
}
