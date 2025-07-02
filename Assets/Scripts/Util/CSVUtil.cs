using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVUtil
{
	public static string[] Lines(TextAsset sourceAsset)
	{
		return sourceAsset.text.Trim().Replace('\r', '\n').Split('\n');
	}

	public static string[] Lines(string text)
	{
		return text.Trim().Replace('\r', '\n').Split('\n');
	}

	public static string[] Tokenise(string sourceLine, char separator = ',')
	{
		var result = sourceLine.Trim().Split(separator);

		for (var i = 0; i < result.Length; ++i)
			result[i] = result[i].Trim();

		return result;
	}

	public static bool IsLineValid(string[] tokens)
	{
		return tokens != null && tokens.Length > 0 && !string.IsNullOrEmpty(tokens[0]);
	}

	public static bool IsLineComment(string[] tokens)
	{
		return tokens != null && tokens.Length > 0 && tokens[0].Length > 0 && tokens[0][0] == '#';
	}

	public static bool SkipLine(string[] tokens)
	{
		return !IsLineValid(tokens) || IsLineComment(tokens);
	}

	public static float ParseFloat(string[] tokens, int index, float defaultValue)
	{
		if (index < tokens.Length)
		{
			float result;
			if (float.TryParse(tokens[index], out result))
				return result;
		}

		return defaultValue;
	}

	public static int ParseInt(string[] tokens, int index, int defaultValue)
	{
		if (index < tokens.Length)
		{
			int result;
			if (int.TryParse(tokens[index], out result))
				return result;
		}

		return defaultValue;
	}

	public static int ParseEnum(string input, string[] enumNames, int[] enumValues, char delimiter = ',')
	{
		var tokens = Tokenise(input, delimiter);
		var result = 0;

		for (int i = 0; i < tokens.Length; ++i)
		{
			for (int j = 0; j < enumNames.Length; ++j)
			{
				//casting to lower case to get rid of capitalisation errors in the CSV. 
				if (tokens[i].ToLower() == enumNames[j].ToLower())
					result |= enumValues[j];
			}
		}

		return result;
	}

	public static string ParseString(string[] tokens, int index, string defaultValue)
	{
		return index < tokens.Length ? tokens[index] : defaultValue;
	}
}
