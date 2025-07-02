using System.Collections.Generic;
using UnityEngine;

public class RecycleStack
{
	Stack<GameObject> Pool = new Stack<GameObject>();

	public void Recycle(GameObject gameObj)
	{
		gameObj.SetActive(false);
		Pool.Push(gameObj);
	}

	public GameObject RetrieveOrCreate(GameObject prefab)
	{
		if (Pool.Count > 0)
		{
			GameObject toReturn = Pool.Pop ();
			toReturn.SetActive (true);
			return toReturn;
		}
		else
			return GameObject.Instantiate(prefab);
	}

	public GameObject Create(GameObject prefab)
	{
		return GameObject.Instantiate (prefab);
	}

	public void Empty()
	{
		while (Pool.Count > 0)
			GameObject.Destroy(Pool.Pop());
	}

	public void Clear()
	{
		Pool.Clear();
	}
}
