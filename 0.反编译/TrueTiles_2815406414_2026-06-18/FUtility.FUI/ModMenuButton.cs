using System;
using System.Collections;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace FUtility.FUI;

public class ModMenuButton
{
	public static void AddModSettingsButton(object displayedMods, string modName, Action OnClick, string label = "Settings")
	{
		IEnumerator enumerator = ((IEnumerable)displayedMods).GetEnumerator();
		try
		{
			while (enumerator.MoveNext() && !TryAddButton(enumerator.Current, modName, OnClick, label))
			{
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	public static void AddCustomModSettingsButton(object displayedMods, string modPath, Action OnClick, GameObject prefab)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		foreach (object? item in (IEnumerable)displayedMods)
		{
			int value = Traverse.Create(item).Field("mod_index").GetValue<int>();
			Mod mod = Global.Instance.modManager.mods[value];
			if (IsThisMyMod(value, mod, modPath))
			{
				RectTransform value2 = Traverse.Create(item).Field("rect_transform").GetValue<RectTransform>();
				if ((Object)(object)value2 != (Object)null)
				{
					KButton obj = Util.KInstantiateUI<KButton>(prefab, ((Component)((Transform)value2).Find("ManageButton").parent).gameObject, true);
					((KMonoBehaviour)obj).transform.position = Vector3.zero;
					obj.onClick += OnClick;
					((KMonoBehaviour)obj).transform.SetSiblingIndex(value);
				}
			}
		}
	}

	private static bool TryAddButton(object modEntry, string modID, Action action, string label)
	{
		int value = Traverse.Create(modEntry).Field("mod_index").GetValue<int>();
		Mod mod = Global.Instance.modManager.mods[value];
		if (IsThisMyMod(value, mod, modID))
		{
			MakeButton(modEntry, action, label);
			return true;
		}
		return false;
	}

	private static bool IsThisMyMod(int index, Mod mod, string ID)
	{
		if (mod.staticID == ID)
		{
			return mod.IsEnabledForActiveDlc();
		}
		return false;
	}

	private static void MakeButton(object modEntry, Action action, string label)
	{
		RectTransform value = Traverse.Create(modEntry).Field("rect_transform").GetValue<RectTransform>();
		if ((Object)(object)value != (Object)null)
		{
			CloneButton(((Transform)value).Find("ManageButton"), action, label);
		}
	}

	private static void CloneButton(Transform prefab, Action action, string label)
	{
		if (!((Object)(object)prefab == (Object)null))
		{
			Helper.MakeKButton(new Helper.ButtonInfo(LocString.op_Implicit(label), action, 14), ((Component)prefab).gameObject, ((Component)prefab.parent).gameObject, prefab.GetSiblingIndex() - 1);
		}
	}
}
