using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using CodeStage.AntiCheat.ObscuredTypes;

//LocManager reads tab separated files of the following format (expressed as CSV here for clarity)
// string_id, language 0 text, language 1 text, language 2 text, ..., last language text

//using a string_id rather than pattern matching against english because it caused so many issues
//in LegoBatman that its hard to explain. the largest offenders were:
//	- capitalisation mismatches (uppercase only fonts were a huge problem)
//	- english text changes in the localisation doc (means you would have to go
//	  through ALL the UI prefabs and make all the changes there as well)

//made everything static access because .instance is annoying and passing
//around pointers to the localisation manager constantly is worse

//removed the concept of categories. server data just overwrites the base data file.
//in LegoBatman categories just added more complications than were required.

//UILocTextLabel now adds ### as a prefix and postfix to make it easier to tell
//when something a label isnt being pushed though the localisation system at all

//TODO: might be worth thread locking the ParseStringTable function so that 
//we can request all the server data at once, rather than sequentially.


public class LocManager : MonoBehaviour
{
    public LocManager Loc
    {
        get { return instance; }
    }

	static LocManager instance;

	public enum Language        //中文、韩语、日语、俄语、西班牙语、法语、意大利语、德语、英文
    {
        English,
        Chinese_Simplified,
        Korean,
        Japanese,
        Russian,
        Spanish,
        French,
        Italian,
        German,
    }
		
	public class StringTable
	{
		public List<string[]> textOrdered = null;
		public Dictionary<string, string[]> text = null;
		public bool isLoaded = false;
	}

	[System.Serializable]
	public class ServerLocalisationData
	{
		public string tag;
		public string serverAssetURL;
		public TextAsset localAsset;

		[HideInInspector] public bool isLoaded = false;
	};

	[System.Serializable]
	public class ColorTag
	{
		public string identifier;
		public Color colour;
	};

	[System.Serializable]
	public class FontOverride
	{
		public Language language;
		public string fontName;
	};

	[System.Serializable]
	public class FontSwap
	{
		public string baseFontName;
		public List<FontOverride> overrides;
	};
		
	public TextAsset baseCSV;

	public Language defaultLanguage = Language.Chinese_Simplified;
	public Language currentLanguage { get; private set; }

	public List<ServerLocalisationData> serverCSVs;
		
	public string languageCode { get; private set; }
	public string regionCode { get; private set; }

	public Dictionary<string, string[]> stringTable = new Dictionary<string, string[]>();

	public List<ColorTag> colourTags;
	Dictionary<string, Color> colourMap;

	public List<FontSwap> fontSwaps;
	Dictionary<string, Dictionary<Language, string>> fontMap;
	Dictionary<string, Font> currentFonts;

	[Space(10)]
	public float minTranslatedTextScale = 0.5f;
	public float parentWidthScale = 0.8f;

	[Space(10)]
	public bool useLocalCSVData;
	public bool replaceBackticksWithLineBreaks = true;
	public bool performFontSwapping = false;
	public bool useAlternateTextScalingMethod = false;

