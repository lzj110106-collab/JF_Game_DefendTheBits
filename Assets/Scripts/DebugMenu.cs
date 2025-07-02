using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugMenu : MonoBehaviour {

	public GameObject FPSPanel;

	float sliderMin;
	float sliderMax;
	float sliderCurrent;

	public void Show()
	{
		gameObject.SetActive(true);

		sliderMin = 0.0f;
//		sliderMax = EnemyWaveController.WaveData().Length;
		sliderMax = 150;
		sliderCurrent = EnemyWaveController.CurrentWaveNumber();
	}

	public void AddGold()
	{
		HUD.instance.AddGold (10000);
	}

	public void AddHealth()
	{
		HUD.instance.AddLives (500);
	}

	public void AddCash()
	{
		SaveData.AddCash(500);
	}

	public void ResetSaveData()
	{
		SaveData.ResetSaveData();
	}

	public void FPSToggle()
	{
		FPSPanel.SetActive (!FPSPanel.activeSelf);
	}

	public void SkipWave()
	{
		if (!EnemyWaveController.IsEndlessMode())
			EnemyWaveController.DebugSkipWave();
	}

	public void ToggleBlur()
	{
		UISceneManager.instance.ToggleBlur();
	}

	public void UnlockAll()
	{
		LevelDatabase.UnlockAll();
		TowerLoader.UnlockAll();
	}

	public void ToggleUISceneCamera()
	{
		UISceneManager.instance.uiSceneCamera.camera.enabled = !UISceneManager.instance.uiSceneCamera.camera.enabled;
	}

	public void ToggleGameCamera()
	{
		MainCameraController.instance.cachedCamera.enabled = !MainCameraController.instance.cachedCamera.enabled;
	}

	public void SkipFTUE()
	{
		SaveData.SkipFTUE();
	}

	public void OnGUI()
	{
		if (EnemyWaveController.instance == null || EnemyWaveController.IsEndlessMode())
			return;
		
		var sliderBounds = new Rect(50, 100, 100, Screen.height - 200);
			
		var sliderStyle = new GUIStyle(GUI.skin.verticalSlider);
		var sliderStyleThumb = new GUIStyle(GUI.skin.verticalSliderThumb);

		float scale = 4.0f;
		sliderStyleThumb.fixedWidth *= scale;
		sliderStyleThumb.fixedHeight *= scale;
		sliderStyleThumb.contentOffset = scale*sliderStyleThumb.contentOffset;

		sliderCurrent = GUI.VerticalSlider(sliderBounds, 
			sliderCurrent, 
			sliderMin, 
			sliderMax, 
			sliderStyle, 
			sliderStyleThumb);

		float t = (sliderCurrent - sliderMin)/(sliderMax - sliderMin);
		float y = Mathf.Lerp(sliderBounds.min.y, sliderBounds.max.y, t);

		var buttonText = "Skip To: " + ((int)sliderCurrent + 1).ToString(); //convert to 1-indexing

		if (GUI.Button(new Rect(150, y, 150, 50), buttonText))
			EnemyWaveController.DebugSkipToWave((int)sliderCurrent);
	}
}
