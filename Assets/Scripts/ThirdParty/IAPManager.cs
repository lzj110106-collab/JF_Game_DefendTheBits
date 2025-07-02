//using UnityEngine;
//using UnityEngine.Purchasing;
//using UnityEngine.Purchasing.Extension;
//using System.Collections;
//using System.Collections.Generic;
//using System;

//public class IAPManager : MonoBehaviour, IStoreListener
//{
//	public class ProductInfo
//	{
//		public int quantity;
//		public ProductType type;
//		public ProductInfo(int a_quantity, ProductType a_type)
//		{
//			quantity = a_quantity; type = a_type; 
//		}
//	}

//	IStoreExtension ext;

//	static Dictionary<string, ProductInfo> database = new Dictionary<string, ProductInfo>();
//	[SerializeField]
//	TextAsset costCSV;

//	static IStoreController storeController = null;
//	static IExtensionProvider storeExtensionProvider = null;

//	static bool isBusy;
//	//Need to pass a string for restore purchases,
//	//calling restore purcahses returns a ProcessPurchase call for each item we recieve.
//	//Having the string paramater will allow us to set a callback before calling RestorePurcahses to handle the products.
//	static Action<string> successCallBack; 
//	static Action<string> failCallBack = null;

//	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
//	{
//		// Overall Purchasing system, configured with products for this application.
//		storeController = controller;
//		// Store specific subsystem, for accessing device-specific store features.
//		storeExtensionProvider = extensions;
//	}

//	public void OnInitializeFailed(InitializationFailureReason error)
//	{
//		// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
//		Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
//	}

//	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
//	{
//		if (successCallBack != null)
//		{
//			successCallBack(args.purchasedProduct.definition.id);
//		}
//		isBusy = false;
//		AudioController.Play("UI_Purchase");
//		return PurchaseProcessingResult.Complete;
//	}


//	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
//	{
//		if (failCallBack != null)
//		{
//			failCallBack(failureReason.ToString());
//		}
//		isBusy = false;
//		// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
//		// this reason with the user to guide their troubleshooting actions.
//		Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
//		AudioController.Play ("Fail");
//	}

//	void Start()
//	{
//		//InitProducts();
//	}

//	public void ReadProductsFromCSV()
//	{
//        string csv = costCSV.text.Replace("\r\n", "\n");
//        string[] lines = csv.Split('\n');
        
//        for (int rowNo = 1; rowNo < lines.Length; ++rowNo)
//        {
//			int itter = 0;

//            string line = lines[rowNo];
//            if (string.IsNullOrEmpty(line.Trim()))
//                continue;

//            // Fix whitespace issues
//            string[] data = line.Trim().Split(',');
//            for (int i = 0; i < data.Length; ++i)
//                data[i] = data[i].Trim();


//            // Skip empty lines & comments
//            if (data.Length == 0 || data[0] == "")
//                continue;
//            else if (data.Length != 3)
//            {
//                Debug.LogError("CSV row " + rowNo + ": Incorrect number of columns (has " + data.Length + ", expected 3");
//                continue;
//            }

//			string identifier = data[itter++];
//			int quantity = int.Parse(data[itter++]);
//			ProductType myType = (ProductType)Enum.Parse(typeof(ProductType), data[itter++]);

//			if (database.ContainsKey(identifier))
//				Debug.LogError("[IAPCosts] duplicate productID: " + identifier);
//            else
//                database.Add(identifier, new ProductInfo(quantity, myType));
//        }
//	}

//    //zbs 20180728 屏蔽支付相关
//	//public void InitProducts()
//	//{
//	//	if (storeController == null)
//	//	{
//	//		// If we have already connected to Purchasing ...
//	//		if (IsInitialized())
//	//		{
//	//			// ... we are done here.
//	//			return;
//	//		}

//	//		ReadProductsFromCSV();
//	//		// Create a builder, first passing in a suite of Unity provided stores.
//	//		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

//	//		foreach (var kv in database)
//	//		{
//	//			builder.AddProduct(kv.Key, kv.Value.type);
//	//		}
//	//		// Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
//	//		// and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
//	//		UnityPurchasing.Initialize(this, builder);

//	//	}
//	//}

//	public static bool IsInitialized()
//	{
//		return /*!Application.isEditor && */storeController != null && storeExtensionProvider != null;
//	}

