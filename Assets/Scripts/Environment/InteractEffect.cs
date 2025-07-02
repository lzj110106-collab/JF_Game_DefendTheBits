using UnityEngine;
using System.Collections;

public class InteractEffect : InteractObject {

	public float cooldown = 0.125f;
	public GameObject effectPrefab;
	private float refreshTime;

	public override void Awake()
	{
		base.Awake();
	}


	public void Update()
	{
		//TODO: not sure whats going on with the fixed time stuff in a non-fixed time function
		if (Time.fixedTime > refreshTime && InputUtil.MouseDrag())
			OnTouch();
	}

	public override void OnSpawned(GameObject sourcePrefab, Vector3 spawnLocation) {}
	public override void OnSwiped(Vector3 swipeDirection){}

	public override void OnTouch()
	{
		Ray ray;
		RaycastHit info;

		if((GameState.instance.currentState == GameState.State.MainMenu))
			ray = UISceneManager.instance.uiSceneCamera.camera.ScreenPointToRay(InputUtil.MousePosition());
		else
			ray = Camera.main.ScreenPointToRay(InputUtil.MousePosition());

		if (interactCollider.Raycast(ray, out info, float.MaxValue))
		{
			GameObject pfxObj = (GameObject) Instantiate(effectPrefab);
			pfxObj.transform.position = info.point;

			Vector3 midPoint = new Vector3(info.transform.position.x, info.point.y, info.transform.position.z);
			pfxObj.transform.rotation = Quaternion.LookRotation(info.point - midPoint);

			pfxObj.GetComponent<ParticleSystem>().Play();
			Destroy(pfxObj, pfxObj.GetComponent<ParticleSystem>().main.duration);
			isInteracting = true;
			refreshTime = Time.fixedTime + cooldown;
			OnFinished();
		}
	}
}
