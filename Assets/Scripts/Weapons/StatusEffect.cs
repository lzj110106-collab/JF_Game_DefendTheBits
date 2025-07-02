using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public struct StatusEffectData
{
	public StatusEffectData(WeaponStatusEffectType type, float duration, float value)
	{
		this.type = type;
		this.duration = duration;
		this.value = value;
	}

	public WeaponStatusEffectType type;
	public float duration;
	public float value;


	public static string Name(WeaponStatusEffectType type) 			{ return LocManager.Translate("effect_name_" + type.ToString().ToLower()); }
	public static string Description(WeaponStatusEffectType type) 	{ return LocManager.Translate("effect_desc_" + type.ToString().ToLower()); }
};

public struct StatusEffect
{
	GameObject target;

	public WeaponStatusEffectType type;
	public float modifier;

	public float duration;
	float elapsed;

	GameObject pfxInstance;
	GameObject pfxSource;

	//TODO: pool these somewhere in the final optimisation passes before release
	GameObject meshInstance;
	MeshRenderer meshRenderer;

	AudioObject fireEffectAudio;
	AudioObject freezeEffectAudio;

	public void Initialise(WeaponStatusEffectType type, float modifier, float duration, GameObject target)
	{
		this.target = target;
		this.type = type;
		this.modifier = modifier;
		this.duration = duration;

		elapsed = 0.0f;

		var effectData = StatusEffectDatabase.Get(type);
		if (effectData != null)
		{
			if (effectData.pfxPrefab != null)
			{
				pfxInstance = PFXPool.Get(effectData.pfxPrefab);
				if (pfxInstance != null)
				{
					pfxInstance.SetActive(true);
					pfxSource = effectData.pfxPrefab;
				}
			}

			//TODO: pooling
			if (effectData.meshPrefab)
			{
				meshInstance = GameObject.Instantiate(effectData.meshPrefab);
				meshRenderer = meshInstance.GetComponent<MeshRenderer>();
				meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
			}

			AttachPFX();
		}
	}

	public void Initialise(StatusEffect copyFrom, GameObject target)
	{
		Reset();
		Initialise(copyFrom.type, copyFrom.modifier, copyFrom.duration, target);
		elapsed = copyFrom.elapsed;
	}

	public void Reset()
	{
		if (pfxInstance)
		{
			PFXPool.Return(pfxSource, pfxInstance);
			pfxInstance = null;
			pfxSource = null;
		}

		if (meshInstance)
		{
			GameObject.Destroy(meshInstance);
			meshInstance = null;
			meshRenderer = null;
		}
		if (fireEffectAudio != null && type ==  WeaponStatusEffectType.Burn)
			fireEffectAudio.Stop(0.3f);

		if (freezeEffectAudio != null && type == WeaponStatusEffectType.Freeze) {
			freezeEffectAudio.Stop(0.3f);
			AudioController.Play ("Tower_Polar_Freeze_End");
		}
	}

	//since structs are copy on assignement, we need to clear the references to PFX and meshes
	//when the status effect pool in the character gets modified, otherwise when this
	//instance is used by the next effect, it will still have the references.
	public void Clear()
	{
		meshInstance = null;
		meshRenderer = null;
		pfxInstance = null;
		pfxSource = null;
	}

	public void Refresh(float modifier, float duration)
	{
		//dont refresh timers with status effects that lock the enemy in place.
		if (type == WeaponStatusEffectType.Freeze ||
			type == WeaponStatusEffectType.Stun)
		{
			return;
		}

		bool largerModiferIsBetter = true;
		if (type == WeaponStatusEffectType.Slow)
			largerModiferIsBetter = false;

		if ((largerModiferIsBetter && modifier >= this.modifier) ||
		    (!largerModiferIsBetter && modifier <= this.modifier))
		{
			//the passed in effect refresh data is more powerful. 
			this.modifier = modifier;
			this.elapsed = 0;
			this.duration = duration;
		}
		else
		{
			//just refresh the timer
			this.elapsed = 0;
			this.duration = Mathf.Max (this.duration, duration);
		}
	}

