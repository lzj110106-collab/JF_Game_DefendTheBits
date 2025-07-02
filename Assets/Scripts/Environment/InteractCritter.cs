using UnityEngine;
using System.Collections;

public class InteractCritter : InteractObject {

	public Animator animator;

	public float moveDistance = 1.0f;
	public float moveSpeed = 1.0f;
	public float moveAnimSpeed = 1.0f;

	public enum MoveDirections { Left, Right, Forward, Back}
	public MoveDirections[] movePattern;
	public int currentMove;

	private Vector3 destination;

	public override void Awake()
	{
		base.Awake();
		if(animator == null)
			animator = GetComponent<Animator>();
		animator.speed = moveAnimSpeed;

		currentMove = 0;
	}


	public void Update()
	{
		if (InputUtil.MousePressed() && InputUtil.IsWorldHovered())
		{
			OnTouch();
		}

		if(destination != Vector3.zero)
		{
			transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);
			if(transform.position == destination)
				destination = Vector3.zero;
		}
	}

	public override void OnSpawned(GameObject sourcePrefab, Vector3 spawnLocation) {}
	public override void OnSwiped(Vector3 swipeDirection){}

	public override void OnTouch()
	{
		if(!isInteracting)
		{
			Ray ray = Camera.main.ScreenPointToRay(InputUtil.MousePosition());
			RaycastHit info;

			if (interactCollider.Raycast(ray, out info, float.MaxValue))
			{
				isInteracting = true;
				MoveCritter();
			}
		}
	}

	public void MoveCritter()
	{
		// Orient to face move direction
		switch (movePattern[currentMove])
		{
			case MoveDirections.Left:
				transform.eulerAngles += new Vector3(0.0f, 90.0f, 0.0f);
				break;
			case MoveDirections.Right:
				transform.eulerAngles += new Vector3(0.0f, -90.0f, 0.0f);
				break;
			case MoveDirections.Forward:
				// No direciton change
				break;
			case MoveDirections.Back:
				transform.eulerAngles += new Vector3(0.0f, 180.0f, 0.0f);
				break;
			default:
				Debug.LogError("Unhandled MoveDirections " + movePattern[currentMove]);
				break;

		}

		destination = transform.position + transform.forward * moveDistance;
		animator.SetTrigger("MoveForward");

		// Increment and wrap move pattern
		currentMove++;
		if(currentMove >= movePattern.Length)
			currentMove = 0;
	}
}
