using UnityEngine;
using System.Collections;

//NPCs are just simplified versions of the controllable player characters.
public class NonPlayerCharacter : PlayerCharacter
{
	private TowerSpawnDefenders parentTower;

	public override void Awake()
	{
		base.Awake ();
		type = CharacterType.NPC;
	}

	//pass in the parent tower so that when this NPC dies, the tower
	//can be told to generate a new NPC to take its place
	public void Initialise(TowerSpawnDefenders parent, Vector3 start, Vector3 rallyPoint)
	{
		parentTower = parent;
		position = start;
		UpdateTransform();

		OnInput_MoveToPosition(rallyPoint);
	}
}