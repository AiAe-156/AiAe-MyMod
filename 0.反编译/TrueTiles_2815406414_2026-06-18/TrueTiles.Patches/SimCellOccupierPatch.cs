using System;
using HarmonyLib;
using UnityEngine;

namespace TrueTiles.Patches;

public class SimCellOccupierPatch
{
	[HarmonyPatch(typeof(SimCellOccupier), "OnSpawn")]
	public class SimCellOccupier_OnSpawn_Patch
	{
		public static void Postfix(SimCellOccupier __instance)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			if (!KPrefabIDExtensions.HasTag((Component)(object)__instance, ModAssets.Tags.texturedTile))
			{
				return;
			}
			int cell = Grid.PosToCell((KMonoBehaviour)(object)__instance);
			PrimaryElement component = ((Component)__instance).GetComponent<PrimaryElement>();
			if (component != null)
			{
				ElementGrid.Add(cell, component.ElementID);
			}
			if (!__instance.doReplaceElement)
			{
				GameScheduler.Instance.ScheduleNextFrame("refresh cell", (Action<object>)delegate
				{
					RefreshCell(cell);
				}, (object)null, (SchedulerGroup)null);
			}
		}

		private static void RefreshCell(int cell)
		{
			TileVisualizer.RefreshCell(cell, (ObjectLayer)9, (ObjectLayer)11);
		}
	}

	[HarmonyPatch(typeof(SimCellOccupier), "OnModifyComplete")]
	public class SimCellOccupier_OnModifyComplete_Patch
	{
		public static void Postfix(SimCellOccupier __instance)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (__instance.doReplaceElement && KPrefabIDExtensions.HasTag((Component)(object)__instance, ModAssets.Tags.texturedTile))
			{
				TileVisualizer.RefreshCell(Grid.PosToCell((KMonoBehaviour)(object)__instance), (ObjectLayer)9, (ObjectLayer)11);
			}
		}
	}

	[HarmonyPatch(typeof(SimCellOccupier), "OnCleanUp")]
	public class SimCellOccupier_OnCleanup_Patch
	{
		public static void Prefix(SimCellOccupier __instance)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if (KPrefabIDExtensions.HasTag((Component)(object)__instance, ModAssets.Tags.texturedTile))
			{
				ElementGrid.Remove(Grid.PosToCell((KMonoBehaviour)(object)__instance));
			}
		}
	}
}
