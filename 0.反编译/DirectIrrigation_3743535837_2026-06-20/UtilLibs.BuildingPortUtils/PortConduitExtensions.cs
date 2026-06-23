using UnityEngine;

namespace UtilLibs.BuildingPortUtils;

public static class PortConduitExtensions
{
	internal static int GetConduitObjectLayer(this ConduitType conduitType)
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
			_ => 0, 
		};
	}

	internal static int GetPortObjectLayer(this ConduitType conduitType)
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
			0 => 15, 
			1 => 19, 
			2 => 23, 
			_ => 0, 
		};
	}

	internal static bool IsConnected(this ConduitType conduitType, int cell)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = ((ObjectLayerIndexer)(ref Grid.Objects))[cell, conduitType.GetConduitObjectLayer()];
		BuildingComplete val2 = default(BuildingComplete);
		return (Object)(object)val != (Object)null && val.TryGetComponent<BuildingComplete>(ref val2);
	}

	public static int GetCellWithOffset(this Building building, CellOffset offset)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		int num = GameUtil.NaturalBuildingCell((KMonoBehaviour)(object)building);
		CellOffset rotatedOffset = building.GetRotatedOffset(offset);
		return Grid.OffsetCell(num, rotatedOffset);
	}
}
