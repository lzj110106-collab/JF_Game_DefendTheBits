using UnityEngine;
using System.Collections;
using PigeonCoopToolkit.Effects.Trails;

public class WeaponProjectileTrack : Projectile
{
	GameObject sourcePrefab;
	[HideInInspector] public Weapon parentWeapon;

	[HideInInspector] public Character targetCharacter;
	[HideInInspector] public Vector3 targetPosition;

	private Vector3 currentPosition;
	private Vector3 currentVelocity;
	public float speed;
	public float turnSpeed;
	public float timeOut;

	private Transform cachedTransform;
	
	void Awake () 
	{
		cachedTransform = GetComponent<Transform>();
	}
		
	public override void Initialise(Weapon parent, Vector3 firePosition, Vector3 toPosition, Character target)
	{
		gameObject.SetActive(true);
		CacheEffects();

		sourcePrefab = parent.projectilePrefab;
		parentWeapon = parent;

		targetCharacter = target;
		targetPosition = toPosition;

		//initialise with max speed towards the target
		currentPosition = firePosition;
		currentVelocity = speed * Vector3.Normalize(targetPosition - firePosition);
		transform.position = firePosition;

		ResetEffects();
		World.instance.AddProjectile(this, gameObject);
	}

	public override void OnPause(bool pause) {}
	public override void OnTimeScaleAdjusted(float timeScale) {}

	public override bool UpdateTick() 
	{
		//if the enemy is killed by something else, then this will be
		//null and the projectile will move to the last position
		//that enemy was at before it was killed
		if (targetCharacter != null)
			targetPosition = targetCharacter.position;

		timeOut -= World.frameTime;
		if (timeOut <= 0)
		{
			DealSplashDamage();
			return false;
		}
			
		if (currentPosition.y <= Landscape.SnapToMesh(currentPosition).y)
		{
			DealSplashDamage();
			return false; //hit the ground.
		}

		float distanceToTarget = Vector3.Magnitude(currentPosition - targetPosition);
		if (distanceToTarget < speed * World.frameTime)
		{
			parentWeapon.ApplyDamage(targetCharacter);
			DealSplashDamage();
			return false;
		}
		else
		{
			var toTarget = targetPosition - currentPosition;
			currentVelocity += turnSpeed * World.frameTime * Vector3.Normalize(toTarget);
			currentVelocity = speed * Vector3.Normalize(currentVelocity);

			currentPosition += currentVelocity * World.frameTime;
			cachedTransform.position = currentPosition;

			float angle = MathUtil.GetAngleInDegreesToPositionXZ(currentPosition, targetPosition);
			transform.localRotation = Quaternion.AngleAxis(angle + 180.0f, Vector3.up);
		}

		return true;
	}

	//TODO: untangle the UpdateTick function and get rid of this call entirely.
	void DealSplashDamage()
	{
		parentWeapon.ApplySplashDamage(targetPosition, targetCharacter);

		ProjectilePool.Return(sourcePrefab, gameObject);
	}

	public override void OnCharacterKilled(Character c)
	{
		if (targetCharacter == c)
			targetCharacter = null;
	}

	public override void OnWorldReset()
	{
	}
}
