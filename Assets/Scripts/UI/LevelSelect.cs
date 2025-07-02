using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;

public class LevelSelect : MonoBehaviour 
{

    public Transform content;

	void OnEnable()
	{
		CurrencyDisplay.ShowStarDisplay();
        if (ObscuredPrefs.GetInt("LastSelectLevel", 0) == 0)
        {
            content.localPosition = new Vector3(-450 * ObscuredPrefs.GetInt("LastSelectLevel", 0), content.localPosition.y, content.localPosition.z);
        }else if(ObscuredPrefs.GetInt("LastSelectLevel", 0) == 14)
        {
            content.localPosition = new Vector3(-450 * (ObscuredPrefs.GetInt("LastSelectLevel", 0) + 1), content.localPosition.y, content.localPosition.z);
        }
        else
        {
            content.localPosition = new Vector3(-450 * (ObscuredPrefs.GetInt("LastSelectLevel", 0)-1), content.localPosition.y, content.localPosition.z);
        }
       if(ObscuredPrefs.GetInt("LastSelectLevel")<=2)
        GetComponent<LevelSelectScreen>().SetDifficultyUI(0);
       else if(ObscuredPrefs.GetInt("LastSelectLevel") <= 8)
            GetComponent<LevelSelectScreen>().SetDifficultyUI(1);
       else
            GetComponent<LevelSelectScreen>().SetDifficultyUI(2);
    }

	public void Show()
	{
		GetComponent<Panel>().Transition ("PulseIn");
	}

	public void Hide()
	{
		GetComponent<Panel>().Transition("PulseOut");
	}
}
