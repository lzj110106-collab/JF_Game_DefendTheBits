using UnityEngine;
using System.Collections;

public class UISceneManager : MonoBehaviour {

	public static UISceneManager instance;
	public UISceneIDs startupScene;
	[HideInInspector] public UpgradeCharacterLocator upgradeCharacterLocator;

	UIScene currentScene;
	UIScene[] scenes;
	public UISceneCamera uiSceneCamera;
	bool blurEnabled = true;

	void Awake ()
	{
		instance = this;
		scenes = GetComponentsInChildren<UIScene>(true);
		uiSceneCamera = GetComponentInChildren<UISceneCamera>(true);
		upgradeCharacterLocator = GetComponentInChildren<UpgradeCharacterLocator>(true);

		if(startupScene != UISceneIDs.None)
			SetScene(startupScene);
	}
	
	public void SetScene(UISceneIDs scene)
	{
		if(currentScene != null)
			currentScene.Disable();

		for(int i=0; i<scenes.Length; i++)
		{
			if(scenes[i].id == scene && scene != UISceneIDs.None)
			{
				uiSceneCamera.Enable();
				scenes[i].Enable();
				currentScene = scenes[i];
				//UserInterface.SceneBlur().enabled = (blurEnabled && currentScene.blur);
				return;
			}
		}

		//UserInterface.SceneBlur().enabled = false;
		uiSceneCamera.Disable();
	}
		
	public void ToggleBlur()
	{
		blurEnabled = !blurEnabled;
		//UserInterface.SceneBlur().enabled = (blurEnabled && currentScene.blur);
	}
}
