using UnityEngine;
using System.Collections;

//context menu that floats on a selected tower. used for buy/sell/upgrade/etc
public class TowerContextMenu : MonoBehaviour 
{
	public static TowerContextMenu instance;

	public enum Type { Rotation, Placement };
	Type type;

	public GameObject buttonPrefab;
	float ringRadius = 100.0f;

	Tower currentTower = null;

	void Awake()
	{
		instance = this;
	}

	void OnDestroy()
	{
		instance = null;
	}

	public void Hide()
	{
		if (currentTower != null)
			currentTower.ContextMenuDismissed(type);
		
		gameObject.SetActive(false);
		currentTower = null;

		RangeObjectPool.ClearPlacementArrow();
	}

	public void ShowRotationContext(Tower tower)
	{
		UnityUtil.DestroyAllChildren (gameObject);

		float theta = Mathf.Deg2Rad * 270.0f;
		float incr = Mathf.PI * 0.5f;



		if (!gameObject.activeSelf)
			gameObject.SetActive (true);

		var canvas = UserInterface.GetCanvas();
		GetComponent<RectTransform> ().localPosition = canvas.WorldToCanvas (tower.transform.position);


		/*var button = InstantiateButton (theta);
		button.SetButtonInteraction (tower.FinalisePlacementOnNonRotaters);
		button.text.text = "Place";
		button.cost.gameObject.SetActive(false);*/

		theta += incr;
		var button = InstantiateButton (theta);
		button.SetButtonInteraction (() => tower.Rotate(1));
		//button.text.text = "Rotate Right";
		//button.cost.gameObject.SetActive(false);
		//button.transform.Translate (Vector3.right * 0.15f, Space.World);

		/*theta -= incr * 2;
		button = InstantiateButton (theta);
		button.SetButtonInteraction (() => tower.Rotate(-1));
		button.text.text = "Rotate Left";
		button.cost.gameObject.SetActive(false);
		button.transform.Translate (-Vector3.right * 0.15f, Space.World);*/

		currentTower = tower;
		type = Type.Rotation;

		RangeObjectPool.ShowPlacementArrow(tower);
	}

	public void ShowPlacementContext(Tower tower)
	{
		gameObject.SetActive (true);
		UnityUtil.DestroyAllChildren (gameObject);

		float theta = Mathf.Deg2Rad * 270.0f;
		float incr = Mathf.PI * 0.5f;


		var canvas = UserInterface.GetCanvas();
		GetComponent<RectTransform> ().localPosition = canvas.WorldToCanvas (tower.transform.position);

		theta += incr;
		var button = InstantiateButton (theta);
		button.SetButtonInteraction (() => ConfirmPlacement());
		button.text.text = "Build";
		button.cost.gameObject.SetActive(false);
		button.transform.Translate (Vector3.right * 0.15f, Space.World);

		theta -= incr * 2;
		button = InstantiateButton (theta);
		button.SetButtonInteraction (() => CancelPlacement());
		button.text.text = "Cancel";
		button.cost.gameObject.SetActive(false);
		button.transform.Translate (-Vector3.right * 0.15f, Space.World);

		currentTower = tower;
		type = Type.Placement;
	}

	TowerContextMenuButton InstantiateButton (float theta)
	{
		var button = GameObject.Instantiate(buttonPrefab).GetComponent<TowerContextMenuButton>();
		button.transform.SetParent(transform, false);
		//button.transform.localPosition = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0) * ringRadius;
		button.transform.localPosition = new Vector3(0.0f, -100.0f, 0.0f);
		
		return button;
	}

	void ConfirmPlacement()
	{
		currentTower.ConfirmPlacementContextMenu();
		currentTower = null;

		Hide();
	}

	void CancelPlacement()
	{
		currentTower.CancelPlacementContextMenu();
		currentTower = null;

		Hide();
	}
}
