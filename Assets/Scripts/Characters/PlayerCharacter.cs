using UnityEngine;
using System.Collections;

//NB: this class is deprecated. keeping it around just in case.
//NB: will need to fix character pathing to use the new PathNetworkNodes

public class PlayerCharacter : Character 
{
	public float awarenessRadius = 2.0f;
	public float lockEnemyMovementRadius = 1.0f;

	//rally point is the last position the player character was supposed to be in,
	//that is, the end of a path, a position the human player directed the character to, etc.
	private Vector3 rallyPoint;

	private enum PlayerState
	{
		Combat,
		Idle,
		MoveToPosition,
		MoveToTarget,
		ReturnToRallyPoint
	};

	PlayerState state = PlayerState.Idle;

	public override void Awake()
	{
		base.Awake();
		type = CharacterType.Player;

		rallyPoint = position;

//		var path = new GameObject();
//		currentPath = path.AddComponent<Path>();
	}

	public override bool UpdateTick()
	{
		UpdateStatusEffects();

		switch (state)
		{
		case PlayerState.Combat:				UpdateCombat();					break;
		case PlayerState.Idle:					UpdateIdle();					break;
		case PlayerState.MoveToPosition:		UpdateMoveToPosition();			break;
		case PlayerState.MoveToTarget: 			UpdateMoveToTarget(); 			break;
		case PlayerState.ReturnToRallyPoint:	UpdateReturnToRallyPoint();		break;
		}

		UpdateTransform();

		return true;
	}

	public void OnInput_MoveToPosition(Vector3 destination)
	{
//		if (state == PlayerState.Combat || state == PlayerState.MoveToTarget)
//		{
//			//free up the person we are attacking if they are focused on us
//			targetCharacter.OnEndCombat(this);
//		}
//
//		currentPath.Initialise(position, destination);
//		SetState (PlayerState.MoveToPosition);
	}

	public override void OnCharacterKilled(Character character)
	{
		if (character == targetCharacter)
			SetState(PlayerState.Idle);
	}
	
	public override bool OnDamageReceived(Weapon weapon, float amount)
	{
		return true;
	}

	public override void OnPathComplete()
	{

	}

	public override bool OnStartCombat(Character character)
	{
		//TODO: possibly change priorities if we get attacked by a certain type of character
		return true;
	}

	public override void OnEndCombat(Character character)
	{
	}

	public override void OnPrepareForCombat(Character character)
	{
		//nothing
	}

	void UpdateCombat()
	{
//		if (targetCharacter != null)
//			targetCharacter.OnAttacked(this, weapon);
//
//		if (targetCharacter == null || !targetCharacter.alive)
//		{
//			//killed or lost the target. find an new one
//			targetCharacter = FindNewTarget();
//			if (targetCharacter != null)
//			{
//				SetState (PlayerState.MoveToTarget);
//				return;
//			}
//
//			//couldnt find one. go home.
//			SetState (PlayerState.ReturnToRallyPoint);
//		}
	}

	void UpdateIdle()
	{
		//search for a enemy within range to attack
		targetCharacter = FindNewTarget();

		if (targetCharacter != null)
			SetState(PlayerState.MoveToTarget);
	}

	void UpdateMoveToPosition()
	{
		if (FollowPath())
		{
			rallyPoint = position; //end of the path is our new rally point
			SetState (PlayerState.Idle);
		}
	}

	//LEGACY
	void UpdateMoveToTarget()
	{
//		//if we lost our target, go back to idle to search for a new one next frame
//		if (targetCharacter == null)
//		{
//			SetState (PlayerState.ReturnToRallyPoint);
//			return;
//		}
//
//		//target may still be moving, so recalc our seek position.
//		//move to the closest position to the target such that both their radii touch
//		Vector3 direction = Vector3.Normalize(position - targetCharacter.position);
//		Vector3 moveTo = (radius + targetCharacter.radius) * direction + targetCharacter.position;
//
//		if (Seek(moveTo))
//		{
//			//arrived at the target
//			SetState (PlayerState.Combat);
//			targetCharacter.OnStartCombat(this);
//		}
//		else if (Vector3.Magnitude(moveTo - position) < lockEnemyMovementRadius)
//		{
//			//close enough to the enemy now that we mean business
//			targetCharacter.OnPrepareForCombat(this);
//		}
	}

	void UpdateReturnToRallyPoint()
	{
		//if somebody else wandered into range, go after them
		targetCharacter = FindNewTarget();
		if (targetCharacter != null)
		{
			SetState (PlayerState.MoveToTarget);
			return;
		}

		//otherwise just move to the original location
		if (Seek (rallyPoint))
			SetState (PlayerState.Idle);
	}

	void SetState(PlayerState state)
	{
//		Debug.Log ("Setting player state: " + state);

		//state specific set up code. play animations etc.
//		switch (state)
//		{
//		case PlayerState.Idle:
//			break;
//
//		case PlayerState.MoveToPosition:
//			targetCharacter = null;
//			pathIndex = 0;
//			pathT = 0.0f;
//			pathOffset = 0.0f;
//			break;
//
//		default:
//			break;
//		}

		this.state = state;
	}

	Character FindNewTarget()
	{
		//TODO: better way of figuring out what the best character to attack is. 
		var potentialTargets = World.instance.FindCharactersInArea(position, 
		                                                           awarenessRadius, 
		                                                           CharacterType.Enemy);

		for (int i = 0; i < potentialTargets.found; ++i)
		{
			var target = potentialTargets.Get(i);
			if (target.type == CharacterType.Enemy && target.alive)
				return target;
		}

		return null;
	}
}
