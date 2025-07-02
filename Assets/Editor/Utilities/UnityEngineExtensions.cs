using UnityEngine;
using System.Collections.Generic;

public static class UnityEngineExtensions
{
	public static void Normalize(this Quaternion q)
	{
#if false
		q = Quaternion.Lerp(q, q, 1.0f); // Lerp normalizes	
#else
		float sum = 0;
		for (int i = 0; i < 4; ++i)
			sum += q[i] * q[i];
	
		float magnitudeInverse = 1 / Mathf.Sqrt(sum);
		for (int i = 0; i < 4; ++i)
			q[i] *= magnitudeInverse;
#endif
	}
	
	//public static Quaternion GetRotation(this Matrix4x4 matrix)
	//{
	//	return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
	//}
	
	public static void SetPosX(this Transform tr, float x)
	{
		Vector3 pos = tr.position;
		pos.x = x;
		tr.position = pos;
	}
	
	public static void SetPosY(this Transform tr, float y)
	{
		Vector3 pos = tr.position;
		pos.y = y;
		tr.position = pos;
	}
	
	public static void SetPosZ(this Transform tr, float z)
	{
		Vector3 pos = tr.position;
		pos.z = z;
		tr.position = pos;
	}
	
	public static void SetIdentity(this Transform tr)
	{
		tr.localPosition = Vector3.zero;
		tr.localRotation = Quaternion.identity;
		tr.localScale = Vector3.one;
	}

	public static T GetComponent<T>(this Transform transform) where T : Component
	{
		return transform.gameObject.GetComponent<T>();
	}

	public static T[] GetComponents<T>(this Transform transform) where T : Component
	{
		return transform.gameObject.GetComponents<T>();
	}

	public static T GetComponentInChildren<T>(this Transform transform) where T : Component
	{
		return transform.gameObject.GetComponentInChildren<T>();
	}

	public static T[] GetComponentsInChildren<T>(this Transform transform) where T : Component
	{
		return transform.gameObject.GetComponentsInChildren<T>();
	}
	
	public delegate bool ForEachComponentDelegate<T>(T component) where T : Component;

	private static bool ForEachComponentInDescendantsDepthFirst<T>(this GameObject gameObject, ForEachComponentDelegate<T> del) where T : Component
	{
		foreach (Transform child in gameObject.transform)
			if (!child.gameObject.ForEachComponentInDescendantsDepthFirst<T>(del))
				return false;

		foreach (var component in gameObject.GetComponents<T>())
		{
			if (!del(component))
				return false;
		}

		return true;
	}

	private static bool ForEachComponentInDescendantsBreadthFirst<T>(this GameObject gameObject, ForEachComponentDelegate<T> del) where T : Component
	{
		foreach (var component in gameObject.GetComponents<T>())
		{
			if (!del(component))
				return false;
		}

		foreach (Transform child in gameObject.transform)
			if (!child.gameObject.ForEachComponentInDescendantsDepthFirst<T>(del))
				return false;

		return true;
	}

	public static bool ForEachComponentInDescendants<T>(this GameObject gameObject, ForEachComponentDelegate<T> del, bool depthFirst = true) where T : Component
	{
		if (depthFirst)
			return gameObject.ForEachComponentInDescendantsDepthFirst<T>(del);
		else
			return gameObject.ForEachComponentInDescendantsBreadthFirst<T>(del);
	}

	public static bool ForEachComponentInChildren<T>(this GameObject gameObject, ForEachComponentDelegate<T> del) where T : Component
	{
		foreach (Transform child in gameObject.transform)
		{
			var components = child.GetComponents<T>();
			foreach (var component in components)
				if (!del(component))
					return false;
		}

		return true;
	}

	public static bool ForEachComponent<T>(this GameObject gameObject, ForEachComponentDelegate<T> del) where T : Component
	{
		var components = gameObject.GetComponents<T>();
		foreach (var component in components)
			if (!del(component))
				return false;

		return true;
	}

	public static bool ForEachComponentInSiblings<T>(this GameObject gameObject, UnityEngineExtensions.ForEachComponentDelegate<T> del) where T : Component
	{
		var parent = gameObject.transform.parent;
		if (parent == null)
			return true;

		foreach (Transform sibling in parent)
		{
			var components = sibling.GetComponents<T>();
			foreach (var component in components)
				if (!del(component))
					return false;
		}

		return true;
	}

