using UnityEngine;
using System.Collections;
using System;

public class LocNotifier : MonoBehaviour
{
	public GameObject uiRootNode;

	void Start()
	{
		//make sure we start in the correct font
		if (uiRootNode != null)
			LocManager.RefreshFonts(uiRootNode);
	}

	public void ChangeToNextLanguage() { ChangeLanguage(1); }
	public void ChangeToPreviousLanguage() { ChangeLanguage(-1); }

	void ChangeLanguage(int direction)
	{

		int languageCount = Enum.GetValues(typeof(LocManager.Language)).Length;

		int currentLanguage = (int)LocManager.CurrentLanguage();
		int newLanguage = (currentLanguage + languageCount + direction) % languageCount;

		//Debug.Log("Changing language to " + ((LocManager.Language)newLanguage).ToString());

		bool didSwapFonts = LocManager.SetLanguage((LocManager.Language)newLanguage);

        if (uiRootNode)
        {
            //swap all normal text fonts over first. fonts will need to be set up so
            //that the text scaling code works correctly. 
            if (didSwapFonts)
                LocManager.RefreshFonts(uiRootNode);

            var textLabels = uiRootNode.GetComponentsInChildren<UILocTextLabel>(true);
            foreach (var textLabel in textLabels)
                textLabel.RefreshText();

            var spriteSwappers = uiRootNode.GetComponentsInChildren<UILocSpriteSwapper>(true);
            foreach (var spriteSwapper in spriteSwappers)
                spriteSwapper.Refresh();

            //do a resource unload now to dump any localised sprites that just got swapped out
            //this now should also unload any unreferenced fonts in theory
            UnityUtil.UnloadUnusedAssets();
        }
    }
}
