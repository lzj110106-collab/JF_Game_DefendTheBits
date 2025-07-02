using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

using UnityEditorInternal;

using System.Reflection;


[CustomEditor(typeof( PS_SortingLayer))]
public class PS_SortingLayerEditor : Editor
{
	private  PS_SortingLayer  sortingLayer;

	public int popupMenuIndex;
	public int orderInLayer;

	string[] sortingLayerNames;

    private void OnEnable()
    {
		sortingLayerNames = GetSortingLayerNames();
    	sortingLayer = (PS_SortingLayer)target;

    	Renderer renderer = sortingLayer.gameObject.GetComponent<Renderer>();
		if (!renderer)
			return;

		for (int i = 0; i < sortingLayerNames.Length; i++) 
		{
			if ( sortingLayerNames [i] == renderer.sortingLayerName)
				popupMenuIndex = i;
		}
		orderInLayer = renderer.sortingOrder; 
    }
	
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

		Renderer renderer = sortingLayer.gameObject.GetComponent<Renderer>();
		if (!renderer)
			return;


		popupMenuIndex = EditorGUILayout.Popup("Sorting Layer", popupMenuIndex, sortingLayerNames);//The popup menu is displayed simple as that
		if (sortingLayerNames[popupMenuIndex] != renderer.sortingLayerName) {
			Undo.RecordObject(renderer, "Edit Sorting Layer Name");
			renderer.sortingLayerName = sortingLayerNames[popupMenuIndex];
			EditorUtility.SetDirty(renderer);
		}

		int newSortingLayerOrder = orderInLayer;
		newSortingLayerOrder = EditorGUILayout.IntField("Sorting Layer Order", renderer.sortingOrder);
		if (newSortingLayerOrder != renderer.sortingOrder) {
			Undo.RecordObject(renderer, "Edit Sorting Order");
			renderer.sortingOrder = newSortingLayerOrder;
			EditorUtility.SetDirty(renderer);
		}

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
    }


	public string[] GetSortingLayerNames()
	{
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null, new object[0]);
	}
}
