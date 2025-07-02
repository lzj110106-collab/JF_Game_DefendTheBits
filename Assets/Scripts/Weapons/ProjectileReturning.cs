using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PigeonCoopToolkit.Effects.Trails;

public class ProjectileReturning : Projectile
{
	GameObject sourcePrefab;
	Weapon parentWeapon;

	public bool destroyOnImpact = false;
	public float speed = 2.0f;
	public float radius = 0.5f;
	float range;

	public bool rotateObject;
	public float rotationSpeed;
	float rotation;

	Vector3 origin;
	Vector3 direction;

	float elapsed;
	float duration;

	//similar to ProjectileStraight, need to track prior hits for piercing weapons.
	//this stores hit directions as well so that the return hits still hit
	Character[] previouslyHit = new Character[16];
	bool[] previouslyHitDirection = new bool[16];
	int previouslyHitCount = 0;

	public override void Initialise(Weapon parent, Vector3 from, Vector3 to, Character target)
	{
		gameObject.SetActive(true);
		transform.position = from;

		CacheEffects();
		ResetEffects();

		sourcePrefab = parent.projectilePrefab;
		parentWeapon = parent;

		//auto calculate the range of the tile
		range = RangePlots.GetPlotRange(parentWeapon.parentTower.towerInfo.plotName);
		range = (range - 0.5f) * Landscape.instance.tileWidth;

		elapsed = 0.0f;
		duration = range * 2.0f / speed;

		float angle = MathUtil.GetAngleInDegreesToPositionXZ(from, to);		
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
		transform.position = from;

		origin = from;
		direction = Vector3.Normalize(to - from);
		World.instance.AddProjectile(this, gameObject);

		//make sure to reset the caching of prior hits
		previouslyHitCount = 0;
	}

	public override bool UpdateTick()
	{
		elapsed += World.frameTime;
		rotation += World.frameTime * rotationSpeed;

		if (elapsed >= duration)
		{
			ProjectilePool.Return(sourcePrefab, gameObject);
			return false;
		}

		//TODO: this is all very very similar to ProjectileStraightLine.

		var pos0 = transform.position;
			
		float t = Mathf.Sin(elapsed/duration * Mathf.PI);
		bool forwards = elapsed < duration*0.5f;

		var pos1 = origin + direction * t * range;
		var dir = Vector3.Normalize(pos1 - pos0);

		var mid = 0.5f * (pos0 + pos1);
		var searchRadius = Vector3.Magnitude(pos1 - mid) + radius;

		var result = World.instance.FindCharactersInArea(mid, searchRadius, CharacterType.Enemy);
		for (int i = 0; i < result.found; ++i)
		{
			var enemy = result.Get(i);

			bool wasPreviouslyHit = false;
			bool wasPreviouslyHitDirection = false;
			int wasPreviouslyHitIndex = 0;

			for (int j = 0; j < previouslyHitCount; ++j)
			{
				if (previouslyHit[j] == enemy)
				{
					wasPreviouslyHitIndex = j;
					wasPreviouslyHitDirection = previouslyHitDirection[j];

					wasPreviouslyHit = true;
					break;
				}
			}

			//same direction, skip this hit entirely.
			if (wasPreviouslyHit && wasPreviouslyHitDirection == forwards)
				continue;

			//project the enemy position onto the line formed by the previous
			//frames position and the new position. this is to avoid
			//tunneling effects for fast moving projectile 
			var enemyPos = result.Get(i).transform.position;
			enemyPos.y = pos0.y; //move to same plane as projectile

			var toEnemy = new Vector3(enemyPos.x - pos0.x, 0.0f, enemyPos.z - pos0.z);
			var projected = pos0 + dir * Vector3.Dot(dir, toEnemy);

			var distance = Vector3.Magnitude(enemyPos - projected);
			if (distance <= radius)
			{
				bool alive = parentWeapon.ApplyDamage(enemy);
				parentWeapon.pfx.Play(PFX.Weapon_OnHitEnemy, transform.position, true);

				if (destroyOnImpact)
				{
					ProjectilePool.Return(sourcePrefab, gameObject);
					return false;
				}
				else if (alive)
				{
					if (wasPreviouslyHit)
					{
						//update the hit direction. make sure the enemy doesnt end up 
						//int the list of hit things multiple times
						previouslyHitDirection[wasPreviouslyHitIndex] = forwards;
					}
					else if (previouslyHitCount < previouslyHit.Length)
					{
						previouslyHit[previouslyHitCount] = enemy;
						previouslyHitDirection[previouslyHitCount] = forwards;
						previouslyHitCount += 1;
					}
				}
			}
		}

		transform.position = pos1;

		if (rotateObject)
			transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);

		return true;
	}

	public override void OnPause(bool pause)
	{
	}

	public override void OnTimeScaleAdjusted(float timeScale)
	{
	}

	public override void OnCharacterKilled(Character c)
	{
		//see ProjectileStraightLine
		for (int i = 0; i < previouslyHitCount; ++i)
		{
			if (previouslyHit[i] == c)
			{
				previouslyHit[i] = previouslyHit[previouslyHitCount - 1];
				previouslyHitDirection[i] = previouslyHitDirection[previouslyHitCount - 1];
				previouslyHitCount -= 1;
			}
		}
	}

	public override void OnWorldReset()
	{
	}
}
