using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum TileFlag
{
	None 			= 0,
//	Path 			= (1 << 0),
//	PathStart 		= (1 << 1),
//	PathEnd 		= (1 << 2),
	Buildable 		= (1 << 3),

	MeteorLocation = (1 << 4),

	//TODO: these should really be moved into their own set of flags,
	//but i really dont want to blow the current serialisation up

	HasPath_RuntimeAssigned = (1 << 16),
	HasLoweredPath_RuntimeAssigned = (1 << 17),

	PathStart_RuntimeAssigned = (1 << 18),
	PathEnd_RuntimeAssigned = (1 << 19),

	PathSlopeLeft_RuntimeAssigned = (1 << 20),
	PathSlopeRight_RuntimeAssigned = (1 << 21),
	PathSlopeUp_RuntimeAssigned = (1 << 22),
	PathSlopeDown_RuntimeAssigned = (1 << 23),
	PathBridgeH_RuntimeAssigned = (1 << 24),
	PathBridgeV_RuntimeAssigned = (1 << 25),

	BuildingExists_RuntimeAssigned  = (1 << 26),
	BuildingWillBePlaced_RuntimeAssigned = (1 << 27),
}

[System.Serializable]
public struct LandscapeTile
{
	public int height;
	public int flags;

	public int tileIndexSurface;
	public int tileIndexDirtTop;
	public int tileIndexDirt;
	public int tileRotation;

	Vector3 worldPosition;
}

[System.Serializable, ExecuteInEditMode]
public class Landscape : MonoBehaviour
{
	public static Landscape instance;

	public float tileWidth = 1.0f;
	public float tileHeight = 0.25f;
	public int towerDepth = 2;
	
	public Texture2D tileSheet;
	public int tileSheetPixelsPerTile = 16;
	public int paintIndexSurface = 0;
	public int paintIndexDirtTop = 3;
	public int paintIndexDirt = 2;

	[HideInInspector] public int x = 0;
	[HideInInspector] public int y = 0;
	[HideInInspector] public int w = 1;
	[HideInInspector] public int h = 1;
	[HideInInspector] public int extraDepth = 2;

	[HideInInspector] public LandscapeTile[] tiles;
	[HideInInspector] public int[] pathFloodFillIndices; //runtime

	public Mesh[] meshes { get; private set; }

	LandscapeMeshBuilder meshBuilder;

	//avoid allocation by declaring this upfront for use in tile searches
	const int SEARCH_AREA = 3;
	Vector3[] searchArray = new Vector3[(2 * SEARCH_AREA + 1) * (2 * SEARCH_AREA + 1)];
	int searchArrayUsed;

	Vector3 debugLandscapePicking;

	void Awake()
	{
		instance = this;
	}

	void OnDestroy()
	{
		instance = null;
	}
		
	//ensure that the mesh is generated correctly at runtime.
	void Start()
	{
		Initialise();
	}

	//this will trigger when the landscape script is first added to the scene
	void OnEnable()
	{
		//Initialise();
	}

	public void Initialise()
	{
		//generate mesh data if it doesnt exist yet
		if (meshes == null)
		{
			meshes = new Mesh[2] { new Mesh(), new Mesh() };
			meshes[0].name ="Terrain";
			meshes[1].name ="Terrain_Towers";

			meshBuilder = new LandscapeMeshBuilder(this);
		}

		var filters = GetComponentsInChildren<MeshFilter>(true);

		//generate missing child meshes
		for (int i = filters.Length; i < 2; ++i)
		{
			var child = new GameObject();
			child.name = i == 0 ? "terrain_mesh" : "terrain_mesh_towers";
			child.transform.SetParent(transform, false);
			child.AddComponent<MeshFilter>();
		}

		//assign meshes
		filters = GetComponentsInChildren<MeshFilter>(true);
		for (int i = 0; i < 2; ++i)
		{
            try
            {
                filters[i].mesh = meshes[i];
            }
            catch(Exception e)
            {

            }
			

			if (filters[i].GetComponent<MeshRenderer>() == null)
			{
				filters[i].gameObject.AddComponent<MeshRenderer>();
			}
		}

		//generate tile data
		if (tiles == null || tiles.Length != w*h)
		{
			tiles = new LandscapeTile[w*h];
			InitialiseTiles(ref tiles);
		}

		//and finally the mesh itself
		RebuildMesh();
	}

	public bool IsValidTile(int tileX, int tileY)
	{
		return tileX >= 0 && tileX < w && tileY >= 0 && tileY < h;
	}

