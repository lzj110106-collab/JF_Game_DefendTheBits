using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;

public class Achievement
{
    public enum Type { Defeat, Collect, CompleteLevel, Upgrade }; //order matches UI layout
    public enum RewardType { Cash, Trinket, Tower };

    public string guid;
    public string headerID;
    public string bodyID;

    public Type type;
    public string subType;
    public string levelID;
    public string towerID;
    public int count;

    public RewardType rewardType;
    public string rewardTypeName;
    public int rewardCount;

    public bool isCompleted;
    public bool isCollected;

    public string Description()
    {
        if (type == Type.Defeat || type == Type.Collect)
            return LocManager.BuildString(bodyID, count);

        return LocManager.Translate(bodyID);
    }

    public void AssignDescription(Text target)
    {
        if (type == Type.Defeat || type == Type.Collect)
            LocManager.Assign(target, bodyID, count);
        else
            LocManager.Assign(target, bodyID);
    }
}

public class AchievementDatabase : MonoBehaviour
{
    public TextAsset csv;
    public float cashRate;
    public static Dictionary<string, Achievement> achievements;
    public static List<Achievement> orderedAchievements;

    //this maps tower ID to achievement ID.
    public static Dictionary<string, string> unlockableTowers;

    void Awake()
    {
        achievements = new Dictionary<string, Achievement>();
        orderedAchievements = new List<Achievement>();
        unlockableTowers = new Dictionary<string, string>();


        var lines = CSVUtil.Lines(csv.text);
        for (var i = 0; i < lines.Length; ++i)
        {
            var tokens = CSVUtil.Tokenise(lines[i]);
            if (CSVUtil.SkipLine(tokens))
                continue;

            int j = 0;

            var data = new Achievement();
            data.guid = CSVUtil.ParseString(tokens, j++, "ach");
            data.headerID = CSVUtil.ParseString(tokens, j++, "");
            data.bodyID = CSVUtil.ParseString(tokens, j++, "");

            var typeString = CSVUtil.ParseString(tokens, j++, "complete_level");
            data.type = typeString == "complete_level" ? Achievement.Type.CompleteLevel :
                        typeString == "defeat" ? Achievement.Type.Defeat :
                        typeString == "collect" ? Achievement.Type.Collect :
                        Achievement.Type.Upgrade;

            data.subType = CSVUtil.ParseString(tokens, j++, "");
            data.levelID = CSVUtil.ParseString(tokens, j++, "");
            data.towerID = CSVUtil.ParseString(tokens, j++, "");
            data.count = CSVUtil.ParseInt(tokens, j++, 1);

            data.rewardType = Achievement.RewardType.Cash;
            data.rewardTypeName = CSVUtil.ParseString(tokens, j++, "cash");

            if (TrinketDatabase.GetPrefab(data.rewardTypeName) != null)
                data.rewardType = Achievement.RewardType.Trinket;


            if (TowerLoader.GetTowerPrefab(data.rewardTypeName) != null)
            {
                data.rewardType = Achievement.RewardType.Tower;
                //加到不可解锁的列表中
                unlockableTowers.Add(data.rewardTypeName, data.guid);
                //				Debug.Log("[AchievementDatabase] " + data.guid + " unlocks tower: " + data.rewardTypeName);
            }

            if (data.rewardType == Achievement.RewardType.Cash)
                data.rewardCount = (int)(CSVUtil.ParseInt(tokens, j++, 1) * cashRate);
            else
                data.rewardCount = CSVUtil.ParseInt(tokens, j++, 1);

            if (achievements.ContainsKey(data.guid))
            {
                Debug.Log("[AchievementsDatabase] duplicate ID: " + data.guid);
            }
            else
            {
                //data.isCompleted = PlayerPrefs.GetInt(data.guid + "_complete", 0) == 1;
                //data.isCollected = PlayerPrefs.GetInt(data.guid + "_collected", 0) == 1;

                data.isCompleted = ObscuredPrefs.GetInt(data.guid + "_complete", 0) == 1;
                data.isCollected = ObscuredPrefs.GetInt(data.guid + "_collected", 0) == 1;

                achievements.Add(data.guid, data);
                orderedAchievements.Add(data);
            }
        }

        // Refreshes achievements for any balancing unexpected balancing changes
        RefreshAchievements();
    }

