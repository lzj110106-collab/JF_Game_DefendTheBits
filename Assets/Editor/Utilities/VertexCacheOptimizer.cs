using UnityEngine;
using System.Collections.Generic;

public class VertexCacheOptimizer
{
	private const int kMaxVertexCacheSize = 64;
	private const int kMaxPrecomputedVertexValenceScores = 64;
	private const int kEvictedCachedIndex = int.MaxValue;
	private const float kFindVertexScore_CacheDecayPower = 1.5f;
	private const float kFindVertexScore_LastTriScore = 0.75f;
	private const float kFindVertexScore_ValenceBoostScale = 2.0f;
	private const float kFindVertexScore_ValenceBoostPower = 0.5f;
	private static float[,] sm_vertexCacheScores = new float[kMaxVertexCacheSize + 1,kMaxVertexCacheSize];
	private static float[] sm_vertexValenceScores = new float[kMaxPrecomputedVertexValenceScores];

	class VertexData
	{
		public float score;
		public int indexStart;
		public int indexCount;
		public int cachePos0 = kEvictedCachedIndex;
		public int cachePos1 = kEvictedCachedIndex;
	}

	static VertexCacheOptimizer()
	{
		for (int cacheSize = 0; cacheSize <= kMaxVertexCacheSize; ++cacheSize)
		{
			for (int cachePos = 0; cachePos < cacheSize; ++cachePos)
			{
				sm_vertexCacheScores[cacheSize,cachePos] = ComputeVertexCacheScore(cachePos, cacheSize);
			}
		}

		for (int valence = 0; valence < kMaxPrecomputedVertexValenceScores; ++valence)
		{
			sm_vertexValenceScores[valence] = ComputeVertexValenceScore(valence);
		}
	}

	public static void Optimize(Mesh mesh, int modelledCacheSize = 32)
	{
		int subMeshCount = mesh.subMeshCount;
		var vertices = mesh.vertices;
		for (int i = 0; i < subMeshCount; ++i)
		{
			var triangles = mesh.GetTriangles(i);
			var newTriangles = Optimize(vertices, triangles, modelledCacheSize);
			mesh.SetTriangles(newTriangles, i);
		}
	}

