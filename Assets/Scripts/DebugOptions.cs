using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOptions : MonoBehaviour 
{
	public bool showFPS = true;
	float fpsTimer = 0.0f;
	int currentFps = 0;

	void Update()
	{
		if (Input.anyKeyDown)
		{
			if (Application.platform == RuntimePlatform.OSXEditor ||
				Application.platform == RuntimePlatform.WindowsEditor)
			{
				//command-k to win the level
				//command-shift-k to lose the level
				if (Input.GetKeyDown(KeyCode.K))
				{
					if (Input.GetKey(KeyCode.LeftCommand) ||
						Input.GetKey(KeyCode.LeftControl))
					{
						GameState.TriggerEOR();
					}
				}

				if (Input.GetKeyDown(KeyCode.R))
					SaveData.ResetSaveData();

				if (Input.GetKeyDown(KeyCode.F))
					SaveData.SkipFTUE();

				if (Input.GetKeyDown(KeyCode.T))
				{
					foreach (var kv in TrinketDatabase.trinketPrefabsByID)
					{
						HUD.IncreaseTrinketTally(kv.Key, 100);	
						SaveData.AddTrinket(kv.Key, 100);
					}
				}

				if (Input.GetKeyDown(KeyCode.U))
					TowerLoader.UnlockAll();

				if (Input.GetKeyDown(KeyCode.L))
					LevelDatabase.UnlockAll();

				if (Input.GetKeyDown(KeyCode.A))
					AchievementDatabase.DebugUnlockNextAchievement();


				if (Input.GetKeyDown(KeyCode.Alpha1))
					UserInterface.GetRateAppDialog().Show();
			}
		}
	}

	void OnGUI()
	{
		if (showFPS)
		{
			fpsTimer -= Time.deltaTime;
			if (fpsTimer <= 0.0f)
			{
				currentFps = (int)(1.0f/Time.smoothDeltaTime);
				fpsTimer = 0.5f;
			}

			var style = new GUIStyle();
			style.fontSize = (int) (Screen.height * 0.06f);
			style.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width * 0.025f, Screen.height * 0.125f, 100, 100), string.Format("FPS:{0}", currentFps) , style);        
		}
	}
}
