using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodexHint : MonoBehaviour 
{
	public HintData hintData { get; private set; }

	public GameObject headerHint;
	public GameObject headerEnemy;

	public Text description;
	public Image icon;

	public void Initialise(HintData source)
	{
		hintData = source;

		headerHint.SetActive(hintData.hintType == HintTypes.Hint);
		headerEnemy.SetActive(hintData.hintType == HintTypes.NewEnemy);
        //print(hintData.headerLocID);
        //GetComponentInChildren<UILocTextLabel>().stringID = hintData.headerLocID;
        LocManager.Assign(description, hintData.headerLocID);
		icon.sprite = hintData.imageIcon;
	}

	public void ShowFullHintDialog()
	{
		HintsPanel.AddHint(hintData);
		HintsPanel.instance.Trigger(); //force trigger this immediately.
	}
}
