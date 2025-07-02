using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface WorldObject
{
	//returns true if the object is still alive
	bool UpdateTick();
	void UpdateTicksComplete();

	//timing overrides
	//TODO: delete these, this is all implicit in World.Update
	void OnPause(bool pause);
	void OnTimeScaleAdjusted(float timeScale);

	//everything needs to react to characters dying for
	//targetting changes and that kind of thing
	void OnCharacterKilled(Character c);

	//chance for world objects to clean up any state before
	//the world objects lists are cleared.
	void OnWorldReset();
}
