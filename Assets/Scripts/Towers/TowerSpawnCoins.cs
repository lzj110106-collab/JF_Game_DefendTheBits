using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSpawnCoins : MonoBehaviour 
{
	public GameObject coinPrefab;

	public string coinReadyAnimationTrigger = "PresentCoin";
	public string coinCollectAnimationTrigger = "CollectCoin";

	Tower tower = null;
	float timer = 0.0f;

	bool coinReady = false;
	bool canSpawn = false;
	int totalSpawned = 0;

	GameObject passivePFXSource;
	GameObject passivePFX;

	public int totalCoinsCollected { get; private set; }

	public void Initialise()
	{
		tower = GetComponent<Tower>();
		totalCoinsCollected = 0;

		//make sure the effect can be triggered this wave. checking the spawn
		//count here, otherwise upgrading a tower after collecting the
		//reward can get the reward to trigger again this round (independant
		//of the global upgrade that can trigger multiple rewards)
		if (EnemyWaveController.IsWaveInProgress() && !canSpawn && totalSpawned == 0)
			OnWaveLaunched();
	}

	public void OnWorldReset()
	{
		ClearCollectionIdlePFX();
		canSpawn = false;
	}

	public void OnLoad(int totalCoinsCollected)
	{
		this.totalCoinsCollected = totalCoinsCollected;
	}

	public void UpdateTick()
	{
		if (coinReady)
		{
			//waiting...
		}
		else if (canSpawn)
		{
			timer -= World.frameTime;
			if (timer <= 0.0f)
			{
				coinReady = true;
				canSpawn = false;
				timer = 0.0f;

				tower.animator.SetTrigger(coinReadyAnimationTrigger);
				tower.pfx.Play(PFX.Tower_SpawnObject, true);
				AudioController.Play ("Tower_Duck_Produce");

				CreateCollectionIdlePFX();
			}
		}
	}

	public void OnTowerUpgraded()
	{
		//make sure the ready animation persists across upgrades
		if (coinReady)
			tower.animator.SetTrigger(coinReadyAnimationTrigger);
	}

	public bool HandleMousePick()
	{
		if (coinReady)
		{
			tower.animator.SetTrigger(coinCollectAnimationTrigger);
			tower.pfx.Play(PFX.Tower_SpawnObject, true);

			//generate and immediately collect a reward coin for feedback.
			//the reward coin is worth the damage value of the tower (which
			//otherwise does not have a weapon attached)
			var reward = World.AddInteractReward(coinPrefab, transform.position);
			if (reward != null)
			{
				int amount = (int)tower.towerInfo.weaponData.damage;
				amount += TowerLoader.GetBonusGoldRewardForEnemyKill(tower.towerInfo.name);

				reward.Collect(amount);
				totalCoinsCollected += amount;
			}

			coinReady = false;
			canSpawn = false;
			totalSpawned += 1;

			if (totalSpawned == 1)
			{
				//check the tower upgrades to see if a second egg can be spawned
				float chance = TowerLoader.GetDoubleEggChance(tower.towerInfo.name);
				if (Random.value < chance)
				{
					timer = 1.0f;
					canSpawn = true;
				}
			}

			AchievementDatabase.CollectEgg();
			AudioController.Play ("Tower_Duck_Collect");

			ClearCollectionIdlePFX();

			return true;
		}

		return false;
	}

#region WAVES

	public void OnWaveLaunched()
	{
		totalSpawned = 0;
		coinReady = false;
		canSpawn = true;

		//randomise the spawn time a bit to be between 50% and 100% of
		//the time defined in the CSV for the tower
		timer = Random.Range(0.5f, 1.0f) / tower.weaponData.attacksPerSecond;
	}

	public void OnWaveComplete()
	{
		if (coinReady)
			tower.animator.SetTrigger(coinCollectAnimationTrigger);

		canSpawn = false;
		coinReady = false;
	}

	public void OnWaveSpawningComplete()
	{
	}

#endregion

	public void CreateCollectionIdlePFX()
	{
		var passiveData = tower.pfx.entriesOrdered[(int)PFX.Tower_DuckCollectionIdle];
		if (passiveData != null)
		{
			var passiveInstance = tower.pfx.Play(PFX.Tower_Passive, false);
			if (passiveInstance != null)
			{
				passivePFXSource = passiveData.prefab;
				passivePFX = passiveInstance.gameObject;
			}
		}
	}

	public void ClearCollectionIdlePFX()
	{
		if (passivePFX != null)
		{
			PFXPool.Return(passivePFXSource, passivePFX);
			passivePFX = null;
			passivePFXSource = null;
		}
	}
}
