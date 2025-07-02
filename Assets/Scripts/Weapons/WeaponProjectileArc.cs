using UnityEngine;
using System.Collections;
using PigeonCoopToolkit.Effects.Trails;

public class WeaponProjectileArc : Projectile
{
	GameObject sourcePrefab;

	Weapon parentWeapon;
	ParabolaHelper parabolaHelper = new ParabolaHelper();
	
	public float gravity = 200.0f;
	public float launchAngle = 45.0f;
	public float speedMultiplier = 1.0f;

	public bool alwaysHitTarget = false;
	Character targetCharacter;

	public bool destroyOnGroundImpact = true;

	public float postImpactDuration = 1.0f;
	public float postImpactTimePerTick = 0.25f;
	float postImpactTickTimer = 0;

	enum State { Default, PostImpact, Finished };
	State state;
	float timeElapsed;

	public override void Initialise(Weapon weapon, Vector3 from, Vector3 to, Character target)
	{
		gameObject.SetActive(true);
		transform.position = from;

		CacheEffects();
		ResetEffects();

		sourcePrefab = weapon.projectilePrefab;
		parentWeapon = weapon;
		targetCharacter = target;

		parabolaHelper.Init(from, to, launchAngle, gravity);
		World.instance.AddProjectile(this, gameObject);

		if ((weapon.category & WeaponCategory.Explosive) != 0)
		{
			//explosive weapons dont track the target character, they just
			//fire at the targets current location, and then rely on
			//the explosion radius to deal damage to whatever is
			//in range when the projectile hits the ground
			targetCharacter = null;
		}

		SetState(State.Default);
	}

	void SetState(State newState)
	{
		state = newState;
		timeElapsed = 0.0f;
	}

	public override void OnPause(bool pause) {}
	public override void OnTimeScaleAdjusted(float timeScale) {}
	
	public override bool UpdateTick()
	{
		switch (state)
		{
		case State.Default:		
			UpdateParabolicMotion();	
			break;

		case State.PostImpact:	
			UpdatePostImpact();			
			break;

		case State.Finished:	
			ProjectilePool.Return(sourcePrefab, gameObject); 
			return false;	
		}

		return true;
	}

	public void UpdatePostImpact()
	{
		timeElapsed += World.frameTime;
		postImpactTickTimer += World.frameTime;

		if (postImpactTickTimer >= postImpactTimePerTick)
		{
			postImpactTickTimer -= postImpactTimePerTick;
			PerformPostImpactTick();
		}

		if (timeElapsed >= postImpactDuration)
			state = State.Finished;
	}

	public void UpdateParabolicMotion()
	{
		if (alwaysHitTarget && targetCharacter != null)
		{
			//default to normal character position if the art transform isnt set up for whatever reason
			var dest = targetCharacter.artTransform == null ? targetCharacter.transform : targetCharacter.artTransform;

			//while the target is still valid, keep recalculating the arc 
			//of the parabola while preserving the update time.
			float currentTime = parabolaHelper.currentTime;
			parabolaHelper.Init(parentWeapon.transform.position, dest.position, launchAngle, gravity);
			parabolaHelper.currentTime = currentTime;
		}

		//see if we have hit the ground yet
		if (parabolaHelper.Update(World.frameTime * speedMultiplier))
		{
			SetState(destroyOnGroundImpact ? State.Finished : State.PostImpact);

			if (parentWeapon != null)
			{
				parentWeapon.pfx.Play(PFX.Weapon_OnHitGround, parabolaHelper.currentPosition, true);
				ApplyDirectDamage();
				ApplySplashDamage();
			}
		}

		//fix up our transform to follow the trajectory
		transform.rotation = Quaternion.LookRotation(parabolaHelper.currentPosition - transform.position);
		transform.position = parabolaHelper.currentPosition;
	}

	void OnDrawGizmos()
	{
		if (parentWeapon != null)
		{
			var ground = Landscape.SnapToMesh(transform.position);
			DebugDrawUtil.Draw(ground, parentWeapon.transform.position, parentWeapon.debugColor);
				
			if (parentWeapon.DealsSplashDamage())
				DebugDrawUtil.DrawCircleXZ(ground, parentWeapon.weaponData.projectileSplashRadius, parentWeapon.debugColor);
		}
			
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(parabolaHelper.targetPosition, 0.25f);
	}

	void PerformPostImpactTick()
	{
		ApplySplashDamage();
	}

	void ApplyDirectDamage()
	{
		if (alwaysHitTarget && targetCharacter != null)
		{
			CameraShake.instance.TriggerShake(parentWeapon.onImpactCameraShake);
			parentWeapon.ApplyDamage(targetCharacter);
		}
	}

	void ApplySplashDamage()
	{
		parentWeapon.ApplySplashDamage(transform.position, targetCharacter);
	}

	public override void OnCharacterKilled(Character c)
	{
		if (targetCharacter == c)
			targetCharacter = null;
	}

	public override void OnWorldReset()
	{
		//null this so we dont access it and its artTransform, which
		//may be null due to it being recycled as well.
		targetCharacter = null;

		parentWeapon = null;
		state = State.Finished;
	}
}
