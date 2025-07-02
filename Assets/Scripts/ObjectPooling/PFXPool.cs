using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFXPool : MonoBehaviour 
{
	static PFXPool instance;

	public GenericPrefabPool source;

	void Awake()
	{
		instance = this;
		source.Initialise("PFXPool");

		//add wrapper objects to all the PFX instances to aid with playback related stuff
		foreach (var kv in source.pools)
			OnPoolExpanded(kv.Value, 0, kv.Value.prefabInstances.Count);

		//make sure we do the same for PFX instances that are added when pools expand.
		source.poolExpandedCallback = OnPoolExpanded;
	}

	void OnDestroy()
	{
		instance = null;
	}

	public static GameObject Get(GameObject prefab)
	{
		return instance.source.Get(prefab);
	}

	public static GameObject Get(GameObject prefab, GameObject attachTo)
	{
		return instance.source.Get(prefab, attachTo);
	}

	public static void Return(GameObject prefab, GameObject prefabInstance)
	{
		instance.source.Return(prefab, prefabInstance);
	}

	public static void Reset()
	{
		instance.source.ResetPool();
	}

	//fire and forget helper function.
	public static void Play(GameObject prefab, Vector3 position)
	{
		if (prefab != null && instance != null)
		{
			var pfx = instance.source.Get(prefab);
			if (pfx != null)
			{
				var wrapper = pfx.GetComponent<PFXWrapper>();
				wrapper.Play(position, true);
			}
		}
	}	

	void OnPoolExpanded(GenericPrefabPool.Pool pool, int oldSize, int newSize)
	{
		for (int i = oldSize; i < newSize; ++i)
		{
			var prefabInstance = pool.prefabInstances[i];

			var wrapper = prefabInstance.AddComponent<PFXWrapper>();
			wrapper.ps = prefabInstance.GetComponent<ParticleSystem>();
			wrapper.sourcePrefab = pool.prefab; //store the source prefab for pooling Return
		}
	}
}
