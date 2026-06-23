using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace FUtility.ElementUtil;

public class ElementUtil
{
	public static readonly Dictionary<SimHashes, string> SimHashNameLookup = new Dictionary<SimHashes, string>();

	public static readonly Dictionary<string, object> ReverseSimHashNameLookup = new Dictionary<string, object>();

	public static readonly List<ElementInfo> elements = new List<ElementInfo>();

	public static SimHashes RegisterSimHash(string name)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		SimHashes val = (SimHashes)Hash.SDBMLower(name);
		SimHashNameLookup.Add(val, name);
		ReverseSimHashNameLookup.Add(name, val);
		return val;
	}

	public unsafe static Substance CreateSubstance(SimHashes id, bool specular, string anim, State state, Color color, Material material, Color uiColor, Color conduitColor, Color? specularColor, string normal)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		KAnimFile val = Assets.Anims.Find((KAnimFile a) => ((Object)a).name == anim);
		if ((Object)(object)val == (Object)null)
		{
			val = Assets.Anims.Find((KAnimFile a) => ((Object)a).name == "glass_kanim");
		}
		Material val2 = new Material(material);
		Log.Assert("newMaterial", val2);
		if ((int)state == 3)
		{
			SetTexture(val2, ((object)(*(SimHashes*)(&id))/*cast due to .constrained prefix*/).ToString().ToLowerInvariant(), "_MainTex");
			if (specular)
			{
				SetTexture(val2, ((object)(*(SimHashes*)(&id))/*cast due to .constrained prefix*/).ToString().ToLowerInvariant() + "_spec", "_ShineMask");
				if (specularColor.HasValue)
				{
					val2.SetColor("_ShineColour", specularColor.Value);
				}
			}
			if (!Util.IsNullOrWhiteSpace(normal))
			{
				SetTexture(val2, normal, "_NormalNoise");
			}
		}
		Substance val3 = ModUtil.CreateSubstance(((object)(*(SimHashes*)(&id))/*cast due to .constrained prefix*/).ToString(), state, val, val2, Color32.op_Implicit(color), Color32.op_Implicit(uiColor), Color32.op_Implicit(conduitColor));
		Log.Debug("created substance");
		Log.Assert("substance", val3);
		return val3;
	}

	private static void SetTexture(Material material, string texture, string property)
	{
		string text = Path.Combine(Utils.ModPath, "assets", "textures", texture + ".png");
		if (FAssets.TryLoadTexture(text, out var texture2))
		{
			material.SetTexture(property, (Texture)(object)texture2);
		}
		else
		{
			Debug.Log((object)("no texture " + text));
		}
	}

	public static void AddModifier(Element element, float decor, float overHeat)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		if (decor != 0f)
		{
			element.attributeModifiers.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).BuildingAttributes.Decor).Id, decor, element.name, true, false, true));
		}
		if (overHeat != 0f)
		{
			element.attributeModifiers.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).BuildingAttributes.OverheatTemperature).Id, overHeat, element.name, false, false, true));
		}
	}

	public static void FixTags()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		FieldInfo fieldInfo = AccessTools.Field(typeof(Substance), "nameTag");
		if (!(fieldInfo != null))
		{
			return;
		}
		foreach (ElementInfo element in elements)
		{
			fieldInfo.SetValue(element.Get().substance, TagManager.Create(((object)element.SimHash/*cast due to .constrained prefix*/).ToString()));
		}
	}

	public static ElementAudioConfig CopyElementAudioConfig(SimHashes referenceId, SimHashes id)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
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
