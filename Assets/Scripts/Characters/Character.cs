using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CharacterType
{
	Enemy,
	NPC,
	Player
};

public enum CharacterSize
{
	Normal,
	Large,
	Boss
}

[System.Serializable]
public struct InteractRewardData
{
	public GameObject prefab;
	public float chanceToSpawn;
	public int maxNumberToSpawn;
};

public abstract class Character : MonoBehaviour, WorldObject 
{
	[HideInInspector] public CharacterType type { get; protected set; }

	//these are used in the pooling system
	[HideInInspector] public GameObject sourcePrefab;
	[HideInInspector] public GameObject sourcePrefabArt;

	//basic positioning info
	[HideInInspector] public Vector3 position;
	[HideInInspector] public Quaternion hitDirection;
	[HideInInspector] public Vector2 tilePos;
	public float movementSpeed { get; protected set; }
	protected float rotationSpeed = 360.0f;

	protected float rotation;
	protected float desiredRotation;

	public float baseHealth { get; protected set; }
	public float currentHealth { get; protected set; }

	protected float knockBackDistance = 1.0f;

	//path following info
	public FollowPath followPath { get; private set; }

	//combat info
	[HideInInspector] public Character targetCharacter;
	[HideInInspector] public bool alive = true;

	//current status effects
	protected StatusEffect[] statusEffectPool;
	protected int statusEffectPoolSize = 4;
	protected int statusEffectPoolUsed = 0;
	protected float[] statusEffectLastHitTimers;
	protected WeaponStatusEffectType[] statusEffectTypes;

	//these are the above stats after being modified by status effects
	[HideInInspector] public float currentMovementSpeed;
	[HideInInspector] public float currentRotationSpeed;

	//particle fx.
	[HideInInspector] public GameObject characterArt;
	[HideInInspector] public CharacterHitFlashHelper hitFlash;
	[HideInInspector] public CharacterArtHooks artHooks;

	//tappable rewards
	public List<InteractRewardData> tapRewardOnKill;

	public Color debugColor = Color.white;
	public Animator[] animators { get; protected set; }
	public Collider cachedCollider { get; protected set; } //property .collider still exists but is deprecated.
	public Weapon weapon { get; private set; }
	public Transform artTransform { get; protected set; }

	[HideInInspector] public EnemyAttributes attributes;
	[HideInInspector] public WeaponCategory categoryImmunities;
	[HideInInspector] public WeaponStatusEffectType statusEffectImmunities;
	
	//message to let characters know to switch targets and behaviours
	public abstract void OnCharacterKilled(Character character);
	public abstract bool OnDamageReceived(Weapon weapon, float amount); //true if still alive
	public abstract void OnPathComplete();
	
	//called to engage this character in h2h combat. returns true if we switched targets to deal with the combat.
	//this might be useful in the event that a character deals double damage on characters already in combat.
	public abstract bool OnStartCombat(Character character);
	public abstract void OnEndCombat(Character character);
	public abstract void OnPrepareForCombat(Character character);

	public virtual void Awake()
	{
		followPath = new FollowPath();

//		animators = GetComponentsInChildren<Animator>(true);
//		cachedCollider = GetComponent<CapsuleCollider>();
		weapon = GetComponent<Weapon>();
		position = transform.position;
		artTransform = transform;

		statusEffectPool = new StatusEffect[statusEffectPoolSize];
		statusEffectPoolUsed = 0;

		statusEffectTypes = (WeaponStatusEffectType[])System.Enum.GetValues(typeof(WeaponStatusEffectType));
		statusEffectLastHitTimers = new float[statusEffectTypes.Length];

		hitFlash = GetComponent<CharacterHitFlashHelper>();
	}
		
	public virtual bool UpdateTick()
	{
		return true; //always alive
	}

	public virtual void UpdateTicksComplete()
	{
	}

	public void SetHealth(float health)
	{
		currentHealth = health;
	}

	public virtual void OnPause(bool pause)
	{
		//do nothing. pause will stop World calling UpdateTick()
	}

	public virtual void OnTimeScaleAdjusted(float timeScale)
	{
		//as above
	}

	public virtual void OnWorldReset()
	{
	}
	
	public void UpdateTransform()
	{
		//NB: the look at position is Sin,0,Cos instead of Cos,0,Sin because
		//the art faces down the z-axis rather than the x-axis
		transform.position = position;
		transform.LookAt(position + new Vector3(Mathf.Sin(rotation), 0.0f, Mathf.Cos(rotation)));
	}
	
	public bool Seek(Vector3 target)
	{
		Vector3 direction = target - position;
		float distance = Vector3.Magnitude(direction);

		float movement = World.frameTime * currentMovementSpeed;
		if (distance <= movement)
		{
			position = target;
			return true;
		}

		position += movement * direction/distance;
		return false;
	}

	public bool FollowPath()
	{
		if (followPath.IsFollowingPath())
		{
			bool result = followPath.Update(currentMovementSpeed, World.frameTime);

			//update internal tracking of position and rotation (independant of .transform)
			position = followPath.position;
			desiredRotation = MathUtil.GetAngleInRadiansToPositionXZ(position, followPath.lookAt);
			rotation = MathUtil.UpdateRotationAngle(rotation, desiredRotation, rotationSpeed * Mathf.Deg2Rad * World.frameTime);

			//update the unity transform
			UpdateTransform();

			if (result)
				OnPathComplete();
			
			return result;
		}
			
		OnPathComplete();
		return true;
	}

#region DAMAGE

	public Quaternion GetHitDirection(Vector3 sourcePos)
	{
		sourcePos.y = position.y;
		return hitDirection = Quaternion.LookRotation((position - sourcePos).normalized, Vector3.up);
	}

