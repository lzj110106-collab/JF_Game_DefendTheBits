//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Net;
//using System.Text;
//using LitJson;
//using UnityEngine;
//using UnityEngine.Purchasing;
//using UnityEngine.Networking;
//using System.Runtime.InteropServices;

//namespace TianBoWang.Function
//{
//    [Serializable]
//    public class Products
//    {
//        public string id;
//        public int productType;
//        public float price;
//        public int rewardCount;
//    }

//    /// <summary>
//    /// 购买管理
//    /// </summary>
//    public class PurchaseManager : MonoBehaviour, IStoreListener
//    {
//        public List<Products> products = new List<Products>();
//        public string publicKey;

//        public GameObject shader;
//        public DalyClam dalyClamPanel;
//        private string AllorderID;

//        ConfigurationBuilder builder;
//        private IStoreController m_Controller;
//        private IAppleExtensions m_AppleExtensions;
//        private int productIndex;
//        private static bool isInited = false;
//        private bool isInitFailed = false;

//        void Awake()
//        {

//            if (!isInited)
//            {
//                InitPurchase();
//            }

//            if (SaveData.GetSubRecipt() == null)
//            {
//                //print("现在还没有凭证，肯定不是高贵的VIP");
//                SaveData.SetPlayerVIP(false);
//            }
//            else
//            {
//                //如果有凭证，则拿凭证去验证真身
//                //print("验证真身！");
//                StartCoroutine("CheckRecipe", SaveData.GetSubRecipt());
//            }
//        }

//        void Update()
//        {
//            if(Application.internetReachability == NetworkReachability.NotReachable)
//            {
//                return;
//            }
//            else
//            {
//                if (!isInited)
//                {
//                    InitPurchase();
//                }
//            }
//        }

//        /// <summary>
//        /// 初始化
//        /// </summary>
//        void InitPurchase()
//        {
//#if IOS
//            var module = StandardPurchasingModule.Instance();
//            builder = ConfigurationBuilder.Instance(module);

//            for (int i = 0; i < products.Count; i++)
//            {
//                builder.AddProduct(products[i].id, (ProductType)products[i].productType);
//            }

//            UnityPurchasing.Initialize(this, builder);
//#endif
//        }

//      public static  Dictionary<string, string> priceDir = new Dictionary<string, string>();

//        void SavePrice(string key,string v)
//        {
//            if (priceDir.ContainsKey(key))
//                return;
//            else
//            {
//                priceDir.Add(key, v);
//            }
//        }


//        /// <summary>
//        /// 初始化成功
//        /// </summary>
//        /// <param name="controller">Controller.</param>
//        /// <param name="extensions">Extensions.</param>
//        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
//        {
//            //print("初始化成功");

//            //print("可购买的商品有"+controller.products.all.Length+"个！");

//            for (int i=0;i< controller.products.all.Length;i++)
//            {
//                string key = "";
//                switch (i)
//                {
//                    case 0:
//                        key = "48";
//                        break;
//                    case 1:
//                        key = "12";
//                        break;
//                    case 2:
//                        key = "18";
//                        break;
//                    case 3:
//                        key = "128";
//                        break;
//                    case 4:
//                        key = "6";
//                        break;
//                    case 5:
//                        key = "30";
//                        break;
//                    case 6:
//                        key = "98";
//                        break;
//                    case 7:
//                        key = "1";
//                        break;
//                    case 8:
//                        key = "6";
//                        break;
//                    case 9:
//                        key = "12";
//                        break;
//                    case 10:
//                        key = "18";
//                        break;
//                    case 11:
//                        key = "30";
//                        break;
//                    case 12:
//                        key = "128";
//                        break;
//                    case 13:
//                        key = "328";
//                        break;
//                    case 14:
//                        key = "648";
//                        break;
//                }
//                SavePrice(key, controller.products.all[i].metadata.localizedPriceString);
//                //print(controller.products.all[i].metadata.isoCurrencyCode+"---"+controller.products.all[i].metadata.localizedPrice + "---" + controller.products.all[i].metadata.localizedDescription + "---" + controller.products.all[i].metadata.localizedPriceString+ controller.products.all[i].metadata .localizedTitle+ "\n");
//            }

