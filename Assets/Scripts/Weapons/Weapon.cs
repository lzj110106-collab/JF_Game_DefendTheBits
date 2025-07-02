using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum WeaponType 
{
	ArcProjectile,
	AreaBurst,
	CloseCombat,
	Instantaneous,
	FollowPath,
	Laser,
	Lightning,
	TrackingProjectile,
	ProjectileReturning,
	CloseCombatAllTargets,
	FourWayDirectional,
	Bullet,
	Passive
}
	
[System.Flags]
public enum WeaponCategory
{
	None = 0,
	Melee = 1,
	Projectile = 2,
	Explosive = 4,
}

[System.Flags]
public enum WeaponStatusEffectType
{
	None = 0,
	Burn = 1,
	Poison = 2,
	Slow = 4,
	Stun = 8,
	Freeze = 16
}

[System.Serializable]
public partial class Weapon : MonoBehaviour 
{
	public WeaponType type = WeaponType.Instantaneous; //attack type

	public WeaponCategory category; //targeting type
	public WeaponData weaponData; //actual stats (damage, etc);

	string plotName;
	public int plotRotation { get; private set; }
	public List<Vector2> plotData { get; private set; }
	public List<Vector2> plotDataMinimalSet { get; private set; }

	public Color debugColor = Color.white;
	public Color flashColor = Color.white;

	[HideInInspector] public float fireAnimationSpeed = 0.5f;
	float animTime = 1.0f; //used to determine time between attacks

	//all this info is pulled from TowerArtHooks
	public GameObject projectilePrefab { get; private set; }
	public GameObject projectilePathPrefab { get; private set; }
	public TowerDefensePFXContainer pfx { get; private set; }

	//list of origins to fire projectiles and things from. the size is equivalent to maxTargets
	public List<GameObject> fireLocations { get; private set; }
	[HideInInspector] public int maxEnemiesHit = 1;

	public WeaponTargetController targetController { get; private set; }

	private Transform cachedTransform;
	float timeSinceLastFired = 0;
	float timeBetweenAttacks = 0;

	public Animator animator { get; private set; }
	public Transform meshTransform { get; private set; }

	//rotation params
	const float defaultRotationCountdown = 5.0f; //for rotating back towards the path after enemies go away
	const float rotationSpeed = 540.0f; //degrees per second
	float defaultRotationTimer;
	float defaultRotation;
	float currentRotation = 0;


	public CameraShake.Shake onFireCameraShake;
	public CameraShake.Shake onImpactCameraShake;

	enum State { Idle, AttackStart, AttackLoop, AttackEnd };
	State currentState = State.Idle;
	float stateTimer = 0.0f;

	//the weapon always belongs to either a tower or a character.
	[HideInInspector] public Character parentCharacter = null;
	[HideInInspector] public Tower parentTower = null;

	LineRenderer laserRenderer;

	[Header("Path Overlays")]
	public PathOverlayData.Type pathOverlayType;
	public float pathOverlayDuration = 5.0f;
	public bool createPathOverlays = false;

	[Header("Passive")]
	public float passiveAttackLoopDuration = 2.0f;

	[Space(10)]
	public bool rotateToFaceEnemy = true;
	public bool showKillCount = true;

	public int totalKills { get; private set; }

	public float passivePFXRate = 1.0f;
	float passivePFXTimer;

	AudioObject toesLaserAudioObject;

	public void Initialise(Tower parent)
	{
		parentTower = parent;
		weaponData = parent.weaponData;

		cachedTransform = GetComponent<Transform>();
		parentCharacter = GetComponent<Character>();
		targetController = new WeaponTargetController(this);

		UpdateTransforms();

		currentState = State.Idle;
		timeSinceLastFired = 0.0f;
		totalKills = 0;
	}

	public void OnLoad(int totalKills)
	{
		this.totalKills = totalKills;
	}

	public void OnTowerUpgraded()
	{
		if (type == WeaponType.Laser)
		{
			if (currentState != State.Idle)
			{
				animator.Play("Idle", 0, 0.0f);
				HideLaser();
			}
		}

		currentState = State.Idle;
		stateTimer = 0.0f;
		timeSinceLastFired = 0.0f;
	}

