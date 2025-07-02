using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogNewVersion : MonoBehaviour 
{
	bool isVisible = false;

	public void Trigger()
	{
		gameObject.SetActive(true);
		isVisible = true;
	}

	public void TriggerReadMore()
	{
        //2018 8 15  zbs 屏蔽版本更新提示界面
        //Hide();
        //UserInterface.GetNewVersionMoreDialog().Trigger();
    }

    public void Hide()
	{
		gameObject.SetActive(false);
		isVisible = false;
	}

	public bool IsVisible()
	{
		return isVisible;
	}
}
