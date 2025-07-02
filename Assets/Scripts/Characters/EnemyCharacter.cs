using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Flags]
public enum EnemyAttributes
{
	Default = 0,
	Flying = 1,
	Stealth = 2,
	Frozen = 4,
	Berserk = 8,
	Regen = 16
}

[System.Serializable]
public class EnemyLevelData
{
	public GameObject artPrefab;
	public int health;
	public float scale = 1.0f;
}

public class EnemyCharacter : Character
{
	private enum State
	{
		Combat,
		FollowPath,
		WaitForCombat
	};

	public string enemyName;
	private State state;

	public float minTimeBetweenHitReactions = 0.0f;
	float timeSinceLastHitReaction = 0.0f;

	public EnemyWave parentWave { get; private set; }

	bool deathAnimationComplete;
	public bool alwaysPlayDeathPFX = false;

	int[] updateTicksSinceLastAnimationCycle;

	//keep a reference the spawn data of this enemy so that appropriate rewards can be given
	public MasterEnemyTable.Entry enemyData { get; private set; }
	public MasterEnemyTable.Entry enemyDataInitial { get; private set; }

	//this is set by the wave that spawned the enemy.
	public float groupSpeedMultiplier { get; private set; }

	//need to cache this multiplier so that it can be applied to all
	//the enemies that this enemy turns into as it takes damage
	public float additionalHealthMultiplier { get; private set; }

	public AnimationCurve berserkerSpeedCurve;
	float regenTimer = 0.0f;

	public override void Awake()
	{
		base.Awake();
		type = CharacterType.Enemy;
        //print("hi");
	}

	public void InitialiseState(MasterEnemyTable.Entry sourceData, EnemyWave wave, bool playSpawnAnimation)
	{
		//set the object as active. its turned off while pooled.
		gameObject.SetActive(true);
		alive = true;
		deathAnimationComplete = false;

		enemyData = sourceData;
		enemyDataInitial = sourceData;
		parentWave = wave;

		InitialiseArt();
		InitialiseAnimation(playSpawnAnimation);
		ResetStatusEffects();

		currentHealth = sourceData.health;
		additionalHealthMultiplier = 1.0f;

		movementSpeed = MasterEnemyTable.BaseMovementSpeed() * sourceData.speedMultiplier;
		currentMovementSpeed = movementSpeed;

		OnTimeScaleAdjusted(World.instance.timeScale);

		rotationSpeed = 360.0f;
		knockBackDistance = enemyData.knockback;
		timeSinceLastHitReaction = minTimeBetweenHitReactions; //so we can react to immediate hits
		regenTimer = 0.0f;

		attributes = enemyData.abilities;
		statusEffectImmunities = enemyData.statusEffectImmunities;
		categoryImmunities = enemyData.categoryImmunities;

		//need to make sure all attached component helpers are reset as well. 
		if (hitFlash != null)
			hitFlash.Stop();

		//HUD.instance.AddHealthBar(this);

		state = State.FollowPath;
	}

	public void InitialisePath(int startNodeIndex, float pathOffset)
	{
		//TODO: set up character size properly
		followPath.Initialise(startNodeIndex, 
			CharacterSize.Normal, 
			((enemyData.abilities & EnemyAttributes.Flying) != 0),
			pathOffset);

		//snap the position and facing to the path
		position = followPath.position;
		rotation = MathUtil.GetAngleInRadiansToPositionXZ(position, followPath.lookAt);
		desiredRotation = rotation;

		UpdateTransform();
	}

	public void InitialisePath(EnemyCharacter sourceCharacter)
	{
		followPath.Initialise(sourceCharacter.followPath, enemyData.maxPathOffset);
	}

	public void SetGroupSpeedMultiplier(float multiplier)
	{
		groupSpeedMultiplier = multiplier;

		movementSpeed *= groupSpeedMultiplier;
		currentMovementSpeed = movementSpeed;
	}

	public void SetAdditionalHealthMultiplier(float multiplier)
	{
		additionalHealthMultiplier = multiplier;
		currentHealth *= multiplier;
	}

