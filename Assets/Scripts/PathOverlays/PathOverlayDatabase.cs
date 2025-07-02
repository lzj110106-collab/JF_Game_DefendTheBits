using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PathOverlayData
{
	public enum Type
	{
		Mud
	}

	public Type type;
	public GameObject prefab;
	public GameObject prefabSlope;
	public float fadeOutDuration;

	public string entryAnimation;
	public string exitTrigger;

	public bool doesAdjustLandscape;
}

public class PathOverlayDatabase: MonoBehaviour
{
	static PathOverlayDatabase instance;

	public List<PathOverlayData> overlaySourceData;

	void Awake()
	{ 
		instance = this; 

		//make sure to sort this so indexing doesnt blow up
		overlaySourceData.Sort((item0, item1) => { return item0.type.CompareTo(item1.type); });
	}


	void OnDestroy()
	{ 
		instance = null; 
	}

	public static PathOverlayData Get(PathOverlayData.Type type)
	{
		return instance.overlaySourceData[(int)type];
	}

	public static PathOverlayData Get(int type)
	{
		return instance.overlaySourceData[type];
	}
}
