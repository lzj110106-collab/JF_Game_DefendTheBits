using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TowerIconData
{
	public string identifer;
	public Sprite icon;
}

public class TowerIconDatabase : MonoBehaviour 
{
	static TowerIconDatabase instance;

	public List<TowerIconData> iconData;

	Dictionary<string, Sprite> iconMap;

	void Awake() 
	{ 
		instance = this; 

		iconMap = new Dictionary<string, Sprite>();
		for (int i = 0; i < iconData.Count; ++i)
			iconMap.Add(iconData[i].identifer, iconData[i].icon);
	}

	void OnDestroy() 
	{
		instance = null; 
	}
		
	public static Sprite GetIcon(string identifier)
	{
		return instance == null ? null : instance.iconMap[identifier.Trim()];
	}

	public static string GetName(string identifier)
	{
		return LocManager.Translate("icon_name_" + identifier.Trim());
	}

	public static string GetDescription(string identifier)
	{
		return LocManager.Translate("icon_desc_" + identifier.Trim());
	}

	public static string GetNameID(string identifier)
	{
		return "icon_name_" + identifier.Trim();
	}

	public static string GetDescriptionID(string identifier)
	{
		return "icon_desc_" + identifier.Trim();
	}

	public static bool IsValidIcon(string identifier)
	{
		return instance != null && instance.iconMap.ContainsKey(identifier);
	}
}
