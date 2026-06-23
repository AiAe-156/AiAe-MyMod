using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Klei.AI;
using UnityEngine;
using UtilLibs;

namespace ElementUtilNamespace;

public class SgtElementUtil
{
	public static readonly Dictionary<SimHashes, string> SimHashNameLookup = new Dictionary<SimHashes, string>();

	public static readonly Dictionary<string, object> ReverseSimHashNameLookup = new Dictionary<string, object>();

	public static readonly List<ElementInfo> Elements = new List<ElementInfo>();

	public static void ExecuteElementEnumPatches(Harmony harmony)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		SgtLogger.l("Attempting to patch Enum.ToString's internal InternalFormat method...");
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(Enum), "InternalFormat", (Type[])null, (Type[])null);
			HarmonyMethod val = new HarmonyMethod(typeof(SgtElementUtil), "SimHashInternalFormat_EnumPatch", (Type[])null);
			harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, val, (HarmonyMethod)null, (HarmonyMethod)null);
		}
		catch (Exception ex)
		{
			SgtLogger.error("Error while patching Enum.InternalFormat:\n" + ex);
		}
		SgtLogger.l("Attempting to patch Enum.Parse...");
		try
		{
			MethodInfo methodInfo2 = AccessTools.Method(typeof(Enum), "Parse", new Type[3]
			{
				typeof(Type),
				typeof(string),
				typeof(bool)
			}, (Type[])null);
			HarmonyMethod val2 = new HarmonyMethod(typeof(SgtElementUtil), "SimhashParse_EnumPatch", (Type[])null);
			harmony.Patch((MethodBase)methodInfo2, (HarmonyMethod)null, val2, (HarmonyMethod)null, (HarmonyMethod)null);
		}
		catch (Exception ex2)
		{
			SgtLogger.error("Error while patching Enum.Parse:\n" + ex2);
			return;
		}
		SgtLogger.l("Element enum patches successful!");
	}

	public static void SimHashInternalFormat_EnumPatch(Type eT, object value, ref string __result)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (eT == typeof(SimHashes) && SimHashNameLookup.TryGetValue((SimHashes)value, out var value2))
		{
			__result = value2;
		}
	}

	public static bool SimHashToString_EnumPatch(Enum __instance, ref string __result)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (__instance is SimHashes key)
		{
			return !SimHashNameLookup.TryGetValue(key, out __result);
		}
		return true;
	}

	public static void SimhashParse_EnumPatch(Type enumType, string value, ref object __result)
	{
		if (enumType == typeof(SimHashes) && ReverseSimHashNameLookup.TryGetValue(value, out var value2))
		{
			__result = value2;
		}
	}

	public unsafe static SimHashes RegisterSimHash(string name)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		SimHashes val = (SimHashes)Hash.SDBMLower(name);
		SgtLogger.l("Element: " + name + ", simhash " + ((object)(*(SimHashes*)(&val))/*cast due to .constrained prefix*/).ToString());
		SimHashNameLookup.Add(val, name);
		ReverseSimHashNameLookup.Add(name, val);
		return val;
	}

	public static void SetTexture_Main(Material material, string texture)
	{
		SetTexture(material, texture, "_MainTex");
	}

	public static void SetTexture_ShineMask(Material material, string texture, Color? specularColor = null)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		SetTexture(material, texture, "_ShineMask");
		if (specularColor.HasValue)
		{
			material.SetColor("_ShineColour", specularColor.Value);
		}
	}

	public static void SetTexture_NormalNoise(Material material, string normal)
	{
		SetTexture(material, normal, "_NormalNoise");
	}

	public unsafe static Substance CreateSubstance(SimHashes id, bool specular, string anim, State state, Color color, Material material, Color uiColor, Color conduitColor, Color? specularColor, string normal, bool isCloned = false)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Invalid comparison between Unknown and I4
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		KAnimFile val = Assets.Anims.Find((KAnimFile a) => ((Object)a).name == anim);
		if ((Object)(object)val == (Object)null)
		{
			val = Assets.Anims.Find((KAnimFile a) => ((Object)a).name == "glass_kanim");
		}
		Material val2 = new Material(material);
		if ((int)state == 3 && !isCloned)
		{
			SetTexture_Main(val2, ((object)(*(SimHashes*)(&id))/*cast due to .constrained prefix*/).ToString().ToLowerInvariant());
			if (specular)
			{
				SetTexture_ShineMask(val2, ((object)(*(SimHashes*)(&id))/*cast due to .constrained prefix*/).ToString().ToLowerInvariant() + "_spec", specularColor);
			}
			if (!Util.IsNullOrWhiteSpace(normal))
			{
				SetTexture_NormalNoise(val2, normal);
			}
		}
		Substance val3 = ModUtil.CreateSubstance(((object)(*(SimHashes*)(&id))/*cast due to .constrained prefix*/).ToString(), state, val, val2, Color32.op_Implicit(color), Color32.op_Implicit(uiColor), Color32.op_Implicit(conduitColor));
		val3.anims = (KAnimFile[])(object)new KAnimFile[1] { val };
		return val3;
	}

	private static void SetTexture(Material material, string texture, string property)
	{
		string path = Path.Combine(UtilMethods.ModPath, "assets", "textures", texture + ".png");
		if (TryLoadTexture(path, out var texture2))
		{
			material.SetTexture(property, (Texture)(object)texture2);
		}
	}

	public static bool TryLoadTexture(string path, out Texture2D texture)
	{
		texture = LoadTexture(path);
		return (Object)(object)texture != (Object)null;
	}

	public static Texture2D LoadTexture(string path, bool warnIfFailed = true)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		Texture2D val = null;
		if (File.Exists(path))
		{
			byte[] array = TryReadFile(path);
			val = new Texture2D(1, 1);
			ImageConversion.LoadImage(val, array);
		}
		else if (warnIfFailed)
		{
			SgtLogger.dlogwarn("Could not load texture at path " + path + ".");
		}
		return val;
	}

	public static byte[] TryReadFile(string texFile)
	{
		try
		{
			return File.ReadAllBytes(texFile);
		}
		catch (Exception ex)
		{
			SgtLogger.dlogwarn("Could not read file: " + ex);
			return null;
		}
	}

	public static void AddModifier(Element element, float decor, float overHeat)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		if (decor != 0f)
		{
			element.attributeModifiers.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).BuildingAttributes.Decor).Id, decor, element.name, true, false, true));
		}
		if (overHeat != 0f)
		{
			element.attributeModifiers.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).BuildingAttributes.OverheatTemperature).Id, overHeat, element.name, false, false, true));
		}
	}

	public static ElementAudioConfig GetCrystalAudioConfig(SimHashes id)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		ElementAudioConfig configForElement = ElementsAudio.Instance.GetConfigForElement((SimHashes)(-2123557039));
		return new ElementAudioConfig
		{
			elementID = id,
			ambienceType = (AmbienceType)(-1),
			solidAmbienceType = (SolidAmbienceType)10,
			miningSound = "PhosphateNodule",
			miningBreakSound = configForElement.miningBreakSound,
			oreBumpSound = configForElement.oreBumpSound,
			floorEventAudioCategory = "tileglass",
			creatureChewSound = configForElement.creatureChewSound
		};
	}

	public static ElementAudioConfig CopyElementAudioConfig(ElementAudioConfig reference, SimHashes id)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		return new ElementAudioConfig
		{
			elementID = reference.elementID,
			ambienceType = reference.ambienceType,
			solidAmbienceType = reference.solidAmbienceType,
			miningSound = reference.miningSound,
			miningBreakSound = reference.miningBreakSound,
			oreBumpSound = reference.oreBumpSound,
			floorEventAudioCategory = reference.floorEventAudioCategory,
			creatureChewSound = reference.creatureChewSound
		};
	}

	public static ElementAudioConfig CopyElementAudioConfig(SimHashes referenceId, SimHashes id)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		ElementAudioConfig configForElement = ElementsAudio.Instance.GetConfigForElement(referenceId);
		return new ElementAudioConfig
		{
			elementID = configForElement.elementID,
			ambienceType = configForElement.ambienceType,
			solidAmbienceType = configForElement.solidAmbienceType,
			miningSound = configForElement.miningSound,
			miningBreakSound = configForElement.miningBreakSound,
			oreBumpSound = configForElement.oreBumpSound,
			floorEventAudioCategory = configForElement.floorEventAudioCategory,
			creatureChewSound = configForElement.creatureChewSound
		};
	}
}
