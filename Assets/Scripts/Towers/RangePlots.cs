using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum RangePlotNames
{
	NONE,
	RNG1,
	RNG2,
	RNG3,
	RNG4,
	RNG5,
	RNG6,
	RNG7,
	NUM_ELEMEMTS,

}

class csvData
{
	public csvData(int x, int y, int a_val)
	{
		xPos = x;
		yPos = y;
		val = a_val;
	}

	public int xPos;
	public int yPos;
	public int val;
}

class PlotData
{
	List<List<Vector2>> data = new List<List<Vector2>>();
	public int range { get; private set; }

	public PlotData(List<Vector2> baseRotation)
	{
		data.Add(baseRotation);

		//creating the other three rotations.
		for (var i = 1; i < 4; ++i)
		{
			var prev = data[i - 1];
			var result = new List<Vector2>(prev.Count);

			//easy way to do this is to just store the perpendicular
			//of the previous rotation entries.
			for (var j = 0; j < prev.Count; ++j)
				result.Add(new Vector2(prev[j].y, -prev[j].x));

			//done
			data.Add(result);
		}

		//calculate the range. only required for UI purposes
		//store manhattan distance max because everything
		//is grid based, rather than circular ranges
		range = 0;
		for (int i = 0; i < data[0].Count; ++i)
		{
			range = (int)Mathf.Max(range, Mathf.Abs(data[0][i].x));
			range = (int)Mathf.Max(range, Mathf.Abs(data[0][i].y));
		}
	}

	public List<Vector2> GetData(int rotation)
	{
		return data[rotation % 4];
	}
}

public class RangePlots : MonoBehaviour 
{
	static RangePlots instance;

	public TextAsset RangeCSV;

	Dictionary<string, PlotData> plots = new Dictionary<string, PlotData>();

	void Awake()
	{
		Debug.Assert(!instance);
		instance = this;

		Load();
	}

	void OnDestroy()
	{
		Debug.Assert(instance == this);
		instance = null;
	}
		
	public void Load()
	{
		string csv = RangeCSV.text.Replace ("\r\n", "\n");
		string[] lines = csv.Split ('\n');

		for (int rowNo = 0; rowNo < lines.Length; ++rowNo) 
		{
			string line = lines [rowNo];
			if (string.IsNullOrEmpty (line.Trim ()))
				continue;
				
			if (!String.IsNullOrEmpty(line.Trim(',').Trim()))
			{
				string plotName = line.Trim (',').Trim ();

				List<List<csvData>> matrix = new List<List<csvData>> ();

				int playerX = 0;
				int playerY = 0;

				while (!String.IsNullOrEmpty (line))
				{
					rowNo++;
					line = lines [rowNo].Trim ();

					if (String.IsNullOrEmpty (line))
						break;

					string[] rowData = line.Trim ().Split (',');

					if (String.IsNullOrEmpty (rowData [0]))
						break;

					matrix.Add (new List<csvData> ());



					for (int x = 0; x < rowData.Length; x++)
					{
						if (!rowData [x].Equals ("1") && !rowData [x].Equals ("2") && !rowData [x].Equals ("0"))
						{
							break;
						}
							

						if (int.Parse (rowData [x]) == 1)
						{
							matrix [matrix.Count - 1].Add (new csvData (x, matrix.Count - 1, int.Parse (rowData [x])));
						}

						if (int.Parse (rowData [x]) == 2)
						{
							playerX = x;
							playerY = matrix.Count - 1;
						}
					}
				}
				
				List<Vector2> vals = new List<Vector2> ();
				for (int y = 0; y < matrix.Count; y++)
				{
					for (int x = 0; x < matrix [y].Count; x++)
					{
						vals.Add(new Vector2(matrix[y][x].xPos - playerX, 
											 matrix[y][x].yPos - playerY));
					}
				}
					
				plots.Add(plotName, new PlotData(vals));
			}
		}
	}

	public static List<Vector2> GetPlotData(string name, int rotationIndex)
	{
		PlotData plotData = null;

		if (instance != null && instance.plots.TryGetValue(name, out plotData))
			return plotData.GetData(rotationIndex);

		return null;
	}

	public static float GetPlotRange(string name)
	{
		PlotData plotData = null;

		if (instance != null && instance.plots.TryGetValue(name, out plotData))
			return plotData.range;

		return 0;
	}

	public static bool IsPlotSymmetrical(string name)
	{
		return !(name.Contains("MELEE") || name.Contains("LASER"));
	}
}
