using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Adjusting : MonoBehaviour {


    public GameObject obj;

    public GameObject speedObj;

    public void ApplyBtnPressed()
    {
        if (transform.GetChild(0).GetComponent<InputField>().text == "" || transform.GetChild(0).GetComponent<InputField>().text == null|| transform.GetChild(1).GetComponent<InputField>().text == "" || transform.GetChild(1).GetComponent<InputField>().text == null)
        {
            return;
        }
        float value1 = float.Parse(transform.GetChild(0).GetComponent<InputField>().text);
        for(int i = 0; i < obj.transform.GetChild(0).childCount; i++)
        {
            List<InteractRewardData> temp = obj.transform.GetChild(0).GetChild(i).GetComponent<EnemyCharacter>().tapRewardOnKill;
            InteractRewardData tempData = new InteractRewardData();
            tempData.prefab = temp[1].prefab;
            tempData.chanceToSpawn = value1;
            tempData.maxNumberToSpawn = temp[1].maxNumberToSpawn;
            obj.transform.GetChild(0).GetChild(i).GetComponent<EnemyCharacter>().tapRewardOnKill[1] = tempData;
        }
        //speedObj.GetComponent<SpeedButtonControl>().speedInfos[1].speedMultiplier = 2;
        speedObj.GetComponent<SpeedButtonControl>().speedInfos[1].speedMultiplier = int.Parse(transform.GetChild(1).GetComponent<InputField>().text);
    }
}
