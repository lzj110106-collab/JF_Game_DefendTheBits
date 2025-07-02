using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 本脚本中所有_1 的字段 都是观看视频的4小时一次获得奖励
/// </summary>
public class TimeManager : MonoBehaviour
{

    private static TimeManager instance;
    public static TimeManager Instance
    {
        get
        {
            return instance;
        }
    }

    public DalyClam clamPanel;

    DateTime time;
    DateTime time_1;
    DateTime time_VIP;
    string dateTime;
    bool isconnect;

    public static bool getAppointReward = false;

    public void GetDateTime()
    {
        isconnect = false;
        StartCoroutine(GetTime());
    }

    IEnumerator GetAppointReward()
    {
        WWW www = new WWW("http://param.east2west.cn/defendthebits/yuyue_reward.php");
        yield return www;

        try
        {

            main.appointRewardCount = int.Parse(www.text);
            shop.appointRewardCount = int.Parse(www.text);
            getAppointReward = true;
        }
        catch(Exception e)
        {
            main.appointRewardCount = 0;
            shop.appointRewardCount = 0;
            getAppointReward = false;

        }
    }

    public MainMenu main;
    public ShopScreen shop;

    void OnEnable()
    {
        gotTime = false;
        claimed = true;

        LocManager.Assign(dalyTime, "ui_noconnect"); 

        if (SaveData.GetDalyTime() == "null")
        {
            //print("no time");

            //说明玩家还没有领取过，是第一次进入游戏
            dalyTime.text = "00:00:00";
            claimed = false;
        }
        else
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {

                LocManager.Assign(dalyTime, "ui_noconnect");
                main.appointRewardCount = 0;
                shop.appointRewardCount = 0;
                claimed = true;
            }
            else
            {

                //claimed = true;
                StartCoroutine(GetTime());
                StartCoroutine(GetAppointReward());
            }
        }
        gotTime_1 = false;
        claimed_1 = true;

        LocManager.Assign(dalyTime_video, "ui_noconnect");
      
