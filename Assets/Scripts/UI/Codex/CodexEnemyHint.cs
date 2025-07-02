using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodexEnemyHint : MonoBehaviour 
{
	public CodexScreen codex { get; private set; }
	public HintData hint { get; private set; }

	public Image icon;

	public void Initialise(CodexScreen codexScreen, HintData hintData, float scaleOverride)
	{
		codex = codexScreen;
		hint = hintData;

		icon.sprite = hintData.imageIcon;
		icon.rectTransform.localScale = Vector3.one * scaleOverride;
	}

	public void OnButtonPressed()
	{
		codex.OnEnemySelected(this);
	}
}
