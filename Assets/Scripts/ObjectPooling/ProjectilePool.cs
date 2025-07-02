using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour 
{
	static ProjectilePool instance;

	public GenericPrefabPool source;

	void Awake()
	{
		instance = this;
		source.Initialise("ProjectilePool");
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
}
