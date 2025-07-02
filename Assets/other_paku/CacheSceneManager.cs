using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 此类提供异步缓存加载场景（目前仅支持缓存加载一个场景），注意：在unity编辑器里调试时，异步加载会阻塞主线程，打包后，异步加载不会阻塞主线程，
/// 具体参考： https://forum.unity3d.com/threads/scenemanager-loadsceneasync-not-working-as-expected.369714/ 
/// </summary>
public class CacheSceneManager : MonoBehaviour
{

    public static CacheSceneManager _instance;

    /// <summary>
    /// 预加载场景名称
    /// </summary>
    public string CacheSceneName;
    private AsyncOperation async;
    private bool cacheFinish = false;
    private bool cacheStart = true;
    private bool startLoadScene = false;
    private long startLoadTime = 0;
    private long cacheAfterSeconds = 2;
    public Slider loading_Slider;

    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("Unity SDK  init begin ");
        TalkingDataGA.OnStart("8C8923ADF00E44DEAD602C2A0CB9D456", "TalkingData");
        Debug.Log("Unity SDK  init completed ");
        TDGAAccount.SetAccount(TalkingDataGA.GetDeviceId());
#endif



    }

    private void Start()
    {

        //设置后台加载线程的优
        Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
        cacheFinish = false;
        _instance = this;
        if (CacheSceneName != null)
        {
            CacheSceneManager._instance.cacheScene(CacheSceneName);
        }
    }

    private void cacheScene(string name)
    {
        CacheSceneName = name;
        cacheFinish = false;
        cacheStart = false;
        startLoadScene = false;
        startLoadTime = 0;
        async = null;
    }

    void Update()
    {
        //在脚本加载完成后延迟2秒钟开始缓存，避免和主场景抢占资源，具体时间可根据主场景的大小以及实际情况来设置
        if (!cacheStart)
        {
            if (startLoadTime == 0)
            {
                startLoadTime = getCurrentTimeSeconds();
            }
            if ((getCurrentTimeSeconds() - startLoadTime) > cacheAfterSeconds)
            {
                cacheStart = true;
                Debug.Log(CacheSceneName + " start async cache---------------------->" + getCurrentTimeSeconds());
                async = SceneManager.LoadSceneAsync(CacheSceneName);
                async.allowSceneActivation = false;
            }
        }

        if (async != null)
        {
            //如果加载完成则更新标记
            if (async.progress == 0.9f && !cacheFinish)
            {
                cacheFinish = true;
                Debug.Log(CacheSceneName + " end async cache--------------------------> use Time:" + (getCurrentTimeSeconds() - startLoadTime - cacheAfterSeconds));
                CacheSceneManager._instance.startLoad();
            }
            //加载完成后，检测到切换场景时，加载缓存场景
            if (async.progress == 0.9f && startLoadScene)
            {
                async.allowSceneActivation = true;
            }

        }
        if (loading_Slider.value <= 1f)
        {
            loading_Slider.value += 0.003f;
        }
        else if (loading_Slider.value > 1f)
        {
            return;
        }
    }
   
    //private void LateUpdate()
    //{
    //    if (loading_Slider.value <= 1f)
    //    {
    //        loading_Slider.value += 0.002f;
    //    }
    //    else if (loading_Slider.value > 1f)
    //    {
    //        return;
    //    }

    //}

    //需要加载缓存场景时调用 CacheSceneManager._instance.startLoad(); 
    public void startLoad()
    {
        startLoadScene = true;
    }

    public static long getCurrentTimeSeconds()
    {
        return ConvertDateTimeToInt(System.DateTime.Now) / 1000;

    }

    public static long ConvertDateTimeToInt(System.DateTime time)
    {
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
        long t = (time.Ticks - startTime.Ticks) / 10000;
        return t;
    }

}