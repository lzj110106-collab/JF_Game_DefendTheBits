using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponTargetControllerSortHelper: Comparer<Character>
{
	public void Sort(WorldQuery<Character> query)
	{
		System.Array.Sort(query.objects, 0, query.found, this);
	}
		
	public override int Compare(Character x, Character y)
	{
		float remaining0 = x.followPath.CalculateRemainingPathLength();
		float remaining1 = y.followPath.CalculateRemainingPathLength();

		return remaining0.CompareTo(remaining1);
	}
}

public class WeaponTargetController
{
	Weapon weapon;
	public Character[] targets { get; private set; }
	public Vector3[] targetLocations { get; private set; }
	public int currentTargetCount { get; private set; }

	WeaponTargetControllerSortHelper sortHelper = new WeaponTargetControllerSortHelper();

	public WeaponTargetController(Weapon parentWeapon)
	{
		weapon = parentWeapon;

		targets = new Character[weapon.weaponData.maxTargets];
		targetLocations = new Vector3[weapon.weaponData.maxTargets];
		currentTargetCount = 0;
	}

	public bool FindTarget() 
	{
		currentTargetCount = 0;;

		if (weapon == null ||
			weapon.plotDataMinimalSet == null ||
			weapon.plotDataMinimalSet.Count == 0)
		{
			//this weapon doesnt overlap any paths in the map
			return false;
		}

		var potentialTargets = World.FindCharactersInTileRange(weapon, CharacterType.Enemy);
		if (potentialTargets.found > 0)
		{
			//prioritise the targets before storing as many as we can
			sortHelper.Sort(potentialTargets);

			for (int i = 0; i < targets.Length && i < potentialTargets.found; ++i)
			{
				targets[i] = potentialTargets.Get(i);
				currentTargetCount += 1;
			}

			return true;
		}

		return false;
	}

	public bool HasAnyTarget()
	{
		return GetAnyTarget() != null;
	}

	public void SetTargetCap(int cap)
	{
		targets = new Character[cap];
		targetLocations = new Vector3[cap];
	}

	public Character GetTarget(int index)
	{
		if (index < targets.Length)
			return targets[index];

		return null;
	}

	public Character GetAnyTarget()
	{
		for (int i = 0; i < currentTargetCount; ++i)
			if (targets[i] != null)
				return targets[i];

		return null;
	}
		
	public void OnEnemyDestroyed(Character e)
	{
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] == e)
			{
				//store last known position. eg. so that slow firing weapons
				//with splash damage dont get constantly interrupted
				//by faster towers killing their target
				targetLocations[i] = targets[i].transform.position;
				targets[i] = null;
			}
		}
	}

	public void RefreshTargetLocations()
	{
		for (int i = 0; i < targets.Length; ++i)
			if (targets[i] != null)
				targetLocations[i] = targets[i].transform.position;
	}

	public void Clear()
	{
		currentTargetCount = 0;
	}
}