//            m_Controller = controller;
//            m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
//            m_AppleExtensions.RegisterPurchaseDeferredListener(OnDeferred);

//            Dictionary<string, string> introductory_info_dict = m_AppleExtensions.GetIntroductoryPriceDictionary();

//            isInited = true;

//            foreach (var item in controller.products.all)
//            {
//                //print(string.Format("商品有订单吗？{0}", item.receipt != null));
//                if (item.receipt != null)
//                {
//                    //print(string.Format("商品类型是订阅吗？{0}", item.definition.type == ProductType.Subscription));
//                    if (item.definition.type == ProductType.Subscription)
//                    {
//                        //print(string.Format("SubscriptionManager是否适用该商品？{0}", checkIfProductIsAvailableForSubscriptionManager(item.receipt)));
//                        if (checkIfProductIsAvailableForSubscriptionManager(item.receipt))
//                        {
//                            string intro_json = (introductory_info_dict == null || !introductory_info_dict.ContainsKey(item.definition.storeSpecificId)) ? null : introductory_info_dict[item.definition.storeSpecificId];
//                            SubscriptionManager p = new SubscriptionManager(item, intro_json);
//                            SubscriptionInfo info = p.getSubscriptionInfo();
//                            //Debug.Log("商品ID是: " + info.getProductId());
//                            //Debug.Log("购买日期是: " + info.getPurchaseDate());
//                            //Debug.Log("订阅下一个账单日期是: " + info.getExpireDate());
//                            //Debug.Log("是否已经订阅? " + info.isSubscribed().ToString());
//                            //Debug.Log("是否过期? " + info.isExpired().ToString());
//                            //Debug.Log("是否取消? " + info.isCancelled());
//                            //Debug.Log("商品是否正在试用期? " + info.isFreeTrial());
//                            //Debug.Log("商品是否是自动续期? " + info.isAutoRenewing());
//                            //Debug.Log("订阅剩余时间: " + info.getRemainingTime());
//                            //Debug.Log("商品是否在宣传阶段? " + info.isIntroductoryPricePeriod());
//                            //Debug.Log("商品宣传的本地价格: " + info.getIntroductoryPrice());
//                            //Debug.Log("商品宣传日期是: " + info.getIntroductoryPricePeriod());
//                            //Debug.Log("商品宣传价格周期的次数为: " + info.getIntroductoryPricePeriodCycles());

//                            //Debug.Log("product id is: " + info.getProductId());
//                            //Debug.Log("purchase date is: " + info.getPurchaseDate());
//                            //Debug.Log("subscription next billing date is: " + info.getExpireDate());
//                            //Debug.Log("is subscribed? " + info.isSubscribed().ToString());
//                            //Debug.Log("is expired? " + info.isExpired().ToString());
//                            //Debug.Log("is cancelled? " + info.isCancelled());
//                            //Debug.Log("product is in free trial peroid? " + info.isFreeTrial());
//                            //Debug.Log("product is auto renewing? " + info.isAutoRenewing());
//                            //Debug.Log("subscription remaining valid time until next billing date is: " + info.getRemainingTime());
//                            //Debug.Log("is this product in introductory price period? " + info.isIntroductoryPricePeriod());
//                            //Debug.Log("the product introductory localized price is: " + info.getIntroductoryPrice());
//                            //Debug.Log("the product introductory price period is: " + info.getIntroductoryPricePeriod());
//                            //Debug.Log("the number of product introductory price period cycles is: " + info.getIntroductoryPricePeriodCycles());
//                        }
//                        else
//                        {
//                            //Debug.Log("This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class.");
//                        }
//                    }
//                    else
//                    {
//                        //Debug.Log("the product is not a subscription product");
//                    }
//                }
//                else
//                {
//                    //Debug.Log("the product should have a valid receipt");
//                }

