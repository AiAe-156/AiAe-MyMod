using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using STRINGS;
using UnityEngine;

namespace UtilLibs.SharedTweaks;

public sealed class ElementConverterDescriptionImprovement : PForwardedComponent
{
	public override Version Version => new Version(1, 0, 0, 0);

	public static void Register()
	{
		new ElementConverterDescriptionImprovement().RegisterForForwarding();
	}

	public override void Initialize(Harmony plibInstance)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(ElementConverter), "GetDescriptors", (Type[])null, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(ElementConverterDescriptionImprovement), "ReplaceElementConverterDescriptorsPrefix", (Type[])null, (Type[])null);
			plibInstance.Patch((MethodBase)methodInfo, new HarmonyMethod(methodInfo2), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			Debug.Log((object)(GetType().ToString() + " successfully patched"));
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)(GetType().ToString() + " patch failed!"));
			Debug.LogWarning((object)ex.Message);
		}
	}

	public static bool ReplaceElementConverterDescriptorsPrefix(ElementConverter __instance, GameObject go, ref List<Descriptor> __result)
	{
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		ElementConverter[] components = go.GetComponents<ElementConverter>();
		ElementConverter[] array = components;
		foreach (ElementConverter val in array)
		{
			if (val.showDescriptors && val.consumedElements != null)
			{
				num++;
			}
		}
		if (!__instance.showDescriptors || num < 2 || __instance.consumedElements == null || !__instance.consumedElements.Any())
		{
			return true;
		}
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string format = "{0} ({1})";
		if (__instance.consumedElements != null)
		{
			ConsumedElement[] consumedElements = __instance.consumedElements;
			for (int j = 0; j < consumedElements.Length; j++)
			{
				ConsumedElement val2 = consumedElements[j];
				if (!Util.IsNullOrWhiteSpace(text))
				{
					text += ", ";
				}
				if (!Util.IsNullOrWhiteSpace(text3))
				{
					text3 += "\n";
				}
				text += string.Format(format, ((ConsumedElement)(ref val2)).Name, GameUtil.GetFormattedMass(val2.MassConsumptionRate, (TimeSlice)2, (MetricMassFormat)0, true, "{0:0.##}"));
				text3 += string.Format(LocString.op_Implicit(TOOLTIPS.ELEMENTCONSUMED), ((ConsumedElement)(ref val2)).Name, GameUtil.GetFormattedMass(val2.MassConsumptionRate, (TimeSlice)2, (MetricMassFormat)0, true, "{0:0.##}"));
			}
			text3 += "\n\n";
		}
		if (__instance.outputElements != null)
		{
			OutputElement[] outputElements = __instance.outputElements;
			for (int k = 0; k < outputElements.Length; k++)
			{
				OutputElement val3 = outputElements[k];
				if (!Util.IsNullOrWhiteSpace(text2))
				{
					text2 += ", ";
				}
				if (val3.IsActive)
				{
					LocString val4 = TOOLTIPS.ELEMENTEMITTED_INPUTTEMP;
					if (val3.useEntityTemperature)
					{
						val4 = TOOLTIPS.ELEMENTEMITTED_ENTITYTEMP;
					}
					else if (val3.minOutputTemperature > 0f)
					{
						val4 = TOOLTIPS.ELEMENTEMITTED_MINTEMP;
					}
					text2 += string.Format(format, ((OutputElement)(ref val3)).Name, GameUtil.GetFormattedMass(val3.massGenerationRate, (TimeSlice)2, (MetricMassFormat)0, true, "{0:0.##}"), GameUtil.GetFormattedTemperature(val3.minOutputTemperature, (TimeSlice)0, (TemperatureInterpretation)0, true, false));
					text3 += string.Format(val4.Replace("\n\n", "\n"), ((OutputElement)(ref val3)).Name, GameUtil.GetFormattedMass(val3.massGenerationRate, (TimeSlice)2, (MetricMassFormat)0, true, "{0:0.##}"), GameUtil.GetFormattedTemperature(val3.minOutputTemperature, (TimeSlice)0, (TemperatureInterpretation)0, true, false));
					text3 += "\n";
				}
			}
		}
		string text4 = string.Format(LocString.op_Implicit(CRAFTINGTABLE.RECIPE_DESCRIPTION), text, text2);
		Descriptor item = default(Descriptor);
		((Descriptor)(ref item)).SetupDescriptor(text4, text3, (DescriptorType)1);
		__result = new List<Descriptor>(1) { item };
		return false;
	}
}
