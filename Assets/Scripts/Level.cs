using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Level : MonoBehaviour 
{
	public LevelData sourceData { get; private set; }

	public Sprite portraitImage;
	public GameObject mapPrefab;
	public List<Tower> towerList;

	//using this as a sort target so that towerList doesnt get populated
	//when the game quits. im not sure why unity does that....
	public List<Tower> towerListFinal { get; private set; }

	public void Initialise(LevelData sourceData)
	{
		this.sourceData = sourceData;
		this.towerListFinal = new List<Tower>(towerList);

		if (towerList == null || towerList.Count == 0)
			PopulateDefaultTowerList();
	}

	void PopulateDefaultTowerList()
	{
		var allTowers = TowerLoader.instance.towerInfoByPrefab;
		towerListFinal = new List<Tower>(allTowers.Count);

		foreach (var kv in allTowers)
			towerListFinal.Add(kv.Key);
	}

	public void SortTowerList()
	{
		towerListFinal.Sort((item0, item1) => {

			var info0 = TowerLoader.GetTowerInfo(item0, 0);
			var info1 = TowerLoader.GetTowerInfo(item1, 0);

			bool unlocked0 = SaveData.IsTowerUnlocked(info0.name);
			bool unlocked1 = SaveData.IsTowerUnlocked(info1.name);

			if (unlocked0 != unlocked1)
			{
				//sort unlocked stuff first
				return unlocked1.CompareTo(unlocked0);
			}
			else
			{
				//sort by price and then name regardless of
				//whether they are both locked, or both
				//unlocked

				if (info0.price == info1.price)
					return info0.name.CompareTo(info1.name);

				return info0.price.CompareTo(info1.price);
			}
		});
	}
}
