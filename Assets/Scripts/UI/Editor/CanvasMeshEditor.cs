using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CanvasMesh))]
public class CanvasMeshEditor : Editor {


	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (GUILayout.Button("Refresh Mesh"))
		{
			((CanvasMesh)target).RefreshMesh();
		}
	}
}
