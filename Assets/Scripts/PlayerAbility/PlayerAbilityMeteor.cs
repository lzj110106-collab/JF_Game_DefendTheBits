using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityMeteor : PlayerAbility 
{
	public int cashRequired = 10;
	public override int CashRequired() { return cashRequired; }

	public string rangePlotName = "RNG2";
	public override string RangePlotName() { return rangePlotName; }

	public GameObject meteorPrefab;
	GameObject meteorInstance;
	Animator meteorAnimator;

	public GameObject scorchPrefab;
	List<GameObject> scorchInstances = new List<GameObject>();
	List<Animator> scorchAnimators = new List<Animator>();
	int scorchInstancesUsed = 0;

	GameObject meteorContainer;

	public int damage;
	int locationX;
	int locationY;

	public float damageDelay = 3.0f;
	public float duration = 5.0f;
	public float scorchMarkFadeDuration = 1.0f;
	float elapsed = 0.0f;

	public void Start()
	{
		meteorContainer = new GameObject("meteor");

		meteorInstance = GameObject.Instantiate(meteorPrefab);
		meteorInstance.transform.SetParent(meteorContainer.transform, false);
		meteorInstance.SetActive(false);

		meteorAnimator = meteorInstance.GetComponent<Animator>();
		meteorAnimator.enabled = false;

		GenerateScorchInstances(32);
		scorchInstancesUsed = 0;
	}

	public override bool Trigger(int tileX, int tileY)
	{
		AudioController.Play ("Ability_Meteor");

		//find the painted meteor location. if its not found, use the centre of the map
		if (!Landscape.FindFirstTileOfType(TileFlag.MeteorLocation, out locationX, out locationY))
			PathOverlay.GetCentreTile(out locationX, out locationY);			

		meteorInstance.SetActive(true);
		meteorInstance.transform.position = Landscape.instance.GetTileCentre(locationX, locationY);
		meteorAnimator.Play("Strike", 0, 0.0f);

		inProgress = true;
		elapsed = 0.0f;

		return true;
	}

	public override void UpdateTick()
	{
		meteorAnimator.Update(World.frameTime);

		float start = elapsed;
		elapsed += World.frameTime;

		if (start < damageDelay && elapsed >= damageDelay)
		{
			//hit everything
			for (int i = 0; i < World.instance.CharacterCount(); ++i)
				World.instance.characters[i].ApplyDamage(damage);

			PlaceScorchMarks();
		}
		else if (start < duration && elapsed >= duration)
		{
			Restart();
		}
		else
		{
			float animTrigger = duration - scorchMarkFadeDuration;
			if (start < animTrigger && elapsed >= animTrigger)
			{
				for (int i = 0; i < scorchInstancesUsed; ++i)
					scorchAnimators[i].SetTrigger("Off");
			}
		}

		for (int i = 0; i < scorchInstancesUsed; ++i)
			scorchAnimators[i].Update(World.frameTime);
	}

	public override void Restart()
	{
		if (inProgress)
		{
			meteorInstance.SetActive(false);
			inProgress = false;

			ClearScorchMarks();
		}
	}

	void PlaceScorchMarks()
	{
		//place on all path tiles
		var tileData = PathOverlay.AllTileData();

		//expand the scorch pool to meet the demands of the level
		int required = tileData.Length - scorchInstances.Count;
		if (required > 0)
			GenerateScorchInstances(required);

		for (int i = 0; i < tileData.Length; ++i)
		{
			scorchInstances[scorchInstancesUsed].transform.position = Landscape.instance.GetTileCentre(tileData[i].locationX, tileData[i].locationY);
			scorchInstances[scorchInstancesUsed].transform.rotation = Quaternion.Euler(new Vector3(90, 90 * Random.Range(0, 4), 0.0f));
			scorchInstances[scorchInstancesUsed].SetActive(true);
			scorchInstancesUsed += 1;
		}
	}

	void ClearScorchMarks()
	{
		for (var i = 0; i < scorchInstances.Count; ++i)
			scorchInstances[i].SetActive(false);

		scorchInstancesUsed = 0;
	}

	void GenerateScorchInstances(int count)
	{
		for (var i = 0; i < count; ++i)
		{
			scorchInstances.Add(GameObject.Instantiate(scorchPrefab));
			scorchInstances[i].transform.SetParent(meteorContainer.transform, false);
			scorchInstances[i].SetActive(false);

			scorchAnimators.Add(scorchInstances[i].GetComponent<Animator>());
			scorchAnimators[i].enabled = false; //update manually
		}
	}
}