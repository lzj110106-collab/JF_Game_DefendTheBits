using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class splashScreen : MonoBehaviour {
    
    private int width;
    private int height;
    [SerializeField] private RectTransform image_E2wlogo;

    [SerializeField] private RectTransform image_GonghuLogo;
    [SerializeField] private RectTransform image_CanelLogo;
    [SerializeField] private bool isShowChanelLogo;
    private void Start()
    {

        if (isShowChanelLogo)
        {
            image_E2wlogo.gameObject.SetActive(false);
            image_CanelLogo.gameObject.SetActive(true);
            image_GonghuLogo.gameObject.SetActive(false);
            Invoke("showLogo", 2f);
        }
        else
        {
            image_E2wlogo.gameObject.SetActive(false);
            image_CanelLogo.gameObject.SetActive(true);
            showLogo();
        }

    }

    void showLogo()
    {
        if(isShowChanelLogo)
            image_CanelLogo.gameObject.SetActive(false);
        image_E2wlogo.gameObject.SetActive(true);
        Invoke("loadNext", 2f);
    }

    void loadNext()
    {
        image_E2wlogo.gameObject.SetActive(false);
        image_GonghuLogo.gameObject.SetActive(true);
        Invoke("loadNextScene", 2f);
       
    }

    void loadNextScene()
    {
        SceneManager.LoadScene("CacheScene");
    }


}