	void Awake()
	{
		instance = this;
        //print("当前的语言为"+Application.systemLanguage);
#if IOS
        switch (Application.systemLanguage)
        {
            case SystemLanguage.ChineseSimplified:
                currentLanguage = Language.Chinese_Simplified;
                break;
            case SystemLanguage.ChineseTraditional:
                currentLanguage = Language.Chinese_Simplified;
                break;
            case SystemLanguage.Chinese:
                currentLanguage = Language.Chinese_Simplified;
                break;
            case SystemLanguage.Korean:
                currentLanguage = Language.Korean;
                break;
            case SystemLanguage.Japanese:
                currentLanguage = Language.Japanese;
                break;
            case SystemLanguage.Russian:
                currentLanguage = Language.Russian;
                break;
            case SystemLanguage.Spanish:
                currentLanguage = Language.Spanish;
                break;
            case SystemLanguage.French:
                currentLanguage = Language.French;
                break;
            case SystemLanguage.Italian:
                currentLanguage = Language.Italian;
                break;
            case SystemLanguage.German:
                currentLanguage = Language.German;
                break;
            default:
                currentLanguage = Language.Chinese_Simplified;
                break;
        }
#else
        currentLanguage = Language.Chinese_Simplified;
#endif
        //languageCode = PreciseLocale.GetLanguageID();
        //currentLanguage = defaultLanguage;
        //currentLanguage = Language.Korean;
        //languageCode = "zh_CN";
        //languageCode = "en_US";
        regionCode = PreciseLocale.GetRegion();

		colourMap = new Dictionary<string, Color>();
		for (var i = 0; i < colourTags.Count; ++i)
			colourMap.Add(colourTags[i].identifier, colourTags[i].colour);

		//figure out the language first so we dont load in english fonts then
		//immediately load them out again if the app is starting in russian or something
		//RestorePreviousLanguage();
		LoadData();
        LoadPrices();

		InitialiseFonts();
	}
		
	public static bool IsRegionUS()
	{
		return instance == null ? false : instance.regionCode == "US";
	}

	public static string LanguageCode()
	{
		return instance == null ? "en_US" : instance.languageCode;
	}

	public static string RegionCode()
	{
		return instance == null ? "US" : instance.regionCode;
	}

	public static Language CurrentLanguage()
	{
		return instance == null ? Language.Chinese_Simplified : instance.currentLanguage;
	}

	public static string CurrentLanguageName()
	{
		return TranslatedLanguageName(instance.currentLanguage);
	}

	public static bool PerformFontSwapping()
	{
		return instance == null ? false : instance.performFontSwapping;
	}
				
#region TRANSLATION

	public static bool TranslationAvailable(string stringID)
	{
		//trivial case
		string[] result;
		if (instance.stringTable.TryGetValue(stringID, out result))
			return true;

		return false;
	}

	public static string Translate(string stringID, bool parseColourInformation = true)
	{
		return Translate(stringID, instance.currentLanguage);
	}

	public static string Translate(string stringID, Language targetLanguage, bool parseColourInformation = true)
	{
		if (string.IsNullOrEmpty(stringID))
			return "NULL STRING ID";

		string[] results;
        if (instance.stringTable.TryGetValue(stringID, out results))
		{
			if (parseColourInformation)
            {
                return ParseColourInformation(results[(int)targetLanguage]);
            }

            return results[(int)targetLanguage];
		}
		Debug.LogWarning("[LocManager] could not translate: '" + stringID + "'");
		return stringID;
	}

	public static string TranslatedLanguageName(Language targetLanguage)
	{
		//category 0 is the Game category that is shipped in the resources dir.
		//row 0 is the translated language names.
		return instance.stringTable["language_name"][(int)targetLanguage];
	}

#endregion

#region STRING BUILDERS

	//TODO: a builder that takes a string[] and an int[] would cut down this duplication
	//TODO: but it would make the API more irritating to use

	public static string BuildString(Language language, string baseStringID, string nameStringID)
	{
		var baseTextTranslated = Translate(baseStringID, language, false);
		var nameTextTranslated = Translate(nameStringID, language, false);

		return ParseColourInformation(baseTextTranslated.Replace("{name0}", nameTextTranslated));
	}

	public static string BuildString(Language language, string baseStringID, string nameStringID, int num0)
	{
		var baseTextTranslated = Translate(baseStringID, language, false);
		var nameTextTranslated = Translate(nameStringID, language, false);

		var temp = baseTextTranslated.Replace("{name0}", nameTextTranslated);
		return ParseColourInformation(temp.Replace("{number0}", num0.ToString()));
	}

