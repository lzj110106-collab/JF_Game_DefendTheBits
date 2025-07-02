using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathNetworkNode : MonoBehaviour 
{
	[System.Flags]
	public enum ConnectionFlags
	{
		AllowNormalEnemies 	= (1 << 0),
		AllowLargeEnemies	= (1 << 1),
		AllowBossEnemies	= (1 << 2),
		FlyingOnly			= (1 << 3),

		IsActive 			= (1 << 4)
	}

	[System.Serializable]
	public class Connection
	{
		public PathNetworkNode child;
		public ConnectionFlags flags = (ConnectionFlags.AllowNormalEnemies |
										ConnectionFlags.IsActive);

		public float distance = float.MaxValue;
		public float distanceToEndNode = float.MaxValue;
	};

	public List<Connection> connections;
	public bool startNode = false;
	public bool destinationNode = false;

	public bool HasConnectionTo(PathNetworkNode childNode)
	{
		if (childNode == null || connections == null)
			return false;
		
		foreach (var connection in connections)
			if (connection.child == childNode)
				return true;

		return false;
	}

	public void RemoveConnection(PathNetworkNode childNode)
	{
		foreach (var connection in connections)
		{
			if (connection.child == childNode)
			{
				connections.Remove(connection);
				return;
			}
		}
	}
}