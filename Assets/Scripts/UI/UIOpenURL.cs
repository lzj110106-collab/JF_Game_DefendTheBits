using UnityEngine;
using System.Collections;

public class UIOpenURL : MonoBehaviour
{
	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	public void OpenURLAndroidOnly(string url)
	{
		#if UNITY_ANDROID
		Application.OpenURL(url);
		#endif
	}

	public void OpenUrliOSOnly(string url)
	{
		#if UNITY_IOS
		Application.OpenURL(url);
		#endif
	}
}
