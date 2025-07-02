using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class GameState : MonoBehaviour
{
	public static GameState instance;

	public HUD hud;
	public EOR eor;
    public EOR eor_en;
	public MainMenu mainMenu;

	public LevelData level { get; private set; }
	public World world { get; private set; }

	public enum State { MainMenu, Game, EndOfRound };
	public State currentState { get; private set; }

	[Header("Global State")]
	public bool randomiseEndlessWaves = false;
	
	public void Awake()
	{
		Application.targetFrameRate = 60;
		currentState = State.MainMenu;
		instance = this;
		//AudioController.Play ("Music_Menu");

        //make sure the initial save data state is set up correctly on first load.
        //previously the TowerLoader would unlock default towers, but now some
        //towers are locked by achievements, since deciding default unlocks
        //would rely on both of them referencing each other in their load
        //functions, its better to resolve it all here after they have
        //both loaded
        //if (PlayerPrefs.GetInt("first_load", 1) == 1)
        if (ObscuredPrefs.GetInt("first_load", 1) == 1)
            SaveData.ResetSaveData();
	}

	public void OnDestroy()
	{
		instance = null;
	}

	public static void TriggerGame(LevelData data)
	{
		instance.level = data;
		SaveData.ClearSaveState(); //started a new game, clear the old

		var prefabInstance = GameObject.Instantiate(data.prefab.mapPrefab);
		prefabInstance.transform.SetParent(instance.transform, false);

		//init HUD first. the world initialisation will potentially
		//modify it. eg showing the level hint pop-up
		instance.hud.Initialise(data);

		instance.world = prefabInstance.GetComponent<World>(); 
		instance.world.Initialise(data);

		//make sure all the pooled objects are inactive.
		//TODO: maybe do this at EOR instead. depends on UI I guess.
		RangeObjectPool.Reset();
		TowerContextMenu.instance.Hide ();

		//disable main menu rendering stuff
		// Now used for canvas_top so leaving this on for now
		//UserInterface.Camera3D().gameObject.SetActive(false);

		instance.currentState = State.Game;

		AudioController.Stop ("Music_Menu");
	}

	public static void TriggerResumeGame()
	{
		var header = SaveData.GetSaveStateHeader();
		var data = LevelDatabase.GetLevelData(header[(int)SaveStateHeader.LevelName]);

		instance.level = data;

		var prefabInstance = GameObject.Instantiate(data.prefab.mapPrefab);
		prefabInstance.transform.SetParent(instance.transform, false);

		//init HUD first. the world initialisation will potentially
		//modify it. eg showing the level hint pop-up
		instance.hud.Initialise(data);

		instance.world = prefabInstance.GetComponent<World>(); 
		instance.world.LoadState(header);

		//make sure all the pooled objects are inactive.
		//TODO: maybe do this at EOR instead. depends on UI I guess.
		RangeObjectPool.Reset();
		TowerContextMenu.instance.Hide ();

		instance.currentState = State.Game;		

		AudioController.Stop ("Music_Menu");
	}

	public static void TriggerEndlessMode()
	{
		//continue on in endless mode. 
		World.TriggerEndlessMode();
		HUD.instance.Show();
		HUD.UnlockStartWaveButton(false);

		instance.currentState = State.Game;
	}

	public void TriggerMainMenu()
	{
		hud.Hide ();
		mainMenu.Show ();

		DestroyWorld();

		UserInterface.Camera3D().gameObject.SetActive(true);
		CurrencyDisplay.HideTrinketDisplay();
		CurrencyDisplay.ShowStarDisplay();
		CurrencyDisplay.ShowStoreButton();

		currentState = State.MainMenu;
		AudioController.Play ("Music_Menu");
		AudioController.StopCategory ("Music_Game", 0.3f);

	}

	public void RestartWorld()
	{
        if (LocManager.isInChina())
            eor.Hide();
        else
            eor_en.Hide();

		hud.Initialise(level);
		world.Restart();

		currentState = State.Game;
	}

	public void DestroyWorld()
	{
		if (world != null)
		{
			world.Restart(); //return all pooled objects
			GameObject.Destroy(world.gameObject);
			world = null;
		}
	}

	public static void TriggerEOR()
	{
		instance._TriggerEOR(); 
		AudioController.StopCategory ("Music_Game", 0.3f);
	}

	void _TriggerEOR()
	{
		if (currentState == State.Game)
		{
			hud.Hide();
            if (LocManager.isInChina())
                eor.Show(level);
            else
                eor_en.Show(level);
			currentState = State.EndOfRound;
		}
	}
		
	void OnApplicationPause(bool paused)
	{
		if (currentState == State.Game && paused)
		{
			if (World.instance != null && !World.instance.paused)
			{
				//this will call TogglePause on this object and deal
				//with hiding the HUD and all that jazz
				PauseMenu.instance.Show();
			}
		}
	}
}