	public static string BuildString(Language language, string baseStringID, string nameStringID, int num0, int num1)
	{
		var baseTextTranslated = Translate(baseStringID, language, false);
		var nameTextTranslated = Translate(nameStringID, language, false);

		var temp = baseTextTranslated.Replace("{name0}", nameTextTranslated);
		temp = temp.Replace("{number0}", num0.ToString());
		temp = temp.Replace("{number1}", num1.ToString());

		return ParseColourInformation(temp);
	}

	public static string BuildString(Language language, string baseStringID, string nameStringID, int num0, int num1, int num2)
	{
		var baseTextTranslated = Translate(baseStringID, language, false);
		var nameTextTranslated = Translate(nameStringID, language, false);

		var temp = baseTextTranslated.Replace("{name0}", nameTextTranslated);
		temp = temp.Replace("{number0}", num0.ToString());
		temp = temp.Replace("{number1}", num1.ToString());
		temp = temp.Replace("{number2}", num2.ToString());

		return ParseColourInformation(temp);
	}

	public static string BuildString(Language language, string baseStringID, int num0)
	{
        var baseTextTranslated = Translate(baseStringID, language, false);
		return ParseColourInformation(baseTextTranslated.Replace("{number0}", num0.ToString()));
	}

	public static string BuildString(Language language, string baseStringID, int num0, int num1)
	{
		var baseTextTranslated = Translate(baseStringID, language, false);
		baseTextTranslated = baseTextTranslated.Replace("{number0}", num0.ToString());
		baseTextTranslated = baseTextTranslated.Replace("{number1}", num1.ToString());

		return ParseColourInformation(baseTextTranslated);
	}

	public static string BuildString(Language language, string baseStringID, int num0, int num1, int num2)
	{
		var baseTextTranslated = Translate(baseStringID, language, false);
		baseTextTranslated = baseTextTranslated.Replace("{number0}", num0.ToString());
		baseTextTranslated = baseTextTranslated.Replace("{number1}", num1.ToString());
		baseTextTranslated = baseTextTranslated.Replace("{number2}", num2.ToString());

		return ParseColourInformation(baseTextTranslated);
	}

	public static string BuildString(Language language, string baseStringID, float number)
	{
		var baseTextTranslated = Translate(baseStringID, language, false);
		return ParseColourInformation(baseTextTranslated.Replace("{number0}", number.ToString("N1")));
	}

	public static string StripTrademarks(string baseText)
	{
		return baseText.Replace("™", "");
	}

#endregion

	public static string BuildString(string baseStringID, string nameStringID) 
	{
		return BuildString(instance.currentLanguage, baseStringID, nameStringID);
	}

	public static string BuildString(string baseStringID, string nameStringID, int num0)
	{
		return BuildString(instance.currentLanguage, baseStringID, nameStringID, num0);
	}

	public static string BuildString(string baseStringID, string nameStringID, int num0, int num1)
	{
		return BuildString(instance.currentLanguage, baseStringID, nameStringID, num0, num1); 
	}

	public static string BuildString(string baseStringID, string nameStringID, int num0, int num1, int num2)
	{
		return BuildString(instance.currentLanguage, baseStringID, nameStringID, num0, num1, num2);
	}

	public static string BuildString(string baseStringID, int num0)
	{
		return BuildString(instance.currentLanguage, baseStringID, num0);
	}

	public static string BuildString(string baseStringID, int num0, int num1)
	{
		return BuildString(instance.currentLanguage, baseStringID, num0, num1);
	}

	public static string BuildString(string baseStringID, int num0, int num1, int num2)
	{
		return BuildString(instance.currentLanguage, baseStringID, num0, num1, num2);
	}

	public static string BuildString(string baseStringID, Language language, float number)
	{
		return BuildString(instance.currentLanguage, baseStringID, number);
	}

#region TEXT SCALING HELPERS

	public static void Assign(Text destination, string stringID, bool allowScaling = true)
	{
		ApplyText(destination, 
				  Translate(stringID, Language.Chinese_Simplified), 
				  Translate(stringID, instance.currentLanguage),
				  allowScaling);
	}

