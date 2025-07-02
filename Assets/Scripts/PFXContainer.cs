using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: would be really nice if i could parameterise PFX container classes based on enum but i cant.
//TODO: instead, everything is in one giant enum until i can think of a better way of doing it...

public enum PFX
{
	Enemy_OnHit,
	Enemy_OnDeath,

	Weapon_OnFire,
	Weapon_OnHitEnemy,
	Weapon_OnHitGround,

	Tower_OnSpawn,
	Tower_OnDestroy,
	Tower_SpawnObject,
	Tower_Passive,
	Tower_Passive_2,
	Tower_Passive_3,
	Tower_DuckCollectionIdle
}

[System.Serializable]
public class PFXEntry
{
	public PFX type;
	public GameObject prefab;
	public Transform locator;

	public Color startColour = Color.white;
	public bool overrideColour = false;
	public float emissionRate = 1.0f;
	public bool overrideEmissionRate = false;
	public float minSize = 1.0f;
	public float maxSize = 1.0f;
	public bool overrideSize = false;

	public string sfx;
	public string sfxSecondary;
	public string sfxRandom;
	public float sfxRandomChance;
}

[System.Serializable]
public class TowerDefensePFXContainer
{
	public List<PFXEntry> entries;
	public PFXEntry[] entriesOrdered { get; private set; } //faster lookups

	Transform parentTransform;

	public void Initialise(Transform parentTransform)
	{
		entriesOrdered = new PFXEntry[System.Enum.GetValues(typeof(PFX)).Length];

		foreach (var entry in entries)
			entriesOrdered[(int)entry.type] = entry;

		this.parentTransform = parentTransform;
	}

	public PFXWrapper Play(PFX type, bool fireAndForget, bool playSFX = true)
	{
		var instance = Get(type, playSFX);
		if (instance)
		{
			var locator = entriesOrdered[(int)type].locator;
			if (locator == null)
				locator = parentTransform;
			
			instance.Play(locator, fireAndForget);
			return instance;
		}

		return null;
	}

	public PFXWrapper Play(PFX type, Vector3 position, bool fireAndForget, bool playSFX = true)
	{
		var instance = Get(type, playSFX);
		if (instance)
		{
			instance.Play(position, fireAndForget);
			return instance;
		}

		return null;
	}

	public PFXWrapper Play(PFX type, Vector3 position, Quaternion rotation, bool fireAndForget, bool playSFX = true)
	{
		var instance = Get(type, playSFX);
		if (instance)
		{
			instance.Play(position, rotation, fireAndForget);
			return instance;
		}

		return null;
	}

	public PFXWrapper Play(PFX type, Transform transform, bool fireAndForget, bool playSFX = true)
	{
		var instance = Get(type, playSFX);
		if (instance)
		{
			instance.Play(transform, fireAndForget);
			return instance;
		}

		return null;
	}

	PFXWrapper Get(PFX type, bool playSFX)
	{
		var data = entriesOrdered[(int)type];

		if (data != null)
		{
			//if requested, always play SFX regardless of 
			//the state of the PFX pooling system.
			if (playSFX)
			{
				if (!string.IsNullOrEmpty(data.sfx))
					AudioController.Play(data.sfx);

				if (!string.IsNullOrEmpty(data.sfxSecondary))
					AudioController.Play(data.sfxSecondary);

				
				if (!string.IsNullOrEmpty(data.sfxRandom) && Random.Range (0f,1f) <= data.sfxRandomChance)
					AudioController.Play(data.sfxRandom);
			}

			//pull a PFX instance from the pool and return the wrapper object.
			if (data.prefab != null)
			{
				var pfxInstance = PFXPool.Get(data.prefab);
				if (pfxInstance == null)
					return null;
				
				var pfxWrapper = pfxInstance.GetComponent<PFXWrapper>();
				if (pfxWrapper == null)
					return null;

				PFXWrapper.ApplyPFXOverrides(pfxWrapper.ps, data);

				return pfxWrapper;
			}
		}

		return null;
	}
}

public class PFXWrapper: MonoBehaviour
{
	public ParticleSystem ps;
	public GameObject sourcePrefab;

	bool isFireAndForget = false;

	//if no rotation is specified, dont overwrite the one that was originally
	//in the prefab with the identity quaternion. 
	public void Play(Vector3 position, bool fireAndForget)
	{
		ps.gameObject.SetActive(true);

		ps.gameObject.transform.SetParent(null, false);
		ps.gameObject.transform.position = position;

		ps.Stop();
		ps.Clear();
		ps.Play();

		isFireAndForget = fireAndForget;
	}

	public void Play(Vector3 position, Quaternion rotation, bool fireAndForget)
	{
		ps.gameObject.SetActive(true);

		//attach to the world so that playback happens correctly (the PFX pool is inactive).
		//once playback is finished, it will return to the PFX pool automatically.
		ps.gameObject.transform.SetParent(null, false);
		ps.gameObject.transform.position = position;
		ps.gameObject.transform.rotation = rotation;

		ps.Stop();
		ps.Clear();
		ps.Play();

		isFireAndForget = fireAndForget;
	}
		
	public void Play(Transform transform, bool fireAndForget)
	{
		ps.gameObject.SetActive(true);
		ps.gameObject.transform.SetParent(transform, false);

		ps.Stop();
		ps.Clear();
		ps.Play();

		isFireAndForget = fireAndForget;
	}

	public void Stop()
	{
		ps.Stop();
	}

	public bool IsPlaying()
	{
		return ps.isPlaying;
	}

	public void ReturnToPool()
	{
		PFXPool.Return(sourcePrefab, ps.gameObject);
	}


	void Update()
	{
		if (isFireAndForget)
		{
			//test if the particle system has finished playback
            //性能
			if (ps == null || !ps.isPlaying || ps.time > ps.main.duration)
				ReturnToPool();
		}
	}

	public static void ApplyPFXOverrides(ParticleSystem ps, PFXEntry data)
	{
		var mainModule = ps.main;
		var emissionModule = ps.emission;

		if (data.overrideColour)
			mainModule.startColor = new ParticleSystem.MinMaxGradient(data.startColour);

		if (data.overrideSize)
			mainModule.startSize = Random.Range(data.minSize, data.maxSize);

		if (data.overrideEmissionRate)
			emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(data.emissionRate);
	}
}