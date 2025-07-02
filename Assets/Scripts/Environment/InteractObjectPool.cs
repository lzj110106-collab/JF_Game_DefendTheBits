using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System;
using UnityEngine.Networking;
using System.Text;

public class InteractObjectPool : MonoBehaviour 
{
	static InteractObjectPool instance;

	public GenericPrefabPool source;

	void Awake()
	{
		instance = this;
		source.Initialise("InteractObjectPool");
	}

	void OnDestroy()
	{
		instance = null;
	}

	public static GameObject Get(GameObject prefab)
	{
		return instance.source.Get(prefab);
	}

	public static GameObject Get(GameObject prefab, GameObject attachTo)
	{
		return instance.source.Get(prefab, attachTo);
	}

	public static void Return(GameObject prefab, GameObject prefabInstance)
	{
		instance.source.Return(prefab, prefabInstance);
	}

	public static void Reset()
	{
		instance.source.ResetPool();
	}

#if UNITY_EDITOR
    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.F3))
        //{
        //    Debug.Log("input f3!!");
        //    StartCoroutine(east2westChcekRecepit());
        //}
    }
    IEnumerator east2westChcekRecepit()
    {
        //UnityWebRequest www = UnityWebRequest.Put("http://params.east2west.cn/defendthebits/IOSValidate.php", jsonData.ToJson());
        //www.method = UnityWebRequest.kHttpVerbPOST;
        //yield return www.Send();
        //Debug.LogError(string.Format("iserr:{0}  err:{2}   redata: {1}", www.isError, www.downloadHandler.text, www.error));

        WWWForm wwwForm = new WWWForm();
        wwwForm.AddField("data", Convert.ToBase64String(Encoding.Default.GetBytes(getPurchaseReceiptData())));
        WWW www = new WWW("http://param.east2west.cn/defendthebits/IOSValidate.php", wwwForm);
        yield return www;

        Debug.Log("re:" + www.text);
        // if (www.isError)
        if (!string.IsNullOrEmpty( www.error))
        {
            //Debug.Log("re err!!" + www.responseCode);
            Debug.Log("re err!!" + www.error);
        }
        else
        {
            //JsonData json = JsonMapper.ToObject(www.downloadHandler.text);
            JsonData json = JsonMapper.ToObject(www.text);
            if (json != null && !string.IsNullOrEmpty(json["error"].ToString()))
                switch (int.Parse(json["error"].ToString()))
                {
                    case 0:
                        //sucess
                        Debug.Log("code:" + 0);
                        break;
                    case -1:
                    case -2:
                    default:
                        //fail
                        Debug.Log("code:" + int.Parse(json["error"].ToString()));
                        break;
                }
        }
    }
    string getPurchaseReceiptData()
    {
        JsonData jsonReceipt = new JsonData();
        jsonReceipt["receipt"] = "ewoJInNpZ25hdHVyZSIgPSAiQTZGbHpVZmJ4eVpINHRycW9lYmorN2tYekxTSmhzbzJJMGxDYVZkMlpmRXAraFBJQ3NYZHVHcjNSQ0lvN1QwdHlidk15SkMrRDdMYVlNaitIeHM3aGlYRkJaVlVSUldjNlpkNUhhZUxwbzR5b2JtSVp2czZrVklBWW1OWUdzNzVuc2xJTXpPNGZmeWIrSUhrajRkVkI0MUVmV0E5aDFpUWx6dTl0azFkeEtiUmpvSlFWeWZvS0tSd2M1eW11RWdnSUg3MkxDQkE2RmJxNHcyWFNYVGszazZYR3A4cjRCZ3MvQWt6ZGFhM29rS1RJV09xTVZSNU9EY3MwaitqNDN2VlZWS21TRnArdXMwVXJac2ZxSG05bENIeDUxZTV6VGp5ODBYWC9RbG9PeTRZMUlTL3NSRTI0TmlFYnBoaEtQRzh5d3NGVU5WV2ZhRWwza1hwVFN6UkFlWUFBQVdBTUlJRmZEQ0NCR1NnQXdJQkFnSUlEdXRYaCtlZUNZMHdEUVlKS29aSWh2Y05BUUVGQlFBd2daWXhDekFKQmdOVkJBWVRBbFZUTVJNd0VRWURWUVFLREFwQmNIQnNaU0JKYm1NdU1Td3dLZ1lEVlFRTERDTkJjSEJzWlNCWGIzSnNaSGRwWkdVZ1JHVjJaV3h2Y0dWeUlGSmxiR0YwYVc5dWN6RkVNRUlHQTFVRUF3dzdRWEJ3YkdVZ1YyOXliR1IzYVdSbElFUmxkbVZzYjNCbGNpQlNaV3hoZEdsdmJuTWdRMlZ5ZEdsbWFXTmhkR2x2YmlCQmRYUm9iM0pwZEhrd0hoY05NVFV4TVRFek1ESXhOVEE1V2hjTk1qTXdNakEzTWpFME9EUTNXakNCaVRFM01EVUdBMVVFQXd3dVRXRmpJRUZ3Y0NCVGRHOXlaU0JoYm1RZ2FWUjFibVZ6SUZOMGIzSmxJRkpsWTJWcGNIUWdVMmxuYm1sdVp6RXNNQ29HQTFVRUN3d2pRWEJ3YkdVZ1YyOXliR1IzYVdSbElFUmxkbVZzYjNCbGNpQlNaV3hoZEdsdmJuTXhFekFSQmdOVkJBb01Da0Z3Y0d4bElFbHVZeTR4Q3pBSkJnTlZCQVlUQWxWVE1JSUJJakFOQmdrcWhraUc5dzBCQVFFRkFBT0NBUThBTUlJQkNnS0NBUUVBcGMrQi9TV2lnVnZXaCswajJqTWNqdUlqd0tYRUpzczl4cC9zU2cxVmh2K2tBdGVYeWpsVWJYMS9zbFFZbmNRc1VuR09aSHVDem9tNlNkWUk1YlNJY2M4L1cwWXV4c1FkdUFPcFdLSUVQaUY0MWR1MzBJNFNqWU5NV3lwb041UEM4cjBleE5LaERFcFlVcXNTNCszZEg1Z1ZrRFV0d3N3U3lvMUlnZmRZZUZScjZJd3hOaDlLQmd4SFZQTTNrTGl5a29sOVg2U0ZTdUhBbk9DNnBMdUNsMlAwSzVQQi9UNXZ5c0gxUEttUFVockFKUXAyRHQ3K21mNy93bXYxVzE2c2MxRkpDRmFKekVPUXpJNkJBdENnbDdaY3NhRnBhWWVRRUdnbUpqbTRIUkJ6c0FwZHhYUFEzM1k3MkMzWmlCN2o3QWZQNG83UTAvb21WWUh2NGdOSkl3SURBUUFCbzRJQjF6Q0NBZE13UHdZSUt3WUJCUVVIQVFFRU16QXhNQzhHQ0NzR0FRVUZCekFCaGlOb2RIUndPaTh2YjJOemNDNWhjSEJzWlM1amIyMHZiMk56Y0RBekxYZDNaSEl3TkRBZEJnTlZIUTRFRmdRVWthU2MvTVIydDUrZ2l2Uk45WTgyWGUwckJJVXdEQVlEVlIwVEFRSC9CQUl3QURBZkJnTlZIU01FR0RBV2dCU0lKeGNKcWJZWVlJdnM2N3IyUjFuRlVsU2p0ekNDQVI0R0ExVWRJQVNDQVJVd2dnRVJNSUlCRFFZS0tvWklodmRqWkFVR0FUQ0IvakNCd3dZSUt3WUJCUVVIQWdJd2diWU1nYk5TWld4cFlXNWpaU0J2YmlCMGFHbHpJR05sY25ScFptbGpZWFJsSUdKNUlHRnVlU0J3WVhKMGVTQmhjM04xYldWeklHRmpZMlZ3ZEdGdVkyVWdiMllnZEdobElIUm9aVzRnWVhCd2JHbGpZV0pzWlNCemRHRnVaR0Z5WkNCMFpYSnRjeUJoYm1RZ1kyOXVaR2wwYVc5dWN5QnZaaUIxYzJVc0lHTmxjblJwWm1sallYUmxJSEJ2YkdsamVTQmhibVFnWTJWeWRHbG1hV05oZEdsdmJpQndjbUZqZEdsalpTQnpkR0YwWlcxbGJuUnpMakEyQmdnckJnRUZCUWNDQVJZcWFIUjBjRG92TDNkM2R5NWhjSEJzWlM1amIyMHZZMlZ5ZEdsbWFXTmhkR1ZoZFhSb2IzSnBkSGt2TUE0R0ExVWREd0VCL3dRRUF3SUhnREFRQmdvcWhraUc5Mk5rQmdzQkJBSUZBREFOQmdrcWhraUc5dzBCQVFVRkFBT0NBUUVBRGFZYjB5NDk0MXNyQjI1Q2xtelQ2SXhETUlKZjRGelJqYjY5RDcwYS9DV1MyNHlGdzRCWjMrUGkxeTRGRkt3TjI3YTQvdncxTG56THJSZHJqbjhmNUhlNXNXZVZ0Qk5lcGhtR2R2aGFJSlhuWTR3UGMvem83Y1lmcnBuNFpVaGNvT0FvT3NBUU55MjVvQVE1SDNPNXlBWDk4dDUvR2lvcWJpc0IvS0FnWE5ucmZTZW1NL2oxbU9DK1JOdXhUR2Y4YmdwUHllSUdxTktYODZlT2ExR2lXb1IxWmRFV0JHTGp3Vi8xQ0tuUGFObVNBTW5CakxQNGpRQmt1bGhnd0h5dmozWEthYmxiS3RZZGFHNllRdlZNcHpjWm04dzdISG9aUS9PamJiOUlZQVlNTnBJcjdONFl0UkhhTFNQUWp2eWdhWndYRzU2QWV6bEhSVEJoTDhjVHFBPT0iOwoJInB1cmNoYXNlLWluZm8iID0gImV3b0pJbTl5YVdkcGJtRnNMWEIxY21Ob1lYTmxMV1JoZEdVdGNITjBJaUE5SUNJeU1ERTRMVEV5TFRJd0lEQXpPakF3T2pReUlFRnRaWEpwWTJFdlRHOXpYMEZ1WjJWc1pYTWlPd29KSW5WdWFYRjFaUzFwWkdWdWRHbG1hV1Z5SWlBOUlDSmxOVGs1TkRjM05ETTNNRFJrTVRKbE5tWXdPV1F3Tmpaall6UmpaakZqTURjeU56QXpaVGN6SWpzS0NTSnZjbWxuYVc1aGJDMTBjbUZ1YzJGamRHbHZiaTFwWkNJZ1BTQWlNVEF3TURBd01EUTRPRGM1TXpBME1pSTdDZ2tpWW5aeWN5SWdQU0FpTVM0eExqRXlJanNLQ1NKMGNtRnVjMkZqZEdsdmJpMXBaQ0lnUFNBaU1UQXdNREF3TURRNE9EYzVNekEwTWlJN0Nna2ljWFZoYm5ScGRIa2lJRDBnSWpFaU93b0pJbTl5YVdkcGJtRnNMWEIxY21Ob1lYTmxMV1JoZEdVdGJYTWlJRDBnSWpFMU5EVXpNRE0yTkRJeE16SWlPd29KSW5WdWFYRjFaUzEyWlc1a2IzSXRhV1JsYm5ScFptbGxjaUlnUFNBaU1Ua3lORGswUkVRdE9EQkdNUzAwTUVFekxUazRRME10UWtWR1JEWkRPRGM1TURVNElqc0tDU0p3Y205a2RXTjBMV2xrSWlBOUlDSmxZWE4wTW5kbGMzUmZkMkZ5Y21sdmNuTXVZM0o1YzNSaGJITXVjRzkxWTJnaU93b0pJbWwwWlcwdGFXUWlJRDBnSWpFek16Z3hOREF5TnpraU93b0pJblpsY25OcGIyNHRaWGgwWlhKdVlXd3RhV1JsYm5ScFptbGxjaUlnUFNBaU1DSTdDZ2tpYVhNdGFXNHRhVzUwY204dGIyWm1aWEl0Y0dWeWFXOWtJaUE5SUNKbVlXeHpaU0k3Q2draWNIVnlZMmhoYzJVdFpHRjBaUzF0Y3lJZ1BTQWlNVFUwTlRNd016WTBNakV6TWlJN0Nna2ljSFZ5WTJoaGMyVXRaR0YwWlNJZ1BTQWlNakF4T0MweE1pMHlNQ0F4TVRvd01EbzBNaUJGZEdNdlIwMVVJanNLQ1NKcGN5MTBjbWxoYkMxd1pYSnBiMlFpSUQwZ0ltWmhiSE5sSWpzS0NTSnZjbWxuYVc1aGJDMXdkWEpqYUdGelpTMWtZWFJsSWlBOUlDSXlNREU0TFRFeUxUSXdJREV4T2pBd09qUXlJRVYwWXk5SFRWUWlPd29KSW1KcFpDSWdQU0FpWTI5dExtVmhjM1F5ZDJWemRDNTNZWEp5YVc5eWN5STdDZ2tpY0hWeVkyaGhjMlV0WkdGMFpTMXdjM1FpSUQwZ0lqSXdNVGd0TVRJdE1qQWdNRE02TURBNk5ESWdRVzFsY21sallTOU1iM05mUVc1blpXeGxjeUk3Q24wPSI7CgkiZW52aXJvbm1lbnQiID0gIlNhbmRib3giOwoJInBvZCIgPSAiMTAwIjsKCSJzaWduaW5nLXN0YXR1cyIgPSAiMCI7Cn0=";
        jsonReceipt["deviceID"] = "test";
        jsonReceipt["paymentID"] = "east2west_warriors.crystals.pouch";
        jsonReceipt["orderID"] = "1000000488793042";
        return jsonReceipt.ToJson();
    }
#endif
}