	public static void Assign(Text destination, string stringID, string name0, bool allowScaling = true)
	{
		ApplyText(destination, 
				  BuildString(Language.Chinese_Simplified, stringID, name0),
				  BuildString(instance.currentLanguage, stringID, name0),
				  allowScaling);
	}

	public static void Assign(Text destination, string stringID, string name0, int num0, bool allowScaling = true)
	{
		ApplyText(destination, 
				  BuildString(Language.Chinese_Simplified, stringID, name0, num0),
				  BuildString(instance.currentLanguage, stringID, name0, num0),
				  allowScaling);
	}

	public static void Assign(Text destination, string stringID, string name0, int num0, int num1, bool allowScaling = true)
	{
		ApplyText(destination, 
				  BuildString(Language.Chinese_Simplified, stringID, name0, num0, num1),				   
				  BuildString(instance.currentLanguage, stringID, name0, num0, num1),
				  allowScaling);
	}

	public static void Assign(Text destination, string stringID, string name0, int num0, int num1, int num2, bool allowScaling = true)
	{
		ApplyText(destination, 
				  BuildString(Language.Chinese_Simplified, stringID, name0, num0, num1, num2),
				  BuildString(instance.currentLanguage, stringID, name0, num0, num1, num2),
				  allowScaling);
	}

	public static void Assign(Text destination, string stringID, int num0, bool allowScaling = true)
	{
		ApplyText(destination, 
				  BuildString(Language.Chinese_Simplified, stringID, num0),
				  BuildString(instance.currentLanguage, stringID, num0),
				  allowScaling);
	}

	public static void Assign(Text destination, string stringID, int num0, int num1, bool allowScaling = true)
	{
		ApplyText(destination, 
				  BuildString(Language.Chinese_Simplified, stringID, num0, num1),
				  BuildString(instance.currentLanguage, stringID, num0, num1),
				  allowScaling);
	}

	public static void Assign(Text destination, string stringID, int num0, int num1, int num2, bool allowScaling = true)
	{
		ApplyText(destination, 
				  BuildString(Language.Chinese_Simplified, stringID, num0, num1, num2),
				  BuildString(instance.currentLanguage, stringID, num0, num1, num2),
				  allowScaling);
	}
		
	static void ApplyText(Text destination, string english, string translated, bool allowScaling)
	{
		var cache = GetOrCreateFontSwapCache(destination);
		var rect = destination.GetComponent<RectTransform>();

		bool didScale = false;

		if (destination.horizontalOverflow == HorizontalWrapMode.Overflow && 
			instance.currentLanguage != Language.Chinese_Simplified)
		{
			//using layout utility to figure out the actual length of the text. this
			//is much more accurate than just comparing character counts.
			destination.text = english;
			float englishLength = LayoutUtility.GetPreferredWidth(rect);
            
			destination.text = translated;
			float translatedLength = LayoutUtility.GetPreferredWidth(rect);

			//TODO: real padding value.

			//we use the max of the available space and the english text to scale the translated text.
			//this is because the english text might only use a quarter of the parent rect size,
			//which in turn would cause long text to get squished even though there is more space
			//to use. hard to explain.
			float availableSpace = rect.parent.GetComponent<RectTransform>().rect.width * instance.parentWidthScale;

			if (translatedLength > 0)
			{
				//only scaling things down
				var scale = Mathf.Max(englishLength, availableSpace)/translatedLength;
				if (scale < 1.0f && allowScaling)														
				{
					if (scale < instance.minTranslatedTextScale)
						scale = instance.minTranslatedTextScale;

					var localScale = cache.originalScale;
					localScale.x *= scale;
					rect.localScale = localScale;

					// Allow the pivot to anchor appropriately
					if (destination.alignment == TextAnchor.LowerLeft || 
						destination.alignment == TextAnchor.MiddleLeft || 
						destination.alignment == TextAnchor.UpperLeft)
					{
						rect.pivot = new Vector2(0.0f, cache.originalPivot.y);
					}
					else if (destination.alignment == TextAnchor.LowerRight || 
							 destination.alignment == TextAnchor.MiddleRight || 
							 destination.alignment == TextAnchor.UpperRight)
					{
						rect.pivot = new Vector2(1.0f, cache.originalPivot.y);
					}

					didScale = true;
				}
			}
		}

		if (!didScale)
		{
			//restore original values
			rect.localScale = cache.originalScale;
			rect.pivot = cache.originalPivot;
		}
        //print(translated);
		destination.text = translated;
		destination.alignByGeometry = destination.font.dynamic ? false : cache.wasAlignByGeometry;
	}
		
#endregion

#region PLAYER PREFS

