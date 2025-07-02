using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class WorldAnalytics
{
	static WorldAnalytics instance;
	static LevelData levelData;

	public static void SetLevel(LevelData level)
	{
		levelData = level;
	}

	public static void EndOfRound(int wavesComplete, bool endlessMode)
	{
		Analytics.CustomEvent("eor", new Dictionary<string, object>() {
			{"map", levelData.identifier},
			{"waves_complete", wavesComplete},
			{"is_endless_mode", endlessMode}
		});
	}

	public static void CreateTower(Tower tower)
	{
		RecordTowerEvent("create_tower", tower);
	}

	public static void UpgradeTower(Tower tower)
	{
		RecordTowerEvent("upgrade_tower", tower);
	}

	public static void SellTower(Tower tower)
	{
		RecordTowerEvent("sell_tower", tower);
	}

	static void RecordTowerEvent(string eventName, Tower tower)
	{
		Analytics.CustomEvent(eventName, new Dictionary<string, object>() {
			{"map", levelData.identifier},
			{"tower", tower.towerInfo.name},
			{"level", tower.currentLevel},
			{"x", tower.transform.position.x},
			{"y", tower.transform.position.y},
			{"z", tower.transform.position.z}
		});	
	}
}
