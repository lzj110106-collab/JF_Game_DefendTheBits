using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpriteScrollingUV : MonoBehaviour {
	

	public SpriteRenderer target;
	public bool autoScroll;
	public Vector2 scrollValue;
	public Vector2 scrollRate;

	private Material mat;
	private Rect newUVRect;
	private Vector2 originalSize;

	void Awake()
	{
		originalSize = new Vector2(target.sprite.rect.width, target.sprite.rect.height);
		mat = target.material;
	}

	void FixedUpdate()
	{
		if(autoScroll)
		{
			scrollValue += scrollRate * Time.fixedDeltaTime;
		}
		mat.SetTextureOffset( "_MainTex", scrollValue);
	}
}