	public const string AUTO_DETECT_KEY = "auto_detect_language";
	public const string LANGUAGE_KEY = "current_language";

	//returns true if fonts changed
	public static bool SetLanguage(Language language)
	{
		if (instance != null)
		{
			instance.currentLanguage = language;

            //PlayerPrefs.SetInt(LANGUAGE_KEY, (int)instance.currentLanguage);
            //PlayerPrefs.Save();
            //Debug.Log((int)language);
            ObscuredPrefs.SetInt(LANGUAGE_KEY, (int)instance.currentLanguage);
            ObscuredPrefs.Save();

            return LoadNewFonts();
		}

		return false;
	}

	void RestorePreviousLanguage()
	{
        //if (PlayerPrefs.GetInt(AUTO_DETECT_KEY, 1) == 1)
        //{
        //	SetLanguage(ParseRegionCode(languageCode));
        //	PlayerPrefs.SetInt(AUTO_DETECT_KEY, 0);
        //}
        //else
        //{
        //	SetLanguage((Language)PlayerPrefs.GetInt(LANGUAGE_KEY, (int)defaultLanguage));
        //}
        

        if (ObscuredPrefs.GetInt(AUTO_DETECT_KEY, 1) == 1)
        {
            SetLanguage(ParseRegionCode(languageCode));
            ObscuredPrefs.SetInt(AUTO_DETECT_KEY, 0);
        }
        else
        {
            SetLanguage((Language)ObscuredPrefs.GetInt(LANGUAGE_KEY, (int)defaultLanguage));
        }
    }

#endregion

#region COLOUR TAGS

	//NB: this assumes there are no nested {} tags in the input string. this will be the
	//case in things such as "Destroy {{name}|purple} {{number}|red} times!". to 
	//resolve this, perform the {name} and {number} substitutions prior to
	//calling this function.
	static string ParseColourInformation(string input)
	{
		var result = input;

		int startIndex = 0;
		while (true)
		{
			var i0 = result.IndexOf('{', startIndex);
			var i1 = result.IndexOf('|', startIndex);
			var i2 = result.IndexOf('}', startIndex);

			if (i0 == -1 || i1 == -1 || i2 == -1)
				break;

			if (i1 > i0 && i1 < i2)
			{
				Color colourSampler;

				var token = result.Substring(i0 + 1, i1 - i0 - 1);
				var colourID = result.Substring(i1 + 1, i2 - i1 - 1).Trim();

				//default to white.
				var hex = "#ffffffff";
				if (instance.colourMap.TryGetValue(colourID, out colourSampler))
				{
					hex = "#" + ((int)(colourSampler.r * 255)).ToString("x2") +
								((int)(colourSampler.g * 255)).ToString("x2") +
								((int)(colourSampler.b * 255)).ToString("x2") +
								"ff";
				}

				var prefix = result.Substring(0, i0);
				var suffix = result.Substring(i2 + 1);

				//TODO: use a string builder
				result = prefix + "<color=" + hex +">" + token + "</color>" + suffix;

				//restart the process with the new string
				startIndex = 0;
			}
			else
			{
				//no colour info in this tag
				startIndex = i2;
			}
		}

		return result;
	}

#endregion

#region SPRITE MANAGEMENT

