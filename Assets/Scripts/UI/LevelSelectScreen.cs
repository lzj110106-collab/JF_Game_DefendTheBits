using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class LevelSelectScreen : MonoBehaviour 
{
	public GameObject levelButtonPrefab;
	public GameObject levelButtonContainer;
	public GameObject easyAlert;
	public GameObject mediumAlert;
	public GameObject hardyAlert;

	[Header("UI")]
	public ScrollRect scrollRect;
	public Text difficultyText;
	public Color[] difficultyTextColor;
	public Sprite[] difficultyImages;
	public Image difficultyIcon;
	public ShrinkScrollEdgeItems shrinkScroll;

	public List<RectTransform> buttonTransforms { get; private set; }

	//allocate this once up front
	Vector3[] corners = new Vector3[4];

	public bool pageDifficultyLevels = true;

	void Awake()
	{
		if (!pageDifficultyLevels)
			CreateLevelButtons();
		
		SetDifficulty(0);
	}

	void Update()
	{
		if (!pageDifficultyLevels)
		{
			//figure out which level button is in the centre of the screen
			float distance = float.MaxValue;
			int closest = -1;

			for (int i = 0; i < buttonTransforms.Count; ++i)
			{
				buttonTransforms[i].GetWorldCorners(corners);

				var centre = 0.5f * (corners[0] + corners[2]);
				var screen = UserInterface.Camera2D().WorldToScreenPoint(centre);

				float dist = Mathf.Abs(Screen.width*0.5f - screen.x);
				if (dist < distance)
				{
					distance = dist;
					closest = i;
				}
			}
				
			if (closest != 0)
			{
				var data = buttonTransforms[closest].GetComponent<LevelSelectButton>().levelData;
				SetDifficultyUI(data.difficulty);
			}
		}
	}


	void CreateLevelButtons()
	{
		UnityUtil.DestroyAllChildren(levelButtonContainer);

		buttonTransforms = new List<RectTransform>();

		for (int i = 0; i < LevelDatabase.levelData.Count; ++i)
		{
			var levelData = LevelDatabase.levelData[i];
			if (levelData.IsFTUE())
				continue;
			
			var selectButton = GameObject.Instantiate(levelButtonPrefab);
			selectButton.name = levelData.identifier;
			selectButton.transform.SetParent(levelButtonContainer.transform, false);
			selectButton.GetComponent<LevelSelectButton>().SetLevelData(levelData);
            selectButton.GetComponent<LevelSelectButton>().index = i;


            buttonTransforms.Add(selectButton.GetComponent<RectTransform>());
		}
	}
		
	public void SetDifficulty(int difficulty)
	{
		if (pageDifficultyLevels)
		{
			UnityUtil.DestroyAllChildren(levelButtonContainer);
			buttonTransforms = new List<RectTransform>();

			for (int i = 0; i < LevelDatabase.levelData.Count; ++i)
			{
				var levelData = LevelDatabase.levelData[i];
				if (levelData.IsFTUE() || levelData.difficulty != difficulty)
					continue;

				var selectButton = GameObject.Instantiate(levelButtonPrefab);
				selectButton.name = levelData.identifier;
				selectButton.transform.SetParent(levelButtonContainer.transform, false);
				selectButton.GetComponent<LevelSelectButton>().SetLevelData(levelData);			

				buttonTransforms.Add(selectButton.GetComponent<RectTransform>());
			}

			scrollRect.horizontalNormalizedPosition = 0.0f;
			shrinkScroll.Initialize();

			SetDifficultyUI(difficulty);
		}
		else
		{
			int index = -1;

			float x0 = float.PositiveInfinity;
			float x1 = float.NegativeInfinity;
			float x2 = 0.0f;

			for (var i = 0; i < levelButtonContainer.transform.childCount; ++i)
			{
				var child = levelButtonContainer.transform.GetChild(i);
				var button = child.GetComponent<LevelSelectButton>();

				buttonTransforms[i].GetWorldCorners(corners);

				var p0 = UserInterface.Camera2D().WorldToScreenPoint(corners[0]);
				var p1 = UserInterface.Camera2D().WorldToScreenPoint(corners[2]);

				x0 = Mathf.Min(x0, p0.x);
				x1 = Mathf.Max(x1, p1.x);

				if (button.levelData.difficulty == difficulty && index == -1)
				{
					x2 = p0.x; //align using the left hand side of the panel
					index = i;
				}
			}
				
			scrollRect.horizontalNormalizedPosition = (x2 - x0)/(x1 - x0);
		}
	}

    void OnEnable()
    {
        //if (ObscuredPrefs.GetInt("LastSelectLevel", 0) == 0)
        //{
        //    var data = buttonTransforms[ObscuredPrefs.GetInt("LastSelectLevel", 0)].GetComponent<LevelSelectButton>().levelData;
        //    SetDifficulty(data.difficulty);
        //}
        //else if (ObscuredPrefs.GetInt("LastSelectLevel", 0) == 14)
        //{
        //    var data = buttonTransforms[ObscuredPrefs.GetInt("LastSelectLevel", 0) - 1].GetComponent<LevelSelectButton>().levelData;
        //    SetDifficulty(data.difficulty);
        //}
        //else
        //{
        //    var data = buttonTransforms[ObscuredPrefs.GetInt("LastSelectLevel", 0) + 1].GetComponent<LevelSelectButton>().levelData;
        //    SetDifficulty(data.difficulty);
        //}


    }

    public void SetDifficultyUI(int difficulty)
	{
		LocManager.Assign(difficultyText, "ui_diff_" + difficulty.ToString());
		difficultyText.color = difficultyTextColor[difficulty];	
		difficultyIcon.sprite = difficultyImages[difficulty];
	}
}
