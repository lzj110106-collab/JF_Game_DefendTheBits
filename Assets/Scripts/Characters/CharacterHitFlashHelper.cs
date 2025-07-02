using UnityEngine;
using System.Collections;

public class CharacterHitFlashHelper : MonoBehaviour 
{
	public AnimationCurve curve;

	MeshRenderer[] renderers;
	Color color;

	float elapsed = 0;
	float duration;
	bool animating = false;
	
	void Start () 
	{
		duration = curve.keys[curve.length - 1].time;
		CacheRenderers();
	}

	public void UpdateTick()
	{
		if (animating)
		{
			elapsed += World.frameTime;
			float t = Mathf.Clamp01(elapsed/duration);
			float alpha = curve.Evaluate(t);

			if (renderers != null)
			{
				Color newColor = new Color(color.r, color.g, color.b, alpha);
				for (int i = 0; i < renderers.Length; ++i)
					if (renderers[i] != null)
						renderers[i].sharedMaterial.SetColor("_Color", newColor);
			}

			if (elapsed >= duration)
			{
				animating = false;
				elapsed = 0.0f;
			}
		}
	}

	public void Stop()
	{
		animating = false;
	}

	public void OnHit(Weapon weapon)
	{
		if (weapon == null || renderers == null)
			return;

		animating = true;
		color = weapon.flashColor;
		elapsed = 0;
	}

	public void RestoreRenderers()
	{
		if (renderers == null)
			return;
		
		for (int i = 0; i < renderers.Length; ++i)
			if (renderers[i] != null)
				renderers[i].sharedMaterial.SetColor("_Color", new Color(color.r, color.g, color.b, 0.0f));
	}

	public void CacheRenderers()
	{
		renderers = GetComponentsInChildren<MeshRenderer>(true);
		if (renderers == null)
			return;
		
		foreach (var renderer in renderers)
		{
			renderer.sharedMaterial = new Material(renderer.sharedMaterial);
			if(renderer.sharedMaterial.shader.name == "PlaySide/Unlit/Shadow/Diffuse")
			{
				renderer.sharedMaterial.shader = MaterialCache.instance.unlitFlashShader;
				renderer.sharedMaterial.SetColor ("_Color", new Color(1, 1, 1, 0));
			}
		}
	}
}