        if (SaveData.GetDalyTime_1() == "null")
        {
            //print("no time");
            //说明玩家还没有领取过，是第一次进入游戏
            dalyTime_video.text = "00:00:00";
            claimed_1 = false;
        }
        else
        {
           
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                LocManager.Assign(dalyTime_video, "ui_noconnect");
                //main.appointRewardCount = 0;
                //shop.appointRewardCount = 0;
                claimed_1 = true;
            }
            else
            {
                //claimed = true;
                StartCoroutine(GetTime_1());
               
            }
        }

        if (SaveData.GetVIP() == "null")
        {
            //print("no time");
            //说明玩家还没有领取过，是第一次进入游戏
            dalyTime_VIP.text = "00";
            claimed_VIP = false;
        }
        else
        {

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                LocManager.Assign(dalyTime_video, "ui_noconnect");
                //main.appointRewardCount = 0;
                //shop.appointRewardCount = 0;
                claimed_VIP = true;
            }
            else
            {
                //claimed = true;
                StartCoroutine(GetTime_VIP());

            }
        }



    }


    public IEnumerator GetTime()
    {
        //print(0);

//StopCoroutine("CDTime");
#if UNITY_IOS || UNITY_IPHONE
        if (Application.internetReachability == NetworkReachability.NotReachable)   //如果没有联网，就弹出提示框，提示不联网奖励无法领取
        {
            LocManager.Assign(dalyTime, "ui_noconnect");
            claimed = true;
        }
        else
#endif
        {
            WWW www = new WWW("http://www.hko.gov.hk/cgi-bin/gts/time5a.pr?a=1");
            // WWW www = new WWW("http://param.east2west.cn/datetime.php");

            yield return www;

            //print(www.text);
            try
            {
                timeStr = www.text.Substring(2);

            }
            catch (Exception e)
            {

            }


            /*time = DateTime.MinValue;
            startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            time = startTime.AddMilliseconds(Convert.ToDouble(timeStr));*/


            if (time == null)       //如果没有获取到网络时间，则return
                yield return null;

            try
            {
                oldtime = Convert.ToDateTime(SaveData.GetDalyTime());

            }
            catch (Exception e)
            {
          
            }

            timeRemain = oldtime.AddDays(1) - time;
            totalSeconds = timeRemain.TotalSeconds;

            gotTime = true;

            if (timeRemain.TotalSeconds <= 0)
            {
                claimed = false;
                dalyTime.text = "00:00:00";

            }
            else
            {

                tempStr.Remove(0, tempStr.ToString().Length);
                if (timeRemain.Hours < 10)
                    tempStr.Append(string.Format("0{0}:", timeRemain.Hours));
                else
                    tempStr.Append(string.Format("{0}:", timeRemain.Hours));

                if (timeRemain.Minutes < 10)
                    tempStr.Append(string.Format("0{0}:", timeRemain.Minutes));
                else
                    tempStr.Append(string.Format("{0}:", timeRemain.Minutes));

                if (timeRemain.Seconds < 10)
                    tempStr.Append(string.Format("0{0}", timeRemain.Seconds));
                else
                    tempStr.Append(string.Format("{0}", timeRemain.Seconds));

                dalyTime.text = tempStr.ToString();
                claimed = true;
            }

            //StartCoroutine(CDTime());
        }
    }

    public IEnumerator GetTime_1()
    {
        //print(0);

        //StopCoroutine("CDTime");
#if UNITY_IOS || UNITY_IPHONE
        if (Application.internetReachability == NetworkReachability.NotReachable)   //如果没有联网，就弹出提示框，提示不联网奖励无法领取
        {
            LocManager.Assign(dalyTime, "ui_noconnect");
            claimed = true;
        }
        else
#endif
        {
            WWW www = new WWW("http://www.hko.gov.hk/cgi-bin/gts/time5a.pr?a=2");
            // WWW www = new WWW("http://param.east2west.cn/datetime.php");

            yield return www;

            //print(www.text);
            try
            {
                
                timeStr_1 = www.text.Substring(2);
            }
            catch (Exception e)
            {

            }


            /*time_1 = DateTime.MinValue;
            startTime_1 = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            time_1 = startTime_1.AddMilliseconds(Convert.ToDouble(timeStr_1));*/

            if (time_1 == null)       //如果没有获取到网络时间，则return
                yield return null;

            try
            {
                oldtime_1 = Convert.ToDateTime(SaveData.GetDalyTime_1());
            }
            catch (Exception e)
            {

            }

            //oldtime = Convert.ToDateTime(SaveData.GetDalyTime());

            timeRemain_1 = oldtime_1.AddHours(4) - time_1;

            totalSeconds_1 = timeRemain_1.TotalSeconds;

            gotTime_1 = true;

            if (timeRemain_1.TotalSeconds <= 0)
            {
                claimed_1 = false;
                dalyTime_video.text = "00:00:00";
            }
            else
            {
                tempStr_1.Remove(0, tempStr_1.ToString().Length);
                if (timeRemain_1.Hours < 10)
                    tempStr_1.Append(string.Format("0{0}:", timeRemain_1.Hours));
                else
                    tempStr_1.Append(string.Format("{0}:", timeRemain_1.Hours));

                if (timeRemain_1.Minutes < 10)
                    tempStr_1.Append(string.Format("0{0}:", timeRemain_1.Minutes));
                else
                    tempStr_1.Append(string.Format("{0}:", timeRemain_1.Minutes));

                if (timeRemain_1.Seconds < 10)
                    tempStr_1.Append(string.Format("0{0}", timeRemain_1.Seconds));
                else
                    tempStr_1.Append(string.Format("{0}", timeRemain_1.Seconds));

                dalyTime_video.text = tempStr_1.ToString();
                claimed_1 = true;
            }

            //StartCoroutine(CDTime());
        }
    }

    public IEnumerator GetTime_VIP()
    {
        WWW www = new WWW("http://www.hko.gov.hk/cgi-bin/gts/time5a.pr?a=2");
        // WWW www = new WWW("http://param.east2west.cn/datetime.php");

        yield return www;

        //print(www.text);
        try
        {

            timeStr_VIP = www.text.Substring(2);
        }
        catch (Exception e)
        {

        }

        
        /*time_VIP = DateTime.MinValue;
        startTime_VIP = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        time_VIP = startTime_VIP.AddMilliseconds(Convert.ToDouble(timeStr_VIP));*/


        if (time_VIP == null)       //如果没有获取到网络时间，则return
            yield return null;
        try
        {
            oldtime_VIP = Convert.ToDateTime(SaveData.GetVIP());
        }
        catch (Exception e)
        {

        }

        timeRemain_VIP = oldtime_VIP.AddDays(30) - time_VIP;

        totalSeconds_VIP = timeRemain_VIP.TotalSeconds;

        gotTime_VIP = true;

        if (timeRemain_VIP.TotalSeconds <= 0)
        {
            claimed_VIP = false;
            dalyTime_VIP.text = "00";
        }
        else
        {
            tempStr_VIP.Remove(0, tempStr_VIP.ToString().Length);
            if (timeRemain_VIP.Days < 10)
                tempStr_VIP.Append(string.Format("0{0}:", timeRemain_VIP.Days));
            else
                tempStr.Append(string.Format("{0}:", timeRemain_VIP.Days));
            if (timeRemain.Hours < 10)
                tempStr_VIP.Append(string.Format("0{0}:", timeRemain_VIP.Hours));
            else
                tempStr_VIP.Append(string.Format("{0}:", timeRemain_VIP.Hours));
            if (timeRemain.Minutes < 10)
                tempStr_VIP.Append(string.Format("0{0}:", timeRemain_VIP.Minutes));
            else
                tempStr_VIP.Append(string.Format("{0}:", timeRemain_VIP.Minutes));
            if (timeRemain.Seconds < 10)
                tempStr_VIP.Append(string.Format("0{0}", timeRemain_VIP.Seconds));
            else
                tempStr_VIP.Append(string.Format("{0}", timeRemain_VIP.Seconds));
            dalyTime_VIP.text = tempStr_VIP.ToString();
            claimed_VIP = true;
        }      
    }

    string timeStr;
    string timeStr_1;
    string timeStr_VIP;

 
    DateTime oldtime;
    DateTime oldtime_1;
    DateTime oldtime_VIP;

    DateTime startTime;
    DateTime startTime_1;
    DateTime startTime_VIP;


    TimeSpan timeRemain;
    TimeSpan timeRemain_1;
    TimeSpan timeRemain_VIP;
    StringBuilder tempStr = new StringBuilder();
    StringBuilder tempStr_1 = new StringBuilder();
    StringBuilder tempStr_VIP = new StringBuilder();

    DateTime tempTime = new DateTime(1970, 1, 1);
    DateTime tempTime_1 = new DateTime(1970, 1, 1);
    DateTime tempTime_VIP = new DateTime(1970, 1, 1);

    public static double totalSeconds;
    public static double totalSeconds_1;
    public static double totalSeconds_VIP;

    public static bool gotTime;
    public static bool gotTime_1;
    public static bool gotTime_VIP;
    public Text dalyTime;

    string oldDalyNmae = "每日奖励";
    string newDalyNmae = "VIP每日奖励";
    bool isvipFlag = true;
    public Text daly_Name;
    //Text daly_newName;
    public Text dalyTime_video;
    public Text dalyTime_VIP;
    public int time_count = 0;
    public static bool claimed;
    public static bool claimed_1;
    public static bool claimed_VIP;

    public void Awake()
    {
        instance = this;
        time_count = 0;
        Debug.Log("SaveData.GetPlayerVIP() : " + SaveData.GetPlayerVIP());
        if (SaveData.GetPlayerVIP() == true)
        {
            daly_Name.text = newDalyNmae;
        }
        else
        {
            daly_Name.text = oldDalyNmae;
        }

    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            //print("get focus!");
            gotTime = false;
            StartCoroutine(GetTime());
           
        }
        if (hasFocus)
        {
            //print("get focus!");
            gotTime_1 = false;
           
            StartCoroutine(GetTime_1());
        }
        if (hasFocus)
        {
            //print("get focus!");
            gotTime_1 = false;
            StartCoroutine(GetTime_VIP());
        }
    }

    void Update()
    {
        if (isvipFlag == true)
        {
            if (SaveData.GetPlayerVIP() == true)
            {
                daly_Name.text = newDalyNmae;
                isvipFlag = false;
            }
            else
            {
                daly_Name.text = oldDalyNmae;
            }
        }



        if (claimed)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable || !gotTime)
            {
                LocManager.Assign(dalyTime, "ui_noconnect");
                return;
            }

            totalSeconds -= Time.deltaTime;
            //print(totalSeconds);
            if (totalSeconds <= 0)
            {
                claimed = false;
                dalyTime.text = "00:00:00";
            }
            else
            {
                try
                {
                    tempTime = tempTime.AddSeconds(totalSeconds);
                }
                catch(Exception e)
                {

                }
                
                tempStr.Remove(0, tempStr.ToString().Length);
                if (totalSeconds / 3600 < 10)
                    tempStr.Append(string.Format("0{0}:", (int)totalSeconds / 3600));
                else
                    tempStr.Append(string.Format("{0}:", (int)totalSeconds / 3600));

                if ((totalSeconds % 3600) / 60 < 10)
                    tempStr.Append(string.Format("0{0}:", (int)(totalSeconds % 3600) / 60));
                else
                    tempStr.Append(string.Format("{0}:", (int)(totalSeconds % 3600) / 60));

                if ((totalSeconds % 3600) % 60 < 10)
                    tempStr.Append(string.Format("0{0}", (int)(totalSeconds % 3600) % 60));
                else
                    tempStr.Append(string.Format("{0}", (int)(totalSeconds % 3600) % 60));

                dalyTime.text = tempStr.ToString();
                claimed = true;
            }
        }

        if (claimed_1)
        {
           
            if (Application.internetReachability == NetworkReachability.NotReachable || !gotTime_1)
            {
                LocManager.Assign(dalyTime_video, "ui_noconnect");
                return;
            }

            totalSeconds_1 -= Time.deltaTime;
            //print(totalSeconds);
            if (totalSeconds_1 <= 0)
            {
                claimed_1 = false;
                dalyTime_video.text = "00:00:00";
              
            }
            else
            {
                try
                {
                    tempTime_1 = tempTime_1.AddSeconds(totalSeconds_1);
                  
                }
                catch (Exception e)
                {

                }

                tempStr_1.Remove(0, tempStr_1.ToString().Length);
                if (totalSeconds_1 / 3600 < 10)
                    tempStr_1.Append(string.Format("0{0}:", (int)totalSeconds_1 / 3600));
                else
                    tempStr_1.Append(string.Format("{0}:", (int)totalSeconds_1 / 3600));

                if ((totalSeconds_1 % 3600) / 60 < 10)
                    tempStr_1.Append(string.Format("0{0}:", (int)(totalSeconds_1 % 3600) / 60));
                else
                    tempStr_1.Append(string.Format("{0}:", (int)(totalSeconds_1 % 3600) / 60));

                if ((totalSeconds_1 % 3600) % 60 < 10)
                    tempStr_1.Append(string.Format("0{0}", (int)(totalSeconds_1 % 3600) % 60));
                else
                    tempStr_1.Append(string.Format("{0}", (int)(totalSeconds_1 % 3600) % 60));

                dalyTime_video.text = tempStr_1.ToString();
                claimed_1 = true;
            }
        }

        if (claimed_VIP)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable || !gotTime_1)
            {
                LocManager.Assign(dalyTime_video, "ui_noconnect");
                return;
            }

            totalSeconds_VIP -= Time.deltaTime;
           
            if (totalSeconds_VIP <= 0)
            {
                claimed_VIP = false;
                dalyTime_VIP.text = "00";
            }
            else
            {
                try
                {
                    tempTime_VIP = tempTime_VIP.AddDays(totalSeconds_VIP);
                }
                catch (Exception e)
                {

                }
                int days = (int)(totalSeconds_VIP / 3600) / 24;
                string dd = days < 10 ? "0" + days : days.ToString();
                dalyTime_VIP.text = dd ;
                time_count = days;               
              
                claimed_VIP = true;
            }
        }
    }



    public void ShowClamPanel()
    {
        if (SaveData.GetDalyTime().Equals(dateTime))
            return;
        else
            StartCoroutine(ShowClamPanel1());
    }
    public IEnumerator ShowClamPanel1()
    {
        yield return new WaitForSeconds(0.5f);
        //clamPanel.ShowClamPanel(isconnect);
    }

    //void OnApplicationFocus(bool hasFocus)
    //{
    //    if (hasFocus)
    //    {
    //        print("获取焦点");
    //        StartCoroutine(GetTime());
    //    }
    //}
}
