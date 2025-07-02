using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerArtHooks : MonoBehaviour 
{
	public Sprite icon;
	public List<GameObject> fireLocations = new List<GameObject>();
	public GameObject projectile;
	public GameObject projectilePath;
	public Transform swivelPoint;
	public TowerDefensePFXContainer PFX;
}