//            }


//        }

//        private bool checkIfProductIsAvailableForSubscriptionManager(string receipt)
//        {
//            var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
//            if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload"))
//            {
//                //Debug.Log("The product receipt does not contain enough information");
//                return false;
//            }
//            var store = (string)receipt_wrapper["Store"];
//            var payload = (string)receipt_wrapper["Payload"];

//            if (payload != null)
//            {
//                switch (store)
//                {
//                    case GooglePlay.Name:
//                        {
//                            var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
//                            if (!payload_wrapper.ContainsKey("json"))
//                            {
//                                //Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
//                                return false;
//                            }
//                            var original_json_payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
//                            if (original_json_payload_wrapper == null || !original_json_payload_wrapper.ContainsKey("developerPayload"))
//                            {
//                                //Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
//                                return false;
//                            }
//                            var developerPayloadJSON = (string)original_json_payload_wrapper["developerPayload"];
//                            var developerPayload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(developerPayloadJSON);
//                            if (developerPayload_wrapper == null || !developerPayload_wrapper.ContainsKey("is_free_trial") || !developerPayload_wrapper.ContainsKey("has_introductory_price_trial"))
//                            {
//                                //Debug.Log("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
//                                return false;
//                            }
//                            return true;
//                        }
//                    case AppleAppStore.Name:
//                    case AmazonApps.Name:
//                    case MacAppStore.Name:
//                        {
//                            return true;
//                        }
//                    default:
//                        {
//                            return false;
//                        }
//                }
//            }
//            return false;
//        }



//        /// <summary>
//        /// iOS 网络延迟错误
//        /// </summary>
//        /// <param name="item">Item.</param>
//        private void OnDeferred(Product item)
//        {
//            // Debug.Log("网络连接不稳");
//        }

//        /// <summary>
//        /// 初始化失败
//        /// </summary>
//        /// <param name="error">Error.</param>
//        public void OnInitializeFailed(InitializationFailureReason error)
//        {
//            isInitFailed = true;
//            //Debug.Log("IAPInitializeFailed!!!" + "Reason：" + error);
//        }

//        /// <summary>
//        /// 恢复购买
//        /// </summary>
//        public void RestorePurchases()
//        {

//            if (Application.platform == RuntimePlatform.IPhonePlayer ||
//                Application.platform == RuntimePlatform.OSXPlayer)
//            {

//                if (!isInited)
//                {
//                    //loading.SetActive(false);
//                    InitPurchase();
//                }

//                StartCoroutine("InitAndRestore");
//            }

//        }

//        IEnumerator InitAndRestore()
//        {

//            if (isInitFailed || !isInited)
//            {
//                //初始化失败
//                StopCoroutine("InitAndRestore");

//            }
//            yield return new WaitUntil(() => { return m_Controller != null && m_AppleExtensions != null; });

//            m_AppleExtensions.RestoreTransactions((result) =>
//            {
//                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
//                // no purchases are available to be restored.
//                //Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");

//                if (result)
//                {
//                    //产品已经restore，不过官方的解释是恢复过程成功了，并不代表所购买的物品都恢复了
//                }
//                else
//                {
//                    // 恢复失败
//                }

//                StopCoroutine("InitAndRestore");
//            });

//        }

//        int buyIndex;

//        public void SetButIndex()
//        {
//            buyIndex = 0;
//        }
        
//        /// <summary>
//        /// 购买产品  购买的第几个    按钮点击
//        /// </summary>
//        /// <param name="index">Index.</param>
//        public void OnPurchaseClicked(int index)
//        {
//#if UNITY_IOS || UNITY_IPHONE
//            //print("准备购买");
//            if (Application.internetReachability == NetworkReachability.NotReachable)
//            {
//                UserInterface.ShowOKDialog(LocManager.Translate("ui_connect_wifi_header"),
//                                           LocManager.Translate("ui_connect_wifi_body"),
//                                           null);