	public static T GetComponentInDescendants<T>(this GameObject go, bool depthFirst = true) where T : Component
	{
		T component = null;
		go.ForEachComponentInDescendants<T>(
			delegate(T c)
			{
				component = c;
				return false;
			},
			depthFirst
		);
		return component;
	}
	
	public static T GetComponentInDescendants<T>(this Component component) where T : Component
	{
		return GetComponentInDescendants<T>(component.gameObject);
	}
	
	public static T GetComponentInDescendants<T>(this Transform transform) where T : Component
	{
		return GetComponentInDescendants<T>(transform.gameObject);
	}

	private static void GetComponentsInDescendantsDepthFirst<T>(GameObject go, ref List<T> components) where T : Component
	{
		foreach (Transform tr in go.transform)
		{
			GetComponentsInDescendantsDepthFirst(tr.gameObject, ref components);
		}

		components.AddRange(go.GetComponents<T>());
	}
	
	private static void GetComponentsInDescendantsBreadthFirst<T>(GameObject go, ref List<T> components) where T : Component
	{
		components.AddRange(go.GetComponents<T>());

		foreach (Transform tr in go.transform)
		{
			GetComponentsInDescendantsBreadthFirst(tr.gameObject, ref components);
		}
	}

	public static T[] GetComponentsInDescendants<T>(this GameObject go, bool depthFirst = true) where T : Component
	{
		List<T> components = new List<T>();
		if (depthFirst)
			GetComponentsInDescendantsDepthFirst(go, ref components);
		else
			GetComponentsInDescendantsBreadthFirst(go, ref components);
		return components.ToArray();
	}
	
	public static T[] GetComponentsInDescendants<T>(this Component component) where T : Component
	{
		return GetComponentsInDescendants<T>(component.gameObject);
	}

	public static T[] GetComponentsInDescendants<T>(this Transform transform) where T : Component
	{
		return GetComponentsInDescendants<T>(transform.gameObject);
	}
	
	public static T GetComponentInAscendants<T>(this GameObject go) where T : Component
	{
		while (go.transform.parent != null)
		{
			T component = go.transform.parent.GetComponent<T>();
			if (component != null)
				return component;
			go = go.transform.parent.gameObject;
		}
		
		return null;
	}
	
	public static T GetComponentInAscendants<T>(this Component component) where T : Component
	{
		return GetComponentInAscendants<T>(component.gameObject);
	}

	public static T GetComponentInAscendants<T>(this Transform transform) where T : Component
	{
		return GetComponentInAscendants<T>(transform.gameObject);
	}

	public static T[] GetComponentsInSiblings<T>(this GameObject go) where T : Component
	{
		if (go.transform.parent == null)
			return null;
		
		List<T> components = new List<T>();
		foreach (Transform transform in go.transform.parent)
			if (transform != go.transform)
				components.AddRange(transform.gameObject.GetComponents<T>());
		return components.ToArray();
	}
	
	public static T[] GetComponentsInSiblings<T>(this Component component) where T : Component
	{
		return GetComponentsInSiblings<T>(component.gameObject);
	}

	public static T[] GetComponentsInSiblings<T>(this Transform transform) where T : Component
	{
		return GetComponentsInSiblings<T>(transform.gameObject);
	}

	public static T GetComponentInSiblings<T>(this GameObject go) where T : Component
	{
		if (go.transform.parent == null)
			return null;

		foreach (Transform transform in go.transform.parent)
		{
			if (transform != go.transform)
			{
				T component = transform.gameObject.GetComponent<T>();
				if (component != null)
					return component;
			}
		}

		return null;
	}

	public static T GetComponentInSiblings<T>(this Component component) where T : Component
	{
		return GetComponentInSiblings<T>(component.gameObject);
	}

	public static T GetComponentInSiblings<T>(this Transform transform) where T : Component
	{
		return GetComponentInSiblings<T>(transform.gameObject);
	}
	
	public static void SetActive(this Component component, bool active)
	{
		component.gameObject.SetActive(active);
	}
	
	public static bool Approximately(this Vector3 a, Vector3 b)
	{
		return Mathf.Approximately(a.x, b.x)
			&& Mathf.Approximately(a.y, b.y)
			&& Mathf.Approximately(a.z, b.z);
	}

	public static Transform Search(this Transform target, string name)
	{
		if (target.name == name)
			return target;
 
		for (int i = 0; i < target.childCount; ++i)
		{
			var result = Search(target.GetChild(i), name);
 
			if (result != null)
				return result;
		}
 
		return null;
	}

