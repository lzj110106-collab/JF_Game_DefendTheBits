using UnityEngine;
using System.Collections;

public class MaterialCache : MonoBehaviour 
{
	public static MaterialCache instance;

	public Color towerValidPlacementHighlightColour = Color.green;
	public Color towerInvalidPlacementHighlightColour = Color.red;
	public Material waterMaterial;
	public Material disabledMeshMaterial;

	public Shader unlitFlashShader;

	public Material selectedTowerMaterial;

	[Header("HACK")]
	public GameObject pathIndicatorPrefab;

	void Awake()
	{
		instance = this;
		waterMaterial = new Material(waterMaterial);
	}

	void OnDestroy()
	{
		instance = null;
	}
}
