using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace UtilLibs.MarkdownExport;

public static class MarkdownUtil
{
	public const string EmptyTableCell = "&#8288 {: style=\"padding:0\"}";

	private static Dictionary<Texture2D, Texture2D> Copies = new Dictionary<Texture2D, Texture2D>();

	private static Dictionary<Sprite, Texture2D> Copies2 = new Dictionary<Sprite, Texture2D>();

	private static StringBuilder builder = new StringBuilder(64);

	private static void CleanTag(ref string tagKey)
	{
		if (tagKey.Contains("PLANTFIBER"))
		{
			tagKey = tagKey.Replace("PLANTFIBER", "PLANT_FIBER");
		}
		if (tagKey.Contains("SPICEVINE"))
		{
			tagKey = tagKey.Replace("SPICEVINE", "SPICE_VINE");
		}
		if (tagKey.Contains("FORESTTREE"))
		{
			tagKey = tagKey.Replace("FORESTTREE", "WOOD_TREE");
		}
		if (tagKey.Contains("GASGRASSHARVESTED"))
		{
			tagKey = tagKey.Replace("GASGRASSHARVESTED", "GASGRASS");
		}
		if (tagKey.Contains("BLUEGRASS"))
		{
			tagKey = tagKey.Replace("BLUEGRASS", "BLUE_GRASS");
		}
	}

