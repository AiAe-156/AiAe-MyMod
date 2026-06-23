using UnityEngine;

namespace UtilLibs.BuildingPortUtils;

public abstract class DisplayConduitPortInfo
{
	internal readonly ConduitType type;

	internal readonly CellOffset offset;

	internal readonly CellOffset offsetFlipped;

	internal readonly bool input;

	internal readonly Color color;

	public ConduitType Type => type;

	protected DisplayConduitPortInfo(ConduitType type, CellOffset offset, CellOffset? offsetFlipped, bool input, Color? color)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		this.type = type;
		this.offset = offset;
		this.input = input;
		this.offsetFlipped = offsetFlipped ?? offset;
		if (color.HasValue)
		{
			this.color = (Color)(((_003F?)color) ?? Color.white);
		}
		else
		{
			this.color = SharedConduitUtils.GetIOColor(input, type);
		}
	}
}
