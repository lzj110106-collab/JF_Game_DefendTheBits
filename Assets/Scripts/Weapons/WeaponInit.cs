using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Weapon : MonoBehaviour 
{
	public void OnTowerInitialised(Tower tower)
	{
		var info = tower.towerInfo;
		var infoArt = tower.towerArtInfo;

		//don't reset weapon rotation data during upgrades
		if (tower.level == 1)
			plotRotation = 0;

		//pull in new plot info
		plotName = info.plotName;
		plotData = RangePlots.GetPlotData(info.plotName, plotRotation);
		GeneratePlotMinimalSet();

		//setting up art hooks
		fireLocations = infoArt.fireLocations;
		meshTransform = infoArt.swivelPoint;

		projectilePrefab = infoArt.projectile;
		projectilePathPrefab = infoArt.projectilePath;
		
		pfx = tower.pfx;
		SetAnimator(tower.animator);

		OnWeaponDataChanged(tower);
	}

	public void OnWeaponDataChanged(Tower tower)
	{
		weaponData = tower.weaponData;
		UpdateTargetCap(tower.weaponData.maxTargets);
	}
}