	public void UpdateTick()
	{
		UpdateTransforms();

		timeSinceLastFired -= World.frameTime;
		stateTimer += World.frameTime;
		passivePFXTimer -= World.frameTime;

		switch (currentState)
		{
			case State.Idle:
			{
				if (timeSinceLastFired > 0)
					break;

				if (type == WeaponType.Passive)
				{
					AttackAllInRange(true);
					timeSinceLastFired = timeBetweenAttacks;

					if (passivePFXTimer <= 0.0f)
					{
						pfx.Play(PFX.Weapon_OnFire, true);
						passivePFXTimer = passivePFXRate;
					}
				}
				else if (targetController.FindTarget())
				{
					StartAttackAnimation();
					timeSinceLastFired = timeBetweenAttacks;

					//adjust the state machine for continuous attack type towers.
					if (type == WeaponType.Laser)
						currentState = State.AttackStart;
				}

				break;
			}

			case State.AttackLoop:
			{
				//if continuous fire weapons run out of targets, turn the attack off
				if (type == WeaponType.Laser)
				{
					if (targetController.FindTarget())
					{
						//prevent the attack from happening every frame
						if (timeSinceLastFired <= 0.0f)
						{
							var laserPosition = laserRenderer.transform.position;
							var laserDirection = laserRenderer.transform.forward;
							laserDirection = Vector3.Normalize(new Vector3(laserDirection.x, 0.0f, laserDirection.z));

							var targets = World.FindCharactersInTileRange(this, CharacterType.Enemy);
							for (var i = 0; i < targets.found; ++i)
							{
								//testing if the enemiy lines under the actual laser beam. the range
								//plots of the tower doesnt necessarily form a straight line
								//in front of the tower
								var enemyPosition = targets.Get(i).transform.position;
								var toEnemy = new Vector3(enemyPosition.x - laserPosition.x, 0.0f, enemyPosition.z - laserPosition.z);

								var d = Vector3.Dot(laserDirection, toEnemy);
								if (d < 0.0f)
									continue; //enemy is behind the tower

								var projected = laserPosition + laserDirection * d;
								var distance = Vector3.Magnitude(enemyPosition - projected);

								if (distance <= Landscape.instance.tileWidth)
								{
									ApplyDamage(targets.Get(i));
									stateTimer = 0.0f; //hit something, keep the beam going
								}
							}

							timeSinceLastFired = timeBetweenAttacks;
						}
					}
					else if (stateTimer >= 1.0f)
					{
						HideLaser();

						animator.SetTrigger("AttackEnd");
						currentState = State.AttackEnd;
					}
				}

				break;
			}

			case State.AttackStart:
			case State.AttackEnd:
			{
				//both of these states are waiting for animation events via OnAnimEnd()
				break;
			}
		}
	}

	public void OnEnemyDestroyed(Character e)
	{
		targetController.OnEnemyDestroyed(e);
	}

