using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class TowerInfo
{
    public string name;
    public Tower prefab;

    public int rating;
    public int price;
    public string plotName;

    public WeaponData weaponData = new WeaponData();

    //these are purely visual. WeaponData is where all the actual
    //effects and targeting and things are actually found.
    public string[] uiIcons;

    public struct VIP
    {
        public bool isVIP;
        public float vipPrice;
        public string vipDescription;
    }

    public VIP vip;

    public struct Cash
    {
        public bool isCash;
        public float cashPrice;
        public string cashDescription;
    }

    public Cash cash;

    public string DisplayName() { return LocManager.Translate("tower_name_" + name); }
    public string DisplayDesc(int level)
    {
        var stringID = "tower_desc_" + name + "_" + level.ToString();
        if (LocManager.TranslationAvailable(stringID))
            return LocManager.Translate(stringID);

        return LocManager.Translate("tower_desc_" + name);
    }

    public void AssignName(Text target)
    {
        LocManager.Assign(target, "tower_name_" + name);
    }

    public void AssignDescription(Text target, int level)
    {
        LocManager.Assign(target, "tower_desc_" + name + "_" + level.ToString());
    }
}

public class UpgradeInfo
{
    //info for display in the UI
    public string towerName;
    public string title;
    public string description;
    public string[] uiIcons; //as above

    //unlock costs.
    public int cash;
    public string trinketID;
    public int trinketCount;

    public float costMultiplier;
    public float sellMultiplier;
    public int bonusGoldReward;
    public float doubleEggSpawnChance;

    //this is the actual upgrade information
    public WeaponData weaponData = new WeaponData();

    public string DisplayName() { return LocManager.Translate(title); }
    public string DisplayDesc() { return LocManager.Translate(description); }
}

[System.Serializable]
public class CSVInfoTier
{
    public List<CSVInfo> list;
}

[System.Serializable]
public class CSVInfo
{
    public TextAsset csv;
    public GameObject towerPrefab;
    public string unlockTrinket;
    public int trinketsRequired;

    public bool isCash;
    public float cashPrice;
    public string cashDescription;


    public bool isVIP;
    public string vipDescription;
    public float vipPrice;
}

public class TowerLoader : MonoBehaviour
{
    public static TowerLoader instance = null;

    public Dictionary<string, List<TowerInfo>> towerInfo = new Dictionary<string, List<TowerInfo>>();
    public Dictionary<Tower, List<TowerInfo>> towerInfoByPrefab = new Dictionary<Tower, List<TowerInfo>>();

    Dictionary<Tower, CSVInfo> csvInfoByPrefab = new Dictionary<Tower, CSVInfo>();
    Dictionary<Tower, int> tierByPrefab = new Dictionary<Tower, int>();

    public Dictionary<string, List<UpgradeInfo>> towerUpgradeInfo = new Dictionary<string, List<UpgradeInfo>>();

    [SerializeField]
    List<CSVInfoTier> tiers;
    [SerializeField]
    TextAsset persistantTowerUpgrades;

    public string serverLocation = "https://storage.googleapis.com/towers_test/Towers/";
    public bool performServerLoad = true;

    //parsing helpers
    string[] targetingFlagNames = (string[])Enum.GetNames(typeof(EnemyAttributes));
    int[] targetingFlagValues = (int[])Enum.GetValues(typeof(EnemyAttributes));

    public static float maxDamage { get; private set; }
    public static float maxRange { get; private set; }
    public static float maxAttackSpeed { get; private set; }

    void Awake()
    {
        instance = this;

        maxDamage = 0;
        maxRange = 0;
        maxAttackSpeed = 0;

        Load();
    }

    void OnDestroy()
    {
        instance = null;
    }

    public static TowerInfo GetTowerInfo(string name, int level)
    {
        List<TowerInfo> info;
        if (instance.towerInfo.TryGetValue(ToGUID(name), out info))
            return level < info.Count ? info[level] : null;

        return null;
    }

