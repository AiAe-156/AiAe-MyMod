using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UtilLibs;

public class TMPConverter
{
	private static TMP_FontAsset NotoSans;

	private static TMP_FontAsset GrayStroke;

	public TMPConverter()
	{
		Initialize();
	}

	public void Initialize()
	{
		List<TMP_FontAsset> source = new List<TMP_FontAsset>(Resources.FindObjectsOfTypeAll<TMP_FontAsset>());
		NotoSans = source.FirstOrDefault((TMP_FontAsset f) => ((Object)f).name == "NotoSans-Regular");
		GrayStroke = source.FirstOrDefault((TMP_FontAsset f) => ((Object)f).name == "GRAYSTROKE REGULAR SDF");
	}

	public void ReplaceAllText(GameObject parent, bool realign = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		Component[] componentsInChildren = parent.GetComponentsInChildren(typeof(Text), true);
		Component[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Text val = (Text)array[i];
			if (((Object)((Component)val).gameObject).name == "SettingsDialogData")
			{
				continue;
			}
			string text = val.text;
			GameObject gameObject = ((Component)val).gameObject;
			TMPSettings tMPSettings = ExtractTMPData(text, val);
			if (tMPSettings != null)
			{
				LocText val2 = gameObject.AddComponent<LocText>();
				((TMP_Text)val2).font = (tMPSettings.Font.Contains("GRAYSTROKE") ? GrayStroke : NotoSans);
				((TMP_Text)val2).fontStyle = tMPSettings.FontStyle;
				((TMP_Text)val2).fontSize = tMPSettings.FontSize;
				((TMP_Text)val2).maxVisibleLines = tMPSettings.MaxVisibleLines;
				((TMP_Text)val2).textWrappingMode = (TextWrappingModes)(tMPSettings.EnableWordWrapping ? 1 : 0);
				((TMP_Text)val2).text = "";
				((TMP_Text)val2).overflowMode = tMPSettings.Overflow;
				((Graphic)val2).color = new Color(tMPSettings.Color[0], tMPSettings.Color[1], tMPSettings.Color[2]);
				((TMP_Text)val2).fontSizeMin = tMPSettings.VariableFontSizeMinimum;
				((TMP_Text)val2).fontSizeMax = tMPSettings.VariableFontSizeMaximum;
				val2.key = tMPSettings.Content.Replace(" ", string.Empty);
				if (realign)
				{
					TMPImportFix tMPImportFix = ((Component)val2).gameObject.AddComponent<TMPImportFix>();
					tMPImportFix.alignment = tMPSettings.Alignment;
					tMPImportFix.textOverflow = tMPSettings.Overflow;
					tMPImportFix.fontSizeMin = tMPSettings.VariableFontSizeMinimum;
					tMPImportFix.fontSizeMax = tMPSettings.VariableFontSizeMaximum;
					tMPImportFix.autoResize = tMPSettings.VariableFontSize;
				}
			}
		}
	}

	private static bool isValidJSon(string data)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			return false;
		}
		return data.StartsWith("{") && data.EndsWith("}");
	}

	private static TMPSettings ExtractTMPData(string TMPData, Text text)
	{
		//IL_001a: Expected O, but got Unknown
		TMPSettings result = null;
		if (isValidJSon(TMPData))
		{
			try
			{
				result = JsonConvert.DeserializeObject<TMPSettings>(TMPData);
			}
			catch (JsonReaderException ex)
			{
				JsonReaderException ex2 = ex;
				SgtLogger.warning("Not valid Json format\n" + ((object)ex2).ToString());
			}
			Object.DestroyImmediate((Object)(object)text);
		}
		return result;
	}
}
