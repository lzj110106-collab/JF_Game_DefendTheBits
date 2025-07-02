using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Text;
using LitJson;

public class ShopScreen : MonoBehaviour
{

    private static ShopScreen instance;
    public static ShopScreen Instance
    {

        get
        {
            return instance;
        }

    }

    public GameObject shopButtonPrefab;
    public GameObject buttonContainer;

    [Header("UI")]
    public ScrollRect scrollRect;

    public List<ShopButton> buttons { get; private set; }

    public Sprite[] backgroundImages;
    public List<int> gemRewards;

    public string AllorderID;

    public GameObject shader;

    public Text[] timeText;

    public GameObject giftbagButton;

    public GameObject[] giftbagItem;

    public string[] tokenID7;

    #region
    public int appointRewardCount;
    public GameObject item2;
    public GameObject item3;
    public GameObject bottomNav;
    public GameObject header;
    public GameObject appointReward;

    #endregion

    private DateTime time_zero;

    private bool giftbagAvailable = true;

    [Header("Cash")]
    public Text cashDisplay;
    int cashAmount;

    void Awake()
    {
        CreateLevelButtons();
        cashAmount = SaveData.GetCash();
        RefreshCashDisplay();
    }

    void OnEnable()
    {
        //每次进入商店，先把正常状态的显示出来
        header.SetActive(true);
        item2.SetActive(true);
        item3.SetActive(true);
        bottomNav.SetActive(true);
        appointReward.SetActive(false);


        CreateLevelButtons();
        //if (Screen.width / Screen.height >= 2)
        //    buttonContainer.transform.localPosition = new Vector3(1000, buttonContainer.transform.localPosition.y, buttonContainer.transform.localPosition.z);
        //else
        buttonContainer.transform.localPosition = new Vector3(921, buttonContainer.transform.localPosition.y, buttonContainer.transform.localPosition.z);
        //print(SaveData.GetNextGiftbagTime());

        //每次进入商店，都先显示钥匙购买界面
        buyItem[0].SetActive(false);
        buyItem[1].SetActive(false);
        buyItem[2].SetActive(false);

        tokenShader.SetActive(false);

        if (CurrencyDisplay.enterShopIndex == 0)
        {
            //如果有可购买的礼包
            if (SaveData.GetItem(0) || SaveData.GetItem(1) || SaveData.GetItem(2))
            {
                //if (SaveData.GetNextGiftbagTime() == null)      //如果获取到的时间为空，则玩家是第一次进入，那就把显示结束的时间设置为当前时间的24小时后
                //{
                //    DateTime dt = DateTime.Now;
                //   dt= dt.AddDays(1);
                //    print(string.Format("{0}-{1}-{2} {3}:{4}:{5}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second));
                //    time_zero = Convert.ToDateTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second));
                //    SaveData.SetNextGiftbagTime(time_zero.ToString());
                //    giftbagAvailable = true;
                //}
                //else                //如果获取到了时间，那就跟现在的时间进行对比，这里不必考虑玩家修改本地时间
                //{
                //    //print(SaveData.GetNextGiftbagTime());
                //    time_zero = Convert.ToDateTime(SaveData.GetNextGiftbagTime());
                //    //print((time_zero - System.DateTime.Now).TotalHours);
                //    if ((time_zero - System.DateTime.Now).TotalHours < 24 && (time_zero - System.DateTime.Now).TotalHours > 0)      //如果保存的时间与现在的时间差在0-24小时之内，说明现在应该显示出来
                //        giftbagAvailable = true;
                //    else if((time_zero - System.DateTime.Now).TotalHours > 24)      //说明显示的时间还没到
                //        giftbagAvailable = false;
                //    else            //说明显示的时间已经过了，要在原先时间的基础上加上三天作为下一次显示的时间
                //    {
                //        giftbagAvailable = false;
                //        time_zero = Convert.ToDateTime(SaveData.GetNextGiftbagTime());
                //        //DateTime temp = Convert.ToDateTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", time_zero.Year, time_zero.Month, time_zero.Day + 3, time_zero.Hour, time_zero.Minute, time_zero.Second));
                //        DateTime temp = time_zero.AddDays(3);
                //        SaveData.SetNextGiftbagTime(temp.ToString());
                //    }
                //}
                buyItem[2].SetActive(true);

                for (int i = 0; i < giftbagItem.Length; i++)
                    giftbagItem[i].SetActive(SaveData.GetItem(i));

                giftbagAvailable = true;

                if (giftbagAvailable)
                {
                    //如果礼包可用，礼包按钮就显示出来
                    giftbagButton.SetActive(true);
                }
                else
                {
                    //否则，隐藏礼包按钮
                    giftbagButton.SetActive(false);
                }
            }
            else
            {
                giftbagButton.SetActive(false);
                buyItem[2].SetActive(false);

                buyItem[0].SetActive(true);
            }
        }
        else if (CurrencyDisplay.enterShopIndex == 1)
        {
            buyItem[0].SetActive(true);
            buyItem[1].SetActive(false);
            buyItem[2].SetActive(false);
        }
        else if (CurrencyDisplay.enterShopIndex == 2)       //进入商店，这是预约奖励进来的
        {
            header.SetActive(false);
            item2.SetActive(false);
            item3.SetActive(false);
            bottomNav.SetActive(false);
            appointReward.SetActive(true);
            UpdateappointReward(appointRewardCount);
        }
        else        //饰品不足或礼包购买完了
        {
            buyItem[0].SetActive(false);
            buyItem[1].SetActive(true);
            buyItem[2].SetActive(false);
        }
    }

