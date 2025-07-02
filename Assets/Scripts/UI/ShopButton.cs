using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;

public class ShopButton : MonoBehaviour
{
    ShopScreen parent;
    public string productID { get; private set; }

    public Text displayText;
    public Text priceText;
    public Text gemText;
    public Image mapPortrait;
    public float price;
    public int rewardCount;

    public GameObject child;
    string purchaseItem;
    string purchaseDiscribe;
    public Text cost;
    string keyProductID = "";

    public void Initialise(ShopScreen parent, string productID, int index)
    {
        this.parent = parent;
        this.productID = productID;
        LocManager.Assign(displayText, productID);
        //priceText.text = IAPManager.GetCostText(productID);
        price = GetBuyuInfo(index);
       
        rewardCount = GetBuyInfo(index);

        //priceText.text = LocManager.GetCurrencySymbol() + LocManager.GetTargetPrice((int)price);
        //if (PurchaseManager.priceDir.ContainsKey(((int)price).ToString()))
        //    priceText.text = PurchaseManager.priceDir[((int)price).ToString()];

        //TODO: probably need a more robust solution for this in the future
        for (int i = 0; i < parent.backgroundImages.Length; ++i)
        {
            if (i >= 6)
            {
                if (productID == "iap_pack_pay_" + (i + 1).ToString())
                {
                    mapPortrait.sprite = parent.backgroundImages[i];
                    gemText.text = parent.gemRewards[i].ToString();
                }
            }
            else
            {
                if (productID == "iap_pack_pay_" + i.ToString())
                {
                    mapPortrait.sprite = parent.backgroundImages[i];
                    gemText.text = parent.gemRewards[i].ToString();
                }
            }
        }
    }

    int GetBuyInfo(int index)
    {
        int result = 0;
        result = parent.gemRewards[index];
        return result;
    }

    float GetBuyuInfo(int index)
    {
        float result = 0.00f;

        switch (index)
        {
            case 0:
                result = 1.00f;
                purchaseItem = "迷你钥匙袋";
                purchaseDiscribe = "购买可获得一个迷你钥匙袋";
                cost.text = "￥1.00";
                keyProductID = "com.east2west.defendthebits.keyProductID_1";
                break;
            case 1:
                result = 6.00f;
                purchaseItem = "少量钥匙袋";
                purchaseDiscribe = "购买可获得一个少量钥匙袋";
                cost.text = "￥6.00";
                keyProductID = "com.east2west.defendthebits.keyProductID_6";
                break;
            case 2:
                result = 12.00f;
                purchaseItem = "中量钥匙袋";
                purchaseDiscribe = "购买可获得一个中量钥匙袋";
                cost.text = "￥12.00";
                keyProductID = "com.east2west.defendthebits.keyProductID_12";
                break;
            case 3:
                result = 18.00f;
                purchaseItem = "大量钥匙袋";
                purchaseDiscribe = "购买可获得一个大量钥匙袋";
                cost.text = "￥18.00";
                keyProductID = "com.east2west.defendthebits.keyProductID_18";
                break;
            case 4:
                result = 30.00f;
                purchaseItem = "一堆金钥匙";
                purchaseDiscribe = "购买可获得一堆金钥匙";
                cost.text = "￥30.00";
                keyProductID = "com.east2west.defendthebits.keyProductID_30";
                break;
            case 5:
                result = 128.00f;
                purchaseItem = "一箱金钥匙";
                purchaseDiscribe = "购买可获得一箱金钥匙";
                cost.text = "￥128.00";
                keyProductID = "com.east2west.defendthebits.keyProductID_128";
                break;
            case 6:
                result = 328.00f;
                purchaseItem = "一大箱金钥匙";
                purchaseDiscribe = "购买可获得一大箱金钥匙";
                cost.text = "￥328.00";
                keyProductID = "com.east2west.defendthebits.keyProductID_328";
                break;
            case 7:
                result = 648.00f;
                purchaseItem = "一车金钥匙";
                purchaseDiscribe = "购买可获得一车金钥匙";
                cost.text = "￥648.00";
                keyProductID = "com.east2west.defendthebits.keyProductID_648";
                break;
        }

        return result;
    }

    public void OnEnable()
    {
        LocManager.Assign(displayText, productID);
        //priceText.text = LocManager.GetCurrencySymbol() + LocManager.GetTargetPrice((int)price);
        //if (PurchaseManager.priceDir.ContainsKey(((int)price).ToString()))
        //    priceText.text = PurchaseManager.priceDir[((int)price).ToString()];
    }

   // public PurchaseManager purchaseManager;



    public void ButtonPressed()
    {
        //bool noConnection = Application.internetReachability == NetworkReachability.NotReachable;
        //if (noConnection)
        //{
        //    UserInterface.ShowOKDialog(LocManager.Translate("ui_connect_wifi_header"),
        //                               LocManager.Translate("ui_connect_wifi_body"),
        //                               null);

        //    GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = true;
        //}
        //else
        //{
            child.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            isbig = true;

        parent.ShowShader();
        Debug.Log("price: " + price);
        //PXY_AndroidBuy.Instance.Buy(keyProductID, "购买钥匙", price, "");
        parent.PayFailed();
#if UNITY_IOS || UNITY_IPHONE



            //print(productID);
            int productIndex = -1;
            for (int i = 0; i < purchaseManager.products.Count; i++)
            {
                if (purchaseManager.products[i].id == productID)
                    productIndex = i;
            }
            //purchaseManager.OnPurchaseClicked(productIndex);
            parent.BuyKey(price, productID, purchaseItem, purchaseDiscribe);
            //Purchase(price, productID, purchaseItem, purchaseDiscribe, "");
            string orderID = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmss"), Random.Range(0, 999999));
            parent.AllorderID = orderID;
            TDGAVirtualCurrency.OnChargeRequest(orderID, productID, price, "CNY", rewardCount, "苹果官方");
#endif

        //}
    }



    bool isbig;

    void Update()
    {
        if (child.transform.localScale.x > 1.0f && isbig)
        {
            child.transform.localScale -= new Vector3(0.02f, 0.02f, 0.02f);
            if (child.transform.localScale.x <= 1.0f)
                isbig = false;
        }
    }
}
