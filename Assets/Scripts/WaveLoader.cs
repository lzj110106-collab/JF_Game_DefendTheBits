using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

public class WaveData
{
	public List<EnemyWave.PathData> pathData = new List<EnemyWave.PathData>();
	public int reward = 0;
	public bool isHorde = false;
}

public class WaveLoader : MonoBehaviour 
{
	Dictionary<string, List<WaveData>> completeWaves = new Dictionary<string, List<WaveData>>();

	[SerializeField] TextAsset[] WaveCSV;

	public int endlessGoldBaseValue = 700;
	public int endlessGoldIncrease = 100;
	public int endlessGoldWavesPerIncrease = 10;
	public int endlessWaveCountPerDifficulty = 20;

	public static WaveLoader inst;

	public string serverLocation = "https://storage.googleapis.com/towers_test/Waves/";
	public bool performServerLoad = true;

	void Awake()
	{
		inst = this;
	}

	void Start()
	{
		Load ();
	}

	public static List<WaveData> GetWave(string waveName)
	{
		List<WaveData> result = null;
		if (inst.completeWaves.TryGetValue(waveName, out result))
			return result;

		return null;
	}

	public static List<WaveData> GetEndlessWave(int difficulty)
	{
		var levelData = GameState.instance.level;
		return GetWave(levelData.endlessWaves[difficulty]);
	}

	public static int GetEndlessWaveDifficultyCount()
	{
		return GameState.instance.level.endlessWaves.Length;
	}

	public void Load()
	{
		for (int i = 0; i < WaveCSV.Length; i++)
		{
			if (WaveCSV[i] == null)
				continue;
			
			string waveName;
			var waveData = LoadWaveData(WaveCSV[i].name, WaveCSV[i].text, out waveName);

			if (inst.completeWaves.ContainsKey(waveName))
				Debug.Log("[WaveLoader] duplicate wave name in file: " + WaveCSV[i].name);
			else
				inst.completeWaves.Add(waveName, waveData);
		}

		if (performServerLoad)
		{
			for (var i = 0; i < WaveCSV.Length; ++i)
				if (WaveCSV[i] != null)
					StartCoroutine(LoadWaveData(WaveCSV[i].name));
		}
	}


	IEnumerator LoadWaveData(string csvName)
	{
		var path = serverLocation + csvName + ".csv?t=" + UnityEngine.Random.value.ToString();
		var www = new WWW(path);
		yield return www;

		if (!string.IsNullOrEmpty(www.error))
		{
//			Debug.Log("[WaveLoader] error reading " + path + ": " + www.error);
		}
		else
		{
//			Debug.Log("[WaveLoader] read: " + path);
			var waveName = "";
			var waveData = LoadWaveData(csvName, www.text, out waveName);
			inst.completeWaves[waveName] = waveData;
		}
	}

	List<WaveData> LoadWaveData(string csvName, string csvData, out string wavesName)
	{
		var lines = CSVUtil.Lines(csvData);
		var waves = new List<WaveData>();

		WaveData currentWave = null;

		//first two lines aren't valid. skip them.
		for (int i = 2; i < lines.Length; ++i)
		{
			var tokens = CSVUtil.Tokenise(lines[i]);	
			if (!CSVUtil.IsLineValid(tokens))
				continue;

			if (tokens[0] == "#reward") 
			{
				//need to protect against this, not all wave files have been
				//updated for the latest format yet.
				if (currentWave != null)
				{
					currentWave.reward = CSVUtil.ParseInt(tokens, 1, 0);
					currentWave = null;
				}
			}
			else if (tokens[0] == "#horde")
			{
				if (currentWave != null)
					currentWave.isHorde = true;
			}
			else
			{
				if (CSVUtil.IsLineComment(tokens))
					continue;

				var enemyData = MasterEnemyTable.Get(tokens[0]);
				if (enemyData != null)
				{
					if (currentWave == null)
					{
						currentWave = new WaveData();
						waves.Add(currentWave);
					}

					int j = 1;

					EnemyWave.EnemyGroup newGroup = new EnemyWave.EnemyGroup ();
					newGroup.enemyData = enemyData;
					newGroup.spawnStartDelay = CSVUtil.ParseFloat(tokens, j++, 0.0f);
					newGroup.enemyCount = CSVUtil.ParseInt(tokens, j++, 1);
					newGroup.enemySpawnSpacing = CSVUtil.ParseFloat(tokens, j++, 1.0f);
					newGroup.speedMultiplier = CSVUtil.ParseFloat(tokens, j++, 1.0f);

					//ignore empty groups.
					if (newGroup.enemyCount > 0)
					{
						//make sure the timing value for enemy spacing is valid. otherwise
						//we end up with heaps of enemies spawning on the same position
						//and the same frame which kills the framerate with some waves
						if (newGroup.enemySpawnSpacing <= 0.0f)
						{
							Debug.Log("[WaveLoader] invalid enemy spacing on line: " + i + " of " + csvName);
							newGroup.enemySpawnSpacing = 0.5f;
						}

						if (newGroup.speedMultiplier <= 0.0f)
						{
							Debug.Log("[WaveLoader] invalid enemy speed multiplier on line: " + i + " of " + csvName);
							newGroup.speedMultiplier = 1.0f;
						}
							

						//default to the first path if no entry is found in the wave file
						int pathIndex = CSVUtil.ParseInt(tokens, j++, 0);

						//generate new path objects on demand.
						while (pathIndex >= currentWave.pathData.Count)
						{
							int size = currentWave.pathData.Count;

							currentWave.pathData.Add(new EnemyWave.PathData());
							currentWave.pathData[size].pathStartNodeIndex = size; //make sure to assign the start node
						}

						currentWave.pathData[pathIndex].groups.Add(newGroup);
					}
				}
			}
		}

		//TODO: should change this to use the CSV file name. 
		wavesName = CSVUtil.Tokenise(lines[0])[0];

		return waves;
	}
    
    public void PassedLevel()
    {
        GameState.TriggerEOR();
//#if UNITY_IOS && !UNITY_EDITOR
//        print("显示互推。");
//        ShowPromote(Screen.width*(3.0f / 5.0f), Screen.height * (1.0f / 6.0f));
//#endif

    }

    GameObject enemyParent;
    public void CleanEnemy()
    {
        enemyParent = GameObject.Find("container_enemy");

        for(int i = 0; i < enemyParent.transform.childCount; i++)
        {
            enemyParent.transform.GetChild(i).GetComponent<EnemyCharacter>().KilledRightNow();
        }
    }

    //[DllImport("__Internal")]
    //private static extern void ShowPromote(float x, float y);
}