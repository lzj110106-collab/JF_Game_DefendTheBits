using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinketDatabase : MonoBehaviour 
{
	static TrinketDatabase instance;

	public List<InteractReward> trinketPrefabs;
	public static Dictionary<string, GameObject> trinketPrefabsByID;

	void Awake()
	{ 
		trinketPrefabsByID = new Dictionary<string, GameObject>();

		for (int i = 0; i < trinketPrefabs.Count; ++i)
			trinketPrefabsByID.Add(trinketPrefabs[i].trinketID, trinketPrefabs[i].gameObject);
		
		instance = this; 
	}

	void OnDestroy() 
	{ 
		instance = null;
	}

	public static GameObject GetPrefab(string identifier)
	{
		GameObject result;
		if (trinketPrefabsByID.TryGetValue(identifier, out result))
			return result;

		return null;
	}

	public static Sprite GetIcon(string identifier)
	{
		//this is the worst. the worst.
		return TrinketDatabase.GetPrefab(identifier).GetComponent<InteractReward>().trinketIcon;
	}
}
