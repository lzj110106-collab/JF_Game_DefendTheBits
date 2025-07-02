using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UILocSpriteSwapper : MonoBehaviour 
{
	[System.Serializable]
	public class SpriteData
	{
		public LocManager.Language language;
		public string spriteResourceName;
	}

	public List<SpriteData> spriteData;

	void Start()
	{
		//make sure the label is translated to the current 
		//language setting before it is displayed
		Refresh();
	}

	public void Refresh()
	{
		var image = GetComponent<Image>();
		if (image != null)
			RefreshInternal(image, LocManager.CurrentLanguage());
	}

	void RefreshInternal(Image image, LocManager.Language language)
	{
		foreach (var data in spriteData)
		{
			if (data.language == language)
			{
				image.overrideSprite = LocManager.LoadSprite(data.spriteResourceName);
				return;
			}
		}

		//couldnt find an image for this language, attempt to load the default sprite, which will be the english one.
		if (language != LocManager.Language.English)
			RefreshInternal(image, LocManager.Language.English);
	}
}
