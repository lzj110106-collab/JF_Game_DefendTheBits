using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WeightedObjectListEntry
{
	public GameObject prefab;
	public float weighting;
}

[System.Serializable]
public class WeightedObjectList<T> where T : WeightedObjectListEntry, new()
{
	public List<T> objects;
	int lastChoice = -1;

	public T PickObject()
	{
		//deal with the trivial cases first
		if (objects == null || objects.Count == 0)
			return new T();

		if (objects.Count == 1)
			return objects[0];

		//we could precalc the weighting, but that stops runtime
		//editing of the values and its cheap to calc anyway
		float totalWeight = 0.0f;
		foreach (var obj in objects)
			totalWeight += obj.weighting;
		
		float choice = Random.Range(0, totalWeight);
		foreach (var obj in objects)
		{
			if (choice <= obj.weighting)
				return obj;
			
			choice -= obj.weighting;
		}

		//account for floating point error, return the first object
		return objects[0];
	}

	public T PickObjectNoRepeat()
	{
		//deal with the trivial cases first
		if (objects == null || objects.Count == 0)
			return new T();
		
		if (objects.Count == 1)
			return objects[0];
		
		//we could precalc the weighting, but that stops runtime
		//editing of the values and its cheap to calc anyway
		float totalWeight = 0.0f;
		foreach (var obj in objects)
			totalWeight += obj.weighting;

		int attempts = 0;
		while (attempts < 8)
		{
			float choice = Random.Range(0, totalWeight);
			for (int i = 0; i < objects.Count; ++i)
			{
				if (choice <= objects[i].weighting)
				{
					if (lastChoice == i)
					{
						//roll the dice again
						attempts += 1;
						break;
					}

					lastChoice = i;
					return objects[i];
				}
				
				choice -= objects[i].weighting;
			}
		}

		lastChoice = 0;
		return objects[0];
	}
}