	public void PlayHitEffects(Weapon weapon)
	{
		//dont play hit effects on null weapons (like status effect burns)
		if (weapon != null && weapon.type != WeaponType.Passive)
		{
			artHooks.pfx.Play(PFX.Enemy_OnHit, transform.position, true);

			if (hitFlash != null)
				hitFlash.OnHit(weapon);		
		}
	}
		
	//all these damage functions return true if the target is still alive

	bool ApplyDamage(Weapon weapon, float amount)
	{
		if (OnDamageReceived(weapon, amount))
		{	
			HandleStatusEffects(weapon); 
			return true;
		}

		return false;
	}

	public bool ApplyDirectDamage(Weapon weapon)
	{
		return ApplyDamage(weapon, weapon.weaponData.damage);
	}

	public bool ApplyDamage(float amount)
	{
		return ApplyDamage(null, amount);
	}

	public bool ApplySplashDamage(Weapon weapon)
	{
		if ((weapon.weaponData.targetingFlags & attributes) != attributes)
			return true; //no damage. still alive.
		
		return ApplyDamage(weapon, weapon.weaponData.projectileSplashDamage);
	}

	protected void HandleStatusEffects(Weapon weapon)
	{
		if (weapon != null && alive)
		{
			for (int i = 0; i < weapon.weaponData.statusEffectCount; ++i)
			{
				var data = weapon.weaponData.statusEffects[i];
				AddStatusEffect(data.type, data.value, data.duration);
			}
		}
	}

#endregion

#region STATUS EFFECTS

	public bool AddStatusEffect(WeaponStatusEffectType type, float modifier, float duration)
	{
		//ignore effects that we are immune to, or that arent status effects
		if ((type & statusEffectImmunities) != 0)
			return false;
		
		//new effect, see if we can add it
		if (statusEffectPoolUsed >= statusEffectPoolSize)
			return false;

		//checking time-outs so that enemies cant be stun locked, etc.
		for (int i = 0; i < statusEffectTypes.Length; ++i)
		{
			if (statusEffectTypes[i] == type)
			{
				if (statusEffectLastHitTimers[i] > 0.0f)
					return false;

				statusEffectLastHitTimers[i] = 0.5f;
				break;
			}
		}

//		//single timer for all effects
//		if (statusEffectLastHitTimers[0] > 0.0f)
//			return false;
//
//		statusEffectLastHitTimers[0] = 0.5f;
		
		//look for an existing effect of the same type
		for (int i = 0; i < statusEffectPoolUsed; ++i)
		{
			if (statusEffectPool[i].type == type)
			{
				statusEffectPool[i].Refresh(modifier, duration);
				return true;
			}
		}
			
		statusEffectPool[statusEffectPoolUsed].Initialise(type, modifier, duration, gameObject);
		statusEffectPoolUsed++;

		return true;
	}

	public void UpdateStatusEffects()
	{
		//reset all parameters that are effected by different status effects
		currentMovementSpeed = movementSpeed;
		animators [1].speed = currentMovementSpeed * World.instance.timeScale; //TODO: this doesnt look correct.
		currentRotationSpeed = rotationSpeed;
		
		for (int i = 0; i < statusEffectPoolUsed; ++i)
		{
			if (!statusEffectPool[i].Process(this, World.frameTime))
			{
				//remove the effect from the pool, dont worry about preserving order.
				statusEffectPool[i] = statusEffectPool[statusEffectPoolUsed - 1];
				statusEffectPool[statusEffectPoolUsed - 1].Clear();
				statusEffectPoolUsed -= 1;
				--i;
			}
		}

		for (int i = 0; i < statusEffectLastHitTimers.Length; ++i)
			statusEffectLastHitTimers[i] -= World.frameTime;
	}

	public void ResetStatusEffects()
	{
		for (int i = 0; i < statusEffectPool.Length; ++i)
			statusEffectPool[i].Reset();

		for (int i = 0; i < statusEffectLastHitTimers.Length; ++i)
			statusEffectLastHitTimers[i] = 0.0f;

		statusEffectPoolUsed = 0;
	}

	public void CopyStatusEffects(Character copyFrom)
	{
		ResetStatusEffects();

		for (var i = 0; i < copyFrom.statusEffectPoolUsed; ++i)
		{
			//dont copy over effects if we are immune to them
			if ((statusEffectImmunities & copyFrom.statusEffectPool[i].type) != 0)
				continue;

			statusEffectPool[statusEffectPoolUsed].Initialise(copyFrom.statusEffectPool[i], gameObject);
			statusEffectPoolUsed += 1;
		}
	}

	public bool HasStatusEffect(WeaponStatusEffectType type)
	{
		for (var i = 0; i < statusEffectPoolUsed; ++i)
			if (statusEffectPool[i].type == type)
				return true;

		return false;
	}

#endregion

#region TAP REWARDS

	public void SpawnTapRewards()
	{
		if (tapRewardOnKill == null || FTUE.IsActive())
			return;

		//TODO: instead of rolling against each object on the list, maybe we should
		//add up all the weights and check against that. i imagine most objects will
		//only have a single tap reward type though so maybe it doesnt matter.
		foreach (var reward in tapRewardOnKill)
		{
			if (Random.value > reward.chanceToSpawn)
				continue;

			int numberToSpawn = Random.Range(1, reward.maxNumberToSpawn);
			for (int i = 0; i < numberToSpawn; ++i)
				World.AddInteractReward(reward.prefab, transform.position);

			//only spawn a single group of things from the list. 
			return;
		}
	}

#endregion
}
