using UnityEngine;
using UnityEditor;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class DeletePlayerPrefs : Editor {
	[MenuItem("PlayerPrefs/DeleteAll")]
	public static void DeleteAllPrefs()
	{
		//PlayerPrefs.DeleteAll ();
        ObscuredPrefs.DeleteAll();
    }


    [MenuItem("PlayerPrefs/SaveYesterday")]
    public static void SaveYesterday()
    {
        ObscuredPrefs.SetString("LastClamTime", "2018-9-28");
    }

    [MenuItem("PlayerPrefs/RecoveryGiftbag")]
    public static void RecoveryGiftbag()
    {
        ObscuredPrefs.DeleteKey("gift0");
        ObscuredPrefs.DeleteKey("gift1");
        ObscuredPrefs.DeleteKey("gift2");
    }

    [MenuItem("PlayerPrefs/NextGiftbagTime")]
    public static void NextGiftbagTime()
    {
        // SaveData.SetNextGiftbagTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day + 1,System. DateTime.Now.Hour, System.DateTime.Now.Minute, System.DateTime.Now.Second));
        SaveData.SetNextGiftbagTime(System.DateTime.Now.AddDays(1).ToString());
    }
}
