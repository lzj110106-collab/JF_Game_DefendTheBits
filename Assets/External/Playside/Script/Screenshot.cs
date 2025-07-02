using UnityEngine;
using System.Collections;

public class Screenshot : MonoBehaviour {
    [SerializeField] KeyCode key = KeyCode.S;
    [SerializeField] [Range(0, 80)] int sizeMultiplier = 4;
	void Update() {
		if (Input.GetKeyDown (key))
        {
            string path = "Screenshot_";
            string date = System.DateTime.Now.ToShortDateString().Replace("/", "");
            string time = System.DateTime.Now.ToLongTimeString().Replace(":", "");
            time = time.Remove(time.IndexOf(" "));
            path = path + date + "_" + time + ".png";

            Debug.Log ("Screenshot saving to: " + path);

			//ScreenCapture.CaptureScreenshot (path, sizeMultiplier);
		}
	}
}