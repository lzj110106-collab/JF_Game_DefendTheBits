using UnityEngine;
using System.Collections;

public class TriggerScreenCover : MonoBehaviour {

	public ScreenCoverIDs targetId;
	public Color coverColor = Color.white;

	public void FadeOffBlack (float duration) { ScreenCover.Instances[(int)targetId].FadeCoverOff(duration, Color.black); }
	public void FadeOffWhite (float duration) { ScreenCover.Instances[(int)targetId].FadeCoverOff(duration, Color.white); }
	public void FadeOffColor (float duration) { ScreenCover.Instances[(int)targetId].FadeCoverOff(duration, coverColor); }

	public void FadeOnBlack (float duration) { ScreenCover.Instances[(int)targetId].FadeCoverOn(duration, Color.black); }
	public void FadeOnWhite (float duration) { ScreenCover.Instances[(int)targetId].FadeCoverOn(duration, Color.white); }
	public void FadeOnColor (float duration) { ScreenCover.Instances[(int)targetId].FadeCoverOn(duration, coverColor); }

	// Flash
	public void FadeFlashBlack (float duration) { ScreenCover.Instances[(int)targetId].Flash(duration, Color.black); }
	public void FadeFlashWhite (float duration) { ScreenCover.Instances[(int)targetId].Flash(duration, Color.white); }
	public void FadeFlashColor (float duration) { ScreenCover.Instances[(int)targetId].Flash(duration, coverColor); }
}
