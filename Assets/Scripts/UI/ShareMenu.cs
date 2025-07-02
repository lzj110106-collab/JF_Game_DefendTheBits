using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ShareMenu : MonoBehaviour
{
    bool isInGame;

    //分享截图的时候，分享按钮一般是需要隐藏的
    //分享按钮的背景图
    public Image shareBtnImage { get; set; }
    //分享按钮的字体
    public Text shareBtnText { get; set; }
    //分享按钮的图标
    public Image shareBtnIcon { get; set; }


    //分享面板弹出来之后，其他界面需要隐藏，防止玩家点击后面的UI也响应，项目中已解决这个问题的话可忽略，也可采用其他方法
    public Animator[] targetAnim { get; set; }

    //用来显示最终截图的image
    public Image shotImage;

    //二维码
    public Texture2D code1;

    public Texture2D code2;

    //二维码偏移
    public Vector2 offset;

    //分享类型，0：分享固定的图片，1：分享实时截图
    int shareType;

    /// <summary>
    /// 点击分享按钮
    /// </summary>
    public void ShareButtonPressed(bool temp)
    {
        isInGame = temp;

        shareType = 1;

        //隐藏分享按钮元素
        shareBtnImage.enabled = false;
        shareBtnText.enabled = false;
        shareBtnIcon.enabled = false;
        gameObject.SetActive(true);
        //开启协程截图
        StartCoroutine(ScreenShot());
    }

    //截图的宽高
    float width;
    float height;
    //图片最终转换成byte数组
    byte[] bt;

    IEnumerator ScreenShot()
    {
        yield return new WaitForEndOfFrame();       //当前帧渲染完毕之后再截图

        width = Screen.width;
        height = Screen.height;
        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);

        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();


        Texture2D code;

        if ((float)Screen.width / (float)Screen.height > 1.5f)
            code = code2;
        else
            code = code1;

        //计算绘制二维码时的开始坐标和结束坐标
        int startX = texture.width - code.width;
        int endX = startX + code.width;
        int startY = (int)offset.y;
        int endY = startY + code.height;

        //开始绘制
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Color bgcolor = texture.GetPixel(x, y);
                Color codecolor = code.GetPixel(x - startX, y - startY);
                Color final = Color.Lerp(bgcolor, codecolor, codecolor.a / 1.0f);
                texture.SetPixel(x, y, final);
            }
        }
        //对Texture2D对象作出修改后都要应用一下
        texture.Apply();

        //转换成数组备用
        bt = texture.EncodeToPNG();
        //将最终处理好的图片显示出来
        shotImage.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero);

        //最后再显示分享面板，防止分享面板也被截图截到
        ShowShareUI();
    }


    /// <summary>
    /// 显示分享界面
    /// </summary>
    /// <returns></returns>
    void ShowShareUI()
    {


        //分享面板通过动画显示出来
        GetComponent<Animator>().SetTrigger("on");

        //其他界面通过动画隐藏，防止玩家点击到
        for (int i = 0; i < targetAnim.Length; i++)
            targetAnim[i].SetBool("Share", true);
    }

    /// <summary>
    /// 隐藏分享界面,玩家点击关闭button时响应该方法
    /// </summary>
    public void HideShareUI()
    {
        if (isInGame)
        {
            PanelManager.Instance.SwitchToScreen(PanelID.Share, PanelID.HUD);
            World.instance.TogglePause();
            FTUE.OnPauseResume();
            UserInterface.GetFTUEGuide().OnResume();
            return;
        }

        shareBtnImage.enabled = true;
        shareBtnText.enabled = true;
        shareBtnIcon.enabled = true;

        GetComponent<Animator>().SetTrigger("off");
        //gameObject.SetActive(false);

        for (int i = 0; i < targetAnim.Length; i++)
            targetAnim[i].SetBool("Share", false);
    }

    /// <summary>
    /// 点击分享到会话按钮
    /// </summary>
    public void ShareToWeChatBtnPressed()
    {
        ShareToWeChat(bt, shareType, bt.Length, 0);
    }

    /// <summary>
    /// 点击分享到朋友圈
    /// </summary>
    public void ShareToFriendsBtnPressed()
    {
        ShareToWeChat(bt, shareType, bt.Length, 1);
    }

    [DllImport("__Internal")]
    private static extern void ShareToWeChat(byte[] imageData, int shareType, int arrayLength, int approach);
}
