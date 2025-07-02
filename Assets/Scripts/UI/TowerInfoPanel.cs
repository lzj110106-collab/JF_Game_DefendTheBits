using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TowerInfoPanel : MonoBehaviour 
{
	static TowerInfoPanel instance;

	public float timeout = 10.0f;
	float elapsed;
	bool isVisible = false;

	public Animator anim;

	[Header("Description")]
	public Text txtName;
	public Text txtDescription;
	public Text txtDescriptionNext;
	public Text txtAbilityDescriptionNext;
	public GameObject currentAbilitiesHierarchy;
	public Image[] imgAbilityIcons;
	public Image imgAbilityIconNext;
	public Image imgIconNext;
	public GameObject nextLevelHierarchy;
	public GameObject nextAbilityHierarchy;
	string[] abilityIconNames;

	[Header("Stats")]
	public Text statPR;
	public Text statDamage;
	public Text statAttacksPerSecond;

	[Header("Buttons")]
	public Button sellButton;
	public Text sellButtonCostText;
	public GameObject sellNormalHierarchy;
	public GameObject sellConfirmHierarchy;

	public GameObject upgradeButtonPurchaseHierarchy;
	public GameObject upgradeButtonMaxHierarchy;

	public Button upgradeButton;
	public Text upgradeButtonCostText;
	public Image upgradeButtonBG;

	public GameObject starContainer;

	public Color statDefaultColor = Color.white;
	public Color statUpgradeColor = Color.green;

	public Color upgradeButtonColor = Color.white;
	public Color upgradeButtonColorUnaffordable = Color.grey;

	[Header("SELL")]
	public float fullPriceTimeOut = 5.0f;
	public float sellPriceMultiplier = 0.5f;
	int sellPrice;
	float sellConfirmElapsed;
	bool waitingForSellConfirm;

	[Header("TOTALS")]
	public GameObject icnCoinsSpawned;
	public GameObject icnKillCount;
	public Text txtRunningTotal;

	Tower referenceTower;

	void Awake() 
	{ 
		instance = this; 
		abilityIconNames = new string[imgAbilityIcons.Length];
	}

	void OnDestroy() 
	{ 
		instance = null; 
	}

	public static void ShowInfo(TowerBuildMenuButton towerButton)
	{	
		TowerContextMenu.instance.Hide();

		//show the base tower details when selecting unbuilt towers
		instance._ShowInfo(towerButton.towerPrefab.GetComponent<Tower>(), 0);
	}

	public static void ShowInfo(Tower tower)
	{
		TowerContextMenu.instance.Hide();

		//pass in the current level of the built tower.
		instance._ShowInfo(tower, tower.currentLevel);
	}

	public static void ShowUpgradeInfo(Tower tower)
	{
		TowerContextMenu.instance.Hide();

		//if this is a selection switch, make sure to clear the previous highlight
		if (instance.referenceTower != null)
			instance.referenceTower.ClearSelectionHighlight();
		
		instance._ShowUpgradeInfo(tower);

		//set up the new one
		if (instance.referenceTower != null)
		{
			instance.referenceTower.SetSelectionHighlight();
			instance.referenceTower.PlaceRangeObjects();
		}
	}

	public static void Hide()
	{
		instance.anim.SetBool("Show", false);
		instance.isVisible = false;

		if (instance.referenceTower != null)
		{
			instance.referenceTower.ClearSelectionHighlight();
			instance.referenceTower.RemoveRangeObjects();
			instance.referenceTower = null;
		}
	}

	public static bool IsVisible()
	{
		return instance.isVisible;
	}

	public static bool IsShowingUpgrade()
	{
		return instance.referenceTower != null;
	}

	//TODO: localise
	void _ShowInfo(Tower tower, int level)
	{
		var info = TowerLoader.GetTowerInfo(tower.towerName, level);
		var info1 = TowerLoader.GetTowerInfo(tower.towerName, tower.currentLevel + 1);
		bool levelCapped = (info1 == null);

		//make sure we are displaying data that includes the global weapon upgrades
		var weaponData = new WeaponData();
		weaponData.Merge(info.weaponData);
		weaponData.Merge(TowerLoader.GetGlobalWeaponUpgrades(info.name));

		// DESCRIPTIONS
		info.AssignName(txtName);
		info.AssignDescription(txtDescription, level);

		nextLevelHierarchy.SetActive(!levelCapped);

		if (!levelCapped)
		{
			info1.AssignDescription(txtDescriptionNext, level + 1);
			imgIconNext.sprite = GetIcon(tower, Mathf.Clamp(level + 1, 0, 6));
			//txtNextLevel.text = "LEVEL " + (level + 2).ToString();
		}

		// STATS
		statPR.text = info.rating.ToString();

		InitialiseAbilityIcons(info, currentAbilitiesHierarchy, imgAbilityIcons, abilityIconNames);
			
		SetCategoryInfo(tower.GetComponent<Weapon>());
		SetStatusEffectInfo(weaponData.statusEffects, weaponData.statusEffectCount);
		SetTargettingInfo(weaponData.targetingFlags);

		instance.anim.SetBool("Show", true);

		AudioController.Play ("UI_Popup");

		//this should only be being called from TowerBuildMenuButtons.
		//TODO: pass in a TowerBuildMenuButton instead
		referenceTower = null;
		sellButton.gameObject.SetActive(false);
		upgradeButton.gameObject.SetActive(false);
		ShowStarRating(0);

		if (info1 != null)
		{
			var weapon1 = new WeaponData();
			weapon1.Merge(info1.weaponData);
			weapon1.Merge(TowerLoader.GetGlobalWeaponUpgrades(info.name));

			//TODO: need to somehow display which effects are new with the upgrade
			SetCategoryInfo(tower.weapon);
			SetStatusEffectInfo(weapon1.statusEffects, weapon1.statusEffectCount);
			SetTargettingInfo(weapon1.targetingFlags);

			InitialiseNextAbilityDisplay(info, info1);
		}
		else
		{
			SetCategoryInfo(tower.weapon);
			SetStatusEffectInfo(weaponData.statusEffects, weaponData.statusEffectCount);
			SetTargettingInfo(weaponData.targetingFlags);

			HideNextAbilityDisplay();
		}

		isVisible = true;
		elapsed = 0.0f;

		SetSellHierarchy(false);
	}

	void _ShowUpgradeInfo(Tower tower)
	{
		var info0 = TowerLoader.GetTowerInfo(tower.towerName, tower.currentLevel);
		var info1 = TowerLoader.GetTowerInfo(tower.towerName, tower.currentLevel + 1);
		bool levelCapped = (info1 == null);

		//TODO: localise
		if (info0 != null)
		{
			//need to be displaying upgraded info, not the base stats
			var weapon0 = new WeaponData();
			weapon0.Merge(info0.weaponData);
			weapon0.Merge(TowerLoader.GetGlobalWeaponUpgrades(info0.name));

			info0.AssignName(txtName);
			info0.AssignDescription(txtDescription, 0); //always show the base tower description in the top panel

			nextLevelHierarchy.SetActive(!levelCapped);

			if (!levelCapped)
			{
				info1.AssignDescription(txtDescriptionNext, tower.currentLevel + 1);
				imgIconNext.sprite = GetIcon(tower, Mathf.Clamp(tower.currentLevel + 1, 0, 6));
			}

			//set defaults. only change these values if an upgrade exists
			statPR.text = info0.rating.ToString();
			statDamage.text = weapon0.damage.ToString();
			statAttacksPerSecond.text = weapon0.attacksPerSecond.ToString();

			InitialiseAbilityIcons(info0, currentAbilitiesHierarchy, imgAbilityIcons, abilityIconNames);

			if (info1 != null)
			{
				var weapon1 = new WeaponData();
				weapon1.Merge(info1.weaponData);
				weapon1.Merge(TowerLoader.GetGlobalWeaponUpgrades(info0.name));

				//TODO: need to somehow display which effects are new with the upgrade
				SetCategoryInfo(tower.weapon);
				SetStatusEffectInfo(weapon1.statusEffects, weapon1.statusEffectCount);
				SetTargettingInfo(weapon1.targetingFlags);

				InitialiseNextAbilityDisplay(info0, info1);
			}
			else
			{
				SetCategoryInfo(tower.weapon);
				SetStatusEffectInfo(weapon0.statusEffects, weapon0.statusEffectCount);
				SetTargettingInfo(weapon0.targetingFlags);

				HideNextAbilityDisplay();
			}
		}
			
		anim.SetBool("Show", true);

		AudioController.Play ("UI_CharSelect");

		referenceTower = tower;
		sellButton.gameObject.SetActive(true);
		upgradeButton.gameObject.SetActive(true);
		upgradeButton.enabled = tower.CanUpgrade();

		bool canUpgrade = referenceTower.CanUpgrade();

		upgradeButtonPurchaseHierarchy.SetActive(canUpgrade);
		upgradeButtonMaxHierarchy.SetActive(!canUpgrade);

		if (canUpgrade)
		{
			var price = (int)(info1.price * TowerLoader.GetTowerUpgradePriceMultiplier(info1.name));
			upgradeButtonCostText.text = price.ToString();
		}
		else
		{
			upgradeButtonBG.color = upgradeButtonColorUnaffordable;
		}

		ShowStarRating(tower.currentLevel);

		instance.isVisible = true;
		elapsed = 0.0f;
		SetSellHierarchy(false);
	}

	void SetCategoryInfo(Weapon weapon)
	{
		if (weapon != null)
		{
			var names = (string[])System.Enum.GetNames(typeof(WeaponCategory));
			var values = (WeaponCategory[])System.Enum.GetValues(typeof(WeaponCategory));

			var found = 0;
			var result = "";

			for (var i = 0; i < names.Length; ++i)
			{
				if ((weapon.category & values[i]) != 0)
				{
					result += names[i];
					found += 1;
				}
			}

			if (found == 0)
				result = "Default";

			//categoryDisplay.text = "Category: " + result;
		}
		else
		{
			//categoryDisplay.text = "Category: Support";
		}
	}

	void SetStatusEffectInfo(StatusEffectData[] modifiers, int count)
	{
		if (count > 0)
		{
			var result = "Effects: ";
			for (int i = 0; i < count; ++i)
			{
				if (i < count - 1)
				{
					result += modifiers[i].type + ", ";
				}
				else
				{
					result += modifiers[i].type;
				}
			}

			//statusEffectsDisplay.text = result;
		}
		else
		{
			//statusEffectsDisplay.text = "Effects: None";
		}
	}

	void SetTargettingInfo(EnemyAttributes attributes)
	{
		var names = (string[])System.Enum.GetNames(typeof(EnemyAttributes));
		var values = (EnemyAttributes[])System.Enum.GetValues(typeof(EnemyAttributes));

		int total = 0;
		for (int i = 0; i < values.Length; ++i)
		{
			if ((attributes & values[i]) != 0)
				total += 1;
		}

		if (total == 0)
		{
			//targetDisplay.text = "Target: Default";
		}
		else
		{
			var result = "Target: ";
			int counter = 0;
			for (int i = 0; i < values.Length; ++i)
			{
				if ((attributes & values[i]) != 0)
				{
					if (counter < total - 1)
					{
						result += names[i] + ", ";
					}
					else
					{
						result += names[i];
					}
				}
			}

			//targetDisplay.text = result;
		}
	}

	public static void InitialiseAbilityIcons(TowerInfo info, GameObject hierarchy, Image[] destIcons, string[] destIconNames)
	{
		// Disable abilities hierarchy unless an icon is found and populated
		hierarchy.SetActive(false);

		for (int i = 0; i < destIcons.Length; ++i)
			destIcons[i].gameObject.SetActive(false);

		int write = 0;

		InitialiseAbilityIcons(hierarchy, destIcons, destIconNames, info.uiIcons, ref write);

		var upgrades = TowerLoader.GetPersistantUpgradeInfo(info.name);
		if (upgrades != null)
		{
			for (int i = 0; i < upgrades.Length; ++i)
				if (SaveData.IsUpgradeUnlocked(info.name, i))
					InitialiseAbilityIcons(hierarchy, destIcons, destIconNames, upgrades[i].uiIcons, ref write);
		}
	}

	static void InitialiseAbilityIcons(GameObject hierarchy, Image[] destIcons, string[] destIconNames, string[] srcIcons, ref int write)
	{
		for (int i = 0; i < srcIcons.Length; ++i)
		{
			if (TowerIconDatabase.IsValidIcon(srcIcons[i]) && write < destIcons.Length - 1)
			{
				destIcons[write].gameObject.SetActive(true);
				destIcons[write].sprite = TowerIconDatabase.GetIcon(srcIcons[i]);
                
                destIconNames[write] = srcIcons[i];

				write++;

				if (!hierarchy.activeSelf)
					hierarchy.SetActive(true);
			}
		}
	}

	void InitialiseNextAbilityDisplay(TowerInfo info0, TowerInfo info1)
	{
		//turn on the next ability display if we find something worth displaying
		HideNextAbilityDisplay();

		//this loop is figuring out what the new icon is. this assumes towers
		//only get 1 additional icon per upgrade level.
		string newEffect = "";
		for (int i = 0; i < info1.uiIcons.Length; ++i)
		{
			if (string.IsNullOrEmpty(info1.uiIcons[i]))
				continue; //skipping empty columns in the tower CSV
			
			bool found = false;
			for (int j = 0; j < info0.uiIcons.Length && !found; ++j)
				if (info0.uiIcons[j] == info1.uiIcons[i])
					found = true;

			if (!found)
				newEffect = info1.uiIcons[i];
		}

		if (!string.IsNullOrEmpty(newEffect))
		{
			nextAbilityHierarchy.SetActive(true);
			txtAbilityDescriptionNext.gameObject.SetActive(true);
			LocManager.Assign(txtAbilityDescriptionNext, TowerIconDatabase.GetDescriptionID(newEffect));

			imgAbilityIconNext.gameObject.SetActive(true);
			imgAbilityIconNext.sprite = TowerIconDatabase.GetIcon(newEffect);
		}
		else
			nextAbilityHierarchy.SetActive(false);
	}

	void HideNextAbilityDisplay()
	{
		txtAbilityDescriptionNext.gameObject.SetActive(false);
		imgAbilityIconNext.gameObject.SetActive(false);
		nextAbilityHierarchy.SetActive(false);
	}

	public static Sprite GetIcon(Tower tower, int upgradeLevel)
	{
		if (upgradeLevel < tower.towerUpgradePrefabs.Count)
		{
			var hooks = tower.towerUpgradePrefabs[upgradeLevel].GetComponent<TowerArtHooks>();
			if (hooks != null && hooks.icon != null)
				return hooks.icon;
		}

		return tower.icon;
	}

	public void OnButtonPressedUpgrade()
	{
		if (referenceTower && referenceTower.CanUpgrade())
		{
			var upgradeInfo = TowerLoader.GetTowerInfo(referenceTower.towerInfo.name, 
													   referenceTower.currentLevel + 1);
			var price = (int)(upgradeInfo.price * TowerLoader.GetTowerUpgradePriceMultiplier(upgradeInfo.name));

			if (HUD.instance.CanAfford(price))
			{
				anim.Play("Upgrade", 0, 0.0f);
				HUD.instance.MakePurchase(price);
				AudioController.Play ("UI_Purchase");

				referenceTower.Upgrade();
				ShowUpgradeInfo(referenceTower);
			}
		}
	}

	public void OnButtonPressedSell()
	{
		if(waitingForSellConfirm)
		{
			if (referenceTower)
			{
				referenceTower.Sell(sellPrice, true);
				Hide();
				AudioController.Play("UI_Sell");
				SetSellHierarchy(false);
			}
		}
		else
		{
			SetSellHierarchy(true);
		}
	}


	void SetSellHierarchy(bool _showConfirm)
	{
		waitingForSellConfirm = _showConfirm;
		sellConfirmHierarchy.SetActive(_showConfirm);
		sellNormalHierarchy.SetActive(!_showConfirm);
	}

	void ShowStarRating(int rating)
	{
		starContainer.SetActive(true);

		for (int i = 0; i < starContainer.transform.childCount; ++i)
		{
			var child = starContainer.transform.GetChild(i).GetChild(0);
			var image = child.GetComponent<Image>();

			image.enabled = i <= rating ? true :false;
		}
	}

	void Update()
	{
		if (isVisible)
		{
			elapsed += Time.deltaTime;

			if (elapsed >= timeout)
			{
				Hide();

				RangeObjectPool.Reset();
				return;
			}
			
			if (referenceTower != null)
			{
				if (referenceTower.CanUpgrade())
				{
					var info = TowerLoader.GetTowerInfo(referenceTower.towerInfo.name, referenceTower.currentLevel + 1);
					var price = (int)(info.price * TowerLoader.GetTowerUpgradePriceMultiplier(info.name));

					bool canAfford = HUD.instance.CanAfford(price);
					upgradeButton.interactable = canAfford;
					upgradeButtonBG.color = canAfford ? upgradeButtonColor : upgradeButtonColorUnaffordable;
				}

				UpdateSellPrice();

				sellButton.gameObject.SetActive(FTUE.CanSellTower());
				upgradeButton.gameObject.SetActive(FTUE.CanUpgradeTower());
			}

			UpdateKillCountEtc();
		}
	}

	void UpdateSellPrice()
	{
		sellPrice = referenceTower.CalculateTotalCost();

		//sell price changes depending on how long the tower has been in play for
		if ((Time.time - referenceTower.creationTime) > fullPriceTimeOut)
		{
			var info = TowerLoader.GetTowerInfo(referenceTower.towerInfo.name, referenceTower.currentLevel);

			sellPrice = (int)(sellPrice * sellPriceMultiplier);
			sellPrice = (int)(sellPrice * TowerLoader.GetTowerSellPriceMultiplier(info.name));
		}

		sellButtonCostText.text = sellPrice.ToString();
	}

	void UpdateKillCountEtc()
	{
		icnKillCount.SetActive(false);
		icnCoinsSpawned.SetActive(false);

		if (referenceTower != null)
		{
			if (referenceTower.weapon != null && referenceTower.weapon.showKillCount)
			{
				txtRunningTotal.text = referenceTower.weapon.totalKills.ToString();
				icnKillCount.SetActive(true);
			}
			else if (referenceTower.spawnCoins != null) 
			{
				txtRunningTotal.text = referenceTower.spawnCoins.totalCoinsCollected.ToString();
				icnCoinsSpawned.SetActive(true);
			}

			txtRunningTotal.gameObject.SetActive(true);
		}
		else
		{
			txtRunningTotal.gameObject.SetActive(false);
		}
	}

	public void ShowIconInformation(int iconIndex)
	{
		UserInterface.ShowOKDialog(LocManager.Translate("icon_name_" + abilityIconNames[iconIndex]),
								   LocManager.Translate("icon_desc_" + abilityIconNames[iconIndex]),
								   null);
        GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = false;
    }
}
