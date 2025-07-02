using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowPath
{
	//these are interpolated values used by external observers
	public Vector3 position { get; private set; }
	public Vector3 lookAt { get; private set; }

	//need to track three path nodes in order to calculate
	//path offset values correctly without any position
	//snapping around corners
	PathNetworkNode node0;
	PathNetworkNode node1;
	PathNetworkNode node2;
	PathNetworkNode.Connection connection0;
	PathNetworkNode.Connection connection1;

	Vector3 pathPosition0;
	Vector3 pathPosition1;

	float progress;
	float pathOffset;
	float speedMultiplier; //used to prevent enemies overtaking each other around corners

	//determines what connections we can follow at each node intersection
	CharacterSize characterSize;
	bool isFlying = false;
	bool isFollowingPath = false;

	public enum FollowMethod { Random, Shortest };
	public FollowMethod followMethod = FollowMethod.Shortest;

	//create a block of data up front for connection searching so that
	//we dont allocate constantly for each object during its path following
	List<PathNetworkNode.Connection> availableConnections;

	void InitialiseBackingData()
	{
		//the game is grid based, so in theory no path node should have
		//more than 4 connections, so create enough space for
		//8 just in case there is some weird set-up somewhere that
		//introduces so diagonals (eg for flying enemies or something)
		if (availableConnections == null)
			availableConnections = new List<PathNetworkNode.Connection>(8);
	}

	public void Initialise(int startNodeIndex, CharacterSize size, bool flying, float offset)
	{
		InitialiseBackingData();

		characterSize = size;
		isFollowingPath = false;
		isFlying = flying;

		node0 = PathNetwork.GetStartNode(startNodeIndex);
		node1 = ChooseDestinationNode(node0, ref connection0);
		node2 = ChooseDestinationNode(node1, ref connection1);

		//node2 is optional. eg a level with a single path segment
		if (node0 != null && node1 != null)
		{
			pathOffset = offset;

			pathPosition0 = CalculateInitialPosition(node0, node1, pathOffset);
			pathPosition1 = CalculateDesinationPosition(node0, node1, node2, pathOffset);

			position = pathPosition0;
			lookAt = pathPosition1;

			isFollowingPath = true;
			progress = 0.0f;

			float baseDistance = Vector3.Magnitude(node1.transform.position - node0.transform.position);
			float offsetDistance = Vector3.Magnitude(pathPosition1 - pathPosition0);
			speedMultiplier = offsetDistance / baseDistance;
		}
	}
		
	public void Initialise(FollowPath sourcePath, float maxPathOffset)
	{
		InitialiseBackingData();

		//TODO: deal with randomising the path offset.

		position = sourcePath.position;
		lookAt = sourcePath.lookAt;

		node0 = sourcePath.node0;
		node1 = sourcePath.node1;
		node2 = sourcePath.node2;

		connection0 = sourcePath.connection0;
		connection1 = sourcePath.connection1;

		pathPosition0 = sourcePath.pathPosition0;
		pathPosition1 = sourcePath.pathPosition1;

		progress = sourcePath.progress; 
		pathOffset = sourcePath.pathOffset; //TODO: maybe want to randomise this...

		characterSize = sourcePath.characterSize;
		isFlying = sourcePath.isFlying;

		followMethod = sourcePath.followMethod;
		isFollowingPath = true;
	}

	//passing in the speed of the object every frame so that the external object
	//can deal with keeping track of pauses and status effects.
	//return true when the end of the path is reached
	public bool Update(float movementSpeed, float timeElapsed)
	{
		if (!isFollowingPath || !node0 || !node1)
		{
			isFollowingPath = false;
			return true;
		}

		float movementThisFrame = movementSpeed * timeElapsed * speedMultiplier;
		while (true)
		{
			var totalDistance = Vector3.Magnitude(pathPosition1 - pathPosition0);
			var currentDistance = totalDistance * progress;
			var remaining = totalDistance - currentDistance;

			if (movementThisFrame < remaining)
			{
				currentDistance += movementThisFrame;
				progress = currentDistance/totalDistance;

				position = Vector3.Lerp(pathPosition0, pathPosition1, progress);
				lookAt = position + Vector3.Normalize(pathPosition1 - pathPosition0);

				return false;
			}
			else
			{
				movementThisFrame -= remaining;
				progress = 0.0f;

				if (node2 == null)
				{
					//end of the line
					isFollowingPath = false;
					return true;
				}

				//shuffle the positioning along
				connection0 = connection1;

				node0 = node1;
				node1 = node2;
				node2 = ChooseDestinationNode(node1, ref connection1);

				pathPosition0 = pathPosition1;
				pathPosition1 = CalculateDesinationPosition(node0, node1, node2, pathOffset);

				//update move speed multiplier for the new segment
				float baseDistance = Vector3.Magnitude(node1.transform.position - node0.transform.position);
				float offsetDistance = Vector3.Magnitude(pathPosition1 - pathPosition0);
				speedMultiplier = offsetDistance / baseDistance;

				progress = 0;
			}
		}
	}

	static Vector3 CalculateInitialPosition(PathNetworkNode node0, PathNetworkNode node1, float offset)
	{
		var p0 = node0.transform.position;
		var p1 = node1.transform.position;

		var dir = Vector3.Normalize(p1 - p0);
		var perp = new Vector3(-dir.z, 0.0f, dir.x);

		return p0 + perp*offset;
	}

	static Vector3 CalculateDesinationPosition(PathNetworkNode node0, PathNetworkNode node1, PathNetworkNode node2, float offset)
	{
		var p0 = node0.transform.position;
		var p1 = node1.transform.position;

		var dir0 = Vector3.Normalize(p1 - p0);
		var perp0 = new Vector3(-dir0.z, 0.0f, dir0.x);

		if (node2 != null)
		{
			var p2 = node2.transform.position;
			var dir1 = Vector3.Normalize(p2 - p1);
			var perp1 = new Vector3(-dir1.z, 0.0f, dir1.x);

			//use the perpendiculars to generate two lines offset from 
			//the two path segments. where they intersect is the
			//correct destination point
			Vector3 result0, result1;
			if (MathUtil.LineLineIntersection(p0 + perp0 * offset, 
											  p1 + perp0 * offset, 
											  p1 + perp1 * offset, 
											  p2 + perp1 * offset,
											  out result0, 
											  out result1))
			{
				//lines are in the same plane, return either result
				return result0;
			}
		}

		//lines are either parallel, or there are only two nodes.
		//either way, just offset from the middle node
		return p1 + perp0*offset;
	}
		
	public float CalculateRemainingPathLength()
	{
		return connection0.distanceToEndNode - progress * connection0.distance;
	}

	PathNetworkNode ChooseDestinationNode(PathNetworkNode node, ref PathNetworkNode.Connection output)
	{
		//destination nodes dont have any outgoing connections, so skip this whole function
		if (node == null || node.destinationNode)
			return null;

		availableConnections.Clear();

		foreach (var connection in node.connections)
		{
			//skip connections that arent available for whatever reason (eg broken bridges)
			if ((connection.flags & PathNetworkNode.ConnectionFlags.IsActive) == 0)
				continue;

			if (CanTravelAlongConnection(connection.flags, characterSize, isFlying))
			{
				//TODO: should we just pick the first destination?
				availableConnections.Add(connection);
			}
		}

		if (availableConnections.Count == 0)
		{
			Debug.Log("[FollowPath] couldnt find destination node from " + node.name + " for size " + characterSize);
			return null;
		}

		//single choice, return it
		if (availableConnections.Count == 1)
		{
			output = availableConnections[0];
			return output.child;
		}

		switch (followMethod)
		{
		case FollowMethod.Random:
			{
				//randomly pick between the remaining available ones. Random.Range with ints is [incl, excl]
				int choice = Random.Range(0, availableConnections.Count);
				output = availableConnections[choice];
				return output.child;
			}
		
		case FollowMethod.Shortest:
			{
				var index = 0;
				var distance = availableConnections[0].distanceToEndNode;

				for (int i = 1; i < availableConnections.Count; ++i)
				{
					if (availableConnections[i].distanceToEndNode < distance)
					{
						distance = availableConnections[i].distanceToEndNode;
						index = i;
					}
				}

				output = availableConnections[index];
				return output.child;
			}
		}

		output = null;
		return null;
	}

	static bool CanTravelAlongConnection(PathNetworkNode.ConnectionFlags type, CharacterSize size, bool flying)
	{
		//cull away flying paths first. there may be paths that only allow large flying creatures, etc.
		if ((type & PathNetworkNode.ConnectionFlags.FlyingOnly) != 0 && !flying)
			return false;

		if ((type & PathNetworkNode.ConnectionFlags.AllowNormalEnemies) == 0)
			return size == CharacterSize.Normal;

		if ((type & PathNetworkNode.ConnectionFlags.AllowLargeEnemies) == 0)
			return size == CharacterSize.Normal || size == CharacterSize.Large;

		return size == CharacterSize.Boss;
	}

	public bool IsFollowingPath()
	{
		return isFollowingPath;
	}
}