    public static TowerInfo GetTowerInfo(Tower towerPrefab, int level = 0)
    {
        List<TowerInfo> info;
        if (instance.towerInfoByPrefab.TryGetValue(towerPrefab, out info))
            return level < info.Count ? info[level] : null;

        return null;
    }

    public static List<TowerInfo> GetTowerInfo(string name)
    {
        List<TowerInfo> result;
        if (instance.towerInfo.TryGetValue(ToGUID(name), out result))
            return result;

        return null;
    }

    public static List<TowerInfo> GetTowerInfo(Tower prefab)
    {
        List<TowerInfo> result;
        if (instance.towerInfoByPrefab.TryGetValue(prefab, out result))
            return result;

        return null;
    }

    public static int GetTowerTier(Tower prefab)
    {
        int result = 0;
        if (instance.tierByPrefab.TryGetValue(prefab, out result))
            return result;

        return 0;
    }

    public static Tower GetTowerPrefab(string name)
    {
        var info = GetTowerInfo(name, 0);
        return info == null ? null : info.prefab;
    }

    public static string GetTowerID(Tower towerPrefab)
    {
        var towerInfo = GetTowerInfo(towerPrefab, 0);
        if (towerInfo != null)
            return towerInfo.name;

        return "";
    }

    public static int GetTowerMaxLevel(string name)
    {
        List<TowerInfo> info;
        if (instance.towerInfo.TryGetValue(ToGUID(name), out info))
            return info.Count;

        return 0;
    }

    public static bool UnlockRequirements(Tower tower, out string trinketID, out int count)
    {
        trinketID = "";
        count = 0;

        CSVInfo info;
        if (instance.csvInfoByPrefab.TryGetValue(tower, out info))
        {
            if (!string.IsNullOrEmpty(info.unlockTrinket))
            {
                trinketID = info.unlockTrinket;
                count = info.trinketsRequired;

                return true;
            }
        }

        //no unlock requirements found
        return false;
    }

    public static void UnlockAll()
    {
        if (instance == null)
            return;

        foreach (var kv in instance.towerInfo)
            SaveData.UnlockTower(kv.Key);
    }

    public static List<Tower> lockedTower;

    void Load()
    {
        lockedTower = new List<Tower>();
        //pull in towers and their upgrades
        for (int i = 0; i < tiers.Count; ++i)
        {
            for (int j = 0; j < tiers[i].list.Count; ++j)
            {
                var info = tiers[i].list[j];

                var towerPrefab = info.towerPrefab.GetComponent<Tower>();
                //var result = ParseTowerCSV(info.csv.name, info.csv.text, towerPrefab);
                var result = ParseTowerCSV(info, info.csv.name, info.csv.text, towerPrefab);

                towerInfo.Add(result[0].name, result);
                towerInfoByPrefab.Add(towerPrefab, result);
                csvInfoByPrefab.Add(towerPrefab, info);
                tierByPrefab.Add(towerPrefab, i);

                if (!SaveData.IsTowerUnlocked(TowerLoader.GetTowerID(towerPrefab))&&!GetTowerInfo(towerPrefab, 0).cash.isCash)
                {
                    lockedTower.Add(towerPrefab);
                }
            }
        }

        ParseUpgradeInfo(persistantTowerUpgrades);

        //calculate max stats etc before server upload. they get updated progressively
        //as the server info arrives (if it arrives)
        {
            foreach (var kv in towerInfo)
                RefreshMaxStats(kv.Value[kv.Value.Count - 1]);

            RefreshCalculators();
        }

        if (performServerLoad)
        {
            for (int i = 0; i < tiers.Count; ++i)
            {
                for (int j = 0; j < tiers[i].list.Count; ++j)
                {
                    var info = tiers[i].list[j];
                    var prefab = info.towerPrefab.GetComponent<Tower>();
                    StartCoroutine(LoadServerCSV(info.csv.name, prefab));
                }
            }
        }

        //		Debug.Log("MAX STATS - " + maxDamage + " " + maxAttackSpeed + " " + maxRange);
    }

