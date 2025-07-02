using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PigeonCoopToolkit.Effects.Trails;

public abstract class Projectile: MonoBehaviour, WorldObject
{
	//cache effects. projectiles dont add particle effects to themselves.
	TrailRenderer_Base[] trailsPigeon;
	TrailRenderer[] trailsUnity;
	ParticleSystem[] particleSystems;

	public virtual void Initialise(Weapon parent, Vector3 from, Vector3 to, Character target)
	{
	}

	protected void CacheEffects()
	{
		if (trailsUnity == null)
		{
			trailsPigeon = GetComponentsInChildren<TrailRenderer_Base>(true);
			trailsUnity = GetComponentsInChildren<TrailRenderer>(true);
			particleSystems = GetComponentsInChildren<ParticleSystem>(true);
		}
	}
		
	protected void ResetEffects()
	{
		if (trailsUnity != null)
		{
			for (int i = 0; i < trailsPigeon.Length; ++i)
				trailsPigeon[i].ClearSystem(true);

			for (int i = 0; i < trailsUnity.Length; ++i)
				trailsUnity[i].Clear();

			for (int i = 0; i < particleSystems.Length; ++i)
				particleSystems[i].Clear();
		}
	}

#region WorldObject

	public virtual bool UpdateTick() { return true; }
	public virtual void UpdateTicksComplete() {}

	public abstract void OnPause(bool pause);
	public abstract void OnTimeScaleAdjusted(float timeScale);
	public abstract void OnCharacterKilled(Character c);
	public abstract void OnWorldReset();

#endregion
}
