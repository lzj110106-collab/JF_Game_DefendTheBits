using UnityEngine;
using System.Collections.Generic;

public class MeshUtils
{
	public class Triangle
	{
		public int i0;
		public int i1;
		public int i2;
		public Vector3 centroid;
	}

	public class NegativeZTriangleComparer : IComparer<Triangle>
	{
		public int Compare(Triangle t0, Triangle t1)
		{
			return -t0.centroid.z.CompareTo(t1.centroid.z);
		}
	}

	public class PositiveZTriangleComparer : IComparer<Triangle>
	{
		public int Compare(Triangle t0, Triangle t1)
		{
			return t0.centroid.z.CompareTo(t1.centroid.z);
		}
	}

	public static int[] SortTriangles(Vector3[] vertices, int[] triangles, IComparer<Triangle> comparer)
	{
		int triangleIndexCount = triangles.Length;
		int triangleCount = triangleIndexCount / 3;
		var tris = new Triangle[triangleCount];
		for (int i = 0; i < triangleCount; ++i)
		{
			int i0 = i*3+0;
			int i1 = i*3+1;
			int i2 = i*3+2;
			tris[i] = new Triangle();
			int t0 = tris[i].i0 = triangles[i0];
			int t1 = tris[i].i1 = triangles[i1];
			int t2 = tris[i].i2 = triangles[i2];
			var v0 = vertices[t0];
			var v1 = vertices[t1];
			var v2 = vertices[t2];
			tris[i].centroid = (v0 + v1 + v2) / 3.0f;
		}

		System.Array.Sort(tris, comparer);

		var sortedTriangles = new int[triangleIndexCount];
		for (int i = 0; i < triangleCount; ++i)
		{
			var tri = tris[i];
			sortedTriangles[i*3+0] = tri.i0;
			sortedTriangles[i*3+1] = tri.i1;
			sortedTriangles[i*3+2] = tri.i2;
		}
		return sortedTriangles;		
	}
}
