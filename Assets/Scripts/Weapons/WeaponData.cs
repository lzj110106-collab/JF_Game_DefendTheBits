using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponData
{
	public EnemyAttributes targetingFlags;
	public int maxTargets;

	public float damage;
	public float attacksPerSecond;
	public float rotationSpeed;

	public float projectileSplashDamage;
	public float projectileSplashRadius;

	public StatusEffectData[] statusEffects = new StatusEffectData[4];
	public int statusEffectCount = 0;

	public WeaponData()
	{
		Clear();
	}

	public void Merge(WeaponData other, bool addStatusEffects = true)
	{
		targetingFlags |= other.targetingFlags;
		maxTargets += other.maxTargets;

		damage += other.damage;
		attacksPerSecond += other.attacksPerSecond;
		rotationSpeed += other.rotationSpeed;

		projectileSplashDamage += other.projectileSplashDamage;
		projectileSplashRadius += other.projectileSplashRadius;

		for (var i = 0; i < other.statusEffectCount; ++i)
		{
			bool found = false;

			for (var j = 0; j < statusEffectCount; ++j)
			{
				if (addStatusEffects)
				{
					statusEffects[j].value += other.statusEffects[i].value;
				}
				else
				{
					if (statusEffects[j].type == WeaponStatusEffectType.Freeze ||
						statusEffects[j].type == WeaponStatusEffectType.Slow ||
						statusEffects[j].type == WeaponStatusEffectType.Stun)
					{
						statusEffects[j].value *= 1.0f - other.statusEffects[i].value;
					}
					else
					{
						statusEffects[j].value *= other.statusEffects[i].value;
					}
				}

				statusEffects[j].duration += other.statusEffects[i].duration;
				found = true;
			}

			if (!found && statusEffectCount < statusEffects.Length)
			{
				statusEffects[statusEffectCount].type = other.statusEffects[i].type;
				statusEffects[statusEffectCount].value = other.statusEffects[i].value;
				statusEffects[statusEffectCount].duration = other.statusEffects[i].duration;

				statusEffectCount += 1;
			}
		}
	}

	public void Clear()
	{
		targetingFlags = 0;
		maxTargets = 0;

		damage = 0;
		attacksPerSecond = 0;
		rotationSpeed = 0;

		projectileSplashDamage = 0;
		projectileSplashRadius = 0;

		statusEffectCount = 0;
	}
}