	public static Sprite LoadSprite(string assetName)
	{
		//localisation swapping is a slow operation anyway, dont bother with async loading the correct sprites.
		return Resources.Load<Sprite>("Localisation/" + assetName);
	}

#endregion

#region DATA PARSING

	void LoadData()
	{

		if (baseCSV != null)
			ParseStringDatabase("base", baseCSV.text);

		if (useLocalCSVData)
        {
            foreach (var data in serverCSVs)
			{
				if (data.localAsset != null)
				{
					ParseStringDatabase(data.tag, data.localAsset.text);
					data.isLoaded = true;
				}
			}
		}
		else
        {
            StartCoroutine(LoadServerData());
		}
	}

	IEnumerator LoadServerData()
	{
		foreach (var data in serverCSVs)
		{
			if (string.IsNullOrEmpty(data.serverAssetURL))
				continue;

			var www = new WWW(data.serverAssetURL);
			yield return www;

			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.Log("[LocManager] could not retrieve data at location: '" + data.serverAssetURL + "' (" + www.error + ")");
			}
			else if (string.IsNullOrEmpty(www.text))
			{
				Debug.Log("[LocManager] could not retrieve data at location: '" + data.serverAssetURL + "'");
			}
			else
			{
				ParseStringDatabase(data.tag, www.text);
				data.isLoaded = true;
			}
		}
	}

	public static Language ParseRegionCode(string code)
	{
        //print(code);
		//these are pulled from IOS639-1 language code lists.
		//https://www.wikiwand.com/en/List_of_ISO_639-1_codes
		//https://www.wikiwand.com/en/Language_localisation
		//https://msdn.microsoft.com/en-us/library/ms533052(v=vs.85).aspx
		//etc

		if (code.IndexOf("en") == 0)		return Language.Chinese_Simplified; //en-GB, en-US, etc
//		if (code.IndexOf("pt") == 0)		return Language.Portugese; //pt, pt-br
//		if (code.IndexOf("de") == 0)		return Language.German; //de, de-at, etc
////		if (code == "da")					return Language.Danish;
//		if (code.IndexOf("it") == 0)		return Language.Italian; //it, it-ch
//		if (code.IndexOf("ru") == 0)		return Language.Russian; //ru, ru-mo
//		if (code == "ko")					return Language.Korean;

	if (code == "zh_CN")			return Language.Chinese_Simplified;	
//		if (code.IndexOf("zh") == 0)	return Language.Chinese_Traditional;
////		if (code == "zh_HK")		return Language.Chinese_Traditional_HK;
////		if (code == "zh_TW")		return Language.Chinese_Traditional_TW;
//		if (code == "jp")			return Language.Japanese;

//		if (code.IndexOf("nl") == 0)
//			return code == "nl_BE" ? Language.Belgian_Flemish : Language.Dutch;
//
//		if (code.IndexOf("fr") == 0)
//			return code == "fr_BE" ? Language.Belgian_French : Language.French;
//
//		if (code.IndexOf("es") == 0)	
//			return code == "es_ES" ? Language.Spanish : Language.Spanish_LatinAmerican; //es-MX, es-AR, etc

		//if (code.IndexOf("fr") == 0)	return Language.French;
		//if (code.IndexOf("es") == 0)	return Language.Spanish;

		//if (code.IndexOf("th") == 0)	return Language.Thai;
		//if (code.IndexOf("ar") == 0)	return Language.Arabic;

		Debug.Log("[LocManager] could not determine device language: " + code);
		return Language.Chinese_Simplified;
	}

	//NB: not bothering with category switching or anything like that. if serverCSVs contain
	//the same key as one found in the baseCSV, then the baseCSV is overwritten. 

	void ParseStringDatabase(string tag, string sourceText)
	{
		string[] lines = sourceText.Split('\n');
		int numLanguages = Enum.GetValues(typeof(Language)).Length;
        
        for (int rowNo = 1; rowNo < lines.Length; ++rowNo) //skip the header row.
		{
			string line = lines[rowNo];
			if (!string.IsNullOrEmpty(line.Trim()))
			{
                //string[] tokens = line.Split(',');
                string[] tokens = line.Split('\t');
                if (tokens.Length < numLanguages + 1) //NB: first column is the dictionary key
				{
					Debug.Log("LocManager] could not parse: '" + line + "'");
					continue; //dont throw an error, just continue reading valid strings
				}

				string[] write = new string[tokens.Length - 1];
				for (int i = 0; i < write.Length; ++i)
				{
					write[i] = tokens[i + 1].Trim(); //some of the google doc stuff has leading and trailing newlines and stuff. fix that now.
					//clearly mark missing translations by setting it to the string_id
					if (string.IsNullOrEmpty(write[i]))
						write[i] = tokens[0];

					if (replaceBackticksWithLineBreaks)
						write[i] = write[i].Replace('`', '\n');
				}

                //these two lines are for debugging
                //				if (stringTable.ContainsKey(write[0]))
                //					Debug.Log("[LocManager] overwriting key: " + tokens[0] + "' (" + tag + ")");


                //using .Add here will throw an exception. this 
                //will just overwrite the existing data instead.
				stringTable[tokens[0]] = write;

//				Debug.Log("[LocManager] wrote: '" + tokens[0] + "' (" + tag + ")");
			}
		}

//		Debug.Log("[LocManager] parsed: '" + tag + "'");
	}

