using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData
{
	public string identifier;
    public string LeaderboardID;
    public string waves;
	public string[] endlessWaves;
	public int unlockRequirement;
	public int difficulty;
	public int cashReward;
	public string[] towerRewards;

	public Level prefab;
	public int lives;
	public int gold;

	public int[] starReqs; //TODO: legacy

	public string[] trinketIds;
	public string[] trinketEnemy;
	public float[] trinketChance;

	public float birdHeight = 3.0f;

	public int CalcStarRating(int waveNumber) //TODO: legacy
	{
		int result = 0;
		for (int i = 0; i < starReqs.Length; ++i)
			if (waveNumber >= starReqs[i])
				result = i + 1;

		return result;
	}

	//NB: if these change, be sure to update the UI display in LevelDetails.cs
	public int CalcStarRatingByRemainingLives(int remaining)
	{
		if (IsFTUE())
			return 3; //never fail
		
		//this will catch endless mode and failures to complete the level.
		if (remaining <= 0)
			return 0;
		
//		float percent = (float)remaining/(float)lives;
//		return percent >= 0.75f ? 3 : percent >= 0.25f ? 2 : 1;

		if (remaining >= lives)
			return 3;

		return (remaining < lives/2) ? 1 : 2;
	}

	public bool IsFTUE()
	{
		return identifier == "FTUE";
	}
};

public class LevelDatabase : MonoBehaviour 
{
	public TextAsset levelDataCSV;

	public static List<LevelData> levelData { get; private set; }

	//this maps tower ID to level ID.
	public static Dictionary<string, string> unlockableTowers { get; private set; }

    public float cashRate;

	void Awake()
	{
		levelData = new List<LevelData>();
		unlockableTowers = new Dictionary<string, string>();

		var lines = CSVUtil.Lines(levelDataCSV);
        
		for (int i = 0; i < lines.Length; ++i)
		{
			var tokens = CSVUtil.Tokenise(lines[i], ',');
			if (CSVUtil.SkipLine(tokens))
				continue;

			int j = 0;

            var data = new LevelData();
			data.identifier = tokens[j++];
            data.LeaderboardID = "Leaderboard_" + (i + 1);
            data.waves = tokens[j++];
			data.endlessWaves = CSVUtil.Tokenise(CSVUtil.ParseString(tokens, j++, ""), '&');
			data.unlockRequirement = CSVUtil.ParseInt(tokens, j++, 0);
			data.difficulty = CSVUtil.ParseInt(tokens, j++, 0);
			data.cashReward = (int)(CSVUtil.ParseInt(tokens, j++, 0)*cashRate);
			data.towerRewards = CSVUtil.Tokenise(CSVUtil.ParseString(tokens, j++, ""), '&');

			var prefabName = tokens[j++];
			var prefab = Resources.Load("Levels/" + prefabName) as GameObject;

			if (prefab != null)
			{
				data.prefab = prefab.GetComponent<Level>();
				data.prefab.Initialise(data); //basically just sorts the available tower list

				data.lives = CSVUtil.ParseInt(tokens, j++, 200);
				data.gold = CSVUtil.ParseInt(tokens, j++, 500);

				var stars = CSVUtil.Tokenise(tokens[j++], ';');
				data.starReqs = new int[stars.Length];
				for (int k = 0; k < stars.Length; ++k)
					data.starReqs[k] = CSVUtil.ParseInt(stars, k, 0);

				ParseTrinketData(data, CSVUtil.ParseString(tokens, j++, ""));

				data.birdHeight = CSVUtil.ParseFloat(tokens, j++, 3.0f);

				levelData.Add(data);

				//store locked towers for later
				for (int k = 0; k < data.towerRewards.Length; ++k)
				{
					if (!string.IsNullOrEmpty(data.towerRewards[k]))
					{
						unlockableTowers.Add(data.towerRewards[k], data.identifier);
						//Debug.Log("[LevelDatabase] " + data.identifier + " unlocks tower: " + data.towerRewards[k]);
					}
				}
			}
			else
			{
				Debug.Log("[LevelDatabase] could not load level prefab: " + prefabName);
			}
		}

		RefreshTotalStarRating();
	}

	void ParseTrinketData(LevelData levelData, string trinketData)
	{
		if (string.IsNullOrEmpty(trinketData))
			return;

		var trinketTokens = CSVUtil.Tokenise(trinketData, '&');
		if (trinketTokens == null || trinketTokens.Length == 0)
			return;

		levelData.trinketIds = new string[trinketTokens.Length];
		levelData.trinketEnemy = new string[trinketTokens.Length];
		levelData.trinketChance = new float[trinketTokens.Length];

		for (var i = 0; i < trinketTokens.Length; ++i)
		{
			var tokens = CSVUtil.Tokenise(trinketTokens[i], ';');
			if (tokens.Length >= 3)
			{
				levelData.trinketIds[i] = tokens[0];
				levelData.trinketEnemy[i] = tokens[1];
				levelData.trinketChance[i] = CSVUtil.ParseFloat(tokens, 2, 0.01f);
			}
		}

//		Debug.Log(levelData.trinketIds.Length.ToString() + " trinkets found for " + levelData.identifier);
	}
		
	public static void RefreshTotalStarRating()
	{
		int total = 0;
		foreach (var data in levelData)
		{
			if (!data.IsFTUE())
				total += Mathf.Min(SaveData.StarRating(data), 3);
		}

		SaveData.SetTotalStarCount(total);
	}

	public static int TotalStarsAvailable()
	{
		int total = 0;

		foreach (var data in levelData)
			if (!data.IsFTUE())
				total += 3;

		return total;
	}

	public static LevelData GetLevelData(string identifier)
	{
		for (var i = 0; i < levelData.Count; ++i)
			if (levelData[i].identifier == identifier)
				return levelData[i];

		return null;
	}

	public static void UnlockAll()
	{
		foreach (var data in levelData)
			SaveData.SetStarRating(data, 3);

		RefreshTotalStarRating();
	}
}