    public Transform[] appointRewardContent;

    void UpdateappointReward(int count)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < count)     //说明已达成，奖励显示出来
            {
                //print(appointRewardContent[i].childCount);
                for (int j = 0; j < appointRewardContent[i].childCount - 1; j++)
                {
                    appointRewardContent[i].GetChild(j).gameObject.SetActive(true);
                }
                appointRewardContent[i].GetChild(appointRewardContent[i].childCount - 1).gameObject.SetActive(false);

                appointRewardContent[i].GetComponent<AppointRewardButton>().isActive = true;
                appointRewardContent[i].GetComponent<AppointRewardButton>().RefeashState();
            }
            else
            {
                //未达成，奖励隐藏           
                for (int j = 0; j < appointRewardContent[i].childCount - 1; j++)
                {
                    appointRewardContent[i].GetChild(j).gameObject.SetActive(false);
                }
                appointRewardContent[i].GetChild(appointRewardContent[i].childCount - 1).gameObject.SetActive(true);


                appointRewardContent[i].GetComponent<AppointRewardButton>().isActive = false;
                appointRewardContent[i].GetComponent<AppointRewardButton>().RefeashState();
            }
        }
    }

    void Update()
    {
        //if (giftbagAvailable)       //如果礼包可用的话，就更新时间
        //{
        //    for (int i = 0; i < timeText.Length; i++)
        //    {
        //        string temp= string.Format(":{0}:{1}:{2}", (time_zero - System.DateTime.Now).Hours, (time_zero - System.DateTime.Now).Minutes, (time_zero - System.DateTime.Now).Seconds);
        //        timeText[i].text = LocManager.Translate("ui_remaintime") + temp;
        //       // LocManager.Assign(timeText[i], "ui_remaintime", (time_zero - System.DateTime.Now).Hours, (time_zero - System.DateTime.Now).Minutes, (time_zero - System.DateTime.Now).Seconds);
        //    }

        //    if ((time_zero - System.DateTime.Now).TotalSeconds <= 0.0f)
        //    {
        //        print("倒计时时间到了");
        //        //time_zero = Convert.ToDateTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 3, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
        //        time_zero = time_zero.AddDays(3);
        //        SaveData.SetNextGiftbagTime(time_zero.ToString());
        //        buyItem[2].SetActive(false);
        //        giftbagButton.SetActive(false);

        //        buyItem[0].SetActive(true);

        //        giftbagAvailable = false;
        //    }
        //}
        //else
        //{
        //    //如果礼包有可购买的项，但是当前礼包不可用，还是要检测时间，一旦时间在24小时之内，就把按钮显示出来
        //    if(SaveData.GetItem(0) || SaveData.GetItem(1) || SaveData.GetItem(2))
        //    {
        //        time_zero = Convert.ToDateTime(SaveData.GetNextGiftbagTime());
        //        if((time_zero-System.DateTime.Now).TotalHours<=24&& (time_zero - System.DateTime.Now).TotalHours > 0)
        //        {
        //            giftbagButton.SetActive(true);
        //        }
        //    }
        //}

        int newCashCount = SaveData.GetCash();
        if (newCashCount != cashAmount)
        {
            cashAmount = newCashCount;
            RefreshCashDisplay();
        }

#if PXY_YUNCE
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("测试退出按钮");
            //Application.Quit();
            PXY_AndroidBuy.Instance.ExitGame();
            //ExitGame.Instance.SetSelfTrue();
        }