	public void SetAdditionalHealthMultiplier(int endlessDifficulty, int endlessDifficultyWaveCount)
	{
		//default to no multiplier incase we havent hit endless mode yet
		additionalHealthMultiplier = 1.0f;

		if (endlessDifficulty >= 0)
		{
			//set up the base health multiplier for this difficulty level
			if (endlessDifficulty < enemyDataInitial.endlessModeHealthBaseMultiplier.Length)
				additionalHealthMultiplier = enemyDataInitial.endlessModeHealthBaseMultiplier[endlessDifficulty];

			//add in any additional health per wave (this is currently used to ensure that the
			//game continues to get progressively harder once we have run out of difficulty levels)
			if (endlessDifficulty < enemyDataInitial.endlessModeHealthPerWave.Length)
				additionalHealthMultiplier += enemyDataInitial.endlessModeHealthPerWave[endlessDifficulty] * endlessDifficultyWaveCount;
		}

		currentHealth *= additionalHealthMultiplier;

//		Debug.Log(endlessDifficulty + " " + endlessDifficultyWaveCount + " -> " + additionalHealthMultiplier);
	}

	public override bool UpdateTick()
	{
		timeSinceLastHitReaction += World.frameTime;

		for (var i = 0; i < updateTicksSinceLastAnimationCycle.Length; ++i)
			updateTicksSinceLastAnimationCycle[i] += 1;

		//TODO: move this to the end of the frame as well
		if (hitFlash != null)
			hitFlash.UpdateTick();

		if ((enemyData.abilities & EnemyAttributes.Regen) != 0)
		{
			regenTimer += World.frameTime;
			if (regenTimer >= enemyData.regenFrequency)
			{
				regenTimer -= enemyData.regenFrequency;
				currentHealth += enemyData.regenHP;

				//dont overflow the original health value
				currentHealth = Mathf.Min(currentHealth, enemyData.health * additionalHealthMultiplier);
			}
		}
		
		if (alive)
		{
			UpdateStatusEffects();

			switch (state)
			{
			case State.Combat:		UpdateCombat();			break;
			case State.FollowPath:	UpdateFollowPath();		break;
			}

			UpdateTransform();
		}
		else if (deathAnimationComplete)
		{
			EnemyPool.Return(sourcePrefab, gameObject);
			EnemyPool.Return(sourcePrefabArt, characterArt);

			sourcePrefab = null;
			sourcePrefabArt = null;

			return false;
		}

		World.instance.targetFinder.Add(this);

		return true;
	}

	public override void UpdateTicksComplete()
	{
		UpdateAnimations();
	}

	public override bool OnDamageReceived (Weapon weapon, float amount)
	{
		if (alive)
		{
			//ignore weapons we are immune to
			if (weapon && (enemyData.categoryImmunities & weapon.category) != 0)
				return true;

			//weapons that can target frozen enemies do max damage. otherwise all damage is halved.
			if (HasStatusEffect(WeaponStatusEffectType.Freeze))
			{
				if (weapon == null || (weapon.weaponData.targetingFlags & EnemyAttributes.Frozen) == 0)
					amount *= 0.5f;
			}

            //if this was a fatal blow, we need to figure out what
            //enemy type to turn this enemy into (if anything)
            
			if (currentHealth <= amount)
			{
				//generate child enemies using the original enemy type
				//生成小的方块
                //parentWave.SpawnChildren(this, enemyData);

				float remaining = amount;
				var newEnemyData = enemyData;

				while (remaining >= currentHealth)
				{
					remaining -= currentHealth;
					newEnemyData = newEnemyData.onDeath;

					if (newEnemyData == null)
					{
						OnKillingBlow(weapon);
						return alive;
					}
					else
					{
						//make sure to scale the health of the transitional enemy types
						currentHealth = newEnemyData.health * additionalHealthMultiplier;
					}
				}
					
				//switch art assets and stuff
				currentHealth -= remaining;
				enemyData = newEnemyData;

				movementSpeed = MasterEnemyTable.BaseMovementSpeed() * enemyData.speedMultiplier;
				currentMovementSpeed = movementSpeed;

				attributes = enemyData.abilities;
				statusEffectImmunities = enemyData.statusEffectImmunities;
				categoryImmunities = enemyData.categoryImmunities;

				InitialiseArt();

				if (alwaysPlayDeathPFX)
					artHooks.pfx.Play(PFX.Enemy_OnDeath, transform.position, true);
			}
			else
			{
				currentHealth -= amount;

				if ((enemyData.abilities & EnemyAttributes.Berserk) != 0)
				{
					//beserk enemies ramp their speed up based on how much damage they have taken
					float t = 1.0f - currentHealth/(enemyData.health * additionalHealthMultiplier);
					float u = berserkerSpeedCurve.Evaluate(t);
					float beserk = Mathf.Lerp(1.0f, enemyData.maxBerserkSpeed, Mathf.Clamp01(u));

					float multiplier = groupSpeedMultiplier * enemyData.speedMultiplier;
					movementSpeed = MasterEnemyTable.BaseMovementSpeed() * multiplier * beserk;
				}
			}

			if (timeSinceLastHitReaction >= minTimeBetweenHitReactions)
			{
				animators[0].Play("Hit", 0, 0f);
				updateTicksSinceLastAnimationCycle[0] = 0;

				PlayHitEffects(weapon);
				timeSinceLastHitReaction = 0.0f;
			}
		}

		return alive;
	}

