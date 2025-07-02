using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VIPButton : MonoBehaviour
{

    public GameObject VIPMenu;

    public void VIPButtonPressed()
    {
        //print("获取到价格的数量:"+PurchaseManager.priceDir.Count);
        //if (Application.internetReachability == NetworkReachability.NotReachable || PurchaseManager.priceDir.Count <= 0)
        //{
        //    UserInterface.ShowOKDialog(LocManager.Translate("ui_connect_wifi_header"),
        //                               LocManager.Translate("ui_connect_wifi_body"),
        //                               null);

        //    GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = true;
        //}
        //else
        {
            VIPMenu.SetActive(true);
            VIPMenu.transform.localScale = Vector3.zero;
            VIPMenu.GetComponent<Animator>().SetTrigger("on");
            if (transform.GetChild(0).GetComponent<PanelNotifier>())
                transform.GetChild(0).GetComponent<PanelNotifier>().TransitionAll();
        }
    }

    void IsVIPConfirm()
    {
        VIPMenu.GetComponent<Panel>().NavigateBack();
    }

    public void HideVIPMenu(GameObject obj)
    {
        VIPMenu.GetComponent<Animator>().SetTrigger("off");
        if (GameObject.Find("Menu_Upgrades"))
        {
            //说明此时是在升级界面点击的开通VIP
            //返回的时候只需要显示升级界面，隐藏该界面即可
            GameObject.Find("Menu_Upgrades").GetComponent<Animator>().SetBool("Share", false);
            GameObject.Find("Menu_Currency").GetComponent<Animator>().SetBool("Share", false);
            GameObject.Find("Menu_Upgrades").GetComponent<UIUpgrades>().VIPBackToUp();
            SetActive();
        }
        else
        {
            obj.GetComponent<CurrencyNotifier>().DisableStoreButton();
            obj.transform.parent.parent.GetComponent<Panel>().NavigateBack();
            Invoke("SetActive", 0.43f);
        }
    }


    void SetActive()
    {
        VIPMenu.SetActive(false);
        VIPMenu.transform.localScale = Vector3.zero;
    }
}