	public void OnFire(int fireIndex)
	{
		targetController.RefreshTargetLocations();

		switch (type)
		{
			case WeaponType.ArcProjectile:
			case WeaponType.TrackingProjectile:
			case WeaponType.Instantaneous:
			{
				for (int i = 0; i < targetController.currentTargetCount; i++)
				{
					var projectile = ProjectilePool.Get(projectilePrefab).GetComponent<Projectile>();
					projectile.Initialise(this, 
										  GetFirePosition(fireIndex), 
										  targetController.targetLocations[i],
										  targetController.targets[i]);

					if (type == WeaponType.Instantaneous)
						ApplyDamage(targetController.targets[i]);
				}
					
				var location = GetFireLocation(fireIndex);
				if (location != null)
				{
					pfx.Play(PFX.Weapon_OnFire, 
						location.transform.position,
						location.transform.rotation,
						true);
				}

				break;
			}

			case WeaponType.FollowPath:
			{
				var projectile = ProjectilePool.Get(projectilePrefab);
				Vector3 pathRotation =  (targetController.GetTarget(0).transform.position - transform.position).normalized;
				projectile.GetComponent<ProjectileFollowPath>().Initialise(this, 
																		   GetFireLocation(fireIndex), 
																		   projectilePathPrefab, 
																		   pathRotation);
				break;
			}

			case WeaponType.Laser:					
			// Do nothing, laser is controlled in the update loop

					break ;

			case WeaponType.CloseCombat:
			{
				var targetCharacter = targetController.GetTarget(0);
				if (targetCharacter != null)
					ApplyDamage(targetCharacter, true);

				var swivelLocation = parentTower.towerArtInfo.swivelPoint.position;
				var impactLocation = targetController.targetLocations[0];
				impactLocation = Landscape.SnapToMesh(impactLocation, 0.01f);

				var rotation = parentTower.towerArtInfo.swivelPoint.rotation; //inherit tower location
				pfx.Play(PFX.Weapon_OnFire, swivelLocation, rotation, true); 
				pfx.Play(PFX.Weapon_OnHitEnemy, impactLocation, rotation, true); 
					
				ApplySplashDamage(impactLocation, targetCharacter);
				CameraShake.instance.TriggerShake(onImpactCameraShake);

				break;
			}

			case WeaponType.CloseCombatAllTargets:
			{
				AttackAllInRange(false);
				CameraShake.instance.TriggerShake(onImpactCameraShake);
				break;
			}

			case WeaponType.AreaBurst:
			{
				//same as lasers, just attack everything in all tiles
				var targets = World.FindCharactersInTileRange(this, CharacterType.Enemy);
				for (var i = 0; i < targets.found; ++i)
					ApplyDamage(targets.Get(i));

				CameraShake.instance.TriggerShake(onImpactCameraShake);
				pfx.Play(PFX.Weapon_OnFire, transform, true);
				break;
			}

			case WeaponType.FourWayDirectional:
			{
				//ignoring the actual target, the target is just acting as a trigger.
				var dirX = new float[4] { 1.0f, 0.0f, -1.0f, 0.0f };
				var dirZ = new float[4] { 0.0f, 1.0f, 0.0f, -1.0f };

				var firePosition = GetFireLocation(fireIndex).transform.position;

				for (int i = 0; i < 4; ++i)
				{
					var projectile = ProjectilePool.Get(projectilePrefab).GetComponent<Projectile>();
					projectile.Initialise(this, firePosition, firePosition + new Vector3(dirX[i], 0.0f, dirZ[i]), null); 
				}

				pfx.Play(PFX.Weapon_OnFire, true);

				break;
			}

			case WeaponType.Bullet:
			case WeaponType.ProjectileReturning:
			{
				var from = GetFireLocation(fireIndex).transform.position;

				for (int i = 0; i < targetController.currentTargetCount; i++)
				{
					//project the target position onto the fire locator height
					//so that the projectile has no vertical movement
					var to = targetController.targetLocations[i];
					to.y = from.y; 

					//if the weapon is rotation locked, snap the projectile to move
					//along the cardinal directions only.
					if (!rotateToFaceEnemy)
					{
						to.x = from.x + meshTransform.forward.x;
						to.z = from.z + meshTransform.forward.z;
					}

					var projectile = ProjectilePool.Get(projectilePrefab).GetComponent<Projectile>();
					projectile.Initialise(this, from, to, targetController.targets[i]);
				}

				var location = GetFireLocation(fireIndex);
				if (location != null)
					pfx.Play(PFX.Weapon_OnFire, location.transform, true);

				break;
			}

		case WeaponType.Passive:
			//ignore fire events with passive weapons. its all handled in UpdateTick()
			//good spot for SFX as part of the animation though.
			return;

		}

		GenerateOverlays();
	}

	public bool DealsSplashDamage() 
	{ 
		return weaponData.projectileSplashRadius > 0; 
	}

	void ShowLaser()
	{
		Vector3 furthestPos = Vector3.zero;
		float highestDist = 0;

		for (int i = 0; i < plotData.Count; i++)
		{
			float val = Vector2.SqrMagnitude (plotData[i]);
			if (val > highestDist)
			{
				highestDist = val;
				furthestPos = transform.position;
				furthestPos.x += plotData[i].x;
				furthestPos.z += plotData[i].y;
			}
		}

		Vector3 myPos = transform.position;

		//will need to refresh this if the tower is upgraded while the laser is active
		var renderers = parentTower.GetComponentsInChildren<LineRenderer>(true);
			if (renderers != null && renderers.Length > 0)
			{
				laserRenderer = renderers[0];
				laserRenderer.gameObject.SetActive(true);
			
				//adjust z position of the far lineRender point. the rotation of
				//the tower to face the enemy will do the rest
				float length = RangePlots.GetPlotRange(plotName);
				length = (length + 0.5f) * Landscape.instance.tileWidth;
			laserRenderer.SetPosition(1, Vector3.forward * length);
					
					if (toesLaserAudioObject == null)
						toesLaserAudioObject = AudioController.Play ("Tower_Toes_Laser_Lp");
					else if (!toesLaserAudioObject.IsPlaying())
						toesLaserAudioObject.Play();

			AudioController.Play ("Tower_Toes_Laser_Start");

			}
			
	}

	void HideLaser()
	{
		if (laserRenderer != null)
		{
			laserRenderer.gameObject.SetActive(false);
			laserRenderer = null;
			AudioController.Play ("Tower_Toes_Laser_Stop");
		
			if (toesLaserAudioObject != null && toesLaserAudioObject.IsPlaying())
				toesLaserAudioObject.Stop ();
		}
	}