    public static void UpgradeTower(string towerID, int upgradeLevel)
    {
        foreach (var kv in achievements)
        {
            var data = kv.Value;

            if (data.type == Achievement.Type.Upgrade &&
                data.subType == towerID &&
                !data.isCompleted)
            {
                if (upgradeLevel >= data.count)
                    OnAchievementCompleted(data);

                //PlayerPrefs.SetInt(data.guid + "_count", upgradeLevel);

                ObscuredPrefs.SetInt(data.guid + "_count", upgradeLevel);

                return;
            }
        }
    }

    public static void LevelComplete(string levelID)
    {
        foreach (var kv in achievements)
        {
            var data = kv.Value;

            if (data.type == Achievement.Type.CompleteLevel && !data.isCompleted)
            {
                if (data.levelID == levelID)
                {
                    OnAchievementCompleted(data);
                    return;
                }
            }
        }
    }

    //TODO: need to think of a way to speed this up...
    public static void AddKill(string enemyIdentifier, string towerIdentifier)
    {
        if (FTUE.IsActive())
            return;

        var levelIdentifier = GameState.instance.level.identifier;

        for (var i = 0; i < orderedAchievements.Count; ++i)
        {
            var data = orderedAchievements[i];

            if (data.isCompleted || data.type != Achievement.Type.Defeat)
                continue;

            //if subtype is null, then all enemy kills are being tracked by this achievement
            if (string.IsNullOrEmpty(data.subType) || data.subType == enemyIdentifier)
            {
                if (!string.IsNullOrEmpty(data.levelID) && data.levelID != levelIdentifier)
                    continue;

                if (!string.IsNullOrEmpty(data.towerID) && data.towerID != towerIdentifier)
                    continue;

                //cumulative enemy kills
                //var prev = PlayerPrefs.GetInt(data.guid + "_count", 0);

                var prev = ObscuredPrefs.GetInt(data.guid + "_count", 0);
                var next = prev + 1;

                if (next >= data.count)
                    OnAchievementCompleted(data);

                //PlayerPrefs.SetInt(data.guid + "_count", next);

                ObscuredPrefs.SetInt(data.guid + "_count", next);
            }
        }
    }

    public static void DefeatAchievementRefresh()
    {
        for (var i = 0; i < orderedAchievements.Count; ++i)
        {
            var data = orderedAchievements[i];

            if (data.isCompleted || data.type != Achievement.Type.Defeat)
                continue;

            //cumulative enemy kills
            //var prev = PlayerPrefs.GetInt(data.guid + "_count", 0);

            var prev = ObscuredPrefs.GetInt(data.guid + "_count", 0);
            if (prev >= data.count)
                OnAchievementCompleted(data);
        }
    }

    public static void CollectCoin()
    {
        Collect("coins");
    }

    public static void CollectEgg()
    {
        Collect("eggs");
    }

    //NB: make sure to only pass in star differences, so that repeatedly completing
    //early levels in order to boost this collection count does not work
    public static void CollectStars(int count)
    {
        Collect("stars", count);
    }

    public static void CollectTrinket(string trinketID, int count = 1)
    {
        Collect(trinketID, count);

        foreach (var kv in achievements)
        {
            var data = kv.Value;
            if (data.isCompleted)
                continue;

            if (data.type == Achievement.Type.Collect)
            {
                //null collection type just means any trinketID is fine
                if (string.IsNullOrEmpty(data.subType) || data.subType == trinketID)
                {
                    //var prev = PlayerPrefs.GetInt(data.guid + "_count", 0);
                    var prev = ObscuredPrefs.GetInt(data.guid + "_count", 0);
                    var next = prev + 1;

                    if (next >= data.count)
                        OnAchievementCompleted(data);

                    //PlayerPrefs.SetInt(data.guid + "_count", next);
                    ObscuredPrefs.SetInt(data.guid + "_count", next);
                }
            }
        }
    }

    static void Collect(string subType, int count = 1)
    {
        foreach (var kv in achievements)
        {
            var data = kv.Value;
            if (data.isCompleted)
                continue;

            if (data.type == Achievement.Type.Collect && data.subType == subType)
            {
                //var prev = PlayerPrefs.GetInt(data.guid + "_count", 0);
                var prev = ObscuredPrefs.GetInt(data.guid + "_count", 0);
                var next = prev + count;

                if (next >= data.count)
                    OnAchievementCompleted(data);

                //PlayerPrefs.SetInt(data.guid + "_count", next);

                ObscuredPrefs.SetInt(data.guid + "_count", next);
            }
        }
    }

