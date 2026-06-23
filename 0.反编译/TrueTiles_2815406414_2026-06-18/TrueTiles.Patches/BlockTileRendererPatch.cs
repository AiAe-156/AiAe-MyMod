using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Rendering;
using UnityEngine;

namespace TrueTiles.Patches;

public class BlockTileRendererPatch
{
	[HarmonyPatch(typeof(BlockTileRenderer), "GetConnectionBits")]
	public class BlockTileRenderer_GetConnectionBits_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> orig)
		{
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Expected O, but got Unknown
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Expected O, but got Unknown
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Expected O, but got Unknown
			List<CodeInstruction> list = orig.ToList();
			MethodInfo method = typeof(BlockTileRenderer).GetMethod("MatchesDef", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo method2 = typeof(BlockTileRenderer_GetConnectionBits_Patch).GetMethod("MatchesElement", new Type[4]
			{
				typeof(bool),
				typeof(int),
				typeof(int),
				typeof(int)
			});
			for (int i = 0; i < list.Count; i++)
			{
				CodeInstruction val = list[i];
				if (val.opcode == OpCodes.Call && val.operand is MethodInfo methodInfo && methodInfo == method)
				{
					list.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_1, (object)null));
					list.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_2, (object)null));
					list.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_3, (object)null));
					list.Insert(i + 4, new CodeInstruction(OpCodes.Call, (object)method2));
				}
			}
			return list;
		}

		public static bool MatchesElement(bool matchesDef, int x, int y, int layer)
		{
			if (!matchesDef || lastCheckedCell == -1)
			{
				return false;
			}
			if (layer == 11)
			{
				return true;
			}
			int num = Grid.XYToCell(x, y);
			return ElementGrid.elementIdx[num] == ElementGrid.elementIdx[lastCheckedCell];
		}
	}

	[HarmonyPatch(typeof(BlockTileRenderer), "MatchesDef")]
	public class BlockTileRenderer_MatchesDef_Patch
	{
		public static void Prefix(GameObject go, BuildingDef def, ref bool __result)
		{
			if ((Object)(object)go == (Object)null)
			{
				lastCheckedCell = -1;
			}
			else
			{
				lastCheckedCell = Grid.PosToCell(go);
			}
		}
	}

	[HarmonyPatch(typeof(BlockTileRenderer), "AddBlock")]
	public static class Rendering_BlockTileRenderer_AddBlock_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> orig)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected O, but got Unknown
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			List<CodeInstruction> list = orig.ToList();
			int num = FindEntryPoint(list);
			if (num == -1)
			{
				return list;
			}
			num++;
			list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_2, (object)null));
			list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_S, (object)4));
			list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)GetRenderLayerForTileMethod));
			return list;
		}
	}

	[HarmonyPatch(typeof(BlockTileRenderer), "RemoveBlock")]
	public static class Rendering_BlockTileRenderer_RemoveBlock_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> orig)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			List<CodeInstruction> list = orig.ToList();
			int num = FindEntryPoint(list);
			if (num == -1)
			{
				return list;
			}
			num++;
			list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_1, (object)null));
			list.Insert(num++, new CodeInstruction(OpCodes.Ldarg_3, (object)null));
			list.Insert(num++, new CodeInstruction(OpCodes.Call, (object)GetRenderLayerForTileMethod));
			return list;
		}
	}

	public static MethodInfo GetRenderInfoLayerMethod;

	public static MethodInfo GetRenderLayerForTileMethod;

	private const int OFFSET = 451;

	private static int lastCheckedCell = -1;

	private static int FindEntryPoint(List<CodeInstruction> codes)
	{
		return codes.FindIndex((CodeInstruction c) => c.operand is MethodInfo methodInfo && methodInfo == GetRenderInfoLayerMethod);
	}

	internal static RenderInfoLayer GetRenderLayerForTile(RenderInfoLayer layer, BuildingDef def, SimHashes elementId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)layer != 0 || !KPrefabIDExtensions.HasTag(def.BuildingComplete, ModAssets.Tags.texturedTile))
		{
			return layer;
		}
		return (RenderInfoLayer)(elementId + 451);
	}
}
