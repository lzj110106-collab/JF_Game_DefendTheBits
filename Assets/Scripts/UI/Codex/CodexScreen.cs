using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodexScreen : MonoBehaviour
{
	[Header("Prefabs")]
	public GameObject hintPrefab;
	public GameObject hintScrollContent;

	public GameObject enemyHintPrefab;
	public GameObject enemyHintScrollContent;

	[Header("Enemy Info")]
	public Text txtEnemyName;
	public Text txtEnemyDescription;
	public Text txtEnemyHealth;
	public Text txtEnemySpeed;
	public Image icnEnemyPortrait;
	public GameObject weaknessParentNode;
	public Image[] weaknessImages;

	List<CodexEnemyHint> enemyHints = new List<CodexEnemyHint>();
	List<CodexHint> hints = new List<CodexHint>();

	void Start() 
	{
		UnityUtil.DestroyAllChildren(hintScrollContent);
		UnityUtil.DestroyAllChildren(enemyHintScrollContent);

		var ftueHints = HintsDatabase.AllFTUEHintsOrdered();
		for (var i = 0; i < ftueHints.Count; ++i)
		{
			if (ftueHints[i].hintType != HintTypes.NewEnemy) //this will show up in the enemy half of the codex instead
				AddHint(ftueHints[i]);
		}

		var levelHints = HintsDatabase.AllLevelHints();
		foreach (var kv in levelHints)
		{
			for (var i = 0; i < kv.Value.Count; ++i)
				AddHint(kv.Value[i]);
		}

		var codexHints = HintsDatabase.CodexHints();
		for (var i = 0; i < codexHints.Count; ++i)
		{
			var prefabInstance = GameObject.Instantiate(enemyHintPrefab);
			prefabInstance.name = codexHints[i].identifier;
			prefabInstance.transform.SetParent(enemyHintScrollContent.transform, false);

			var codexHint = prefabInstance.GetComponent<CodexEnemyHint>();

			float scaleOverride = (((float)i/(float)codexHints.Count)+0.6f)*0.6f;

			codexHint.Initialise(this, codexHints[i], scaleOverride);
			enemyHints.Add(codexHint);
		}
			
		//last minute changes. add all these hints to the normal hints section
		AddHint(HintsDatabase.GetEnemyHintData("enemy_8"));
		AddHint(HintsDatabase.GetEnemyHintData("enemy_9"));
		AddHint(HintsDatabase.GetEnemyHintData("enemy_10"));
		AddHint(HintsDatabase.GetEnemyHintData("enemy_11"));
		AddHint(HintsDatabase.GetEnemyHintData("enemy_13"));
		AddHint(HintsDatabase.GetEnemyHintData("boss_wayno"));
		AddHint(HintsDatabase.GetEnemyHintData("boss_dragon"));
		AddHint(HintsDatabase.GetEnemyHintData("boss_goo"));
		AddHint(HintsDatabase.GetEnemyHintData("boss_ice"));
		AddHint(HintsDatabase.GetEnemyHintData("boss_ogre"));
		AddHint(HintsDatabase.GetEnemyHintData("boss_pult"));
		AddHint(HintsDatabase.GetEnemyHintData("boss_samurai"));

		RefreshEnemyCodexVisibility();
	}

	void OnEnable()
	{
        RefreshEnemyCodexVisibility();
		SaveData.ClearCodexAlert();
	}
    private void Update()
    {
#if PXY_YUNCE
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("测试退出按钮");
            //Application.Quit();
            PXY_AndroidBuy.Instance.ExitGame();
            //ExitGame.Instance.SetSelfTrue();
        }
#endif
    }

    void RefreshEnemyCodexVisibility()
	{
		for (var i = 0; i < enemyHints.Count; ++i)
		{
			//check save data to see if we have seen this enemy type yet. if not, hide the codex entry
			var enemyIdentifier = enemyHints[i].hint.identifier.Replace("_codex", "");
			enemyHints[i].gameObject.SetActive(!SaveData.ShowEnemyHint(enemyIdentifier));
		}

		//point the default codex entry at the lowest level enemy
		if (enemyHints.Count > 0)
			enemyHints[0].OnButtonPressed();


		//need to do this on the codex side of things as well now
		for (var i = 0; i < hints.Count; ++i)
		{
			if (hints[i].hintData.hintType == HintTypes.NewEnemy)
			{
				hints[i].gameObject.SetActive(!SaveData.ShowEnemyHint(hints[i].hintData.identifier));
			}
		}
	}

	public void OnEnemySelected(CodexEnemyHint enemyHint)
	{
		var enemyIdentifier = enemyHint.hint.identifier.Replace("_codex", "");
		var enemyData = MasterEnemyTable.Get(enemyIdentifier);

		LocManager.Assign(txtEnemyName, enemyHint.hint.headerLocID);
		LocManager.Assign(txtEnemyDescription, enemyHint.hint.textDot1LocID);
		txtEnemyHealth.text = enemyData.health.ToString();
		txtEnemySpeed.text = enemyData.speedMultiplier.ToString();
		icnEnemyPortrait.sprite = enemyHint.hint.imgPortrait;

		SetWeaknesses(enemyHint.hint.weaknesses);

		AudioController.Play ("UI_Select");
	}

	void SetWeaknesses(string[] _weaknessIDs)
	{
		weaknessParentNode.SetActive(false);

		for (int i = 0; i < weaknessImages.Length; ++i)
		{
			if (_weaknessIDs == null || i >= _weaknessIDs.Length || string.IsNullOrEmpty(_weaknessIDs[i]))
			{
				weaknessImages[i].gameObject.SetActive(false);
			}
			else if (TowerIconDatabase.IsValidIcon(_weaknessIDs[i]) && i < weaknessImages.Length - 1)
			{
				weaknessImages[i].sprite = TowerIconDatabase.GetIcon(_weaknessIDs[i]);
//				weaknessImages[i].GetComponentInChildren<Text>(true).text = TowerIconDatabase.GetName(_weaknessIDs[i]);
				weaknessImages[i].gameObject.SetActive(true);

				weaknessParentNode.SetActive(true);
			}
		}
	}

	void AddHint(HintData hintData)
	{
		if (hintData != null)
		{
			var prefabInstance = GameObject.Instantiate(hintPrefab);
			prefabInstance.name = hintData.identifier;
			prefabInstance.transform.SetParent(hintScrollContent.transform, false);
            prefabInstance.transform.GetChild(0).GetChild(0).GetChild(3).GetComponent<UILocTextLabel>().stringID = hintData.headerLocID;
            var hint = prefabInstance.GetComponent<CodexHint>();

			hint.Initialise(hintData);

			hints.Add(hint);
		}
	}
}