    static void CollectAchievementRefresh()
    {
        foreach (var kv in achievements)
        {
            var data = kv.Value;
            if (data.isCompleted)
                continue;

            if (data.type == Achievement.Type.Collect)
            {
                //var prev = PlayerPrefs.GetInt(data.guid + "_count", 0);
                var prev = ObscuredPrefs.GetInt(data.guid + "_count", 0);
                if (prev >= data.count)
                    OnAchievementCompleted(data);
            }
        }
    }

    static void OnAchievementCompleted(Achievement data)
    {
        //PlayerPrefs.SetInt(data.guid + "_complete", 1);
        //PlayerPrefs.SetInt("ach_uncollected", PlayerPrefs.GetInt("ach_uncollected") + 1);
        ObscuredPrefs.SetInt(data.guid + "_complete", 1);
        ObscuredPrefs.SetInt("ach_uncollected", ObscuredPrefs.GetInt("ach_uncollected") + 1);

        data.isCompleted = true;

        UserInterface.ShowAchievementUnlocked(data);
    }

    public static float AchievementProgress(Achievement data)
    {
        if (data == null)
            return 0.0f;

        if (data.isCompleted)
            return 1.0f;

        //int current = PlayerPrefs.GetInt(data.guid + "_count", 0);
        int current = ObscuredPrefs.GetInt(data.guid + "_count", 0);
        return current / (float)data.count;
    }

    public static string AchievementProgressText(Achievement data)
    {
        if (data == null)
            return "null";

        //if (data.isCollected)
        //	return LocManager.Translate("Complete!");

        //int current = PlayerPrefs.GetInt(data.guid + "_count", 0);
        int current = ObscuredPrefs.GetInt(data.guid + "_count", 0);

        current = Mathf.Min(current, data.count);

        if (data.isCompleted)
            current = data.count;

        return current.ToString() + "/" + data.count.ToString();
    }

    public static void CollectAchievement(Achievement data)
    {
        switch (data.rewardType)
        {
            case Achievement.RewardType.Cash:
                SaveData.AddCash(data.rewardCount);
                break;

            case Achievement.RewardType.Trinket:
                SaveData.AddTrinket(data.rewardTypeName, data.rewardCount);
                break;

            case Achievement.RewardType.Tower:
                SaveData.UnlockTower(data.rewardTypeName);
                break;
        }

        data.isCollected = true;
        //PlayerPrefs.SetInt(data.guid + "_collected", 1);
        //PlayerPrefs.SetInt("ach_uncollected", PlayerPrefs.GetInt("ach_uncollected") - 1);

        ObscuredPrefs.SetInt(data.guid + "_collected", 1);
        ObscuredPrefs.SetInt("ach_uncollected", ObscuredPrefs.GetInt("ach_uncollected") - 1);
    }

    public static int UncollectedAchievementCount()
    {
        //return PlayerPrefs.GetInt("ach_uncollected", 0);

        return ObscuredPrefs.GetInt("ach_uncollected", 0);
    }

    public static void OnTowerUnlocked(Tower tower)
    {
        var towerIdentifier = TowerLoader.GetTowerID(tower);

        if (unlockableTowers.ContainsKey(towerIdentifier))
        {
            var achievementIdentifier = unlockableTowers[towerIdentifier];
            var achievementData = achievements[achievementIdentifier];

            achievementData.isCollected = true;

            //PlayerPrefs.SetInt(achievementData.guid + "_collected", 1);
            //PlayerPrefs.SetInt("ach_uncollected", PlayerPrefs.GetInt("ach_uncollected") - 1);

            ObscuredPrefs.SetInt(achievementData.guid + "_collected", 1);
            ObscuredPrefs.SetInt("ach_uncollected", ObscuredPrefs.GetInt("ach_uncollected") - 1);
        }
    }

    public static void DebugUnlockNextAchievement()
    {
        for (var i = 0; i < orderedAchievements.Count; ++i)
        {
            if (!orderedAchievements[i].isCompleted)
            {
                OnAchievementCompleted(orderedAchievements[i]);
                return;
            }
        }
    }

    public static void RefreshAchievements()
    {
        CollectAchievementRefresh();
        DefeatAchievementRefresh();
    }
}