using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class TowerTurntableButton : MonoBehaviour
{
    public UIUpgrades parent { get; private set; }
    public Tower towerPrefab { get; private set; }

    public Image imgTowerIcon;
    public Animator lockAnimator;
    public GameObject lockHierarchy;
    public GameObject alertHierarchy;
    public Color lockedColor;

    Animator buttonAnimator;

    public void SetScale(float scale)
    {
        buttonAnimator.SetFloat("Scale", scale);
    }

    public void Initialise(UIUpgrades upgradesScreen, Tower prefab, bool locked, bool unlockReady, bool showAlert)
    {
        parent = upgradesScreen;
        towerPrefab = prefab;

        buttonAnimator = GetComponent<Animator>();
        imgTowerIcon.sprite = prefab.icon;

        RefreshLockedState(locked, unlockReady, showAlert);
    }

    public void OnClick()
    {
        if (parent)
        {
            parent.OnTowerSelected(this);

            AudioController.Play("UI_SelectCharUpgrade");
        }
    }

    public void RefreshLockedState(bool locked, bool unlockReady, bool showAlert)
    {
        if (locked)
        {
            lockHierarchy.SetActive(true);
            lockAnimator.Play(unlockReady ? "Idle_Ready" : "Idle", 0, 0.0f);
            imgTowerIcon.color = lockedColor;
        }
        else
        {
            lockHierarchy.SetActive(false);
            imgTowerIcon.color = Color.white;
        }

        alertHierarchy.SetActive(showAlert);
    }
}
