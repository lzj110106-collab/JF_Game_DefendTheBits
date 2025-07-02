using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppointRewardButton : MonoBehaviour {

    public bool isActive;
    public Sprite activeImg;
    public Sprite notActiveImg;
    public Image buttonImg;
    public int index;
    public ShopScreen shop;
    public Text claimText;

    public void RefeashState()
    {
        if (isActive)       //如果可以点击，并且没有被领取
        {
            if (!SaveData.GetAppointRewardStatue(index))
            {
                buttonImg.sprite = activeImg;
                claimText.text = "领取";
            }
            else
            {
                buttonImg.sprite = notActiveImg;
                claimText.text = "已领取";
            }
        }
        else
        {
            buttonImg.sprite = notActiveImg;
            claimText.text = "无法领取";
        }
    }


    public void AppointRewardButtonPressed()
    {
        if (!isActive|| SaveData.GetAppointRewardStatue(index))
            return;

        switch (index)
        {
            case 0:
                SaveData.AddCash(66);
                break;
            case 1:
                SaveData.AddCash(233);
                //开启一个中饰品宝箱
                shop.GetToken(2);
                break;
            case 2:
                SaveData.AddCash(666);
                //开启一个大饰品宝箱
                shop.GetToken(5);
                //解锁巨魔防御塔
                SaveData.UnlockTower("ogre");
                break;
        }
        buttonImg.sprite = notActiveImg;
        claimText.text = "已领取";
        SaveData.SetAppointReward(SaveData.GetAppointReward() + 1);
        SaveData.SetAppointRewardStatue(index);
    }
}
