using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: this also acts as a PR calculator. probably should rename it
//to TowerStatCalculator with a dropdown, but i dont want to 
//blow up the serialisation for the existing prices at this point

public class TowerPriceCalculator : MonoBehaviour 
{
	[System.Serializable]
	public struct StatusEffectCost
	{
		public WeaponStatusEffectType effectType;
		public float effectCost;
		public float durationCost;
		public float modifierCost;
		public bool modifierSmallerNumbersAreBetter;
	}

	[System.Serializable]
	public struct TargettingCost
	{
		public EnemyAttributes attribute;
		public int cost;
	}

	[System.Serializable]
	public struct WeaponCategoryCost
	{
		public WeaponCategory type;
		public int cost;
	}

	[System.Serializable]
	public struct WeaponTypeCost
	{
		public WeaponType type;
		public int cost;
	}

	[System.Serializable]
	public struct TierCost
	{
		public int additionalBaseCost;
		public List<float> levelMultipliers;
	}

	public bool calculatesPR = false;

	public int damageCost = 100;
	public int attacksPerSecondCost = 100;
	public int rangeCost = 100;
	public int additionalTargetsCost = 100;
	public int splashDamageCost = 100;
	public int splashRadiusCost = 100;

	public List<TierCost> tierCosts;
	public List<WeaponCategoryCost> weaponCategoryCosts;
	public List<WeaponTypeCost> weaponTypeCosts;
	public List<StatusEffectCost> statusEffectCosts;
	public List<TargettingCost> targettingCosts;

	public bool calculateRangeByPlotRadius = true;

	public int roundToTheNearest = 50;

	public void Refresh()
	{
		if (TowerLoader.instance == null)
			return;

		foreach (var kv in TowerLoader.instance.towerInfoByPrefab)
		{
			//for now, don't auto-price anything that doesnt have a weapon.
			var weaponComponent = kv.Key.GetComponent<Weapon>();
			if (weaponComponent == null)
				continue;

			var tier = TowerLoader.GetTowerTier(kv.Key);
			
			for (int i = 0; i < kv.Value.Count; ++i)
			{
				var towerInfo = kv.Value[i];
				var weaponData = towerInfo.weaponData;

				var cost = 0.0f;
					
				cost += weaponData.damage * damageCost;
				cost += weaponData.attacksPerSecond * attacksPerSecondCost;
				cost += (weaponData.maxTargets - 1) * additionalTargetsCost;
				cost += weaponData.projectileSplashDamage * splashDamageCost;
				cost += weaponData.projectileSplashRadius * splashRadiusCost;

				//different ways of measuring the cost. either do it by radius,
				//which is what the UI displays, or by the number of tiles
				//the plot shape can cover.
				if (calculateRangeByPlotRadius)
				{
					cost += RangePlots.GetPlotRange(towerInfo.plotName) * rangeCost;
				}
				else
				{
					cost += RangePlots.GetPlotData(towerInfo.plotName, 0).Count * rangeCost;
				}

				//weapon type and category modifiers
				for (var j = 0; j < weaponCategoryCosts.Count; ++j)
					if (weaponCategoryCosts[j].type == weaponComponent.category)
						cost += weaponCategoryCosts[j].cost;

				for (var j = 0; j < weaponTypeCosts.Count; ++j)
					if (weaponTypeCosts[j].type == weaponComponent.type)
						cost += weaponTypeCosts[j].cost;

				//iterate over weapon status effects to add to the cost
				for (var j = 0; j < weaponData.statusEffectCount; ++j)
				{
					var effect = weaponData.statusEffects[j];

					for (var k = 0; k < statusEffectCosts.Count; ++k)
					{
						if (effect.type == statusEffectCosts[k].effectType)
						{
							cost += statusEffectCosts[k].effectCost;
							cost += statusEffectCosts[k].durationCost * effect.duration;

							if (statusEffectCosts[k].modifierSmallerNumbersAreBetter)
								cost += (1.0f - effect.value) * statusEffectCosts[k].modifierCost;
							
							break;
						}
					}
				}

				//targetting flags are an additional cost
				for (var j = 0; j < targettingCosts.Count; ++j)
				{
					if ((targettingCosts[j].attribute & weaponData.targetingFlags) != 0)
						cost += targettingCosts[j].cost;
				}

				//finally, account for tiering
				if (tier < tierCosts.Count)
				{
					cost += tierCosts[tier].additionalBaseCost;

					if (i < tierCosts[tier].levelMultipliers.Count)
						cost *= tierCosts[tier].levelMultipliers[i];
				}
					
				//rounding upwards only
				if (roundToTheNearest > 1)
				{
					int temp = (int)(cost)/roundToTheNearest;
					if (towerInfo.price % roundToTheNearest != 0)
						temp += 1;
					
					cost = temp * roundToTheNearest;
				}	

				if (calculatesPR)
					towerInfo.rating = (int)cost;
				else
					towerInfo.price = (int)cost;
			}
		}
	}
}
