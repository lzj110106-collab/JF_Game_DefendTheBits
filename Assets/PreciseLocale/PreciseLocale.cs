using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class PreciseLocale : System.Object {

	public static string GetRegion() {
		#if UNITY_ANDROID && !UNITY_EDITOR
		return PreciseLocaleAndroid.GetRegion();                           
		#elif UNITY_IOS && !UNITY_EDITOR
		return PreciseLocaleiOS.GetRegion(); 
		#endif
		return "US";
	}

	public static string GetLanguageID() {
        
#if UNITY_ANDROID && !UNITY_EDITOR
		return PreciseLocaleAndroid.GetLanguageID();                           
#elif UNITY_IOS && !UNITY_EDITOR
        Debug.Log(PreciseLocaleiOS.GetLanguageID());
		return PreciseLocaleiOS.GetLanguageID(); 
#endif
        return "en_US";
	}

	public static string GetLanguage() {
		#if UNITY_ANDROID && !UNITY_EDITOR
		return PreciseLocaleAndroid.GetLanguage();                           
		#elif UNITY_IOS && !UNITY_EDITOR
		return PreciseLocaleiOS.GetLanguage(); 
		#endif
		return "en";
	}

	public static string GetCurrencyCode() {
		#if UNITY_ANDROID && !UNITY_EDITOR
		return PreciseLocaleAndroid.GetCurrencyCode();                           
		#elif UNITY_IOS && !UNITY_EDITOR
		return PreciseLocaleiOS.GetCurrencyCode(); 
		#endif
		return "USD";
	}

	public static string GetCurrencySymbol() {
		#if UNITY_ANDROID && !UNITY_EDITOR
		return PreciseLocaleAndroid.GetCurrencySymbol();                           
		#elif UNITY_IOS && !UNITY_EDITOR
		return PreciseLocaleiOS.GetCurrencySymbol(); 
		#endif
		return "$";
	}

	#if UNITY_ANDROID && !UNITY_EDITOR
	private class PreciseLocaleAndroid {
		private static AndroidJavaClass _preciseLocale = new AndroidJavaClass("com.kokosoft.preciselocale.PreciseLocale");

		public static string GetRegion() {
			return _preciseLocale.CallStatic<string>("getRegion");                                 
		}

		public static string GetLanguage() {
			return _preciseLocale.CallStatic<string>("getLanguage");                                 
		}

		public static string GetLanguageID() {
			return _preciseLocale.CallStatic<string>("getLanguageID");                                 
		}

		public static string GetCurrencyCode() {
			return _preciseLocale.CallStatic<string>("getCurrencyCode");                                 
		}

		public static string GetCurrencySymbol() {
			return _preciseLocale.CallStatic<string>("getCurrencySymbol");                                 
		}

	}
	#endif

	#if UNITY_IPHONE && !UNITY_EDITOR
	private class PreciseLocaleiOS {

		[DllImport ("__Internal")]
		private static extern string _getRegion();

		[DllImport ("__Internal")]
		private static extern string _getLanguageID();

		[DllImport ("__Internal")]
		private static extern string _getLanguage();

		[DllImport ("__Internal")]
		private static extern string _getCurrencyCode();

		[DllImport ("__Internal")]
		private static extern string _getCurrencySymbol();

		public static string GetRegion() {
			return _getRegion();
		}

		public static string GetLanguage() {
			return _getLanguage();
		}

		public static string GetLanguageID() {
			return _getLanguageID();
		}

		public static string GetCurrencyCode() {
			return _getCurrencyCode();
		}

		public static string GetCurrencySymbol() {
			return _getCurrencySymbol();
		}

	}
	#endif
}