using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//TODO: support for loading and unloading folders on demand.

//NB: not thread safe.

public class GenericPrefabPool : MonoBehaviour 
{
	[Serializable]
	public class PrefabFolder
	{
		public string name;
		public int initialPoolSize = 8;
		public int expansionSize = 4;

		//list of prefabs for stuff that is in art folders and things. 
		//stuff that isnt loaded on demand
		[HideInInspector, SerializeField]
		public List<GameObject> prefabs;

		//list of stuff that is loaded on demand from resources
		[HideInInspector, SerializeField]
		public List<string> resourceLocations;
	}

	[Serializable]
	public class Pool
	{
		public GameObject prefab;
		public GameObject prefabContainer;
		public List<GameObject> prefabInstances;
		public List<bool> isAvailable;
		public int expansionSize;
	}
		
	[Serializable]
	public class PoolSizeOverride
	{
		public string prefabName;
		public int initialPoolSize;
	}

	public List<PrefabFolder> folders;
	public List<PoolSizeOverride> overrides;

	public Dictionary<GameObject, Pool> pools { get; private set; }
	Dictionary<string, int> poolInitialSizes;

	string debuggingPrefix;
	public bool debuggingEnabled = true;

	public delegate void OnPoolExpanded(Pool pool, int oldSize, int newSize);
	public OnPoolExpanded poolExpandedCallback;

	public void Initialise(string poolName) 
	{
		debuggingPrefix = "[" + poolName + "] ";
		pools = new Dictionary<GameObject, Pool>();

		poolInitialSizes = new Dictionary<string, int>();
		foreach (var data in overrides)
			poolInitialSizes.Add(data.prefabName, data.initialPoolSize);

		foreach (var folder in folders)
		{
			foreach (var prefab in folder.prefabs)
				pools.Add(prefab, CreatePool(prefab, folder.initialPoolSize, folder.expansionSize));

			foreach (var location in folder.resourceLocations)
			{
				var prefab = Resources.Load(location) as GameObject;
				if (prefab == null)
					continue;

				Pool poolInstance = null;
				if (!pools.TryGetValue(prefab, out poolInstance))
					pools.Add(prefab, CreatePool(prefab, folder.initialPoolSize, folder.expansionSize));
			}
		}

		//disable the game object so that unity doesnt attempt to
		//ever update us or the potentially huge hierarchy of
		//stuff underneath the pool.
		gameObject.SetActive(false);
	}

	//Reset() aliases a static function
	public void ResetPool()
	{
		foreach (var kv in pools)
		{
			for (int i = 0; i < kv.Value.prefabInstances.Count; ++i)
			{
				kv.Value.prefabInstances[i].transform.SetParent(kv.Value.prefabContainer.transform, false);
				kv.Value.prefabInstances[i].SetActive(false);

				kv.Value.isAvailable[i] = true;
			}
		}
	}

	public GameObject Get(GameObject prefab)
	{
		//generate a pool if we encounter a request for a prefab that wasnt pooled for some reason
		if (!pools.ContainsKey(prefab))
		{
			Log("generating pool for unknown prefab: " + prefab.name);
			pools.Add(prefab, CreatePool(prefab, 4, 4));
		}

		var pool = pools[prefab];

		//search for a free instance. only attempt this twice. once after an expansion if
		//the first attempt fails. don't want to cause infinite loops here somehow.
		for (int j = 0; j < 2; ++j)
		{
			for (int i = 0; i < pool.isAvailable.Count; ++i)
			{
				if (pool.isAvailable[i])
				{
					pool.isAvailable[i] = false;
					return pool.prefabInstances[i];
				}
			}

			//no free instances, expand the pool if possible.
			if (j == 0 && pool.expansionSize > 0)
			{
				ExpandPool(pool, pool.expansionSize);
			}
			else
			{
				break;
			}
		}

		//couldnt find a pooled instance
		return null;
	}

	public GameObject Get(GameObject prefab, GameObject attachTo)
	{
		var result = Get(prefab);
		if (result != null)
		{
			result.transform.SetParent(attachTo.transform, false);
			result.SetActive(false);

			return result;
		}

		return null;
	}

	public void Return(GameObject prefab, GameObject prefabInstance)
	{
		//null check here so the callers dont have to
		if (prefab == null || prefabInstance == null)
			return;

		//make sure the prefab exists in this pool first, otherwise something went wrong
		if (!pools.ContainsKey(prefab))
		{
			Log("couldnt return prefab: " + prefab.name + " (unknown prefab type)");
			return;
		}

		var pool = pools[prefab];

		for (int i = 0; i < pool.prefabInstances.Count; ++i)
		{
			if (pool.prefabInstances[i] == prefabInstance)
			{
				prefabInstance.transform.SetParent(pool.prefabContainer.transform, false);
				prefabInstance.SetActive(false);

				pool.isAvailable[i] = true;
				return;
			}
		}

		Log("couldnt return prefab: " + prefab.name + " (unknown prefab instance)");
	}

	Pool CreatePool(GameObject prefab, int initialPoolSize, int expansionSize)
	{
		var result = new Pool();
		result.prefab = prefab;
		result.prefabContainer = new GameObject(prefab.name + "_container");
		result.prefabContainer.transform.SetParent(transform, false);
		result.expansionSize = expansionSize;

		result.prefabInstances = new List<GameObject>();
		result.isAvailable = new List<bool>();

		int size = 0;
		if (!poolInitialSizes.TryGetValue(prefab.name, out size))
			size = initialPoolSize;

		ExpandPool(result, size);

		return result;
	}

	void ExpandPool(Pool pool, int additionalEntries)
	{
		int oldSize = pool.prefabInstances.Count;
		int newSize = oldSize + additionalEntries;

		var newPrefabInstances = new List<GameObject>(newSize);
		var newIsAvailable = new List<bool>(newSize);

		//copy over existing
		for (int i = 0; i < pool.prefabInstances.Count; ++i)
		{
			newPrefabInstances.Add(pool.prefabInstances[i]);
			newIsAvailable.Add(pool.isAvailable[i]);
		}

		//init new instances
		for (int i = pool.prefabInstances.Count; i < newSize; ++i)
		{
			var prefabInstance = GameObject.Instantiate(pool.prefab);
			prefabInstance.transform.SetParent(pool.prefabContainer.transform, false);
			prefabInstance.SetActive(false);

			//strip the (Clone) off the instance name and give it a number instead.
			//this is done purely for debugging reasons
			var name = prefabInstance.name;
			name = name.Replace("(Clone)", "").Trim();
			name = name + "_" + i.ToString();
			prefabInstance.name = name;

			newPrefabInstances.Add(prefabInstance);
			newIsAvailable.Add(true);
		}

		//copy over.
		pool.prefabInstances = newPrefabInstances;
		pool.isAvailable = newIsAvailable;

		if (oldSize > 0)
			Log("pool expanded: " + pool.prefab.name + " from " + oldSize + " to " + newSize);
		
		if (poolExpandedCallback != null)
			poolExpandedCallback(pool, oldSize, newSize);
	}

	public void Log(string message)
	{
		if (debuggingEnabled)
			Debug.Log(debuggingPrefix + message);
	}
}
