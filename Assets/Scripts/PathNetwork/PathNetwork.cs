using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathNetwork : MonoBehaviour 
{
	static PathNetwork instance;

	public List<Animator> spawnAnimators;

	public List<PathNetworkNode> nodes { get; private set; }
	public List<PathNetworkNode> startNodes { get; private set; }

	public List<PathNetworkNode> endNodes { get; private set; }

	List<GameObject> pathIndicators;

	void Awake() { instance = this; }
	void OnDestroy() { instance = null; }

	public void Initialise()
	{
		nodes = new List<PathNetworkNode>(GetComponentsInChildren<PathNetworkNode>(true));
		startNodes = new List<PathNetworkNode>();
		endNodes = new List<PathNetworkNode>();

		foreach (var node in nodes)
		{
			if (node.startNode)
			{
				//cache all the start nodes
				startNodes.Add(node);
			}
			else if (node.destinationNode)
			{
				endNodes.Add(node);
			}
			else
			{
				//snap nodes that are not start nodes to the mesh. this is an ugly hack to get around
				//problems caused by the tiles under the elevators being much lower than the actual
				//path the enemy will follow, which causes them to render underground during the
				//first segment of the path following.
				node.transform.position = Landscape.SnapToMesh(node.transform.position);
			}

			//set up connection distances for all nodes
			foreach (var connection in node.connections)
			{
				connection.distance = Vector3.Magnitude(node.transform.position - connection.child.transform.position);
				connection.distanceToEndNode = float.MaxValue;
			}

			//disable all update and rendering of path nodes
			node.gameObject.SetActive(false);
		}

		//now calculate total path lengths to the destination. this will be useful for 
		//enemies who always take the fastest route, rather than randomly picking
		//a path connection when presented with 
		FloodFillDistances();

		AssignPathFlagsToLandscape();

		if (startNodes.Count == 0)
			Debug.LogError("[PathNetwork] no start nodes found");

		//generate a path indicator for each start node in the this network. they
		//will get turned on and off between waves so that the player knows
		//where enemies will come from next wave.
		pathIndicators = new List<GameObject>(startNodes.Count);
		for (var i = 0; i < startNodes.Count; ++i)
		{
			pathIndicators.Add(GameObject.Instantiate(MaterialCache.instance.pathIndicatorPrefab));
			pathIndicators[i].transform.SetParent(transform, false);

			//assign these directly as all the path nodes are disabled
			pathIndicators[i].transform.position = startNodes[i].transform.position;
			pathIndicators[i].transform.rotation = startNodes[i].transform.rotation;
			pathIndicators[i].SetActive(false);
		}
	}

	public static PathNetworkNode GetStartNode(int index)
	{
		if (index < instance.startNodes.Count)
			return instance.startNodes[index];

		//if the passed in index is broken some how, return the first path node.
		if (instance.startNodes.Count > 0)
			return instance.startNodes[0];

		return null;
	}

	public static Animator GetSpawnAnimator(int index)
	{
		if (index < instance.spawnAnimators.Count)
			return instance.spawnAnimators[index];

		return null;
	}

	void FloodFillDistances()
	{
		//could optimise this, but its start up code, so whatever

		while (true)
		{
			bool changesMade = false;

			foreach (var node in nodes)
			{
				foreach (var conn in node.connections)
				{
					if (conn.distanceToEndNode != float.MaxValue)
						continue;
					
					if (endNodes.Contains(conn.child))
					{
						//trivial case. this connects directly to the destination point of the map
						conn.distanceToEndNode = conn.distance;	
						changesMade = true;
					}
					else 
					{
						//otherwise search the connections at the other end, and figure out
						//which one has the shortest path to the end point.
						float closest = float.MaxValue;
						foreach (var other in conn.child.connections)
							if (other.distanceToEndNode != float.MaxValue && other.distanceToEndNode < closest)
								closest = other.distanceToEndNode;

						//if one was found, assign it.
						if (closest != float.MaxValue)
						{
							conn.distanceToEndNode = conn.distance + closest;
							changesMade = true;
						}
					}
				}	
			}

			//this floodfill step didnt change anything. we are done.
			if (!changesMade)
				break;
		}
	}

	void AssignPathFlagsToLandscape()
	{
		var landscape = Landscape.instance;

		//NB: this assumes there are no diagonal paths.
		var pos0 = Vector2.zero;
		var pos1 = Vector2.zero;

		foreach (var node in nodes)
		{
			landscape.GetTileIndexFromPosition(node.transform.position.x, node.transform.position.z, ref pos0);

			//marking start and end nodes 
			if (node.startNode) 		landscape.AddFlag((int)pos0.x, (int)pos0.y, TileFlag.PathStart_RuntimeAssigned);
			if (node.destinationNode)	landscape.AddFlag((int)pos0.x, (int)pos0.y, TileFlag.PathEnd_RuntimeAssigned);

			foreach (var connection in node.connections)
			{
				Landscape.instance.GetTileIndexFromPosition(connection.child.transform.position.x,
															connection.child.transform.position.z,
															ref pos1);

				//fill in the block with the path flag. safe to do this because
				//of the lack of diagonals, meaning this should be a 1xN rectangle

				//sort the values first. no idea on the orientation of the connection
				int x0 = (int)Mathf.Min(pos0.x, pos1.x);
				int x1 = (int)Mathf.Max(pos0.x, pos1.x);

				int y0 = (int)Mathf.Min(pos0.y, pos1.y);
				int y1 = (int)Mathf.Max(pos0.y, pos1.y);

				for (var j = y0; j <= y1; ++j)
					for (var i = x0; i <= x1; ++i)
						Landscape.instance.AddFlag(i, j, TileFlag.HasPath_RuntimeAssigned);
			}
		}

		//finally we need to assign slope flags to paths. this is because sloped
		//paths used different range display quads.
		for (int j = 0; j < landscape.h; ++j)
		{
			for (int i = 0; i < landscape.w; ++i)
			{
				int tileIndex = i + j*landscape.w;
				int pathHeight = landscape.tiles[tileIndex].height;

				//ignore non-paths
				if (!landscape.HasFlag(i, j, TileFlag.HasPath_RuntimeAssigned))
					continue;

				//ignore path starts (they are usually sunken compared to neigbours
				if (landscape.HasFlag(i, j, TileFlag.PathStart_RuntimeAssigned) ||
					landscape.HasFlag(i, j, TileFlag.PathEnd_RuntimeAssigned))
				{
					continue;
				}

				bool left = PathDiscontinuity(i - 1, j, pathHeight);
				bool right = PathDiscontinuity(i + 1, j, pathHeight);
				bool up = PathDiscontinuity(i, j + 1, pathHeight);
				bool down = PathDiscontinuity(i, j - 1, pathHeight);

				if (left)
				{
					if (right)
						landscape.AddFlag(i, j, TileFlag.PathBridgeH_RuntimeAssigned);
					else
						landscape.AddFlag(i, j, TileFlag.PathSlopeLeft_RuntimeAssigned);
				}
				else if (right)
				{
					landscape.AddFlag(i, j, TileFlag.PathSlopeRight_RuntimeAssigned);
				}

				if (up)
				{
					if (down)
						landscape.AddFlag(i, j, TileFlag.PathBridgeV_RuntimeAssigned);
					else
						landscape.AddFlag(i, j, TileFlag.PathSlopeUp_RuntimeAssigned);
				}
				else if (down)
				{
					landscape.AddFlag(i, j, TileFlag.PathSlopeDown_RuntimeAssigned);
				}
			}
		}

		Landscape.InitialisePathFloodFillIndices(endNodes);
	}

	bool PathDiscontinuity(int i, int j, int pathHeight)
	{
		var landscape = Landscape.instance;

		return landscape.tiles[i + j*landscape.w].height > pathHeight && 
			   landscape.HasFlag(i, j, TileFlag.HasPath_RuntimeAssigned);
	}

	public static void ShowPathIndicator(int pathIndex)
	{
//		Debug.Log("SHOW PATH INDICATOR: " + pathIndex);

		if (FTUE.IsActive())
			return;
		
		if (pathIndex < instance.pathIndicators.Count)
			instance.pathIndicators[pathIndex].SetActive(true);
	}

	public static void ClearPathIndicators()
	{
//		Debug.Log("CLEAR PATH INDICATORS");

		for (int i = 0; i < instance.pathIndicators.Count; ++i)
			instance.pathIndicators[i].SetActive(false);
	}
}