#endregion

#region FONT MANAGEMENT

	void InitialiseFonts()
	{
		if (PerformFontSwapping())
		{
			currentFonts = new Dictionary<string, Font>();
			fontMap = new Dictionary<string, Dictionary<Language, string>>();

			//convert list of lists into a dictionary for faster lookups
			for (var i = 0; i < fontSwaps.Count; ++i)
			{
				var mapping = new Dictionary<Language, string>();
				for (var j = 0; j < fontSwaps[i].overrides.Count; ++j)
					mapping.Add(fontSwaps[i].overrides[j].language, fontSwaps[i].overrides[j].fontName);

				fontMap.Add(fontSwaps[i].baseFontName, mapping);

				//set up the initial font mapping based on the language that has already been detected
				var mappedFont = GetMappedFont(fontSwaps[i].baseFontName);

				//pull font data in
				var fontData = Resources.Load<Font>("Fonts/" + mappedFont);
				if (fontData != null)
				{
					currentFonts.Add(fontSwaps[i].baseFontName, fontData);
					//Debug.Log("[LocManager] loaded font: " + fontData.name);
				}
				else
				{
					//Debug.Log("[LocManager] couldnt not load font: " + fontSwaps[i].baseFontName);
				}
			}
		}
	}

	//returns true if font data was loaded or unloaded
	static bool LoadNewFonts()
	{
		if (!PerformFontSwapping())
			return false;

		//this function gets called via SetLanguage at start up. InitialiseFonts
		//requires the language to be set up, so just null check our way
		//out of the cycle.
		if (instance.currentFonts == null)
			return false;
		
		bool didSwapFonts = false;

		//cant iterate and change values at the same time. so shove the new mapping
		//into a separate dictionary and reassign it afterwards
		var newFonts = new Dictionary<string, Font>();

		foreach (var kv in instance.currentFonts)
		{
			var newFontName = GetMappedFont(kv.Key);
			if (newFontName != kv.Value.name)
			{
				var fontData = Resources.Load<Font>("Fonts/" + newFontName);
				if (fontData != null)
				{
					newFonts.Add(kv.Key, fontData);
					didSwapFonts = true;
					continue;
				}
				else
				{
					Debug.Log("[LocManager] couldnt not load font: " + kv.Value);
				}		
			}

			//preserve the old state if a new font wasnt loaded in
			newFonts.Add(kv.Key, kv.Value);
		}

		instance.currentFonts = newFonts;

		//Debug.Log("[LocManager] new font mappings: ");
		//foreach (var kv in instance.currentFonts)
		//	Debug.Log("[LocManager]    " + kv.Key + " -> " + kv.Value.name);

		return didSwapFonts;
	}
		
	public static void RefreshFonts(GameObject rootNode)
	{
		if (PerformFontSwapping() && rootNode != null)
		{
			var textInstances = rootNode.GetComponentsInChildren<Text>(true);
			for (var i = 0; i < textInstances.Length; ++i)
			{
				//if we havent cached the original name of the font yet, do that now
				//before the font gets swapped to the new font and the original
				//data is lost forever
				var cache = GetOrCreateFontSwapCache(textInstances[i]);
					
				//only re-assign the font if there is a valid font mapping. 
				Font fontData = null;
				if (instance.currentFonts.TryGetValue(cache.originalFontName, out fontData))
				{
//					Debug.Log("[LocManager] changed: " + textInstances[i].text + " to " + fontData.name + " cached: " + cache.originalFontName);
					textInstances[i].font = fontData;
					textInstances[i].alignByGeometry = fontData.dynamic ? false : cache.wasAlignByGeometry;
				}
				else
				{
					//Debug.Log("[LocManager] failed: " + textInstances[i].text + " cached: " + cache.originalFontName);
				}
			}
		}
	}

	static string GetMappedFont(string baseFontName)
	{
		return GetMappedFont(baseFontName, CurrentLanguage());
	}

	static string GetMappedFont(string baseFontName, Language language)
	{
		if (PerformFontSwapping())
		{
			Dictionary<Language, string> overrides;
			if (instance.fontMap.TryGetValue(baseFontName, out overrides))
			{
				//see if there is an override font set for this language
				string mappedFont;
				if (overrides.TryGetValue(language, out mappedFont))
					return mappedFont;
			}
		}

		//something went wrong, or there is no font mapping for this language
		return baseFontName;
	}

