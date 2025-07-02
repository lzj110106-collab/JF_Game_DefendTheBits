using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(PathNetworkNode))]
public class Editor_PathNetworkNode : Editor 
{
	PathNetworkNode targetNode;
	PathNetworkNode[] allNodes;

	bool showConnections = false;
	List<bool> foldOutStates;

	void OnEnable()
	{
		targetNode = (PathNetworkNode)target;
		if (targetNode.connections == null)
			targetNode.connections = new List<PathNetworkNode.Connection>();

		foldOutStates = new List<bool>();
		for (int i = 0; i < targetNode.connections.Count; ++i)
			foldOutStates.Add(false);

		//we want /all/ the nodes in one array. GetComponentsInSiblings will skip the targetNode.
		if (targetNode.transform.parent != null)
			allNodes = targetNode.transform.parent.GetComponentsInChildren<PathNetworkNode>();
		else
			allNodes = null;
	}

	public override void OnInspectorGUI()
	{
		GUI.changed = false;

		targetNode.startNode = EditorGUILayout.Toggle("Is Start Node", targetNode.startNode);
		targetNode.destinationNode = EditorGUILayout.Toggle("Is End Node", targetNode.destinationNode);

		showConnections = EditorGUILayout.Foldout(showConnections, "Connections");
		if (showConnections)
		{
			EditorGUI.indentLevel++;
			int newSize = EditorGUILayout.IntField("Size", targetNode.connections.Count);
			if (newSize == 0)
			{
				targetNode.connections.Clear();
				foldOutStates.Clear();
			}
			if (newSize > 0 && newSize != targetNode.connections.Count)
			{
				while (newSize > targetNode.connections.Count)
				{
					targetNode.connections.Add(new PathNetworkNode.Connection());
					foldOutStates.Add(false);
				}

				if (newSize < targetNode.connections.Count)
				{
					int last = targetNode.connections.Count;
					targetNode.connections.RemoveRange(newSize, last - newSize);
					foldOutStates.RemoveRange(newSize, last - newSize);
				}
			}

			for (int i = 0; i < foldOutStates.Count; ++i)
			{
				foldOutStates[i] = EditorGUILayout.Foldout(foldOutStates[i], "Connection");
				if (foldOutStates[i])
				{
					EditorGUI.indentLevel++;
					targetNode.connections[i].child = (PathNetworkNode)EditorGUILayout.ObjectField(targetNode.connections[i].child, typeof(PathNetworkNode), true, null);
					targetNode.connections[i].flags = (PathNetworkNode.ConnectionFlags)EditorGUILayout.EnumMaskPopup(new GUIContent("Flags"), targetNode.connections[i].flags, GUIStyle.none);
					EditorGUI.indentLevel--;
				}
			}

			EditorGUI.indentLevel--;
		}

		if (GUI.changed)
			EditorUtility.SetDirty(targetNode);
	}

	public void OnSceneGUI()
	{
		if (targetNode == null || targetNode.gameObject != Selection.activeObject)
			return;
		
		if (Event.current.commandName == "Delete" && targetNode != null)
		{
			Debug.Log("DELETE");

			if (allNodes != null)
				foreach (var node in allNodes)
					node.RemoveConnection(targetNode);

			targetNode = null;
			return;
		}

		DrawNetwork();

		if (Event.current.modifiers == (EventModifiers.Alt | EventModifiers.Shift))
		{
			//mouse picking to place another node in the same plane as the selected node
			var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			var ground = new Plane(Vector3.up, targetNode.transform.position);

			//first check for an intersection with any of the existing nodes, excluding ourself
			bool didFindExistingNode = false;
			if (allNodes != null)
			{
				foreach (var node in allNodes)
				{
					if (node == targetNode)
						continue;

					Vector3 intersectionPoint;
					float intersectionParameter;

					if (!MathUtil.ClosestPointOnLine(node.transform.position,
							mouseRay.GetPoint(0.0f), 
							mouseRay.GetPoint(1.0f),
							out intersectionPoint,
							out intersectionParameter))
						continue;
					
					if (Vector3.Magnitude(intersectionPoint - node.transform.position) > 0.5f)
						continue;
					
					//found a node. whether we interact with it is unimportant, we just
					//want to avoid the flow further down this function that attempts
					//to create new nodes
					didFindExistingNode = true;

					//dont double up on connections.
					bool invalid = targetNode.HasConnectionTo(node) ||
								   node.HasConnectionTo(targetNode);

					Handles.color = invalid ? Color.red : Color.green;
					Handles.DrawLine(targetNode.transform.position, node.transform.position);

					if (!invalid && Event.current.type == EventType.MouseDown && Event.current.button == 0)
					{
						//create a connection between these two nodes
						var connection = new PathNetworkNode.Connection();
						connection.child = node;
						targetNode.connections.Add(connection);
						break;
					}
				}
			}

			if (!didFindExistingNode)
			{
				float intersectionDistance;
				if (ground.Raycast(mouseRay, out intersectionDistance))
				{
					var poi = mouseRay.GetPoint(intersectionDistance);

					Handles.color = Color.magenta;
					Handles.DrawLine(targetNode.transform.position, poi);

					if (Event.current.type == EventType.MouseDown &&
						Event.current.button == 0)
					{
						//generate a new path network node
						var newNode = GameObject.Instantiate(targetNode.gameObject);
						newNode.name = newNode.name.Split('(')[0];
						newNode.transform.SetParent(targetNode.transform.parent, false);
						newNode.transform.position = poi;

						//connect it to this node
						var connection = new PathNetworkNode.Connection();
						connection.child = newNode.GetComponent<PathNetworkNode>();
						connection.child.connections = null; //dont duplicate connections
						targetNode.connections.Add(connection);

						//switch focus to the new node so we can place a whole bunch really quickly
						Selection.activeGameObject = newNode;
					}
				}
			}
		}

		//force repaint messages to be sent while the mouse is moving.
		SceneView.RepaintAll();
	}

	void DrawNetwork()
	{
		if (allNodes == null)
			return;

		foreach (var node in allNodes)
		{
			foreach (var connection in node.connections)
			{
				if (connection.child == null)
					continue;

				var p0 = node.transform.position;
				var p1 = connection.child.transform.position;
				var dir = Vector3.Normalize(p1 - p0);

				var capSize = 0.5f;
				var capPosition = p0 + dir * (Vector3.Magnitude(p1 - p0) - capSize);
				var capRotation = Quaternion.FromToRotation(Vector3.forward, dir);

				Handles.color = Color.grey;
				Handles.DrawLine(p0, capPosition);
				Handles.ConeHandleCap(0, capPosition, capRotation, capSize, EventType.Repaint);
			}
		}
	}
}
