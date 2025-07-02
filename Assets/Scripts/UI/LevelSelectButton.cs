using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class LevelSelectButton : MonoBehaviour 
{
	public LevelData levelData { get; private set; }

	public Sprite[] panelBackgrounds;
	public Color[] portraitEdgeColors;
	public Color panelDisabledColor;
	public Color titleDisabledColor;

	public Image panelImage;
	public Text displayText;
	public Image mapPortrait;
	public Image portraitEdgeImage;

	public Slider progressSlider;
	public GameObject starContainer;
	public GameObject[] stars;

	public GameObject lockHierarchy;
	public GameObject trinketHierarchy;
	public Image imgTrinketIcon;
	public Text txtTrinketDropChance;


	public Text lockedStarRequirement;

    public int index;

	public void SetLevelData(LevelData data)
	{
		levelData = data;
		Refresh ();
	}

	void OnEnable()
	{
		Refresh ();
	}

	void Refresh()
	{
		if (levelData != null)
		{
			if (levelData.trinketIds.Length > 0)
			{
				trinketHierarchy.SetActive(true);
				imgTrinketIcon.sprite = TrinketDatabase.GetIcon(levelData.trinketIds[0]);

				var chance = Mathf.RoundToInt(levelData.trinketChance[0] * 100.0f);
				LocManager.Assign(txtTrinketDropChance, "ui_drop_chance", chance);
			}
			else
				trinketHierarchy.SetActive(false);

			int rating = SaveData.StarRating(levelData);
			for (int i = 0; i < stars.Length; ++i)
				stars[i].SetActive(rating > i);

            int totalStars = SaveData.TotalStarCount();
            //int totalStars = 45;
            if (totalStars < levelData.unlockRequirement)
			{
				GetComponent<Button> ().interactable = false;
				mapPortrait.color = new Color(0.3f, 0.3f, 0.3f, 1f);
				lockHierarchy.SetActive(true);
				starContainer.SetActive(false);

				displayText.color = titleDisabledColor;
				panelImage.sprite = panelBackgrounds[0];
				panelImage.color = panelDisabledColor;
				portraitEdgeImage.color = portraitEdgeColors[0];

				progressSlider.value = (float)totalStars / (float)levelData.unlockRequirement;
				lockedStarRequirement.text = totalStars.ToString() + "/" + levelData.unlockRequirement.ToString();
			}
			else
			{
				GetComponent<Button> ().interactable = true;
				mapPortrait.color = Color.white;
				displayText.color =  Color.white;
				lockHierarchy.SetActive(false);
				starContainer.SetActive(true);

				//need to +1 because entry 0 is for the inactive state
				panelImage.sprite = panelBackgrounds[levelData.difficulty + 1];
				panelImage.color = Color.white;
				portraitEdgeImage.color = portraitEdgeColors[levelData.difficulty + 1];
			}

			LocManager.Assign(displayText, levelData.identifier);
			mapPortrait.sprite = levelData.prefab.portraitImage;

		}
	}

	public void OnLevelSelected()
	{
        ObscuredPrefs.SetInt("LastSelectLevel", index);
        LevelDetails.instance.SetLevelData(levelData);
		PanelManager.Instance.SwitchToScreen(PanelID.LevelSelect, PanelID.LevelDetails);
	}
}
