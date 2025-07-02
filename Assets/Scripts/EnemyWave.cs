using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyWave
{
	[System.Serializable]
	public class EnemyGroup
	{
		public MasterEnemyTable.Entry enemyData;
		public int enemyCount = 0;
		public float enemySpawnSpacing;
		public float spawnStartDelay;
		public float speedMultiplier;

		public float timeToNextSpawn;
		public int nextEnemy;
	};

	[System.Serializable]
	public class PathData
	{
		public int pathStartNodeIndex;
		public List<EnemyGroup> groups = new List<EnemyGroup>();

		public int currentGroup;
	};

	public enum State
	{
		Prelaunch,
		Active,
		Complete
	}
	
	public float timeBetweenEnemies = 0.1f;
	public float timeUntilNextWave = 20.0f;
	public float timeUntilWaveFinished;
	public List<PathData> pathData;
	public bool isHordeWave;

	public int reward = 100;
	public int enemiesRemaining { get; private set; }

	public int totalEnemiesToSpawn { get; private set; }
	public int numEnemiesSpawned { get; private set; }

	public State state { get; private set; }

	public static bool DEBUG_BATMAN = false;

	public EnemyWave(WaveData waveData)
	{
		pathData = waveData.pathData;

		state = State.Prelaunch;
		reward = waveData.reward;
		isHordeWave = waveData.isHorde;

		//count up the total number of enemies to spawn. using this
		//to drive the wave progression meter
		totalEnemiesToSpawn = 0;
		numEnemiesSpawned = 0;

		foreach (var data in pathData)
			foreach (var group in data.groups)
				totalEnemiesToSpawn += group.enemyCount;
	}
	
	public void UpdateTick(System.Random rng, int endlessDifficulty, int endlessDifficultyWave)
	{
		if (state == State.Prelaunch || state == State.Complete)
			return;

		foreach (var data in pathData)
		{
			//update each path independantly, so grab the frame elapsed time for each one
			float remainingTime = World.frameTime;
			
			//while we still have groups to spawn
			while (data.currentGroup < data.groups.Count)
			{
				//test if its time to spawn the next enemy in this group
				var currentGroup = data.groups[data.currentGroup];
				if (currentGroup.timeToNextSpawn <= remainingTime)
				{
					var toSpawn = currentGroup.enemyData;
					if (endlessDifficulty >= 0 && toSpawn.isBoss)
					{
						//need to swap out this boss type for one of the types available in this level
						var level = GameState.instance.level;
						var chosen = level.trinketEnemy[Random.Range(0, level.trinketEnemy.Length)]; //exclusive range.
						toSpawn = MasterEnemyTable.Get(chosen);
					}

					var c = World.SpawnEnemyCharacter(toSpawn);
					if (c != null)
					{
						//play optional spawn animator for this path node location. eg elevator anim.
						var animator = PathNetwork.GetSpawnAnimator(data.pathStartNodeIndex);
						if (animator != null)
							animator.Play("Spawn", 0, 0.0f);

						float maxOffset = toSpawn.maxPathOffset;
						float offset = Mathf.Lerp(-maxOffset, maxOffset, (float)rng.NextDouble());

						c.InitialiseState(toSpawn, this, true);
						c.InitialisePath(data.pathStartNodeIndex, offset);
						c.SetGroupSpeedMultiplier(currentGroup.speedMultiplier);
						c.SetAdditionalHealthMultiplier(endlessDifficulty, endlessDifficultyWave);
					}
						
					currentGroup.timeToNextSpawn = currentGroup.enemySpawnSpacing;
					currentGroup.nextEnemy += 1;
					
					//if we have run out of enemies to spawn for this group, move on to the next
					if (currentGroup.nextEnemy >= currentGroup.enemyCount) 
					{
						data.currentGroup += 1;
						if (data.currentGroup < data.groups.Count)
						{
							data.groups[data.currentGroup].timeToNextSpawn = data.groups[data.currentGroup].spawnStartDelay;
						}
						else
						{
							EnemyWaveController.OnWaveSpawningComplete();
						}
					}
					
					remainingTime -= currentGroup.timeToNextSpawn;
					numEnemiesSpawned += 1;
				}
				else
				{
					//not time to spawn yet, continue the countdown.
					currentGroup.timeToNextSpawn -= remainingTime;
					break;
				}
			}
		}
	}

	public void Launch()
	{
		if (state == State.Prelaunch)
		{
			enemiesRemaining = 0;

            //initialise all the enemy group timing etc.
            //Debug.Log(pathData.Count);
			foreach (var data in pathData)
			{
                //Debug.Log(data.groups.Count);
                foreach (var group in data.groups)
				{
                    group.timeToNextSpawn = group.spawnStartDelay + timeBetweenEnemies;
                    group.nextEnemy = 0;
                    enemiesRemaining += group.enemyCount;
                }
				
				data.currentGroup = 0;
			}

			state = State.Active;
		}
	}

	public void Stop()
	{
		state = State.Complete;
		numEnemiesSpawned = 0;
	}

	public void Reset()
	{
		state = State.Prelaunch;
		enemiesRemaining = 0;
		numEnemiesSpawned = 0;
	}

	public bool FinishedSpawningEnemies()
	{
		switch (state)
		{
		case State.Prelaunch:	return false;
		case State.Complete:	return true;

		case State.Active:
			foreach (var data in pathData)
				if (data.currentGroup < data.groups.Count)
					return false;

			break;
		}

		return true;
	}

	public void OnCharacterKilled(Character character)
	{
		enemiesRemaining -= 1;
	}

	//TODO: might need to switch this to a fixed RNG as well. probably not the
	//same one that is used for the path offsets for the enemies that
	//are spawned normally.
	public void SpawnChildren(EnemyCharacter parent, MasterEnemyTable.Entry enemyData)
	{
        //Debug.Log(enemyData.spawnTypes.Count);
        
        if (enemyData.spawnTypes != null && enemyData.spawnTypes.Count > 0)
		{
            //randomise the number of enemies to spawn
            //Debug.Log(enemyData.minSpawnCount+","+ enemyData.maxSpawnCount);
			var toSpawn = Random.Range(enemyData.minSpawnCount, enemyData.maxSpawnCount + 1);
			for (var i = 0; i < toSpawn; ++i)
			{
				//randomly choose from the available enemy types for the character that was just killed
				var choice = Random.Range(0, enemyData.spawnTypes.Count);
				var sourceData = enemyData.spawnTypes[choice];

				var c = World.SpawnEnemyCharacter(sourceData);
				if (c != null)
				{
					//copying pathing information over from the killed enemy to the new enemy.
					c.InitialiseState(sourceData, this, false);
					c.InitialisePath(parent);
					c.CopyStatusEffects(parent); //status effects carry over to children
					c.SetGroupSpeedMultiplier(c.groupSpeedMultiplier);
					c.SetAdditionalHealthMultiplier(parent.additionalHealthMultiplier);

					//the new enemy was spawned in this wave. keep track of it.
					enemiesRemaining += 1;
				}
			}
		}
	}
}