//                GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = true;
//            }
//            else
//#elif UNITY_ANDROID || UNITY_EDITOR
//            {
//                shader.SetActive(true);
//                buyIndex = index;

//                if (index == 0 && SaveData.GetPlayerVIP())
//                {
//                    UserInterface.ShowOKDialog(LocManager.Translate("ui_isviphead"),
//                                          LocManager.Translate("ui_isvipbody"),
//                                          null);
//                    shader.SetActive(false);
//                    return;
//                }

//#if paydebug
//            if (buyIndex == 0)
//            {
//                UserInterface.ShowOKDialog(LocManager.Translate("ui_isviphead"),
//                                      LocManager.Translate("ui_buyvipsucceed"),
//                                      null);
//                //成功开通VIP功能，今天的150枚金钥匙已直接入账
//                SaveData.AddCash(150);
//                SaveData.SetPlayerVIP(true);
//            }
                

//            if (buyIndex > 0 && buyIndex <= 3)          //说明我之前买的是防御塔
//            {
//                GameObject.Find("Menu_Upgrades").GetComponent<UIUpgrades>().BuySucceedCallBack();
//            }

//            if (buyIndex > 3 && buyIndex <= 6)      //说明我买的是礼包
//            {
//                //安卓购买礼包
//                PXY_AndroidBuy.Instance.Buy("com.east2west.defendthebits.GiftPackage", "购买大礼包", 1.00f, "");
//               // GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PaySuccessCallBack("");
//            }

//            if (buyIndex > 6)       //说明我买的是钥匙
//            {
//               // GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PaySuccessCallBack(products[buyIndex].id);
//            }

//            shader.SetActive(false);
//#elif   UNITY_IOS || UNITY_IPHONE
//                if (Application.platform == RuntimePlatform.IPhonePlayer ||
//                    Application.platform == RuntimePlatform.OSXPlayer)
//                {
//                    //print("运行在IOS设备上");
//                    //print("是否已经初始化？"+isInited);
//                    if (!isInited)
//                        InitPurchase();

//                    StartCoroutine("InitAndPurchase", index);
//                }

//#endif
//            }
//#endif


//        }
//        IEnumerator InitAndPurchase(int index)
//        {
//            //print("初始化并且购买");
//            if (isInitFailed || !isInited)
//            {
//                //print("初始化失败");
//                //初始化失败
//                StopCoroutine("InitAndPurchase");

//            }
//            AllorderID = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmss"), UnityEngine.Random.Range(0, 999999));
//            TDGAVirtualCurrency.OnChargeRequest(AllorderID, products[index].id, products[index].price, "CNY", products[index].rewardCount, "苹果官方");
//            yield return new WaitUntil(() => { return m_Controller != null && m_AppleExtensions != null; });
//            //print("要购买的商品的id:"+products[index].id);
//            m_Controller.InitiatePurchase(products[index].id);
//            StopCoroutine("InitAndPurchase");
//        }

//        /// <summary>
//        /// 购买成功回调
//        /// </summary>
//        /// <returns>The purchase.</returns>
//        /// <param name="e">E.</param>
//        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
//        {
//            //print("购买成功回调");
//            //print(e.purchasedProduct.definition.id);
//            //print(products[0].id);
//            //使用id判断是否是当前购买的产品，我这里只有一个产品，所以就是products[0]

//            try
//            {
//                if (e.purchasedProduct.definition.id == products[0].id)         //我之前买的是订阅
//                {
//                    string transactionReceipt = m_AppleExtensions.GetTransactionReceiptForProduct(e.purchasedProduct);
//                    //print("返回的订单："+ transactionReceipt);
//                    StartCoroutine("CheckRecipe", transactionReceipt);//使用苹果的服务器进行验证订单是否有效
//                }

//                if (buyIndex > 0 && buyIndex <= 3)          //说明我之前买的是防御塔
//                {
//                    //StartCoroutine(east2westChcekRecepit(2, null, 0));
//                    GameObject.Find("Menu_Upgrades").GetComponent<UIUpgrades>().BuySucceedCallBack();
//                }