	public static bool IsVisible(this GameObject go)
	{
		foreach (Transform child in go.transform)
		{
			if (child.GetComponent<Renderer>() != null && child.GetComponent<Renderer>().isVisible)
				return true;
			if (child.gameObject.IsVisible())
				return true;
		}

		if (go.GetComponent<Renderer>() != null && go.GetComponent<Renderer>().isVisible)
			return true;
		else
			return false;
	}

	public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
	{
		var planes = GeometryUtility.CalculateFrustumPlanes(camera);
		return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
	}

	public static string GetPath(this Transform transform)
	{
		string path = "";
		while (transform != null)
		{
			string newPath = transform.name + path;
			transform = transform.parent;
			if (transform != null)
				if (transform.parent != null)
					path = "/" + newPath;
				else
					path = newPath;
		}
		return path;
	}

    public static void FromMatrix4x4(this Transform transform, Matrix4x4 matrix)
    {
        transform.localPosition = matrix.GetPosition();
        transform.localRotation = matrix.GetRotation();
        transform.localScale = matrix.GetScale();
    }

    public static Vector3 GetPosition(this Matrix4x4 matrix)
    {
        var x = matrix.m03;
        var y = matrix.m13;
	    var z = matrix.m23;

        return new Vector3(x, y, z);
    }

    public static Quaternion GetRotation(this Matrix4x4 matrix)
    {
        var qw = Mathf.Sqrt(1f + matrix.m00 + matrix.m11 + matrix.m22) / 2;
        var w = 4 * qw;

        var qx = (matrix.m21 - matrix.m12) / w;
        var qy = (matrix.m02 - matrix.m20) / w;
        var qz = (matrix.m10 - matrix.m01) / w;

        return new Quaternion(qx, qy, qz, qw);
    }

    public static Vector3 GetScale(this Matrix4x4 m)
    {
        var x = Mathf.Sqrt(m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
        var y = Mathf.Sqrt(m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
        var z = Mathf.Sqrt(m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);

        return new Vector3(x, y, z);
    }

	public static float Determinant(this Matrix4x4 m)
	{
		return
			m.m03 * m.m12 * m.m21 * m.m30-m.m02 * m.m13 * m.m21 * m.m30-m.m03 * m.m11 * m.m22 * m.m30+m.m01 * m.m13 * m.m22 * m.m30+
			m.m02 * m.m11 * m.m23 * m.m30-m.m01 * m.m12 * m.m23 * m.m30-m.m03 * m.m12 * m.m20 * m.m31+m.m02 * m.m13 * m.m20 * m.m31+
			m.m03 * m.m10 * m.m22 * m.m31-m.m00 * m.m13 * m.m22 * m.m31-m.m02 * m.m10 * m.m23 * m.m31+m.m00 * m.m12 * m.m23 * m.m31+
			m.m03 * m.m11 * m.m20 * m.m32-m.m01 * m.m13 * m.m20 * m.m32-m.m03 * m.m10 * m.m21 * m.m32+m.m00 * m.m13 * m.m21 * m.m32+
			m.m01 * m.m10 * m.m23 * m.m32-m.m00 * m.m11 * m.m23 * m.m32-m.m02 * m.m11 * m.m20 * m.m33+m.m01 * m.m12 * m.m20 * m.m33+
			m.m02 * m.m10 * m.m21 * m.m33-m.m00 * m.m12 * m.m21 * m.m33-m.m01 * m.m10 * m.m22 * m.m33+m.m00 * m.m11 * m.m22 * m.m33;
	}

    public static bool IsPointVisible(this Camera camera, Vector3 point)
    {
		Vector3 viewportPos = camera.WorldToViewportPoint(point);
		return !(viewportPos.x < 0.0f || viewportPos.x > 1.0f || viewportPos.y < 0.0f || viewportPos.y > 1.0f);
    }

    public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        var dir = body.transform.position - explosionPosition;
        float wearoff = 1 - (dir.magnitude / explosionRadius);
        body.AddForce(dir.normalized * explosionForce * wearoff);
    }

    public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier)
    {
        var dir = body.transform.position - explosionPosition;
        float wearoff = 1 - (dir.magnitude / explosionRadius);
        Vector3 baseForce = dir.normalized * explosionForce * wearoff;
        body.AddForce(baseForce);

        float upliftWearoff = 1 - upliftModifier / explosionRadius;
        Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
        body.AddForce(upliftForce);
    }

	public static Bounds TransformBounds(this Transform transform, Bounds bounds)
	{
		bounds.min = transform.TransformPoint(bounds.min);
		bounds.max = transform.TransformPoint(bounds.max);
		return bounds;
	}
}