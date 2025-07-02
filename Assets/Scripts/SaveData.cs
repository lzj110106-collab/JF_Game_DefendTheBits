using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Linq;

public enum SaveStateHeader
{
    LevelName,
    WaveNumber,
    GoldRemaining,
    LivesRemaining
}

public enum SaveStateTowerData
{
    Type,
    UpgradeLevel,
    TileX,
    TileY,
    PlotRotation,
    StatTracking //kill count, coins collected, etc
}

public class SaveData
{
    static int INITIAL_CASH = 0;

    public static int WavesComplete(LevelData data)
    {
        //return PlayerPrefs.GetInt(data.identifier + "_waves_complete", 0);

        return ObscuredPrefs.GetInt(data.identifier + "_waves_complete", 0);
    }

    public static void SetWavesComplete(LevelData data, int waves)
    {
        //PlayerPrefs.SetInt(data.identifier + "_waves_complete", waves);

        ObscuredPrefs.SetInt(data.identifier + "_waves_complete", waves);
    }

    public static int StarRating(LevelData data)
    {
        //return PlayerPrefs.GetInt(data.identifier + "_stars", 0);

        return ObscuredPrefs.GetInt(data.identifier + "_stars", 0);
    }

    public static void SetStarRating(LevelData data, int rating)
    {
        //PlayerPrefs.SetInt(data.identifier + "_stars", rating);

        ObscuredPrefs.SetInt(data.identifier + "_stars", rating);
    }

    public static int TotalStarCount()
    {
        //return PlayerPrefs.GetInt("total_stars", 0);

        return ObscuredPrefs.GetInt("total_stars", 0);
    }

    public static void SetTotalStarCount(int total)
    {
        //PlayerPrefs.SetInt("total_stars", total);

        ObscuredPrefs.SetInt("total_stars", total);
    }

    public static bool HasReceivedReward(LevelData data)
    {
        //return PlayerPrefs.GetInt(data.identifier + "_reward_given", 0) == 1;

        return ObscuredPrefs.GetInt(data.identifier + "_reward_given", 0) == 1;
    }

    public static void ReceivedReward(LevelData data)
    {
        //PlayerPrefs.SetInt(data.identifier + "_reward_given", 1);

        ObscuredPrefs.SetInt(data.identifier + "_reward_given", 1);
    }

    #region SAVE STATE

    public static bool WasGameInProgress()
    {
        //return PlayerPrefs.GetString("save_state", "") != "";

        return ObscuredPrefs.GetString("save_state", "") != "";
    }

    public static string[] GetSaveStateHeader()
    {
        //return CSVUtil.Tokenise(PlayerPrefs.GetString("save_state", ""));

        return CSVUtil.Tokenise(ObscuredPrefs.GetString("save_state", ""));
    }

    public static void ClearSaveState()
    {
        //PlayerPrefs.SetString("save_state", "");

        ObscuredPrefs.SetString("save_state", "");
    }

    #endregion

    #region META

    public static void AddCash(int cash)
    {
        //PlayerPrefs.SetInt("cash", GetCash() + cash);

        ObscuredPrefs.SetInt("cash", GetCash() + cash);
    }

    public static int GetCash()
    {
        //return PlayerPrefs.GetInt("cash", INITIAL_CASH);

        return ObscuredPrefs.GetInt("cash", INITIAL_CASH);
    }

    public static void UnlockTower(string identifier)
    {
        //PlayerPrefs.SetInt(identifier + "_is_unlocked", 1);
        //Debug.Log("有新的防御塔解锁:"+identifier);
        ObscuredPrefs.SetInt(identifier + "_is_unlocked", 1);
    }

    public static bool IsTowerUnlocked(string identifier)
    {
        //return PlayerPrefs.GetInt(identifier + "_is_unlocked", 0) == 1;

        return ObscuredPrefs.GetInt(identifier + "_is_unlocked", 0) == 1;
    }

    public static void LockTower(string identifier)
    {
        ObscuredPrefs.SetInt(identifier + "_is_unlocked", 0);
    }



    #endregion

    #region UPGRADES

    public static int GetUpgradeLevel(string towerName)
    {
        //return PlayerPrefs.GetInt(towerName + "_level", 0);

        return ObscuredPrefs.GetInt(towerName + "_level", 0);
    }

    public static void SetUpgradeLevel(string towerName, int level)
    {
        //PlayerPrefs.SetInt(towerName + "_level", level);

        ObscuredPrefs.SetInt(towerName + "_level", level);
    }

    public static bool IsUpgradeUnlocked(string towerName, int level)
    {
        return GetUpgradeLevel(towerName) > level;
    }

    #endregion

    #region STATS

    //returns true if this was the first kill
    public static bool AddKill(MasterEnemyTable.Entry enemy)
    {
        var key = enemy.identifier + "_kills";
        //int previousKillCount = PlayerPrefs.GetInt(key, 0);
        //PlayerPrefs.SetInt(key, previousKillCount + 1);
        int previousKillCount = ObscuredPrefs.GetInt(key, 0);
        ObscuredPrefs.SetInt(key, previousKillCount + 1);

        return previousKillCount == 0;
    }

