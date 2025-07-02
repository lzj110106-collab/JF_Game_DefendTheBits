using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ShareButton : MonoBehaviour
{
    public ShareMenu shareMenu;
    public Animator[] targetAnim;
    
    public void ShareButtonPressed(bool isInGame)
    {
        //shareMenu.gameObject.SetActive(true);
        if (HUD.instance&& World.instance&&isInGame)
        {
            HUD.instance.Hide();
            World.instance.TogglePause();
            UserInterface.GetFTUEGuide().OnPause();
        }

        shareMenu.shareBtnImage = GetComponent<Image>();
        shareMenu.shareBtnText = transform.GetChild(0).GetComponent<Text>();
        shareMenu.shareBtnIcon= transform.GetChild(1).GetComponent<Image>();
        shareMenu.targetAnim = targetAnim;

        shareMenu.ShareButtonPressed(isInGame);
    }
}
