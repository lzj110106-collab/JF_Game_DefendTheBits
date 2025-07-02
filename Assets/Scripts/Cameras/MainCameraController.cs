#if UNITY_IOS || UNITY_ANDROID
#define TOUCH_INPUT
#endif

using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MainCameraController : MonoBehaviour 
{
	public static MainCameraController instance;

	public float rotationAngleDegrees = 45;
	public float rotationTiltDegrees = 45;
	public float distanceFromFocusWide = 45.0f;	

	//TEMP old variables for controlling camera movement, the camera is now fixed.
	float panSpeedX = -0.1f;
	float panSpeedY = 0.1f;
	
	float minZoom = 2.0f;
	float maxZoom = 10.0f;
	float zoomSpeed = 4.0f;
	float distanceFromFocus = 35.0f;

#if UNITY_EDITOR || UNITY_WEBPLAYER
	Vector3 mouseDown;
	bool mouseDidMove;
	bool mouseRotate;
	bool mouseZoom;
	float mouseDeadZone = 20;
#endif

#if TOUCH_INPUT
	List<Vector3> touchPositionInitial;
	List<Vector3> touchPositionCurrent;
	int touchesUsed;

	float initialTwoTouchDistance;
	float initialTwoTouchAngle;
#endif

	Vector3 cameraPositionSpherical; //r, theta, phi
	Vector3 cameraTarget;

	Vector3 initialCameraPositionSpherical;
	Vector3 initialCameraTarget;
	public float directionInDegrees { get; private set; }

	public Camera cachedCamera { get; private set; }

	PlayerCharacter selectedPlayer;
	bool movingCamera = false;

	void Awake()
	{
	#if TOUCH_INPUT
		touchPositionInitial = new List<Vector3>(2);
		touchPositionCurrent = new List<Vector3>(2);

		for (int i = 0; i < 2; ++i)
		{
			touchPositionInitial.Add (Vector3.zero);
			touchPositionCurrent.Add (Vector3.zero);
		}

		touchesUsed = 0;
	#endif

		instance = this;
	}

	void OnDestroy()
	{
		instance = null;
	}

	// Use this for initialization
	void Start ()
	{
		cachedCamera = GetComponentInChildren<Camera>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		#if UNITY_EDITOR || UNITY_WEBPLAYER
		//prevent ui interactions passing through to the game
		if (!EventSystem.current.IsPointerOverGameObject())
		{

			HandleMouseInput();

		}
		#else
		if (!EventSystem.current.IsPointerOverGameObject(0))
		{
			HandleTouchInput();
		}
		#endif

		UpdateCameraTransform();
	}

	public void SetFocus(CameraFocusPoint focus)
	{
		if(cachedCamera == null)
			cachedCamera = GetComponentInChildren<Camera>();

		cachedCamera.enabled = true;
		cameraTarget = focus.transform.position;
		distanceFromFocus = Mathf.Lerp(focus.distanceFromFocusWide, focus.distanceFromFocus, (cachedCamera.aspect - 1.3f) / (1.7f - 1.3f));
		UpdateCameraTransform();
	}

	void HandleMouseInput()
	{
	#if UNITY_EDITOR || UNITY_WEBPLAYER
		if(World.instance==null)
			return;
			
		if (Input.GetMouseButtonDown(0))
			OnTouchStart(Input.mousePosition);

		if (Input.GetMouseButtonUp(0))
			OnTouchEnd(0);

//		if (Input.GetMouseButton(0))
//		{
//			if (Vector3.Magnitude(Input.mousePosition - mouseDown) > mouseDeadZone)
//				mouseDidMove = true;
//
//			if (movingCamera)
//			{
//				if (mouseRotate)
//				{
//					float diff = Input.mousePosition.x - mouseDown.x;
//					float angle = diff * 0.5f * Mathf.Deg2Rad; //TODO: rotation speed
//					cameraPositionSpherical.z = initialCameraPositionSpherical.z + angle;
//				}
//				else if (mouseZoom)
//				{
//					float diff = Input.mousePosition.y - mouseDown.y;
//					float zoom = diff * 0.1f; //TODO: zoom speed 
//					cameraPositionSpherical.x = initialCameraPositionSpherical.x - zoom;
//					cameraPositionSpherical.x = Mathf.Clamp (cameraPositionSpherical.x, 2.0f, 10.0f); //TODO: zoom bounds
//				}
//				else
//				{
//					Vector3 eye = SphericalToCartesian(initialCameraPositionSpherical);
//					Vector3 dir = Vector3.Normalize(new Vector3(-eye.x, 0, -eye.z)); //move in x/z plane
//					Vector3 side = Vector3.Normalize (Vector3.Cross (dir, Vector3.up));
//
//					float speed = 0.05f; //TODO: pan speed
//					float moveX = (Input.mousePosition.x - mouseDown.x) * speed; 
//					float moveY = (Input.mousePosition.y - mouseDown.y) * speed;
//
//					cameraTarget = initialCameraTarget - dir*moveY + side*moveX;
//				}
//			}
//			else if (mouseDidMove)
//			{
//				movingCamera = true;
//
////				Vector3 poi;
////				if (selectedPlayer && FindIntersectionWithWorld(out poi))
////					selectedPlayer.OnInput_MoveToPosition(poi);
//
//				//TODO: can update tower rally points or targetting here
//			}
//		}
	#endif
	}

	void HandleTouchInput()
	{
	#if TOUCH_INPUT
		//first handle touches starting and ending
		for (int i = 0; i < 2 && i < Input.touchCount; ++i)
		{
			var touch = Input.GetTouch(i);

			if (touch.phase == TouchPhase.Began)
			{
				if (touchesUsed == 0 && touch.fingerId == 0)
				{
					//OnTouchStart will deal with selecting game objects
					if (!OnTouchStart(touch.position))
					{
						//nothing was selected, start a drag
						touchPositionInitial[0] = touch.position;
						touchPositionCurrent[0] = touch.position;
						touchesUsed = 1;
					}
				}
				else if (touchesUsed == 1 && touch.fingerId == 1)
				{
					touchPositionInitial[1] = touch.position;
					touchPositionCurrent[1] = touch.position;
					
					//treating the start of the second pointer as the start of
					//a two pointer gesture, so overwrite the first pointers
					//initial state with its current state
					touchPositionInitial[0] = touchPositionCurrent[0];
					
					//figure out the start rotation and zoom for future updates
					initialTwoTouchDistance = CalcTouchDistance();
					initialTwoTouchAngle = CalcTouchRotation();
					
					//store orientation of the camera at the start of the pinch/zoom etc
					initialCameraTarget = cameraTarget;
					initialCameraPositionSpherical = cameraPositionSpherical;

					touchesUsed = 2;
				}
			}
			else if (touch.phase == TouchPhase.Moved)
			{
				touchPositionCurrent[i] = touch.position;
			}
			else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
			{
				//cancel all input
				touchesUsed = 0;
			}
		}

		//now handle the movement
		if (touchesUsed == 1)
		{
			//single pointer panning
			var touch = Input.GetTouch(0);

			float rx = touch.position.x - touchPositionInitial[0].x;
			float ry = touch.position.y - touchPositionInitial[0].y;
			
			var eye = SphericalToCartesian(initialCameraPositionSpherical); //relative to origin
			var dir = Vector3.Normalize(new Vector3(-eye.x, 0, -eye.z)); //move in the x/z plane
			var side = Vector3.Normalize(Vector3.Cross(dir, Vector3.up));
			
			cameraTarget = initialCameraTarget + rx*panSpeedX*side + ry*panSpeedY*dir;
			
			//TODO:bind the target to the size of the current world area.
		}
		else if (touchesUsed == 2)
		{
			float distance = CalcTouchDistance();
			float angle = CalcTouchRotation();
			
			//pinch gesture to control zoom
			float zoomAmount = zoomSpeed * (distance - initialTwoTouchDistance)/initialTwoTouchDistance;
			cameraPositionSpherical.x = initialCameraPositionSpherical.x + zoomAmount;
			cameraPositionSpherical.x = Mathf.Clamp(cameraPositionSpherical.x, minZoom, maxZoom);
			
			//rotation gesture maps directly to the camera rotation
			cameraPositionSpherical.z = initialCameraPositionSpherical.z - (angle - initialTwoTouchAngle);
		}
	#endif
	}

	Vector3 SphericalToCartesian(Vector3 spherical)
	{
		return new Vector3(spherical[0]*Mathf.Sin(spherical[1])*Mathf.Cos(spherical[2]),
		            	   spherical[0]*Mathf.Cos(spherical[1]),
		            	   spherical[0]*Mathf.Sin(spherical[1])*Mathf.Sin(spherical[2]));
	}

	void UpdateCameraTransform()
	{
		//overwrite any camera movement via the mouse. using a fixed camera now.
		//leaving the current camera code as is, just in case. also it
		//handles mouse picking of towers etc.
		cameraPositionSpherical = new Vector3(distanceFromFocus,
		                                      rotationTiltDegrees * Mathf.Deg2Rad,
		                                      rotationAngleDegrees * Mathf.Deg2Rad);

		//convert spherical coords to cartesian to send to the camera component
		var cartesian = SphericalToCartesian(cameraPositionSpherical);
		transform.position = cameraTarget + cartesian;
		transform.LookAt(cameraTarget, Vector3.up);

		//store camera facing direction so that tower billboarding is super simple.
		directionInDegrees = MathUtil.GetAngleInDegreesToPositionXZ(transform.position, cameraTarget);
	}

	bool OnTouchStart(Vector3 screenPosition)
	{
		if (World.instance == null)
			return false;
		//start input
	#if UNITY_EDITOR || UNITY_WEBPLAYER
		mouseDown = screenPosition;
		mouseDidMove = false;
		movingCamera = false;
	#endif
		
		Ray ray = cachedCamera.ScreenPointToRay(screenPosition);
		
//		selectedTowerSite = null;
		selectedPlayer = (PlayerCharacter)World.instance.PickCharacterOfType(ray, CharacterType.Player);
		
		if (selectedPlayer == null)
		{
//			selectedTowerSite = World.instance.FindIntersectionWithTower(ray);
//			if (selectedTowerSite == null)
			{
				initialCameraTarget = cameraTarget;
				initialCameraPositionSpherical = cameraPositionSpherical;

			#if UNITY_EDITOR || UNITY_WEBPLAYER
				mouseRotate = Input.GetKey(KeyCode.LeftAlt);
				mouseZoom = Input.GetKey (KeyCode.LeftControl) && !mouseRotate;
			#endif
			}
		}

		if (selectedPlayer)
		{
			//if we click on the same character twice, deselect it
			if (World.instance.selectedPlayerCharacter == selectedPlayer)
				selectedPlayer = null;

			World.instance.SelectPlayer(selectedPlayer);
//			World.instance.SelectTowerSite(null);
		}
//		else if (selectedTowerSite)
//		{
//			World.instance.SelectPlayer(null);
//			World.instance.SelectTowerSite(selectedTowerSite);
//		}
//		else
//		{
//			World.instance.SelectTowerSite(null);
//		}

		return selectedPlayer != null;// || selectedTowerSite != null;
	}

	void OnTouchEnd(int index)
	{
		if (movingCamera)
		{
			//stop all camera movement
			movingCamera = false;
		}
		else
		{
			//a single click in empty space. if we have a player selected already
			//then move them to the new postition
			if (!selectedPlayer)// && !selectedTowerSite)
			{
				Vector3 poi;
				if (World.instance.selectedPlayerCharacter && FindIntersectionWithWorld(out poi))
					World.instance.selectedPlayerCharacter.OnInput_MoveToPosition(poi);
			}
		}

		selectedPlayer = null;
//		selectedTowerSite = null;
	}

#if TOUCH_INPUT 
	float CalcTouchDistance()
	{
		return Vector3.Magnitude(touchPositionCurrent[0] - touchPositionCurrent[1]);
	}
	
	float CalcTouchRotation()
	{
		var mid = 0.5f*(touchPositionCurrent[0] + touchPositionCurrent[1]);
		var direction = Vector3.Normalize(touchPositionCurrent[0] - mid);
		
		//acosf(dot(dir, xaxis)) to get the angle
		return Mathf.Acos(direction.x) * (direction.y < 0 ? -1.0f : 1.0f);
	}
#endif

	bool FindIntersectionWithWorld(out Vector3 pointOfIntersection)
	{
	#if	UNITY_EDITOR || UNITY_WEBPLAYER
		return World.instance.FindIntersectionWithWorld(cachedCamera.ScreenPointToRay(mouseDown), out pointOfIntersection);
	#endif
	

	#if TOUCH_INPUT
		return World.instance.FindIntersectionWithWorld(cachedCamera.ScreenPointToRay(touchPositionInitial[0]), out pointOfIntersection);
	#endif

		return false;
	}

	public static Vector3 WorldToScreen(Vector3 worldPosition)
	{
		return instance.cachedCamera.WorldToScreenPoint(worldPosition);
	}

	public static Vector3 ScreenToWorld(Vector3 screenPosition)
	{
		return instance.cachedCamera.ScreenToWorldPoint(screenPosition);
	}

	public static bool IsWorldPointVisibleInScreenSpace(Vector3 worldPosition)
	{
		var result = WorldToScreen(worldPosition);

		//TODO: might want to experiment with not using the exact screen size for this.
		return result.x >= 0 && result.x < Screen.width &&
			result.y >= 0 && result.y < Screen.height;
	}
}