#endif
    }

    void RefreshCashDisplay()
    {
        cashDisplay.text = cashAmount.ToString();
    }

    void CreateLevelButtons()
    {
        UnityUtil.DestroyAllChildren(buttonContainer);
        buttons = new List<ShopButton>();

        //NB: dont show the highest level IAP for now
        if (LocManager.CurrentLanguage() == LocManager.Language.Chinese_Simplified)
        {
            for (int i = 0; i < gemRewards.Count; ++i)
            {
                var instance = GameObject.Instantiate(shopButtonPrefab);
                instance.transform.SetParent(buttonContainer.transform, false);

                var button = instance.GetComponent<ShopButton>();
                if (i >= 6)
                    button.Initialise(this, "iap_pack_pay_" + (i + 1).ToString(), i);
                else
                    button.Initialise(this, "iap_pack_pay_" + i.ToString(), i);

                buttons.Add(button);
            }
        }
        else
        {
            for (int i = 1; i < gemRewards.Count; ++i)
            {
                var instance = GameObject.Instantiate(shopButtonPrefab);
                instance.transform.SetParent(buttonContainer.transform, false);

                var button = instance.GetComponent<ShopButton>();
                if (i >= 6)
                    button.Initialise(this, "iap_pack_pay_" + (i + 1).ToString(), i);
                else
                    button.Initialise(this, "iap_pack_pay_" + i.ToString(), i);

                buttons.Add(button);
            }
        }
    }

    string GetProductID(int index)
    {
        string result = "";
        switch (index)
        {
            case 0:
                result = "iap_pack_pay_3";
                break;
            case 1:
                result = "iap_pack_pay_4";
                break;
            case 2:
                result = "iap_pack_pay_5";
                break;
            case 3:
                result = "iap_pack_pay_0";
                break;
            case 4:
                result = "iap_pack_pay_1";
                break;
            case 5:
                result = "iap_pack_pay_2";
                break;
            case 6:
                result = "iap_pack_pay_6";
                break;
            case 7:
                result = "iap_pack_pay_7";
                break;
        }


        return result;
    }


    int GetProductIndex(int index)
    {
        int temp = 0;
        switch (index)
        {
            case 0:
                temp = 3;
                break;
            case 1:
                temp = 4;
                break;
            case 2:
                temp = 5;
                break;
            case 3:
                temp = 0;
                break;
            case 4:
                temp = 1;
                break;
            case 5:
                temp = 2;
                break;
            case 6:
                temp = 6;
                break;
            case 7:
                temp = 7;
                break;
        }

        return temp;
    }

    public void OnIAPButtonPressed(ShopButton button)
    {
        //GameObject.Find("PurchaseManager").GetComponent<PurchaseManager>().SetButIndex();

        //for (var i = 0; i < buttons.Count; ++i)
        //{
        //    if (buttons[i] != button)
        //        continue;

        //    //TODO: need to lock the UI at this point while the purchase is attempted

        //    IAPManager.BuyProduct(button.productID,
        //        (string productID) =>
        //        {

        //            //print("支付成功");

        //            //					Debug.Log("[ShopScreen] purchase successful: " + productID); 
        //            UserInterface.ShowOKDialog(LocManager.Translate("iap_success"),
        //                                       LocManager.BuildString("iap_success_body", gemRewards[i]),
        //                                       null);
        //            GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = false;

        //            SaveData.AddCash(gemRewards[i]);
        //        },
        //        (string productID) =>
        //        {
        //            //print("支付失败");
        //            //					Debug.Log("[ShopScreen] purchase failed: " + productID); 
        //            UserInterface.ShowOKDialog(LocManager.Translate("ui_connect_wifi_header"),
        //                                       LocManager.Translate("ui_connect_wifi_body"),
        //                                       null);
        //            GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = false;
        //        });


        //    break;
        //}
    }

    IEnumerator east2westChcekRecepit(string productID, int times = 0)
    {
        //PurchaseManager purchaseM = GameObject.FindObjectOfType<PurchaseManager>();

        yield return null;

        //DateTime lastTime = DateTime.Now;
        //while (string.IsNullOrEmpty(purchaseM.receiptData) && DateTime.Now.Subtract(lastTime).Seconds < 3)
        //{
        //    Debug.Log("IsNullOrEmpty---");
        //    yield return null;
        //}

        //if (purchaseM != null)
        //{
        //    WWWForm wwwForm = new WWWForm();
        //    wwwForm.AddField("data", Convert.ToBase64String(Encoding.Default.GetBytes(string.IsNullOrEmpty(purchaseM.receiptData) ? "" : purchaseM.receiptData)));
        //    WWW www = new WWW("http://param.east2west.cn/defendthebits/IOSValidate.php", wwwForm);
        //    yield return www;

        //    Debug.Log("re:" + www.text);
        //    // if (www.isError)
        //    if (!string.IsNullOrEmpty(www.error))
        //    {
        //        //Debug.Log("re err!!" + www.responseCode);
        //        if (times < 3)
        //            StartCoroutine(east2westChcekRecepit(productID, ++times));
        //        else
        //        { //fail
        //            PayFailed();
        //            purchaseM.receiptData = "";
        //        }
        //        Debug.Log("re err!!" + www.error);
        //    }
        //    else
        //    {
        //        //JsonData json = JsonMapper.ToObject(www.downloadHandler.text);
        //        JsonData json = JsonMapper.ToObject(www.text);
        //        if (json != null && !string.IsNullOrEmpty(json["error"].ToString()))
        //            switch (int.Parse(json["error"].ToString()))
        //            {
        //                case 0:
        //                    StartCoroutine(temp(productID));
        //                    purchaseM.receiptData = "";
        //                    break;
        //                case -1:
        //                case -2:
        //                default:
        //                    PayFailed();
        //                    purchaseM.receiptData = "";
        //                    break;
        //            }
        //    }
        //}
        //else
        //{
        //    PayFailed();
        //    purchaseM.receiptData = "";
        //}
    }

    public void PaySuccess(string productID)
    {

        StartCoroutine(temp(productID));
#if UNITY_IOS || UNITY_IPHONE
        StartCoroutine(east2westChcekRecepit(productID, 0));
#endif
        //
    }

    IEnumerator temp(string productID)
    {
        yield return new WaitForSeconds(1.0f);
        shader.SetActive(false);
        if (isBuyGiftBag)
        {
            //print("购买的是礼包");
            //如果是购买礼包，增加钥匙数量，并且调用随机获得两个或三个饰品的方法


            if (rewardToken > 0)
            {
                //说明要获得饰品
                GetToken(rewardToken);
            }

            GetKeys(rewardCash);

            if (rewardToken == 0)
            {
                //print("购买的是新手礼包");
                tokenShader.transform.GetChild(2).gameObject.SetActive(false);
                giftbagItem[0].SetActive(false);
                SaveData.SetItem(0, false);
            }

            if (rewardToken == 2)
            {
                //print("购买的是助力包");
                tokenShader.transform.GetChild(2).gameObject.SetActive(false);
                giftbagItem[1].SetActive(false);
                SaveData.SetItem(1, false);
            }

            if (rewardToken == 3)
            {
                //print("购买的是豪华包");
                tokenShader.transform.GetChild(2).gameObject.SetActive(false);
                giftbagItem[2].SetActive(false);
                SaveData.SetItem(2, false);
            }


            //如果没有可购买的礼包
            if (!SaveData.GetItem(0) && !SaveData.GetItem(1) && !SaveData.GetItem(2))
            {
                giftbagButton.SetActive(false);
                buyItem[0].SetActive(true);
            }

            isBuyGiftBag = false;
        }
        else
        {
            print("购买的是钥匙");
            int index = int.Parse(productID.Substring(13, 1));
            print("index : " + index);
            if (index >= 6)
                index--;

            UserInterface.ShowOKDialog(LocManager.Translate("iap_success"), LocManager.BuildString("iap_success_body", gemRewards[index]), null);
            GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = false;

            SaveData.AddCash(gemRewards[index]);
        }
        //TDGAVirtualCurrency.OnChargeSuccess(AllorderID);
    }

    public void PaySuccessCallBack(string productID,bool isBuyGiftBag,int rewardToken,int rewardCash)
    {

        shader.SetActive(false);
        if (GameObject.Find("Image (2)"))
            GameObject.Find("Image (2)").SetActive(false);
        if (isBuyGiftBag)
        {
            //print("购买的是礼包");
            //如果是购买礼包，增加钥匙数量，并且调用随机获得两个或三个饰品的方法
            //SaveData.AddCash(rewardCash);
            GetKeys(rewardCash);
            if (rewardToken > 0)
            {
                //说明要获得饰品
                GetToken(rewardToken);
            }

            if (rewardToken == 1)
            {
               // Debug.Log("购买的是新手礼包");
                giftbagItem[0].SetActive(false);
                SaveData.SetItem(0, false);
            }

            if (rewardToken == 2)
            {
                //Debug.Log("购买的是助力包");
                giftbagItem[1].SetActive(false);
                SaveData.SetItem(1, false);
            }

            if (rewardToken == 5)
            {
                //Debug.Log("购买的是豪华包");
                giftbagItem[2].SetActive(false);
                SaveData.SetItem(2, false);
            }

            //TDGAVirtualCurrency.OnChargeSuccess(AllorderID);

            //如果没有可购买的礼包
            if (!SaveData.GetItem(0) && !SaveData.GetItem(1) && !SaveData.GetItem(2))
            {
                giftbagButton.SetActive(false);
                buyItem[0].SetActive(true);
            }

            isBuyGiftBag = false;
        }
        else
        {
            if (productID == "" || string.IsNullOrEmpty(productID))
                return;
            print("购买的是钥匙" + productID);
            int index = int.Parse(productID.Substring(13, 1));
            print(index);
            if (index >= 6)
                index--;
            print(index);
            UserInterface.ShowOKDialog(LocManager.Translate("iap_success"), LocManager.BuildString("iap_success_body", gemRewards[index]), null);
            GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = false;
            print(gemRewards.Count);
            SaveData.AddCash(gemRewards[index]);
            print("购买成功!");
        }
    }

    /// <summary>
    /// 支付失败
    /// </summary>
    public void PayFailed()
    {
        shader.SetActive(false);
    }

    /// <summary>
    /// 显示遮罩，防止玩家在购买延迟的时候继续点击操作
    /// </summary>
    public void ShowShader()
    {
        shader.SetActive(true);
    }

    public GameObject[] buyItem;

    /// <summary>
    /// 点击不同的标题，显示不同的购买列表
    /// </summary>
    public void ShowBuyItem(int index)
    {
        for (int i = 0; i < buyItem.Length; i++)
        {
            if (index == i)
            {
                buyItem[i].SetActive(true);
            }
            else
            {
                buyItem[i].SetActive(false);
            }
        }


        //buyItem[index].SetActive(true);
    }

    //public PurchaseManager purchaseManager;

    bool isBuyGiftBag = false;
    int rewardCash = 0;
    int rewardToken = 0;
    /// <summary>
    /// 购买礼包
    /// </summary>
    /// <param name="index"></param>
    public void BuyGiftBag(int index)
    {
#if UNITY_IOS || UNITY_IPHONE
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            UserInterface.ShowOKDialog(LocManager.Translate("ui_connect_wifi_header"),
                                       LocManager.Translate("ui_connect_wifi_body"),
                                       null);

            GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = true;
        }
        else
