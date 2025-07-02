using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LOGOLoading : MonoBehaviour
{
    public Animator logoanim_zh;
    public Animator logoanim_en;
    public void EnterGame()
    {
        AudioController.Play("UI_SelectCharUpgrade");
        AudioController.Play("Music_Menu");
        if (LocManager.isInChina())
        {
            logoanim_zh.enabled = true;
        }
        else
        {
           logoanim_en.enabled = true;
        }
        gameObject.SetActive(false);
    }

    public void StartGame()
    {
        //GameObject.Find("_ThirdParty").GetComponent<TimeManager>().GetDateTime();
    }
}
