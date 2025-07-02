using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBossCollectInfo : MonoBehaviour
{
	public Image bossImage;
	public Image dropImage;
	public Text dropChanceText;


	public void SetInfo ()
	{
		// TODO: update boss info when there's CSV for it
		//bossImage.sprite = ;
		//dropImage.sprite = ;
		dropChanceText.text = "15%";
	}
}
