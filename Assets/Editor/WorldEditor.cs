using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor (typeof(World))]
public class WorldEditor : Editor
{
	static World world;
	static Collider worldCollider;

	public void OnEnable()
	{
		world = (World)target;
		worldCollider = world.GetComponentInChildren<TerrainCollider>();

		SceneView.onSceneGUIDelegate += OnSceneGUI;
	}

	public void OnDestroy()
	{
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
	}

	public static void OnSceneGUI(SceneView view)
	{
		Event e = Event.current;

		//flipping coordinate systems arond so that mouse picking works
		var screen = new Vector3(e.mousePosition.x, -e.mousePosition.y + view.camera.pixelHeight, 0);

		if (e.type == EventType.MouseDown && e.button == 1)
		{
			//find intersection with the world map
			Ray ray = view.camera.ScreenPointToRay(screen);
			var info = new RaycastHit();
			
			if(worldCollider != null)
			{
				if (worldCollider.Raycast(ray, out info, float.MaxValue))
				{
					var poi = ray.GetPoint(info.distance);

					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Add Player Base"), false, AddPlayerBase, poi);
					menu.AddItem(new GUIContent("Add Tower Site"), false, AddTowerSite, poi);
					menu.ShowAsContext();
				}
			}
		}
	}

	public static void AddTowerSite(object userData)
	{
		var position = (Vector3)userData;

		var prefabPath = "Assets/Prefabs/TowerPlacementSite.prefab";
		var prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

		go.transform.parent = world.transform;
		go.transform.localPosition = position + 0.5f*Vector3.up; //TEMP shift up to get on world geo.
	}

	public static void AddPlayerBase(object userData)
	{
		var position = (Vector3)userData;
		
		var prefabPath = "Assets/Prefabs/PlayerBase.prefab";
		var prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		
		go.transform.parent = world.transform;
		go.transform.localPosition = position + 0.5f*Vector3.up; //TEMP shift up to get on world geo.
	}
}
