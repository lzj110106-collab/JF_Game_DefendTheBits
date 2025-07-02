using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileInstant : Projectile
{
	GameObject sourcePrefab;

	public float timeOut;
	float elapsed;

	public override void Initialise(Weapon parent, Vector3 from, Vector3 to, Character target)
	{
		CacheEffects();
		ResetEffects();

		sourcePrefab = parent.projectilePrefab;
		elapsed = 0.0f;

		gameObject.SetActive(true);
		gameObject.transform.position = to;

		parent.ApplySplashDamage(to, target);

		World.instance.AddProjectile(this, gameObject);
	}

	public override bool UpdateTick()
	{
		elapsed += World.frameTime;
		if (elapsed >= timeOut)
		{
			ProjectilePool.Return(sourcePrefab, gameObject);
			return false;
		}

		return true;
	}

	public override void OnPause(bool pause) {}
	public override void OnTimeScaleAdjusted(float timeScale) {}
	public override void OnCharacterKilled(Character c) {}
	public override void OnWorldReset() {}
}