	public void UpdateTransforms()
	{
		float desiredRotation = currentRotation;

		//if we havent targetted an enemy for awhile, revert to the default rotation
		defaultRotationTimer -= World.frameTime;
		if (defaultRotationTimer <= 0.0f)
			desiredRotation = defaultRotation;

		//rotate towards the enemy assuming a firing angle actually exists.
		var target = targetController.GetAnyTarget();
		if (target != null && rotateToFaceEnemy)
		{
			desiredRotation = MathUtil.GetAngleInRadiansToPositionXZ(cachedTransform.position, target.position);		
			defaultRotationTimer = defaultRotationCountdown;
		}

		float movement = World.frameTime*Mathf.Deg2Rad*rotationSpeed;
		currentRotation = MathUtil.UpdateRotationAngle(currentRotation, desiredRotation, movement);

		//rotating the mesh. z axis in art is backwards, so rotate by pi.
		if (meshTransform != null && rotationSpeed > 0)
			meshTransform.rotation = Quaternion.AngleAxis(currentRotation * Mathf.Rad2Deg, Vector3.up);
	}

	public bool ApplyDamage(Character target, bool suppressVFX = false)
	{
		if (target != null)
		{
			bool stillAlive = target.ApplyDirectDamage(this);

			if (!stillAlive)
			{
				var enemyData = ((EnemyCharacter)target).enemyDataInitial;
				AchievementDatabase.AddKill(enemyData.identifier, parentTower.towerInfo.name);

				totalKills += 1;
			}

			if (!suppressVFX)
				pfx.Play(PFX.Weapon_OnHitEnemy, target.transform.position, true);

			return stillAlive;
		}

		return true;
	}

	public void ApplySplashDamage(Vector3 location, Character ignore)
	{
		if (DealsSplashDamage())
		{
			var splashTargets = World.instance.FindCharactersInSplashArea(this, CharacterType.Enemy, location);

			for (int i = 0; i < splashTargets.found; ++i)
			{
				var target = splashTargets.Get(i);

				//dont deal damage twice
				if (target != ignore)
				{
					if (!target.ApplySplashDamage(this))
					{
						var enemyData = ((EnemyCharacter)target).enemyDataInitial;
						AchievementDatabase.AddKill(enemyData.identifier, parentTower.towerInfo.name);

						totalKills += 1;
					}
				}
			}
		}
	}

	public void ApplySplashDamage(Character target, bool ignoreTarget = true)
	{
		ApplySplashDamage(target.transform.position, ignoreTarget ? target : null);
	}

	void OnDrawGizmos()
	{
//		Gizmos.color = currentState == State.Idle ? Color.red :
//			currentState == State.AttackStart ? Color.green :
//			currentState == State.AttackLoop ? Color.blue : 
//			Color.white;
//
//		Gizmos.DrawWireSphere(transform.position, 1.0f);

		if (type == WeaponType.CloseCombat)
		{
			var location = GetFireLocation(0);
			if (location != null)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(location.transform.position, 0.5f);
			}
		}
	}

	public Vector3 GetFirePosition(int index)
	{
		if (fireLocations != null && index < fireLocations.Count && fireLocations[index] != null)
			return fireLocations[index].transform.position;

		Debug.Log ("fire location " + index + " not set for tower " + name);
		return transform.position + Vector3.up;
	}

	public GameObject GetFireLocation(int index)
	{
		if (fireLocations != null && index < fireLocations.Count && fireLocations[index] != null)
			return fireLocations[index];
		
		return null;
	}

#region ANIMATION

	public void SetAnimator(Animator animator)
	{
		this.animator = animator;

		for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
			if (animator.runtimeAnimatorController.animationClips [i].name.Contains ("Attack"))
				animTime = animator.runtimeAnimatorController.animationClips [i].length;
	}

	void SetIdleAnimation()
	{
		//animator.Play ("Idle", 0, 0.0f);
		animator.SetTrigger("AttackInterrupt");
	}

	void StartAttackAnimation()
	{
		//don't play PFX.Weapon_OnFire here, wait for the OnFire animation trigger 

		fireAnimationSpeed = animTime * weaponData.attacksPerSecond;
		timeBetweenAttacks = 1f / weaponData.attacksPerSecond;

		parentTower.localTimeScale = fireAnimationSpeed;

		//animator.speed = fireAnimationSpeed * World.instance.timeScale;
		animator.SetFloat("AttackSpeed", Mathf.Max(fireAnimationSpeed, 1.0f) );
		animator.SetTrigger("Attack");
	}