    public void KilledRightNow()
    {
        OnKillingBlow(null);
    }

	public override void OnCharacterKilled(Character character)
	{
		//if the character that died is the one we are attacking, resume path following
		if (character == targetCharacter)
			SetState (State.FollowPath);
	}

	public void OnKillingBlow(Weapon weapon)
	{
		int reward = enemyDataInitial.rewardCoins;

		//ensure the character no longer updates movement and things
		//dont check for "alive = true" here because ApplyDamage does
		//that for us. 
		alive = false;

		AudioController.Play ("Enemy_Splat");
	
		AudioController.Play ("Enemy_Death");
			                
		//inform the wave that spawned us, so it can spawn child enemies in our place
		if (parentWave != null)
			parentWave.OnCharacterKilled(this);
		
		if (weapon != null)
		{
			//Apply directional knockback
//			GetHitDirection(weapon.parentTower.transform.position);
			hitDirection = Quaternion.identity;

			//if the weapon belonged to a tower, check the global upgrades
			//for this tower and figure out if any additional reward
			//should be metered out.
			if (weapon.parentTower)
				reward += weapon.parentTower.bonusGoldRewardForEnemyKill;
		}
		
//		transform.position += hitDirection * Vector3.one * knockBackDistance;
		artHooks.pfx.Play(PFX.Enemy_OnDeath, transform.position, true);
		artHooks.pfx.Play(PFX.Enemy_OnHit, transform.position, true);

        //we died, let the world know
#if UNITY_EDITOR  
        HUD.instance.AddGold(reward*30); //编译器下金币获取数量变多
#elif UNITY_ANDROID
        HUD.instance.AddGold(reward);
#endif
        HUD.instance.RemoveHealthBar(this);
		World.instance.OnCharacterKilled(this, true);

		//spawn bonus coin rewards into the world or whatever else is assigned to the character.
		//we only want to perform this on death, not during OnCharacterKilled which is called at the end of paths
		SpawnTapRewards();

		//make sure to clear the stun effects. stun prevents the animator for the death sequence updating. 
		ResetStatusEffects();
		TriggerDeathAnimation();
	}

	void TriggerDeathAnimation()
	{
		if (animators.Length > 1)
		{
			//animation contains a trigger to destroy the object. see EnemyCharacterAnimationResponder.cs
			animators[1].SetTrigger("Death");
			animators[1].speed = 1.0f;
			updateTicksSinceLastAnimationCycle[1] = 0;
		}
		else
		{
			//no animation. immediate destruction
			Debug.Log ("[warning] enemy is missing death animator: " + name);
			deathAnimationComplete = true;
		}
	}

	public void OnDeathAnimationComplete()
	{
		deathAnimationComplete = true;
	}

	public override void OnPathComplete()
	{
		//ensure the character no longer updates movement and things
		//dont check for "alive = true" here because ApplyDamage does
		//that for us. 
		alive = false;

		//we died, let the world know
		HUD.instance.RemoveHealthBar(this);
		World.instance.OnCharacterKilled(this, false);
		parentWave.OnCharacterKilled(this);

		TriggerDeathAnimation();
	}

	public override bool OnStartCombat(Character character)
	{
		if (state == State.WaitForCombat)
		{
			targetCharacter = character;
			SetState (State.Combat);
			return true;
		}

		return false;
	}

	public override void OnEndCombat(Character character)
	{
		//if we are in combat with a different character, stay that way.
		if (character == targetCharacter)
		{
			//TODO: follow path resumption should depend on weapon type
			SetState(State.FollowPath);
		}
	}

	public override void OnPrepareForCombat(Character character)
	{
		//interrupt path finding
		if (state == State.FollowPath)
		{
			targetCharacter = character;
			SetState (State.WaitForCombat);
		}
	}

	void UpdateCombat()
	{
		//end combat if we lose our target or the target goes out of range
		/*if (targetCharacter == null || Vector3.Magnitude(targetCharacter.position - position) > weapon.range)
			SetState (State.FollowPath);*/
	}

