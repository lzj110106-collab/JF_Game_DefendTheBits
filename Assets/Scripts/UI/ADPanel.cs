using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ADPanel : MonoBehaviour {
    public HUD hud;

    public Text payCash_text;
    public Text tipe_text;

    //private static ADPanel instance;
    //public static ADPanel Instance
    //{

    //    get
    //    {
    //        return instance;
    //    }

    //}

    //private void Awake()
    //{
    //    instance = this;
    //}


    public void ShowADPanel()
    {
        gameObject.SetActive(true);
        //GetComponent<PanelNotifier>().TransitionAll();
        GetComponent<Animator>().SetTrigger("on");
    }

    public void HideADPanel()
    {
        GetComponent<Animator>().SetTrigger("off");
    }

    public void DontNeed()
    {
        GetComponent<Animator>().SetTrigger("off");
        hud.NoAD();
    }

    public void WacthADVideo()
    {
        //IronSource.Agent.showRewardedVideo();
        //hud.RewardAD();
    }

    //---------------------------------------------------------------------------

    public void ToAndroid_RestoreLife()
    {
        //PXY_AndroidBuy.Instance.Buy("com.east2west.defendthebits.RestoreLife", "1元获得20点生命", 1.00f, "");.
        Invoke("test", 0.1f);

    }

    void test()
    {
        if (SaveData.GetCash() >= 20)
        {
            hud.RewardAD();
            SaveData.AddCash(-20);
        }
        else
        {
            payCash_text.gameObject.SetActive(false);
            tipe_text.gameObject.SetActive(true);
        }
    }
}
