using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStraightLine : Projectile
{
	GameObject sourcePrefab;
	Weapon parentWeapon;

	public bool destroyOnImpact = false;
	public float timeOut = 2.0f;
	public float speed = 2.0f;
	public float radius = 0.5f;

	float elapsed;

	Vector3 direction;

	//straight line weapons can pierce targets
	Character[] previouslyHit = new Character[16];
	int previouslyHitCount = 0;

	public override void Initialise(Weapon parent, Vector3 from, Vector3 to, Character target)
	{
		gameObject.SetActive(true);
		transform.position = from;

		CacheEffects();
		ResetEffects();

		sourcePrefab = parent.projectilePrefab;
		parentWeapon = parent;
		elapsed = 0.0f;

		float angle = MathUtil.GetAngleInDegreesToPositionXZ(from, to);		
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
		transform.position = from;

		direction = Vector3.Normalize(to - from);
		World.instance.AddProjectile(this, gameObject);

		previouslyHitCount = 0;
	}

	public override bool UpdateTick()
	{
		elapsed += World.frameTime;
		if (elapsed >= timeOut)
		{
			ProjectilePool.Return(sourcePrefab, gameObject);
			return false;
		}

		var pos0 = transform.position;
		var pos1 = pos0 + direction * speed * World.frameTime;
		var dir = Vector3.Normalize(pos1 - pos0);

		var mid = 0.5f * (pos0 + pos1);
		var searchRadius = Vector3.Magnitude(pos1 - mid) + radius;

		var result = World.instance.FindCharactersInArea(mid, searchRadius, CharacterType.Enemy);
		for (int i = 0; i < result.found; ++i)
		{
			var enemy = result.Get(i);

			bool wasPreviouslyHit = false;
			for (int j = 0; j < previouslyHitCount && !wasPreviouslyHit; ++j)
				if (previouslyHit[j] == enemy)
					wasPreviouslyHit = true;

			if (wasPreviouslyHit)
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
					if (previouslyHitCount < previouslyHit.Length)
					{
						previouslyHit[previouslyHitCount] = enemy;
						previouslyHitCount += 1;
					}
				}
			}
		}
			
		transform.position = pos1;

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
		//make sure to remove the character from the hit list, otherwise
		//it might somehow get respawned and avoid damage. not sure
		//how that would ever happen, but maybe.
		for (int i = 0; i < previouslyHitCount; ++i)
		{
			if (previouslyHit[i] == c)
			{
				//order doesnt matter
				previouslyHit[i] = previouslyHit[previouslyHitCount - 1];
				previouslyHitCount -= 1;
			}
		}
	}

	public override void OnWorldReset()
	{
	}
}