    public static int KillCount(string enemyIdentifier)
    {
        //return PlayerPrefs.GetInt(enemyIdentifier + "_kills", 0);

        return ObscuredPrefs.GetInt(enemyIdentifier + "_kills", 0);
    }

    public static void AddTrinket(string trinketId, int count = 1)
    {
        //PlayerPrefs.SetInt(trinketId + "_count", TrinketCount(trinketId) + count);
        //Debug.Log(TrinketCount(trinketId) + count);
        ObscuredPrefs.SetInt(trinketId + "_count", TrinketCount(trinketId) + count);
    }

    public static int TrinketCount(string trinketId)
    {
        //return PlayerPrefs.GetInt(trinketId + "_count", 0);
        if(ObscuredPrefs.GetInt(trinketId + "_count", 0) <= 0)
        {
            ObscuredPrefs.SetInt(trinketId + "_count", 0);
            return 0;
        }
        else
        {
            return ObscuredPrefs.GetInt(trinketId + "_count");
        }
    }

    #endregion

    #region HINTS

    public static bool ShowEnemyHint(string enemyIdentifier)
    {
        //		Debug.Log("show enemy hint? " + enemyIdentifier + " " + (PlayerPrefs.GetInt("hint_" + enemyIdentifier, 1) == 1));
        //return PlayerPrefs.GetInt("hint_" + enemyIdentifier, 1) == 1;

        return ObscuredPrefs.GetInt("hint_" + enemyIdentifier, 1) == 1;
    }

    public static void ClearEnemyHint(string enemyIdentifier)
    {
        //PlayerPrefs.SetInt("hint_" + enemyIdentifier, 0);
        //PlayerPrefs.SetInt("show_codex_alert", 1);

        ObscuredPrefs.SetInt("hint_" + enemyIdentifier, 0);
        ObscuredPrefs.SetInt("show_codex_alert", 1);
        //		Debug.Log("clear enemy hint " + enemyIdentifier);
    }

    #endregion

    #region RESET

    public static void ResetSaveData()
    {
        //preserve language settings.
        var currentLanguage = LocManager.CurrentLanguage();

        //PlayerPrefs.DeleteAll();

        //PlayerPrefs.SetInt(LocManager.LANGUAGE_KEY, (int)currentLanguage);
        //PlayerPrefs.SetInt(LocManager.AUTO_DETECT_KEY, 0);


        ObscuredPrefs.DeleteAll();

        ObscuredPrefs.SetInt(LocManager.LANGUAGE_KEY, (int)currentLanguage);
        ObscuredPrefs.SetInt(LocManager.AUTO_DETECT_KEY, 0);

        //unlock default towers
        foreach (var kv in TowerLoader.instance.towerInfoByPrefab)
        {
            string trinketID = "";
            int trinketCount = 0;

            //checking for trinket unlocks
            if (TowerLoader.UnlockRequirements(kv.Key, out trinketID, out trinketCount))
                continue;

            //checking for achievement unlocks
            var towerID = TowerLoader.GetTowerID(kv.Key);
            TowerInfo info = TowerLoader.GetTowerInfo(towerID.First().ToString().ToUpper()+ towerID.Substring(1), 0);

            if (info.cash.isCash || info.vip.isVIP)
                continue;

            if (AchievementDatabase.unlockableTowers.ContainsKey(towerID))
                continue;

            //checking for stage rewards
            if (LevelDatabase.unlockableTowers.ContainsKey(towerID))
                continue;

            //if we get here its a default tower
            SaveData.UnlockTower(kv.Value[0].name);
        }

        Debug.Log("RESET SAVE DATA");

        //make sure to clear this flag otherwise things go weird 
        //with the debug keys for unlocks and stuff.
        //PlayerPrefs.SetInt("first_load", 0);

        ObscuredPrefs.SetInt("first_load", 0);
    }

    public static void SkipFTUE()
    {
        var data = LevelDatabase.GetLevelData("FTUE");
        for (var i = 0; i < data.towerRewards.Length; ++i)
            if (!string.IsNullOrEmpty(data.towerRewards[i]))
                SaveData.UnlockTower(data.towerRewards[i]);

        //PlayerPrefs.SetInt("show_ftue", 0);

        ObscuredPrefs.SetInt("show_ftue", 0);
    }

    #endregion

    #region
    /// <summary>
    /// 保存免费试用的防御塔
    /// </summary>
    /// <param name="id">Identifier.</param>
    public static void SetFreeTower(string id)
    {
        ObscuredPrefs.SetString("RandomTower", id);
    }

    /// <summary>
    /// 获取免费使用的防御塔
    /// </summary>
    /// <returns>The free tower.</returns>
    public static string GetFreeTower()
    {
        return ObscuredPrefs.GetString("RandomTower");
    }