//                if (buyIndex > 3 && buyIndex <= 6)      //说明我买的是礼包
//                {
//                    StartCoroutine(east2westChcekRecepit(1, null, 0));
//                    //GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PaySuccessCallBack("");
//                }

//                //if (buyIndex > 6)       //说明我买的是钥匙
//                //{
//                //    GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PaySuccessCallBack(products[buyIndex].id);
//                //}
//                return PurchaseProcessingResult.Complete;
//            }
//            catch(Exception e2)
//            {
//                return PurchaseProcessingResult.Complete;
//            }
            
//        }

//        //note-east2west verify receipt
//        IEnumerator east2westChcekRecepit(int index,string receipt,int times=0)
//        {
//            yield return null;

//            DateTime lastTime = DateTime.Now;
//            while (string.IsNullOrEmpty(receiptData) && DateTime.Now.Subtract(lastTime).Seconds < 3)
//            {
//                Debug.Log("IsNullOrEmpty---");
//                yield return null;
//            }

//            WWWForm wwwForm = new WWWForm();
//            wwwForm.AddField("data", Convert.ToBase64String(Encoding.Default.GetBytes(string.IsNullOrEmpty( receiptData)?"":receiptData)));
//            WWW www = new WWW("http://param.east2west.cn/defendthebits/IOSValidate.php", wwwForm);
//            yield return www;

//            Debug.Log("re:" + www.text);
//            // if (www.isError)
//            if (!string.IsNullOrEmpty(www.error))
//            {
//                //Debug.Log("re err!!" + www.responseCode);
//                if (times < 3)
//                    StartCoroutine(east2westChcekRecepit(index,receipt,++times));
//                else
//                { //fail
//                    if (index == 0)//订阅
//                    {
//                        shader.SetActive(false);
//                        StopCoroutine("CheckRecipe");
//                    }
//                    else if (index == 1)//礼包
//                    {
//                        GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PayFailed();
//                    }
//                    else if (index == 2)//防御塔
//                    {
//                        GameObject.Find("Menu_Upgrades").GetComponent<UIUpgrades>().BuyFaileCallBack();
//                    }
//                    receiptData = "";
//                }
//                Debug.Log("re err!!" + www.error);
//            }
//            else
//            {
//                //JsonData json = JsonMapper.ToObject(www.downloadHandler.text);
//                JsonData json = JsonMapper.ToObject(www.text);
//                if (json != null && !string.IsNullOrEmpty(json["error"].ToString()))
//                    switch (int.Parse(json["error"].ToString()))
//                    {
//                        case 0:
//                            //sucess
//                            if(index==0)//订阅
//                            {
//                                //验证成功的逻辑
//                                //print("验证成功");
//                                if (!SaveData.IsPaid())
//                                {
//                                    UserInterface.ShowOKDialog(LocManager.Translate("ui_isviphead"),
//                                         LocManager.Translate("ui_buyvipsucceed"),
//                                         null);
//                                    //成功开通VIP功能，今天的150枚金钥匙已直接入账
//                                    SaveData.AddCash(150);

//                                    SaveData.SetIsPaid(true);

//                                    TDGAVirtualCurrency.OnChargeSuccess(AllorderID);
//                                }
//                                SaveData.SetPlayerVIP(true);
//                                SaveData.SetSubRecipt(receipt);

//                                shader.SetActive(false);
//                                StopCoroutine("CheckRecipe");
//                            }
//                            else if(index ==1)//礼包
//                            {
//                               // GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PaySuccessCallBack("");
//                            }
//                            else if(index == 2)//防御塔
//                            {
//                                GameObject.Find("Menu_Upgrades").GetComponent<UIUpgrades>().BuySucceedCallBack();
                                
//                            }

//                            receiptData = "";
//                            break;
//                        case -1:

//                        case -2:
//                        default:
//                            if (index == 0)//订阅
//                            {
//                                shader.SetActive(false);
//                                StopCoroutine("CheckRecipe");
//                            }
//                            else if (index == 1)//礼包
//                            {
//                                GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PayFailed();
//                            }
//                            else if (index == 2)//防御塔
//                            {
//                                GameObject.Find("Menu_Upgrades").GetComponent<UIUpgrades>().BuyFaileCallBack();
//                            }
//                            receiptData = "";
//                            break;
//                    }
//            }
//        }

//        public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
//        {
//            //购买失败的逻辑
//            //print("购买失败");
//            shader.SetActive(false);
//            if (buyIndex > 3 && buyIndex <= 6)      //说明我买的是礼包
//            {
//                GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PayFailed();
//            }

//            if (buyIndex > 6)       //说明我买的是钥匙
//            {
//                GameObject.Find("Menu_Shop").GetComponent<ShopScreen>().PayFailed();
//            }
//        }



//        HttpWebRequest request;
//        UnityWebRequest www;
//        byte[] postBytes;
//        //JsonData resoultJson;
//#if UNITY_IPHONE || UNITY_IOS
//        IEnumerator CheckRecipe(string s)
//        {
//            //print("开始验证");
//            JsonData json = new JsonData();
//            json["receipt-data"] = s;
//            json["password"] = publicKey;

//            string urlReal = "https://buy.itunes.apple.com/verifyReceipt";//正式验证网址
//            //string urlSandBox = "https://sandbox.itunes.apple.com/verifyReceipt";//沙箱测试验证网址

//            using (www = new UnityWebRequest(urlReal, "POST"))
//            {
//                postBytes = Encoding.UTF8.GetBytes(json.ToJson());
//                www.uploadHandler = (UploadHandler)new UploadHandlerRaw(postBytes);
//                www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
//                www.SetRequestHeader("Content-Type", "application/json");
//                www.timeout = 20;//20秒后超时
//                yield return www.Send();


//                if (www.isNetworkError)
//                {
//                    //Debug.Log("网络错误:"+www.error);
//                    shader.SetActive(false);
//                    StopCoroutine("CheckRecipe");
//                }
//                else
//                {
//                    if (www.responseCode == 200)
//                    {
//                        //print("www.responseCode == 200");
//                        resoultJson = JsonMapper.ToObject(www.downloadHandler.text);
//                        //print(resoultJson["status"].ToString());
//                        if (resoultJson["status"].ToString() == "0")
//                        {
//                            //StartCoroutine(east2westChcekRecepit(0,s,0));
//                            //验证成功的逻辑
//                            //print("验证成功");
//                            if (!SaveData.IsPaid())
//                            {
//                                UserInterface.ShowOKDialog(LocManager.Translate("ui_isviphead"),
//                                     LocManager.Translate("ui_buyvipsucceed"),
//                                     null);
//                                //成功开通VIP功能，今天的150枚金钥匙已直接入账
//                                SaveData.AddCash(150);

//                                SaveData.SetIsPaid(true);

//                                TDGAVirtualCurrency.OnChargeSuccess(AllorderID);
//                            }
//                            SaveData.SetPlayerVIP(true);
//                            SaveData.SetSubRecipt(s);

//                            shader.SetActive(false);
//                            StopCoroutine("CheckRecipe");
//                        }
//                        else
//                        {
//                            SaveData.SetPlayerVIP(false);
//                            SaveData.SetIsPaid(false);

//                            shader.SetActive(false);
//                            StopCoroutine("CheckRecipe");
//                        }
//                    }
//                    else
//                    {
//                        shader.SetActive(false);
//                        StopCoroutine("CheckRecipe");
//                    }
//                }
//            }
//        }
//#endif


//        //note-check add!!
//        public string receiptData
//        {
//            get;
//            set;
//        }
//        public void getReceiptSucess(string jsonData)
//        {
//            receiptData = jsonData;
//        }
//    }
//}