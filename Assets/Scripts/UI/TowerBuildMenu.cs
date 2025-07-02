using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class TowerBuildMenu : MonoBehaviour 
{
	static TowerBuildMenu instance;

	public GameObject buttonPrefab;
	public GameObject buttonContainer;
	public ScrollRect scrollRect;

	RectTransform containerTransform;

	TowerBuildMenuButton currentSelection = null;

	public void Awake() { instance = this; }
	public void OnDestroy() { instance = null; }

	public void Initialise(LevelData level, bool addLockedTowersToBuildMenu)
	{
		UnityUtil.DestroyAllChildren(buttonContainer);

		//sort now when the level is being created, as the player
		//star rating may have changed since the last time
		//they played this level.
		level.prefab.SortTowerList();

        

		foreach (var tower in level.prefab.towerListFinal)
		{
			var info = TowerLoader.GetTowerInfo(tower, 0);

           if (!addLockedTowersToBuildMenu && !SaveData.IsTowerUnlocked(info.name)&&!(info.name.Equals( SaveData.GetFreeTower().ToLower())))
				continue; //mask out locked tower



			var button = GameObject.Instantiate(buttonPrefab);
			button.transform.SetParent(buttonContainer.transform, false);
			button.name = tower.name;

			var towerButton = button.GetComponent<TowerBuildMenuButton>();
			towerButton.Initialise(this, tower);
			towerButton.scrollRect = scrollRect;
		}
			
		Canvas.ForceUpdateCanvases();

		containerTransform = buttonContainer.GetComponent<RectTransform>();
		currentSelection = null;
	}

	public void Update()
	{
		if (InputUtil.MousePressed() && !InputUtil.IsHovered(containerTransform, UserInterface.Camera2D()))
		{
			//performed a click somewhere on the screen that wasnt the scrollable area.
			OnBuildMenuItemSelected(null);
		}
	}

	public static void ClearSelection()
	{
		instance.OnBuildMenuItemSelected(null);
	}

	public void OnBuildMenuItemSelected(TowerBuildMenuButton newSelection)
	{
		if (newSelection != currentSelection)
		{
			if (currentSelection)
			{
				//for some reason Idle doesnt work. but the highlight trigger does.
//				currentSelection.animator.Play("Idle", 0, 0.0f);
				currentSelection.animator.SetTrigger("Highlighted");
				currentSelection.isSelected = false;
			}

			currentSelection = newSelection;

			if (newSelection)
			{
				newSelection.animator.SetTrigger("Pressed");
				newSelection.isSelected = true;

				TowerInfoPanel.ShowInfo(newSelection);
			}
			else
			{
				TowerInfoPanel.Hide();
			}
		}
	}
}