    /// <summary>
    /// 设置上次领取的时间
    /// </summary>
    /// <param name="datetime">Datetime.</param>
    public static void SetDalyTime(string datetime)
    {
        ObscuredPrefs.SetString("LastClamTime", datetime);
    }

    /// <summary>
    /// 获取上次领取的时间
    /// </summary>
    /// <returns></returns>
    public static string GetDalyTime()
    {
        return ObscuredPrefs.GetString("LastClamTime","null");
    }

    /// <summary>
    /// 设置上次领取的时间 观看视频
    /// </summary>
    /// <param name="datetime">Datetime.</param>
    public static void SetDalyTime_1(string datetime)
    {
        ObscuredPrefs.SetString("LastClamTime_1", datetime);
    }

    /// <summary>
    /// 获取上次领取的时间   观看视频
    /// </summary>
    /// <returns></returns>
    public static string GetDalyTime_1()
    {
        return ObscuredPrefs.GetString("LastClamTime_1", "null");
    }

    /// <summary>
    /// VIP
    /// </summary>
    /// <param name="datetime">Datetime.</param>
    public static void SetVIP(string datetime)
    {
        ObscuredPrefs.SetString("LastVIP", datetime);
    }

    /// <summary>
    /// 获取上次领取的时间   观看视频
    /// </summary>
    /// <returns></returns>
    public static string GetVIP()
    {
        return ObscuredPrefs.GetString("LastVIP", "null");
    }

    /// <summary>
    /// 设置下一次礼包显示的时间
    /// </summary>
    /// <param name="datetime"></param>
    public static void SetNextGiftbagTime(string datetime)
    {
        ObscuredPrefs.SetString("NextGiftbagTime", datetime);
    }

    /// <summary>
    /// 获取下一次礼包显示的时间
    /// </summary>
    /// <param name="datetime"></param>
    /// <returns></returns>
    public static string GetNextGiftbagTime()
    {
        //ObscuredPrefs.DeleteKey("NextGiftbagTime");
        return ObscuredPrefs.GetString("NextGiftbagTime", null);
    }

    /// <summary>
    /// 获取礼包的购买情况
    /// 未购买的礼包状态为可购买
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool GetItem(int index)
    {
        return ObscuredPrefs.GetBool("gift" + index, true);
    }

    /// <summary>
    /// 设置礼包的购买情况
    /// 已购买的礼包不再可购买
    /// </summary>
    /// <param name="index"></param>
    public static void SetItem(int index,bool isvaliable)
    {
        ObscuredPrefs.SetBool("gift" + index, isvaliable);
    }

    /// <summary>
    /// 设置玩家订阅的凭证
    /// </summary>
    /// <param name="recipt"></param>
    public static void SetSubRecipt(string recipt)
    {
        ObscuredPrefs.SetString("subrecipt", recipt);
    }

    /// <summary>
    /// 获取玩家订阅的凭证
    /// </summary>
    public static string GetSubRecipt()
    {
        return ObscuredPrefs.GetString("subrecipt",null);
    }

    public static void SetPlayerVIP(bool isVIP)
    {
        ObscuredPrefs.SetBool("isVIP", isVIP);
    }

    public static bool GetPlayerVIP()
    {
        return ObscuredPrefs.GetBool("isVIP",false);
    }


    public static void SetTowerBuy(string towerName,bool isBuy)
    {
        ObscuredPrefs.SetBool(towerName, isBuy);
    }

    public static bool GetTowerBuy(string towerName)
    {
        return ObscuredPrefs.GetBool(towerName, false);
    }


    public static void SetRemainTime(int second)
    {
        ObscuredPrefs.SetInt("remainTime", second);
    }

    public static int GetRemainTime()
    {
        return ObscuredPrefs.GetInt("remainTime");
    }


    public static bool IsPaid()
    {
        return ObscuredPrefs.GetBool("ispaid", false);
    }

    public static void SetIsPaid(bool isPaid)
    {
        ObscuredPrefs.SetBool("ispaid", isPaid);
    }

    public static void SetAppointReward(int getCount)
    {
        ObscuredPrefs.SetInt("AppointReward", getCount);
    }

    public static int GetAppointReward()
    {
        return ObscuredPrefs.GetInt("AppointReward", 0);
    }

    public static void SetAppointRewardStatue(int index)
    {
        ObscuredPrefs.SetBool("AppointReward_"+index, true);
    }

    public static bool GetAppointRewardStatue(int index)
    {
        return ObscuredPrefs.GetBool("AppointReward_" + index);
    }

    #endregion

    public static bool ShouldShowCodexAlert()
    {
        //return PlayerPrefs.GetInt("show_codex_alert", 0) == 1;

        return ObscuredPrefs.GetInt("show_codex_alert", 0) == 1;
    }

    public static void ClearCodexAlert()
    {
        //PlayerPrefs.SetInt("show_codex_alert", 0);
        //PlayerPrefs.SetInt("show_codex_alert", 0);
        ObscuredPrefs.SetInt("show_codex_alert", 0);
    }
}
