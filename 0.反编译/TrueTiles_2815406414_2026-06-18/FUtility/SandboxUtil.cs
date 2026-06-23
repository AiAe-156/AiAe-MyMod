using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace FUtility;

public class SandboxUtil
{
	public static SearchFilter CreateSimpleTagFilter(SandboxToolParameterMenu menu, string name, Tag tag, string prefabIDToUseForIcon, string parentFilterID = null)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		Tuple<Sprite, Color> uISprite = Def.GetUISprite((object)Assets.GetPrefab(Tag.op_Implicit(prefabIDToUseForIcon)), "ui", false);
		SearchFilter val = FindParent(menu, parentFilterID);
		return new SearchFilter(name, (Func<object, bool>)delegate(object obj)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			KPrefabID val2 = (KPrefabID)((obj is KPrefabID) ? obj : null);
			return val2 != null && val2.HasTag(tag);
		}, val, uISprite);
	}

	public static SearchFilter AddModMenu(SandboxToolParameterMenu menu, string name, Sprite icon, Func<object, bool> condition)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return AddModMenu(menu, name, new Tuple<Sprite, Color>(icon, Color.white), condition);
	}

	public static SearchFilter AddModMenu(SandboxToolParameterMenu menu, string name, Tuple<Sprite, Color> icon, Func<object, bool> condition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		SearchFilter val = AddOrGetModsMenu(menu);
		SearchFilter val2 = new SearchFilter(name, condition, val, icon);
		menu.entitySelector.filters = CollectionExtensions.AddToArray<SearchFilter>(menu.entitySelector.filters, val2);
		return val2;
	}

	public static SearchFilter AddOrGetModsMenu(SandboxToolParameterMenu menu)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		Color32 color = new Color32((byte)254, (byte)254, (byte)254, byte.MaxValue);
		int num = menu.entitySelector.filters.ToList().FindIndex(delegate(SearchFilter x)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			if (x.icon != null)
			{
				Sprite first = x.icon.first;
				if (((first != null) ? ((Object)first).name : null) == "mod_machinery")
				{
					return ((Color)(ref x.icon.second)).Equals(Color32.op_Implicit(color));
				}
			}
			return false;
		});
		new List<SearchFilter>();
		if (num == -1)
		{
			Sprite sprite = Assets.GetSprite(HashedString.op_Implicit("mod_machinery"));
			SearchFilter val = new SearchFilter(LocString.op_Implicit(MODS.TITLE), (Func<object, bool>)((object _) => false), (SearchFilter)null, new Tuple<Sprite, Color>(sprite, Color32.op_Implicit(color)));
			menu.entitySelector.filters = CollectionExtensions.AddToArray<SearchFilter>(menu.entitySelector.filters, val);
			return val;
		}
		return menu.entitySelector.filters[num];
	}

	public static void AddFilters(SandboxToolParameterMenu menu, params SearchFilter[] newFilters)
	{
		SearchFilter[] filters = menu.entitySelector.filters;
		if (filters == null)
		{
			Log.Warning("Filters are null");
		}
		else
		{
			List<SearchFilter> list = new List<SearchFilter>(filters);
			list.AddRange(newFilters);
			menu.entitySelector.filters = list.ToArray();
		}
	}

	public static void UpdateOptions(SandboxToolParameterMenu menu)
	{
		SearchFilter[] filters = menu.entitySelector.filters;
		if (filters == null)
		{
			return;
		}
		PooledList<object, SandboxToolParameterMenu> val = ListPool<object, SandboxToolParameterMenu>.Allocate();
		foreach (KPrefabID prefab in Assets.Prefabs)
		{
			SearchFilter[] array = filters;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].condition(prefab))
				{
					((List<object>)(object)val).Add((object)prefab);
					break;
				}
			}
		}
		menu.entitySelector.options = ((List<object>)(object)val).ToArray();
		val.Recycle();
	}

	private static SearchFilter FindParent(SandboxToolParameterMenu menu, string parentFilterID)
	{
		if (parentFilterID == null)
		{
			return null;
		}
		return menu.entitySelector.filters.First((SearchFilter x) => x.Name == parentFilterID);
	}
}
