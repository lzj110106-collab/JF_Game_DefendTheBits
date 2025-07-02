using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerUnlock : MonoBehaviour
{
    Tower tower;

    public Animator animator;
    public Animator animatorCharacter;
    public GameObject artLocator;
    public Text requirementsText;
    public Text txtUnlockName;
    public Image requirementsIcon;
    public Transform cameraTransform;

    public GameObject shareBtn;

    public delegate void UnlockCompleteCallback();
    UnlockCompleteCallback unlockCompleteCallback;

    string trinketID;
    int trinketCount;

    List<Tower> towersToUnlock = new List<Tower>();
    int nextTowerToUnlock = 0;

    Animator currentTowerAnim;

    public void Add(Tower tower)
    {
        towersToUnlock.Add(tower);
    }

    void OnEnable()
    {
        //if (LocManager.isInChina())
        //{
        //    shareBtn.GetComponent<Image>().enabled = true;
        //    shareBtn.GetComponent<Button>().enabled = true;
        //    shareBtn.transform.GetChild(0).GetComponent<Text>().enabled = true;
        //    shareBtn.transform.GetChild(1).GetComponent<Image>().enabled = true;
        //}
        //else
        //{
            shareBtn.GetComponent<Image>().enabled = false;
            shareBtn.GetComponent<Button>().enabled = false;
            shareBtn.transform.GetChild(0).GetComponent<Text>().enabled = false;
            shareBtn.transform.GetChild(1).GetComponent<Image>().enabled = false;
        //}
    }

    public void Show(UnlockCompleteCallback cb)
    {
        unlockCompleteCallback = cb;

        nextTowerToUnlock = 1;
        ShowInternal(towersToUnlock[0]);
    }

    public void ShowInternal(Tower tower)
    {
        this.tower = tower;
        UnityUtil.DestroyAllChildren(artLocator);

        //dont bother going through the pooling system here as it
        //causes rendering issues with the layer changing code.
        var prefab = TowerLoader.GetTowerInfo(tower)[0].prefab;
        var artPrefab = prefab.towerUpgradePrefabs[0];

        var art = GameObject.Instantiate(artPrefab);
        art.gameObject.SetActive(true);
        art.transform.SetParent(artLocator.transform, false);

        currentTowerAnim = art.GetComponent<Animator>();

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

        //fix up billboarding to use the main menu scene camera
        var billboards = art.GetComponentsInChildren<CameraFacingBillboard>(true);
        for (var i = 0; i < billboards.Length; ++i)
            billboards[i].SetCameraType(CameraFacingBillboard.CameraType.Unlock_Scene);

        //make sure everything is in the correct layer for rendering
        UnityUtil.SetLayerRecursive(art, artLocator);


        //set up requirements display
        if (TowerLoader.UnlockRequirements(tower, out trinketID, out trinketCount))
        {
            requirementsText.text = SaveData.TrinketCount(trinketID).ToString() + "/" + trinketCount.ToString();
            requirementsIcon.sprite = TrinketDatabase.GetPrefab(trinketID).GetComponent<InteractReward>().trinketIcon;

            requirementsText.gameObject.SetActive(true);
            requirementsIcon.gameObject.SetActive(true);
        }
        else
        {
            requirementsText.gameObject.SetActive(false);
            requirementsIcon.gameObject.SetActive(false);

            trinketID = "";
        }

        var towerInfo = TowerLoader.GetTowerInfo(tower);
        towerInfo[0].AssignName(txtUnlockName);

        gameObject.SetActive(true);
        animator.Play("On", 0, 0.0f);
        animatorCharacter.Play("On", 0, 0.0f);
    }

    public void PlayTowerIdle()
    {
        currentTowerAnim.Play("Idle", 0, 0.0f);
    }

    public void PerformUnlock()
    {
        //make purchase if this wasnt a reward.
        //print("trinketID" + trinketID);
        //if (!string.IsNullOrEmpty(trinketID))
        //    SaveData.AddTrinket(trinketID, -trinketCount);

        //print("开始准备解锁");
        SaveData.UnlockTower(TowerLoader.GetTowerID(tower));
        //print("解锁成功");

        //print(nextTowerToUnlock + "," + towersToUnlock.Count);

        if (nextTowerToUnlock < towersToUnlock.Count)
        {
            ShowInternal(towersToUnlock[nextTowerToUnlock]);
            nextTowerToUnlock += 1;
        }
        else
        {
            gameObject.SetActive(false);
            towersToUnlock.Clear();

            if (unlockCompleteCallback != null)
            {
                unlockCompleteCallback();
                unlockCompleteCallback = null;
            }
        }
    }


}
