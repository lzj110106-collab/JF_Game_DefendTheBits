using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TowerBuildMenuButton : MonoBehaviour
{
	TowerBuildMenu buildMenu;

	[HideInInspector] public GameObject towerPrefab;
	[HideInInspector] public ScrollRect scrollRect;				//Assigned on instantiation

	public float infoPopupDelay;
	public Image imgTowerIcon;
	public Image imgTowerIconTint;
	public Image imgTowerIconFaded;

	public Text txtPrice;
	public Image imgPrice;
	public int price;

	public Color colourAvailable;
	public Color colourDisabled;

	public GameObject alertHierarchy;

	Tower towerToPlace;
	LandscapeTile currentTile;

	TowerInfo towerVars;
	List<TowerInfo> towerInfo;

	Camera cameraGame;
	[HideInInspector] public RectTransform rectTransform;
	[HideInInspector] public Animator animator;


	int placementTileX;
	int placementTileY;

	bool meetsLevelRequirements = true;
	bool canAfford = false;

	[HideInInspector] public bool isSelected = false;
	[HideInInspector] public bool isPlacingTower = false;
	bool mouseDown = false;
	bool mouseHasLeftDeadZone = false;
	Vector2 mouseStartPosition;

	float deadZoneBase = 10.0f; //this is based on iphone7 res 750x1334
	float deadZone;

	public void Initialise(TowerBuildMenu parent, Tower prefab)
	{
		deadZone = deadZoneBase * (Screen.width / 1334.0f);

		buildMenu = parent;
		towerInfo = TowerLoader.GetTowerInfo(prefab);

		rectTransform = GetComponent<RectTransform>();
		animator = GetComponent<Animator>();

		cameraGame = Camera.main;
		scrollRect = GetComponentInParent<ScrollRect>();

		//TODO: sort this out
		SetTowerPrefab(prefab.gameObject);

		meetsLevelRequirements = SaveData.IsTowerUnlocked(towerInfo[0].name);
	}

	public void SetTowerPrefab(GameObject prefab)
	{
		towerPrefab = prefab;

		var towerComponent = prefab.GetComponent<Tower>();
		towerVars = TowerLoader.GetTowerInfo (towerComponent.towerName, 0);
		price = (int)(towerVars.price * TowerLoader.GetTowerUpgradePriceMultiplier(towerVars.name));

		txtPrice.text = price.ToString();
		imgTowerIconFaded.sprite = imgTowerIconTint.sprite = imgTowerIcon.sprite = prefab.GetComponent<Tower>().icon;
        
	}

	public void ToggleAlert(bool _show)
	{
		alertHierarchy.SetActive(_show);
	}

	public void LateUpdate()//性能
	{
		//updating the icon and text to match affordability states
		if (towerVars != null)
		{
			canAfford = HUD.instance.CanAfford(price);
			canAfford &= FTUE.AllowPlacement(towerVars.name);

			if (FTUE.AllowPlacement(towerVars.name))
				animator.Play( (/*meetsLevelRequirements && */canAfford) ? "Idle" : "Idle_Disabled", 0, 0.0f);
			else
				animator.Play("Idle_Faded", 0, 0.0f);

			// TODO: make alerts show when a tower is newly awarded, or when placing in FTUE
			alertHierarchy.SetActive(FTUE.ShowAlertHierarchy(towerVars.name));
        }
		else
		{
			if (towerPrefab != null)
			{
                var towerComponent = towerPrefab.GetComponent<Tower>();
				if (towerComponent != null)
				{
                    Debug.Log("[TowerBuildMenuButton] null towerInfo for " + towerComponent.name);
					return;
				}
			}
			
			Debug.Log("[TowerBuildMenuButton] null towerPrefab or null towerInfo");
			return;
		}
		
		var isHovered = InputUtil.IsHovered(rectTransform, UserInterface.Camera2D());
		var mousePosition = InputUtil.MousePosition();

		if (InputUtil.MousePressed() && isHovered)    //鼠标开始点击，获取鼠标位置
		{
            if (isSelected)
				StartPlacement();

			mouseDown = true;
			mouseHasLeftDeadZone = false;
			mouseStartPosition = InputUtil.MousePosition();

			//started interacting with the build menu. clear the current
			//towers highlights and info panel.
			if (TowerInfoPanel.IsShowingUpgrade())
			{
                RangeObjectPool.Reset();
				TowerInfoPanel.Hide();
			}
		}
		else if (mouseDown)
		{
            if (InputUtil.MouseReleased())    //鼠标释放
			{
                if (isPlacingTower)
				{
                    //clear the current selection
                    buildMenu.OnBuildMenuItemSelected(null);

					FinalisePlacement();
					isPlacingTower = false;
					towerToPlace = null;
				}
				else if (isHovered && !mouseHasLeftDeadZone)
				{
                    //if we get here, it was a single tap. attempt to switch focus points
                    buildMenu.OnBuildMenuItemSelected(this);
				}

				mouseDown = false;
				scrollRect.horizontal = true;
			}
			else if (InputUtil.MouseDrag())
			{
                //check for mouse movement out of the deadzone, for single click operations
                if (Mathf.Abs(mousePosition.x - mouseStartPosition.x) >= deadZone)
					mouseHasLeftDeadZone = true;

                //check for mouse movement out of the build menu and into the world
                //检查鼠标是否从构建菜单移动到世界中
                if (!InputUtil.IsHovered(scrollRect.content, UserInterface.Camera2D()) && !isSelected)
				{
                    if (!isSelected)
					{
                        isSelected = true;
						buildMenu.OnBuildMenuItemSelected(this);

						StartPlacement();
					}
						
					mouseHasLeftDeadZone = true;
				}

				//update tower placement
				if (isPlacingTower)
				{
                    if (EventSystem.current != null && (EventSystem.current.IsPointerOverGameObject() || 
														EventSystem.current.IsPointerOverGameObject(0)))
					{
                        //do nothing. currently moving over the user interface
                        InvalidatePlacement();
					}
					else
					{
                        //only update the placement if a pick was successful, otherwise leave it in place.
                        //this prevents the tower disappearing as the input position moves over
                        //the dirt walls of tiles.

                        //there was some code here to place the tower above the users finger, but it felt weird.
                        var ray = cameraGame.ScreenPointToRay(InputUtil.MousePosition());

						int tileX = 0, tileY = 0;
						if (Landscape.instance.PickTile(ray, ref tileX, ref tileY))
							UpdatePlacement(tileX, tileY);
					}
				}
			}
		}
	}

	void StartPlacement()
	{
		//prevent attempting a placement with towers that the player cant afford
		if (/*meetsLevelRequirements && */canAfford)
		{
			scrollRect.horizontal = false;

			towerToPlace = TowerPool.Get(towerPrefab).GetComponent<Tower>();
			towerToPlace.Initialise(towerPrefab, towerInfo);
			towerToPlace.StartPlacement();
			InvalidatePlacement();
			isPlacingTower = true;

			//tower info panel overlaps a significant chunk of the screen, so
			//hide it to allow the player to place towers in locations it
			//covers
			TowerInfoPanel.Hide();
		}
	}

	void UpdatePlacement(int tileX, int tileY)
	{
		//dont update the position if there is already a building here. 
		//this is to prevent some z-fighting issues. 
		if (Landscape.instance.HasFlag(tileX, tileY, TileFlag.BuildingExists_RuntimeAssigned))
			return;

		if (placementTileX != tileX || placementTileY != tileY)
		{
			//remove the 'will place tower' flag from the previous location so that
			//we arent left with a hole in the mesh
			Landscape.instance.RemoveFlag(placementTileX, placementTileY, TileFlag.BuildingWillBePlaced_RuntimeAssigned);
			Landscape.instance.AddFlag(tileX, tileY, TileFlag.BuildingWillBePlaced_RuntimeAssigned);
			Landscape.instance.RebuildTowerMesh();

			if (towerToPlace)
			{
				towerToPlace.gameObject.SetActive(true); //in case we are dragging from the UI again

				towerToPlace.SetTile(tileX, tileY);
				towerToPlace.RemoveRangeObjects ();
				towerToPlace.CalculateInitialRotation();
			}

			placementTileX = tileX;
			placementTileY = tileY;
		}
	}

	void InvalidatePlacement()
	{
		Landscape.instance.RemoveFlag(placementTileX, placementTileY, TileFlag.BuildingWillBePlaced_RuntimeAssigned);
		Landscape.instance.RebuildTowerMesh();
		RangeObjectPool.Reset();

		placementTileX = int.MaxValue;
		placementTileY = int.MaxValue;

		if (towerToPlace != null)
			towerToPlace.gameObject.SetActive(false);
	}

	void FinalisePlacement()
	{
		if (IsValidPlacement())
		{
			//if this fails, the tower will remove itself from the world etc
			if (towerToPlace.ConfirmPlacement())
			{
				//bring the info panel back up so we can immediately start upgrading
				if (HUD.instance.showInfoPanelOnTowerPlacement)
					TowerInfoPanel.ShowUpgradeInfo(towerToPlace);
			}
		}
		else
		{
			towerToPlace.CancelPlacement();
		}

		//clear local placement data
		towerToPlace = null;
		InvalidatePlacement();
	}

	bool IsValidPlacement()
	{
		return placementTileX != int.MaxValue;
	}

	public void ClearSelection()
	{
		isSelected = false;
	}
}
