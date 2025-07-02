using UnityEngine;
using System.Collections;

public class EnemyCharacterAnimationResponder : MonoBehaviour 
{
	public EnemyCharacter parent;

	public void OnDeathComplete()
	{
		if (parent != null)
			parent.OnDeathAnimationComplete();
	}
}
