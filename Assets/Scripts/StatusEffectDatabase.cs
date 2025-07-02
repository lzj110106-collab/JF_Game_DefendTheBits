using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectDatabase : MonoBehaviour 
{
	static StatusEffectDatabase instance;

	[System.Serializable]
	public class EffectData
	{
		public WeaponStatusEffectType type;
		public GameObject pfxPrefab;
		public GameObject meshPrefab;
		public string attachToLocator;
		public Sprite iconUI;
	}

	public List<EffectData> effectData;

	void Awake() { instance = this; }
	void OnDestroy() { instance = null; }

	public static EffectData Get(WeaponStatusEffectType type)
	{
		foreach (var data in instance.effectData)
			if (data.type == type)
				return data;

		return null;
	}
}