    IEnumerator LoadServerCSV(string csvName, Tower towerPrefab)
    {
        var path = serverLocation + csvName + ".csv?t=" + UnityEngine.Random.value.ToString();
        var www = new WWW(path);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogWarning("[TowerLoader] error reading " + path + ": " + www.error);
        }
        else
        {
            //			Debug.Log("[TowerLoader] read: " + path);

            var result = ParseTowerCSV(csvName, www.text, towerPrefab);

            towerInfo[result[0].name] = result;
            towerInfoByPrefab[towerPrefab] = result;

            RefreshMaxStats(result[result.Count - 1]);

            //NB: dont do this. the calculators are being used to generate default values
            //which are then tweaked as needed and added by hand to the server files
            //			RefreshCalculators();
        }
    }

    List<TowerInfo> ParseTowerCSV(string csvName, string csvData, Tower towerPrefab)
    {
        var lines = CSVUtil.Lines(csvData);
        var towers = new List<TowerInfo>();

        for (var i = 1; i < lines.Length; ++i)
        {
            var tokens = CSVUtil.Tokenise(lines[i]);
            if (CSVUtil.IsLineComment(tokens) || !CSVUtil.IsLineValid(tokens))
                continue;

            int CT = 0;

            var tower = new TowerInfo();
            tower.name = ToGUID(csvName);
            tower.prefab = towerPrefab;
            tower.rating = CSVUtil.ParseInt(tokens, CT++, 0);
            tower.price = int.Parse(tokens[CT++]);
            tower.plotName = tokens[CT++];

            tower.weaponData.targetingFlags = ParseTargetingFlags(tokens[CT++]);
            ParseStatusEffects(tokens[CT++], ref tower.weaponData);

            tower.weaponData.damage = float.Parse(tokens[CT++]);
            tower.weaponData.attacksPerSecond = float.Parse(tokens[CT++]);
            tower.weaponData.maxTargets = int.Parse(tokens[CT++]);
            tower.weaponData.projectileSplashDamage = float.Parse(tokens[CT++]);
            tower.weaponData.projectileSplashRadius = float.Parse(tokens[CT++]);

            tower.uiIcons = CSVUtil.ParseString(tokens, CT++, "").Split('&');

            towers.Add(tower);
        }

        if (!towerUpgradeInfo.ContainsKey(towers[0].name))
            towerUpgradeInfo.Add(towers[0].name, new List<UpgradeInfo>());

        return towers;
    }

    List<TowerInfo> ParseTowerCSV(CSVInfo info, string csvName, string csvData, Tower towerPrefab)
    {
        var lines = CSVUtil.Lines(csvData);
        var towers = new List<TowerInfo>();

        for (var i = 1; i < lines.Length; ++i)
        {
            var tokens = CSVUtil.Tokenise(lines[i]);
            if (CSVUtil.IsLineComment(tokens) || !CSVUtil.IsLineValid(tokens))
                continue;

            int CT = 0;

            var tower = new TowerInfo();
            tower.name = ToGUID(csvName);
            tower.prefab = towerPrefab;
            tower.rating = CSVUtil.ParseInt(tokens, CT++, 0);
            tower.price = int.Parse(tokens[CT++]);
            tower.plotName = tokens[CT++];

            if (info.isCash)
            {
                tower.cash = new TowerInfo.Cash();
                tower.cash.isCash = true;
                tower.cash.cashPrice = info.cashPrice;
                tower.cash.cashDescription = info.cashDescription;
            }

            if (info.isVIP)
            {
                tower.vip = new TowerInfo.VIP();
                tower.vip.isVIP = true;
                tower.vip.vipPrice = info.vipPrice;
                tower.vip.vipDescription = info.vipDescription;
            }


            tower.weaponData.targetingFlags = ParseTargetingFlags(tokens[CT++]);
            ParseStatusEffects(tokens[CT++], ref tower.weaponData);

            tower.weaponData.damage = float.Parse(tokens[CT++]);
            tower.weaponData.attacksPerSecond = float.Parse(tokens[CT++]);
            tower.weaponData.maxTargets = int.Parse(tokens[CT++]);
            tower.weaponData.projectileSplashDamage = float.Parse(tokens[CT++]);
            tower.weaponData.projectileSplashRadius = float.Parse(tokens[CT++]);

            tower.uiIcons = CSVUtil.ParseString(tokens, CT++, "").Split('&');

            towers.Add(tower);
        }

        if (!towerUpgradeInfo.ContainsKey(towers[0].name))
            towerUpgradeInfo.Add(towers[0].name, new List<UpgradeInfo>());

        return towers;
    }

    public float cashRate;

    void ParseUpgradeInfo(TextAsset csv)
    {
        var lines = CSVUtil.Lines(csv);
        for (int i = 1; i < lines.Length; i++)
        {
            var tokens = CSVUtil.Tokenise(lines[i]);

            if (CSVUtil.SkipLine(tokens))
                continue;

            int j = 0;

            var upgrade = new UpgradeInfo();

            upgrade.towerName = ToGUID(tokens[j++]);
            upgrade.title = tokens[j++];
            upgrade.description = tokens[j++];
            upgrade.uiIcons = CSVUtil.ParseString(tokens, j++, "").Split('&');

            upgrade.cash = (int)(CSVUtil.ParseInt(tokens, j++, 0)*cashRate);
            upgrade.trinketID = CSVUtil.ParseString(tokens, j++, "");
            upgrade.trinketCount = CSVUtil.ParseInt(tokens, j++, 0);

            ParseStatusEffects(tokens[j++], ref upgrade.weaponData);

            upgrade.weaponData.targetingFlags = ParseTargetingFlags(tokens[j++]);
            upgrade.weaponData.damage = CSVUtil.ParseFloat(tokens, j++, 0.0f);
            upgrade.weaponData.attacksPerSecond = CSVUtil.ParseFloat(tokens, j++, 0.0f);
            upgrade.weaponData.projectileSplashDamage = CSVUtil.ParseFloat(tokens, j++, 0.0f);
            upgrade.weaponData.projectileSplashRadius = CSVUtil.ParseFloat(tokens, j++, 0.0f);
            upgrade.weaponData.maxTargets = CSVUtil.ParseInt(tokens, j++, 0);

            upgrade.costMultiplier = CSVUtil.ParseFloat(tokens, j++, 1.0f);
            upgrade.sellMultiplier = CSVUtil.ParseFloat(tokens, j++, 1.0f);
            upgrade.bonusGoldReward = CSVUtil.ParseInt(tokens, j++, 0);
            upgrade.doubleEggSpawnChance = CSVUtil.ParseFloat(tokens, j++, 0.0f);


            List<UpgradeInfo> upgrades;
            if (towerUpgradeInfo.TryGetValue(upgrade.towerName, out upgrades))
            {
                upgrades.Add(upgrade);
            }
            else
            {
                Debug.Log("[TowerLoader] unknown tower: " + upgrade.towerName);
            }
        }
    }

    void ParseStatusEffects(string input, ref WeaponData weaponData)
    {
        var flags = CSVUtil.Tokenise(input, '&');

        for (int i = 0; i < flags.Length; i++)
        {
            if (string.IsNullOrEmpty(flags[i]))
                continue;

            var values = CSVUtil.Tokenise(flags[i], ';');

            StatusEffectData effect;
            effect.type = (WeaponStatusEffectType)Enum.Parse(typeof(WeaponStatusEffectType), values[0]);
            effect.value = CSVUtil.ParseFloat(values, 1, 0.0f);
            effect.duration = CSVUtil.ParseFloat(values, 2, 0.0f);

            if (weaponData.statusEffectCount < weaponData.statusEffects.Length)
            {
                weaponData.statusEffects[i] = effect;
                weaponData.statusEffectCount += 1;
            }
        }
    }

    EnemyAttributes ParseTargetingFlags(string source)
    {
        return (EnemyAttributes)CSVUtil.ParseEnum(source, targetingFlagNames, targetingFlagValues, '&');
    }

    public static string ToGUID(string input)
    {
        return input.ToLower().Replace(' ', '_');
    }

    void RefreshMaxStats(TowerInfo towerInfo)
    {
        //ignore towers without weapons. they will bust the stats because they use
        //the columns for different things eg the duck tower uses the damage
        //column as the value of the coin reward 
        if (towerInfo.prefab.GetComponent<Weapon>() == null)
            return;

        var data = new WeaponData();

        data.Merge(towerInfo.weaponData);
        data.Merge(GetGlobalWeaponUpgrades(towerInfo.name), false);

        maxDamage = Mathf.Max(maxDamage, data.damage);
        maxAttackSpeed = Mathf.Max(maxAttackSpeed, data.attacksPerSecond);
        maxRange = Mathf.Max(maxRange, RangePlots.GetPlotRange(towerInfo.plotName));
    }

    void RefreshCalculators()
    {
        var calculators = GetComponentsInChildren<TowerPriceCalculator>(true);
        for (int i = 0; i < calculators.Length; ++i)
            calculators[i].Refresh();
    }

    #region UPGRADE HELPERS

    //TODO: these should really be moved into their own class

    //TODO: also, since these are all just min/max search functions,
    //the UpgradeInfo class should just be arrays of floats
    //and ints, and then these search functions can take
    //an array and an index to cut the copy/paste down to nothing.

    public static UpgradeInfo[] GetPersistantUpgradeInfo(string name)
    {
        List<UpgradeInfo> info;
        if (instance.towerUpgradeInfo.TryGetValue(ToGUID(name), out info))
            return info.ToArray();

        return null;
    }

    public static WeaponData GetGlobalWeaponUpgrades(string towerName)
    {
        var result = new WeaponData();
        result.Clear();

        var upgradeInfo = GetPersistantUpgradeInfo(towerName);
        if (upgradeInfo != null)
        {
            int upgradeLevel = SaveData.GetUpgradeLevel(towerName);
            for (int i = 0; i < upgradeLevel; i++)
                result.Merge(upgradeInfo[i].weaponData, true); //add status effects together
        }

        return result;
    }

    public static int GetBonusGoldRewardForEnemyKill(string tower)
    {
        var result = 0; //maximise result
        var upgradeInfo = GetPersistantUpgradeInfo(tower);

        for (int i = 0; i < upgradeInfo.Length; ++i)
            if (SaveData.IsUpgradeUnlocked(tower, i))
                result = Mathf.Max(result, upgradeInfo[i].bonusGoldReward);

        return result;
    }

    public static float GetTowerUpgradePriceMultiplier(string tower)
    {
        var result = 1.0f; //minimse cost price
        var upgradeInfo = GetPersistantUpgradeInfo(tower);

        for (int i = 0; i < upgradeInfo.Length; ++i)
            if (SaveData.IsUpgradeUnlocked(tower, i))
                result = Mathf.Min(result, upgradeInfo[i].costMultiplier);

        return result;
    }

    public static float GetTowerSellPriceMultiplier(string tower)
    {
        var result = 1.0f; //maximise sell price
        var upgradeInfo = GetPersistantUpgradeInfo(tower);

        for (int i = 0; i < upgradeInfo.Length; ++i)
            if (SaveData.IsUpgradeUnlocked(tower, i))
                result = Mathf.Max(result, upgradeInfo[i].sellMultiplier);

        return result;
    }

    public static float GetDoubleEggChance(string tower)
    {
        var result = 0.0f; //maximise chance
        var upgradeInfo = GetPersistantUpgradeInfo(tower);

        for (int i = 0; i < upgradeInfo.Length; ++i)
            if (SaveData.IsUpgradeUnlocked(tower, i))
                result = Mathf.Max(result, upgradeInfo[i].doubleEggSpawnChance);

        return result;
    }

    #endregion
}
