using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldQuery<T>
{
	public WorldQuery(int size)
	{
		objects = new T[size];
	}

	public void Add(T item)
	{
		//NB: all queries are created to have the same size
		//as the total possible objects of the query type in the world.
		//this means we can skip the bounds checking for each item
		//which should save lots of branches
		objects[found++] = item;
	}

	public T Get(int index)
	{
		return objects[index];
	}

	public void Clear()
	{
		found = 0;
	}

	public T[] objects { get; private set; }
	public int found;
}