	public bool Process(Character target, float dt)
	{
		elapsed += dt;
		if (elapsed >= duration)
		{
			Reset(); //clear PFX
			return ChainStatusEffect(target); //some effects morph into others
		}

		switch (type)
		{
		case WeaponStatusEffectType.Slow:	
			target.currentMovementSpeed = target.movementSpeed * modifier;
			target.animators [1].speed = target.currentMovementSpeed *  World.instance.timeScale;
			break;

		case WeaponStatusEffectType.Stun:
			target.currentMovementSpeed = 0;
			target.animators [1].speed = target.currentMovementSpeed;
			break;

		case WeaponStatusEffectType.Freeze:
			target.currentMovementSpeed = 0;
			target.animators [1].speed = target.currentMovementSpeed;

			if (meshRenderer != null)
				meshRenderer.sharedMaterial.SetFloat("_Cutoff", elapsed/duration);

			if (freezeEffectAudio == null) {
				freezeEffectAudio = AudioController.Play ("Tower_Polar_Freeze_Lp");
			}
			else if (!freezeEffectAudio.IsPlaying())
				freezeEffectAudio.Play();
			
			break;

		case WeaponStatusEffectType.Burn:
			if (fireEffectAudio == null)
				fireEffectAudio = AudioController.Play ("Tower_Flame_OnFire");
			else if (!fireEffectAudio.IsPlaying())
				fireEffectAudio.Play();

			ApplyDamageOverTime(target, dt);
			break;

		case WeaponStatusEffectType.Poison:
			ApplyDamageOverTime(target, dt);
			break;
			
		default:
			break;
		}

//		HUD.SetStatusIconPosition(hudSpriteInstance, target.transform.position);
		return true;
	}

	void ApplyDamageOverTime(Character target, float dt)
	{
		if (!target.ApplyDamage (modifier * dt))
		{
			var enemy = (EnemyCharacter)target;
			AchievementDatabase.AddKill(enemy.enemyDataInitial.identifier, "");
		}
	}
	
	public bool Process(Tower target, float dt)
	{
		elapsed += dt;
		if (elapsed >= duration)
		{
			Reset();
			return false;
		}

//		HUD.SetStatusIconPosition(hudSpriteInstance, target.transform.position);
		return true;
	}

	public void AttachPFX()
	{
		if (pfxInstance != null)
			pfxInstance.transform.SetParent(target.transform, false);

		if (meshInstance != null)
			meshInstance.transform.SetParent(target.transform, false);
		
		//some effects are parented to particular nodes in the target character.
		var effectData = StatusEffectDatabase.Get(type);
		if (effectData.attachToLocator != "")
		{
			var locator = UnityUtil.FindChild(target.transform, effectData.attachToLocator);
			if (locator != null)
			{
				if (pfxInstance != null)
					pfxInstance.transform.SetParent(locator.transform, false);

				if (meshInstance != null)
					meshInstance.transform.SetParent(locator.transform, false);
				
				return;
			}
		}
	}

	public void DetachPFX()
	{
		if (pfxInstance != null)
			pfxInstance.transform.SetParent(null, false);

		if (meshInstance != null)
			meshInstance.transform.SetParent(null, false);
	}

	bool ChainStatusEffect(Character target)
	{
		//freeze itself doesnt use the modifier parameter. if it is
		//non-zero then it indicates that freeze triggers a
		//slowing effect when it ends instead
		if (type == WeaponStatusEffectType.Freeze && modifier > 0.0f)
		{
			Initialise(WeaponStatusEffectType.Slow,
					   modifier,
					   duration * 0.5f,
					   target.gameObject);
			
			return true;
		}
			
		return false;
	}
}