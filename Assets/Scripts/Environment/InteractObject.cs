using UnityEngine;
using System.Collections;

public abstract class InteractObject : MonoBehaviour, WorldObject
{
	public Collider interactCollider { get; private set; }
	[HideInInspector] public bool isInteracting;

	public abstract void OnTouch();
	public abstract void OnSwiped(Vector3 swipeDirection);

	public abstract void OnSpawned(GameObject sourcePrefab, Vector3 worldLocation);

	public virtual void OnFinished()
	{
		isInteracting = false;
	}

	public virtual void Awake()
	{
		interactCollider = GetComponent<Collider>();
	}

#region WorldObject

	public virtual bool UpdateTick() { return true; }
	public virtual void UpdateTicksComplete() {}

	public virtual void OnPause(bool pause) {}
	public virtual void OnTimeScaleAdjusted(float timeScale) {}

	public virtual void OnCharacterKilled(Character c) {}
	public virtual void OnWorldReset() {}

#endregion
}