	public Vector3 GetTileCentre(int tileX, int tileY)
	{
		if (IsValidTile(tileX, tileY))
		{
			//TODO: could precalc this easily enough. 

			//NB: this is actually wrong. doesnt take into account lowered ground,
			//but there are too many things relying on it now...
			return new Vector3((x + tileX + 0.5f)*tileWidth, 
		    	               tiles[tileX + tileY*w].height*tileHeight,
		        	           (y + tileY + 0.5f)*tileWidth);
		}

		return Vector3.zero;
	}

	public Vector3 GetAdjustedTileCentre(int tileX, int tileY)
	{
		if (IsValidTile(tileX, tileY))
		{
			var result = GetTileCentre(tileX, tileY);

			//for bridging tiles, alter the height so that it matches
			//one of the neighbouring path tiles. 
			{
				if (HasFlag(tileX, tileY, TileFlag.PathBridgeH_RuntimeAssigned))
					result.y = GetTileCentre(tileX + 1, tileY).y;

				if (HasFlag(tileX, tileY, TileFlag.PathBridgeV_RuntimeAssigned))
					result.y = GetTileCentre(tileX, tileY + 1).y;
			}

			return result;
		}

		return Vector3.zero;
	}

	public void AddFlag(int tileX, int tileY, TileFlag flag)
	{
		if (IsValidTile(tileX, tileY))
			tiles[tileX + tileY*w].flags |= (int)flag;
	}

	public void RemoveFlag(int tileX, int tileY, TileFlag flag)
	{
		if (IsValidTile(tileX, tileY))
			tiles[tileX + tileY*w].flags &= ~((int)flag);
	}

	public bool HasFlag(int tileX, int tileY, TileFlag flag)
	{
		if (IsValidTile (tileX, tileY))
		{
			if (flag == TileFlag.None)
				return tiles [tileX + tileY * w].flags == 0;
			else
				return (tiles [tileX + tileY * w].flags & (int)flag) != 0;
		}

		return false;
	}

	public void PaintTile(int tileX, int tileY)
	{
		var index = tileX + tileY*w;
		tiles[index].tileIndexSurface = paintIndexSurface;
		tiles[index].tileIndexDirtTop = paintIndexDirtTop;
		tiles[index].tileIndexDirt = paintIndexDirt;
	}

	public void RotateTile(int tileX, int tileY)
	{
		var index = tileX + tileY*w;
		tiles[index].tileRotation = (tiles[index].tileRotation + 1) % 4;

		RebuildMesh();
	}
	
	public bool PickTile(Ray r, ref int tileX, ref int tileY, TileFlag flag = TileFlag.None)
	{
		var start = new Vector3(x * tileWidth, 0, y * tileWidth);
		var pos = start;

		var closest = float.MaxValue;
		var plane = new Plane(Vector3.up, 0.0f);

		for (int j = 0; j < h; ++j)
		{
			for (int i = 0; i < w; ++i)
			{
				//if we are searching for a particular flag, skip tiles that dont have it.
				if (flag == TileFlag.None || HasFlag(i, j, flag))
				{
					plane.distance = -(tiles[i + j*w].height * tileHeight);

					var t = float.MaxValue;

					if (plane.Raycast(r, out t) && t < closest)
					{
						var poi = r.GetPoint(t);
						if (poi.x >= pos.x && poi.x <= pos.x + tileWidth &&
						    poi.z >= pos.z && poi.z <= pos.z + tileWidth)
						{
							tileX = i;
							tileY = j;
							closest = t;

							debugLandscapePicking = poi;
						}
					}
				}

				pos.x += tileWidth;
			}

			pos.x = start.x;
			pos.z += tileWidth;
		}

		return closest < float.MaxValue;
	}

	public bool GetTileIndexFromPosition(float xPos, float yPos, ref int tileX, ref int tileY)
	{
		float TilePosX = (xPos / tileWidth) - x - 0.5f;
		float TilePosY = (yPos / tileWidth) - y - 0.5f;

		if (IsValidTile(Mathf.RoundToInt(TilePosX), Mathf.RoundToInt(TilePosY)))
		{
			tileX = Mathf.RoundToInt (TilePosX);
			tileY = Mathf.RoundToInt (TilePosY);
			return true;
		}

		return false;
	}

