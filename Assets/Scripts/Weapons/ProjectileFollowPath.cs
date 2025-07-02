using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PigeonCoopToolkit.Effects.Trails;

public class ProjectileFollowPath : Projectile
{
	GameObject sourcePrefab;
	GameObject sourcePrefabPath;

	Weapon weapon;

	GameObject path;
	GameObject pathLocator;
	Animator animator;
	public Animator childAnimator;
	public CameraShake.Shake impactCameraShake;

	HashSet<Character> previouslyHit;
	int maxEnemiesHit;

	//the firing animations are scaled. so simply adjusting animator
	//speeds to match the values coming in from OnTimeScaleAdjusted
	//wont work correctly.
	[HideInInspector] public float localTimeScale;

	public void Initialise(Weapon parentWeapon, GameObject fireLocation, GameObject pathPrefab, Vector3 pathRotation)
	{
		gameObject.SetActive(true);
		CacheEffects();

		sourcePrefab = weapon.projectilePrefab;
		sourcePrefabPath = pathPrefab;

		weapon = parentWeapon;

		previouslyHit = new HashSet<Character>();
		maxEnemiesHit = weapon.maxEnemiesHit;

		path = ProjectilePool.Get(pathPrefab);
		path.gameObject.SetActive(true);
		path.transform.position = fireLocation.transform.position;
		path.transform.rotation = Quaternion.LookRotation(pathRotation);

		animator = path.GetComponentsInChildren<Animator>(true)[0];
		pathLocator = path.transform.Find("PathLocator").gameObject;

		// !!!!!!!!!!!! PLEASE FIX THIS !!!!!!!!!!! nothing sets localTimeScale from the weapon side so fastforward doesn't work for paths
		localTimeScale = 1.0f;
		OnTimeScaleAdjusted(World.instance.timeScale);

		transform.position = pathLocator.transform.position;
		transform.rotation = pathLocator.transform.rotation;
		ResetEffects();

		World.instance.AddProjectile(this, gameObject);
	}

	public override void OnPause(bool pause)
	{
		animator.speed = pause ? 0 : World.instance.timeScale;
		if(childAnimator)
			 childAnimator.speed = animator.speed;

		//TODO: unsure about this. everything seems to use UnityEngine.TrailRenderer
		foreach (var trail in GetComponentsInChildren<TrailRenderer_Base>(true))
			trail._pause = pause;
	}

	public override void OnTimeScaleAdjusted(float timeScale)
	{
		animator.speed = localTimeScale * timeScale;
		if(childAnimator)
			 childAnimator.speed = animator.speed;
	}

	public override bool UpdateTick()
	{
		//lock our position to the animated locator
		transform.position = pathLocator.transform.position;
		transform.rotation = pathLocator.transform.rotation;

		//TODO: not sure if this is the best way to kill a projectile
		if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
		{
			DestroyProjectile();
			return false;
		}
		else
		{
			var targets = World.instance.FindCharactersInArea(transform.position, 0.5f, CharacterType.Enemy);
			for (int i = 0; i < targets.found; ++i)
			{
				var target = targets.Get(i);

				if (previouslyHit.Contains(target))
					continue;

				CameraShake.instance.TriggerShake(impactCameraShake);
				weapon.ApplyDamage(target);
				previouslyHit.Add(target);

				//destroy projectile after hitting X number of enemies
				if (maxEnemiesHit >= 0 && maxEnemiesHit >= previouslyHit.Count)
				{
					DestroyProjectile();
					return false;
				}
				else
				{
					//TODO: play PFX?
				}
			}
		}

		return true;
	}
		
	public override void OnCharacterKilled(Character c)
	{
		//do nothing. we dont track targets.
	}

	public override void OnWorldReset()
	{
		ProjectilePool.Return(sourcePrefabPath, path.gameObject);
	}

	public void DestroyProjectile()
	{
		ProjectilePool.Return(sourcePrefab, gameObject);
		ProjectilePool.Return(sourcePrefabPath, path.gameObject);
	}
}
