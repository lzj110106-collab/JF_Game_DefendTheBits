using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ReloadSceneMenu : MonoBehaviour
{
	[MenuItem("PlaySide/Reload Current Scene")]
	public static void ShowWindow()
	{
		if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			EditorSceneManager.OpenScene(EditorSceneManager.GetActiveScene().path);
	}
}