	public bool GetTileIndexFromPosition(float xPos, float yPos, ref Vector2 tilePos)
	{
		float TilePosX = (xPos / tileWidth) - x - 0.5f;
		float TilePosY = (yPos / tileWidth) - y - 0.5f;

		if (IsValidTile(Mathf.RoundToInt(TilePosX), Mathf.RoundToInt(TilePosY)))
		{
			tilePos.x = Mathf.Round(TilePosX);
			tilePos.y = Mathf.Round(TilePosY);
			return true;
		}

		return false;
	}

	public static bool FindFirstTileOfType(TileFlag type, out int tileX, out int tileY)
	{
		for (int j = 0; j < instance.h; ++j)
		{
			for (int i = 0; i < instance.w; ++i)
			{
				if ((instance.tiles[i + j*instance.w].flags & (int)type) != 0)
				{
					tileX = i;
					tileY = j;

					return true;
				}
			}
		}

		tileX = 0;
		tileY = 0;

		return false;
	}

	public static bool FindRandomTileOfType(Vector3 pos, TileFlag type, ref Vector3 result, bool screenSpaceOnly = false, int searchArea = SEARCH_AREA)
	{
		return instance._FindRandomTileOfType(pos, type, ref result, screenSpaceOnly, searchArea);
	}

	bool _FindRandomTileOfType(Vector3 pos, TileFlag type, ref Vector3 result, bool screenSpaceOnly, int searchArea)
	{
		int tileX = 0;
		int tileY = 0;

		GetTileIndexFromPosition (pos.x, pos.z, ref tileX, ref tileY);

		//make sure we dont overflow the prealloced helper array
		searchArea = Mathf.Min(searchArea, SEARCH_AREA);
		searchArrayUsed = 0;

		for (int j = -searchArea; j <= searchArea; ++j)
		{
			for (int i = -searchArea; i <= searchArea; ++i)
			{
				int x = tileX + i;
				int y = tileY + j;

				if (instance.HasFlag(x, y, type))
				{
					var tilePosition = GetTileCentre(x, y);
					if (!screenSpaceOnly || MainCameraController.IsWorldPointVisibleInScreenSpace(tilePosition))
					{
						searchArray[searchArrayUsed] = tilePosition;
						searchArrayUsed += 1;
					}
				}
			}
		}

		if (searchArrayUsed == 0)
			return false;

		result = searchArray[UnityEngine.Random.Range(0, searchArrayUsed)];

		return true;
	}

	//NB: if weapons ever use this, its a good idea to change this to a dedicated
	//randomg number generator so that the same results can be produced across
	//multiple playthroughs of the same level
	public static Vector3 FindRandomTile(Vector3 pos, bool screenSpaceOnly = false, int searchArea = SEARCH_AREA)
	{
		return instance._FindRandomTile(pos, screenSpaceOnly, searchArea);
	}

	Vector3 _FindRandomTile(Vector3 pos, bool screenSpaceOnly, int searchArea)
	{
		int tileX = 0;
		int tileY = 0;

		GetTileIndexFromPosition (pos.x, pos.z, ref tileX, ref tileY);

		for (int i = 0; i < 128; ++i)
		{
			var resultX = tileX + UnityEngine.Random.Range(-searchArea, searchArea + 1); //inclusive, exclusive
			var resultY = tileY + UnityEngine.Random.Range(-searchArea, searchArea + 1);

			if (IsValidTile(resultX, resultY))
			{
				var tileCentre = GetTileCentre(resultX, resultY);
				if (!screenSpaceOnly || MainCameraController.IsWorldPointVisibleInScreenSpace(tileCentre))
					return tileCentre;
			}
		}

		//couldnt find a position...
		return pos;
	}

	//TODO: sort this shit out.
	public void RebuildMesh()
	{
		if (meshBuilder == null)
			meshBuilder = new LandscapeMeshBuilder(this);
		
		if (meshes != null && meshes.Length >= 2)
		{
			meshBuilder.RefreshVertexPositionArrays();
			meshBuilder.RebuildMesh(meshes[0], true);
			meshBuilder.RebuildMesh(meshes[1], false);
		}
	}

	public void RebuildTowerMesh()
	{
		if (meshBuilder == null)
			meshBuilder = new LandscapeMeshBuilder(this);
		
		if (meshes != null && meshes.Length >= 2)
		{
			meshBuilder.RefreshVertexPositionArrays();
			meshBuilder.RebuildMesh(meshes[1], false);
		}
	}

