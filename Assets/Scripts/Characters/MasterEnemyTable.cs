using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterEnemyTable : MonoBehaviour 
{
	static MasterEnemyTable instance;

	public class Entry
	{
		public string identifier;
		public bool isBoss;

		public GameObject prefab;
		public GameObject prefabArt;
		public float artScale;

		public Entry onDeath;
		public List<Entry> spawnTypes = new List<Entry>();
		public int minSpawnCount;
		public int maxSpawnCount;

		public float health;
		public float speedMultiplier;
		public int damage;

		public EnemyAttributes abilities;
		public WeaponCategory categoryImmunities;
		public WeaponStatusEffectType statusEffectImmunities;

		public int rewardCoins;
		public int rewardExp;

		public float knockback;
		public float maxPathOffset;

		public float[] endlessModeHealthBaseMultiplier;
		public float[] endlessModeHealthPerWave;

		public float maxBerserkSpeed;

		public int regenHP;
		public float regenFrequency;
	}

	public TextAsset sourceData;
	public float baseMovementSpeed;

	public string serverLocation = "https://storage.googleapis.com/towers_test/Design/MasterEnemyTable.csv";
	public bool performServerLoad = true;

	Dictionary<string, Entry> masterEnemyTable = new Dictionary<string, Entry>();
	Dictionary<string, GameObject> enemyPrefabs = new Dictionary<string, GameObject>();

	void Awake()
	{
		instance = this;

		CollectPrefabs();

		Parse(sourceData.text);

		if (performServerLoad)
			StartCoroutine(LoadCSVData());
	}

	IEnumerator LoadCSVData()
	{
		var www = new WWW(serverLocation + "?t=" + UnityEngine.Random.value.ToString());
		yield return www;

		if (!string.IsNullOrEmpty(www.error))
		{
			Debug.Log("[MasterEnemyTable] server error: " + www.error);
		}
		else
		{
			Parse(www.text);
//			Debug.Log("[MasterEnemyTable] server data read");
		}
	}

	void Parse(string text)
	{
		//do this in two passes, the first parses all the enemy data into the table,
		//the second links up all the spawn types with references to the
		//actual data, rather than storing a list of string identifiers
		for (int i = 0; i < 2; ++i)
		{
			foreach (var line in text.Split('\n'))
			{
				var tokens = line.Trim().Split(',');
				if (tokens.Length == 0 || tokens[0].Length == 0 || tokens[0][0] == '#')
					continue;

				for (int j = 0; j < tokens.Length; ++j)
					tokens[j] = tokens[j].Trim();

				if (i == 0)
				{
					//get the existing entry, so we can overwrite its internals when the server CSV loads
					Entry entry;
					if (!masterEnemyTable.TryGetValue(tokens[0], out entry))
					{
						entry = new Entry();
						entry.identifier = tokens[0];
						masterEnemyTable.Add(tokens[0], entry);
					}

					entry.isBoss = CSVUtil.ParseInt(tokens, 1, 0) == 1;
					entry.prefab = enemyPrefabs["EN_" + tokens[2]];
					entry.prefabArt = enemyPrefabs["EN_" + tokens[2] + "_" + tokens[3]];
					entry.artScale = float.Parse(tokens[4]);

					//skip death change (5) and spawn types (6) for now

					int j = 7;
					entry.minSpawnCount = int.Parse(tokens[j++]);
					entry.maxSpawnCount = int.Parse(tokens[j++]);

					entry.health = float.Parse(tokens[j++]);
					entry.damage = int.Parse(tokens[j++]);
					entry.speedMultiplier = float.Parse(tokens[j++]);

					entry.abilities = ParseEnemyAttributes(tokens[j++], ';');
					entry.categoryImmunities = ParseWeaponCategory(tokens[j], ';'); //same CSV column as statusEffectImmunities
					entry.statusEffectImmunities = ParseWeaponStatusEffectType(tokens[j++], ';');

					entry.rewardCoins = int.Parse(tokens[j++]);
					entry.rewardExp = int.Parse(tokens[j++]);

					entry.knockback = float.Parse(tokens[j++]);
					entry.maxPathOffset = float.Parse(tokens[j++]);

					var healthTokens0 = CSVUtil.Tokenise(CSVUtil.ParseString(tokens, j++, "0"), ';');
					var healthTokens1 = CSVUtil.Tokenise(CSVUtil.ParseString(tokens, j++, "0"), ';');

					entry.endlessModeHealthBaseMultiplier = new float[healthTokens0.Length];
					entry.endlessModeHealthPerWave = new float[healthTokens1.Length];

					for (var k = 0; k < healthTokens0.Length; ++k)
						entry.endlessModeHealthBaseMultiplier[k] = CSVUtil.ParseFloat(healthTokens0, k, 1.0f); 

					for (var k = 0; k < healthTokens1.Length; ++k)
						entry.endlessModeHealthPerWave[k] = CSVUtil.ParseFloat(healthTokens1, k, 0.0f);

					entry.maxBerserkSpeed = CSVUtil.ParseFloat(tokens, j++, 4.0f);
					entry.regenHP = CSVUtil.ParseInt(tokens, j++, 2);
					entry.regenFrequency = CSVUtil.ParseFloat(tokens, j++, 1.0f);
				}
				else
				{
					var entry = Get(tokens[0]);
					entry.onDeath = Get(tokens[5].Trim());
					entry.spawnTypes.Clear();

					var types = tokens[6].Trim();
					if (!string.IsNullOrEmpty(types))
					{
						foreach (var type in types.Split(';'))
						{
							var data = Get(type.Trim());
							if (data != null)
								entry.spawnTypes.Add(data);
						}
					}
						
//					DebugPrint(entry);
				}
			}
		}
	}

	void OnDestroy()
	{
		instance = null;
	}

	public static Entry Get(string identifier)
	{
		Entry result;
		if (instance.masterEnemyTable.TryGetValue(identifier, out result))
			return result;

//		Debug.Log("[MasterEnemyTable] unknown enemy identifier: " + identifier);
		return null;
	}

	public static float BaseMovementSpeed()
	{
		return instance.baseMovementSpeed;
	}

	public static void DebugPrint(Entry entry)
	{
		string write = "[MasterEnemyTable] " + entry.identifier + "\n";
		write += "prefabs: " + entry.prefab.name + " | " + entry.prefabArt.name + "\n";
		write += "health: " + entry.health + ", speed: " + entry.speedMultiplier + "\n";

		string spawns = "";
		foreach (var type in entry.spawnTypes)
			spawns += type.identifier + " ";

		if (spawns == "")
			spawns = "nothing";
		else
			spawns += "[" + entry.minSpawnCount + ", " + entry.maxSpawnCount + "]";

		write += "spawns: " + spawns + "\n";
		write += entry.abilities + "\n";
		write += "category immunities: " + entry.categoryImmunities + "\n";
		write += "status immunities: " + entry.statusEffectImmunities + "\n";

		Debug.Log(write);
	}

	void CollectPrefabs()
	{
		//this will recurse through all sub folders
		var rootFolders = new string[] { "Enemies", "Enemies_SmallPool" };

		for (var i = 0; i < rootFolders.Length; ++i)
		{
			var resources = Resources.LoadAll<GameObject>(rootFolders[i]);
			foreach (var resource in resources)
				enemyPrefabs.Add(resource.name, resource);
		}
	}

	public static EnemyAttributes ParseEnemyAttributes(string data, char token_delimiter)
	{
		var attributeNames = System.Enum.GetNames(typeof(EnemyAttributes));
		var attributeValues = (int[])System.Enum.GetValues(typeof(EnemyAttributes));

		return (EnemyAttributes)Parse(data, attributeNames, attributeValues, token_delimiter);
	}

	public static WeaponCategory ParseWeaponCategory(string data, char token_delimiter)
	{
		var attributeNames = System.Enum.GetNames(typeof(WeaponCategory));
		var attributeValues = (int[])System.Enum.GetValues(typeof(WeaponCategory));

		return (WeaponCategory)Parse(data, attributeNames, attributeValues, token_delimiter);
	}

	public static WeaponStatusEffectType ParseWeaponStatusEffectType(string data, char token_delimiter)
	{
		var attributeNames = System.Enum.GetNames(typeof(WeaponStatusEffectType));
		var attributeValues = (int[])System.Enum.GetValues(typeof(WeaponStatusEffectType));

		return (WeaponStatusEffectType)Parse(data, attributeNames, attributeValues, token_delimiter);
	}

	static int Parse(string sourceData, string[] attributeNames, int[] attributeValues, char token_delimiter)
	{
		var tokens = sourceData.Trim().Split(token_delimiter);
		for (int i = 0; i < tokens.Length; ++i)
			tokens[i] = tokens[i].Trim();

		if (tokens.Length == 0 || string.IsNullOrEmpty(tokens[0]))
			return 0;

		var result = 0;

		for (int i = 0; i < tokens.Length; ++i)
		{
			for (int j = 0; j < attributeNames.Length; ++j)
			{
				if (tokens[i].ToLower() == attributeNames[j].ToLower())
					result |= attributeValues[j];
			}
		}

		return result;
	}
}
