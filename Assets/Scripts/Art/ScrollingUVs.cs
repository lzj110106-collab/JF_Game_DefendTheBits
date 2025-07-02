using UnityEngine;

class ScrollingUVs : MonoBehaviour
{
	public Renderer[] targetRenderers;
	public Vector2 speed = Vector2.zero;
	private Material thisMaterial;
	private Vector2 offset;

	void Awake()
	{
		if (targetRenderers[0] == null)
		{
			Debug.LogError("Could not find renderer for GameObject '"+name+"' (parent '"+transform.parent.name+"')");
			enabled = false;
		}
		else
		{
			thisMaterial = targetRenderers[0].material;
			offset = thisMaterial.mainTextureOffset;

			for(int i=0; i< targetRenderers.Length; i++)
			{
				if (targetRenderers[i] != null)
					targetRenderers[i].material = thisMaterial;
			}
		}
	}
	
	void Update()
	{
		offset += speed * Time.deltaTime;

		// Wrap into [0..1) range
		if (speed.x != 0.0f) { offset.x -= (int)(offset.x); }
		if (speed.y != 0.0f) { offset.y -= (int)(offset.y); }

		thisMaterial.mainTextureOffset = offset;
	}
}
