using System;
using System.Linq;
using System.Text;
using STRINGS;
using UnityEngine;

namespace UtilLibs.BuildingPortUtils;

internal class SharedConduitUtils
{
	private static StringBuilder sb = new StringBuilder();

	public unsafe static string GetFilteredPortTooltip(ConduitType type, bool isInput, Tag[] filterTags = null, SimHashes[] elementFilterTags = null, bool invertedFilter = false)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (elementFilterTags == null)
		{
			elementFilterTags = Array.Empty<SimHashes>();
		}
		if (filterTags == null)
		{
			filterTags = Array.Empty<Tag>();
		}
		sb.Clear();
		sb.Append(GetPortDescription(type, isInput));
		if (invertedFilter)
		{
			sb.Append(", ");
			sb.Append(LocString.op_Implicit(PRECONDITIONS.IS_PERMITTED));
		}
		sb.Append(": ");
		if (!filterTags.Any() && !elementFilterTags.Any())
		{
			sb.Append(LocString.op_Implicit(TAGS.ANY));
			return sb.ToString();
		}
		bool flag = false;
		for (int i = 0; i < elementFilterTags.Length; i++)
		{
			if (flag)
			{
				sb.Append(", ");
			}
			flag = true;
			SimHashes val = elementFilterTags[i];
			Element element = ElementLoader.GetElement(GameTagExtensions.CreateTag(val));
			if (element != null)
			{
				sb.Append(element.name);
			}
		}
		StringEntry val4 = default(StringEntry);
		for (int j = 0; j < filterTags.Length; j++)
		{
			if (flag)
			{
				sb.Append(", ");
			}
			flag = true;
			Tag val2 = filterTags[j];
			GameObject val3 = Assets.TryGetPrefab(val2);
			if (Strings.TryGet("STRINGS.MISC.TAGS." + ((object)(*(Tag*)(&val2))/*cast due to .constrained prefix*/).ToString().ToUpperInvariant(), ref val4))
			{
				sb.Append(val4.String);
			}
			else if ((Object)(object)val3 != (Object)null)
			{
				sb.Append(KSelectableExtensions.GetProperName(val3));
			}
		}
		return sb.ToString();
	}

	public static Color GetIOColor(bool input, ConduitType type)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		BuildingCellVisualizerResources val = BuildingCellVisualizerResources.Instance();
		IOColours val2 = (((int)type == 1) ? val.gasIOColours : val.liquidIOColours);
		ConnectedDisconnectedColours val3 = (input ? val2.input : val2.output);
		return Color32.op_Implicit(val3.connected);
	}

	public static string GetPortDescription(ConduitType type, bool input)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		if (1 == 0)
		{
		}
		string result = (type - 1) switch
		{
			0 => LocString.op_Implicit((!input) ? GASPLUMBING.CONSUMER : GASPLUMBING.PRODUCER), 
			1 => LocString.op_Implicit((!input) ? LIQUIDPLUMBING.CONSUMER : LIQUIDPLUMBING.PRODUCER), 
			2 => LocString.op_Implicit((!input) ? CONVEYOR.OUTPUT : CONVEYOR.INPUT), 
			_ => "", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public static int GetConduitLayer(ConduitType conduitType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		return (conduitType - 1) switch
		{
			0 => 12, 
			1 => 16, 
			2 => 20, 
			_ => -1, 
		};
	}

	public static IConduitFlow GetConduitFlow(ConduitType conduitType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		return (IConduitFlow)((conduitType - 1) switch
		{
			0 => Game.Instance.gasConduitFlow, 
			1 => Game.Instance.liquidConduitFlow, 
			2 => Game.Instance.solidConduitFlow, 
			_ => null, 
		});
	}

	public static IUtilityNetworkMgr GetConduitMng(ConduitType conduitType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		return (IUtilityNetworkMgr)((conduitType - 1) switch
		{
			0 => Game.Instance.gasConduitSystem, 
			1 => Game.Instance.liquidConduitSystem, 
			2 => Game.Instance.solidConduitSystem, 
			_ => null, 
		});
	}

	public static Sprite GetSprite(bool input, ConduitType type)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected I4, but got Unknown
		BuildingCellVisualizerResources val = BuildingCellVisualizerResources.Instance();
		switch (type - 1)
		{
		case 0:
			if (input)
			{
				return val.gasInputIcon;
			}
			return val.gasOutputIcon;
		case 1:
			if (input)
			{
				return val.liquidInputIcon;
			}
			return val.liquidOutputIcon;
		case 2:
			if (input)
			{
				return val.liquidInputIcon;
			}
			return val.liquidOutputIcon;
		default:
			return null;
		}
	}
}