#elif UNITY_ANDROID    ||  UNITY_EDITOR
        {
            isBuyGiftBag = true;
            ShowShader();
            switch (index)
            {
                case 0:
                    rewardCash = 200;
                    rewardToken = 1;
#if UNITY_IOS || UNITY_IPHONE
                    purchaseManager.OnPurchaseClicked(4);
                     AllorderID = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmss"), UnityEngine.Random.Range(0, 999999));
                     TDGAVirtualCurrency.OnChargeRequest(AllorderID, "iap_pay_newplayerbag", 6.0f, "CNY", 200, "苹果官方");
#elif UNITY_ANDROID || UNITY_EDITOR
                    OnPurchaseClicked(4);
                    AllorderID = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmss"), UnityEngine.Random.Range(0, 999999));
                    Debug.Log("购买礼包AllorderID:   " + AllorderID);
#endif
                    break;
                case 1:
                    rewardCash = 600;
                    rewardToken = 2;
#if UNITY_IOS || UNITY_IPHONE
                    purchaseManager.OnPurchaseClicked(5);
                     AllorderID = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmss"), UnityEngine.Random.Range(0, 999999));
                     TDGAVirtualCurrency.OnChargeRequest(AllorderID, "iap_pay_newplayerbag", 6.0f, "CNY", 200, "苹果官方");
#elif UNITY_ANDROID || UNITY_EDITOR
                    OnPurchaseClicked(5);
                    AllorderID = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmss"), UnityEngine.Random.Range(0, 999999));
                    Debug.Log("购买礼包AllorderID:   " + AllorderID);
#endif
                    break;
                case 2:
                    rewardCash = 2000;
                    rewardToken = 5;
#if UNITY_IOS || UNITY_IPHONE
                    purchaseManager.OnPurchaseClicked(6);
                    AllorderID = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmss"), UnityEngine.Random.Range(0, 999999));
                    TDGAVirtualCurrency.OnChargeRequest(AllorderID, "iap_pay_newplayerbag", 6.0f, "CNY", 200, "苹果官方");
#elif UNITY_ANDROID || UNITY_EDITOR
                    OnPurchaseClicked(6);
                    AllorderID = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmss"), UnityEngine.Random.Range(0, 999999));
                    Debug.Log("购买礼包AllorderID:   " + AllorderID);
                    
#endif
                    break;
            }
        }
