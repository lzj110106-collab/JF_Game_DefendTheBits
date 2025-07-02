using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UnlockPanel : MonoBehaviour {
	public class UnlockInformation
	{
		public string UnlockName;
		public Sprite UnlockICN;

		public UnlockInformation(string name, Sprite icon)
		{
			UnlockName = name;
			UnlockICN = icon;
		}
	}

	public static UnlockPanel inst = null;

	[SerializeField] Text unlockNameText;
	[SerializeField] Image unlockIcon;
	[SerializeField] Button continueButton;
	static Stack<UnlockInformation> unlockStack = new Stack<UnlockInformation>();

	void Awake()
	{
		if (inst == null)
		{
			inst = this;
		}
	}

	public static void AddPanel(string name, Sprite icon)
	{
		UnlockInformation newPanel = new UnlockInformation (name, icon);
		unlockStack.Push (newPanel);
	}

	public static void ClearPanels()
	{
		unlockStack.Clear ();
	}

	public void Show()
	{
		transform.GetChild(0).gameObject.SetActive (true);
		UpdatePanelInfo ();
	}

	public void UpdatePanelInfo()
	{
		if (unlockStack.Count == 0)
		{
			transform.GetChild(0).gameObject.SetActive (false);
			return;
		}


		UnlockInformation updateInfo = unlockStack.Pop ();
		unlockNameText.text = updateInfo.UnlockName;
		unlockIcon.sprite = updateInfo.UnlockICN;
	}

}
