using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowTowerInfo : MonoBehaviour
{
    public Animator[] anis;
    private int lastIndex;
    void OnEnable()
    {
        lastIndex = -1;
    }

    public void ShowTowerInfos(int index)
    {
        if (lastIndex == -1)
        {
            anis[index].SetTrigger("show");
            lastIndex = index;
        }
        else
        {
            anis[lastIndex].SetTrigger("hide");
            anis[index].SetTrigger("show");
            lastIndex = index;
        }

        StartCoroutine(YieldToHide());
    }

   IEnumerator YieldToHide()
    {
        yield return new WaitForSeconds(3.0f);
        anis[lastIndex].SetTrigger("hide");
        lastIndex = -1;
    }
}
