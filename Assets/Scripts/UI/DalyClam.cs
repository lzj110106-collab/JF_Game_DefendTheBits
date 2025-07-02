using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 每日领取
/// </summary>
public class DalyClam : MonoBehaviour
{

    private static DalyClam instance;
    public static DalyClam Instance
    {

        get
        {
            return instance;
        }

    }

    public GameObject rewardContent;
    public GameObject towerContent;
    public GameObject noConnectContent;
    public Text cantClam;
    public Text clamText;
    public Text randomTowerName;
    public Image randomTowerIcon;
    private string towerID;
    [HideInInspector]
    public string datetime;

    bool isClick = false;
    /// <summary>
    /// 显示领取面板
    /// </summary>
    public void ShowClamPanel()
    {
        //print(MainMenu.claimed);
        if (TimeManager.claimed|| Application.internetReachability == NetworkReachability.NotReachable)
            return;

        isClick = false;

        gameObject.SetActive(true);
        GetComponent<PanelNotifier>().TransitionAll();
        GetComponent<Animator>().SetTrigger("on");

        rewardContent.SetActive(false);
        towerContent.SetActive(false);


        noConnectContent.SetActive(false);
        if (SaveData.GetPlayerVIP())
        {   //如果是VIP，显示出今天领取到的奖励
            rewardContent.SetActive(true);
        }

        if (TowerLoader.lockedTower.Count > 0)
        {   //如果还有未解锁的非付费防御塔，随机一个
            towerContent.SetActive(true);

            int randomTower = UnityEngine.Random.Range(0, TowerLoader.lockedTower.Count);
            towerID = TowerLoader.lockedTower[randomTower].towerName;
            randomTowerName.text = LocManager.Translate("ui_randomdefensetower") + LocManager.Translate("tower_name_" + TowerLoader.lockedTower[randomTower].towerName.ToLower());
            randomTowerIcon.sprite = TowerLoader.lockedTower[randomTower].icon;
        }
        LocManager.Assign(clamText, "ui_claim");
    }

    /// <summary>
    /// 领取奖励
    /// </summary>
    public void ClamButtonClick()
    {
        if (!isClick)
        {
            if (rewardContent.activeSelf)
                SaveData.AddCash(150);
            if (towerContent.activeSelf)
                SaveData.SetFreeTower(towerID);

            SaveData.SetDalyTime(System.DateTime.Now.ToString());
            TimeManager.claimed = true;
            TimeManager.totalSeconds = 24 * 3600;
            TimeManager.gotTime = true;
            ClosePanel();

            isClick = true;
        }


        //if (Application.internetReachability == NetworkReachability.NotReachable)   //如果没有联网，就存储当前系统的时间
        //{
        //    //CantClam();

        //    if (rewardContent.activeSelf)
        //        SaveData.AddCash(150);
        //    if (towerContent.activeSelf)
        //        SaveData.SetFreeTower(towerID);

        //    SaveData.SetDalyTime(System.DateTime.Now.ToString());
        //    MainMenu.claimed = true;
        //    ClosePanel();
        //}
        //else
        //{
        //    StartCoroutine(ClamSaveTime());
        //}

        //if (SaveData.GetPlayerVIP())
        //{
        //    Dictionary<string, object> buyTowers = new Dictionary<string, object>();
        //    buyTowers.Add("BuyTowers", string.Format("购买防御塔:{0}", tower.towerName));
        //    TalkingDataGA.OnEvent("BuyTowers", buyTowers);
        //}
    }

    public void ClosePanel()
    {
        GetComponent<Animator>().SetTrigger("off");
        GetComponent<Panel>().NavigateBack();
    }

    //void CantClam()
    //{
    //    cantClam.gameObject.SetActive(true);
    //    Invoke("HideCantClam", 3.0f);
    //}

    private void Awake()
    {
        instance = this;
    }

    IEnumerator ClamSaveTime()
    {
        WWW www = new WWW("http://www.hko.gov.hk/cgi-bin/gts/time5a.pr?a=1");
        //WWW www = new WWW("http://param.east2west.cn/datetime.php");

        yield return www;
        string timeStr = "";
        try
        {
            timeStr = www.text.Substring(2);
        }
        catch (Exception e) { }


        DateTime time = DateTime.MinValue;
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        time = startTime.AddMilliseconds(Convert.ToDouble(timeStr));

        if (time == null)       //如果没有获取到网络时间，则存储本地时间
        {
            //CantClam();
            if (rewardContent.activeSelf)
                SaveData.AddCash(150);
            if (towerContent.activeSelf)
                SaveData.SetFreeTower(towerID);

            SaveData.SetDalyTime(System.DateTime.Now.ToString());
            TimeManager.claimed = true;
            ClosePanel();

            yield return null;
        }


        if (rewardContent.activeSelf)
            SaveData.AddCash(150);
        if (towerContent.activeSelf)
            SaveData.SetFreeTower(towerID);

        SaveData.SetDalyTime(time.ToString());
        TimeManager.claimed = true;
        ClosePanel();
    }

   
}
