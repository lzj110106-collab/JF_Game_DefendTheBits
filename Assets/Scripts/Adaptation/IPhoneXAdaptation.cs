using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IPhoneXAdaptation : MonoBehaviour {

    public GameObject[] upgrade;
    public GameObject upgradeObject;
    public GameObject[] questPopup;
    public GameObject settingBtn;
    public GameObject backBtn;
    public GameObject backBtn_Mission;
    public GameObject backBtn_Tip;
    public GameObject backBtn_LevelSelect;
    public GameObject backBtn_LevelDetails;
    public GameObject backBtn_Credits;
    public GameObject btnStartWave;
    public GameObject btnShare;
    public GameObject BtnSpeed;
    public GameObject BtnShare;
    public GameObject shopBtn;
    public GameObject trinketObj;
    public GameObject currencyBtn;
    public GameObject pauseBtn;
    public GameObject goldText;
    public GameObject FTUEGuide;
    public GameObject hintBtn;
    public GameObject startShow;
    public GameObject towerLocked;
    public GameObject towerList;


    public Transform mainMenu;
    public Transform levelDetailContent;

    
    public Vector3 leftTopBtn;
    public Vector3 rightTopBtn;


    public GameObject anim1;

    public RectTransform bg;
    public RectTransform shotImage;

    public Transform BG;
    public RectTransform closeBtn;
    public RectTransform header;
    public RectTransform frame;

    // Use this for initialization
    void Start () {
        if (Screen.width / Screen.height >= 2)
        {
            //print("iPhone X");
            //当机型是iPhone X的时候，要加载对应的动画
            upgradeObject.GetComponent<Animator>().runtimeAnimatorController = anim1.GetComponent<Animator>().runtimeAnimatorController;
            
            for (int i = 0; i < questPopup.Length; i++)
               questPopup[i].transform.localPosition += new Vector3(100, 0, 0);

            settingBtn.transform.localPosition = leftTopBtn;
            backBtn.transform.localPosition = leftTopBtn;
            backBtn_Mission.transform.localPosition = leftTopBtn;
            backBtn_Tip.transform.localPosition = leftTopBtn;
            backBtn_LevelSelect.transform.localPosition = leftTopBtn;
            backBtn_LevelDetails.transform.localPosition = leftTopBtn;
            backBtn_Credits.transform.localPosition = leftTopBtn;
            shopBtn.transform.localPosition = rightTopBtn;
            trinketObj.transform.localPosition += new Vector3(-35, -16, 0);
            currencyBtn.transform.localPosition = rightTopBtn;
            pauseBtn.transform.localPosition += new Vector3(35,-16,0);
            goldText.transform.localPosition += new Vector3(35, -16, 0);
            FTUEGuide.transform.localPosition += new Vector3(80,0,0);
            hintBtn.transform.localPosition += new Vector3(100, 0, 0);
            startShow.transform.localPosition += new Vector3(55,-20,0);
            btnStartWave.transform.localPosition += new Vector3(-50, 50, 0);
            btnShare.transform.localPosition += new Vector3(-50, 50, 0);
            BtnSpeed.transform.localPosition += new Vector3(-50, 50, 0);
            towerLocked.transform.localPosition += new Vector3(100,0,0);
            towerList.transform.localPosition += new Vector3(0,50,0);

            bg.sizeDelta = new Vector2(1027 * ((float)Screen.width / (float)Screen.height)-50, 1027);
            bg.localPosition=new Vector3(bg.localPosition.x+30, bg.localPosition.y, bg.localPosition.z);
            shotImage.sizeDelta = new Vector2(875 * ((float)Screen.width / (float)Screen.height)-50, 875);
        }
        else
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            bg.sizeDelta = new Vector2(1027 * ((float)Screen.width / (float)Screen.height), 1027);
            shotImage.sizeDelta = new Vector2(875 * ((float)Screen.width / (float)Screen.height), 875);
            towerLocked.transform.localPosition += new Vector3(-100, 0, 0);
        }

        //print(UnityEngine.iOS.Device.generation.ToString());
#if IOS
        if (UnityEngine.iOS.Device.generation.ToString().Substring(0, 3) == "iPa"|| Screen.width / Screen.height<1.4f)
        {
            mainMenu.localScale = new Vector3(0.75f,0.75f,0.75f);
            levelDetailContent.localScale = new Vector3(0.75f, 0.75f, 0.75f);

            ////隐私政策等窗口调整
            //BG.localScale = new Vector3(1,1.33f,1);
            //closeBtn.localPosition = new Vector3(-79.80237f,105,0);
            //header.localPosition = new Vector3(0, 202, 0);
            //frame.offsetMax = new Vector2(126.5f, -102.25f);
            //frame.offsetMin = new Vector2(54.5f, -81.64999f);
        }
        else
        {
            mainMenu.localScale = Vector3.one;
            levelDetailContent.localScale = Vector3.one;

            ////隐私政策等窗口调整
            //BG.localScale = new Vector3(1, 1, 1);
            //closeBtn.localPosition = new Vector3(-79.80237f, -72.6055f, 0);
            //header.localPosition = new Vector3(0, 0, 0);
            //frame.offsetMax = new Vector2(126.5f, 150);
            //frame.offsetMin = new Vector2(54.5f, 50);
        }
#endif
    }
}
