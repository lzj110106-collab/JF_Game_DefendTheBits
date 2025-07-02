using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Material))]
class CustomMaterialEditor : UnityEditor.MaterialEditor
{
	private Shader m_shader;

	public override void OnInspectorGUI()
	{
		if (!isVisible)
			return;

		var material = target as Material;
		int renderQueue = EditorGUILayout.IntField("Render Queue", material.renderQueue);

		base.OnInspectorGUI();

		if (m_shader != null && m_shader != material.shader)
		{
			material.renderQueue = material.shader.renderQueue;
			EditorUtility.SetDirty(material);
		}
		else if (renderQueue != material.renderQueue)
		{
			material.renderQueue = renderQueue;
			EditorUtility.SetDirty(material);
		}

		m_shader = material.shader;
	}
}
