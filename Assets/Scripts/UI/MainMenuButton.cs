using UnityEngine;
using System.Collections;

public class MainMenuButton : MonoBehaviour {

	public void LoadMainMenu ()
	{
		//NB: poorly named class. this exists in the Pause Menu in the main game.
		//need to make sure we reset all the object pooling before we go
		//back to the main menu, because things will break the next time the
		//game is loaded
		if (World.instance != null)
			World.instance.Restart();

		GameState.instance.TriggerMainMenu();
	}
}