#endregion

	static UILocFontSwapCache GetOrCreateFontSwapCache(Text target)
	{
		var cache = target.GetComponent<UILocFontSwapCache>();
		if (cache == null)
		{
			cache = target.gameObject.AddComponent<UILocFontSwapCache>();
			cache.originalFontName = target.font.name;

			var rectTransform = target.GetComponent<RectTransform>();
			cache.originalScale = rectTransform.localScale;
			cache.originalPivot = rectTransform.pivot;

			cache.wasAlignByGeometry = target.alignByGeometry;
		}		

		return cache;
	}

    public TextAsset priceAsset;

   struct Prices
    {
        public string price;
    }

    Dictionary<string, Prices[]> allPrices=new Dictionary<string, Prices[]>();

    void LoadPrices()
    {
        if (priceAsset)
        {
            string[] lines = priceAsset.text.Split('\n');

            for (int rowNo = 1; rowNo < lines.Length; ++rowNo) //skip the header row.
            {
                string line = lines[rowNo];
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    string[] tokens = line.Split(',');

                    Prices[] write = new Prices[tokens.Length - 1];

                    for (int i = 0; i < write.Length; ++i)
                    {
                        write[i].price =tokens[i + 1];
                    }
                    allPrices[tokens[0]] = write;
                }
            }
        }
    }

    public static string GetTargetPrice(int price_zh)
    {
        try
        {
            return instance.allPrices[string.Format("id_{0}", price_zh)][(int)instance.currentLanguage].price;
        }
        catch
        {
            return null;
        }
    }

    public List<string> currencySymbol;

    public static string GetCurrencySymbol()
    {
        return instance.currencySymbol[(int)instance.currentLanguage];
    }

    public static bool isInChina()
    {
        if (instance.currentLanguage == Language.Chinese_Simplified)
            return true;
        else
            return false;
    }
}
