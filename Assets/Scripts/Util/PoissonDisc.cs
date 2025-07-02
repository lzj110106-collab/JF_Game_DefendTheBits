using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoissonDisc
{
	public List<Vector3> points { get; private set; }

	public PoissonDisc(float discRadius, float pointSeparation, int attempts)
	{
		float rad2 = discRadius*discRadius;
		float sep2 = pointSeparation*pointSeparation;

		points = new List<Vector3>(32);

		for (int i = 0; i < attempts; ++i)
		{
			float x = Random.Range (-discRadius, discRadius);
			float y = Random.Range (-discRadius, discRadius);

			var newPoint = new Vector3(x, y, 0);
			bool validPoint = true;

			if (Vector3.SqrMagnitude(newPoint) <= rad2)
			{
				for (int j = 0; j < points.Count && validPoint; ++j)
				{
					float dist2 = Vector3.SqrMagnitude(points[j] - newPoint);
					if (dist2 < sep2)
						validPoint = false;
				}

				if (validPoint)
					points.Add(newPoint);
			}
		}
//
//		Debug.Log("POISSON DISC SIZE " + points.Count);
	}

	public void DrawPoints(Vector3 centre, Color c)
	{
		for (int i = 0; i < points.Count; ++i)
			DebugDrawUtil.DrawCircleXY(centre + points[i] + Vector3.back, 0.25f, c);
	}
}
