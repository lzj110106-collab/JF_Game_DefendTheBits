using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VIPScreen : MonoBehaviour
{
    private static VIPScreen instance;
    public static VIPScreen Instance
    {

       get
        {
            return instance;
        }

    }

    public GameObject shader;
    public Text buyConfirmText;
    public Text tx;

    Button vipButton;
    Text vipText;

    //---------------
    //public GameObject VIPShader_pxy;
    public ParticleSystem VIPPS;
    public GameObject menu;



    public void closeVIPShader_pxy()
    {

        SaveData.SetVIP(System.DateTime.Now.ToString());
        Debug.Log("SaveData.SetVIP(System.DateTime.Now.ToString()) : " + System.DateTime.Now.ToString());
        TimeManager.claimed_VIP = true;
        TimeManager.totalSeconds_VIP = 30 * 86400;
        TimeManager.gotTime_VIP = true;
        //VIPShader_pxy.SetActive(false);
        menu.SetActive(true);
    }
    public void testGetVIP()      
    {
        if (TimeManager.claimed_1 || Application.internetReachability == NetworkReachability.NotReachable)
            return;
        menu.SetActive(false);
        //VIPShader_pxy.SetActive(true);
        VIPPS.Play();
    }

    IEnumerator PlayParticleSystem(ParticleSystem ps)
    {
        yield return new WaitForSeconds(0.0f);

        VIPPS.Play();
    }
    //---------------

    void OnEnable()
    {
        //if (PurchaseManager.priceDir.ContainsKey("48"))
        //{
        //    LocManager.Assign(buyConfirmText, "ui_by7privilegeswers", PurchaseManager.priceDir["48"]);
        //    LocManager.Assign(tx, "ui_vipdescription", PurchaseManager.priceDir["48"]);
        //}
        //else
        //{
        //    LocManager.Assign(buyConfirmText, "ui_connect_wifi_body");
        //    LocManager.Assign(tx, "ui_connect_wifi_body");
        //}

        if (SaveData.GetPlayerVIP())
        {
            
            vipButton.interactable = false;
            vipText.text = "已是尊贵的VIP";
        }
    }
    private void Awake()
    {
        instance = this;
        vipButton = GameObject.Find("BuyBtn").GetComponent<Button>();
        vipText = GameObject.Find("Text").GetComponent<Text>();
    }
    private void Update()
    {
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


    public void StartBuy()
    {
        shader.SetActive(true);
    }

    public void EndBuy()
    {
        shader.SetActive(false);
    }

    public GameObject PrivacyPolicyContent_ipad;
    public GameObject TermsOfServiceContent_ipad;

    public GameObject PrivacyPolicyContent_iphone;
    public GameObject TermsOfServiceContent_iphone;

    public GameObject PrivacyPolicyContent_iphonex;
    public GameObject TermsOfServiceContent_iphonex;

    public void PrivacyPolicyPressed()
    {
        //print("打开隐私政策");
        //Application.OpenURL("http://east2west.cn/statement/term.html");

        //print((float)Screen.width / (float)Screen.height);

        if ((float)Screen.width / (float)Screen.height < 1.4f)
            PrivacyPolicyContent_ipad.SetActive(true);
        else if ((float)Screen.width / (float)Screen.height >= 2.0f)
            PrivacyPolicyContent_iphonex.SetActive(true);
        else
            PrivacyPolicyContent_iphone.SetActive(true);
    }

    public void TermsOfServicePressed()
    {
        //print("服务条款");
        //Application.OpenURL("http://east2west.cn/statement/privacy_policy.html");

        //print((float)Screen.width / (float)Screen.height);

        if ((float)Screen.width / (float)Screen.height < 1.4f)
            TermsOfServiceContent_ipad.SetActive(true);
        else if ((float)Screen.width / (float)Screen.height >= 2.0f)
            TermsOfServiceContent_iphonex.SetActive(true);
        else
            TermsOfServiceContent_iphone.SetActive(true);
    }

    public void PrivacyPolicyClose()
    {
        //print("打开隐私政策");
        //Application.OpenURL("http://east2west.cn/statement/term.html");
        if ((float)Screen.width / (float)Screen.height < 1.4f)
            PrivacyPolicyContent_ipad.SetActive(false);
        else if ((float)Screen.width / (float)Screen.height >= 2.0f)
            PrivacyPolicyContent_iphonex.SetActive(false);
        else
            PrivacyPolicyContent_iphone.SetActive(false);
    }

    public void TermsOfServiceClose()
    {
        //print("服务条款");
        //Application.OpenURL("http://east2west.cn/statement/privacy_policy.html");
        if ((float)Screen.width / (float)Screen.height < 1.4f)
            TermsOfServiceContent_ipad.SetActive(false);
        else if ((float)Screen.width / (float)Screen.height >= 2.0f)
            TermsOfServiceContent_iphonex.SetActive(false);
        else
            TermsOfServiceContent_iphone.SetActive(false);
    }

    public void BuyMonthlyCard()
    {


        int count = TimeManager.Instance.time_count;
        if (count <= 0)
        {
            //PXY_AndroidBuy.Instance.Buy("com.east2west.defendthebits.BuyMonthlyCard", "购买月卡", 48.00f, "");
            //isbuy = false;
            //TimeManager.Instance.time_count = 0;
        }
        else { return; }

       

    }

   
    public  void CardisBuy()
    {
        vipButton.interactable = false;
        vipText.text = "已是尊贵的VIP";
        closeVIPShader_pxy();
        TimeManager.Instance.dalyTime.text = "00:00:00";
        TimeManager.claimed = false;
        DalyClam.Instance.ShowClamPanel();
    }

  
}
