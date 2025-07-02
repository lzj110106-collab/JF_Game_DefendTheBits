using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor( typeof(Panel) )]
public class PanelEditor : Editor {

	private Panel panel;

	public void OnEnable()
	{
		panel = (Panel)target;
	}

	public override void OnInspectorGUI()
	{
		if(EditorApplication.isPlaying)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Transition ON"))
				panel.Transition(panel.defaultOnTransition);
			if (GUILayout.Button("Transition OFF"))
				panel.Transition(panel.defaultOffTransition);
			EditorGUILayout.EndHorizontal();
		}
		base.OnInspectorGUI();
	}
}
