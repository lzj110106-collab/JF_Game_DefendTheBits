using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour {
    
    public ScrollRect scroll;
    public int rate;
   void OnEnable()
    {
        scroll.verticalNormalizedPosition = 1.0f;
    }

    void Update()
    {
        scroll.verticalNormalizedPosition -= Time.deltaTime/rate;
        if (scroll.verticalNormalizedPosition <= 0.0f)
            OnEnable();
    }
}
