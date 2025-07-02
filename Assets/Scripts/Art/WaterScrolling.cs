using UnityEngine;

class WaterScrolling : MonoBehaviour
{
	public Vector2 scrollRate;
	private Vector2 scrollValue;
	private Material mat;

	void Start()
	{
		mat = MaterialCache.instance.waterMaterial;
	}

	void FixedUpdate()
	{
		scrollValue += scrollRate * Time.fixedDeltaTime;
		mat.SetVector("_Position", scrollValue);
	}
}