#endregion

	public void UpdateTargetCap(int cap)
	{
		if (targetController != null)
			targetController.SetTargetCap (cap);
	}

	public void SetPlotRotation(int rotation)
	{
		//update range info stuff.
		plotRotation = rotation;
		plotData = RangePlots.GetPlotData(plotName, rotation);

		//update mesh related stuff
		currentRotation = Mathf.Round(90.0f * plotRotation);
		meshTransform.rotation = Quaternion.AngleAxis(currentRotation, Vector3.up);

		//refresh min set for attacks and stuff
		GeneratePlotMinimalSet();
	}

	public void GeneratePlotMinimalSet()
	{
		var tower = GetComponent<Tower>();

		//when constructing the minimal set, shift the coordinates into world
		//space so that it doesnt have to be done repeatedly for each enemy
		plotDataMinimalSet = new List<Vector2>(plotData.Count);
		for (int i = 0; i < plotData.Count; ++i)
		{
			int xx = (int)(plotData[i].x + tower.tileX);
			int yy = (int)(plotData[i].y + tower.tileY);

			if (Landscape.instance.HasFlag(xx, yy, TileFlag.HasPath_RuntimeAssigned))
				plotDataMinimalSet.Add(new Vector2(xx, yy));
		}
			
		//using the minimal set to generate the default tower rotation value.
		int currentFloodFillIndex = -1;

		for (int j = 0; j < plotDataMinimalSet.Count; ++j)
		{
			int xx = (int)plotDataMinimalSet[j].x;
			int yy = (int)plotDataMinimalSet[j].y;

			//keep the tower default rotation snapped to 90 deg increments
			if (xx == tower.tileX || yy == tower.tileY)
			{
				//see if its floodfill value is larger than the minimum stored so far.
				//testing larger numbers because the floodfill occurs from end
				//nodes outwards.
				int floodFillIndex = Landscape.GetFloodFillIndex(xx, yy);
				if (floodFillIndex > currentFloodFillIndex)
				{
					currentFloodFillIndex = floodFillIndex;

					var pos0 = Landscape.instance.GetTileCentre(tower.tileX, tower.tileY);
					var pos1 = Landscape.instance.GetTileCentre(xx, yy);

					defaultRotation = MathUtil.GetAngleInRadiansToPositionXZ(pos0, pos1);
				}
			}
		}

		//none of the towers minimal set overlaps a valid path, so in this case, have
		//the thing face the direction that the plot rotation is aligned with
		if (currentFloodFillIndex == -1)
		{
			int offsetX = plotRotation == 1 ? 1 : plotRotation == 3 ? -1 : 0;
			int offsetY = plotRotation == 0 ? 1 : plotRotation == 2 ? -1 : 0;

			var pos0 = Landscape.instance.GetTileCentre(tower.tileX, tower.tileY);
			var pos1 = Landscape.instance.GetTileCentre(tower.tileX + offsetX, tower.tileY + offsetY);

			defaultRotation = MathUtil.GetAngleInRadiansToPositionXZ(pos0, pos1);
		}
	}

	public void OnWorldReset()
	{
		targetController.Clear();
		HideLaser();
	}

	public void OnAnimEnd()
	{
		if (type == WeaponType.Laser)
		{
			if (currentState == State.AttackStart)
			{
				if (type == WeaponType.Laser)
					ShowLaser();
				else
					GenerateOverlays(); //passive overlay

				currentState = State.AttackLoop;
				stateTimer = 0.0f;
			}
			else if (currentState == State.AttackEnd)
			{
				currentState = State.Idle;
				timeSinceLastFired = timeBetweenAttacks;
			}
		}
	}

	int AttackAllInRange(bool suppressVFX)
	{
		var targets = World.FindCharactersInTileRange(this, CharacterType.Enemy);
		for (var i = 0; i < targets.found; ++i)
			ApplyDamage(targets.Get(i), suppressVFX);

		if (!suppressVFX)
			CameraShake.instance.TriggerShake(onImpactCameraShake);

		if (type == WeaponType.Passive)
			GenerateOverlays();

		return targets.found;
	}

	void GenerateOverlays()
	{
		if (createPathOverlays)
		{
			for (int i = 0; i < plotDataMinimalSet.Count; ++i)
			{
				PathOverlay.PlaceOverlay(pathOverlayType, 
					(int)plotDataMinimalSet[i].x,
					(int)plotDataMinimalSet[i].y,
					pathOverlayDuration);
			}
		}
	}
}