	public static int[] Optimize(Vector3[] vertices, int[] triangles, int modelledCacheSize = 32)
	{
		int indexCount = triangles.Length;
		var newTriangles = new int[indexCount];

		int vertexCount = vertices.Length;
		var vertexData = new VertexData[vertexCount];
		for (int i = 0; i < vertexCount; ++i)
			vertexData[i] = new VertexData();

		for (int i = 0; i < indexCount; ++i)
		{
			int index = triangles[i];
			if (index >= vertexCount)
				Debug.LogError("Invalid triangle index: " + index + "/" + vertexCount);
			vertexData[index].indexCount++;
		}

		int indexPos = 0;
		for (int i = 0; i < vertexCount; ++i)
		{
			var vd = vertexData[i];
			vd.indexStart = indexPos;
			indexPos += vd.indexCount;
			vd.score = FindVertexScore(vd.indexCount, vd.cachePos0, modelledCacheSize);
			vd.indexCount = 0;
		}

		// fill out triangle list per vertex
		var activeTriangles = new int[indexPos];

		for (int i = 0; i < indexCount; i += 3)
		{
			for (int j = 0; j < 3; ++j)
			{
				int index = triangles[i + j];
				var vd = vertexData[index];
				activeTriangles[vd.indexStart + vd.indexCount] = i;
				vd.indexCount++;
			}
		}

		var processedTriangles = new bool[indexCount];
		var cache0 = new int[modelledCacheSize+3];
		var cache1 = new int[modelledCacheSize+3];
		int entriesInCache0 = 0;
		int bestTriangle = 0;
		float bestScore = -1.0f;
		float maxValenceScore = FindVertexScore(1, kEvictedCachedIndex, modelledCacheSize) * 3.0f;

		for (int i = 0; i < indexCount; i += 3)
		{
			if (bestScore < 0.0f)
			{
				// no verts in the cache are used by any unprocessed faces so
                // search all unprocessed faces for a new starting point
                for (int j = 0; j < indexCount; j += 3)
                {
                	if (!processedTriangles[j])
                	{
                		int triangleIndex = j;
                		float triangleScore = 0.0f;
                		for (int k = 0; k < 3; ++k)
                		{
                			int index = triangles[triangleIndex + k];
                			var vd = vertexData[index];
                			if (vd.indexCount <= 0)
                				Debug.LogError("Invalid index count");
                			if (vd.cachePos0 < modelledCacheSize)
                				Debug.LogError("Invalid cache0 position");
                			triangleScore += vd.score;
                		}

                		if (triangleScore > bestScore)
                		{
                			bestScore = triangleScore;
                			bestTriangle = triangleIndex;

                			if (bestScore > maxValenceScore)
                				Debug.LogError("Max valence score exceeded");
                			if (bestScore >= maxValenceScore)
                				break;
                		}
                	}
                }

                if (bestScore < 0.0f)
                	Debug.LogError("Invalid best score");
			}

			processedTriangles[bestTriangle] = true;
			int entriesInCache1 = 0;

			// add bestTriangle to LRU cache and to newTriangles
			for (int v = 0; v < 3; ++v)
			{
				int index = triangles[bestTriangle + v];
				newTriangles[i + v] = index;

				var vd = vertexData[index];

				if (vd.cachePos1 >= entriesInCache1)
				{
					vd.cachePos1 = entriesInCache1;
					cache1[entriesInCache1++] = index;

					if (vd.indexCount == 1)
					{
						--vd.indexCount;
						continue;
					}
				}

				if (vd.indexCount <= 0)
					Debug.Log("Invalid index count");

				int begin = vd.indexStart;
				int end = begin + vd.indexCount;
				bool found = false;
				for (int j = begin; j < end; ++j)
				{
					if (activeTriangles[j] == bestTriangle)
					{
						int tempTriangle = activeTriangles[j];
						activeTriangles[j] = activeTriangles[end - 1];
						activeTriangles[end - 1] = tempTriangle;
						found = true;
						break;
					}
				}
				if (!found)
					Debug.LogError("Could not find best triangle: " + bestTriangle);

				--vd.indexCount;
				vd.score = FindVertexScore(vd.indexCount, vd.cachePos1, modelledCacheSize);
			}

			// move the rest of the old verts in the cache down and compute their new scores
			for (int c0 = 0; c0 < entriesInCache0; ++c0)
			{
				int index = cache0[c0];
				var vd = vertexData[index];

				if (vd.cachePos1 >= entriesInCache1)
				{
					vd.cachePos1 = entriesInCache1;
					cache1[entriesInCache1++] = index;
					vd.score = FindVertexScore(vd.indexCount, vd.cachePos1, modelledCacheSize);
				}
			}

            // find the best scoring triangle in the current cache (including up to 3 that were just evicted)
            bestScore = -1.0f;
            for (int c1 = 0; c1 < entriesInCache1; ++c1)
            {
            	int index = cache1[c1];
            	var vd = vertexData[index];
            	vd.cachePos0 = vd.cachePos1;
            	vd.cachePos1 = kEvictedCachedIndex;
            	for (int j = 0; j < vd.indexCount; ++j)
            	{
            		int triangle = activeTriangles[vd.indexStart + j];
            		float triangleScore = 0.0f;
            		for (int v = 0; v < 3; ++v)
            		{
            			int triangleIndex = triangles[triangle + v];
            			triangleScore += vertexData[triangleIndex].score;
            		}
            		if (triangleScore > bestScore)
            		{
            			bestScore = triangleScore;
            			bestTriangle = triangle;
            		}
            	}
            }

            var temp = cache0;
            cache0 = cache1;
            cache1 = temp;
            entriesInCache0 = Mathf.Min(entriesInCache1, modelledCacheSize);
		}

		return newTriangles;
	}

	private static float FindVertexScore(int numActiveTriangles, int cachePosition, int vertexCacheSize)
	{
		if (numActiveTriangles == 0)
		{
			// No tri needs this vertex!
			return -1.0f;
		}

		float score = 0.0f;
		if (cachePosition < vertexCacheSize)
		{
			score += sm_vertexCacheScores[vertexCacheSize,cachePosition];
		}

		if (numActiveTriangles < kMaxPrecomputedVertexValenceScores)
		{
			score += sm_vertexValenceScores[numActiveTriangles];
		}
		else
		{
			score += ComputeVertexValenceScore(numActiveTriangles);
		}

		return score;
	}

	private static float ComputeVertexCacheScore(int cachePosition, int vertexCacheSize)
	{
		float score = 0.0f;
		if (cachePosition < 0)
		{
			 // Vertex is not in FIFO cache - no score
		}
		else
		{
			if (cachePosition < 3)
			{
				// This vertex was used in the last triangle,
                // so it has a fixed score, whichever of the three
                // it's in. Otherwise, you can get very different
                // answers depending on whether you add
                // the triangle 1,2,3 or 3,1,2 - which is silly.
                score = kFindVertexScore_LastTriScore;
			}
			else
			{
				if (cachePosition >= vertexCacheSize)
					Debug.LogError("Invalid cache position");
				float scaler = 1.0f / (vertexCacheSize - 3);
				score = 1.0f - (cachePosition - 3) * scaler;
				score = Mathf.Pow(score, kFindVertexScore_CacheDecayPower);
			}
		}

		return score;
	}

	private static float ComputeVertexValenceScore(int numActiveTriangles)
	{
		float score = 0.0f;

		// Bonus points for having a low number of tris still to
		// use the vert, so we get rid of lone verts quickly.
		float valenceBoost = Mathf.Pow((float)numActiveTriangles, -kFindVertexScore_ValenceBoostPower);
		score += kFindVertexScore_ValenceBoostScale * valenceBoost;

		return score;
	}
}