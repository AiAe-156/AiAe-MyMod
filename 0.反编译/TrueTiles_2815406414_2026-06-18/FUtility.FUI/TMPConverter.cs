using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FUtility.FUI;

public class TMPConverter
{
	private static TMP_FontAsset NotoSans;

	private static TMP_FontAsset GrayStroke;

	public TMPConverter()
	{
		Initialize();
	}

	public static void Initialize()
	{
		List<TMP_FontAsset> source = new List<TMP_FontAsset>(Resources.FindObjectsOfTypeAll<TMP_FontAsset>());
		NotoSans = source.FirstOrDefault((TMP_FontAsset f) => ((Object)f).name == "NotoSans-Regular");
		GrayStroke = source.FirstOrDefault((TMP_FontAsset f) => ((Object)f).name == "GRAYSTROKE REGULAR SDF");
	}

	public static void ReplaceAllText(GameObject parent, bool realign = true)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		Component[] componentsInChildren = parent.GetComponentsInChildren(typeof(Text), true);
		if (componentsInChildren == null)
		{
			Log.Warning("trying to replace all TMP, but there were no Text components found.");
			return;
		}
		if ((Object)(object)NotoSans == (Object)null)
		{
			Initialize();
		}
		foreach (Text item in componentsInChildren.Cast<Text>())
		{
			if (((Object)((Component)item).gameObject).name == "SettingsDialogData")
			{
				continue;
			}
			string text = item.text;
			GameObject gameObject = ((Component)item).gameObject;
			TMPSettings tMPSettings = ExtractTMPData(text, item);
			if (tMPSettings != null)
			{
				LocText val = gameObject.AddComponent<LocText>();
				((TMP_Text)val).font = (tMPSettings.Font.Contains("GRAYSTROKE") ? GrayStroke : NotoSans);
				((TMP_Text)val).fontStyle = tMPSettings.FontStyle;
				((TMP_Text)val).fontSize = tMPSettings.FontSize;
				((TMP_Text)val).maxVisibleLines = tMPSettings.MaxVisibleLines;
				((TMP_Text)val).enableWordWrapping = tMPSettings.EnableWordWrapping;
				((TMP_Text)val).autoSizeTextContainer = tMPSettings.AutoSizeTextContainer;
				((TMP_Text)val).text = "";
				((Graphic)val).color = new Color(tMPSettings.Color[0], tMPSettings.Color[1], tMPSettings.Color[2]);
				val.key = tMPSettings.Content;
				if (realign)
				{
					((Component)val).gameObject.AddComponent<TMPFixer>().alignment = tMPSettings.Alignment;
				}
			}
		}
	}

	private static bool IsValidJSon(string data)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			return false;
		}
		if (data.StartsWith("{"))
		{
			return data.EndsWith("}");
		}
		return false;
	}

	private static TMPSettings ExtractTMPData(string TMPData, Text text)
	{
		//IL_0014: Expected O, but got Unknown
		TMPSettings result = null;
		if (IsValidJSon(TMPData))
		{
			try
			{
				result = JsonConvert.DeserializeObject<TMPSettings>(TMPData);
			}
			catch (JsonReaderException ex)
			{
				JsonReaderException ex2 = ex;
				Log.Warning("Not valid Json format", ex2);
			}
			Object.DestroyImmediate((Object)(object)text);
		}
		return result;
	}
}
