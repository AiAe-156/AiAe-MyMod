using System;
using System.Collections.Generic;
using System.Reflection;
using Database;
using HarmonyLib;
using PeterHan.PLib.Core;
using STRINGS;
using UnityEngine;

namespace UtilLibs.BuildingPortUtils;

public static class ConduitDisplayPortPatching
{
	private static class PortInfoDrawing
	{
		private static bool AddAny = false;

		private static string portText = "";

		private static Sprite portSprite;

		private static Color portColor = Color.white;

		private static TextStyleSetting textStyle = null;

		public static void PatchAll(Harmony harmony)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Expected O, but got Unknown
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Expected O, but got Unknown
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Expected O, but got Unknown
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Expected O, but got Unknown
			MethodInfo methodInfo = AccessTools.Method(typeof(EntityCellVisualizer), "DrawIcons", (Type[])null, (Type[])null);
			harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(typeof(PortInfoDrawing), "EntityCellVisualizer_DrawIcons_Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(SelectToolHoverTextCard), "UpdateHoverElements", (Type[])null, (Type[])null);
			MethodInfo methodInfo3 = AccessTools.Method(typeof(BuildToolHoverTextCard), "UpdateHoverElements", (Type[])null, (Type[])null);
			harmony.Patch((MethodBase)methodInfo2, new HarmonyMethod(typeof(PortInfoDrawing), "HoverTextConfiguration_UpdateHoverElements_Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			harmony.Patch((MethodBase)methodInfo3, new HarmonyMethod(typeof(PortInfoDrawing), "HoverTextConfiguration_UpdateHoverElements_Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			MethodInfo methodInfo4 = AccessTools.Method(typeof(HoverTextDrawer), "EndDrawing", (Type[])null, (Type[])null);
			harmony.Patch((MethodBase)methodInfo4, new HarmonyMethod(typeof(PortInfoDrawing), "HoverTextDrawer_EndDrawing_Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}

		private static void EntityCellVisualizer_DrawIcons_Postfix(EntityCellVisualizer __instance, HashedString mode)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			BuildingCellVisualizer val = (BuildingCellVisualizer)(object)((__instance is BuildingCellVisualizer) ? __instance : null);
			if (val != null)
			{
				PortDisplayController.HandleVanillaPortInfo(val, mode);
			}
		}

		private static void HoverTextConfiguration_UpdateHoverElements_Prefix(SelectToolHoverTextCard __instance, List<KSelectable> hoverObjects)
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			if ((Object)(object)textStyle == (Object)null)
			{
				TextStyleSetting standard = ((HoverTextConfiguration)__instance).Styles_Title.Standard;
				textStyle = new TextStyleSetting
				{
					sdfFont = standard.sdfFont,
					fontSize = standard.fontSize,
					textColor = standard.textColor,
					style = standard.style,
					enableWordWrapping = standard.enableWordWrapping
				};
				textStyle.fontSize = 16;
			}
			int num = Grid.PosToCell(Camera.main.ScreenToWorldPoint(KInputManager.GetMousePos()));
			if (!((Object)(object)OverlayScreen.Instance == (Object)null) && Grid.IsValidCell(num))
			{
				if (PortDisplayController.TryGetActivePortDesc(num, out portText, out portSprite, out portColor))
				{
					AddAny = true;
				}
				else
				{
					AddAny = false;
				}
			}
		}

		private static void HoverTextDrawer_EndDrawing_Prefix(HoverTextDrawer __instance)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (AddAny)
			{
				AddAny = false;
				__instance.BeginShadowBar(false);
				__instance.DrawIcon(portSprite, portColor, 20, 2);
				__instance.DrawText(portText, textStyle);
				__instance.EndShadowBar();
			}
		}
	}

	private static HashSet<string> buildings = new HashSet<string>();

	private static readonly string PLib_Registry_StatusItems = "PLib_Registry_PipeStatusItems";

	public static StatusItem M_NeedLiquidIn;

	public static StatusItem M_NeedGasIn;

	public static StatusItem M_NeedLiquidOut;

	public static StatusItem M_NeedGasOut;

	public static StatusItem M_NeedSolidIn;

	public static StatusItem M_NeedSolidOut;

	public static string M_NeedLiquidIn_Key = "M_NeedLiquidIn";

	public static string M_NeedLiquidOut_Key = "M_NeedLiquidOut";

	public static string M_NeedGasIn_Key = "M_NeedGasIn";

	public static string M_NeedGasOut_Key = "M_NeedGasOut";

	public static string M_NeedSolidIn_Key = "M_NeedSolidIn";

	public static string M_NeedSolidOut_Key = "M_NeedSolidOut";

	internal static bool HasBuilding(string name)
	{
		return buildings.Contains(name);
	}

	internal static void AddBuilding(string ID)
	{
		buildings.Add(ID);
	}

	public static void PatchAll(Harmony harmony)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		MethodInfo methodInfo = AccessTools.Method(typeof(EntityCellVisualizer), "DrawIcons", (Type[])null, (Type[])null);
		harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(ConduitDisplayPortPatching), "PortDrawPrefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		MethodInfo methodInfo2 = AccessTools.Method(typeof(BuildingDef), "MarkArea", (Type[])null, (Type[])null);
		harmony.Patch((MethodBase)methodInfo2, (HarmonyMethod)null, new HarmonyMethod(typeof(ConduitDisplayPortPatching), "MarkAreaPostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
		MethodInfo methodInfo3 = AccessTools.Method(typeof(BuildingDef), "AreConduitPortsInValidPositions", (Type[])null, (Type[])null);
		harmony.Patch((MethodBase)methodInfo3, (HarmonyMethod)null, new HarmonyMethod(typeof(ConduitDisplayPortPatching), "AreConduitPortsInValidPositionsPostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
		if (!PRegistry.GetData<bool>(PLib_Registry_StatusItems))
		{
			MethodInfo methodInfo4 = AccessTools.Method(typeof(BuildingStatusItems), "CreateStatusItems", (Type[])null, (Type[])null);
			harmony.Patch((MethodBase)methodInfo4, (HarmonyMethod)null, new HarmonyMethod(typeof(ConduitDisplayPortPatching), "CreatePortStatusItemsPostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
			MethodInfo methodInfo5 = AccessTools.Method(typeof(Localization), "Initialize", (Type[])null, (Type[])null);
			harmony.Patch((MethodBase)methodInfo5, (HarmonyMethod)null, new HarmonyMethod(typeof(ConduitDisplayPortPatching), "CreateStatusItemStrings", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
			PRegistry.PutData(PLib_Registry_StatusItems, true);
		}
		PortInfoDrawing.PatchAll(harmony);
	}

	public static void PortDrawPrefix(EntityCellVisualizer __instance, HashedString mode)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		BuildingCellVisualizer val = (BuildingCellVisualizer)(object)((__instance is BuildingCellVisualizer) ? __instance : null);
		if (val != null && buildings.Contains(((Def)val.building.Def).PrefabID))
		{
			GameObject gameObject = ((Component)val.building).gameObject;
			PortDisplayController component = gameObject.GetComponent<PortDisplayController>();
			if ((Object)(object)component != (Object)null)
			{
				component.Draw(val, mode, gameObject);
			}
		}
	}

	public static void MarkAreaPostfix(BuildingDef __instance, int cell, Orientation orientation, ObjectLayer layer, GameObject go)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected I4, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected I4, but got Unknown
		PortDisplay2[] components = __instance.BuildingComplete.GetComponents<PortDisplay2>();
		foreach (PortDisplay2 portDisplay in components)
		{
			ConduitType type = portDisplay.type;
			ObjectLayer objectLayerForConduitType = Grid.GetObjectLayerForConduitType(type);
			CellOffset rotatedCellOffset = Rotatable.GetRotatedCellOffset(portDisplay.offset, orientation);
			int num = Grid.OffsetCell(cell, rotatedCellOffset);
			__instance.MarkOverlappingPorts(((ObjectLayerIndexer)(ref Grid.Objects))[num, (int)objectLayerForConduitType], go);
			((ObjectLayerIndexer)(ref Grid.Objects))[num, (int)objectLayerForConduitType] = go;
		}
	}

	public static void AreConduitPortsInValidPositionsPostfix(BuildingDef __instance, ref bool __result, GameObject source_go, int cell, Orientation orientation, ref string fail_reason)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!__result)
		{
			return;
		}
		PortDisplay2[] components = __instance.BuildingComplete.GetComponents<PortDisplay2>();
		foreach (PortDisplay2 portDisplay in components)
		{
			CellOffset rotatedCellOffset = Rotatable.GetRotatedCellOffset(portDisplay.offset, orientation);
			int num = Grid.OffsetCell(cell, rotatedCellOffset);
			__result = __instance.IsValidConduitConnection(source_go, portDisplay.type, num, ref fail_reason);
			if (!__result)
			{
				break;
			}
		}
	}

	public static StatusItem GetInputStatusItem(ConduitType type)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		switch (type - 1)
		{
		case 0:
			if (M_NeedGasIn == null)
			{
				M_NeedGasIn = ((ResourceSet<StatusItem>)(object)Db.Get().BuildingStatusItems).Get(M_NeedGasIn_Key);
			}
			return M_NeedGasIn;
		case 1:
			if (M_NeedLiquidIn == null)
			{
				M_NeedLiquidIn = ((ResourceSet<StatusItem>)(object)Db.Get().BuildingStatusItems).Get(M_NeedLiquidIn_Key);
			}
			return M_NeedLiquidIn;
		case 2:
			if (M_NeedSolidIn == null)
			{
				M_NeedSolidIn = ((ResourceSet<StatusItem>)(object)Db.Get().BuildingStatusItems).Get(M_NeedSolidIn_Key);
			}
			return M_NeedSolidIn;
		default:
			throw new ArgumentException($"Unknown conduit type: {type}");
		}
	}

	public static StatusItem GetOutputStatusItem(ConduitType type)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		switch (type - 1)
		{
		case 0:
			if (M_NeedGasOut == null)
			{
				M_NeedGasOut = ((ResourceSet<StatusItem>)(object)Db.Get().BuildingStatusItems).Get(M_NeedGasOut_Key);
			}
			return M_NeedGasOut;
		case 1:
			if (M_NeedLiquidOut == null)
			{
				M_NeedLiquidOut = ((ResourceSet<StatusItem>)(object)Db.Get().BuildingStatusItems).Get(M_NeedLiquidOut_Key);
			}
			return M_NeedLiquidOut;
		case 2:
			if (M_NeedSolidOut == null)
			{
				M_NeedSolidOut = ((ResourceSet<StatusItem>)(object)Db.Get().BuildingStatusItems).Get(M_NeedSolidOut_Key);
			}
			return M_NeedSolidOut;
		default:
			throw new ArgumentException($"Unknown conduit type: {type}");
		}
	}

	public static void CreatePortStatusItemsPostfix(BuildingStatusItems __instance)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		M_NeedGasIn = __instance.CreateStatusItem(M_NeedGasIn_Key, "BUILDING", "status_item_need_supply_in", (IconType)2, (NotificationType)3, true, GasConduits.ID, true, 129022);
		M_NeedGasIn.resolveStringCallback = delegate(string str, object data)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Tuple<ConduitType, Tag> val = (Tuple<ConduitType, Tag>)data;
			string newValue = string.Format(LocString.op_Implicit(NEEDGASIN.LINE_ITEM), GameTagExtensions.ProperName(val.second));
			str = str.Replace("{GasRequired}", newValue);
			return str;
		};
		M_NeedLiquidIn = __instance.CreateStatusItem(M_NeedLiquidIn_Key, "BUILDING", "status_item_need_supply_in", (IconType)2, (NotificationType)3, true, LiquidConduits.ID, true, 129022);
		M_NeedLiquidIn.resolveStringCallback = delegate(string str, object data)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Tuple<ConduitType, Tag> val = (Tuple<ConduitType, Tag>)data;
			string newValue = string.Format(LocString.op_Implicit(NEEDLIQUIDIN.LINE_ITEM), GameTagExtensions.ProperName(val.second));
			str = str.Replace("{LiquidRequired}", newValue);
			return str;
		};
		M_NeedSolidIn = __instance.CreateStatusItem(M_NeedSolidIn_Key, "BUILDING", "status_item_need_supply_in", (IconType)2, (NotificationType)3, true, SolidConveyor.ID, true, 129022);
		M_NeedSolidIn.resolveStringCallback = delegate(string str, object data)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Tuple<ConduitType, Tag> val = (Tuple<ConduitType, Tag>)data;
			string newValue = string.Format(LocString.op_Implicit(NEEDLIQUIDIN.LINE_ITEM), GameTagExtensions.ProperName(val.second));
			str = str.Replace("{LiquidRequired}", newValue);
			return str;
		};
		M_NeedGasOut = __instance.CreateStatusItem(M_NeedGasOut_Key, "BUILDING", "status_item_need_supply_out", (IconType)2, (NotificationType)3, true, GasConduits.ID, true, 129022);
		M_NeedGasOut.resolveStringCallback = delegate(string str, object data)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			Tuple<ConduitType, List<Tag>> val = (Tuple<ConduitType, List<Tag>>)data;
			foreach (Tag item in val.second)
			{
				str += "\n";
				str += string.Format(LocString.op_Implicit(NEEDGASIN.LINE_ITEM), GameTagExtensions.ProperName(item));
			}
			return str;
		};
		M_NeedLiquidOut = __instance.CreateStatusItem(M_NeedLiquidOut_Key, "BUILDING", "status_item_need_supply_out", (IconType)2, (NotificationType)3, true, LiquidConduits.ID, true, 129022);
		M_NeedLiquidOut.resolveStringCallback = delegate(string str, object data)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			Tuple<ConduitType, List<Tag>> val = (Tuple<ConduitType, List<Tag>>)data;
			foreach (Tag item2 in val.second)
			{
				str += "\n";
				str += string.Format(LocString.op_Implicit(NEEDLIQUIDIN.LINE_ITEM), GameTagExtensions.ProperName(item2));
			}
			return str;
		};
		M_NeedSolidOut = __instance.CreateStatusItem(M_NeedSolidOut_Key, "BUILDING", "status_item_need_supply_out", (IconType)2, (NotificationType)3, true, SolidConveyor.ID, true, 129022);
		M_NeedSolidOut.resolveStringCallback = delegate(string str, object data)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			Tuple<ConduitType, List<Tag>> val = (Tuple<ConduitType, List<Tag>>)data;
			foreach (Tag item3 in val.second)
			{
				str += "\n";
				str += string.Format(LocString.op_Implicit(NEEDLIQUIDIN.LINE_ITEM), GameTagExtensions.ProperName(item3));
			}
			return str;
		};
	}

	public static void CreateStatusItemStrings()
	{
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDGASIN.NAME",
			LocString.op_Implicit(NEEDGASIN.NAME)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDGASIN.TOOLTIP",
			LocString.op_Implicit(NEEDGASIN.TOOLTIP)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDLIQUIDIN.NAME",
			LocString.op_Implicit(NEEDLIQUIDIN.NAME)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDLIQUIDIN.TOOLTIP",
			LocString.op_Implicit(NEEDLIQUIDIN.TOOLTIP)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDGASOUT.NAME",
			LocString.op_Implicit(NEEDGASOUT.NAME)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDGASOUT.TOOLTIP",
			LocString.op_Implicit(NEEDGASOUT.TOOLTIP)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDLIQUIDOUT.NAME",
			LocString.op_Implicit(NEEDLIQUIDOUT.NAME)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDLIQUIDOUT.TOOLTIP",
			LocString.op_Implicit(NEEDLIQUIDOUT.TOOLTIP)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDSOLIDOUT.NAME",
			LocString.op_Implicit(NEEDSOLIDOUT.NAME)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDSOLIDOUT.TOOLTIP",
			LocString.op_Implicit(NEEDSOLIDOUT.TOOLTIP)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDSOLIDIN.NAME",
			LocString.op_Implicit(NEEDSOLIDIN.NAME)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDING.STATUSITEMS.M_NEEDSOLIDIN.TOOLTIP",
			LocString.op_Implicit(NEEDSOLIDIN.TOOLTIP)
		});
	}
}
