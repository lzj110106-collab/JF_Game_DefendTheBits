using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugDrawUtil
{
	public static void Draw(Vector3 point, Color c, float size = 1.0f)
	{
	#if !FINAL
		float h = size * 0.5f;
		Draw (point - new Vector3(h, 0, 0), point + new Vector3(h, 0, 0), c);
		Draw (point - new Vector3(0, h, 0), point + new Vector3(0, h, 0), c);
		Draw (point - new Vector3(0, 0, h), point + new Vector3(0, 0, h), c);
	#endif
	}

	public static void Draw(Vector3 from, Vector3 to, Color c, bool arrow = false)
	{
	#if !FINAL
		if (arrow)
		{
			Vector3 direction = Vector3.Normalize(to - from), side, up;

			if (Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > 0.9f)
				side = Vector3.Cross(direction, Vector3.right);
			else
				side = Vector3.Cross(direction, Vector3.up);
			
			up = Vector3.Normalize(Vector3.Cross(direction, side));
			side = Vector3.Normalize(Vector3.Cross(direction, up));
			
			float scale = 0.25f;
			Vector3 pos = to - direction*scale;

			Draw (from, to, c);
			Draw (from, to, c);
			Draw (to, pos + side*scale, c);
			Draw (to, pos - side*scale, c);
			Draw (to, pos + up*scale, c);
			Draw (to, pos - up*scale, c);
		}
		else
		{
			Color original = Gizmos.color;
			Gizmos.color = c;
			Gizmos.DrawLine(from, to);
			Gizmos.color = original;

		}
	#endif
	}
		
	public static void Draw(List<Vector3> polygon, Color c)
	{
	#if !FINAL
		for (int j = polygon.Count - 1, i = 0; i < polygon.Count; j = i++)
			Draw (polygon[j], polygon[i], c);
	#endif
	}

	public static void Draw(Matrix4x4 m)
	{
	#if !FINAL
		Vector3 p = m.MultiplyPoint(Vector3.zero);
		Vector3 x = m.MultiplyVector(Vector3.right);
		Vector3 y = m.MultiplyVector(Vector3.up);
		Vector3 z = m.MultiplyVector(Vector3.forward);

		Draw (p, p + x, Color.red, true);
		Draw (p, p + y, Color.green, true);
		Draw (p, p + z, Color.blue, true);
	#endif
	}

	public static void DrawCircleXY(Vector3 centre, float radius, Color c)
	{
	#if !FINAL
		int segments = 32;
		float incr = 2.0f*Mathf.PI/segments;
		Vector3 from = centre + new Vector3(radius, 0.0f, 0.0f);

		for (int i = 0; i < segments; ++i)
		{
			float theta = (i + 1)*incr;
			Vector3 to = centre + new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), 0.0f);

			Draw (from, to, c, false);
			from = to;
		}
	#endif
	}

	public static void DrawCircleXZ(Vector3 centre, float radius, Color c)
	{
	#if !FINAL
		int segments = 32;
		float incr = 2.0f*Mathf.PI/segments;
		Vector3 from = centre + new Vector3(radius, 0.0f, 0.0f);
		
		for (int i = 0; i < segments; ++i)
		{
			float theta = (i + 1)*incr;
			Vector3 to = centre + new Vector3(radius * Mathf.Cos(theta), 0.0f, radius * Mathf.Sin(theta));
			
			Draw (from, to, c, false);
			from = to;
		}
	#endif
	}
}
