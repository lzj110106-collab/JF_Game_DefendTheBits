using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    private static ExitGame instance;
    public static ExitGame Instance
    {

        get
        {
            return instance;
        }

    }

    private void Awake()
    {
        instance = this;
    }
    public void Exitgame()
    {
        Application.Quit();
    }

    public void SetSelfFalse()
    {
        this.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void SetSelfTrue()
    {
        this.transform.GetChild(0).gameObject.SetActive(true);
    }

}
