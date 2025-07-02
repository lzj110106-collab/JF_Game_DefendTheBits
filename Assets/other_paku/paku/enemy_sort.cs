using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_sort : MonoBehaviour {


    private static enemy_sort pInstance;

    public static enemy_sort Instance
    {
        get
        {
            if (pInstance == null)
                pInstance = FindObjectOfType<enemy_sort>();

            return pInstance;
        }
    }

    //生成炸弹的方法
    public enemy_sort Create(Transform parent)
    {
        GameObject go = (GameObject)Instantiate(Resources.Load("Props/Small_Props"));
        go.transform.parent = parent;
        //go.transform.localPosition = Vector3.zero;
        //go.transform.localScale = Vector3.one;
        go.transform.localPosition = new Vector3(0f, -0.25f, 0f);
        go.transform.localScale = new Vector3(0.8f, 0.8f, 0.7f);
        return go.GetComponent<enemy_sort>();

    }


}