	public void OnDrawGizmos()
	{
		if (tiles == null)
			return;

		var start = new Vector3(x * tileWidth, 0, y * tileWidth);

		for (int j = 0; j < h; ++j)
		{
			for (int i = 0; i < w; ++i)
			{
				float height = tiles[i + j*w].height * tileHeight;
				var corners = new Vector3[4] {
					new Vector3(start.x + i*tileWidth, height, start.z + j*tileWidth),
					new Vector3(start.x + (i + 1)*tileWidth, height, start.z + j*tileWidth),
					new Vector3(start.x + (i + 1)*tileWidth, height, start.z + (j + 1)*tileWidth),
					new Vector3(start.x + i*tileWidth, height, start.z + (j + 1)*tileWidth)
				};

				var shrunkCorners = new Vector3[4] {
					new Vector3(start.x + (i + 0.25f)*tileWidth, height, start.z + (j + 0.25f)*tileWidth),
					new Vector3(start.x + (i + 0.75f)*tileWidth, height, start.z + (j + 0.25f)*tileWidth),
					new Vector3(start.x + (i + 0.75f)*tileWidth, height, start.z + (j + 0.75f)*tileWidth),
					new Vector3(start.x + (i + 0.25f)*tileWidth, height, start.z + (j + 0.75f)*tileWidth)
				};

				Gizmos.color = Color.grey;
				GizmoHelper(corners, false);

				if (HasFlag(i, j, TileFlag.Buildable))
				{
					Gizmos.color = Color.green;
					GizmoHelper(shrunkCorners, false);
				}
//				else if (HasFlag (i, j, TileFlag.PathStart))
//				{
//					Gizmos.color = Color.red;
//					Gizmos.DrawLine (shrunkCorners[0], shrunkCorners[2]);
//					Gizmos.DrawLine (shrunkCorners[1], shrunkCorners[3]);
//				}
//				else if (HasFlag (i, j, TileFlag.PathEnd))
//				{
//					Gizmos.color = Color.black;
//					Gizmos.DrawLine (shrunkCorners[0], shrunkCorners[2]);
//					Gizmos.DrawLine (shrunkCorners[1], shrunkCorners[3]);
//				}
				else if (HasFlag(i, j, TileFlag.PathSlopeLeft_RuntimeAssigned) ||
						 HasFlag(i, j, TileFlag.PathSlopeRight_RuntimeAssigned) ||
						 HasFlag(i, j, TileFlag.PathSlopeUp_RuntimeAssigned) ||
						 HasFlag(i, j, TileFlag.PathSlopeDown_RuntimeAssigned))
				{
					Gizmos.color = Color.black;
					GizmoHelper(shrunkCorners, true);
				}
				else if (HasFlag(i, j, TileFlag.PathBridgeH_RuntimeAssigned) ||
						 HasFlag(i, j, TileFlag.PathBridgeV_RuntimeAssigned))
				{
					Gizmos.color = Color.magenta;
					GizmoHelper(shrunkCorners, true);
				}

				if (HasFlag(i, j, TileFlag.MeteorLocation))
				{
					Gizmos.color = Color.white;
					GizmoHelper(shrunkCorners, false);
				}
			}
		}

		Gizmos.color = Color.red;
		Gizmos.DrawLine(debugLandscapePicking, debugLandscapePicking + Vector3.up);
	}

	void GizmoHelper(Vector3[] corners, bool drawCross)
	{
		Gizmos.DrawLine(corners[0], corners[1]);
		Gizmos.DrawLine(corners[1], corners[2]);
		Gizmos.DrawLine(corners[2], corners[3]);
		Gizmos.DrawLine(corners[3], corners[0]);

		if (drawCross)
		{
			Gizmos.DrawLine(corners[0], corners[2]);
			Gizmos.DrawLine(corners[1], corners[3]);
		}
	}

//	void OnGUI()
//	{
////		DebugFloodFillIndices();
//		DebugHeights();
//	}

	public void InitialiseTiles(ref LandscapeTile[] tiles)
	{
		for (int i = 0; i < tiles.Length; ++i)
		{
			tiles[i].height = 0;
			tiles[i].flags = 0;

			tiles[i].tileIndexSurface = paintIndexSurface;
			tiles[i].tileIndexDirtTop = paintIndexDirtTop;
			tiles[i].tileIndexDirt = paintIndexDirt;
		}
	}

	public int GetHeightOfTile(int i, int j, int lowestHeight)
	{
		return GetHeightOfTile(i + j*w, lowestHeight);
	}

	public int GetHeightOfTile(int index, int lowestHeight)
	{
		if (index >= 0 && index < w*h)
		{
			var flags = TileFlag.BuildingExists_RuntimeAssigned | 
						TileFlag.BuildingWillBePlaced_RuntimeAssigned |
						TileFlag.HasLoweredPath_RuntimeAssigned;
			
			if ((tiles[index].flags & (int)flags) != 0)
				return tiles[index].height - towerDepth;
		
			return tiles[index].height;
		}
	
		return lowestHeight;
	}

