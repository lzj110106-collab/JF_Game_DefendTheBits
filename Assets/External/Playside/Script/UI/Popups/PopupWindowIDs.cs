using UnityEngine;
using System.Collections;

public partial class PopupWindowContainer : MonoBehaviour
{
	public enum PopupIDs
	{
		Default,
		LargeMessage,
		LargeMessageWithImage,
		SmallMessage,
		SmallMessageWithImage,
		Build_SkipWithPremium,
		Build_NeedMoreBuilders,
		Fight_NeedMoreEnergy,
		Build_NeedMoreCurrency,
		OCPUpgrade,
		BuildingUpgrade,
		Purchase
	}
}
