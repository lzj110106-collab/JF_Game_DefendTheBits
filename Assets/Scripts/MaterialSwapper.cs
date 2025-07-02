using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwapper
{
	List<MeshRenderer> meshRenderers;
	List<SkinnedMeshRenderer> meshRenderersSkinned;

	Material[] originalMaterials;
	Material[] originalMaterialsSkinned;

	bool hasSwappedMaterials = false;

	public void CacheMaterials(GameObject target)
	{
		//restore the previous set of materials before we attempt to
		//cache all the new ones for this object
		RestoreMaterials();

		//we dont want to perform material swapping on all meshes,
		//so mask those out first before caching anything
		{
			var allMeshRenderers = target.GetComponentsInChildren<MeshRenderer>(true);
			var allMeshRenderersSkinned = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);

			meshRenderers = new List<MeshRenderer>(allMeshRenderers.Length);
			meshRenderersSkinned = new List<SkinnedMeshRenderer>(allMeshRenderersSkinned.Length);

			for (int i = 0; i < allMeshRenderers.Length; ++i)
				if (ShouldCacheRenderer(allMeshRenderers[i].name))
					meshRenderers.Add(allMeshRenderers[i]);

			for (int i = 0; i < allMeshRenderersSkinned.Length; ++i)
				if (ShouldCacheRenderer(allMeshRenderersSkinned[i].name))
					meshRenderersSkinned.Add(allMeshRenderersSkinned[i]);
		}

		originalMaterials = new Material[meshRenderers.Count];
		originalMaterialsSkinned = new Material[meshRenderersSkinned.Count];

		for (int i = 0; i < meshRenderers.Count; ++i)
			originalMaterials[i] = meshRenderers[i].sharedMaterial;
			
		for (int i = 0; i < meshRenderersSkinned.Count; ++i)
			originalMaterialsSkinned[i] = meshRenderersSkinned[i].sharedMaterial;
	}

	public void SetMaterial(Material material)
	{
		RestoreMaterials();

		if (meshRenderers != null)
		{
			for (int i = 0; i < meshRenderers.Count; ++i)
				meshRenderers[i].sharedMaterial = material;

			for (int i = 0; i < meshRenderersSkinned.Count; ++i)
				meshRenderersSkinned[i].sharedMaterial = material;

			hasSwappedMaterials = true;
		}
	}

	public void SetShader(Shader shader)
	{
		RestoreMaterials();

		if (meshRenderers != null)
		{
			for (int i = 0; i < meshRenderers.Count; ++i)
			{
				meshRenderers[i].sharedMaterial = new Material(meshRenderers[i].sharedMaterial);
				meshRenderers[i].sharedMaterial.shader = shader;
			}

			for (int i = 0; i < meshRenderersSkinned.Count; ++i)
			{
				meshRenderersSkinned[i].sharedMaterial = new Material(meshRenderersSkinned[i].sharedMaterial);
				meshRenderersSkinned[i].sharedMaterial.shader = shader;
			}

			hasSwappedMaterials = true;
		}
	}

	public void RestoreMaterials()
	{
		if (meshRenderers != null && hasSwappedMaterials)
		{
			for (int i = 0; i < meshRenderers.Count; ++i)
			{
//				GameObject.DestroyImmediate(meshRenderers[i].sharedMaterial); //prevent mem leak
				meshRenderers[i].sharedMaterial = originalMaterials[i];
			}

			for (int i = 0; i < meshRenderersSkinned.Count; ++i)
			{
//				GameObject.DestroyImmediate(meshRenderersSkinned[i].sharedMaterial); //as above
				meshRenderersSkinned[i].sharedMaterial = originalMaterialsSkinned[i];
			}

			hasSwappedMaterials = false;
		}
	}

	public void SetMaterialColour(string shaderAttribute, Color colour)
	{
		if (meshRenderers != null)
		{
			for (int i = 0; i < meshRenderers.Count; ++i)
				if (meshRenderers[i].sharedMaterial != null)
					meshRenderers[i].sharedMaterial.SetColor(shaderAttribute, colour);

			for (int i = 0; i < meshRenderersSkinned.Count; ++i)
				if (meshRenderersSkinned[i].sharedMaterial != null)
					meshRenderersSkinned[i].sharedMaterial.SetColor(shaderAttribute, colour);
		}
	}

	static bool ShouldCacheRenderer(string rendererName)
	{
		if (rendererName.ToLower().Contains("shadow"))
			return false;

		return true;
	}
}
