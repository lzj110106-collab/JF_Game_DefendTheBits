using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace PigeonCoopToolkit.Effects.Trails.Editor
{
    [CustomEditor(typeof(SmokePlumeGravity))]
    [CanEditMultipleObjects]
    public class SmokePlumeGravityEditor : SmokePlumeEditor
    {
        protected override void DrawTrailSpecificGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityBias"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ConstantForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RandomForceScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TimeBetweenPoints"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxNumberOfPoints"));
        }
    }
}
