using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour 
{
	Canvas canvas;
	Character character;

	public GameObject visuals;
	public Image fillImage;

	public void Initialise(Canvas canvas, Character character)
	{
		this.canvas = canvas;
		this.character = character;

		Update (); //set position immediately
	}

	public void Update()
	{
		if(character!=null)
		{
			transform.localPosition = canvas.WorldToCanvas(character.transform.position);
			fillImage.fillAmount = Mathf.Clamp01(character.currentHealth/character.baseHealth);
			visuals.gameObject.SetActive(character.currentHealth < character.baseHealth);
		}
	}
}
