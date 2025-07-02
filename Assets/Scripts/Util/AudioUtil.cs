using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class AudioUtil : MonoBehaviour 
{
	public static bool isSFXMuted { get; private set; }
	public static bool isMusicMuted { get; private set; }

	public void Awake()
	{
        //MuteSFX(PlayerPrefs.GetInt("mute_sfx", 0) == 1);
        //MuteMusic(PlayerPrefs.GetInt("mute_music", 0) == 1);

        MuteSFX(ObscuredPrefs.GetInt("mute_sfx", 0) == 1);
        MuteMusic(ObscuredPrefs.GetInt("mute_music", 0) == 1);
    }

	public static void MuteSFX(bool mute)
	{
//		AudioController.SetCategoryVolume("SFX", mute ? 0.0f : 1.0f);
//		AudioController.SetCategoryVolume("VO", mute ? 0.0f : 1.0f);
//		AudioController.SetCategoryVolume("UI", mute ? 0.0f : 1.0f);
//		AudioController.SetCategoryVolume("Ambience", mute ? 0.0f : 1.0f);

		isSFXMuted = mute;
		//PlayerPrefs.SetInt ("mute_sfx", mute ? 1 : 0);

        ObscuredPrefs.SetInt("mute_sfx", mute ? 1 : 0);
    }

	public static void MuteMusic(bool mute)
	{
//		AudioController.SetCategoryVolume("Music", mute ? 0.0f : 1.0f);

		isMusicMuted = mute;
		//PlayerPrefs.SetInt ("mute_music", mute ? 1 : 0);

        ObscuredPrefs.SetInt("mute_music", mute ? 1 : 0);
    }

	public static void ToggleSFX()
	{
		MuteSFX (!isSFXMuted);
	}

	public static void ToggleMusic()
	{
		MuteMusic (!isMusicMuted);
	}
}
