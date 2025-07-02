using UnityEditor;
using UnityEngine;
using System.Collections;

public class EditorHelpers
{
	static public void OnInspectorGUI(TowerDefensePFXContainer pfx, bool show)
	{
		if (pfx.entries == null)
			return;
		
		if (show)
		{
			EditorGUI.indentLevel++;
			
			int newSize = EditorGUILayout.IntField("Size", pfx.entries.Count);
			while (newSize < pfx.entries.Count)
				pfx.entries.RemoveAt(pfx.entries.Count - 1);
			
			while (newSize > pfx.entries.Count)
				pfx.entries.Add (new PFXEntry());
			
			EditorGUI.indentLevel++;
			for (int i = 0; i < pfx.entries.Count; ++i)
			{
				var newEntry = new PFXEntry();
				newEntry.type = (PFX)EditorGUILayout.EnumPopup("Type", pfx.entries[i].type);
				newEntry.prefab = (GameObject)EditorGUILayout.ObjectField("Particle System",  
					pfx.entries[i].prefab, 
					typeof(GameObject),
					false,
					null);
				
				pfx.entries[i] = newEntry;
			}
			EditorGUI.indentLevel--;
			
			EditorGUI.indentLevel--;
		}
	}
}
