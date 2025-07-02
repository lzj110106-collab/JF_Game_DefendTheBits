using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

public class UIUpgrades : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeTowerInfoPanel
    {
        public Text towerName;
        public Text towerDescription;
        public Text towerPR;
        public Image towerIcon;
        public UpgradeButtonObjects[] UpgradeButtons;
        public Image[] stars;

        public GameObject towerNextHierarchy;
        public GameObject towerNextAbilityHierarchy;
        public Text txtTowerNextDescription;
        public Text txtTowerNextAbility;
        public Image icnTowerNextAbility;
        public Image icnTowerNextLevel;

        public GameObject currentAbilitiesHierarchy;
        public Image[] imgAbilityIcons;
    }

    [System.Serializable]
    public class UpgradeButtonObjects
    {
        public Button upgradeButton;
        public Animator upgradeButtonAnimator;
        public Text txtUpgradeTitle;
        public Text txtUpgradeDescription;
        public Image[] upgradeImages;
        public GameObject costContainer;
        public Button upgradeButton2;
        public GameObject cashCostContainer;
        public GameObject trinketCostContainer;
        public Text txtCashCost;
        public Text txtTrinketCost;
        public Image icnTrinket;
    }

    public Animator infoAnimator;
    public UpgradeTowerInfoPanel infoPanel;
    public GameObject towerScrollContentNode;
    public GameObject towerTurntableButtonPrefab;
    public GameObject metaUpgradeHierarchy;
    public GameObject lockedOverlayHierarchy;
    public ShrinkScrollEdgeItems shrinkScrollEdgeItems;
    public List<UpgradeInfo> CurrentUpgrades = new List<UpgradeInfo>();
    public GameObject[] towerPreviewButtons;


    public Button unlockButton;
    public Text unlockButtonText;
    public Animator unlockButtonAnim;
    public Text lockedDescriptionText;
    public Text lockedCurrency;
    public Image lockedIcon;
    public Image lockedTowerIcon;
    public Slider lockedProgressBar;

    public GameObject description;
    public GameObject progress;

    public GameObject shareBtn;

    public GameObject needTrinketObj;
    public Image needTrinketIcon;
    public Text needTrinketCount;

    // Spinning
    Animator towerArtAnimator;
    GameObject swivel;
    public float rotateSpeed = 10;
    public float rotateDecay = 0.9f;
    private Vector3 mouseLastFrame = Vector3.zero;
    int curPanel = 0;
    float xVel = 0;
    bool spinning = false;

    Tower tower;
    TowerTurntableButton currentTowerButton = null;
    bool performingUnlockSequence = false;

    string[] iconTypeNames;

    void Awake()
    {
        iconTypeNames = new string[infoPanel.imgAbilityIcons.Length];
    }

    public void VIPBackToUp()
    {
        UpdateTowerInfo(tower);
        RefreshLockedStatus();
        if (description.GetComponent<VerticalLayoutGroup>().childAlignment == TextAnchor.UpperCenter)
        {
            description.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
        }

        if (description.GetComponent<VerticalLayoutGroup>().childAlignment == TextAnchor.UpperLeft)
        {
            description.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        }
    }

    void OnEnable()
    {
        needTrinketObj.SetActive(false);

        lastIndex = -1;

        //if (LocManager.isInChina())
        //{
        //    shareBtn.SetActive(true);
        //}
        //else
        //{
            shareBtn.SetActive(false);
        //}


        if (performingUnlockSequence)
            return;

        //TODO: should probs only do this once at launch. must not get
        //out of sync with server load of the unlocks file though
        UnityUtil.DestroyAllChildren(towerScrollContentNode);

        //bool invokeFirstButton = true;

        foreach (var kv in TowerLoader.instance.towerInfo)
        {
            //if (TowerLoader.GetTowerInfo(kv.Key, 0).vip.isVIP)
            //    continue;
            //only display towers that have upgrade info. ignore the rest
            var upgradeInfo = TowerLoader.GetPersistantUpgradeInfo(kv.Key);
            if (upgradeInfo != null && upgradeInfo.Length > 0)
            {
                var button = GameObject.Instantiate(towerTurntableButtonPrefab);
                button.transform.SetParent(towerScrollContentNode.transform, false);
                button.name = kv.Key;

                Canvas.ForceUpdateCanvases();
                var towerButton = button.GetComponent<TowerTurntableButton>();
                if (towerButton != null)
                    towerButton.Initialise(this, TowerLoader.GetTowerPrefab(kv.Key), false, false, false);

                //invoke the first button so that the turntable art and the
                //upgrade panel can both be set up with the details of
                //the first upgradable tower.
                //if (invokeFirstButton)
                //{
                //    button.GetComponent<Button>().onClick.Invoke();
                //    invokeFirstButton = false;
                //}
            }
        }

        //this use to occur during construction of the list, but its needed
        //when then UI is brought back after the unlock sequence. previously
        //this would just refresh the newly unlocked tower, but animating
        //the upgrades screen back on resets all of the button animations
        RefreshLockedStatus();

        shrinkScrollEdgeItems.Initialize();

        towerScrollContentNode.transform.GetChild(0).GetComponent<TowerTurntableButton>().OnClick();
    }

    void RefreshLockedStatus()
    {
        if (description.GetComponent<VerticalLayoutGroup>().childAlignment == TextAnchor.UpperCenter)
        {
            description.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
        }

        if (description.GetComponent<VerticalLayoutGroup>().childAlignment == TextAnchor.UpperLeft)
        {
            description.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        }

        foreach (Transform child in towerScrollContentNode.transform)
        {
            var towerButton = child.GetComponent<TowerTurntableButton>();
            if (towerButton == null || towerButton.towerPrefab == null)
                continue;

            var towerPrefab = towerButton.towerPrefab;
            var towerIdentifier = TowerLoader.GetTowerID(towerPrefab);

            if (SaveData.IsTowerUnlocked(towerIdentifier))      //已经解锁了
            {
                if (TowerLoader.GetTowerInfo(child.name, 0).vip.isVIP)       //如果是需要VIP解锁的
                {
                    if (SaveData.GetPlayerVIP())        //判断是否已经开通的VIP
                    {
                        towerButton.RefreshLockedState(false, true, false);
                    }
                    else
                    {
                        towerButton.RefreshLockedState(true, false, false);
                        SaveData.LockTower(towerIdentifier);
                    }
                }
                else
                {
                    towerButton.RefreshLockedState(false, false, false);
                }
            }
            else
            {
                string trinketID = "";
                int trinketCount = 0;

                if (TowerLoader.GetTowerInfo(child.name, 0).vip.isVIP)       //如果是需要VIP解锁的
                {
                    if (SaveData.GetPlayerVIP())        //判断是否已经开通的VIP
                    {
                        towerButton.RefreshLockedState(false, true, true);
                    }
                    else
                    {
                        towerButton.RefreshLockedState(true, false, false);
                    }
                }
                else if (TowerLoader.GetTowerInfo(child.name, 0).cash.isCash)      //如果是需要购买解锁的，
                {
                    //判断是否已经购买
                    if (SaveData.GetTowerBuy(child.name.ToLower()))
                    {
                        towerButton.RefreshLockedState(false, true, true);
                    }
                    else
                    {
                        towerButton.RefreshLockedState(true, false, false);
                    }
                }
                else if (TowerLoader.UnlockRequirements(towerPrefab, out trinketID, out trinketCount))
                {
                    bool showAlert = false; //TODO
                    bool unlockReady = SaveData.TrinketCount(trinketID) >= trinketCount;

                    towerButton.RefreshLockedState(true, unlockReady, showAlert);
                }
                else
                {
                    //locked via achievements or level completion
                    string achievementIdentifier = "";
                    if (AchievementDatabase.unlockableTowers.TryGetValue(towerIdentifier, out achievementIdentifier))
                    {
                        var achievementData = AchievementDatabase.achievements[achievementIdentifier];
                        bool unlockReady = (AchievementDatabase.AchievementProgress(achievementData) >= 1.0f);

                        towerButton.RefreshLockedState(true, unlockReady, false);
                    }
                    else
                    {
                        //locked by FTUE. this only happens when messing with save data.
                        towerButton.RefreshLockedState(true, false, false);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (InputUtil.MousePressed() && InputUtil.IsWorldHovered() && !spinning)
        {
            spinning = true;
            mouseLastFrame = InputUtil.MousePosition();
        }
        else if (spinning)
        {
            if (InputUtil.MouseReleased())
            {
                spinning = false;
            }
            else if (InputUtil.MouseDrag())
            {
                float diff = InputUtil.MousePosition().x - mouseLastFrame.x;
                xVel = -(diff / (float)Screen.width) * rotateSpeed;

                mouseLastFrame = InputUtil.MousePosition();
            }
        }

        if (swivel != null)
            swivel.transform.Rotate(new Vector3(0, xVel, 0), Space.Self);

        xVel *= rotateDecay;



#if PXY_YUNCE
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("测试退出按钮");
            //Application.Quit();
            PXY_AndroidBuy.Instance.ExitGame();
            //ExitGame.Instance.SetSelfTrue();
        }
#endif
    }


    TowerInfo info;

    public void UpdateTowerInfo(Tower _tower)
    {
        if (description.GetComponent<VerticalLayoutGroup>().childAlignment == TextAnchor.UpperCenter)
        {
            description.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
        }

        if (description.GetComponent<VerticalLayoutGroup>().childAlignment == TextAnchor.UpperLeft)
        {
            description.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        }

        //print("更新防御塔信息");
        this.tower = _tower;

        //var info = TowerLoader.GetTowerInfo(_tower.towerName, 0);
        info = TowerLoader.GetTowerInfo(_tower.towerName, 0);
        currentTowerName = _tower.towerName;
        //SaveData.UnlockTower(info.name);
        //已经解锁
        if (SaveData.IsTowerUnlocked(info.name))
        {
            lockedOverlayHierarchy.SetActive(false);
            metaUpgradeHierarchy.SetActive(true);
        }
        else
        {
            bool isTowerReadyToUnlock = false;          //是否准备好了解锁

            lockedOverlayHierarchy.SetActive(true);
            metaUpgradeHierarchy.SetActive(false);

            if (info.cash.isCash)
            {
                if (SaveData.GetTowerBuy(info.name.ToLower()))
                {
                    print("已经用现金购买");
                    //unlockButtonText.text = "解锁";
                    LocManager.Assign(unlockButtonText, "ui_unlock");
                    //lockedDescriptionText.text = "";
                    progress.SetActive(false);
                    LocManager.Assign(lockedDescriptionText, "ui_cashtowerunlocked");
                    isTowerReadyToUnlock = true;
                }
                else
                {
                    print("我需要用现金购买");
                    //如果该防御塔是直接现金解锁
                    isTowerReadyToUnlock = true;
                    //description.transform.GetChild(0).GetComponent<Text>().text = info.cash.cashDescription;
                    //LocManager.Assign(lockedDescriptionText, "ui_spendmoneytounlock",LocManager.GetTargetPrice( (int)info.cash.cashPrice));
                    //print(((int)info.cash.cashPrice).ToString()+"---"+ PurchaseManager.priceDir[((int)info.cash.cashPrice).ToString()]);
                    //if (PurchaseManager.priceDir.ContainsKey(((int)info.cash.cashPrice).ToString()))
                    //    LocManager.Assign(lockedDescriptionText, "ui_spendmoneytounlock", PurchaseManager.priceDir[((int)info.cash.cashPrice).ToString()]);
                    //else
                    //    LocManager.Assign(lockedDescriptionText, "ui_connect_wifi_body");
                    //unlockButtonText.text = "购买";
                    LocManager.Assign(unlockButtonText, "ui_buy");
                    description.transform.localPosition = new Vector3(description.transform.localPosition.x, 0, description.transform.localPosition.z);
                    progress.SetActive(false);
                }
            }
            else if (info.vip.isVIP)
            {
                if (SaveData.GetPlayerVIP())
                {
                    //unlockButtonText.text = "解锁";
                    LocManager.Assign(unlockButtonText, "ui_unlock");
                    progress.SetActive(false);
                    LocManager.Assign(lockedDescriptionText, "ui_viptowerunlocked");
                    isTowerReadyToUnlock = true;
                }
                else
                {
                    print("我需要用VIP解锁");
                    //如果该防御塔需要开通vip来解锁
                    isTowerReadyToUnlock = true;
                    //description.transform.GetChild(0).GetComponent<Text>().text = info.vip.vipDescription;
                    //if (PurchaseManager.priceDir.ContainsKey("48"))
                    //    LocManager.Assign(lockedDescriptionText, "ui_open7towers", PurchaseManager.priceDir["48"]);
                    //else
                    //    LocManager.Assign(lockedDescriptionText, "ui_connect_wifi_body");

                    //unlockButtonText.text = "开通VIP";
                    LocManager.Assign(unlockButtonText, "ui_dredgevip");
                    description.transform.localPosition = new Vector3(description.transform.localPosition.x, 0, description.transform.localPosition.z);
                    progress.SetActive(false);
                }
            }
            else
            {
                print("我正常解锁就好了");
                //print(AchievementDatabase.unlockableTowers.ContainsKey(info.name));
                description.transform.localPosition = new Vector3(description.transform.localPosition.x, 85, description.transform.localPosition.z);
                progress.SetActive(true);

                //如果塔还不能解锁
                if (AchievementDatabase.unlockableTowers.ContainsKey(info.name))
                {
                    var achievementIdentifier = AchievementDatabase.unlockableTowers[info.name];
                    var achievementData = AchievementDatabase.achievements[achievementIdentifier];

                    var iconName = achievementData.type == Achievement.Type.Collect ? "quest_collect" :
                        achievementData.type == Achievement.Type.Defeat ? "quest_defeat" :
                        achievementData.type == Achievement.Type.Upgrade ? "quest_upgrade" :
                        "quest_complete";

                    //print(achievementData.Description());
                    //achievementData.AssignDescription(lockedDescriptionText);
                    //string tem = lockedDescriptionText.text;
                    lockedDescriptionText.text = achievementData.Description();
                    //print(lockedDescriptionText.text);
                    //LocManager.Assign(unlockButtonText, "ui_lock");
                    unlockButtonText.text = "未解锁";
                    lockedDescriptionText.text = "点击解锁按钮进行解锁";
                    lockedIcon.sprite = TowerIconDatabase.GetIcon(iconName);
                    lockedTowerIcon.sprite = TowerInfoPanel.GetIcon(_tower, 0);
                    lockedCurrency.text = AchievementDatabase.AchievementProgressText(achievementData);
                    lockedProgressBar.value = AchievementDatabase.AchievementProgress(achievementData);

                    if (achievementData.isCompleted && !achievementData.isCollected)
                    {
                        //print("已经达成解锁条件。");
                        //LocManager.Assign(unlockButtonText, "ui_unlock");
                        unlockButtonText.text = "解锁";
                        isTowerReadyToUnlock = true;
                    }
                }
                else
                {
                    string trinketID;
                    int trinketsRequired;
                    TowerLoader.UnlockRequirements(_tower, out trinketID, out trinketsRequired);

                    int trinketCount = SaveData.TrinketCount(trinketID);
                    trinketCount = Mathf.Min(trinketCount, trinketsRequired); //cap the display

                    LocManager.Assign(lockedDescriptionText, "ui_collect_trinkets", trinketID);
                    print(trinketID);
                    lockedIcon.sprite = TrinketDatabase.GetIcon(trinketID);
                    lockedTowerIcon.sprite = TowerInfoPanel.GetIcon(_tower, 0);
                    lockedCurrency.text = trinketCount.ToString() + "/" + trinketsRequired.ToString();
                    lockedProgressBar.value = (float)trinketCount / (float)trinketsRequired;

                    if (trinketCount >= trinketsRequired)
                    {
                        print("已经达成解锁条件。");
                        LocManager.Assign(unlockButtonText, "ui_unlock");
                        //unlockButtonText.text = "解锁";
                        isTowerReadyToUnlock = true;
                    }
                }
            }

            if (isTowerReadyToUnlock)
            {
                unlockButton.interactable = true;
                unlockButtonAnim.Play("Idle", 0, 0.0f);
            }
            else
            {
                unlockButton.interactable = false;
                unlockButtonAnim.Play("Disabled", 0, 0.0f);
            }
        }

        //upgrades panel
        var upgradeInfoForTower = TowerLoader.GetPersistantUpgradeInfo(_tower.towerName);
        if (upgradeInfoForTower != null)
        {
            CurrentUpgrades.Clear();
            CurrentUpgrades = new List<UpgradeInfo>(upgradeInfoForTower);

            for (int i = 0; i < infoPanel.UpgradeButtons.Length; i++)
            {
                LocManager.Assign(infoPanel.UpgradeButtons[i].txtUpgradeTitle, CurrentUpgrades[i].title);
                LocManager.Assign(infoPanel.UpgradeButtons[i].txtUpgradeDescription, CurrentUpgrades[i].description);

                for (int m = 0; m < infoPanel.UpgradeButtons[i].upgradeImages.Length; m++)
                    infoPanel.UpgradeButtons[i].upgradeImages[m].sprite = TowerIconDatabase.GetIcon(CurrentUpgrades[i].uiIcons[0]); ;

                // Cash cost
                infoPanel.UpgradeButtons[i].cashCostContainer.SetActive(CurrentUpgrades[i].cash > 0);
                infoPanel.UpgradeButtons[i].txtCashCost.text = CurrentUpgrades[i].cash.ToString();

                // Trinket cost
                var trinketPrefab = TrinketDatabase.GetPrefab(CurrentUpgrades[i].trinketID);
                if (trinketPrefab != null)
                {
                    infoPanel.UpgradeButtons[i].trinketCostContainer.SetActive(CurrentUpgrades[i].trinketCount > 0);
                    infoPanel.UpgradeButtons[i].txtTrinketCost.text = CurrentUpgrades[i].trinketCount.ToString();
                    infoPanel.UpgradeButtons[i].icnTrinket.sprite = trinketPrefab.GetComponent<InteractReward>().trinketIcon;
                    needTrinketCount.text = SaveData.TrinketCount(CurrentUpgrades[i].trinketID).ToString();
                    needTrinketIcon.sprite = trinketPrefab.GetComponent<InteractReward>().trinketIcon;
                    needTrinketObj.SetActive(true);
                }
                else
                {
                    needTrinketObj.SetActive(false);
                    infoPanel.UpgradeButtons[i].trinketCostContainer.SetActive(false);
                }

                bool playFull = SaveData.IsUpgradeUnlocked(CurrentUpgrades[i].towerName, i);
                if (infoPanel.UpgradeButtons[i].upgradeButtonAnimator && infoPanel.UpgradeButtons[i].upgradeButtonAnimator.gameObject.activeSelf)
                    infoPanel.UpgradeButtons[i].upgradeButtonAnimator.Play(playFull ? "NowUpgraded" : "UnavailableIdle", 0, 0.0f);

                bool currentUpgradeLevel = SaveData.GetUpgradeLevel(CurrentUpgrades[i].towerName) == i;
                if (currentUpgradeLevel && infoPanel.UpgradeButtons[i].upgradeButtonAnimator.gameObject.activeSelf && infoPanel.UpgradeButtons[i].upgradeButtonAnimator)
                {
                    infoPanel.UpgradeButtons[i].upgradeButtonAnimator.Play("NowAvailable", 0, 0.0f);
                }

                // Enable/Disable upgrade button so only one shows
                infoPanel.UpgradeButtons[i].upgradeButton.interactable = (currentUpgradeLevel && !playFull && SaveData.IsTowerUnlocked(info.name));
            }
        }
        else
        {
            needTrinketObj.SetActive(false);
        }

        //refresh the turntable display
        SetTowerDisplayLevel(0);

        infoAnimator.Play("SwitchInfo", 0, 0.0f);
    }

    public void SetTowerDisplayLevel(int displayLevel)
    {

        var info = TowerLoader.GetTowerInfo(tower.towerName, displayLevel);

        UnityUtil.DestroyAllChildren(UISceneManager.instance.upgradeCharacterLocator.gameObject);
        xVel = 0.0f; //stop existing rotation

        var art = GameObject.Instantiate(tower.towerUpgradePrefabs[displayLevel], UISceneManager.instance.upgradeCharacterLocator.transform, false);
        swivel = UnityUtil.FindChild(art.transform, "Swivel"); //transform to rotate with user input
        UnityUtil.SetLayerRecursive(art, UISceneManager.instance.upgradeCharacterLocator.gameObject); //fix rendering

        towerArtAnimator = art.GetComponentInChildren<Animator>(true);

        if (SaveData.IsTowerUnlocked(info.name))
        {

            var artHooks = art.GetComponent<TowerArtHooks>();
            artHooks.PFX.Initialise(art.transform);

            //spawn passive PFX on the tower. dont use the pooling system here, no need.
            for (int i = (int)PFX.Tower_Passive; i <= (int)PFX.Tower_Passive_3; ++i)
            {
                var data = artHooks.PFX.entriesOrdered[i];
                if (data != null && data.prefab != null && data.locator != null)
                {
                    var pfxInstance = GameObject.Instantiate(data.prefab);
                    pfxInstance.transform.SetParent(data.locator, false);

                    var ps = pfxInstance.GetComponent<ParticleSystem>();
                    PFXWrapper.ApplyPFXOverrides(ps, data);
                }
            }
        }
        else
        {

            // Apply disabled material if locked. 
            {
                //need to assign the entire materials array at once, so just generate it up front.
                //creating an array of length 2 to cover the ice tower which has multiple materials
                var newMaterials = new Material[2] {
                    new Material(MaterialCache.instance.disabledMeshMaterial),
                    new Material(MaterialCache.instance.disabledMeshMaterial)
                };

                foreach (var renderer in art.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (renderer.name.Contains("Glow_"))
                        renderer.gameObject.SetActive(false); //hide glow billboards
                    else
                        renderer.materials = newMaterials;
                }

                foreach (var renderer in art.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    renderer.materials = newMaterials;
            }

            //remove PFX and trails
            {
                var ps = art.GetComponentsInChildren<ParticleSystem>(true);
                for (int i = 0; i < ps.Length; ++i)
                    Destroy(ps[i]);

                var trails = art.GetComponentsInChildren<TrailRenderer>(true);
                for (int i = 0; i < trails.Length; ++i)
                    Destroy(trails[i]);

                var trails2 = art.GetComponentsInChildren<PigeonCoopToolkit.Effects.Trails.TrailRenderer_Base>();
                for (int i = 0; i < trails2.Length; ++i)
                    Destroy(trails2[i]);
            }
        }

        //fix up billboarding to use the main menu scene camera
        var billboards = art.GetComponentsInChildren<CameraFacingBillboard>(true);
        for (var i = 0; i < billboards.Length; ++i)
            billboards[i].SetCameraType(CameraFacingBillboard.CameraType.UI_Scene);

        //fix up UI to reflect the displayed art level
        {
            for (int i = 0; i < infoPanel.stars.Length; ++i)
            {
                infoPanel.stars[i].gameObject.SetActive(i <= displayLevel);
                towerPreviewButtons[i].SetActive(i == displayLevel);
            }

            info.AssignName(infoPanel.towerName);
            info.AssignDescription(infoPanel.towerDescription, 0); //show the base description here

            infoPanel.towerIcon.sprite = tower.icon;
            infoPanel.towerPR.text = info.rating.ToString();

            TowerInfoPanel.InitialiseAbilityIcons(info, infoPanel.currentAbilitiesHierarchy, infoPanel.imgAbilityIcons, iconTypeNames);

            var nextInfo = TowerLoader.GetTowerInfo(tower.towerName, displayLevel + 1);
            if (nextInfo == null)
            {
                infoPanel.towerNextHierarchy.SetActive(false);
            }
            else
            {
                infoPanel.towerNextHierarchy.SetActive(true);
                infoPanel.icnTowerNextLevel.sprite = TowerInfoPanel.GetIcon(tower, displayLevel + 1);
                nextInfo.AssignDescription(infoPanel.txtTowerNextDescription, displayLevel + 1);

                InitialiseNextAbilityDisplay(info, nextInfo);
            }

        }
    }

    //literally a copy/paste of TowerInfoPanel with variable names changed.
    //would be nice to unify all this stuff into one class, but running out of time.....
    void InitialiseNextAbilityDisplay(TowerInfo info0, TowerInfo info1)
    {
        //turn on the next ability display if we find something worth displaying
        infoPanel.towerNextAbilityHierarchy.SetActive(false);

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
            infoPanel.towerNextAbilityHierarchy.SetActive(true);
            infoPanel.icnTowerNextAbility.sprite = TowerIconDatabase.GetIcon(newEffect);
            LocManager.Assign(infoPanel.txtTowerNextAbility, TowerIconDatabase.GetDescriptionID(newEffect));
        }
    }

    string currentTowerName;

    public void BuySucceedCallBack()
    {
        shader.SetActive(false);
        SaveData.SetTowerBuy(currentTowerName.ToLower(), true);
        UpdateTowerInfo(tower);
        RefreshLockedStatus();
    }

    public void BuyFaileCallBack()
    {
        shader.SetActive(false);
    }

    //public PurchaseManager purchaseManager;

    public GameObject shader;

    public void UnlockTowerButtonPressed()
    {
        if (description.GetComponent<VerticalLayoutGroup>().childAlignment == TextAnchor.UpperCenter)
        {
            description.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
        }

        if (description.GetComponent<VerticalLayoutGroup>().childAlignment == TextAnchor.UpperLeft)
        {
            description.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        }

        if (info.cash.isCash)
        {
            if (SaveData.GetTowerBuy(info.name.ToLower()))
            {
                var unlockScreen = UserInterface.GetTowerUnlockScreen();
                unlockScreen.Add(tower);
                unlockScreen.Show(OnUnlockSequenceComplete);

                performingUnlockSequence = true;
                gameObject.SetActive(false);

                GameObject.Find("Menu_Currency").SetActive(false);
            }
            else
            {
                print("前往购买");
                if (Application.internetReachability != NetworkReachability.NotReachable)
                    shader.SetActive(true);
                switch (currentTowerName)
                {
                    case "Ogre":
                        //purchaseManager.OnPurchaseClicked(1);
                        break;
                    case "Wayno":
                        //purchaseManager.OnPurchaseClicked(2);
                        break;
                    case "Penelope":
                        //purchaseManager.OnPurchaseClicked(3);
                        break;
                }
            }
        }
        else if (info.vip.isVIP)
        {
            if (SaveData.GetPlayerVIP())
            {
                var unlockScreen = UserInterface.GetTowerUnlockScreen();
                unlockScreen.Add(tower);
                unlockScreen.Show(OnUnlockSequenceComplete);

                performingUnlockSequence = true;
                gameObject.SetActive(false);

                GameObject.Find("Menu_Currency").SetActive(false);
            }
            else
            {
                GetComponent<Animator>().SetBool("Share", true);
                GameObject.Find("Menu_Currency").GetComponent<Animator>().SetBool("Share", true);
                GetComponent<VIPButton>().VIPButtonPressed();
            }
        }
        else
        {
            var unlockScreen = UserInterface.GetTowerUnlockScreen();
            unlockScreen.Add(tower);
            unlockScreen.Show(OnUnlockSequenceComplete);

            performingUnlockSequence = true;
            gameObject.SetActive(false);

            GameObject.Find("Menu_Currency").SetActive(false);
       
        }
    }

    [DllImport("__Internal")]
    private static extern int Purchase(float price, string purchaseID, string purchaseItem, string purchaseDiscribe, string userID);

    void OnUnlockSequenceComplete()
    {
        //bring back the foreground
        gameObject.SetActive(true);
        GetComponent<Panel>().PerformDefaultOnTransition();
        //restoring the background
        UISceneManager.instance.SetScene(UISceneIDs.MainMenu);
        UISceneManager.instance.uiSceneCamera.SetCameraAnimation(CameraStates.Upgrades);
        //respawn the tower so that it is no longer greyed out
        UpdateTowerInfo(tower);
        //if this was an achievement unlock, then mark that achievement as collected
        AchievementDatabase.OnTowerUnlocked(tower);
        //done
        RefreshLockedStatus();
        performingUnlockSequence = false;
    }

    // Make purchase of upgrade
    public void BuyUpgrade()
    {
        int currentLevel = SaveData.GetUpgradeLevel(CurrentUpgrades[0].towerName);
        if (currentLevel > 2)
        {
            Debug.Log("You are already fully upgraded");
            return;
        }

        var upgrade = CurrentUpgrades[currentLevel];
        if (SaveData.GetCash() >= upgrade.cash)
        {
            //some upgrades require trinkets. check for that as well
            if (upgrade.trinketID != "")
            {
                if (upgrade.trinketCount > SaveData.TrinketCount(upgrade.trinketID))
                {
                    Debug.Log("Insufficient Trinkets: " + upgrade.trinketID);

                    UserInterface.ShowOKDialog(LocManager.Translate("ui_badhead"),
                                 LocManager.Translate("ui_trinketnotenough"),
                                 NotEnoughTrinket);
                    return;
                }

                SaveData.AddTrinket(upgrade.trinketID, -upgrade.trinketCount);
                needTrinketCount.text = SaveData.TrinketCount(upgrade.trinketID).ToString();
            }

            SaveData.SetUpgradeLevel(upgrade.towerName, currentLevel + 1);
            SaveData.AddCash(-upgrade.cash);
            AudioController.Play("UI_Purchase");

            for (int i = 0; i < infoPanel.UpgradeButtons.Length; i++)
            {
                if (i <= currentLevel)
                {
                    infoPanel.UpgradeButtons[currentLevel].upgradeButtonAnimator.Play("NowUpgraded", 0, 0.0f);
                    infoPanel.UpgradeButtons[currentLevel].upgradeButton.interactable = false;
                    if (currentLevel + 1 < infoPanel.UpgradeButtons.Length)
                    {
                        infoPanel.UpgradeButtons[currentLevel + 1].upgradeButtonAnimator.Play("NowAvailable", 0, 0.0f);
                        infoPanel.UpgradeButtons[currentLevel + 1].upgradeButton.interactable = true;
                    }
                }
                else
                {
                    infoPanel.UpgradeButtons[i].costContainer.SetActive(false);
                    infoPanel.UpgradeButtons[i].upgradeButton2.gameObject.SetActive(false);
                }
            }

            //			Debug.Log ("Purchase Successful");

            AchievementDatabase.UpgradeTower(upgrade.towerName, currentLevel + 1);

            //this will play the victory animation once only.
            towerArtAnimator.Play("Victory", 0, 0.0f);
            towerArtAnimator.SetTrigger("Upgrades");
        }
        else
        {
            //Debug.Log("Insufficient Funds");
            UserInterface.ShowOKDialog(LocManager.Translate("ui_badhead"),
                                     LocManager.Translate("ui_cashnotenough"),
                                     NotEnoughCash);
        }
    }

    void NotEnoughCash()
    {
        CurrencyDisplay.instance.OnShopButtonPressed(1);

    }

    void NotEnoughTrinket()
    {
        CurrencyDisplay.instance.OnShopButtonPressed(3);
    }

    public void OnTowerSelected(TowerTurntableButton source)
    {
        currentTowerButton = source;
        UpdateTowerInfo(source.towerPrefab);
    }

    #region         升级界面技能图标含义
    public Animator[] anis;
    private int lastIndex;

    public void ShowIconInformation(int iconIndex)
    {
        //UserInterface.ShowOKDialog(LocManager.Translate("icon_name_" + iconTypeNames[iconIndex]),
        //                           LocManager.Translate("icon_desc_" + iconTypeNames[iconIndex]),
        //                           null);
        //GameObject.Find("Menu_YesNoDialog").GetComponent<DialogYesNo>().isEnteredShop = false;

        anis[iconIndex].transform.GetChild(0).GetChild(1).GetComponent<Text>().text = LocManager.Translate("icon_name_" + iconTypeNames[iconIndex]);
        anis[iconIndex].transform.GetChild(0).GetChild(2).GetComponent<Text>().text = LocManager.Translate("icon_desc_" + iconTypeNames[iconIndex]);


        if (lastIndex == -1)
        {
            anis[iconIndex].SetTrigger("show");
            lastIndex = iconIndex;
        }
        else
        {
            anis[lastIndex].SetTrigger("hide");
            anis[iconIndex].SetTrigger("show");
            lastIndex = iconIndex;
        }

        StartCoroutine(YieldToHide());
    }

    IEnumerator YieldToHide()
    {
        yield return new WaitForSeconds(3.0f);
        if (lastIndex == -1)
        {
            for (int i = 0; i < anis.Length; i++)
            {
                anis[i].SetTrigger("hide");
            }
        }
        else
        {
            anis[lastIndex].SetTrigger("hide");
            lastIndex = -1;
        }
    }
    #endregion
}
