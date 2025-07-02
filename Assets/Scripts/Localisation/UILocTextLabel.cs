using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UILocTextLabel : MonoBehaviour
{
	public string stringID;
	public bool allowScaling = true;

	string originalText;

	void Start()
	{
       
        //make sure the label is translated to the current 
        //language setting before it is displayed
        RefreshText();
	}

	public void RefreshText()
	{
		var label = GetComponent<Text>();
		if (label != null)
		{
			if (string.IsNullOrEmpty(originalText))
			{
				originalText = label.text;
				originalText = "### " + originalText + " ###";
			}

            //check if a translation is available first. if not,
            //leave the label as is. this is so that artists
            //dont have to add to the localisation document
            //immediately.
           
            if (LocManager.TranslationAvailable(stringID))
            {
                LocManager.Assign(label, stringID, allowScaling);
            }
		}
	}
}
