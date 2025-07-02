using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class World : MonoBehaviour 
{
	public static World instance;

	LevelData levelData;
	bool hasCompletedLevel;
	bool shouldTriggerEOR;

	[HideInInspector] public PathNetwork pathNetwork;
	[HideInInspector] public PlayerCharacter[] playerCharacters;
	
	[HideInInspector] public Collider terrainCollider { get; private set; }
	
	[HideInInspector] public PlayerCharacter selectedPlayerCharacter;

	public bool paused { get; private set; }
	public float timeScale { get; private set; }

	EnemyWaveController waveController;

	enum PoolIndex { Enemy, Interact, Projectile, Tower };
	GameObject[] containers; //live pooled objects are moved to here

	public Character[] characters { get; private set; }
	public InteractObject[] interactObjects { get; private set; }
	public Projectile[] projectiles { get; private set; }
	public Tower[] towers { get; private set; }

	int characterCount = 0;
	int interactObjectCount = 0;
	int projectileCount = 0;
	int towerCount = 0;

	public string musicToPlay;

	TowerContextMenu towerContextMenu;

	//query classes to avoid multiple standard library allocations every frame.
	//NB: these are reused constantly, so dont cache them in caller objects
	WorldQuery<Character> characterQuery;
	WorldQuery<Tower> towerQuery;

	public WorldTargetFinder targetFinder { get; private set; }

	//class to manage PFX and stuff that sits on the track
	PathOverlay pathOverlay;

	//manually creating our own version of UpdateFixed that isnt tied to physics calculations.
	//doing this because simply doubling the timeScale causes the game to play differently
	//due to enemies moving further in a single frame (and thus avoiding some towers entirely).
	public const float frameTime = 1.0f/60.0f;
	float frameTimer = 0.0f;
	public static int frameNumber;

	//not sure how to mark input events as 'used'
	public static bool enableTowerSelection;

	public Dictionary<string, int> killsThisRound;

	void Awake() { instance = this; }
	void OnDestroy() { instance = null; }

	public void Initialise(LevelData level)
	{
		levelData = level;
		WorldAnalytics.SetLevel(level);

		paused = false;
		timeScale = 1.0f;

		pathNetwork = GetComponentInChildren<PathNetwork>(true);
		playerCharacters = GetComponentsInChildren<PlayerCharacter>();
		towerContextMenu = GameObject.Find ("/UI").GetComponentsInChildren<TowerContextMenu>(true)[0];
       
        PlayLevelMusic();
		terrainCollider = GetComponentInChildren<MeshCollider>();
        

        //generate container objects for the different object types
        var poolNames = System.Enum.GetNames(typeof(PoolIndex));
		containers = new GameObject[poolNames.Length];
		for (int i = 0; i < containers.Length; ++i)
		{
			containers[i] = new GameObject("container_" + poolNames[i].ToLower());
			containers[i].transform.SetParent(transform, this);
		}

		//hard limit on the amount of stuff to spawn into the world.
		characters = new Character[1024];
		interactObjects = new InteractObject[64];
		projectiles = new Projectile[1024];
		towers = new Tower[256];

		//init queries
		characterQuery = new WorldQuery<Character>(characters.Length);
		towerQuery = new WorldQuery<Tower>(characters.Length);

		//generate path overlay stuff
		GetComponentInChildren<PathNetwork>(true).Initialise();
		pathOverlay = new PathOverlay();

		waveController = new EnemyWaveController(level.waves);
		waveController.waveLaunchedCallback = OnWaveLaunched;
		waveController.waveCompleteCallback = OnWaveComplete;
		waveController.waveSpawningCompleteCallback = OnWaveSpawningComplete;

		targetFinder = new WorldTargetFinder(characterQuery);

		killsThisRound = new Dictionary<string, int>();

		Restart();
	}

	public void Restart()
	{
		HintsPanel.ClearHints();

		//reset the landscape flags and mesh
		{
			for (int i = 0; i < towerCount; ++i)
				Landscape.instance.RemoveFlag(towers[i].tileX, towers[i].tileY, TileFlag.BuildingExists_RuntimeAssigned);

			Landscape.instance.RebuildTowerMesh();
		}

		//reset all our update arrays and the pooling systems
		{
			NotifyWorldObjectsOfReset(projectiles, projectileCount);
			NotifyWorldObjectsOfReset(characters, characterCount);
			NotifyWorldObjectsOfReset(towers, towerCount);
			NotifyWorldObjectsOfReset(interactObjects, interactObjectCount);

			EnemyPool.Reset();
			InteractObjectPool.Reset();
			PFXPool.Reset();
			ProjectilePool.Reset();
			RangeObjectPool.Reset();
			TowerPool.Reset();

			characterCount = 0;
			interactObjectCount = 0;
			projectileCount = 0;
			towerCount = 0;
			frameNumber = 0;
		}

		//reset all the waves and show a hint for the first one
		PathNetwork.ClearPathIndicators();
		waveController.ResetLevel();

		//clear FF data etc
		HUD.ShowHint(0);
		HUD.LockStartWaveButton();

		WavesHUD.Reset();
		SpeedButtonControl.ResetSpeed();

		pathOverlay.Clear();
		targetFinder.Clear();
		killsThisRound.Clear();

		hasCompletedLevel = false;
		paused = false;
	}

    /// <summary>
    /// 貌似是性能卡顿的关键点
    /// </summary>
	void Update()
	{
		//restore tower selection inputs
		enableTowerSelection = true;

        if (Input.GetKey(KeyCode.F1))
            GameState.TriggerEOR();

        if (hasCompletedLevel && shouldTriggerEOR)
		{
			//wait until all the interact objects have been collected/despawned
			if (interactObjectCount == 0 && GameState.instance.currentState == GameState.State.Game)
			{
				//trigger the EOR
				ClearSaveState(); //no longer requred.
				GameState.TriggerEOR();

				//disable FF
				SpeedButtonControl.ResetSpeed();
				SetTimescale(1.0f);

				//the player won. trigger victory anims
				PlayTowerVictoryAnimations();

				shouldTriggerEOR = false;
			}
		}

		//update world objects first, they take input priority
		if (!paused)
		{
			frameTimer += Time.deltaTime * timeScale;
			while (frameTimer >= frameTime)
			{
				FTUE.UpdateTick();

				pathOverlay.UpdateTick();
				waveController.UpdateTick();
				targetFinder.Clear();

				UpdateWorldObjects(characters, ref characterCount);
				UpdateWorldObjects(projectiles, ref projectileCount);
				UpdateWorldObjects(interactObjects, ref interactObjectCount);
				UpdateWorldObjects(towers, ref towerCount);

				HUD.UpdateAbilities();

				frameTimer -= frameTime;
				frameNumber += 1;
			}

			//characters update animators at the end of the frame because they
			//contain no animation triggers that are frame dependent, unlike
			//towers which send attack events. this saves alot of frames
			//on slow devices in the later waves
			NotifyWorldObjectsOfUpdateComplete(characters, characterCount);
		}
			
		//now deal with tower selection things
		if (!enableTowerSelection)
			return;

		if (InputUtil.MousePressed() && InputUtil.IsWorldHovered())
		{
			var chosenTower = PickTower();
			if (chosenTower != null)
			{
				if (!chosenTower.HandleMousePick())
				{
					RangeObjectPool.Reset();
					chosenTower.PlaceRangeObjects ();

					//clear the current selection first, otherwise the BuildMenu
					//will also register a click outside of itself, and then
					//hide the info panel
					TowerBuildMenu.ClearSelection();
					TowerInfoPanel.ShowUpgradeInfo(chosenTower);
				}
			}
			else
			{
				if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
				{
					towerContextMenu.Hide ();
					RangeObjectPool.Reset();
				}

				TowerInfoPanel.Hide();
			}
		}
	}

	void LateUpdate()
	{
		pathOverlay.LateUpdate();
	}

    public void PassedLevel()
    {
        GameState.TriggerEOR();
    }

#region Time

	public void TogglePause()
	{
		paused = !paused;

		//pass pause state into the world to stop animations and pfx and stuff
		PauseWorldObjects(characters, characterCount, paused);
		PauseWorldObjects(interactObjects, interactObjectCount, paused);
		PauseWorldObjects(projectiles, projectileCount, paused);
		PauseWorldObjects(towers, towerCount, paused);
	}

	public void SetTimescale(float timeScale)
	{
		this.timeScale = timeScale;

		AdjustWorldObjectsTimeScale(characters, characterCount, timeScale);
		AdjustWorldObjectsTimeScale(interactObjects, interactObjectCount, timeScale);
		AdjustWorldObjectsTimeScale(projectiles, projectileCount, timeScale);
		AdjustWorldObjectsTimeScale(towers, towerCount, timeScale);

//		TowerInfoPanel.Hide(); //dont do this. annoying.
	}

#endregion

#region Characters

	public Character SpawnCharacter(GameObject prefab)
	{
		if (prefab == null)
		{
			Debug.Log ("World.SpawnEnemy: null character prefab");
			return null;
		}
		
//		var go = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;

		var go = EnemyPool.Get(prefab);
		var c = go.GetComponent<Character>();
		c.sourcePrefab = prefab;
		c.transform.SetParent(containers[(int)PoolIndex.Enemy].transform);

		AddWorldObject(c, characters, ref characterCount);
		return c;
	}

	public static EnemyCharacter SpawnEnemyCharacter(MasterEnemyTable.Entry enemyData)
	{
		return (EnemyCharacter)instance.SpawnCharacter(enemyData.prefab);
	}

	public void OnCharacterKilled(Character character, bool wasKilledByPlayer)
	{
		//let everybody know that this character has died (for target switching or whatever)
		for (var i = 0; i < characterCount; ++i)
			characters[i].OnCharacterKilled(character);

		for (var i = 0; i < projectileCount; ++i)
			projectiles[i].OnCharacterKilled(character);

		if (character.type == CharacterType.Enemy)
		{
			for (var i = 0; i < towerCount; ++i)
				towers[i].OnCharacterKilled(character);
		}

		//if this was an enemy, see if it spawns a trinket on this level
		if (character is EnemyCharacter && wasKilledByPlayer)
		{
			var enemyIdentifier = ((EnemyCharacter)character).enemyDataInitial.identifier;

			if (levelData.trinketEnemy != null) //FTUE has no trinkets.
			{
				for (int i = 0; i < levelData.trinketEnemy.Length; ++i)
				{
					if (levelData.trinketEnemy[i] == enemyIdentifier)
					{
						//first trinket drops with 100% success rate, and throws up a hint panel
						if (/*PlayerPrefs.GetInt("FTUE_Trinkets", 1) == 1*/ObscuredPrefs.GetInt("FTUE_Trinkets", 1) == 1)
						{
							HintsPanel.AddHint(HintsDatabase.GetFTUEHintData("FTUE_Trinkets"));
							//PlayerPrefs.SetInt("FTUE_Trinkets", 0);

                            ObscuredPrefs.SetInt("FTUE_Trinkets", 0);
                        }
						else
						{
							if (Random.value > levelData.trinketChance[i])
								continue;
						}

						//now generate the trinket
						var prefab = TrinketDatabase.GetPrefab(levelData.trinketIds[i]);
                        //print("生成小饰品");
						if (prefab != null)
						{
							var io = InteractObjectPool.Get(prefab).GetComponent<InteractObject>();
							io.OnSpawned(prefab, character.transform.position);
							AddInteractObject(io, io.gameObject);
						}
					}
				}
			}

			if (!FTUE.IsActive())
			{
				int current = 0;
				if (killsThisRound.TryGetValue(enemyIdentifier, out current))
					killsThisRound[enemyIdentifier] = current + 1;
				else
					killsThisRound.Add(enemyIdentifier, 1);
			}
		}

		//don't remove the character from the world yet. if we do, then
		//the character wont animate its death sequence (due to doing
		//everything manually via UpdateTick).
	}

	public void DebugClearEnemyCharacters()
	{
		NotifyWorldObjectsOfReset(characters, characterCount);
		EnemyPool.Reset();
	}

	public int CharacterCount()
	{
		return characterCount;
	}

#endregion

#region towers

	public void AddTower(Tower t)
	{
		t.transform.SetParent (containers[(int)PoolIndex.Tower].transform);

		AddWorldObject(t, towers, ref towerCount);
		RefreshTowerBuffs();
	}

	public void RemoveTower(Tower t)
	{
		//just remove buffs, let the tower return false in UpdateObjects
		//and have it removed there so that iteration orders dont
		//get messed up.
		RefreshTowerBuffs();
	}

	public void RefreshTowerBuffs()
	{
		for (int i = 0; i < towerCount; ++i)
			towers[i].ClearBuffs();

		for (int i = 0; i < towerCount; ++i)
		{
			var tower = towers[i];

			if (tower.support == null)
				continue;

			var plotData = towers[i].support.plotData;
			for (int j = 0; j < plotData.Count; ++j)
			{
				int x = (int)(tower.tileX + plotData[j].x);
				int y = (int)(tower.tileY + plotData[j].y);

				var other = FindTower(x, y);
				if (other != null && other != tower)
					other.AddBuff(tower);
			}
		}

		for (int i = 0; i < towerCount; ++i)
			towers[i].RefreshWeaponData();
	}
		
	void PlayTowerVictoryAnimations()
	{
		for (var i = 0; i < towerCount; ++i)
			towers[i].PlayVictoryAnimations();
	}

	void PlayTowerIdleAnimations()
	{
		for (var i = 0; i < towerCount; ++i)
			towers[i].PlayIdleAnimations();
	}

#endregion

#region projectiles

	public void AddProjectile(Projectile p, GameObject baseObject)
	{
		AddWorldObject(p, projectiles, ref projectileCount);
		baseObject.transform.SetParent(containers[(int)PoolIndex.Projectile].transform);
	}

#endregion

#region interact rewards

	public void AddInteractObject(InteractObject io, GameObject baseObject)
	{
		AddWorldObject(io, interactObjects, ref interactObjectCount);
		baseObject.transform.SetParent(containers[(int)PoolIndex.Interact].transform);
	}

	public static InteractReward AddInteractReward(GameObject prefab, Vector3 location)
	{
		var prefabInstance = InteractObjectPool.Get(prefab);
		if (prefabInstance != null)
		{
			var io = prefabInstance.GetComponent<InteractReward>();
			io.OnSpawned(prefab, location);
			instance.AddInteractObject(io, io.gameObject);

			return io;
		}
			
		return null;
	}

#endregion

#region object searching

	public WorldQuery<Tower> FindTowers(Vector3 pos, float radius)
	{
		float radius2 = radius*radius;

		towerQuery.Clear();

		for (var i = 0; i < towerCount; ++i)
		{
			if (Vector3.SqrMagnitude(pos - towers[i].transform.position) <= radius2)
				towerQuery.Add(towers[i]);
		}

		return towerQuery;
	}

	public Tower FindTower(int tileX, int tileY)
	{
		if (!Landscape.instance.HasFlag(tileX, tileY, TileFlag.BuildingExists_RuntimeAssigned))
			return null;
	
		//TODO: could store an array of towers and get direct access
		for (var i = 0; i < towerCount; ++i)
			if (towers[i].tileX == tileX && towers[i].tileY == tileY)
				return towers[i];

		return null;
	}

	public static WorldQuery<Character> FindCharactersInTileRange(Weapon weapon, CharacterType type)
	{
		return instance.targetFinder.FindCharactersInTileRange(weapon);
	}
		
	public WorldQuery<Character> FindCharactersInArea(Vector3 pos, float radius, CharacterType type)
	{
		return instance.targetFinder.FindCharactersInSplashArea(pos, radius, null);
	}

	public WorldQuery<Character> FindCharactersInArea(List<Vector2> plotData, int tileX, int tileY, bool testCentre = true)
	{
		return targetFinder.FindCharactersInTileRange(plotData, plotData.Count, tileX, tileY, testCentre);
	}

	public WorldQuery<Character> FindCharactersInArea(List<Vector2> plotData, int plotCount, int tileX, int tileY, bool testCentre = true)
	{
		return targetFinder.FindCharactersInTileRange(plotData, plotCount, tileX, tileY, testCentre);
	}
		
	public WorldQuery<Character> FindCharactersInSplashArea(Weapon weapon, CharacterType type, Vector3 position)
	{
		return targetFinder.FindCharactersInSplashArea(weapon, position);
	}

#endregion

#region object selection
	public void SelectPlayer(PlayerCharacter character)
	{
		selectedPlayerCharacter = character;
		//TODO: ui hook
	}
#endregion

#region mouse picking
	public bool FindIntersectionWithWorld(Ray r, out Vector3 pos)
	{
		var info = new RaycastHit();
		if (terrainCollider.Raycast(r, out info, float.MaxValue))
		{
			pos = r.GetPoint(info.distance);
			return true;
		}

		pos = Vector3.zero;
		return false;
	}

	public Character PickCharacter(Ray r)
	{
		RaycastHit info;
		for (int i = 0; i < characterCount; ++i)
		{
			if (characters[i].cachedCollider.Raycast(r, out info, float.MaxValue))
				return characters[i];
		}

		return null;
	}

	public Character PickCharacterOfType(Ray r, CharacterType type)
	{
		RaycastHit info;
		for (int i = 0; i < characterCount; ++i)
		{
			var c = characters[i];

			if (c.type != type)
				continue;

			if (c.cachedCollider.Raycast(r, out info, float.MaxValue))
				return c;
		}
		
		return null;
	}

	public Tower PickTower()
	{
		Ray ray = Camera.main.ScreenPointToRay(InputUtil.MousePosition());

		int tileX = 0;
		int tileY = 0;

		if (Landscape.instance.PickTile(ray, ref tileX, ref tileY, TileFlag.BuildingExists_RuntimeAssigned))
		{
			for (int i = 0; i < towerCount; ++i)
			{
				if (towers[i].tileX == tileX && towers[i].tileY == tileY)
					return towers[i];
			}
		}

		return null;

//		Tower result = null;
//		float closest = float.MaxValue;
//
//		for (int i = 0; i < towerCount; ++i)
//		{
//			if (towers[i] == null)
//				continue;
//			
//			var renderers = towers[i].GetComponentsInChildren<MeshRenderer>(true);
//			if (renderers == null)
//				continue;
//			
//			for (int j = 0; j < renderers.Length; ++j)
//			{
//				if (renderers[j] == null)
//					continue;
//				
//				float u = 0;
//				if (renderers[j].bounds.IntersectRay(ray, out u) && u < closest)
//				{
//					closest = u;
//					result = towers[i];
//				}
//			}
//		}
//
//		return result;
	}
#endregion

#region world objects

	bool AddWorldObject(WorldObject toAdd, WorldObject[] existingObjects, ref int objectCount)
	{
		if (objectCount < existingObjects.Length)
		{
			existingObjects[objectCount] = toAdd;
			objectCount += 1;

			return true;
		}

		return false;
	}

	bool RemoveWorldObject(WorldObject toRemove, WorldObject[] existingObjects, ref int objectCount)
	{
		//TODO: store an index in WorldObject for fast removal
		for (int i = 0; i < objectCount; ++i)
		{
			if (existingObjects[i] == toRemove)
			{
				existingObjects[i] = existingObjects[objectCount - 1];
				objectCount -= 1;

				return true;
			}
		}

		return false;
	}
		
	void UpdateWorldObjects(WorldObject[] objects, ref int objectCount)
	{
		for (int i = 0; i < objectCount; ++i)
		{
			if (!objects[i].UpdateTick())
			{
				objects[i] = objects[objectCount - 1];
				objectCount -= 1;
				i -= 1;
			}
		}
	}

	void PauseWorldObjects(WorldObject[] objects, int objectCount, bool pause)
	{
		for (var i = 0; i < objectCount; ++i)
			objects[i].OnPause(pause);
	}

	void AdjustWorldObjectsTimeScale(WorldObject[] objects, int objectCount, float timeScale)
	{
		for (var i = 0; i < objectCount; ++i)
			objects[i].OnTimeScaleAdjusted(timeScale);
	}

	void NotifyWorldObjectsOfReset(WorldObject[] objects, int objectCount)
	{
		for (var i = 0; i < objectCount; ++i)
			objects[i].OnWorldReset();
	}

	void NotifyWorldObjectsOfUpdateComplete(WorldObject[] objects, int objectCount)
	{
		for (var i = 0; i < objectCount; ++i)
			objects[i].UpdateTicksComplete();
	}

#endregion

#region WAVES

	public void OnWaveLaunched()
	{
		for (var i = 0; i < towerCount; ++i)
			towers[i].OnWaveLaunched();

		HUD.OnWaveLaunched();
		FTUE.OnWaveLaunched();

//		TowerInfoPanel.Hide();
	}

	public void OnWaveComplete(int waveNumber, int waveReward, bool allWavesComplete)
	{
		for (var i = 0; i < towerCount; ++i)
			towers[i].OnWaveComplete();

		//do all the HUD stuff. if the EOR is triggered, then
		//the HUD is hidden anyway, and when the EOR is
		//dismissed to continue in endless mode, 
		//the normal rewards and stuff should show up
		HUD.OnWaveComplete(waveNumber, waveReward);
		WavesHUD.OnWaveComplete(waveNumber, waveReward);
		FTUE.OnWaveComplete(waveNumber);

//		TowerInfoPanel.Hide(); //dont do this anymore. annoying.

		if (allWavesComplete)
		{
			//wait for interact objects to clear
			HUD.LockStartWaveButton();
			hasCompletedLevel = true;
			shouldTriggerEOR = true;
		}
		else
		{
			SaveState(); //store progress for resumption

			//FTUE hints for abilities. only show them once
//			if (!FTUE.IsActive())
//			{
//				if (waveNumber == 8 && PlayerPrefs.GetInt("FTUE_Ability_0", 1) == 1)
//				{
//					HintsPanel.AddHint(HintsDatabase.GetFTUEHintData("FTUE_Ability_0"));
//					PlayerPrefs.SetInt("FTUE_Ability_0", 0);
//				}
//				else if (waveNumber == 9 && PlayerPrefs.GetInt("FTUE_Ability_1", 1) == 1)
//				{
//					HintsPanel.AddHint(HintsDatabase.GetFTUEHintData("FTUE_Ability_1"));
//					PlayerPrefs.SetInt("FTUE_Ability_1", 0);
//				}
//			}
		}
	}

	public void OnWaveSpawningComplete()
	{
		for (var i = 0; i < towerCount; ++i)
			towers[i].OnWaveSpawningComplete();

		FTUE.OnWaveSpawningComplete();
	}

	public static void OnDefeat()
	{
		//as long as the player gained a single star, trigger the victory animation
		if (instance.levelData.CalcStarRating(EnemyWaveController.WavesCompleted()) > 0)
			instance.PlayTowerVictoryAnimations();

		EnemyWaveController.OnDefeat(); //stop enemies spawning

		//force kill all remaining enemies
		for (var i = 0; i < instance.characterCount; ++i)
			instance.characters[i].ApplyDamage(1e8f);

		//turn off FF
		SpeedButtonControl.ResetSpeed();
		instance.SetTimescale(1.0f);
	}

	public static void TriggerEndlessMode()
	{
		EnemyWaveController.TriggerEndlessMode();
		instance.PlayTowerIdleAnimations();
		instance.PlayLevelMusic();

		instance.hasCompletedLevel = false;
		instance.shouldTriggerEOR = false;
	}

#endregion

#region SERIALISATION

	public void SaveState()
	{
		//dont save state in FTUE
		if (FTUE.IsActive())
			return;
		
		int headerSize = System.Enum.GetValues(typeof(SaveStateHeader)).Length;
		int towerDataSize = System.Enum.GetValues(typeof(SaveStateTowerData)).Length;

		//store basic info in PlayerPrefs so we can easily display
		//restoration information in a resume dialog.
		var headerData = new List<string>(headerSize);

		//storing tower information in persistant data so that
		//it doesnt blow up the player pref string length limitations
		var towerData = new List<string>(towerCount * towerDataSize);

		headerData.Add(levelData.identifier);
		headerData.Add(EnemyWaveController.CurrentWaveNumber().ToString());
		headerData.Add(HUD.instance.goldRemaining.ToString());
		headerData.Add(HUD.instance.livesRemaining.ToString());
		headerData.Add(towerCount.ToString());

		for (int i = 0; i < towerCount; ++i)
		{
			towerData.Add(towers[i].towerInfo.name);
			towerData.Add(towers[i].currentLevel.ToString());
			towerData.Add(towers[i].tileX.ToString());
			towerData.Add(towers[i].tileY.ToString());

			if (towers[i].weapon != null)
			{
				towerData.Add(towers[i].weapon.plotRotation.ToString());
				towerData.Add(towers[i].weapon.totalKills.ToString());
			}
			else
			{
				//plot rotation.
				//TODO: this should be stored on the tower.
				towerData.Add("0");

				//stat tracking
				if (towers[i].spawnCoins)
					towerData.Add(towers[i].spawnCoins.totalCoinsCollected.ToString());
				else
					towerData.Add("0");
			}
		}

		var header = string.Join(",", headerData.ToArray());
		var body = string.Join(",", towerData.ToArray());
			
		//PlayerPrefs.SetString("save_state", header);

        ObscuredPrefs.SetString("save_state", header);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/save_state.txt", body);
	}

	public bool LoadState(string[] header)
	{
		var towerSize = System.Enum.GetValues(typeof(SaveStateTowerData)).Length;
		var path = Application.persistentDataPath + "/save_state.txt";

		if (System.IO.File.Exists(path))
		{
			var towerData = CSVUtil.Tokenise(System.IO.File.ReadAllText(path));

			Initialise(LevelDatabase.GetLevelData(header[0]));

			for (var i = 0; i < towerData.Length; i += towerSize)
			{
				var info = TowerLoader.GetTowerInfo(towerData[i]);
				if (info != null)
				{
					var j = i + 1;

					var prefab = info[0].prefab.gameObject;

					var tower = TowerPool.Get(prefab).GetComponent<Tower>();
					tower.Initialise(prefab, info);
					tower.ClearCreationTime();

					tower.SetTowerLevel(int.Parse(towerData[j++]));
					tower.SetTile(int.Parse(towerData[j++]), int.Parse(towerData[j++]), true);
					Landscape.instance.AddFlag(tower.tileX, tower.tileY, TileFlag.BuildingExists_RuntimeAssigned);

					if (tower.weapon != null)
					{
						tower.weapon.SetPlotRotation(int.Parse(towerData[j++]));
						tower.weapon.OnLoad(int.Parse(towerData[j++]));
					}
					else
					{
						j++; //TODO: restore rotation

						if (tower.spawnCoins)
							tower.spawnCoins.OnLoad(int.Parse(towerData[j++]));
					}
						
					AddTower(tower);
				}
			}

			EnemyWaveController.DebugSkipToWave(int.Parse(header[(int)SaveStateHeader.WaveNumber]));
			HUD.instance.SetGold(int.Parse(header[(int)SaveStateHeader.GoldRemaining]));
			HUD.instance.SetLives(int.Parse(header[(int)SaveStateHeader.LivesRemaining]));
			HUD.UnlockStartWaveButton(false); //unlock play button immediately for save state resume

			return true;
		}
		else
		{
			Debug.Log("could not find: " + path);
		}

		return false;
	}

	void ClearSaveState()
	{
		//PlayerPrefs.SetString("save_state", "");
        ObscuredPrefs.SetString("save_state", "");
    }

#endregion

//	void OnGUI()
//	{
//		var style = new GUIStyle();
//		style.fontSize = (int) (Screen.height * 0.06f);
//		style.normal.textColor = Color.white;
//
//		GUI.Label(new Rect(Screen.width * 0.025f, Screen.height * 0.25f, 100, 100), 
//				  EnemyWaveController.WavesCompleted().ToString(),
//				  style);        
//	}

	void OnDrawGizmos()
	{
		pathOverlay.OnDrawGizmos();
	}

	void PlayLevelMusic()
	{
		if (musicToPlay != null && !AudioController.IsPlaying (musicToPlay)) {
			AudioController.GetAudioItem(musicToPlay)._lastChosen = -1;
			AudioController.Play (musicToPlay); // Trigger Per level Music
		}
	}
}