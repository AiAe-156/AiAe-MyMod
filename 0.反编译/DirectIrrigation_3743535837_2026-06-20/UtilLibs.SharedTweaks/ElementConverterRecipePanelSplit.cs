using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PeterHan.PLib.Core;
using STRINGS;
using UnityEngine;

namespace UtilLibs.SharedTweaks;

public sealed class ElementConverterRecipePanelSplit : PForwardedComponent
{
	private static bool MultiConverter = false;

	private static GameObject _prefab;

	private static ElementConverter[] _converters = Array.Empty<ElementConverter>();

	private static CodexElementMap _usedMap;

	private static CodexElementMap _made;

	public override Version Version => new Version(1, 0, 0, 0);

	public static void Register()
	{
		new ElementConverterRecipePanelSplit().RegisterForForwarding();
	}

	public override void Initialize(Harmony plibInstance)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_005b: Expected O, but got Unknown
		try
		{
			MethodBase methodBase = FindInnerCheckPrefabMethod();
			if (!(methodBase == null))
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(ElementConverterRecipePanelSplit), "CheckValiditiy_Prefix", (Type[])null, (Type[])null);
				MethodInfo methodInfo2 = AccessTools.Method(typeof(ElementConverterRecipePanelSplit), "Transpiler", (Type[])null, (Type[])null);
				plibInstance.Patch(methodBase, new HarmonyMethod(methodInfo), (HarmonyMethod)null, new HarmonyMethod(methodInfo2), (HarmonyMethod)null);
				Debug.Log((object)(GetType().ToString() + " successfully patched"));
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)(GetType().ToString() + " patch failed!"));
			Debug.LogWarning((object)ex.Message);
		}
	}

	public static MethodBase FindInnerCheckPrefabMethod()
	{
		Type typeFromHandle = typeof(CodexEntryGenerator_Elements);
		MethodInfo[] methods = typeFromHandle.GetMethods(AccessTools.all);
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.Name.Contains("CheckPrefab"))
			{
				return methodInfo;
			}
		}
		Debug.LogWarning((object)"CodexEntryGenerator_Elements_GetElementEntryContext_Patch FAILED!\n: failed to find target method for CheckPrefab");
		return null;
	}

	public static void CheckValiditiy_Prefix(GameObject prefab, CodexElementMap usedMap, CodexElementMap made)
	{
		MultiConverter = false;
		BuildingComplete val = default(BuildingComplete);
		if (!prefab.TryGetComponent<BuildingComplete>(ref val))
		{
			return;
		}
		ElementConverter[] components = prefab.GetComponents<ElementConverter>();
		if (components != null && components.Length > 1 && !components.Any((ElementConverter c) => c.inputIsCategory))
		{
			_prefab = prefab;
			_usedMap = usedMap;
			_made = made;
			_converters = (from ec in _prefab.GetComponents<ElementConverter>()
				where ec.consumedElements != null && ec.outputElements != null
				select ec).ToArray();
			MultiConverter = _converters.Length > 1;
		}
	}

	public static IEnumerable<CodeInstruction> Transpiler(ILGenerator _, IEnumerable<CodeInstruction> orig, MethodBase original)
	{
		TranspilerHelper.GetLocIndexOfFirst<ConversionEntry>(original, out var ce1_index);
		MethodInfo redistribute = AccessTools.Method(typeof(ElementConverterRecipePanelSplit), "RedistributeExtraConverters", (Type[])null, (Type[])null);
		FieldInfo ce_outSet = AccessTools.Field(typeof(ConversionEntry), "outSet");
		foreach (CodeInstruction ci in orig)
		{
			if (CodeInstructionExtensions.StoresField(ci, ce_outSet))
			{
				yield return ci;
				yield return new CodeInstruction(OpCodes.Ldloc_S, (object)ce1_index);
				yield return new CodeInstruction(OpCodes.Call, (object)redistribute);
			}
			else
			{
				yield return ci;
			}
		}
	}

	private static void RedistributeExtraConverters(ConversionEntry ce1)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Expected O, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Expected O, but got Unknown
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		if (!MultiConverter || (Object)(object)_prefab == (Object)null || ce1.title != KSelectableExtensions.GetProperName(_prefab) || _converters == null || ce1.inSet == null || ce1.outSet == null)
		{
			return;
		}
		Debug.Log((object)("Splitting merged elementconverter panel for " + UI.StripLinkFormatting(KSelectableExtensions.GetProperName(_prefab)) + " into " + _converters.Length + " panels."));
		for (int num = _converters.Length - 1; num >= 1; num--)
		{
			ElementConverter val = _converters[num];
			if (val.consumedElements != null && val.outputElements != null)
			{
				ConversionEntry val2 = new ConversionEntry();
				val2.title = KSelectableExtensions.GetProperName(_prefab);
				val2.prefab = _prefab;
				val2.inSet = new HashSet<ElementUsage>();
				val2.outSet = new HashSet<ElementUsage>();
				ConsumedElement[] consumedElements = val.consumedElements;
				for (int i = 0; i < consumedElements.Length; i++)
				{
					ConsumedElement inputElement = consumedElements[i];
					ElementUsage val3 = ce1.inSet.LastOrDefault((ElementUsage item) => item.tag == inputElement.Tag && item.amount == inputElement.MassConsumptionRate && item.continuous);
					if (val3 != null)
					{
						ce1.inSet.Remove(val3);
						val2.inSet.Add(new ElementUsage(inputElement.Tag, inputElement.MassConsumptionRate, true));
					}
				}
				OutputElement[] outputElements = val.outputElements;
				for (int num2 = 0; num2 < outputElements.Length; num2++)
				{
					OutputElement outputElement = outputElements[num2];
					ElementUsage val4 = ce1.outSet.LastOrDefault((ElementUsage item) => item.tag == ElementLoader.FindElementByHash(outputElement.elementHash).tag && item.amount == outputElement.massGenerationRate && item.continuous);
					if (val4 != null)
					{
						ce1.outSet.Remove(val4);
						val2.outSet.Add(new ElementUsage(ElementLoader.FindElementByHash(outputElement.elementHash).tag, outputElement.massGenerationRate, true));
					}
				}
				if (val2.inSet.Count > 0 && val2.outSet.Count > 0)
				{
					_usedMap.Add(KPrefabIDExtensions.PrefabID(_prefab), val2);
				}
				foreach (ElementUsage item in val2.inSet)
				{
					_usedMap.Add(item.tag, val2);
				}
				foreach (ElementUsage item2 in val2.outSet)
				{
					_made.Add(item2.tag, val2);
				}
			}
		}
	}
}
