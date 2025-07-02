using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GiftBag : MonoBehaviour {

    public Text oldPriceText;
    public Text newPriceText;

    public int oldPrice;
    public int newPrice;
    void OnEnable()
    {
        oldPriceText.text=LocManager.Translate("ui_originalcost") + LocManager.GetCurrencySymbol() + LocManager.GetTargetPrice(oldPrice);
        //if (PurchaseManager.priceDir.Count > 0&&PurchaseManager.priceDir.ContainsKey(newPrice.ToString()))
        //    newPriceText.text = PurchaseManager.priceDir[newPrice.ToString()];
        //else
        //newPriceText.text = LocManager.GetCurrencySymbol() + LocManager.GetTargetPrice(newPrice);
        newPriceText.text = "￥" + newPrice.ToString();
    }
}