#endif

    }
    //--------------------------------------------

    //public List<Products> products = new List<Products>();
    /// <summary>
    /// 购买产品  购买的第几个    按钮点击
    /// </summary>
    /// <param name="index">Index.</param>
    public void OnPurchaseClicked(int index)
    {

//#if UNITY_ANDROID && !UNITY_EDITOR

        shader.SetActive(true);


        if (index == 0 && SaveData.GetPlayerVIP())
        {
            UserInterface.ShowOKDialog(LocManager.Translate("ui_isviphead"),
                                    LocManager.Translate("ui_isvipbody"),
                                    null);
            shader.SetActive(false);
            return;
        }
        //if (index == 0)
        //{
        //    UserInterface.ShowOKDialog(LocManager.Translate("ui_isviphead"),
        //                            LocManager.Translate("ui_buyvipsucceed"),
        //                            null);
        //    //成功开通VIP功能，今天的150枚金钥匙已直接入账
        //    SaveData.AddCash(150);
        //    SaveData.SetPlayerVIP(true);
        //}


        //if (index > 0 && index <= 3)          //说明我之前买的是防御塔
        //{
        //    GameObject.Find("Menu_Upgrades").GetComponent<UIUpgrades>().BuySucceedCallBack();
        //}

        //if (index > 3 && index <= 6)      //说明我买的是礼包
        //{
        //    //安卓购买礼包
        //    PXY_AndroidBuy.Instance.Buy("com.east2west.defendthebits.GiftPackage", "购买大礼包", 1.00f, "");
        //    GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PaySuccessCallBack("");
        //}

        //if (index > 6)       //说明我买的是钥匙
        //{
        //    GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PaySuccessCallBack(products[index].id);
        //}

        //shader.SetActive(false);


//#endif
//  #if paydebug
        if (index == 0)
        {
            UserInterface.ShowOKDialog(LocManager.Translate("ui_isviphead"),
                                    LocManager.Translate("ui_buyvipsucceed"),
                                    null);
            //成功开通VIP功能，今天的150枚金钥匙已直接入账
            SaveData.AddCash(150);
            SaveData.SetPlayerVIP(true);
        }


        if (index > 0 && index <= 3)          //说明我之前买的是防御塔
        {
            GameObject.Find("Menu_Upgrades").GetComponent<UIUpgrades>().BuySucceedCallBack();
        }

        if (index > 3 && index <= 6)      //说明我买的是礼包
        {            
            if (index.Equals(4))
            {
                //安卓购买礼包
                //PXY_AndroidBuy.Instance.Buy("com.east2west.defendthebits.GiftPackage_9", "购买大礼包", 9.00f, "");
            }
            else if (index.Equals(5))
            {
                //安卓购买礼包
                //PXY_AndroidBuy.Instance.Buy("com.east2west.defendthebits.GiftPackage_30", "购买大礼包", 30.00f, "");
            }
            else if (index.Equals(6))
            {
                //安卓购买礼包
                //PXY_AndroidBuy.Instance.Buy("com.east2west.defendthebits.GiftPackage_98", "购买大礼包", 98.00f, "");
            }

            //GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PaySuccessCallBack("");
        }

        if (index > 6)       //说明我买的是钥匙
        {
            //GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PaySuccessCallBack(products[index].id);
        }

        shader.SetActive(false);

//#endif

    }
    //-------------------------------------


    /// <summary>
    /// 仅仅购买钥匙
    /// </summary>
    /// <param name="price"></param>
    /// <param name="purchaseID"></param>
    /// <param name="purchaseItem"></param>
    /// <param name="purchaseDiscribe"></param>
    public void BuyKey(float price, string purchaseID, string purchaseItem, string purchaseDiscribe)
    {
        Purchase(price, purchaseID, purchaseItem, purchaseDiscribe, "");
        string orderID = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmss"), UnityEngine.Random.Range(0, 999999));
        //TDGAVirtualCurrency.OnChargeRequest(orderID, purchaseID, price, "CNY", price, "苹果官方");
    }

    public void BuyTrinkets(int costCash)
    {
        //如果钥匙足够
        if (SaveData.GetCash() >= costCash)
        {
            SaveData.AddCash(-costCash);
            switch (costCash)
            {
                case 200:
                    GetToken(1);
                    break;
                case 450:
                    GetToken(2);
                    break;
                case 2200:
                    GetToken(5);
                    break;
            }
        }
        else            //钥匙不足的话，跳转到购买钥匙的界面
        {
            UserInterface.ShowOKDialog(LocManager.Translate("ui_badhead"),
                                     LocManager.Translate("ui_cashnotenough"),
                                     NotEnoughTrinket);
        }
    }

    void NotEnoughTrinket()
    {
        buyItem[0].SetActive(true);
        buyItem[1].SetActive(false);
        buyItem[2].SetActive(false);

        tokenShader.SetActive(false);
    }

    public GameObject tokenShader;
    public ParticleSystem tokenPS;

    /// <summary>
    /// 获取饰品
    /// </summary>
    /// <param name="count"></param>
    public void GetToken(int count)       //种类数量，总获得数量
    {
        if (count <= 0)
            return;

        tokenShader.SetActive(true);
        tokenPS.Play();
        GetComponent<PlayAudio>().PlayClip("UI_EOR_Star");

        for (int i = 0; i < 6; i++)
        {
            tokenShader.transform.GetChild(4).GetChild(i).gameObject.SetActive(false);
        }

        int index = 0;
        string trinketID = "";

        switch (count)
        {
            case 1:
                //if (isBuyGiftBag)
                //    tokenShader.transform.GetChild(2).gameObject.SetActive(false);
                //else
                //    tokenShader.transform.GetChild(2).GetComponent<Text>().text = LocManager.Translate("ui_opensmallbox");
                index = UnityEngine.Random.Range(0, 3);
                trinketID = tokenID7[index];

                
                tokenShader.transform.GetChild(4).GetChild(1).gameObject.SetActive(true);
                StartCoroutine(PlayParticleSystem(tokenShader.transform.GetChild(4).GetChild(1).GetChild(2).GetComponent<ParticleSystem>()));
                tokenShader.transform.GetChild(4).GetChild(1).GetChild(0).GetComponent<Image>().sprite = TrinketDatabase.GetIcon(trinketID);
                tokenShader.transform.GetChild(4).GetChild(1).GetChild(1).GetComponent<Text>().text = "x100";
                SaveData.AddTrinket(trinketID, 100);
                break;
            case 2:
                //if (isBuyGiftBag)
                //    tokenShader.transform.GetChild(2).gameObject.SetActive(false);
                //else
                //    tokenShader.transform.GetChild(2).GetComponent<Text>().text = LocManager.Translate("ui_openmediumbox");
                for (int i = 0; i < count; i++)
                {
                    index = UnityEngine.Random.Range(0, 5);
                    trinketID = tokenID7[index];
                    
                    tokenShader.transform.GetChild(4).GetChild(i + 1).gameObject.SetActive(true);
                    StartCoroutine(PlayParticleSystem(tokenShader.transform.GetChild(4).GetChild(i + 1).GetChild(2).GetComponent<ParticleSystem>()));
                    tokenShader.transform.GetChild(4).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = TrinketDatabase.GetIcon(trinketID);
                    tokenShader.transform.GetChild(4).GetChild(i + 1).GetChild(1).GetComponent<Text>().text = "x125";
                    SaveData.AddTrinket(trinketID, 125);
                }
                break;
            case 5:
                //if (isBuyGiftBag)
                //    tokenShader.transform.GetChild(2).gameObject.SetActive(false);
                //else
                //    tokenShader.transform.GetChild(2).GetComponent<Text>().text = LocManager.Translate("ui_openlargerbox");
                for (int i = 0; i < count; i++)
                {
                    index = UnityEngine.Random.Range(0, 7);
                    trinketID = tokenID7[index];
             
                    tokenShader.transform.GetChild(4).GetChild(i + 1).gameObject.SetActive(true);
                    StartCoroutine(PlayParticleSystem(tokenShader.transform.GetChild(4).GetChild(i + 1).GetChild(2).GetComponent<ParticleSystem>()));
                    tokenShader.transform.GetChild(4).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = TrinketDatabase.GetIcon(trinketID);
                    tokenShader.transform.GetChild(4).GetChild(i + 1).GetChild(1).GetComponent<Text>().text = "x300";
                    SaveData.AddTrinket(trinketID, 300);
                }
                break;
        }
    }


    public Text tokenShaderKeys;
    public Sprite keysSprite;

    public void GetKeys(int keysCount)
    {
        tokenShader.SetActive(true);
        GetComponent<PlayAudio>().PlayClip("UI_EOR_Star");
        tokenShader.transform.GetChild(4).GetChild(0).gameObject.SetActive(true);
        StartCoroutine(PlayParticleSystem(tokenShader.transform.GetChild(4).GetChild(0).GetChild(2).GetComponent<ParticleSystem>()));
        tokenShaderKeys.text = string.Format("x{0}", keysCount);
        tokenShader.transform.GetChild(4).GetChild(0).GetChild(0).GetComponent<Image>().sprite = keysSprite;
        SaveData.AddCash(keysCount);
    }

    IEnumerator PlayParticleSystem(ParticleSystem ps)
    {
        yield return new WaitForSeconds(0.0f);

        ps.Play();
    }

    public void CloseTokenShader()
    {
        for (int i = 0; i < 4; i++)
        {
            tokenShader.transform.GetChild(4).GetChild(i).gameObject.SetActive(false);
        }
        tokenShader.SetActive(false);
    }


    [DllImport("__Internal")]
    private static extern int Purchase(float price, string purchaseID, string purchaseItem, string purchaseDiscribe, string userID);
}
