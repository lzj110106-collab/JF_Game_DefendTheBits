using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilitySeagulls : PlayerAbility 
{
	public GameObject birdPrefab;
	public GameObject birdShitPrefab;

	GameObject birdsContainer;
	List<GameObject> birds = new List<GameObject>();
	List<Vector3> birdPositionStart = new List<Vector3>();
	List<Vector3> birdPositionEnd = new List<Vector3>();
	int birdCount;

	List<GameObject> birdDroppings = new List<GameObject>();
	List<Vector2> droppingLocations = new List<Vector2>();
	int droppingsUsed;


	public int cashRequired = 10;
	public override int CashRequired() { return cashRequired; }

	public string rangePlotName = "RNG2";
	public override string RangePlotName() { return rangePlotName; }

	enum Direction { Up, Down, Left, Right };
	Direction animationDirection;

	//dont expose bird speed, just a duration for the entire
	//power up effect. this is so that the timing doesnt
	//become dependent on the map size
	public float duration = 5.0f;
	public float birdDuration = 3.0f;
	public float fadeOutDuration = 0.5f;
	public float attackFrequency;
	public float attackSlowAmount;
	public float attackSlowDuration;

	float elapsed;
	float timeBetweenAttacks;

	public bool randomiseBirds = true;
	public bool twoDirectionsOnly = true;

	int locationX;
	int locationY;

	void Start()
	{
		birdsContainer = new GameObject("birds");
		GenerateBirdInstances(16);
		GenerateDroppingInstances(32);
	}

	public override bool Trigger(int tileX, int tileY)
	{
		if (twoDirectionsOnly)
			animationDirection = Random.value < 0.5f ? Direction.Down : Direction.Left;
		else
			animationDirection = (Direction)Random.Range(0, 4);

		var landscape = Landscape.instance;
		var overlay = PathOverlay.instance;

		AudioController.Play ("Ability_Seagull");
		PathOverlay.GetCentreTile(out tileX, out tileY);

		//generate enough birds to cover all the path tiles. this depends on the dimensions
		//of the map and the direction they are animating in from.
		bool useX = animationDirection == Direction.Up || animationDirection == Direction.Down;
		birdCount = useX ? (overlay.maxX - overlay.minX + 1) : (overlay.maxY - overlay.minY + 1);

		//ensure there are enough prefabs to go around
		GenerateBirdInstances(birdCount - birds.Count);
		GenerateDroppingInstances(PathOverlay.AllTileData().Length - birdDroppings.Count);

		int x0 = 0;
		int y0 = 0;
		int x1 = landscape.w - 1;
		int y1 = landscape.h - 1;

		float birdElevation = GameState.instance.level.birdHeight;
		float height = landscape.GetTileCentre(tileX, tileY).y + birdElevation;

		for (int i = 0; i < birdCount; ++i)
		{
			birds[i].SetActive(true);

			int tileIterator = useX ? (overlay.minX + i) : (overlay.minY + i);

			switch (animationDirection)
			{
			case Direction.Up:
				birdPositionStart[i] = landscape.GetTileCentre(tileIterator, y0);
				birdPositionEnd[i] = landscape.GetTileCentre(tileIterator, y1);
				birds[i].transform.rotation = Quaternion.AngleAxis(0.0f, Vector3.up);
				break;

			case Direction.Down:
				birdPositionStart[i] = landscape.GetTileCentre(tileIterator, y1);
				birdPositionEnd[i] = landscape.GetTileCentre(tileIterator, y0);
				birds[i].transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up);
				break;

			case Direction.Left:
				birdPositionStart[i] = landscape.GetTileCentre(x1, tileIterator);
				birdPositionEnd[i] = landscape.GetTileCentre(x0, tileIterator);
				birds[i].transform.rotation = Quaternion.AngleAxis(270.0f, Vector3.up);
				break;

			case Direction.Right:
				birdPositionStart[i] = landscape.GetTileCentre(x0, tileIterator);
				birdPositionEnd[i] = landscape.GetTileCentre(x1, tileIterator);
				birds[i].transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
				break;
			}

			if (randomiseBirds)
			{
				int choice = Random.Range(0, 3);
				float amount = choice == 0 ? 0.0f : (choice == 1 ? 1.0f : -1.0f);

				if (animationDirection == Direction.Up ||
					animationDirection == Direction.Down)
				{
					birdPositionStart[i] += Vector3.forward * (amount * landscape.tileWidth);
					birdPositionEnd[i] += Vector3.forward * ((amount - 10) * landscape.tileWidth);
				}
				else
				{
					birdPositionStart[i] += Vector3.right * (amount * landscape.tileWidth);
					birdPositionEnd[i] += Vector3.right * ((amount - 10) * landscape.tileWidth);
				}
			}

			birdPositionStart[i] += Vector3.up * height;
			birdPositionEnd[i] += Vector3.up * height;

			birds[i].transform.position = birdPositionStart[i];
		}

		elapsed = 0.0f;
		timeBetweenAttacks = 0.0f;
		inProgress = true;

		droppingsUsed = 0;
		locationX = tileX;
		locationY = tileY;

		return true;
	}

	public override void UpdateTick()
	{
		float start = elapsed;
		elapsed += World.frameTime;
		if (elapsed >= duration)
		{
			Restart();
			return;
		}

		float animTriggerTime = duration - fadeOutDuration;
		if (start < animTriggerTime && elapsed >= animTriggerTime)
		{
			for (var i = 0; i < droppingsUsed; ++i)
				birdDroppings[i].GetComponent<Animator>().SetTrigger("Off");
		}
			
		var landscape = Landscape.instance;
		float t = elapsed/birdDuration;

		for (var i = 0; i < birds.Count; ++i)
		{
			var startPosition = birds[i].transform.position;
			var currentPosition = Vector3.Lerp(birdPositionStart[i], birdPositionEnd[i], t);

			int birdX = -1;
			int birdY = -1;
			landscape.GetTileIndexFromPosition(currentPosition.x, currentPosition.z, ref birdX, ref birdY);
		
			//only care about paths
			if (landscape.HasFlag(birdX, birdY, TileFlag.HasPath_RuntimeAssigned))
			{
				var tilePosition = landscape.GetTileCentre(birdX, birdY);
				var validTile = false;

				//see if we passed over the centre of the tile this frame. this
				//calculation depends on the animation direction obviously.
				switch (animationDirection)
				{
				case Direction.Up:
					if (startPosition.z < tilePosition.z && currentPosition.z >= tilePosition.z)
						validTile = true;
					break;

				case Direction.Down:
					if (startPosition.z > tilePosition.z && currentPosition.z <= tilePosition.z)
						validTile = true;
					break;

				case Direction.Left:
					if (startPosition.x > tilePosition.x && currentPosition.x <= tilePosition.x)
						validTile = true;
					break;

				case Direction.Right:
					if (startPosition.x < tilePosition.x && currentPosition.x >= tilePosition.x)
						validTile = true;
					break;
				}

				//was a valid tile. generate droppings.
				if (validTile)
				{
					birdDroppings[droppingsUsed].transform.position = tilePosition;
					birdDroppings[droppingsUsed].transform.rotation = Quaternion.AngleAxis(90 * Random.Range(0, 4), Vector3.up);
					birdDroppings[droppingsUsed].SetActive(true);

					droppingLocations[droppingsUsed] = new Vector2(birdX, birdY);

					droppingsUsed += 1;
				}
			}

			birds[i].transform.position = currentPosition;
		}

		timeBetweenAttacks += World.frameTime;
		if (timeBetweenAttacks >= attackFrequency)
		{
			if (droppingsUsed > 0)
			{
				var query = World.instance.FindCharactersInArea(droppingLocations, droppingsUsed, 0, 0);
				for (int i = 0; i < query.found; ++i)
					query.Get(i).AddStatusEffect(WeaponStatusEffectType.Slow, attackSlowAmount, attackSlowDuration);
			}

			timeBetweenAttacks -= attackFrequency;
		}
	}

	public override void Restart()
	{
		if (inProgress)
		{
			for (var i = 0; i < birds.Count; ++i)
				birds[i].SetActive(false);

			for (var i = 0; i < birdDroppings.Count; ++i)
				birdDroppings[i].SetActive(false);

			inProgress = false;
		}
	}

	public void GenerateBirdInstances(int count)
	{
		if (count >= 0)
		{
			for (int i = 0; i < count; ++i)
			{
				var bird = GameObject.Instantiate(birdPrefab);
				bird.transform.SetParent(birdsContainer.transform, false);
				bird.SetActive(false);
				birds.Add(bird);

				birdPositionStart.Add(Vector3.zero);
				birdPositionEnd.Add(Vector3.zero);
			}
		}
	}

	public void GenerateDroppingInstances(int count)
	{
		if (count >= 0)
		{
			for (int i = 0; i < count; ++i)
			{
				var shit = GameObject.Instantiate(birdShitPrefab);
				shit.transform.SetParent(birdsContainer.transform, false);
				shit.SetActive(false);
				birdDroppings.Add(shit);

				droppingLocations.Add(Vector2.zero);
			}
		}
	}
}