	void UpdateFollowPath()
	{
		if (FollowPath())
		{
			//for now, deal damage to the player and remove from play
			//TODO: animations/effects/exp to player/towers maybe, etc
			HUD.instance.DealDamageToPlayer(enemyData.damage);
		}

		//NB: disabled for now
//		ResolveCollisions(World.instance.FindNeighbours(this, 2.0f, 4));

		//ranged characters should test if they can stand and fire at a player
		/*if (weapon.type != WeaponType.CloseCombat)
		{
			var targets = World.instance.FindCharactersInArea(position, weapon.range, CharacterType.Player);
			if (targets.Count > 0)
			{
				targetCharacter = targets[0];
				SetState (State.Combat);
			}
		}*/

		//TODO: probably want to test for special attack cooldowns and perform those if possible.
		//eg: summon enemies, attack towers, special movement, etc.
	}

	void SetState(State newState)
	{
		//state initialisation
//		switch (newState)
//		{
//		case State.Combat:
//			weapon.target = targetCharacter;
//			break;
//
//		case State.FollowPath:
//			weapon.target = null;
//			targetCharacter = null;
//			break;
//
//		default:
//			break;
//		}

		state = newState;
	}

#region DIFFICULTY SWAPPING

	void InitialiseArt()
	{
		for (int i = 0; i < statusEffectPoolUsed; ++i)
			statusEffectPool[i].DetachPFX();

		//return the current art to the object pool
		if (sourcePrefabArt != null)
			EnemyPool.Return(sourcePrefabArt, characterArt);

		//generate the new art and link up all the responders and stuff
		{
			sourcePrefabArt = enemyData.prefabArt;

			characterArt = EnemyPool.Get(sourcePrefabArt);
			characterArt.transform.SetParent(transform.GetChild(0), false);
			characterArt.SetActive(true);
			characterArt.GetComponent<EnemyCharacterAnimationResponder>().parent = (EnemyCharacter) this;

			animators = GetComponentsInChildren<Animator>(true);
			cachedCollider = GetComponent<CapsuleCollider>();

			//grab a transform that moves with the animation. used for projectile tracking.
			var locator = UnityUtil.FindChild(characterArt.transform, "Body");
			artTransform = locator == null ? characterArt.transform : locator.transform;

			artHooks = characterArt.GetComponent<CharacterArtHooks>();
			if (artHooks)
				artHooks.pfx.Initialise(characterArt.transform);
		}
			
		//and get the flash code to point at the new art
		//the flash helper does some modification to MeshRenderers. so we need to track changes there as well
		if (hitFlash != null)
		{
			hitFlash.CacheRenderers();
			hitFlash.RestoreRenderers();
		}

		//characters also scale by level 
		transform.localScale = Vector3.one * enemyData.artScale;

		//reattach PFX to the new art locators
		for (int i = 0; i < statusEffectPoolUsed; ++i)
			statusEffectPool[i].AttachPFX();
	}

	void InitialiseAnimation(bool playSpawnAnimation)
	{
		updateTicksSinceLastAnimationCycle = new int[animators.Length];

		//manual update of animators
		for (int i = 0; i < animators.Length; ++i)
		{
			animators[i].enabled = false;
			updateTicksSinceLastAnimationCycle[i] = 0;
		}

		//randomise playback time to reduce the chance of enemies being in sync with each other
		animators[1].Play("Run", 0, Random.Range(0, 1f)); 
		animators[1].speed = Random.Range(0.95f, 1.05f);

		if (playSpawnAnimation)
			animators[0].Play("Spawn_FromGround", 0, 0.0f); //need to reset this otherwise pooling gets stuck
	}

	void UpdateAnimations()
	{
		for (var i = 0; i < animators.Length; ++i)
		{
			animators[i].Update(World.frameTime * updateTicksSinceLastAnimationCycle[i]);
			updateTicksSinceLastAnimationCycle[i] = 0;
		}
	}

	void UpdateAnimationsOld()
	{
		for (var i = 0; i < animators.Length; ++i)
			animators[i].Update(World.frameTime);
	}

#endregion

	public override void OnWorldReset()
	{
		if (characterArt != null)
		{
			EnemyPool.Return(sourcePrefabArt, characterArt);
			characterArt = null;
		}

		//this is to ensure the status effect meshes are cleaned up correctly.
		ResetStatusEffects();
	}
}