//	/// <summary> Buys the product </summary>
//	/// <param name="productId"> Product's key on the store </param>
//	/// <param name="callbackSuccess"> Function to call if purchase was successful (The returned string will be our product ID </param>
//	/// <param name="callbackFailed"> Function to call if failed </param>
//	public static void BuyProduct(string productId, Action<string> callbackSuccess, Action<string> callbackFailed)
//	{
//		if (isBusy)
//			return;

//#if !UNITY_Debug
//		if (IsInitialized())
//		{
//			Product product = storeController.products.WithID(productId);
 
//			if (product != null && product.availableToPurchase)
//			{
//				successCallBack = callbackSuccess;
//				failCallBack = callbackFailed;
//				isBusy = true;
//				storeController.InitiatePurchase(product);
//			}
//			else
//			{
//                //print("支付成功");
//                callbackFailed("Product purchase not sent. Product unavailable");
//			}
//		}
//		else
//		{
//            //print("支付成功");
//            callbackFailed("Store not initialized!");
//		}
//#else
//		callbackSuccess(productId);
//		AudioController.Play ("UI_Purchase");
//#endif
//	}

//	/// <summary> Restores all purchases </summary>
//	/// <param name="callback"> Callback for each product </param>
//	public static void RestoreTransactions(Action<string> callback)
//	{
//		if (isBusy)
//		{
//			return;
//		}

//		// If Purchasing has not yet been set up ...
//		if (!IsInitialized())
//		{
//			// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
//			Debug.Log("RestorePurchases FAIL. In App Purchasing has not been initialized");
//			return;
//		}

//		// If we are running on an Apple device ... 
//		if (Application.platform == RuntimePlatform.IPhonePlayer ||
//			Application.platform == RuntimePlatform.OSXPlayer)
//		{
//			// ... begin restoring purchases
//			Debug.Log("RestorePurchases started ...");

//			successCallBack = callback;

//            //zbs 20180728 屏蔽支付相关
////#if UNITY_IOS
////			// Fetch the Apple store-specific subsystem.
////			var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
////			// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
////			// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
////			apple.RestoreTransactions((result) =>
////			{
////				isBusy = result;
////				// The first phase of restoration. If no more responses are received on ProcessPurchase then 
////				// no purchases are available to be restored.
////				Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
////			});
////#endif
//		}
//	}

//	/// <summary> Gets the cost of the item as a formatted string </summary>
//	/// <param name="productId"> Product's key on the store </param>
//	/// <returns> The cost as formatted text </returns>
//	public static string GetCostText(string productId)
//	{
//		//Debug.Log("[IAPManager] GetCostText(" + productId + ") requested");

////#if UNITY_EDITOR
//		return "$0.00";
////#else
		
//		if (!IsInitialized())
//			return string.Empty;

//		Product info = storeController.products.WithID(productId);
//		if (info != null && info.metadata != null)
//			return info.metadata.localizedPriceString;
//		else
//			return string.Empty;
////#endif
//	}

//	/// <summary> Gets the currency of the item as a formatted string </summary>
//	/// <param name="productId"> Product's key on the store </param>
//	/// <returns> The currency as formatted text </returns>
//	public static string GetCurrencyCode(string productId)
//	{
//		Debug.Log("[IAPManager] GetCostText(" + productId + ") requested");

//#if UNITY_EDITOR
//		return "USD";
//#else
//		if (!IsInitialized())
//			return string.Empty;

//		Product info = storeController.products.WithID(productId);

//		if (info != null)
//			return info.metadata.isoCurrencyCode;
//		else
//			return string.Empty;
//#endif
//	}

//	/// <summary> Returns the quantity parameter for a product </summary>
//	/// <param name="productId"> Product's key on the store </param>
//	/// <returns>The product quantity.</returns>
//	public static int GetProductQuantity(string productId)
//	{		
//		ProductInfo info = database[productId];
//		if (info != null)
//			return info.quantity;
//		else
//			return 0;
//	}

//	public static Product GetProduct(string productID)
//	{
//		if (!IsInitialized())
//			return null;

//		Product info = storeController.products.WithID(productID);
//		if (info != null)
//		{
//			return info;
//		}
//		return null;
//	}
//}