using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UILocTextLabel))]
public class UILocTextLabelEditor : Editor
{
	UILocTextLabel textLabel;

	void OnEnable()
	{
		textLabel = (UILocTextLabel)target;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
	}
}
