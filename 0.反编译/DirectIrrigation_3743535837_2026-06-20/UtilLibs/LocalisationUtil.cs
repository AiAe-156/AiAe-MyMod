using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using KMod;
using Klei.AI;
using Klei.CustomSettings;
using PeterHan.PLib.Core;
using STRINGS;

namespace UtilLibs;

public static class LocalisationUtil
{
	private static readonly string TranslationsFixedKey = "LocalisationUtil.TranslationsFixed";

	private static Type stringType;

	private const string settingLevelsKey = "LEVELS.";

	private const string generalSettingsRoot = "STRINGS.UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.";

	private static Dictionary<string, Dictionary<string, string>> LocalizedStrings = null;

	private static Dictionary<string, string> CachedTranslationMods = new Dictionary<string, string> { { "de", "929139073" } };

	public static void FixTranslationStrings()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)Localization.GetSelectedLanguageType() != 0 && !PRegistry.GetData<bool>(TranslationsFixedKey))
		{
			PRegistry.PutData(TranslationsFixedKey, true);
			FixRoomConstrains();
			FixSettingsTranslations();
		}
	}

	private static void FixTraitTranslations()
	{
		StringEntry val = default(StringEntry);
		foreach (Trait resource in ((ResourceSet<Trait>)(object)((ModifierSet)Db.Get()).traits).resources)
		{
			string text = ((Resource)resource).Id.ToUpperInvariant();
			if (text.Contains("BASE"))
			{
				continue;
			}
			string text2 = "STRINGS.DUPLICANTS.TRAITS." + text + ".NAME";
			string key = "STRINGS.DUPLICANTS.TRAITS." + text + ".DESC";
			if (!Strings.TryGet(text2, ref val))
			{
				text2 = "STRINGS.DUPLICANTS.TRAITS.NEEDS." + text + ".NAME";
				key = "STRINGS.DUPLICANTS.TRAITS.NEEDS." + text + ".DESC";
				if (!Strings.TryGet(text2, ref val))
				{
					text2 = "STRINGS.DUPLICANTS.TRAITS.CONGENITALTRAITS." + text + ".NAME";
					if (!Strings.TryGet(text2, ref val))
					{
						continue;
					}
					key = "STRINGS.DUPLICANTS.TRAITS.CONGENITALTRAITS." + text + ".DESC";
				}
			}
			if (TryGetTranslatedString(text2, out var translatedString))
			{
				((Resource)resource).Name = translatedString;
			}
			if (TryGetTranslatedString(key, out var translatedString2))
			{
				((Modifier)resource).description = translatedString2;
			}
		}
	}

	private static void FixSettingsTranslations()
	{
		ReapplyTranslatedSettingStrings((SettingConfig)(object)CustomGameSettingConfigs.WorldgenSeed, "WORLDGEN_SEED");
		ReapplyTranslatedSettingStrings((SettingConfig)(object)CustomGameSettingConfigs.ClusterLayout, "CLUSTER_CHOICE");
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.SandboxMode);
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.FastWorkersMode);
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.SaveToCloud);
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.CalorieBurn, "CALORIE_BURN");
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.BionicWattage, "BIONICPOWERUSE");
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.ImmuneSystem);
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.Morale);
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.Durability);
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.Radiation);
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.Stress);
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.StressBreaks, "STRESS_BREAKS");
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.CarePackages);
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.Teleporters);
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.MeteorShowers, null, new Dictionary<string, string> { { "ClearSkies", "CLEAR_SKIES" } });
		ReapplyTranslatedSettingStrings(CustomGameSettingConfigs.DemoliorDifficulty);
	}

	private static void ReapplyTranslatedSettingStrings(SettingConfig config, string settingsStringId = null, Dictionary<string, string> LevelIdOverrides = null)
	{
		if (settingsStringId == null)
		{
			settingsStringId = config.id;
		}
		settingsStringId = settingsStringId.ToUpperInvariant() + ".";
		if (TryGetTranslatedString("STRINGS.UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS." + settingsStringId + "NAME", out var translatedString))
		{
			config.label = translatedString;
		}
		if (TryGetTranslatedString("STRINGS.UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS." + settingsStringId + "TOOLTIP", out var translatedString2))
		{
			config.tooltip = translatedString2;
		}
		ToggleSettingConfig val = (ToggleSettingConfig)(object)((config is ToggleSettingConfig) ? config : null);
		if (val != null)
		{
			List<SettingLevel> list = new List<SettingLevel>(2) { val.off_level, val.on_level };
			{
				foreach (SettingLevel item in list)
				{
					string text = item.id.ToUpperInvariant() + ".";
					if (LevelIdOverrides != null && LevelIdOverrides.TryGetValue(item.id, out var value))
					{
						text = value.ToUpperInvariant() + ".";
					}
					if (TryGetTranslatedString("STRINGS.UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS." + settingsStringId + "LEVELS." + text + "NAME", out var translatedString3))
					{
						item.label = translatedString3;
					}
					if (TryGetTranslatedString("STRINGS.UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS." + settingsStringId + "LEVELS." + text + "TOOLTIP", out var translatedString4))
					{
						item.tooltip = translatedString4;
					}
				}
				return;
			}
		}
		ListSettingConfig val2 = (ListSettingConfig)(object)((config is ListSettingConfig) ? config : null);
		if (val2 == null || val2.levels == null || !val2.levels.Any())
		{
			return;
		}
		foreach (SettingLevel level in val2.levels)
		{
			string text2 = level.id.ToUpperInvariant() + ".";
			if (LevelIdOverrides != null && LevelIdOverrides.TryGetValue(level.id, out var value2))
			{
				text2 = value2.ToUpperInvariant() + ".";
			}
			if (TryGetTranslatedString("STRINGS.UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS." + settingsStringId + "LEVELS." + text2 + "NAME", out var translatedString5))
			{
				level.label = translatedString5;
			}
			if (TryGetTranslatedString("STRINGS.UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS." + settingsStringId + "LEVELS." + text2 + "TOOLTIP", out var translatedString6))
			{
				level.tooltip = translatedString6;
			}
		}
	}

	public static bool TryGetTranslatedString(string key, out string translatedString)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		translatedString = null;
		SelectedLanguageType selectedLanguageType = Localization.GetSelectedLanguageType();
		if ((int)selectedLanguageType == 0)
		{
			return false;
		}
		string currentLanguageCode = Localization.GetCurrentLanguageCode();
		return TryGetTranslatedString(currentLanguageCode, selectedLanguageType, key, out translatedString);
	}

	public static bool TryGetTranslatedString(string languageCode, string key, out string translatedString)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		translatedString = null;
		string text = languageCode + "_klei";
		SelectedLanguageType val = (SelectedLanguageType)0;
		string value;
		if (Localization.PreinstalledLanguages.Contains(text))
		{
			val = (SelectedLanguageType)1;
		}
		else if (CachedTranslationMods.TryGetValue(languageCode, out value))
		{
			val = (SelectedLanguageType)2;
		}
		if ((int)val == 0)
		{
			return false;
		}
		return TryGetTranslatedString(text, val, key, out translatedString);
	}

	public static bool TryGetTranslatedString(string languageCodeKlei, SelectedLanguageType languageType, string key, out string translatedString)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Invalid comparison between Unknown and I4
		string text = languageCodeKlei.Replace("_klei", string.Empty);
		translatedString = null;
		if (LocalizedStrings == null)
		{
			LocalizedStrings = new Dictionary<string, Dictionary<string, string>>();
		}
		if (!LocalizedStrings.TryGetValue(text, out var value))
		{
			if ((int)languageType == 1 && !string.IsNullOrEmpty(languageCodeKlei) && languageCodeKlei != Localization.DEFAULT_LANGUAGE_CODE)
			{
				string preinstalledLocalizationFilePath = Localization.GetPreinstalledLocalizationFilePath(languageCodeKlei);
				if (!File.Exists(preinstalledLocalizationFilePath))
				{
					return false;
				}
				try
				{
					SgtLogger.l("Loading game translations from: " + preinstalledLocalizationFilePath);
					string[] array = File.ReadAllLines(preinstalledLocalizationFilePath, Encoding.UTF8);
					if (!LocalizedStrings.ContainsKey(text))
					{
						LocalizedStrings[text] = new Dictionary<string, string>();
					}
					LocalizedStrings[text].AddRange(Localization.ExtractTranslatedStrings(array, false));
					value = LocalizedStrings[text];
				}
				catch (Exception ex)
				{
					SgtLogger.error("Error while trying to load translations: \n" + ex.Message);
				}
			}
			else if ((int)languageType == 2 && LanguageOptionsScreen.HasInstalledLanguage())
			{
				string savedLanguageMod = LanguageOptionsScreen.GetSavedLanguageMod();
				if (Util.IsNullOrWhiteSpace(savedLanguageMod) && CachedTranslationMods.TryGetValue(languageCodeKlei, out var value2))
				{
					savedLanguageMod = value2;
				}
				try
				{
					Mod val = Global.Instance.modManager.mods.Find((Mod m) => m.label.id == savedLanguageMod);
					if (val == null)
					{
						Debug.LogWarning((object)("Tried loading a translation from a non-existent mod id: " + savedLanguageMod));
						return false;
					}
					string languageFilename = LanguageOptionsScreen.GetLanguageFilename(val);
					if (!File.Exists(languageFilename))
					{
						return false;
					}
					SgtLogger.l("Loading modded translations from: " + languageFilename);
					string[] array2 = File.ReadAllLines(languageFilename, Encoding.UTF8);
					if (!LocalizedStrings.ContainsKey(text))
					{
						LocalizedStrings[text] = new Dictionary<string, string>();
					}
					LocalizedStrings[text].AddRange(Localization.ExtractTranslatedStrings(array2, false));
					value = LocalizedStrings[text];
				}
				catch (Exception ex2)
				{
					SgtLogger.error("Error while trying to load translations: \n" + ex2.Message);
				}
			}
			string text2 = Path.Combine(IO_Utils.ModPath, "translations", text + ".po");
			if (File.Exists(text2))
			{
				try
				{
					SgtLogger.l("Loading local mod translations from: " + text2);
					string[] array3 = File.ReadAllLines(text2, Encoding.UTF8);
					if (!LocalizedStrings.ContainsKey(text))
					{
						LocalizedStrings[text] = new Dictionary<string, string>();
					}
					Dictionary<string, string> dictionary = Localization.ExtractTranslatedStrings(array3, false);
					SgtLogger.l("Extracted " + dictionary.Count + " localizationStrings");
					foreach (KeyValuePair<string, string> item in dictionary)
					{
						string key2 = item.Key;
						int startIndex = key2.IndexOf("STRINGS");
						string key3 = key2.Substring(startIndex);
						LocalizedStrings[text].Add(key3, item.Value);
					}
					value = LocalizedStrings[text];
				}
				catch (Exception ex3)
				{
					SgtLogger.error("Error while trying to load translations: \n" + ex3.Message);
				}
			}
		}
		if (value == null)
		{
			SgtLogger.error("strings was null");
			return false;
		}
		return value.TryGetValue(key, out translatedString);
	}

	public static void FixRoomConstrains()
	{
		RoomConstraints.CEILING_HEIGHT_4.name = string.Format(LocString.op_Implicit(CEILING_HEIGHT.NAME), "4");
		RoomConstraints.CEILING_HEIGHT_4.description = string.Format(LocString.op_Implicit(CEILING_HEIGHT.DESCRIPTION), "4");
		RoomConstraints.CEILING_HEIGHT_6.name = string.Format(LocString.op_Implicit(CEILING_HEIGHT.NAME), "6");
		RoomConstraints.CEILING_HEIGHT_6.description = string.Format(LocString.op_Implicit(CEILING_HEIGHT.DESCRIPTION), "6");
		RoomConstraints.MINIMUM_SIZE_12.name = string.Format(LocString.op_Implicit(MINIMUM_SIZE.NAME), "12");
		RoomConstraints.MINIMUM_SIZE_12.description = string.Format(LocString.op_Implicit(MINIMUM_SIZE.DESCRIPTION), "12");
		RoomConstraints.MINIMUM_SIZE_24.name = string.Format(LocString.op_Implicit(MINIMUM_SIZE.NAME), "24");
		RoomConstraints.MINIMUM_SIZE_24.description = string.Format(LocString.op_Implicit(MINIMUM_SIZE.DESCRIPTION), "24");
		RoomConstraints.MINIMUM_SIZE_32.name = string.Format(LocString.op_Implicit(MINIMUM_SIZE.NAME), "32");
		RoomConstraints.MINIMUM_SIZE_32.description = string.Format(LocString.op_Implicit(MINIMUM_SIZE.DESCRIPTION), "32");
		RoomConstraints.MAXIMUM_SIZE_64.name = string.Format(LocString.op_Implicit(MAXIMUM_SIZE.NAME), "64");
		RoomConstraints.MAXIMUM_SIZE_64.description = string.Format(LocString.op_Implicit(MAXIMUM_SIZE.DESCRIPTION), "64");
		RoomConstraints.MAXIMUM_SIZE_96.name = string.Format(LocString.op_Implicit(MAXIMUM_SIZE.NAME), "96");
		RoomConstraints.MAXIMUM_SIZE_96.description = string.Format(LocString.op_Implicit(MAXIMUM_SIZE.DESCRIPTION), "96");
		RoomConstraints.MAXIMUM_SIZE_120.name = string.Format(LocString.op_Implicit(MAXIMUM_SIZE.NAME), "120");
		RoomConstraints.MAXIMUM_SIZE_120.description = string.Format(LocString.op_Implicit(MAXIMUM_SIZE.DESCRIPTION), "120");
		RoomConstraints.NO_INDUSTRIAL_MACHINERY.name = LocString.op_Implicit(NO_INDUSTRIAL_MACHINERY.NAME);
		RoomConstraints.NO_INDUSTRIAL_MACHINERY.description = LocString.op_Implicit(NO_INDUSTRIAL_MACHINERY.DESCRIPTION);
		RoomConstraints.NO_COTS.name = LocString.op_Implicit(NO_COTS.NAME);
		RoomConstraints.NO_COTS.description = LocString.op_Implicit(NO_COTS.DESCRIPTION);
		RoomConstraints.NO_LUXURY_BEDS.name = LocString.op_Implicit(NO_COTS.NAME);
		RoomConstraints.NO_LUXURY_BEDS.description = LocString.op_Implicit(NO_COTS.DESCRIPTION);
		RoomConstraints.NO_OUTHOUSES.name = LocString.op_Implicit(NO_OUTHOUSES.NAME);
		RoomConstraints.NO_OUTHOUSES.description = LocString.op_Implicit(NO_OUTHOUSES.DESCRIPTION);
		RoomConstraints.NO_MESS_STATION.name = LocString.op_Implicit(NO_MESS_STATION.NAME);
		RoomConstraints.NO_MESS_STATION.description = LocString.op_Implicit(NO_MESS_STATION.DESCRIPTION);
		RoomConstraints.HAS_LUXURY_BED.name = LocString.op_Implicit(HAS_LUXURY_BED.NAME);
		RoomConstraints.HAS_LUXURY_BED.description = LocString.op_Implicit(HAS_LUXURY_BED.DESCRIPTION);
		RoomConstraints.HAS_BED.name = LocString.op_Implicit(HAS_BED.NAME);
		RoomConstraints.HAS_BED.description = LocString.op_Implicit(HAS_BED.DESCRIPTION);
		RoomConstraints.SCIENCE_BUILDINGS.name = LocString.op_Implicit(SCIENCE_BUILDINGS.NAME);
		RoomConstraints.SCIENCE_BUILDINGS.description = LocString.op_Implicit(SCIENCE_BUILDINGS.DESCRIPTION);
		RoomConstraints.BED_SINGLE.name = LocString.op_Implicit(BED_SINGLE.NAME);
		RoomConstraints.BED_SINGLE.description = LocString.op_Implicit(BED_SINGLE.DESCRIPTION);
		RoomConstraints.LUXURY_BED_SINGLE.name = LocString.op_Implicit(LUXURYBEDTYPE.NAME);
		RoomConstraints.LUXURY_BED_SINGLE.description = LocString.op_Implicit(LUXURYBEDTYPE.DESCRIPTION);
		RoomConstraints.BUILDING_DECOR_POSITIVE.name = LocString.op_Implicit(BUILDING_DECOR_POSITIVE.NAME);
		RoomConstraints.BUILDING_DECOR_POSITIVE.description = LocString.op_Implicit(BUILDING_DECOR_POSITIVE.DESCRIPTION);
		RoomConstraints.DECORATIVE_ITEM.name = string.Format(LocString.op_Implicit(DECORATIVE_ITEM.NAME), 1);
		RoomConstraints.DECORATIVE_ITEM.description = string.Format(LocString.op_Implicit(DECORATIVE_ITEM.DESCRIPTION), 1);
		RoomConstraints.DECORATIVE_ITEM_2.name = string.Format(LocString.op_Implicit(DECORATIVE_ITEM.NAME), 2);
		RoomConstraints.DECORATIVE_ITEM_2.description = string.Format(LocString.op_Implicit(DECORATIVE_ITEM.DESCRIPTION), 2);
		RoomConstraints.POWER_STATION.name = LocString.op_Implicit(POWERPLANT.NAME);
		RoomConstraints.POWER_STATION.description = LocString.op_Implicit(POWERPLANT.DESCRIPTION);
		RoomConstraints.FARM_STATION.name = LocString.op_Implicit(FARMSTATIONTYPE.NAME);
		RoomConstraints.FARM_STATION.description = LocString.op_Implicit(FARMSTATIONTYPE.DESCRIPTION);
		RoomConstraints.RANCH_STATION.name = LocString.op_Implicit(RANCHSTATIONTYPE.NAME);
		RoomConstraints.RANCH_STATION.description = LocString.op_Implicit(RANCHSTATIONTYPE.DESCRIPTION);
		RoomConstraints.SPICE_STATION.name = LocString.op_Implicit(SPICESTATION.NAME);
		RoomConstraints.SPICE_STATION.description = LocString.op_Implicit(SPICESTATION.DESCRIPTION);
		RoomConstraints.COOK_TOP.name = LocString.op_Implicit(COOKTOP.NAME);
		RoomConstraints.COOK_TOP.description = LocString.op_Implicit(COOKTOP.DESCRIPTION);
		RoomConstraints.REC_BUILDING.name = LocString.op_Implicit(RECBUILDING.NAME);
		RoomConstraints.REC_BUILDING.description = LocString.op_Implicit(RECBUILDING.DESCRIPTION);
		RoomConstraints.MACHINE_SHOP.name = LocString.op_Implicit(MACHINESHOPTYPE.NAME);
		RoomConstraints.MACHINE_SHOP.description = LocString.op_Implicit(MACHINESHOPTYPE.DESCRIPTION);
		RoomConstraints.DESTRESSING_BUILDING.name = LocString.op_Implicit(DESTRESSINGBUILDING.NAME);
		RoomConstraints.DESTRESSING_BUILDING.description = LocString.op_Implicit(DESTRESSINGBUILDING.DESCRIPTION);
		RoomConstraints.MASSAGE_TABLE.name = LocString.op_Implicit(MASSAGE_TABLE.NAME);
		RoomConstraints.MASSAGE_TABLE.description = LocString.op_Implicit(MASSAGE_TABLE.DESCRIPTION);
		RoomConstraints.DINING_TABLE.name = LocString.op_Implicit(DININGTABLETYPE.NAME);
		RoomConstraints.DINING_TABLE.description = LocString.op_Implicit(DININGTABLETYPE.DESCRIPTION);
		RoomConstraints.MESS_STATION_SINGLE.name = LocString.op_Implicit(NO_BASIC_MESS_STATIONS.NAME);
		RoomConstraints.MESS_STATION_SINGLE.description = LocString.op_Implicit(DININGTABLETYPE.DESCRIPTION);
		RoomConstraints.MULTI_MINION_DINING_TABLE.name = LocString.op_Implicit(MULTI_MINION_DINING_TABLE.NAME);
		RoomConstraints.MULTI_MINION_DINING_TABLE.description = LocString.op_Implicit(MULTI_MINION_DINING_TABLE.DESCRIPTION);
		RoomConstraints.TOILET.name = LocString.op_Implicit(TOILETTYPE.NAME);
		RoomConstraints.TOILET.description = LocString.op_Implicit(TOILETTYPE.DESCRIPTION);
		RoomConstraints.FLUSH_TOILET.name = LocString.op_Implicit(FLUSHTOILETTYPE.NAME);
		RoomConstraints.FLUSH_TOILET.description = LocString.op_Implicit(FLUSHTOILETTYPE.DESCRIPTION);
		RoomConstraints.WASH_STATION.name = LocString.op_Implicit(WASHSTATION.NAME);
		RoomConstraints.WASH_STATION.description = LocString.op_Implicit(WASHSTATION.DESCRIPTION);
		RoomConstraints.ADVANCEDWASHSTATION.name = LocString.op_Implicit(ADVANCEDWASHSTATION.NAME);
		RoomConstraints.ADVANCEDWASHSTATION.description = LocString.op_Implicit(ADVANCEDWASHSTATION.DESCRIPTION);
		RoomConstraints.CLINIC.name = LocString.op_Implicit(CLINIC.NAME);
		RoomConstraints.CLINIC.description = LocString.op_Implicit(CLINIC.DESCRIPTION);
		RoomConstraints.PARK_BUILDING.name = LocString.op_Implicit(PARK.NAME);
		RoomConstraints.PARK_BUILDING.description = LocString.op_Implicit(PARK.DESCRIPTION);
		RoomConstraints.IS_BACKWALLED.name = LocString.op_Implicit(IS_BACKWALLED.NAME);
		RoomConstraints.IS_BACKWALLED.description = LocString.op_Implicit(IS_BACKWALLED.DESCRIPTION);
		RoomConstraints.WILDANIMAL.name = LocString.op_Implicit(WILDANIMAL.NAME);
		RoomConstraints.WILDANIMAL.description = LocString.op_Implicit(WILDANIMAL.DESCRIPTION);
		RoomConstraints.WILDANIMALS.name = LocString.op_Implicit(WILDANIMALS.NAME);
		RoomConstraints.WILDANIMALS.description = LocString.op_Implicit(WILDANIMALS.DESCRIPTION);
		RoomConstraints.WILDPLANT.name = LocString.op_Implicit(WILDPLANT.NAME);
		RoomConstraints.WILDPLANT.description = LocString.op_Implicit(WILDPLANT.DESCRIPTION);
		RoomConstraints.WILDPLANTS.name = LocString.op_Implicit(WILDPLANTS.NAME);
		RoomConstraints.WILDPLANTS.description = LocString.op_Implicit(WILDPLANTS.DESCRIPTION);
		RoomConstraints.ORNAMENTDISPLAYED.name = LocString.op_Implicit(ORNAMENT.NAME);
		RoomConstraints.ORNAMENTDISPLAYED.description = LocString.op_Implicit(ORNAMENT.DESCRIPTION);
	}

	public static void ManualTranslationPatch(Harmony harmony, Type type)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		stringType = type;
		MethodInfo methodInfo = AccessTools.Method("Localization, Assembly-CSharp:Initialize", (Type[])null, (Type[])null);
		MethodInfo methodInfo2 = AccessTools.Method(typeof(LocalisationUtil), "Postfix", (Type[])null, (Type[])null);
		harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo2), (HarmonyMethod)null, (HarmonyMethod)null);
	}

	public static void Postfix()
	{
		if (stringType != null)
		{
			Translate(stringType, generateTemplate: true);
		}
	}

	public static void Translate(Type root, bool generateTemplate = false)
	{
		Localization.RegisterForTranslation(root);
		OverLoadStrings();
		LocString.CreateLocStringKeys(root, (string)null);
		if (generateTemplate)
		{
			string text = Path.Combine(IO_Utils.ModPath, "translations");
			Directory.CreateDirectory(text);
			Localization.GenerateStringsTemplate(root, Path.Combine(Manager.GetDirectory(), "strings_templates"));
			Localization.GenerateStringsTemplate(root.Namespace, Assembly.GetExecutingAssembly(), Path.Combine(IO_Utils.ModPath, "translation_template.pot"), (Dictionary<string, object>)null);
			Localization.GenerateStringsTemplate(root.Namespace, Assembly.GetExecutingAssembly(), Path.Combine(text, "translation_template.pot"), (Dictionary<string, object>)null);
		}
	}

	private static void OverLoadStrings()
	{
		Locale locale = Localization.GetLocale();
		string text = ((locale != null) ? locale.Code : null);
		if (!Util.IsNullOrWhiteSpace(text))
		{
			string text2 = Path.Combine(UtilMethods.ModPath, "translations", Localization.GetLocale().Code + ".po");
			if (File.Exists(text2))
			{
				Localization.OverloadStrings(Localization.LoadStringsFile(text2, false));
				Debug.Log((object)("Found translation file for " + text + "."));
			}
		}
	}
}
