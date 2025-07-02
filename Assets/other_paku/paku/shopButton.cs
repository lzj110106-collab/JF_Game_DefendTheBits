using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shopButton : MonoBehaviour
{


    public GameObject Shop_Info;
    public GameObject TipsText;
    bool isSetAice = true;
    bool isSetTipsText = true; //显示提示的判定

    public GameObject tips_text_go;
    public GameObject tips_button_go;
    //void Start()
    //{
    //    Shop_Info = GameObject.Find("Shop_Info");
    //    TipsText = Shop_Info.transform.Find("boom/tips/TipsText").gameObject;

    //}
    float time_tips = 8f;
    public void SetTipsText()
    {
        if (true == isSetTipsText)
        {
            TipsText.SetActive(true);
            isSetTipsText = false;
            Invoke("tipsTextSetFasle", time_tips);      
        }
        else if (false == isSetTipsText)
        {
            TipsText.SetActive(false);
            isSetTipsText = true;
        }       
    }
    void tipsTextSetFasle()
    {
        TipsText.SetActive(false);
        time_tips = 8f;
    }
    void OnEnable()
    {
        if (SaveData.GetCash() <= 20)
        {
            tips_text_go.SetActive(true);
            tips_button_go.SetActive(true);
        }
        else
        {
            tips_text_go.SetActive(false);
            tips_button_go.SetActive(false);
        }

    }

    //设置购买炸弹界面的显示和隐藏
    public void ShopTransform_yes()
    {
        Shop_Info.SetActive(true);
    }
    public void ShopTransform_no()
    {
        Shop_Info.SetActive(false);
    }

    public void SetSelfActive()
    {
        this.transform.gameObject.SetActive(false);
        isSetAice = false;
        ShopTransform_no();
    }
    GameObject InstantiateGO;
    //点击生成炸弹按钮的方法
    public void CreatBoom()
    {
        if (SaveData.GetCash() >= 20)
        {
            Debug.Log("SaveData.GetCash()  : " + SaveData.GetCash());
            SaveData.AddCash(-20);

            Debug.Log("SaveData.GetCash()  : " + SaveData.GetCash());
        }
        else { return; }
        GameObject temp = GameObject.Find("MAP_Hard_murky_cross_2L(Clone)");
        if (temp)
        {
            GameObject go_00 = GameObject.Find("enemy_sort");
            enemy_sort.Instance.Create(go_00.transform);
            GameObject go_01 = GameObject.Find("enemy_sort01");
            enemy_sort.Instance.Create(go_01.transform);
            SetSelfActive();
        }
        else
        {
            GameObject go = GameObject.Find("enemy_sort");
            enemy_sort.Instance.Create(go.transform);
            SetSelfActive();
        }

    }






}
