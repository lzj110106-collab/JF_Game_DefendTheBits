using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class WavesHUD : MonoBehaviour 
{
	static WavesHUD instance;

	[Header("Behaviour")]
	public bool showCountdownBetweenWaves = true;
	public bool enableFastForwardBetweenWaves = false;
	public bool enableWaveAutoProgression = true;
	public bool resumeFastForwardOnWaveLaunch = false;
	public bool userInputDismissesWaveCompleteMessage = true;
	public bool userInputDismissesWaveCountdown = true;
	public bool showTotalWaveNumber = true;

	[Header("ProgressBar")]
	public ProgressBar progressBar;
	public Text progressBarText;
	public Text progressMaxText;
	public Image waveProgressBarBG;
	public GameObject hordeHierarchy;
	public Color waveBarColor;
	public Color waveBarHordeColor;

	[Header("Complete")]
	public Animator completeAnimator;
	public Text completeWaveNumber;
	public Text completeWaveReward;
	public float completeDuration = 5.0f;

	[Header("Countdown")]
	public Animator countdownAnimator;
	public Text countdownText;
	public int countdownTicks = 10;
	int currentCountdownTicks;

	enum State { None, Complete, Countdown };
	State currentState = State.None;
	float stateTimer = 0.0f;

	int waveNumber;
	int waveReward;
	int maxWaves;


	void Awake() { instance = this; }
	void OnDestroy() { instance = null; }

    void Start()
    {

    }

	void Update()
	{
		if (World.instance == null)
			return;
		
		float speed = World.instance.timeScale;

		stateTimer += Time.deltaTime * speed;
		completeAnimator.speed = speed;
		countdownAnimator.speed = speed;

		if (currentState == State.Complete)
		{
			//let user inputs skip past the wave complete message
			bool userSkip = userInputDismissesWaveCompleteMessage && InputUtil.MousePressed();

			if (stateTimer >= completeDuration || userSkip)
			{
				completeAnimator.SetBool("Show", false);
				HUD.ShowHint(waveNumber + 1);
				FTUE.OnWaveCompleteMessageComplete(waveNumber);

				if (showCountdownBetweenWaves)
				{
					SetState(State.Countdown);

					currentCountdownTicks = 0;
					TriggerCountdownTick();
				}
				else
				{
					if (enableWaveAutoProgression)
						EnemyWaveController.OnWaveCalled();
					
					SetState(State.None);
				}
			}
		}
		else if (currentState == State.Countdown)
		{
			if (stateTimer >= 1.0f)
			{
				currentCountdownTicks += 1;
				stateTimer = 0.0f;
				TriggerCountdownTick();
			}

			//if we have hit the last tick, stop updating and let the animation
			//play out. also force the next wave to trigger
			bool userSkip = userInputDismissesWaveCountdown && InputUtil.MousePressed();

			if (currentCountdownTicks >= countdownTicks || userSkip)
			{
				if (enableWaveAutoProgression)
					EnemyWaveController.OnWaveCalled();
				
				SetState(State.None);
			}
		}
	}

	void SetState(State state)
	{
		currentState = state;
		stateTimer = 0.0f;
	}
		
	void TriggerCountdownTick()
	{
		int remaining = countdownTicks - currentCountdownTicks;
		countdownText.text = remaining == 0 ? LocManager.Translate("ui_start") : remaining.ToString();

		countdownAnimator.enabled = true;
		countdownAnimator.Play("Tick", 0, 0.0f);
	}

	public static void OnWaveLaunched()
	{
		//interrupt any inprogess animations for complete and countdown
		if (instance.currentState != State.None)
		{
			instance.countdownAnimator.SetBool("Show", false);
			instance.SetState(State.None);
		}
	}
		
	public static void OnWaveComplete(int waveNumber, int waveReward) 
	{
		//TODO: localisation
		//instance.completeWaveNumber.text = "WAVE " + (waveNumber + 1).ToString() + " COMPLETE!";
		instance.completeWaveReward.text = waveReward.ToString();
		instance.completeAnimator.Play("Show", 0, 0.0f);
		instance.waveNumber = waveNumber;
		instance.waveReward = waveReward;

		instance.SetState(State.Complete);
		AudioController.Play ("UI_EndOfWave");

	}

	// Called from WaveComplete animator when coins reach top corner
	// Moved from HUD.cs OnWaveComplete to add anim delay
	public void AddWaveBonusCoins()
	{
		HUD.instance.AddGold(instance.waveReward);
	}

	public static void SetWaveProgress(int waveNumber, float percent, bool isHorde)
	{
		//convert from 0-indexing to 1-indexing for display. build this string ourselves
		//because there isn't a LocManager thing for replacing {number} with X/Y
		if (instance.showTotalWaveNumber)
		{
			var wavesText = LocManager.Translate("ui_wave_number");
			var wavesProgress = (waveNumber + 1).ToString() + "/" + instance.maxWaves.ToString();
			wavesText = wavesText.Replace("{number0}", wavesProgress);
			instance.progressBarText.text = wavesText;
		}
		else
		{
			instance.progressBarText.text = LocManager.BuildString("ui_wave_number", waveNumber + 1);
		}
			
		instance.progressBar.SetMaxValue(1.0f);
		instance.progressBar.SetValue(percent, true);

		instance.hordeHierarchy.SetActive(isHorde);
		instance.waveProgressBarBG.color = (isHorde ? instance.waveBarHordeColor : instance.waveBarColor);
	}

	public static void SetMaxWaves(int maxWaves)
	{
		instance.maxWaves = maxWaves;
	}

	public static void Reset()
	{
		SetWaveProgress(0, 0.0f, false);

		//instance.completeAnimator.Play("Hide", 0, 0.0f);
		instance.countdownAnimator.SetBool("Show", false);
		instance.SetState(State.None);
	}
}