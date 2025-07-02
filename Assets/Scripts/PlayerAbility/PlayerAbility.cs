using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAbility : MonoBehaviour 
{
	public bool inProgress { get; protected set; }

	public abstract int CashRequired();
	public abstract string RangePlotName();

	public abstract bool Trigger(int tileX, int tileY);
	public abstract void UpdateTick();

	public abstract void Restart();

	public virtual bool IsValidTarget(int tileX, int tileY)
	{
		//if any of the range plots overlap a path tile, then this
		//is a valid location to trigger the ability
		var plotData = RangePlots.GetPlotData(RangePlotName(), 0);
		if (plotData != null)
		{
			for (int i = 0; i < plotData.Count; ++i)
			{
				int x = (int)(tileX + plotData[i].x);
				int y = (int)(tileY + plotData[i].y);

				if (Landscape.instance.HasFlag(x, y, TileFlag.HasPath_RuntimeAssigned))
					return true;
			}
		}

		return false;
	}

	void Start()
	{
		inProgress = false;
	}
}
