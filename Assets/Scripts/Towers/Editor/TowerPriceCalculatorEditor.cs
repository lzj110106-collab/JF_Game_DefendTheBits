using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TowerPriceCalculator))]
public class TowerPriceCalculatorEditor : Editor
{
	TowerPriceCalculator calculator;

	bool showPrices = false;
	Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

	void OnEnable()
	{
		calculator = (TowerPriceCalculator)target;
	}

	public override void OnInspectorGUI()
	{
		GUI.changed = false;

		DrawDefaultInspector();

		if (GUI.changed)
			calculator.Refresh();

		//check if the game is running
		if (TowerLoader.instance == null)
			return;

		var foldOutName = calculator.calculatesPR ? "Show PR" : "Show Prices";
		showPrices = EditorGUILayout.Foldout(showPrices, foldOutName);
		if (showPrices)
		{
			EditorGUI.indentLevel++;

			//it is, so render out all the prices
			foreach (var kv in TowerLoader.instance.towerInfo)
			{
				bool result = false;
				if (!foldouts.TryGetValue(kv.Key, out result))
					foldouts[kv.Key] = false;
					
				result = EditorGUILayout.Foldout(result, kv.Key);
				if (result)
				{
					EditorGUI.indentLevel++;

					for (var i = 0; i < kv.Value.Count; ++i)
					{
						int value = calculator.calculatesPR ? kv.Value[i].rating : kv.Value[i].price;
						EditorGUILayout.LabelField("level " + i + " = " + value);
					}

					EditorGUI.indentLevel--;
				}

				foldouts[kv.Key] = result;
			}

			EditorGUI.indentLevel--;
		}
	}
}
