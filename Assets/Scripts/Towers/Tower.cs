using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Tower: MonoBehaviour, WorldObject
{
	List<TowerInfo> allTowerInfo;

	GameObject sourcePrefab;
	GameObject sourcePrefabArt;

	public TowerInfo towerInfo { get; private set; }
	public TowerArtHooks towerArtInfo { get; private set; }

	public WeaponData weaponData { get; private set; }
	public WeaponData weaponDataBuffs { get; private set; } //from support towers

	public string towerName; //TODO: get rid of this. grab via towerInfo instead so keep the ID solely defined by the CSV 
	public int level { get; private set; }

	public Sprite icon;
	public Weapon weapon { get; private set; }

	//tower can have multiple upgrade paths
	public List<GameObject> towerUpgradePrefabs;
	public TowerDefensePFXContainer pfx { get; private set; } //assigned via TowerArtHooks in SetVars

	List<GameObject> passivePFX = new List<GameObject>();
	List<GameObject> sourcePassivePFX = new List<GameObject>();

	public Animator animator { get; private set; }

	public int tileX { get; private set; }
	public int tileY { get; private set; }
	
	public bool isBeingPlaced { get; private set; }
	public bool isBeingRotated { get; private set; }
	public bool canBePlacedAtCurrentLocation { get; private set; }
	public bool waitingOnPlacementConfirmation { get; private set; }

	MaterialSwapper materialSwapper = new MaterialSwapper();

	[System.NonSerialized] public int currentLevel = 1;

	//the firing animations are scaled. so simply adjusting animator
	//speeds to match the values coming in from OnTimeScaleAdjusted
	//wont work correctly.
	[HideInInspector] public float localTimeScale;

	//additional scripts for specialised towers. need to store references here
	//so they go through the same UpdateTick code as the rest of the towers.
	//could subclass this script I guess, but storing references works
	//just as well.
	public TowerSpawnCoins spawnCoins { get; private set; }
	public TowerSupport support { get; private set; }

	public bool shouldDestroyTower = false;

	public float creationTime { get; private set; }

	//bonus comes from global upgrades
	public int bonusGoldRewardForEnemyKill { get; private set; }

	bool[] validRotations = new bool[4];
	int validRotationsFound = 0;

	public void Initialise(GameObject prefab, List<TowerInfo> sourceData)
	{
		gameObject.SetActive(true);

		sourcePrefab = prefab;
		allTowerInfo = sourceData;
		shouldDestroyTower = false;

		//need to store this as the sell price changes
		//a few seconds after the tower has been placed
		creationTime = Time.time;

		//weapon init
		{
			weaponData = new WeaponData();
			weaponDataBuffs = new WeaponData();

			weapon = GetComponent<Weapon>();
			if (weapon != null)
				weapon.Initialise(this);
		}

		isBeingPlaced = false;
		canBePlacedAtCurrentLocation = false;
		localTimeScale = 1.0f;

		//cache this value once at load so we dont need to do upgrade calculations
		//for every kill (could get expensive with area effects in the later levels)
		bonusGoldRewardForEnemyKill = TowerLoader.GetBonusGoldRewardForEnemyKill(sourceData[0].name);

		spawnCoins = GetComponent<TowerSpawnCoins>();
		support = GetComponent<TowerSupport>();

		SetTowerLevel(0);

		//overwrite the victory animation from last round
		animator.Play("Idle", 0, 0.0f);
		animator.Update(World.frameTime);
	}

	public void ClearCreationTime()
	{
		creationTime = 0.0f;
	}

	public void SetTowerLevel(int level)
	{
		towerInfo = allTowerInfo[level];
		currentLevel = level;

		//regenerate the weapon data stats for the new tower upgrade.
		RefreshWeaponData();

		//clear previous art out.
		ReturnArtToPool();

		//pull new art in
		sourcePrefabArt = towerUpgradePrefabs[currentLevel];
		towerArtInfo = TowerPool.Get(sourcePrefabArt).GetComponent<TowerArtHooks>();
		towerArtInfo.GetComponent<TowerAnimationResponder>().SetParentTower(this);
		towerArtInfo.gameObject.SetActive(true);
		towerArtInfo.transform.SetParent(transform, false);
		towerArtInfo.PFX.Initialise(transform); //sets up faster access to PFX prefabs

		//local set up for things.
		//TODO: i think the weapon controls all of this stuff
		animator = towerArtInfo.GetComponent<Animator>();
		animator.enabled = false; //updating animators manually
		pfx = towerArtInfo.PFX;

		//set-up passive PFX effects
		for (int i = 0, j = (int)PFX.Tower_Passive; i < 3; ++i, ++j)
		{
			var passiveData = pfx.entriesOrdered[j];
			if (passiveData != null)
			{
				var passiveInstance = pfx.Play(PFX.Tower_Passive, false);
				if (passiveInstance != null)
				{
					sourcePassivePFX.Add(passiveData.prefab);
					passivePFX.Add(passiveInstance.gameObject);
				}
			}
		}

		if (weapon != null)
			weapon.OnTowerInitialised(this);

		if (spawnCoins)
			spawnCoins.Initialise();

		if (support != null)
			World.instance.RefreshTowerBuffs();

		materialSwapper.CacheMaterials(gameObject);
	}

	public virtual bool UpdateTick()
	{
		//make sure all tower destruction happens in such a way
		//that it doesnt explode the World tower list.
		if (shouldDestroyTower)
		{
			TowerPool.Return(sourcePrefab, gameObject);
			ReturnArtToPool();
			return false;
		}

		//dont update until confirmation has happened
		if (waitingOnPlacementConfirmation)
			return true;

		if (isBeingRotated)
		{
			if (!TowerContextMenu.instance.gameObject.activeSelf)
				TowerContextMenu.instance.ShowRotationContext (this);
		}

		//only fire weapons when we arent interacting with tower placement stuff
		if (weapon != null && !isBeingPlaced && !isBeingRotated)
			weapon.UpdateTick();
		 
		if (spawnCoins != null)
			spawnCoins.UpdateTick();

		if (support != null)
			support.UpdateTick();

		if (animator != null && (!isBeingPlaced && !isBeingRotated))
			animator.Update(World.frameTime);

		return true; //always alive
	}

	public virtual void UpdateTicksComplete()
	{
	}

	public virtual bool HandleMousePick()
	{
		if (spawnCoins != null)
			return spawnCoins.HandleMousePick();

		return false;
	}
		
	public bool DoesPlacementRequireRotation()
	{
		return towerInfo.plotName.Contains("MELEE") || towerInfo.plotName.Contains("LASER");
	}

	public void ContextMenuDismissed(TowerContextMenu.Type type)
	{
		if (type == TowerContextMenu.Type.Rotation)
			isBeingRotated = false;

		if (type == TowerContextMenu.Type.Placement)
			waitingOnPlacementConfirmation = false;
	}

	//this function is basically determining a reasonable starting rotation
	//for the tower given its location on the map. this is done to save
	//the player having to rotate melee towers to face a path when
	//the desired rotation should be obvious given the surrounds of 
	//the tower. the placement UI is also shown, just in case.
	public int CalculateInitialRotation()
	{
		//clear previous data
		{
			RemoveRangeObjects();
			validRotationsFound = 0;

			for (int i = 0; i < validRotations.Length; ++i)
				validRotations[i] = false;
		}

		//multiple rotations may be valid, so attempt to rotate the
		//tower so that it faces towards the start of the enemy
		//path. do this using the floodfill index, where
		//an index of zero is the final point in the path
		int flood = 0;

		if (weapon == null)
		{
			//face adjacent paths if possible
			int[] adjX = new int[4] { -1, 1, 0, 0 };
			int[] adjY = new int[4] { 0, 0, -1, 1 };

			int result = 0;

			for (int i = 0; i < 4; ++i)
			{
				int floodFillIndex = Landscape.GetFloodFillIndex(tileX + adjX[i], tileY + adjY[i]);
				if (floodFillIndex > flood)
				{
					result = i;
					flood = floodFillIndex;
				}
			}
					
			var pos = Landscape.instance.GetTileCentre(tileX + adjX[result], tileY + adjY[result]);
			var angle = MathUtil.GetAngleInDegreesToPositionXZ(transform.position, pos);
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);

			//never show rotation context for weapon-less towers
			validRotationsFound = 1;
		}
		else if (RangePlots.IsPlotSymmetrical(towerInfo.plotName))
		{
			var plotData = RangePlots.GetPlotData(towerInfo.plotName, 0);

			int result = 0;

			for (var i = 0; i < plotData.Count; ++i)
			{
				int x = tileX + (int)(plotData[i].x);
				int y = tileY + (int)(plotData[i].y);

				int floodFill = Landscape.GetFloodFillIndex(x, y);
				if (floodFill > flood)
				{
					int up = 0, right = 1, down = 2, left = 3;

					//ignore tiles that arent in line with the tower
					if (x == tileX)
					{
						result = y < tileY ? down : up;
						flood = floodFill;
					}
					else if (y == tileY)
					{
						result = x < tileX ? left : right;
						flood = floodFill;
					}
				}
			}

			validRotations[result] = true;
			validRotationsFound = 1;

			weapon.SetPlotRotation(result);
			PlaceRangeObjects();
		}
		else
		{
			int result = 0;

			//search for valid rotations
			for (int i = 0; i < 4; ++i)
			{
				var plotData = RangePlots.GetPlotData(towerInfo.plotName, i);

				for (int j = 0; j < plotData.Count; ++j)
				{
					int x = tileX + (int)(plotData[j].x);
					int y = tileY + (int)(plotData[j].y);

					//flood fill index tells us if there is a path
					int floodFill = Landscape.GetFloodFillIndex(x, y);
					if (floodFill != -1)
					{
						if (floodFill > flood)
						{
							//this is the best rotation found so far.
							result = i;
							flood = floodFill;
						}

						validRotations[i] = true;
					}
				}
					
				if (validRotations[i])
					validRotationsFound += 1;
			}

			//assign the rotation we found
			weapon.SetPlotRotation(result);
			PlaceRangeObjects();
		}

		if (validRotationsFound == 0)
			transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up); 	//face the camera

		return validRotationsFound;
	}

	public void Rotate(int dir)
	{
		for (var i = 1; i <= 4; ++i)
		{
			int rotation = (weapon.plotRotation + 4 + i*dir) % 4;
			if (validRotations[rotation])
			{
				weapon.SetPlotRotation(rotation);
				break;
			}
		}

		RemoveRangeObjects ();
		PlaceRangeObjects ();
	}


	public virtual void OnPause(bool pause)
	{
		//do nothing. World doesnt called UpdateTick when paused
	}

	public virtual void OnTimeScaleAdjusted(float timeScale)
	{
		//do nothing. World calls UpdateTick multiple times for FF
	}

	public virtual void OnWorldReset()
	{
		ReturnArtToPool();

		if (weapon != null)
			weapon.OnWorldReset();

		if (spawnCoins != null)
			spawnCoins.OnWorldReset();
	}

	void ReturnArtToPool()
	{
		ClearPlacementShaders();

		if (towerArtInfo != null)
		{
			TowerPool.Return(sourcePrefabArt, towerArtInfo.gameObject);
			sourcePrefabArt = null;
			towerArtInfo = null;
		}

		//return all passive PFX
		{
			for (int i = 0; i < sourcePassivePFX.Count; ++i)
				PFXPool.Return(sourcePassivePFX[i], passivePFX[i]);
			
			sourcePassivePFX.Clear();
			passivePFX.Clear();
		}
	}

	public virtual void SetTile(int tileX, int tileY, bool suppressPlacementShaders = false)
	{
		this.tileX = tileX;
		this.tileY = tileY;

		var landscape = Landscape.instance;

		var buildable = landscape.HasFlag(tileX, tileY, TileFlag.Buildable);
		var buildingExists = landscape.HasFlag(tileX, tileY, TileFlag.BuildingExists_RuntimeAssigned);

		if (buildable && !buildingExists)
		{
			//if FTUE is active, we need to make sure we can place the tower at this location. some
			//steps in the FTUE process require placing towers at exact locations
			if (!FTUE.IsActive() || FTUE.CanPlaceTowerAtLocation(tileX, tileY))
			{
				transform.position = Landscape.instance.GetTileCentre(tileX, tileY);

				canBePlacedAtCurrentLocation = true;

				if (!suppressPlacementShaders)
					UpdatePlacementShaders(true);

				return;
			}
		}

		//shift the position up so that the tower sits on top of the landscape mesh.
		//this is done to avoid z-fighting artifacts caused by the non-buildable
		//part of the landscape mesh being fixed. 
		var position = landscape.GetTileCentre(tileX, tileY);
		position.y += landscape.towerDepth * landscape.tileHeight;
		transform.position = position;

		canBePlacedAtCurrentLocation = false;

		if (!suppressPlacementShaders)
			UpdatePlacementShaders(false);
	}
		
	public void StartPlacement()
	{
//		SetTowerLevel(0);
//
//		if (animator != null)
//			animator.enabled = false;

		isBeingPlaced = true;

		InitialisePlacementShaders();
		World.instance.AddTower(this);
	}

	public bool ConfirmPlacement()
	{
		int price = (int)(towerInfo.price * TowerLoader.GetTowerUpgradePriceMultiplier(towerInfo.name));

		if (canBePlacedAtCurrentLocation && HUD.instance.CanAfford(price))
		{
			Landscape.instance.AddFlag(tileX, tileY, TileFlag.BuildingExists_RuntimeAssigned);
			Landscape.instance.RemoveFlag(tileX, tileY, TileFlag.BuildingWillBePlaced_RuntimeAssigned);
			Landscape.instance.RebuildTowerMesh();
			HUD.instance.MakePurchase(price);

			if (animator != null)
			{
				animator.Play ("Spawn", 0, 0.0f);
				AudioController.Play ("Spawn");
			}

			pfx.Play(PFX.Tower_OnSpawn, true);
			isBeingPlaced = false;

			ClearPlacementShaders();

			//show the rotation submenu if alternate valid rotations have been found
			if (validRotationsFound > 1)
			{
				TowerContextMenu.instance.ShowRotationContext (this);
				isBeingRotated = true;
			} 
			else
			{
				FinalisePlacementOnNonRotaters ();
			}

			if (weapon != null)
				weapon.GeneratePlotMinimalSet();

			if (HUD.instance.showPlacementConfirmationDialog)
			{
				TowerContextMenu.instance.ShowPlacementContext(this);
				waitingOnPlacementConfirmation = true;
			}
			else
			{
				waitingOnPlacementConfirmation = false;
			}

			if (FTUE.IsActive())
			{
				FTUE.OnTowerPlaced(this);
			}
			else
			{
				//this will unlock the play button for the first wave. we do this here rather
				//than in World.AddTower because that gets called before the tower has
				//been placed in a valid position.
				HUD.UnlockStartWaveButton(false);
			}

			//reset this now, otherwise the time it took to place the tower
			//will be factored into the sell price manipulation stuff
			creationTime = Time.time;

			WorldAnalytics.CreateTower(this);
			return true;
		}
		else
		{
			//cant afford. throw the drag-and-drop away
			CancelPlacement();
			return false;
		}
	}

	public void CancelPlacement()
	{
		shouldDestroyTower = true;
		ClearPlacementShaders();
		RemoveRangeObjects();
	}

	public void FinalisePlacementOnNonRotaters()
	{
		isBeingRotated = false;
		TowerContextMenu.instance.Hide ();
		RemoveRangeObjects();
	}

	public void InitialisePlacementShaders()
	{
		if (materialSwapper != null)
			materialSwapper.SetShader(MaterialCache.instance.unlitFlashShader);
	}

	public void UpdatePlacementShaders(bool validPosition)
	{
		if (materialSwapper != null)
		{
			Color c = validPosition ? MaterialCache.instance.towerValidPlacementHighlightColour :
									  MaterialCache.instance.towerInvalidPlacementHighlightColour;

			materialSwapper.SetMaterialColour("_Color", c);
		}
	}

	public void ClearPlacementShaders()
	{
		if (materialSwapper != null)
			materialSwapper.RestoreMaterials();
	}

	public void ConfirmPlacementContextMenu()
	{
		waitingOnPlacementConfirmation = false;
	}

	public void CancelPlacementContextMenu()
	{
		Sell(CalculateTotalCost(), true);
	}

	public void SetSelectionHighlight()
	{
		if (materialSwapper != null)
			materialSwapper.SetMaterial(MaterialCache.instance.selectedTowerMaterial);
	}

	public void ClearSelectionHighlight()
	{
		if (materialSwapper != null)
			materialSwapper.RestoreMaterials();

		if (isBeingRotated)
		{
			FinalisePlacementOnNonRotaters();
			isBeingRotated = false;
		}

		RemoveRangeObjects();
	}

	public void Sell(int sellPrice, bool force)
	{
		if (!isBeingPlaced || force)
		{
			Landscape.instance.RemoveFlag(tileX, tileY, TileFlag.BuildingExists_RuntimeAssigned);
			Landscape.instance.RebuildTowerMesh();
			World.instance.RemoveTower(this);
			WorldAnalytics.SellTower(this);
			RemoveRangeObjects();

			HUD.instance.MakeSale(sellPrice);
			shouldDestroyTower = true;
		}
	}

	public int CalculateTotalCost()
	{
		int result = 0;

		for (int i = 0; i <= currentLevel && i < allTowerInfo.Count; ++i)
			result += allTowerInfo[i].price;

		return (int)(result * TowerLoader.GetTowerUpgradePriceMultiplier(towerInfo.name));
	}

	public virtual Tower Upgrade()
	{
		if (CanUpgrade())
		{
			//record the current level of the tower, not the upgraded level
			WorldAnalytics.UpgradeTower(this);

			SetTowerLevel(currentLevel + 1);
			AudioController.Play ("Spawn");
			pfx.Play(PFX.Tower_OnSpawn, true);

			RemoveRangeObjects();
			PlaceRangeObjects();

			if (spawnCoins)
				spawnCoins.OnTowerUpgraded();

			if (weapon)
				weapon.OnTowerUpgraded();

			//cancel the early sale hack thing.
			ClearCreationTime();

			FTUE.OnTowerUpgraded(this);
		}

		return this;
	}

	public bool CanUpgrade()
	{
		//taking the min here just in case the art and csv info don't match yet
		return currentLevel < Mathf.Min(allTowerInfo.Count, towerUpgradePrefabs.Count) - 1;
	}

	public virtual void OnCharacterKilled(Character character)
	{
		if (weapon != null && character is EnemyCharacter)
			weapon.OnEnemyDestroyed((EnemyCharacter)character);
	}

	void SetMaterial(Material material)
	{
		foreach (var renderer in GetComponentsInChildren<MeshRenderer>(true))
			renderer.sharedMaterial = material;
	}


	public void PlaceRangeObjects()
	{
		if (weapon != null)
		{
			var materialType = RangePlotQuad.MaterialType.Normal;
			if (isBeingPlaced)
				materialType = canBePlacedAtCurrentLocation ? RangePlotQuad.MaterialType.WillPlaceValid :
															  RangePlotQuad.MaterialType.WillPlaceInvalid;

			for (int i = 0; i < weapon.plotData.Count; i++)
			{
				RangeObjectPool.PlaceAt(tileX + (int)weapon.plotData[i].x,
										tileY + (int)weapon.plotData[i].y,
										materialType);
			}
		}
	}

	public void RemoveRangeObjects()
	{
		RangeObjectPool.Reset();

		//range object pool is set up to recycle everything at once,
		//so call out to the FTUE to replace its placement markers
		//whenever the tower kills its own markers. this will
		//happen as the tower is dragged around during placement
		FTUE.RefreshPlacementMarker();
	}

