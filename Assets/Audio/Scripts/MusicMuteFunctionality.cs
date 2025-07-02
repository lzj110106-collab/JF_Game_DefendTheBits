using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using CodeStage.AntiCheat.ObscuredTypes;

public class MusicMuteFunctionality : MonoBehaviour {
	
	public AudioMixer masterMixer;
	
	public GameObject muteSFXSprite;
	public GameObject unmuteSFXSprite;

	public GameObject muteMusicSprite;
	public GameObject unmuteMusicSprite;

	public static bool SfxOn 
	{
		//get { return PlayerPrefs.GetInt("SfxOn", 1) == 1; }
		//set { PlayerPrefs.SetInt("SfxOn", value ? 1 : 0); }

        get { return ObscuredPrefs.GetInt("SfxOn", 1) == 1; }
        set { ObscuredPrefs.SetInt("SfxOn", value ? 1 : 0); }
    }

	public static bool musicOn 
	{
		//get { return PlayerPrefs.GetInt("musicOn", 1) == 1; }
		//set { PlayerPrefs.SetInt("musicOn", value ? 1 : 0); }

        get { return ObscuredPrefs.GetInt("musicOn", 1) == 1; }
        set { ObscuredPrefs.SetInt("musicOn", value ? 1 : 0); }
    }

	void Awake () // Check save data for current mute status.
		
	{
		SetSFX();
		SetMusic();
	}


	public void ToggleSFX()
	{
		SfxOn = !SfxOn;
		SetSFX();
	}

	public void ToggleMusic()
	{
		musicOn = !musicOn;
		SetMusic();
	}

	public void SetSFX ()

	{
		if (SfxOn) 
		{
			masterMixer.SetFloat ("MASTERSFX", 0);
			muteSFXSprite.SetActive (false);
			unmuteSFXSprite.SetActive (true);	
		}
		
		else
		{
			masterMixer.SetFloat ("MASTERSFX", -90f);
			muteSFXSprite.SetActive (true);
			unmuteSFXSprite.SetActive (false);
		}

	}

	public void SetMusic ()
		
	{
		if (musicOn) 
		{
			masterMixer.SetFloat ("Music", 0);
			muteMusicSprite.SetActive (false);
			unmuteMusicSprite.SetActive (true);	
		}
		
		else
		{
			masterMixer.SetFloat ("Music", -90f);
			muteMusicSprite.SetActive (true);
			unmuteMusicSprite.SetActive (false);
		}
		
	}


}


