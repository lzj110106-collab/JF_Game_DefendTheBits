using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeedSliderControl : MonoBehaviour {

	public static SpeedSliderControl Instance;

	public enum SpeedTypes {Normal, Fast, Fastest}
	
	public Color defaultColor;
	[System.Serializable]
	public class SpeedInfo
	{
		public SpeedTypes speed = SpeedTypes.Normal;
		public float speedMultiplier = 1.0f;
		public Color tintColor = Color.white;
		public Graphic[] tintTargets;
		public RectTransform textTransform;
	}
	public SpeedInfo[] speedInfos;
	public Slider speedSlider;
	public Vector3 textUpscale;

	public SpeedTypes currentSpeed { get; private set;}

	void Awake()
	{
		Instance = this;
	}

	public void SetSpeed (int id)
	{
		SpeedTypes newSpeed = (SpeedTypes)id;

		if(currentSpeed == newSpeed)
			return;
		else
		{
			currentSpeed = newSpeed;
			if(World.instance != null)
			{
				World.instance.SetTimescale(speedInfos[id].speedMultiplier);
				speedSlider.value = id;
				switch(currentSpeed)
				{
					case SpeedTypes.Normal:
						foreach(Graphic g in speedInfos[1].tintTargets)
							g.color = defaultColor;
						foreach(Graphic g in speedInfos[2].tintTargets)
							g.color = defaultColor;

						speedInfos[0].textTransform.localScale = textUpscale;
						speedInfos[1].textTransform.localScale = Vector3.one;
						speedInfos[2].textTransform.localScale = Vector3.one;
						break;

					case SpeedTypes.Fast:
						foreach(Graphic g in speedInfos[0].tintTargets)
							g.color = defaultColor;
						foreach(Graphic g in speedInfos[2].tintTargets)
							g.color = defaultColor;
							speedInfos[0].textTransform.localScale = Vector3.one;
							speedInfos[1].textTransform.localScale = textUpscale;
							speedInfos[2].textTransform.localScale = Vector3.one;
						break;

					case SpeedTypes.Fastest:
						foreach(Graphic g in speedInfos[0].tintTargets)
							g.color = defaultColor;
						foreach(Graphic g in speedInfos[1].tintTargets)
							g.color = defaultColor;

							speedInfos[0].textTransform.localScale = Vector3.one;
							speedInfos[1].textTransform.localScale = Vector3.one;
							speedInfos[2].textTransform.localScale = textUpscale;
						break;
				}
				foreach(Graphic g in speedInfos[id].tintTargets)
					g.color = speedInfos[id].tintColor;
			}
		}
	}

	public void GetSpeedFromSlider()
	{
		SetSpeed((int)speedSlider.value);
	}

	public void ResetSpeed()
	{
		if(World.instance != null)
			World.instance.SetTimescale(1f);
	}
}