	public unsafe static string GetTagStringWithIcon(Tag tag, bool prefix = true)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		string text = ((object)(*(Tag*)(&tag))/*cast due to .constrained prefix*/).ToString();
		string tagString = GetTagString(tag);
		string text2 = "elements";
		if (ElementLoader.GetElement(tag) == null)
		{
			Exporter.AddEntity(tag);
			text2 = "entities";
		}
		if (prefix)
		{
			return " ![" + text + "](/assets/images/" + text2 + "/" + text + ".png){.inline-icon} " + GetTagString(Tag.op_Implicit(text));
		}
		return GetTagString(Tag.op_Implicit(text)) + " ![" + text + "](/assets/images/" + text2 + "/" + text + ".png){.inline-icon}";
	}

	public unsafe static string GetTagString(Tag tag, bool desc = false)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		string text = ".NAME";
		if (desc)
		{
			text = ".DESC";
		}
		string tagKey = ((object)(*(Tag*)(&tag))/*cast due to .constrained prefix*/).ToString().ToUpperInvariant();
		CleanTag(ref tagKey);
		GameObject val = Assets.TryGetPrefab(tag);
		if ((Object)(object)val == (Object)null)
		{
			if (desc)
			{
				tagKey += "_DESC";
			}
			return MD_Localization.Strip(MD_Localization.L("STRINGS.MISC.TAGS." + tagKey));
		}
		if (ElementLoader.GetElement(tag) != null)
		{
			return MD_Localization.Strip(MD_Localization.L("STRINGS.ELEMENTS." + ((object)(*(Tag*)(&tag))/*cast due to .constrained prefix*/).ToString().ToUpperInvariant() + text));
		}
		if ((Object)(object)Assets.GetBuildingDef(((object)(*(Tag*)(&tag))/*cast due to .constrained prefix*/).ToString()) != (Object)null)
		{
			return MD_Localization.Strip(MD_Localization.L("STRINGS.BUILDINGS.PREFABS." + ((object)(*(Tag*)(&tag))/*cast due to .constrained prefix*/).ToString().ToUpperInvariant() + text));
		}
		PlantableSeed val2 = default(PlantableSeed);
		if (tagKey.Contains("SEED") && val.TryGetComponent<PlantableSeed>(ref val2))
		{
			string tagKey2 = ((object)Unsafe.As<Tag, Tag>(ref val2.PlantID)/*cast due to .constrained prefix*/).ToString().ToUpperInvariant();
			CleanTag(ref tagKey2);
			string key = "STRINGS.CREATURES.SPECIES.SEEDS." + tagKey2 + text;
			if (MD_Localization.HasKey(key))
			{
				return MD_Localization.Strip(MD_Localization.L(key));
			}
		}
		if (tagKey.Contains("GEYSERGENERIC_"))
		{
			string text2 = tagKey.Replace("GEYSERGENERIC_", string.Empty);
			string key2 = "STRINGS.CREATURES.SPECIES.GEYSER." + text2 + text;
			if (MD_Localization.HasKey(key2))
			{
				return MD_Localization.Strip(MD_Localization.L(key2));
			}
		}
		string key3 = "STRINGS.ITEMS.INDUSTRIAL_PRODUCTS." + tagKey + text;
		if (MD_Localization.HasKey(key3))
		{
			return MD_Localization.Strip(MD_Localization.L(key3));
		}
		string key4 = "STRINGS.ITEMS.INGREDIENTS." + tagKey + text;
		if (MD_Localization.HasKey(key4))
		{
			return MD_Localization.Strip(MD_Localization.L(key4));
		}
		string key5 = "STRINGS.ITEMS.FOOD." + tagKey + text;
		if (MD_Localization.HasKey(key5))
		{
			return MD_Localization.Strip(MD_Localization.L(key5));
		}
		string key6 = "STRINGS.CREATURES.SPECIES." + tagKey + text;
		if (MD_Localization.HasKey(key6))
		{
			return MD_Localization.Strip(MD_Localization.L(key6));
		}
		string key7 = "STRINGS.UI.SPACEDESTINATIONS.COMETS." + tagKey + text;
		if (MD_Localization.HasKey(key7))
		{
			return MD_Localization.Strip(MD_Localization.L(key7));
		}
		if (MD_Localization.TryGetManuallyRegistered(tagKey, out var val3))
		{
			return MD_Localization.Strip(val3);
		}
		if (desc)
		{
			InfoDescription val4 = default(InfoDescription);
			if (val.TryGetComponent<InfoDescription>(ref val4))
			{
				return MD_Localization.Strip(val4.description);
			}
			return GetTagString(tag);
		}
		return MD_Localization.Strip(KSelectableExtensions.GetProperName(val));
	}

	public static string GetElementState(State state)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected I4, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		state = (State)(state & 3);
		State val = state;
		State val2 = val;
		return (val2 - 1) switch
		{
			2 => GetTagString(GameTags.Solid), 
			1 => GetTagString(GameTags.Liquid), 
			0 => GetTagString(GameTags.Gas), 
			_ => throw new NotImplementedException(), 
		};
	}

	public static string GetElementState(ConduitType state)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return (state - 1) switch
		{
			2 => GetTagString(GameTags.Solid), 
			1 => GetTagString(GameTags.Liquid), 
			0 => GetTagString(GameTags.Gas), 
			_ => throw new NotImplementedException(), 
		};
	}

	public static string StrippedBuildingName(string ID)
	{
		return MD_Localization.Strip(MD_Localization.L("STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".NAME"));
	}

	public static string GetPortDescription(ConduitType conduitType, bool input, string material = null)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		if (material == null)
		{
			material = GetElementState(conduitType);
		}
		if (conduitType - 1 > 1)
		{
			if ((int)conduitType == 3)
			{
				return string.Format(MD_Localization.L(input ? "RAIL_INPUT" : "RAIL_OUTPUT"), material);
			}
			throw new NotImplementedException();
		}
		return string.Format(MD_Localization.L(input ? "PIPE_INPUT" : "PIPE_OUTPUT"), material);
	}

	public static Texture2D GetReadableCopy(Texture2D source)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (Copies.ContainsKey(source))
		{
			return Copies[source];
		}
		if ((Object)(object)source == (Object)null || ((Texture)source).width == 0 || ((Texture)source).height == 0)
		{
			return null;
		}
		RenderTexture temporary = RenderTexture.GetTemporary(((Texture)source).width, ((Texture)source).height, 0, (RenderTextureFormat)7, (RenderTextureReadWrite)1);
		Graphics.Blit((Texture)(object)source, temporary);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary;
		Texture2D val = new Texture2D(((Texture)source).width, ((Texture)source).height);
		val.ReadPixels(new Rect(0f, 0f, (float)((Texture)temporary).width, (float)((Texture)temporary).height), 0, 0);
		val.Apply();
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		Copies[source] = val;
		return val;
	}

	private static Texture2D GetSingleSpriteFromTexture(Sprite sprite, Color tint = default(Color))
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)sprite == (Object)null))
		{
			_ = sprite.rect;
			Rect val = sprite.rect;
			if (!(((Rect)(ref val)).width <= 0f))
			{
				val = sprite.rect;
				if (!(((Rect)(ref val)).height <= 0f))
				{
					bool flag = tint != default(Color);
					if (flag || !Copies2.ContainsKey(sprite))
					{
						val = sprite.textureRect;
						int num = Mathf.RoundToInt(((Rect)(ref val)).width);
						val = sprite.textureRect;
						Texture2D val2 = new Texture2D(num, Mathf.RoundToInt(((Rect)(ref val)).height));
						Rect textureRect = sprite.textureRect;
						if (((Rect)(ref textureRect)).width == 0f || ((Rect)(ref textureRect)).height == 0f)
						{
							return null;
						}
						Texture2D readableCopy = GetReadableCopy(sprite.texture);
						if ((Object)(object)readableCopy == (Object)null)
						{
							return null;
						}
						Color[] pixels = readableCopy.GetPixels(Mathf.RoundToInt(((Rect)(ref textureRect)).x), Mathf.RoundToInt(((Rect)(ref textureRect)).y), Mathf.RoundToInt(((Rect)(ref textureRect)).width), Mathf.RoundToInt(((Rect)(ref textureRect)).height));
						if (flag)
						{
							Color[] array = (Color[])(object)new Color[pixels.Length];
							for (int i = 0; i < pixels.Length; i++)
							{
								array[i] = pixels[i] * tint;
							}
							val2.SetPixels(array);
						}
						else
						{
							val2.SetPixels(pixels);
						}
						val2.Apply();
						((Object)val2).name = ((Object)sprite.texture).name + " " + ((Object)sprite).name;
						if (flag)
						{
							return val2;
						}
						Copies2.Add(sprite, val2);
					}
					return Copies2[sprite];
				}
			}
		}
		return null;
	}

	public static void WriteUISpriteToFile(Sprite sprite, string folder, string id, Color tint = default(Color))
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		id = SanitationUtils.SanitizeName(id);
		Directory.CreateDirectory(folder);
		string path = Path.Combine(folder, id + ".png");
		Texture2D singleSpriteFromTexture = GetSingleSpriteFromTexture(sprite, tint);
		if (!((Object)(object)singleSpriteFromTexture == (Object)null))
		{
			byte[] bytes = ImageConversion.EncodeToPNG(singleSpriteFromTexture);
			File.WriteAllBytes(path, bytes);
		}
	}

	internal static string GetElementTransitionProperties(Element element)
	{
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Invalid comparison between Unknown and I4
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Invalid comparison between Unknown and I4
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		string text = "<br/>";
		string arg = "";
		if (element.lowTempTransition != null)
		{
			arg = Math.Round(GameUtil.GetTemperatureConvertedFromKelvin(element.lowTemp, (TemperatureUnit)0), 2) + "°C";
			string text2 = ((element.lowTempTransitionOreMassConversion > 0f) ? ((1f - element.lowTempTransitionOreMassConversion).ToString("P0") + " ") : "->");
			arg = arg + "<br/>" + text2 + GetTagStringWithIcon(element.lowTempTransition.tag);
			if ((int)element.lowTempTransitionOreID != 758759285 && element.lowTempTransitionOreMassConversion > 0f)
			{
				Tag val = GameTagExtensions.CreateTag(element.lowTempTransitionOreID);
				Element element2 = ElementLoader.GetElement(val);
				if (element.lowTemp < element2.lowTemp && element2.lowTempTransition != null)
				{
					val = element2.lowTempTransition.tag;
				}
				else if (element.lowTemp > element2.highTemp && element2.highTempTransition != null)
				{
					val = element2.highTempTransition.tag;
				}
				arg = arg + ",<br/>" + element.lowTempTransitionOreMassConversion.ToString("P0") + GetTagStringWithIcon(val);
			}
			arg += "<br/>";
		}
		string text3 = "";
		if (element.highTempTransition != null)
		{
			text3 = Math.Round(GameUtil.GetTemperatureConvertedFromKelvin(element.highTemp, (TemperatureUnit)0), 2) + "°C";
			string text4 = ((element.highTempTransitionOreMassConversion > 0f) ? ((1f - element.highTempTransitionOreMassConversion).ToString("P0") + " ") : "->");
			text3 = text3 + "<br/>" + text4 + GetTagStringWithIcon(element.highTempTransition.tag);
			if ((int)element.highTempTransitionOreID != 758759285 && element.highTempTransitionOreMassConversion > 0f)
			{
				Tag val2 = GameTagExtensions.CreateTag(element.highTempTransitionOreID);
				Element element3 = ElementLoader.GetElement(val2);
				if (element.highTemp > element3.highTemp && element3.highTempTransition != null)
				{
					val2 = element3.highTempTransition.tag;
				}
				else if (element.highTemp < element3.lowTemp && element3.lowTempTransition != null)
				{
					val2 = element3.lowTempTransition.tag;
				}
				text3 = text3 + ",<br/>" + element.highTempTransitionOreMassConversion.ToString("P0") + GetTagStringWithIcon(val2);
			}
			text3 += "<br/>";
		}
		string arg2 = GetTagString(element.materialCategory) + "<br/>";
		if (element.highTempTransition != null && element.lowTempTransition == null)
		{
			text += MD_Localization.FormatLineBreaks(string.Format(MD_Localization.L("STRINGS.ELEMENTS.ELEMENTDESCSOLID"), arg2, text3, element.hardness));
		}
		else if (element.highTempTransition != null && element.lowTempTransition != null)
		{
			text += MD_Localization.FormatLineBreaks(string.Format(MD_Localization.L("STRINGS.ELEMENTS.ELEMENTDESCLIQUID"), arg2, arg, text3));
		}
		else if (element.highTempTransition == null && element.lowTempTransition != null)
		{
			text += MD_Localization.FormatLineBreaks(string.Format(MD_Localization.L("STRINGS.ELEMENTS.ELEMENTDESCGAS"), arg2, arg));
		}
		text += "<br/><br/>";
		IEnumerable<Tag> source = from tag in element.oreTags.Distinct()
			where tag != element.materialCategory
			select tag;
		if (source.Any())
		{
			string text5 = string.Join(", ", from text6 in Util.StableSort<string>(source.Select((Tag t) => GetTagString(t)))
				where !text6.Contains("MISSING")
				select text6);
			text += string.Format(MD_Localization.L("STRINGS.ELEMENTS.ELEMENTPROPERTIES"), "<br/>" + text5);
		}
		return text;
	}

	internal static string GetElementPhysicalProperties(Element element)
	{
		string text = "";
		text += MD_Localization.FormatLineBreaks(MD_Localization.L("STRINGS.ELEMENTS.THERMALPROPERTIES").TrimStart('\n').Replace("{SPECIFIC_HEAT_CAPACITY}", "<br/>" + GameUtil.GetFormattedSHC(element.specificHeatCapacity) + "<br/>")
			.Replace("{THERMAL_CONDUCTIVITY}", "<br/>" + GameUtil.GetFormattedThermalConductivity(element.thermalConductivity) + "<br/>"));
		text += "<br/>";
		return text + MD_Localization.FormatLineBreaks(string.Format(MD_Localization.L("STRINGS.ELEMENTS.RADIATIONPROPERTIES"), element.radiationAbsorptionFactor + "<br/>", "<br/>" + GetFormattedRads(element.radiationPer1000Mass * 1.1f / 600f, (TimeSlice)3)));
	}

	public static string FormatRadbolts(int amount)
	{
		return amount + "x " + MD_Localization.L("STRINGS.UI.UNITSUFFIXES.HIGHENERGYPARTICLES.PARTRICLES");
	}

	public static void AppendFormattedMass(StringBuilder builder, float mass, TimeSlice timeSlice = (TimeSlice)0, MetricMassFormat massFormat = (MetricMassFormat)0, bool includeSuffix = true, string floatFormat = "{0:0.#}")
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Invalid comparison between Unknown and I4
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Invalid comparison between Unknown and I4
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected I4, but got Unknown
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		if (mass == float.MinValue)
		{
			builder.Append(MD_Localization.L("STRINGS.UI.CALCULATING"));
			return;
		}
		if (float.IsPositiveInfinity(mass))
		{
			builder.Append(MD_Localization.L("STRINGS.UI.POS_INFINITY"));
			builder.Append(MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.TONNE"));
			return;
		}
		if (float.IsNegativeInfinity(mass))
		{
			builder.Append(MD_Localization.L("STRINGS.UI.NEG_INFINITY"));
			builder.Append(MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.TONNE"));
			return;
		}
		mass = GameUtil.ApplyTimeSlice(mass, timeSlice);
		string value;
		if ((int)GameUtil.massUnit == 0)
		{
			value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.TONNE");
			switch ((int)massFormat)
			{
			case 0:
			{
				float num = Mathf.Abs(mass);
				if (0f < num)
				{
					if (num < 5E-06f)
					{
						value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.MICROGRAM");
						mass = Mathf.Floor(mass * 1E+09f);
					}
					else if (num < 0.005f)
					{
						mass *= 1000000f;
						value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.MILLIGRAM");
					}
					else if (Mathf.Abs(mass) < 5f)
					{
						mass *= 1000f;
						value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.GRAM");
					}
					else if (Mathf.Abs(mass) < 5000f)
					{
						value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.KILOGRAM");
					}
					else
					{
						mass /= 1000f;
						value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.TONNE");
					}
				}
				else
				{
					value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.KILOGRAM");
				}
				break;
			}
			case 1:
				value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.KILOGRAM");
				break;
			case 2:
				mass *= 1000f;
				value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.GRAM");
				break;
			case 3:
				mass /= 1000f;
				value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.TONNE");
				break;
			}
		}
		else
		{
			mass /= 2.2f;
			value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.POUND");
			if ((int)massFormat == 0)
			{
				float num2 = Mathf.Abs(mass);
				if (num2 < 5f && num2 > 0.001f)
				{
					mass *= 256f;
					value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.DRACHMA");
				}
				else
				{
					mass *= 7000f;
					value = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.MASS.GRAIN");
				}
			}
		}
		builder.AppendFormat(floatFormat, mass);
		if (includeSuffix)
		{
			builder.Append(value);
			GameUtil.AddTimeSliceText(builder, timeSlice);
		}
	}

	public static string GetFormattedMass(float mass, TimeSlice timeSlice = (TimeSlice)0, MetricMassFormat massFormat = (MetricMassFormat)0, bool includeSuffix = true, string floatFormat = "{0:0.#}")
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedMass(stringBuilder, mass, timeSlice, massFormat, includeSuffix, floatFormat);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	internal static string GetFormattedMass(Tag material, float amount, TimeSlice slice = (TimeSlice)0, string extraSuffix = "")
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		string tagStringWithIcon = GetTagStringWithIcon(material);
		GameUtil.ApplyTimeSlice(amount, slice);
		string text = GetFormattedMass(amount, (TimeSlice)0, (MetricMassFormat)0);
		if (GameTags.DisplayAsUnits.Contains(material))
		{
			text = "x" + amount;
		}
		if (extraSuffix.Length > 0)
		{
			extraSuffix = " " + extraSuffix;
		}
		text += GetTimeSlice(slice);
		return tagStringWithIcon + " (" + text + extraSuffix + ")";
	}

	private static string GetTimeSlice(TimeSlice slice = (TimeSlice)0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if ((int)slice == 2)
		{
			return MD_Localization.L("STRINGS.UI.UNITSUFFIXES.PERSECOND");
		}
		if ((int)slice == 3)
		{
			return MD_Localization.L("STRINGS.UI.UNITSUFFIXES.PERCYCLE");
		}
		return string.Empty;
	}

	internal static string GetFormattedRads(float amount, TimeSlice slice = (TimeSlice)0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		GameUtil.ApplyTimeSlice(amount, slice);
		string empty = string.Empty;
		empty += amount;
		empty += MD_Localization.L("STRINGS.UI.UNITSUFFIXES.RADIATION.RADS");
		return empty + GetTimeSlice(slice);
	}

	public static string GetFormattedWattage(float watts, WattageFormatterUnit unit = (WattageFormatterUnit)2, bool displayUnits = true)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected I4, but got Unknown
		builder.Clear();
		string text = null;
		switch ((int)unit)
		{
		case 2:
			if (Mathf.Abs(watts) > 1000f)
			{
				watts /= 1000f;
				text = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.ELECTRICAL.KILOWATT");
			}
			else
			{
				text = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.ELECTRICAL.WATT");
			}
			break;
		case 1:
			watts /= 1000f;
			text = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.ELECTRICAL.KILOWATT");
			break;
		case 0:
			text = MD_Localization.L("STRINGS.UI.UNITSUFFIXES.ELECTRICAL.WATT");
			break;
		}
		GameUtil.AppendFloatToString(builder, watts, "###0.##");
		if (displayUnits && text != null)
		{
			builder.Append(text);
		}
		return builder.ToString();
	}
}