	public static Vector3 SnapToMesh(Vector3 position, float yOffset = 0.0f) 
	{ 
		return instance._SnapToMesh(position, yOffset); 
	}

	Vector3 _SnapToMesh(Vector3 position, float yOffset = 0.0f)
	{
		var start = new Vector3(x * tileWidth, 0.0f, y * tileWidth);
		var i = (int)((position.x - start.x)/tileWidth);
		var j = (int)((position.z - start.z)/tileWidth);

		return new Vector3(position.x, GetTileCentre(i, j).y + yOffset, position.z);
	}
		
#region PATH INDICES


	public static void InitialisePathFloodFillIndices(List<PathNetworkNode> endNodes)
	{
		int[] indices = new int[instance.w * instance.h];
		instance.pathFloodFillIndices = indices;

		//clear the map with invalid indices
		for (int i = 0; i < indices.Length; ++i)
			indices[i] = -1;

		foreach (var node in endNodes)
		{
			int tileX = -1;
			int tileY = -1;
			instance.GetTileIndexFromPosition(node.transform.position.x, node.transform.position.z, ref tileX, ref tileY);

			//this is a once at start-up function, so just call out to a recursive thing
			//to fill the map, rather than writing a more complicated, faster stack based thing.
			if (tileX != -1 && tileY != -1)
			{
				FloodFill(indices, tileX, tileY, 0);
			}
		}
	}

	static void FloodFill(int[] result, int tileX, int tileY, int distance)
	{
		int index = tileX + tileY * instance.w;

		//if we have already visited this tile, and the distance already stored
		//there is less than the current floodfill distance, then kill the recursion here
		if (result[index] != -1 && result[index] < distance)
			return;

		result[index] = distance;

		//now do the cardinal directions
		if (tileX > 0 && instance.HasFlag(tileX - 1, tileY, TileFlag.HasPath_RuntimeAssigned))
			FloodFill(result, tileX - 1, tileY, distance + 1);

		if (tileX < instance.w - 1 && instance.HasFlag(tileX + 1, tileY, TileFlag.HasPath_RuntimeAssigned))
			FloodFill(result, tileX + 1, tileY, distance + 1);

		if (tileY > 0 && instance.HasFlag(tileX, tileY - 1, TileFlag.HasPath_RuntimeAssigned))
			FloodFill(result, tileX, tileY - 1, distance + 1);

		if (tileY < instance.h - 1 && instance.HasFlag(tileX, tileY + 1, TileFlag.HasPath_RuntimeAssigned))
			FloodFill(result, tileX , tileY + 1, distance + 1);
	}

	public static int GetFloodFillIndex(int tileX, int tileY)
	{
		int index = tileX + tileY*instance.w;
		if (index < instance.pathFloodFillIndices.Length)
			return instance.pathFloodFillIndices[index];

		return -1;
	}

	void DebugFloodFillIndices()
	{
		if (pathFloodFillIndices == null || pathFloodFillIndices.Length < w*h)
			return;

		for (int j = 0; j < h; ++j)
		{
			for (int i = 0; i < w; ++i)
			{
				int floodFillIndex = pathFloodFillIndices[i + j*w];
				if (floodFillIndex == -1)
					continue;
				
				var worldPos = GetTileCentre(i, j);
				var screenPos = Camera.main.WorldToScreenPoint(worldPos);
				var toDisplay = floodFillIndex.ToString();

				Vector2 size = GUI.skin.label.CalcSize(new GUIContent(toDisplay));
				GUI.Label(new Rect(screenPos.x - size.x*0.5f, Screen.height - screenPos.y - size.y*0.5f, size.x, size.y), toDisplay);
			}
		}
	}

#endregion

	void DebugHeights()
	{
		for (int j = 0; j < h; ++j)
		{
			for (int i = 0; i < w; ++i)
			{
				var worldPos = GetTileCentre(i, j);
				var screenPos = Camera.main.WorldToScreenPoint(worldPos);
				var toDisplay = tiles[i + j*w].height.ToString();

				Vector2 size = GUI.skin.label.CalcSize(new GUIContent(toDisplay));
				GUI.Label(new Rect(screenPos.x - size.x*0.5f, Screen.height - screenPos.y - size.y*0.5f, size.x, size.y), toDisplay);
			}
		}
	}
}
