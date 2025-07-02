using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScollShrinkItem : MonoBehaviour {

	public Animator anim;
	[HideInInspector] public RectTransform rectTransform;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	public void SetScale(float scale)
	{
		if(gameObject.activeSelf)
			anim.SetFloat("Scale", scale);
	}
}
