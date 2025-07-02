using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIScrollingUV : MonoBehaviour {
	

	public RawImage targetImage;
	public bool autoScroll;
	public Vector2 scrollValue;
	public Vector2 scrollRate;

	private Material targetMaterial;
	private Rect newUVRect;
	private Vector2 originalSize;

	void Awake()
	{
		originalSize = new Vector2(targetImage.uvRect.width, targetImage.uvRect.height);
	}

	void FixedUpdate()
	{
		if(autoScroll)
		{
			scrollValue = scrollRate * Time.fixedDeltaTime;
		}

		newUVRect = new Rect(targetImage.uvRect.x + scrollValue.x, targetImage.uvRect.y + scrollValue.y, originalSize.x, originalSize.y);
		targetImage.uvRect = newUVRect;
	}
}
