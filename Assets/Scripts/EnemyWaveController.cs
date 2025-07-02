using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EnemyWaveController
{
	public static EnemyWaveController instance;

	string initialWavesName;
	EnemyWave[] waves;

	System.Random rng;

	public int nextWave = 0;
	public int totalWaves; //excludes endless mode. used for UI display in EOR
	public int totalWavesComplete;
	bool wavesHaveLaunched = false;

	public delegate void WaveControllerEvent();
	public WaveControllerEvent waveLaunchedCallback;
	public WaveControllerEvent waveSpawningCompleteCallback;

	public delegate void WaveCompleteEvent(int waveNumber, int waveReward, bool allWavesComplete);
	public WaveCompleteEvent waveCompleteCallback;

	bool performSkipToWave = false;
	int skipToWaveTarget = 0;

	bool endlessMode;
	int endlessModeWaveNumber; //consecutive numbers for UI display
	int endlessModeWaveIndex; //index into EnemyWave[] 
	int endlessModeDifficulty;
	int endlessModeDifficultyProgress;

	//two values here as endless will eventually degenerate into
	//random mode once it runs out of levels sequentially
	bool randomiseEndlessWaves = false;
	bool useRandomEndlessWaves = false;

	public int currentEndlessModeGoldBonus { get; private set; }
	public float currentEndlessModeHealthBonus { get; private set; }

	public EnemyWaveController(string waveName)
	{
		instance = this;
		randomiseEndlessWaves = GameState.instance.randomiseEndlessWaves;

		InitialiseWaveData(WaveLoader.GetWave(waveName));
		WavesHUD.SetMaxWaves(waves.Length);

		initialWavesName = waveName;
		totalWaves = waves.Length;
		totalWavesComplete = 0;

		endlessMode = false;
		endlessModeDifficulty = -1;
	}

	public void Destroy()
	{
		instance = null;
	}
		
	void InitialiseWaveData(List<WaveData> waveData)
	{
		if (waveData != null)
		{
			waves = new EnemyWave[waveData.Count];

			for (int i = 0; i < waveData.Count; ++i)
				waves[i] = new EnemyWave(waveData[i]);
		}

		Reset();
	}

	public void Reset()
	{
		nextWave = 0;
		wavesHaveLaunched = false;
		performSkipToWave = false;

		for (int i = 0; i < waves.Length; ++i)
			waves[i].Reset();

		WavesHUD.SetWaveProgress(0, 0.0f, false);
		ShowPathIndicators();
	}

	public void ResetLevel()
	{
		if (IsEndlessMode())
		{
			InitialiseWaveData(WaveLoader.GetWave(initialWavesName));
			endlessMode = false;
			endlessModeDifficulty = -1;
			useRandomEndlessWaves = randomiseEndlessWaves; //reset this
		}
		else
		{
			Reset();
		}

		//dont do this in reset. reset is being used during the transition
		//to endless mode and that breaks this counter
		totalWavesComplete = 0;
	}

	public void UpdateTick()
	{
		PerformDebugSkipToWave();

		if (!wavesHaveLaunched)
			return;

		var currentWaveIndex = endlessMode ? endlessModeWaveIndex : nextWave - 1;
		var currentWave = waves[currentWaveIndex];

		if (currentWave.FinishedSpawningEnemies() && 
			currentWave.enemiesRemaining <= 0)
		{
			totalWavesComplete += 1;

			if (endlessMode)
			{
				if (waveCompleteCallback != null)
					waveCompleteCallback(endlessModeWaveNumber, currentEndlessModeGoldBonus, false);

				if (randomiseEndlessWaves || useRandomEndlessWaves)
				{
					endlessModeWaveIndex = Random.Range(0, waves.Length);
					endlessModeWaveNumber += 1;

					//dealing with difficulty escalation with endless mode 
					endlessModeDifficultyProgress += 1;
					if (endlessModeDifficulty < WaveLoader.GetEndlessWaveDifficultyCount() - 1)
					{
						if (endlessModeDifficultyProgress >= WaveLoader.inst.endlessWaveCountPerDifficulty)
						{
							InitialiseWaveData(WaveLoader.GetEndlessWave(endlessModeDifficulty + 1));

							endlessModeDifficulty += 1;
							endlessModeDifficultyProgress = 0;
						}
					}
				}
				else
				{
					endlessModeWaveIndex += 1;
					endlessModeWaveNumber += 1;
					endlessModeDifficultyProgress += 1;

					if (endlessModeWaveIndex >= waves.Length)
					{
						if (endlessModeDifficulty < WaveLoader.GetEndlessWaveDifficultyCount() - 1)
						{
							InitialiseWaveData(WaveLoader.GetEndlessWave(endlessModeDifficulty + 1));

							endlessModeDifficulty += 1;
							endlessModeDifficultyProgress = 0;
							endlessModeWaveIndex = 0;
						}
						else
						{
							//no more difficulty levels left
							endlessModeWaveIndex = Random.Range(0, waves.Length);
							useRandomEndlessWaves = true;
						}
					}
				}

				//increasing various stats as endless mode progresses
				{
					if (endlessModeWaveNumber % WaveLoader.inst.endlessGoldWavesPerIncrease == 0)
						currentEndlessModeGoldBonus += WaveLoader.inst.endlessGoldIncrease;
				}

				wavesHaveLaunched = false;
				WavesHUD.SetWaveProgress(endlessModeWaveNumber, 0.0f, ShowHordeIcon());
			}
			else
			{
				if (waveCompleteCallback != null)
					waveCompleteCallback(currentWaveIndex, currentWave.reward, nextWave >= waves.Length);

				wavesHaveLaunched = false;
				WavesHUD.SetWaveProgress(nextWave, 0.0f, ShowHordeIcon());

				//throw up tips for enemies that appear in the next wave
				AddNextWaveHints();
			}

			ShowPathIndicators();
		}
		else
		{
			currentWave.UpdateTick(rng, endlessModeDifficulty, endlessModeDifficultyProgress);

			float progress = (float)currentWave.numEnemiesSpawned/(float)currentWave.totalEnemiesToSpawn;
			WavesHUD.SetWaveProgress(endlessMode ? endlessModeWaveNumber : (nextWave - 1), progress, ShowHordeIcon());
		}
	}

	public static void OnWaveCalled()
	{
		instance._OnWaveCalled();
	}

	void _OnWaveCalled()
	{
		PathNetwork.ClearPathIndicators();
        //Debug.Log(endlessMode);
		if (endlessMode)
		{
			rng = new System.Random(endlessModeWaveNumber);

			//make sure to reset each wave before launching as endless
			//mode is looping on existing wave data over and over.
			waves[endlessModeWaveIndex].Reset();
			waves[endlessModeWaveIndex].Launch();
			wavesHaveLaunched = true;

			if (waveLaunchedCallback != null)
				waveLaunchedCallback();	
		}
		else
		{
			if (nextWave < waves.Length)
			{
				//reseed the RNG for every wave so that the path offset are consistent across
				//multiple playthoughs. set the seed to be the same as the wave number so
				//that enemies dont appear in the same positions on every wave.
				rng = new System.Random(nextWave);
				
				waves[nextWave].Launch();
				wavesHaveLaunched = true;
				nextWave += 1;

                if (waveLaunchedCallback != null)
                    waveLaunchedCallback();
            }
		}

		//wave countdown is cancelled by the player pressing the start wave button
		WavesHUD.OnWaveLaunched();
	}

#region QUERIES

	public static bool IsWaveInProgress()
	{
		return instance.wavesHaveLaunched;
	}

	public static void OnWaveSpawningComplete()
	{
		if (instance.waveSpawningCompleteCallback != null)
			instance.waveSpawningCompleteCallback();
	}

	public static EnemyWave[] WaveData()
	{
		return instance == null ? null : instance.waves;
	}

	public static int CurrentWaveNumber()
	{
		if (instance != null)
		{
			if (instance.endlessMode)
				return instance.endlessModeWaveNumber;

			return Mathf.Max(instance.nextWave - 1, 0);
		}

		return 0;
	}

	public static int TotalWaves()
	{
		return instance.totalWaves;
	}

	public static int WavesCompleted()
	{
		return instance == null ? 0 : instance.totalWavesComplete;
	}

#endregion

#region ENDLESS MODE

	public static void TriggerEndlessMode()
	{
		instance.endlessMode = true;
		instance.endlessModeWaveNumber = instance.waves.Length;

		instance.currentEndlessModeGoldBonus = WaveLoader.inst.endlessGoldBaseValue;
		instance.currentEndlessModeHealthBonus = 1.0f; //this is a multiplier.

		//pull in the new wave data and randomly pick a starting point
		instance.InitialiseWaveData(WaveLoader.GetEndlessWave(0));
		instance.endlessModeWaveIndex = Random.Range(0, instance.waves.Length);

		instance.endlessModeDifficulty = 0;
		instance.endlessModeDifficultyProgress = 0;

		if (!instance.randomiseEndlessWaves)
			instance.endlessModeWaveIndex = 0;

		//InitialiseWaveData() resets the waves hud to zero. 
		//TODO: this class shouldnt deal with HUD at all.
		WavesHUD.SetWaveProgress(instance.endlessModeWaveNumber, 0.0f, instance.ShowHordeIcon());
	}

	public static bool IsEndlessMode()
	{
		return instance == null ? false : instance.endlessMode;
	}

	public static void OnDefeat()
	{
		if (instance != null)
			instance.wavesHaveLaunched = false;
	}

#endregion

#region DEBUG SKIP


	public static void DebugSkipWave()
	{
		DebugSkipToWave(instance.nextWave);
	}

	public static void DebugSkipToWave(int waveNumber)
	{
		//deferring this until the main update cycle. not entirely sure
		//when the OnGUI events trickle in, so best make sure
		//it isnt interrupting EnemyWave.UpdateTick (which has a while loop)
		instance.performSkipToWave = true;
		instance.skipToWaveTarget = waveNumber;
	}

	void PerformDebugSkipToWave()
	{
		if (!performSkipToWave)
			return;

		//clear out the previous stuff
		World.instance.DebugClearEnemyCharacters();

		if (skipToWaveTarget < instance.waves.Length)
		{
			//reset all the waves so skipping backwards in time doesnt explode
			Reset();

			endlessMode = false;
			nextWave = skipToWaveTarget;
			wavesHaveLaunched = false;
			WavesHUD.SetWaveProgress(instance.nextWave, 0.0f, ShowHordeIcon());
		}
		else
		{
			endlessModeWaveNumber = skipToWaveTarget;

			if (!randomiseEndlessWaves)
			{
				int endlessOnly = (endlessModeWaveNumber - waves.Length);
				int difficultyCount = WaveLoader.GetEndlessWaveDifficultyCount();

				endlessModeDifficulty = -1;
				for (int i = 0; i < difficultyCount; ++i)
				{
					var waveData = WaveLoader.GetEndlessWave(i);
					if (endlessOnly < waveData.Count)
					{
						endlessModeDifficulty = i;
						endlessModeDifficultyProgress = endlessOnly;

						int increases = endlessOnly/WaveLoader.inst.endlessGoldWavesPerIncrease;
						currentEndlessModeGoldBonus = WaveLoader.inst.endlessGoldBaseValue + increases*WaveLoader.inst.endlessGoldIncrease;

						break;
					}

					endlessOnly -= waveData.Count;
				}

				//skipped so far ahead that we ran out of wave data.
				if (endlessModeDifficulty == -1)
					useRandomEndlessWaves = true;
			}

			if (randomiseEndlessWaves || useRandomEndlessWaves)
			{
				//we skipped to an endless wave. need to load the correct bank
				//of data for the next random wave choice
				int endlessOnly = (endlessModeWaveNumber - waves.Length);
				int difficultyCount = WaveLoader.GetEndlessWaveDifficultyCount();
				int perDifficulty = WaveLoader.inst.endlessWaveCountPerDifficulty;

				endlessModeDifficulty = endlessOnly/perDifficulty;
				endlessModeDifficulty = Mathf.Clamp(endlessModeDifficulty, 0, difficultyCount);
				endlessModeDifficultyProgress = endlessOnly % perDifficulty;

				int increases = endlessOnly/WaveLoader.inst.endlessGoldWavesPerIncrease;
				currentEndlessModeGoldBonus = WaveLoader.inst.endlessGoldBaseValue + increases*WaveLoader.inst.endlessGoldIncrease;
			}

			InitialiseWaveData(WaveLoader.GetEndlessWave(endlessModeDifficulty));
			WavesHUD.SetWaveProgress(endlessModeWaveNumber, 0.0f, ShowHordeIcon());

			endlessMode = true;
		}

		HUD.instance.NewWaveReady();
		SpeedButtonControl.ResetSpeed();

		performSkipToWave = false;
		totalWavesComplete = Mathf.Max(0, skipToWaveTarget);

		ShowPathIndicators();
	}

#endregion

	void AddNextWaveHints()
	{
		if (nextWave >= waves.Length || FTUE.IsActive())
			return;

		var data = waves[nextWave];
		for (int i = 0; i < data.pathData.Count; ++i)
		{
			for (int j = 0; j < data.pathData[i].groups.Count; ++j)
			{
				var type = data.pathData[i].groups[j].enemyData.identifier;
				if (SaveData.ShowEnemyHint(type))
				{
					HintsPanel.AddHint(HintsDatabase.GetEnemyHintData(type));
					SaveData.ClearEnemyHint(type);
				}
			}
		}
	}

	void ShowPathIndicators()
	{
		var index = endlessMode ? endlessModeWaveIndex : nextWave;
		if (index < waves.Length)
		{
			var nextWaveData = waves[index];
			for (var i = 0; i < nextWaveData.pathData.Count; ++i)
			{
				if (nextWaveData.pathData[i].groups.Count > 0)
					PathNetwork.ShowPathIndicator(nextWaveData.pathData[i].pathStartNodeIndex);
			}
		}
	}

	bool ShowHordeIcon()
	{
		if (endlessMode)
			return waves[endlessModeWaveIndex].isHordeWave;

		if (wavesHaveLaunched)
			return waves[nextWave - 1].isHordeWave;
		
		if (nextWave < waves.Length && nextWave > 0)
			return waves[nextWave].isHordeWave;
		
		return false;
	}
}
