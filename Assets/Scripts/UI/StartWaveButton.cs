using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartWaveButton : MonoBehaviour 
{
	public Button button;

	public void OnStartWavePressed()
	{
		EnemyWaveController.OnWaveCalled();
	}

	public void Lock()
	{
		button.interactable = false;
		GetComponent<Animator>().Play("Disabled", 0, 0.0f);
	}

	public void Unlock(bool _highlighted)
	{
		button.interactable = true;
		GetComponent<Animator>().Play(_highlighted?"Highlighted" : "Normal", 0, 0.0f);
	}
}
