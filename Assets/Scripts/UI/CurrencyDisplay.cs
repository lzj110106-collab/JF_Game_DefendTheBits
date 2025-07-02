using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyDisplay : MonoBehaviour
{
    public static CurrencyDisplay instance;

    /// <summary>
    /// 进入商店的标识
    /// 0：正常进入商店
    /// 1：钥匙不足进入商店
    /// 2：饰品不足进入商店
    /// </summary>
    public static int enterShopIndex;

    [Header("Cash")]
    public Text cashDisplay;
    public GameObject cashCollectionLocator;

    public GameObject storeButtonHierarchy;
    public Button storeButton;

    [Header("Star")]
    public GameObject starHierarchy;
    public Animator starAnimator;
    public Text starsDisplay;

    [Header("Trinkets")]
    public Animator trinketAnimator;
    public Text trinketDisplay;
    public Image trinketSprite0;
    public Image trinketSprite1;

    int starCount;
    int starTotal;
     int cashAmount;
    string trinketID;

    PanelID backPanelD = PanelID.MainMenu;

    public bool forceShopScreenInEditor = true;

    public Image shopIcon;
    public Sprite shopIconCH;
    public Sprite shopIconEn;

    void Awake() {
        instance = this;
        if(Application.systemLanguage== SystemLanguage.ChineseSimplified|| Application.systemLanguage == SystemLanguage.ChineseTraditional || Application.systemLanguage == SystemLanguage.Chinese)
        {
            shopIcon.sprite = shopIconCH;
        }
        else
        {
            shopIcon.sprite = shopIconEn;
        }
    }
    void OnDestroy() { instance = null; }

    public void Start()
    {
        starCount = SaveData.TotalStarCount();
        starTotal = LevelDatabase.TotalStarsAvailable();
        cashAmount = SaveData.GetCash();

        RefreshStarDisplay();
        RefreshCashDisplay();
        HideStarDisplay();
    }

    public void Update()
    {
        int newStarCount = SaveData.TotalStarCount();
        if (newStarCount != starCount)
        {
            starCount = newStarCount;
            RefreshStarDisplay();
        }

        int newCashCount = SaveData.GetCash();
        if (newCashCount != cashAmount)
        {
            cashAmount = newCashCount;
            RefreshCashDisplay();
        }
    }

    void RefreshStarDisplay()
    {
        starsDisplay.text = starCount.ToString() + "/" + starTotal.ToString();
    }

    void RefreshCashDisplay()
    {
        cashDisplay.text = cashAmount.ToString();
    }

    public static void ShowTrinketDisplay(string trinketID)
    {
        if (instance)
        {
            if (string.IsNullOrEmpty(trinketID))
            {
                instance.trinketAnimator.gameObject.SetActive(false);
            }
            else
            {
                instance.trinketAnimator.gameObject.SetActive(true);
                instance.trinketAnimator.Play("On", 0, 0.0f);
                SetTrinketID(trinketID);
            }
        }
    }

    public static void HideTrinketDisplay()
    {
        if (instance)
        {
            instance.trinketAnimator.Play("Off", 0, 0.0f);
        }
    }

    public static void ShowStarDisplay()
    {
        if (instance)
        {
            instance.starHierarchy.SetActive(true);
            instance.starAnimator.Play("On", 0, 0.0f);
        }
    }

    public static void HideStarDisplay()
    {
        if (instance)
        {
            instance.starHierarchy.SetActive(false);
        }
    }

    public static void ShowStoreButton()
    {
        if (instance)
        {
            instance.storeButtonHierarchy.SetActive(true);
        }
    }

    public static void HideStoreButton()
    {
        if (instance)
        {
            instance.storeButtonHierarchy.SetActive(false);
        }
    }

    public static void EnableStoreButton()
    {
        if (instance)
        {
            instance.storeButton.enabled = true;
        }
    }

    public static void DisableStoreButton()
    {
        if (instance)
        {
            instance.storeButton.enabled = false;
        }
    }



    public static void SetTrinketID(string trinketID)
    {
        var trinket = TrinketDatabase.GetPrefab(trinketID);
        if (trinket != null)
        {
            var reward = trinket.GetComponent<InteractReward>();
            instance.trinketDisplay.text = SaveData.TrinketCount(trinketID).ToString();
            instance.trinketSprite0.sprite = reward.trinketIcon;
            instance.trinketSprite1.sprite = reward.trinketIcon;
            instance.trinketID = trinketID;
        }
    }

    public static GameObject GetTrinketCollectionDestination()
    {
        return instance.trinketSprite0.gameObject;
    }

    public static GameObject GetCashCollectionDestination()
    {
        return instance.cashCollectionLocator;
    }

    public static void OnTrinketAdded()
    {
        SetTrinketID(instance.trinketID); //refresh the display
        instance.trinketAnimator.Play("Add", 0, 0.0f);
    }

    public static void SetCurrencyBackPanel(PanelID _id)
    {
        CurrencyDisplay.instance.backPanelD = _id;
    }

    public void SwitchToShop()
    {
        PanelManager.Instance.SwitchToScreen(CurrencyDisplay.instance.backPanelD, PanelID.Shop);
    }

    void NoConnect()
    {
        //show connection pop-up
        UserInterface.ShowOKDialog(LocManager.Translate("ui_connect_wifi_header"),
                                   LocManager.Translate("ui_connect_wifi_body"),
                                   null);
        GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = false;
    }

    public void OnShopButtonPressed(int index)
    {
        bool noConnection = Application.internetReachability == NetworkReachability.NotReachable /*|| !IAPManager.IsInitialized()*/;
        //bool forceShop = forceShopScreenInEditor && (Application.platform == RuntimePlatform.OSXEditor ||
        //Application.platform == RuntimePlatform.WindowsEditor);


#if IOS

        if (noConnection || PurchaseManager.priceDir.Count<=0)
        {
            //if (GameObject.Find("Menu_YesNoDialog"))
            //{
                Invoke("NoConnect", 0.15f);
            //}
            //else
            //    NoConnect();
            
        }
        else
        {
            enterShopIndex = index;
            SwitchToShop();
            storeButton.GetComponent<UISceneSwitch>().SetScene();
            storeButton.GetComponent<TriggerCameraState>().TriggerCamera();
        }
#else
        enterShopIndex = index;
        SwitchToShop();
        storeButton.GetComponent<UISceneSwitch>().SetScene();
        storeButton.GetComponent<TriggerCameraState>().TriggerCamera();
#endif

    }
}
