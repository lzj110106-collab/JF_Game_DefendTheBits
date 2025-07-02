using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscapeMeshBuilder
{
	Landscape landscape;

	//store positions along each axis up front. all calculations in the
	//mesh building code will use these tables to ensure that all the
	//vertices generated share the exact same floating point numbers in
	//order to eliminate cracks in the resulting meshes.
	float[] xpos = null;
	float[] ypos = null;
	float[] zpos = null;

	int minHeight;
	int maxHeight;

	//mesh building constants
	int[] columnOffsets;// = new int[] { -1, 1, -w, w};
	int[] columnIndices = new int[] { 0, 3, 2, 1, 1, 0, 3, 2 };
	Vector3[] columnNormals = new Vector3[] { Vector3.left, Vector3.right, Vector3.back, Vector3.forward };

	int[][] rotationUVs;
	int slicesPerTile;

	//pre-alloc these for speed. matters at runtime on device for the non-fixed mesh rebuilds.
	Vector3[] topCorners = new Vector3[4];
	Vector2[] texData = new Vector2[4];

	public LandscapeMeshBuilder(Landscape target)
	{
		landscape = target;

		rotationUVs = new int[4][];
		rotationUVs[0] = new int[4] { 0, 1, 2, 3 };
		rotationUVs[1] = new int[4] { 1, 3, 0, 2 };
		rotationUVs[2] = new int[4] { 2, 3, 0, 1 };
		rotationUVs[3] = new int[4] { 3, 1, 2, 0 };
	}

	public void RefreshVertexPositionArrays()
	{
		//the landscape class doesnt track the overall height of the terrain. so
		//calculate those bounds before we reconstruct everything.
		minHeight = int.MaxValue;
		maxHeight = int.MinValue;

		for (int j = 0; j < landscape.h; ++j)
		{
			for (int i = 0; i < landscape.w; ++i)
			{
				minHeight = Mathf.Min (landscape.tiles[i + j*landscape.w].height, minHeight);
				maxHeight = Mathf.Max (landscape.tiles[i + j*landscape.w].height, maxHeight);
			}
		}

		minHeight -= (landscape.towerDepth + landscape.extraDepth);

		//now resize all the position arrays to fit the full extent of the terrain.
		//making sure to +1 the size for the last tile in the terrain. eg 4 tiles 
		//require 5 points to render correctly.
		if (xpos == null || xpos.Length < landscape.w + 1)
			xpos = new float[landscape.w + 1];

		if (ypos == null || ypos.Length < (maxHeight - minHeight) + 1)
			ypos = new float[(maxHeight - minHeight) + 1];

		if (zpos == null || zpos.Length < landscape.h + 1)
			zpos = new float[landscape.h + 1];

		//figure out the bottom left corner of the entire terrain.
		var start = new Vector3(landscape.x * landscape.tileWidth, 
								minHeight * landscape.tileHeight, 
								landscape.y * landscape.tileWidth);

		//populate the arrays
		for (var i = 0; i < xpos.Length; ++i)
			xpos[i] = start.x + i*landscape.tileWidth;

		for (var i = 0; i < ypos.Length; ++i)
			ypos[i] = start.y + i*landscape.tileHeight;

		for (var i = 0; i < zpos.Length; ++i)
			zpos[i] = start.z + i*landscape.tileWidth;

		//recalc constants used for checking adjacent columns
		columnOffsets = new int[] { -1, 1, -landscape.w, landscape.w};

		slicesPerTile = (int)Mathf.Round (1.0f/landscape.tileHeight);
	}

	public void RebuildMesh(Mesh target, bool fixedTerrainTilesOnly)
	{
		//dont bother rebuilding until Start() has been called and textures exist
		if (landscape.tileSheet == null || 
			landscape.tiles == null || 
			landscape.meshes == null)
		{
			return;
		}

		//top surfaces.
		int indexCount = 6*landscape.w*landscape.h;
		int vertexCount = 4*landscape.w*landscape.h;

		//need to figure out which sides of which tiles have data in them
		for (int j = 0; j < landscape.h; ++j)
		{
			for (int i = 0; i < landscape.w; ++i)
			{
				int index = i + j*landscape.w;
				var height = landscape.GetHeightOfTile(index, minHeight);

				for (int k = 0; k < 4; ++k)
				{
					var adjacentHeight = landscape.GetHeightOfTile(index + columnOffsets[k], minHeight);
					if (adjacentHeight < height)
						AddTileColumnVertices(height, adjacentHeight, ref vertexCount, ref indexCount);
				}
			}
		}

		//allocate
		var vertices = new Vector3[vertexCount];
		var normals = new Vector3[vertexCount];
		var uvs = new Vector2[vertexCount];
		var triangles = new int[indexCount];

		vertexCount = 0;

		//copy data in
		for (int j = 0; j < landscape.h; ++j)
		{
			for (int i = 0; i < landscape.w; ++i)
			{
				//ignore fixed tiles in the non-fixed mesh and vice-versa
				if (!ShouldAddTileToMeshType(fixedTerrainTilesOnly, i, j))
					continue;
				
				var index = i + j*landscape.w;
				var height = landscape.GetHeightOfTile(index, minHeight);
				var heightLookup = height - minHeight;

				topCorners[0].Set(xpos[i], ypos[heightLookup], zpos[j]);
				topCorners[1].Set(xpos[i + 1], ypos[heightLookup], zpos[j]);
				topCorners[2].Set(xpos[i + 1], ypos[heightLookup], zpos[j + 1]);
				topCorners[3].Set(xpos[i], ypos[heightLookup], zpos[j + 1]);

				//top surface
				{
					vertices[vertexCount + 0] = topCorners[0];
					vertices[vertexCount + 1] = topCorners[1];
					vertices[vertexCount + 2] = topCorners[3];
					vertices[vertexCount + 3] = topCorners[2];

					SetNormals(normals, vertexCount, Vector3.up);
					SetTile (uvs, 
						vertexCount, 
						landscape.tiles[index].tileIndexSurface, 
						landscape.tiles[index].tileRotation, 
						1.0f);

					vertexCount += 4;
				}

				//sides
				for (int k = 0; k < 4; ++k)
				{
					int adjacentHeight = landscape.GetHeightOfTile(index + columnOffsets[k], minHeight);

					if (adjacentHeight < height)
					{
						SetTileColumn(topCorners, columnNormals[k], ref vertexCount,
							vertices, normals, uvs, 
							columnIndices[k*2], 
							columnIndices[k*2 + 1],
							landscape.tiles[index], 
							height,
							adjacentHeight);
					}
				}
			}
		}


		//tris are just a series of quads, so do that now
		for (int i = 0, j = 0; i < indexCount; i += 6, j += 4)
		{
			triangles[i + 0] = j + 0;
			triangles[i + 1] = j + 2;
			triangles[i + 2] = j + 1;
			triangles[i + 3] = j + 2;
			triangles[i + 4] = j + 3;
			triangles[i + 5] = j + 1;
		}

		target.Clear();
		target.vertices = vertices;
		target.normals = normals;
		target.uv = uvs;
		target.triangles = triangles;

		target.RecalculateBounds();
	}

	void SetNormals(Vector3[] normals, int vertex, Vector3 dir)
	{
		for (int i = 0; i < 4; ++i)
			normals[vertex + i] = dir; 
	}

	bool ShouldAddTileToMeshType(bool fixedTerrainTilesOnly, int i, int j)
	{
		if (landscape.IsValidTile(i, j))
		{
			var flags = (int)(TileFlag.Buildable | TileFlag.HasPath_RuntimeAssigned);
			var isTerrain = (landscape.tiles[i + j*landscape.w].flags & flags) == 0;

			return fixedTerrainTilesOnly ? isTerrain : !isTerrain;
		}

		return false;
	}

	void SetTile(Vector2[] uvs, int uvIndex, int tileIndex, int rotationIndex, float percent)
	{
		var uvIncr = 1.0f/(landscape.tileSheet.width/landscape.tileSheetPixelsPerTile);
		int surfaceTileX = tileIndex % 16;
		int surfaceTileY = tileIndex / 16;

		texData[0].Set (surfaceTileX * uvIncr, 1.0f - surfaceTileY * uvIncr);
		texData[1].Set ((surfaceTileX + 1) * uvIncr, 1.0f - surfaceTileY * uvIncr);
		texData[2].Set (surfaceTileX * uvIncr, 1.0f - (surfaceTileY + percent) * uvIncr);
		texData[3].Set ((surfaceTileX + 1) * uvIncr, 1.0f - (surfaceTileY + percent) * uvIncr);

		for (int i = 0; i < 4; ++i)
			uvs[uvIndex + i] = texData[rotationUVs[rotationIndex][i]];
	}

	void AddTileColumnVertices(int height, int adjacentHeight, ref int vertexCount, ref int indexCount)
	{
		int tilesInColumn = (height - adjacentHeight)/slicesPerTile + 1;
		vertexCount += 4 * tilesInColumn;
		indexCount += 6 * tilesInColumn;
	}

	void SetTileColumn(Vector3[] topVertices, 
		Vector3 normal, 
		ref int vertexCount,
		Vector3[] vertices, 
		Vector3[] normals, 
		Vector2[] uvs, 
		int x, int y, 
		LandscapeTile tile, 
		int tileHeight,
		int adjacentHeight)
	{
		int tilesInColumn = (tile.height - adjacentHeight)/slicesPerTile + 1;

		//TODO: im not sure this is exactly correct. shouldnt need to bind this lookups
		int heightLookup0 = Mathf.Max(tileHeight - minHeight, 0);
		int heightLookup1 = Mathf.Max(heightLookup0 - slicesPerTile, 0);

		for (int i = 0; i < tilesInColumn; ++i)
		{
			//copy x/z values over. pull y from look-up table. in
			//theory this should cause everything to line up
			//without any seams between adjacent columns
			vertices[vertexCount + 0].Set(topVertices[x].x, ypos[heightLookup0], topVertices[x].z);
			vertices[vertexCount + 1].Set(topVertices[y].x, ypos[heightLookup0], topVertices[y].z);
			vertices[vertexCount + 2].Set(topVertices[x].x, ypos[heightLookup1], topVertices[x].z);
			vertices[vertexCount + 3].Set(topVertices[y].x, ypos[heightLookup1], topVertices[y].z);

			int tileIndex = (i == 0) ? tile.tileIndexDirtTop : tile.tileIndexDirt;

			if (tile.height != tileHeight) //if the passed in value is lower, then there is a tower on this tile
				tileIndex = tile.tileIndexDirt;

			SetTile (uvs, vertexCount, tileIndex, 0, 1.0f);
			SetNormals (normals, vertexCount, normal);

			vertexCount += 4;

			//TODO: see above
			heightLookup0 = Mathf.Max(heightLookup0 - slicesPerTile, 0);
			heightLookup1 = Mathf.Max(heightLookup1 - slicesPerTile, 0);
		}
	}
}
