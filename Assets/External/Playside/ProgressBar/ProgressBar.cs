using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProgressBar : MonoBehaviour {

	public bool startFull;
	public bool isFull;

	public enum Transition {None, Tint, Animation}
	public Transition transition;

	public enum FillType {Linear, Smooth, Exponential}
	public FillType fillType;
	public float fillRate = 1.0f;
	public float flashTime = 0.25f;

	public enum LabelType {None, Whole, Decimal, Percent}
	public LabelType labelType;
	public bool valueOfTotal;

	// Elements
	public RectTransform fillContainer;
	public Text progressLabel;

	public float currentWidth = 100f;
	public float maxWidth = 100f;

	// Tint
	public Graphic targetGraphic;
	public Color normalColor = Color.white;
	public Color addColor = Color.blue;
	public Color removeColor = Color.red;
	public Color filledColor = Color.green;
	private Color targetColor;

	// Animation
	private Animator anim;
	public string normalTrigger = "Normal";
	public string addTrigger = "Add";
	public string removeTrigger = "Remove";
	public string filledTrigger = "Filled";
	public string emptyTrigger = "Empty";

	// Progress Bar
	private bool isUpdating = false;
	public float value = 0f;
	public float maxValue = 100f;
	public float lagValue = 0f;
	private float lastValue = 0f;
	private float targetValue = 0f;
	private float fillVelocity;
	private float colorTimer;
	private Vector2 fillPos;
	private Vector2 fillSizeDelta;

	void Awake()
	{
		Initialize();
	}
	
	private void Initialize()
	{
		if(transition == Transition.Animation)
			anim = GetComponent<Animator>();

		//Force Fill RectTransform to Anchor Mid Left (only supports left to right fills)
		maxWidth = fillContainer.sizeDelta.x;
		fillPos = fillContainer.anchoredPosition;
		fillSizeDelta = fillContainer.sizeDelta;
		fillContainer.pivot = new Vector2(0, 0.5f);
		fillContainer.anchorMin = new Vector2(0, 0.5f);
		fillContainer.anchorMax = new Vector2(0, 0.5f);

		// Debug max value
		maxValue = 100f;
		value = (startFull)? maxValue : 0;
		isUpdating = false;
		UpdateProgressBar();
	}

	// Add Amount to bar
	public void Add(float amount)
	{
		if(value >= maxValue)
			return;
		targetValue = Mathf.Clamp(targetValue+amount, 0, maxValue);

		if(!isUpdating)
			StartCoroutine("UpdateProgress");

        if (transition==Transition.Tint)
        {
        	colorTimer = flashTime;
        	targetColor = addColor;
        }

        if (transition==Transition.Animation)
        {
			anim.SetTrigger(addTrigger);
   		}

		if(value>maxValue)
		{
			value = maxValue;
			Filled();
		}
	}

	// Remove Amount from bar
	public void Remove(float amount)
	{
		if(value <= 0)
			return;
		targetValue = Mathf.Clamp(targetValue-amount, 0, maxValue);

		if(!isUpdating)
			StartCoroutine("UpdateProgress");

        switch (transition)
        {
        	// NONE
        	case Transition.None:
        	break;

        	// TINT
        	case Transition.Tint:
        		colorTimer = flashTime;
        		targetColor = removeColor;
        	break;

        	// ANIMATION
        	case Transition.Animation:
	        	anim.SetTrigger(removeTrigger);
        	break;
        }
	}

	// Bar has been filled
	private void Filled()
	{
		value = maxValue;

    	// TINT
    	if(transition == Transition.Tint)
    	{
    		targetGraphic.color = filledColor;
    	}

    	// ANIMATION
    	if(transition == Transition.Animation)
    	{
        	anim.SetTrigger(filledTrigger);
    	}
	}

	// Bar has been emptied
	private void Empty()
	{
    	if(transition == Transition.Animation)
    	{
        	anim.SetTrigger(emptyTrigger);
    	}
	}

	private IEnumerator UpdateProgress()
	{
		lastValue = value;
		isUpdating = true;

		while (isUpdating)
		{
			// Increment VALUE
	        switch (fillType)
	        {
	        	// LINEAR
	        	case FillType.Linear:
	        		value = Mathf.MoveTowards(value, targetValue, fillRate*Time.fixedDeltaTime);
	        	break;

	        	// SMOOTH
	        	case FillType.Smooth:
	        		value = Mathf.SmoothDamp(value, targetValue, ref fillVelocity, Time.fixedDeltaTime/fillRate);
	        	break;

	        	// EXPONENTIAL
	        	case FillType.Exponential:
	        		value = Mathf.MoveTowards(value, targetValue, fillRate/Mathf.Abs(targetValue-value)*Time.fixedDeltaTime);
	        	break;
	        }

	        // TINT
	        if (transition == Transition.Tint)
	        {
	        	//Wait for colorTimer
	        	if(colorTimer > 0)
	        	{
	        		targetGraphic.color = Color.Lerp(normalColor, targetColor, colorTimer);
	        		colorTimer = Mathf.Clamp01(colorTimer-Time.fixedDeltaTime);
				}
				else if(Mathf.Abs(targetValue-value) <= 0.5f)
				{
					value = targetValue;
					isUpdating = false;
				}
	        }
	        else if(Mathf.Abs(targetValue-value) <= 0.5f)
			{
				value = targetValue;
				isUpdating = false;
			}

			UpdateProgressBar();
			yield return new WaitForFixedUpdate();
		}

		if(value == maxValue)
			Filled();
		yield return null;	
	}

	private void UpdateProgressBar()
	{
		// BAR FILL
		fillSizeDelta.x = Mathf.Clamp(value/maxValue * maxWidth, 0f, maxWidth);

		fillContainer.sizeDelta = fillSizeDelta;
		fillContainer.anchoredPosition = fillPos;

		// PROGRESS COUNTER
		if(progressLabel != null)
		{
	        switch (labelType)
	        {
	        	// NONE
	        	case LabelType.None:
	        		progressLabel.text = "";
	        	break;

	        	// WHOLE
	        	case LabelType.Whole:
	        		progressLabel.text = (int)value + ( valueOfTotal ? ("/" +  (int)maxValue) : "");
	        	break;

	        	// DECIMAL
	        	case LabelType.Decimal:
	        		progressLabel.text = value.ToString("0.0") + ( valueOfTotal ? ("/" + maxValue.ToString("0.0")) : "");
	        	break;

	        	// PERCENT
	        	case LabelType.Percent:
	        		progressLabel.text = (value/maxValue*100f).ToString("0.0") + "%";
	        	break;
	        }
	    }
	    Canvas.ForceUpdateCanvases();
	}

	public void SetValue(float newValue, bool instant=false)
	{
		if(instant)
		{
			value = newValue;
			UpdateProgressBar();
		}
		else
		{
			if(newValue > targetValue)
				Add(newValue -targetValue);
			else if (newValue < targetValue)
				Remove(targetValue - newValue);
		}
	}

	public void SetMaxValue(float newValue)
	{
		maxValue = newValue;
		UpdateProgressBar();
	}
}
