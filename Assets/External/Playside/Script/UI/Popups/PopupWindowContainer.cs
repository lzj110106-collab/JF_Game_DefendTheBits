using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public partial class PopupWindowContainer : MonoBehaviour
{
	[Header("Main")]
	public PopupIDs				popupId;					// Which popup this is
	public GameObject 			cancelButton;

	[Header("Title")]
	public Text					titleText;					// Title

	[Header("Message + (optional) Image")]
	public Text					messageText;				// Message
	public Image				largeImage;					// Image next to text

	[Header("Buttons")]
	public Text					dismissButtonText;			// Button's label
}
