using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(GenericPrefabPool))]
public class GenericPrefabPoolEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		var targetPool = (GenericPrefabPool)target;

		GUILayoutOption[] options = null;
		if (GUILayout.Button("Refresh Prefab List", options))
		{
			for (int i = 0; i < targetPool.folders.Count; ++i)
			{
				var folder = targetPool.folders[i];
				folder.prefabs.Clear();
				folder.resourceLocations.Clear();

				FindPrefabs(folder, folder.name);
			}
		}

		//print the referenced prefabs out in the inspector so people can see whats being pooled
		if (targetPool.folders != null)
		{
			EditorGUILayout.Separator();

			for (int i = 0; i < targetPool.folders.Count; ++i)
			{
				EditorGUILayout.LabelField(targetPool.folders[i].name);
				EditorGUI.indentLevel++;

				foreach (var prefab in targetPool.folders[i].prefabs)
					EditorGUILayout.LabelField(prefab.name);

				foreach (var location in targetPool.folders[i].resourceLocations)
					EditorGUILayout.LabelField("*** " + location);

				EditorGUI.indentLevel--;
			}
		}

		EditorUtility.SetDirty(target);
	}

	static void FindPrefabs(GenericPrefabPool.PrefabFolder folder, string path)
	{
		string[] files = Directory.GetFiles(Application.dataPath + "/" + path);
		string[] subdirs = Directory.GetDirectories(Application.dataPath + "/" + path);

		var dataPath = Application.dataPath + "/Resources/";

		if (folder.name.Contains("Resources/"))
		{
			foreach (var file in files)
			{
				if (!file.Contains(".prefab") || file.Contains(".meta"))
					continue;

				var assetPath = file.Remove(0, dataPath.Length);
				var prefabName = assetPath.Substring(0, assetPath.LastIndexOf('.'));

				folder.resourceLocations.Add(prefabName);
			}
		}
		else
		{
			foreach (var file in files)
			{
				if (!file.Contains(".prefab") || file.Contains(".meta"))
					continue;

				var assetPath = "Assets" + file.Remove(0, Application.dataPath.Length);
				var prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

				if (prefab != null)
					folder.prefabs.Add(prefab);		
			}
		}

		//recurse
		foreach (var subdir in subdirs)
			FindPrefabs(folder, path + "/" + subdir.Substring(subdir.LastIndexOf('/') + 1));
	}
}
