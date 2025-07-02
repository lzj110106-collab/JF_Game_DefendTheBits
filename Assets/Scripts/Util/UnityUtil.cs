using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnloadUnusedAssetsHelper: MonoBehaviour
{
	static UnloadUnusedAssetsHelper instance = null;

	public void Run()
	{	
		if (instance == null)
		{
			instance = this;
			StartCoroutine(PerformUnload());
		}
		else
		{
			//unload already running. self destruct.
			Destroy(gameObject);
		}
	}

	IEnumerator PerformUnload()
	{
		Debug.Log("[UnloadUnusedAssetHelper] BEGIN UNLOAD");

		var asyncOp = Resources.UnloadUnusedAssets();
		yield return asyncOp;

		Debug.Log("[UnloadUnusedAssetHelper] BEGIN GC");

		//unload complete, do a GC pass now and we are done
		System.GC.Collect(); 
		instance = null;

		Debug.Log("[UnloadUnusedAssetHelper] UNLOAD COMPLETE");
		Destroy(gameObject);
	}
}

public class UnityUtil
{
	public static GameObject FindChild(Transform transform, string name)
	{
		foreach (Transform child in transform)
		{
			if (child.name.Contains(name))
				return child.gameObject;
			
			var result = FindChild(child, name);
			if (result != null)
				return result.gameObject;
		}
		
		return null;
	}

	//NB: editor scripts must use DestroyImmediate
	public static void DestroyAllChildren(GameObject go, bool immediate = false)
	{
		if (go == null)
			return;
		
		var toDestroy = new List<Transform>(go.transform.childCount);
		foreach (Transform child in go.transform)
			toDestroy.Add (child);
		
		foreach (var child in toDestroy)
		{
			if (child.gameObject != null)
			{
				if (immediate)
					GameObject.DestroyImmediate(child.gameObject);
				else
					GameObject.Destroy(child.gameObject);
			}
		}
	}

	public static void SetLayerRecursive(GameObject target, GameObject reference)
	{
		SetLayerRecursive(target, reference.layer);
	}

	public static void SetLayerRecursive(GameObject go, int layer)
	{
		go.layer = layer;
		
		foreach (Transform child in go.transform)
			SetLayerRecursive(child.gameObject, layer);
	}	

	public static void SetActiveRecursively(GameObject target, bool active)
	{
		target.SetActive(active);

		foreach (Transform child in target.transform)
			SetActiveRecursively(child.gameObject, active);
	}
	
	public static Vector2 WorldToCanvas(Canvas canvas, Vector3 world, Camera camera = null)
	{
		//if no camera was supplied, use the one attached to the canvas
		if (camera == null)
			camera = canvas.worldCamera;

		//if that failed, use the main camera
		if (camera == null)
			camera = Camera.main;

		//if camera is still null, then red error everywhere so that one can be passed in
		var viewport = camera.WorldToViewportPoint(world);
		var rect = canvas.GetComponent<RectTransform>();
		
		return new Vector2((viewport.x * rect.sizeDelta.x) - rect.sizeDelta.x*0.5f, 
		                   (viewport.y * rect.sizeDelta.y) - rect.sizeDelta.y*0.5f);
	}

	public static bool CalculateBoundingBox(GameObject target, out Bounds result, bool ignoreShadows = true)
	{
		//rubbish function to get around c# structs (cant test for null) and
		//unity not allowing negative dimension boxes (cant just chain Encapsulates)

		result = new Bounds();
		bool initialised = false;

		if (target == null)
			return false;

		var renderers = target.GetComponentsInChildren<MeshRenderer>(true);
		if (renderers == null || renderers.Length == 0)
			return false;

		foreach (var renderer in renderers)
		{
			if (ignoreShadows && renderer.name.ToLower().Contains("shadow_"))
				continue;

			Bounds bounds = renderer.bounds;
			if (bounds.size == Vector3.zero)
			{
				//skip zero size bounding boxes
			}
			else
			{
				if (!initialised)
				{
					result = bounds;
					initialised = true;
				}
				else
				{
					result.Encapsulate(bounds);
				}
			}
		}

		return initialised;
	}

	public static bool CalculateBoundingBoxLocal(GameObject target, out Bounds result)
	{
		if (CalculateBoundingBox(target, out result))
		{
			result.SetMinMax(target.transform.InverseTransformPoint(result.min), 
							 target.transform.InverseTransformPoint(result.max));

			return true;
		}

		return false;
	}

	public static void PauseObject(GameObject target, bool pause)
	{
		foreach (var animator in target.GetComponentsInChildren<Animator>(true))
			animator.speed = pause ? 0.0f : 1.0f;

		foreach (var ps in target.GetComponentsInChildren<ParticleSystem>(true))
			ps.playbackSpeed = pause ? 0.0f : 1.0f;
	}

	public static void PrintParentHierarchy(GameObject target, string soFar)
	{
		soFar = target.name + " / " + soFar;

		if (target.transform.parent == null)
		{
			Debug.Log(soFar);
			return;
		}

		PrintParentHierarchy(target.transform.parent.gameObject, soFar);
	}

	//helper coroutine for freeing up memory, where you dont care about order of operations particularly
	public static void UnloadUnusedAssets()
	{
		var gameObject = new GameObject("UnloadUnusedAssetsHelper");
		var comp = gameObject.AddComponent<UnloadUnusedAssetsHelper>();
		comp.Run();
	}

	public static GameObject InstantiateResourceImmediate(string path)
	{
		var prefab = Resources.Load<GameObject>(path);
		if (prefab != null)
			return GameObject.Instantiate(prefab);

		Debug.Log("[UnityUtil] unknown resource path: " + path);
		return null;
	}
}
