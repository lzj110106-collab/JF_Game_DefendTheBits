using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SubscriptionButton : MonoBehaviour
{

    VIPScreen parent;
    public float price;
    public string purchaseID;
    public string purchaseItem;
    public string purchaseDiscribe;

    public GameObject shader;

    public string productID { get; private set; }

    // Use this for initialization
    void Start()
    {
        parent = transform.parent.GetComponent<VIPScreen>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ButtonPressed()
    {

        parent.shader.SetActive(true);
#if UNITY_DEBUG
        //parent.PaySuccess(productID);
        SaveData.SetPlayerVIP(true);
#else
        //print("点击订阅按钮");
        //print("订阅的ID："+purchaseID);
        //buySubscription(purchaseID);
        //Subscription(price, purchaseID, purchaseItem, purchaseDiscribe, "");
        string orderID = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmss"), Random.Range(0, 999999));
        //parent.orderID = orderID;
        TDGAVirtualCurrency.OnChargeRequest(orderID, productID, price, "CNY", 0, "苹果官方");
#endif
    }

    /// <summary>
    /// 订阅成功
    /// </summary>
    /// <param name="productID"></param>
    public void SubSuccess(string reciept)
    {
        //int index = int.Parse(productID.Substring(13, 1));
        //print("支付成功！");
        //print(reciept);
        shader.SetActive(false);
        //UserInterface.ShowOKDialog(LocManager.Translate("iap_success"), LocManager.BuildString("iap_success_body", gemRewards[index]), null);
        GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = false;

        //SaveData.AddCash(gemRewards[index]);


        //TDGAVirtualCurrency.OnChargeSuccess(orderID);


    }

    public void SubFaile()
    {
        //print("支付失败！");
        shader.SetActive(false);
    }

    //[DllImport("__Internal")]
    //private static extern int Purchase(float price, string purchaseID, string purchaseItem, string purchaseDiscribe, string userID);
    //[DllImport("__Internal")]

    //private static extern void buySubscription(string productID);//购买商品
    //[DllImport("__Internal")]
    // private static extern int Subscription(float price, string purchaseID, string purchaseItem, string purchaseDiscribe, string userID);
}
