using UnityEngine;
using System.Collections;

public class SwitchSpriteLayer : MonoBehaviour {

	public string startLayer;
	public string startRenderLayer;
	public Renderer[] renderers;
	private bool initialized;

	void Awake ()
	{
		if(!initialized)
		{
			SetLayer(startLayer);
			SetRenderLayer(startRenderLayer);
		}
	}

	public void SetLayer(string layerName="Foreground")
	{
		initialized = true;
		foreach(Renderer s in renderers)
		{
			s.sortingLayerName = layerName;
		}
	}

	public void SetRenderLayer(string layerName="Default")
	{
		initialized = true;
		foreach(Renderer s in renderers)
		{
			s.gameObject.layer = LayerMask.NameToLayer(layerName);
		}
	}
}
