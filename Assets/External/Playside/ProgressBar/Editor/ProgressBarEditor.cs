using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

[CustomEditor( typeof(ProgressBar) )]
public class ProgressBarEditor : Editor {

	private ProgressBar progressBar;

	public void OnEnable()
	{
		progressBar = (ProgressBar)target;
	}

   public override void OnInspectorGUI()
    {
    	progressBar.startFull = EditorGUILayout.Toggle("Start Full", progressBar.startFull);
        progressBar.fillContainer = EditorGUILayout.ObjectField("Fill Container", (RectTransform)progressBar.fillContainer, typeof(RectTransform), true) as RectTransform;
        progressBar.progressLabel = EditorGUILayout.ObjectField("Progress Label", (Text)progressBar.progressLabel, typeof(Text), true) as Text;

        // Fill Type
        progressBar.fillType = (ProgressBar.FillType) EditorGUILayout.EnumPopup("Fill Type", progressBar.fillType);
        progressBar.fillRate = EditorGUILayout.FloatField("Fill Rate", progressBar.fillRate);

        // Transition Type
        progressBar.transition = (ProgressBar.Transition) EditorGUILayout.EnumPopup("Transition", progressBar.transition);
        EditorGUI.indentLevel = 1;

        switch (progressBar.transition)
        {
        	// NONE
        	case ProgressBar.Transition.None:
        	break;

        	// TINT
        	case ProgressBar.Transition.Tint:
        		progressBar.targetGraphic = EditorGUILayout.ObjectField("Target Graphic", (Graphic)progressBar.targetGraphic, typeof(Graphic), true) as Graphic;
                progressBar.flashTime = EditorGUILayout.FloatField("Flash Time", progressBar.flashTime);
        		progressBar.normalColor = 	EditorGUILayout.ColorField("Normal Color", progressBar.normalColor);
				progressBar.addColor = 		EditorGUILayout.ColorField("Add Color", progressBar.addColor);
				progressBar.removeColor = 	EditorGUILayout.ColorField("Remove Color", progressBar.removeColor);
				progressBar.filledColor = 	EditorGUILayout.ColorField("Filled Color", progressBar.filledColor);
        	break;

        	// ANIMATION
        	case ProgressBar.Transition.Animation:
        		progressBar.normalTrigger = 	EditorGUILayout.TextField("Normal Trigger", progressBar.normalTrigger);
	        	progressBar.addTrigger = 		EditorGUILayout.TextField("Add Trigger", progressBar.addTrigger);
	        	progressBar.removeTrigger = 	EditorGUILayout.TextField("Remove Trigger", progressBar.removeTrigger);
	        	progressBar.filledTrigger = 	EditorGUILayout.TextField("Filled Trigger", progressBar.filledTrigger);
                progressBar.emptyTrigger =     EditorGUILayout.TextField("Empty Trigger", progressBar.emptyTrigger);
        	break;
        }
        EditorGUI.indentLevel = 0;

        // Label Type
        progressBar.labelType = (ProgressBar.LabelType) EditorGUILayout.EnumPopup("Label Type", progressBar.labelType);
        EditorGUI.indentLevel = 1;

        switch (progressBar.labelType)
        {
            // NONE
            case ProgressBar.LabelType.None:
            break;

            // Whole
            case ProgressBar.LabelType.Whole:
                progressBar.valueOfTotal = EditorGUILayout.Toggle("Out of Total", progressBar.valueOfTotal);
            break;

            // Decimal
            case ProgressBar.LabelType.Decimal:
                progressBar.valueOfTotal = EditorGUILayout.Toggle("Out of Total", progressBar.valueOfTotal);
            break;
        }
        EditorGUI.indentLevel = 0;

        // Save changes
        if (GUI.changed)
            EditorUtility.SetDirty (progressBar);
    }
}
