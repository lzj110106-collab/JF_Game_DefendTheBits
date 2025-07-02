using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//NB: this class is deprecated. keeping it around just in case.
//NB: will need to fix character pathing to use the new PathNetworkNodes

public class TowerSpawnDefenders : Tower 
{
	//making this a struct so it groups nicely in the inspector
	[System.Serializable]
	public struct SpawnData
	{
		public GameObject characterPrefab;
		public float range;
		public float timeBetweenSpawns;
		public int maxSpawnedCharacters;
	}
	
	public SpawnData spawnData;
	public Color debugColor = Color.white;

	private float elapsed = 0;
	private Vector3 rallyPoint;

	private NonPlayerCharacter[] characters;
	private int charactersControlled;

	void Awake()
	{
		characters = new NonPlayerCharacter[spawnData.maxSpawnedCharacters];
		charactersControlled = 0;
	}

	void Start()
	{
		//initialise the rally point to be as close to an enemy path as possible
//		float closest = float.MaxValue;
//		foreach (var path in World.instance.paths)
//		{
//			Vector3 temp = path.ClosestPointOnPath(transform.position);
//			float dist = Vector3.SqrMagnitude(temp - transform.position);
//			if (dist < closest)
//			{
//				rallyPoint = temp;
//				closest = dist;
//			}
//		}

//		float toRallyPoint = Vector3.Magnitude(rallyPoint - cachedTransform.position);
//		if (toRallyPoint < spawnData.range)
//		{
//			//closest rally point is outside of our range, truncate the position 
//			//so that it is still near the nearest path, just closer to the tower
//			rallyPoint = 0.75f*toRallyPoint*Vector3.Normalize(rallyPoint - cachedTransform.position);
//		}
	}

	public override void OnCharacterKilled(Character character)
	{
		//we dont have weapons. do nothing. our controlled characters
		//will be informed of this characters death in Character.OnCharacterKilled
	}

	public override bool UpdateTick()
	{
		if (spawnData.characterPrefab == null || isBeingPlaced)
			return true;

		elapsed += World.frameTime;

		if (elapsed > spawnData.timeBetweenSpawns && charactersControlled < spawnData.maxSpawnedCharacters)
		{
			var c = World.instance.SpawnCharacter(spawnData.characterPrefab);
			if (c is NonPlayerCharacter)
			{
				var npc = (NonPlayerCharacter)c;

				//spawn NPCs so that they are sitting on a circle around the rally point
				float incr = 2.0f*Mathf.PI/spawnData.maxSpawnedCharacters;
				float theta = incr * charactersControlled;
				float radius = 0.5f; 

				Vector3 offset = new Vector3(radius * Mathf.Cos(theta), 0, radius * Mathf.Sin(theta));
				Vector3 destination = rallyPoint + offset;

				npc.Initialise(this, transform.position, destination);

				characters[charactersControlled] = npc;
				charactersControlled += 1;
			}

			elapsed = 0;
		}

		return true;
	}

	public void OnDrawGizmos()
	{
	#if UNITY_EDITOR
		DebugDrawUtil.DrawCircleXZ(transform.position, spawnData.range, debugColor);
		DebugDrawUtil.Draw(transform.position, rallyPoint, debugColor);
	#endif
	}
}
