using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintData
{
	public HintTypes hintType;
	public HintCircleTypes circleType;

	public string identifier;
	public int waveNumber; //NB: 1-indexing

	public string headerLocID;
	public string textDot1LocID;
	public string textDot2LocID;
	public string textDot3LocID;
	public string textFooterLocID;
	public string[] weaknesses;
	public string rewards;

	public Sprite imageIcon;
	public Sprite imgPortrait;

	public bool forceDisplay = false;

	public delegate void OnHintDismissed();
	public OnHintDismissed onHintDismissed;
}

public enum HintTypes
{
	Hint,
	NewEnemy
}

public enum HintCircleTypes
{
	Standard,
	DragTower,
	StartWave,
	UpgradeTower,
	Enemy,
	Trinkets,
	BasicEnemy,
	BigEnemy
}

public class HintsDatabase : MonoBehaviour 
{
	static HintsDatabase instance;

	public TextAsset sourceCSV;

	//every single hint listed in the CSV file
	List<HintData> totalHintData = new List<HintData>();

	//faster lookups
	Dictionary<string, HintData> enemyHintData = new Dictionary<string, HintData>();
	List<HintData> enemyHintDataOrdered = new List<HintData>();

	Dictionary<string, HintData> ftueHintData = new Dictionary<string, HintData>();
	List<HintData> ftueHintDataOrdered = new List<HintData>();

	Dictionary<string, List<HintData>> levelHintData = new Dictionary<string, List<HintData>>();
	List<List<HintData>> levelHintDataOrdered = new List<List<HintData>>();

	Dictionary<string, HintData> codexHints = new Dictionary<string, HintData>();
	List<HintData> codexHintsOrdered = new List<HintData>();

	const string hintImageLocation = "FTUE/";

	void Awake()
	{
		instance = this;

		//track the level ordering as we go. flatten into an ordered array at the end
		var levelDataOrdering = new Dictionary<string, int>();
		var levelCounter = 0;

		var typeNames = (string[])System.Enum.GetNames(typeof(HintTypes));
		var typeValues = (int[])System.Enum.GetValues(typeof(HintTypes));

		var circleNames = (string[])System.Enum.GetNames(typeof(HintCircleTypes));
		var circleValues = (int[])System.Enum.GetValues(typeof(HintCircleTypes));

		var lines = CSVUtil.Lines(sourceCSV);
		for (int i = 0; i < lines.Length; ++i)
		{
			var tokens = CSVUtil.Tokenise(lines[i]);
			if (CSVUtil.SkipLine(tokens))
				continue;

			var data = new HintData();
			var j = 0;

			data.identifier = CSVUtil.ParseString(tokens, j++, "none");
			data.waveNumber = CSVUtil.ParseInt(tokens, j++, 1);

			data.headerLocID = CSVUtil.ParseString(tokens, j++, "");
			data.hintType = (HintTypes)CSVUtil.ParseEnum(CSVUtil.ParseString(tokens, j++, ""), typeNames, typeValues);
			data.circleType = (HintCircleTypes)CSVUtil.ParseEnum(CSVUtil.ParseString(tokens, j++, ""), circleNames, circleValues);

			data.textDot1LocID = CSVUtil.ParseString(tokens, j++, "");
			data.textDot2LocID = CSVUtil.ParseString(tokens, j++, "");
			data.textDot3LocID = CSVUtil.ParseString(tokens, j++, "");
			data.textFooterLocID = CSVUtil.ParseString(tokens, j++, "");

			data.imageIcon = Resources.Load<Sprite>(hintImageLocation + CSVUtil.ParseString(tokens, j++, ""));
			data.imgPortrait = Resources.Load<Sprite>(hintImageLocation + CSVUtil.ParseString(tokens, j++, ""));
			data.weaknesses = CSVUtil.Tokenise(CSVUtil.ParseString(tokens, j++, ""), '&');

			// TODO: Skip immunities column since it's not used atm, remove later
			j++;
			data.rewards = CSVUtil.ParseString(tokens, j++, "");


			if (MasterEnemyTable.Get(data.identifier) != null)
			{
				enemyHintData[data.identifier] = data;
				enemyHintDataOrdered.Add(data);
			}
			else if (data.identifier.Contains("FTUE"))
			{
				ftueHintData[data.identifier] = data;
				ftueHintDataOrdered.Add(data);

				data.forceDisplay = true;
			}
			else if (data.identifier.Contains("_codex"))
			{
				codexHints[data.identifier] = data;
				codexHintsOrdered.Add(data);
			}
			else
			{
				if (!levelHintData.ContainsKey(data.identifier))
				{
					levelHintData[data.identifier] = new List<HintData>();
					levelDataOrdering[data.identifier] = levelCounter++;
				}

				levelHintData[data.identifier].Add(data);
			}

			totalHintData.Add(data);
		}
			
		//flatten the level hints into the correct order
		{
			var temp = new List<HintData>[levelCounter];
			foreach (var kv in levelDataOrdering)
				temp[kv.Value] = levelHintData[kv.Key];

			levelHintDataOrdered = new List<List<HintData>>(temp);
		}
	}

	void OnDestroy()
	{
		instance = null;
	}

	public static HintData GetEnemyHintData(string enemyIdentifier)
	{
		if (instance != null)
			return GetHintData(instance.enemyHintData, enemyIdentifier);
		
		return null;
	}

	public static HintData GetLevelHintData(string levelIdentifier, int waveNumber)
	{
		if (instance != null)
		{
			List<HintData> levelHints;
			if (instance.levelHintData.TryGetValue(levelIdentifier, out levelHints))
			{
				for (int i = 0; i < levelHints.Count; ++i)
					if (levelHints[i].waveNumber == waveNumber + 1) //account for 1-indexing
						return levelHints[i];
			}
		}

		return null;
	}

	public static HintData GetFTUEHintData(string identifier)
	{
		if (instance != null)
			return GetHintData(instance.ftueHintData, identifier);

		return null;
	}

	static HintData GetHintData(Dictionary<string, HintData> source, string identifier)
	{
		HintData result;
		source.TryGetValue(identifier, out result);
		return result;
	}

	public static Dictionary<string, HintData> AllEnemyHints()
	{
		return instance == null ? null : instance.enemyHintData;
	}

	public static Dictionary<string, HintData> AllFTUEHints()
	{
		return instance == null ? null : instance.ftueHintData;
	}

	public static Dictionary<string, List<HintData>> AllLevelHints()
	{
		return instance == null ? null : instance.levelHintData;
	}

	public static List<HintData> AllEnemyHintsOrdered()
	{
		return instance == null ? null : instance.enemyHintDataOrdered;
	}

	public static List<HintData> AllFTUEHintsOrdered()
	{
		return instance == null ? null : instance.ftueHintDataOrdered;
	}

	public static List<List<HintData>> AllLevelHintsOrdered()
	{
		return instance == null ? null : instance.levelHintDataOrdered;
	}

	public static List<HintData> CodexHints()
	{
		return instance == null ? null : instance.codexHintsOrdered;
	}
}