#region WAVES

	public void OnWaveLaunched()
	{
		if (spawnCoins != null)
			spawnCoins.OnWaveLaunched();
	}

	public void OnWaveComplete()
	{
		if (spawnCoins != null)
			spawnCoins.OnWaveComplete();
	}

	public void OnWaveSpawningComplete()
	{
	}

#endregion

#region SUPPORT TOWERS

	public void AddBuff(Tower other)
	{
		weaponDataBuffs.Merge(other.support.weaponBuffs);
	}

	public void ClearBuffs()
	{
		weaponDataBuffs.Clear();
	}

#endregion

	public void RefreshWeaponData()
	{
		weaponData.Clear();

		weaponData.Merge(towerInfo.weaponData); //base stats for this tower level
		weaponData.Merge(TowerLoader.GetGlobalWeaponUpgrades(towerInfo.name), false); //upgrades via cash/trinkets (multiply status effects)
		weaponData.Merge(weaponDataBuffs); //support towers

		if (weapon != null)
			weapon.OnWeaponDataChanged(this);
	}


	public void OnAnimEnd()
	{
		if (weapon != null)
			weapon.OnAnimEnd();
			
	}

	public void PlayVictoryAnimations()
	{
		animator.Play("Victory", 0, UnityEngine.Random.Range(0.0f, 0.99f));
	}

	public void PlayIdleAnimations()
	{
		animator.Play("Idle", 0, 0.0f);
	}
}
