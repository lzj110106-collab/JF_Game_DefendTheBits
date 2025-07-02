using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WatchVideo : MonoBehaviour
{


    public GameObject tokenShader_pxy;
    public ParticleSystem tokenPS;
    public string[] tokenID7;
    public GameObject menu;


    public void closetokenShader_pxy()
    {
    
        SaveData.SetDalyTime_1(System.DateTime.Now.ToString());
        TimeManager.claimed_1 = true;
        TimeManager.totalSeconds_1 = 4 * 3600;
        TimeManager.gotTime_1 = true;
        tokenShader_pxy.SetActive(false);
        menu.SetActive(true);
    }
    public void testGetToken()       //4小时获得一次随机奖励
    {
        if (TimeManager.claimed_1 || Application.internetReachability == NetworkReachability.NotReachable)
            return;
        menu.SetActive(false);
        tokenShader_pxy.SetActive(true);
        tokenPS.Play();
        //GetComponent<PlayAudio>().PlayClip("UI_EOR_Star");

        for (int i = 0; i < 6; i++)
        {
            tokenShader_pxy.transform.GetChild(4).GetChild(i).gameObject.SetActive(false);
        }
        Debug.Log("tokenShader_pxy  ");
        int index = 0;
        string trinketID = "";


        //tokenShader_pxy.transform.GetChild(2).GetComponent<Text>().text = LocManager.Translate("ui_opensmallbox");
        index = Random.Range(0, 7);
        Debug.Log("index : " + index);
        trinketID = tokenID7[index];
        Debug.Log("trinketID : " + trinketID);

        tokenShader_pxy.transform.GetChild(4).GetChild(1).gameObject.SetActive(true);
        StartCoroutine(PlayParticleSystem(tokenShader_pxy.transform.GetChild(4).GetChild(1).GetChild(2).GetComponent<ParticleSystem>()));
        tokenShader_pxy.transform.GetChild(4).GetChild(1).GetChild(0).GetComponent<Image>().sprite = TrinketDatabase.GetIcon(trinketID);
        SaveData.AddTrinket(trinketID, 4);

    }

    IEnumerator PlayParticleSystem(ParticleSystem ps)
    {
        yield return new WaitForSeconds(0.0f);

        ps.Play();
    }
}
