using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

/// <summary>
/// Patches bugs in Text Mesh Pro.
/// </summary>
internal static class TextMeshProPatcher
{
	/// <summary>
	/// The ID to use for Harmony patches.
	/// </summary>
	private const string HARMONY_ID = "TextMeshProPatch";

	/// <summary>
	/// Tracks whether the TMP patches have been checked.
	/// </summary>
	private static volatile bool patchChecked = false;

	/// <summary>
	/// Serializes multiple thread access to the patch status.
	/// </summary>
	private static readonly object patchLock = new object();

	/// <summary>
	/// Applied to TMP_InputField to fix a bug that prevented auto layout from ever
	/// working.
	/// </summary>
	private static bool AssignPositioningIfNeeded_Prefix(TMP_InputField __instance, RectTransform ___caretRectTrans, TMP_Text ___m_TextComponent)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		if ((Object)(object)___m_TextComponent != (Object)null && (Object)(object)___caretRectTrans != (Object)null && (Object)(object)__instance != (Object)null && ((Behaviour)___m_TextComponent).isActiveAndEnabled)
		{
			RectTransform rectTransform = ___m_TextComponent.rectTransform;
			if (((Transform)___caretRectTrans).localPosition != ((Transform)rectTransform).localPosition || ((Transform)___caretRectTrans).localRotation != ((Transform)rectTransform).localRotation || ((Transform)___caretRectTrans).localScale != ((Transform)rectTransform).localScale || ___caretRectTrans.anchorMin != rectTransform.anchorMin || ___caretRectTrans.anchorMax != rectTransform.anchorMax || ___caretRectTrans.anchoredPosition != rectTransform.anchoredPosition || ___caretRectTrans.sizeDelta != rectTransform.sizeDelta || ___caretRectTrans.pivot != rectTransform.pivot)
			{
				((MonoBehaviour)__instance).StartCoroutine(ResizeCaret(___caretRectTrans, rectTransform));
				result = false;
			}
		}
		return result;
	}

	/// <summary>
	/// Checks to see if a patch with our class name has already been applied.
	/// </summary>
	/// <param name="patchList">The patch list to search.</param>
	/// <returns>true if a patch with this class has already patched the method, or false otherwise.</returns>
	private static bool HasOurPatch(IEnumerable<Patch> patchList)
	{
		bool result = false;
		if (patchList != null)
		{
			foreach (Patch patch in patchList)
			{
				string text = patch.PatchMethod?.DeclaringType?.Name;
				if (text == "TextMeshProPatcher" || text == "PLibPatches")
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	/// <summary>
	/// Patches TMP_InputField with fixes, but only if necessary.
	/// </summary>
	/// <param name="tmpType">The type of TMP_InputField.</param>
	private static void InputFieldPatches(Type tmpType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		Harmony val = new Harmony("TextMeshProPatch");
		MethodInfo methodSafe = tmpType.GetMethodSafe("AssignPositioningIfNeeded", isStatic: false, PPatchTools.AnyArguments);
		if (methodSafe != null && !HasOurPatch(Harmony.GetPatchInfo((MethodBase)methodSafe)?.Prefixes))
		{
			val.Patch((MethodBase)methodSafe, new HarmonyMethod(typeof(TextMeshProPatcher), "AssignPositioningIfNeeded_Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}
		MethodInfo methodSafe2 = tmpType.GetMethodSafe("OnEnable", isStatic: false, PPatchTools.AnyArguments);
		if (methodSafe2 != null && !HasOurPatch(Harmony.GetPatchInfo((MethodBase)methodSafe2)?.Postfixes))
		{
			val.Patch((MethodBase)methodSafe2, (HarmonyMethod)null, new HarmonyMethod(typeof(TextMeshProPatcher), "OnEnable_Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
		}
	}

	/// <summary>
	/// Applied to TMPro.TMP_InputField to fix a clipping bug inside of Scroll Rects.
	///
	/// https://forum.unity.com/threads/textmeshpro-text-still-visible-when-using-nested-rectmask2d.537967/
	/// </summary>
	private static void OnEnable_Postfix(Scrollbar ___m_VerticalScrollbar, TMP_Text ___m_TextComponent)
	{
		if ((Object)(object)___m_TextComponent != (Object)null)
		{
			___m_TextComponent.ignoreRectMaskCulling = (Object)(object)___m_VerticalScrollbar != (Object)null;
		}
	}

	/// <summary>
	/// Patches Text Mesh Pro input fields to fix a variety of bugs. Should be used before
	/// any Text Mesh Pro objects are created.
	/// </summary>
	public static void Patch()
	{
		lock (patchLock)
		{
			if (patchChecked)
			{
				return;
			}
			Type typeSafe = PPatchTools.GetTypeSafe("TMPro.TMP_InputField");
			if (typeSafe != null)
			{
				try
				{
					InputFieldPatches(typeSafe);
				}
				catch (Exception)
				{
					PUtil.LogWarning("Unable to patch TextMeshPro bug, text fields may display improperly inside scroll areas");
				}
			}
			patchChecked = true;
		}
	}

	/// <summary>
	/// Resizes the caret object to match the text. Used as an enumerator.
	/// </summary>
	/// <param name="caretTransform">The rectTransform of the caret.</param>
	/// <param name="textTransform">The rectTransform of the text.</param>
	private static IEnumerator ResizeCaret(RectTransform caretTransform, RectTransform textTransform)
	{
		yield return (object)new WaitForEndOfFrame();
		((Transform)caretTransform).localPosition = ((Transform)textTransform).localPosition;
		((Transform)caretTransform).localRotation = ((Transform)textTransform).localRotation;
		((Transform)caretTransform).localScale = ((Transform)textTransform).localScale;
		caretTransform.anchorMin = textTransform.anchorMin;
		caretTransform.anchorMax = textTransform.anchorMax;
		caretTransform.anchoredPosition = textTransform.anchoredPosition;
		caretTransform.sizeDelta = textTransform.sizeDelta;
		caretTransform.pivot = textTransform.pivot;
	}
}
