using UnityEngine;

namespace UtilLibs.BuildingPortUtils;

public class PortDisplayInput : DisplayConduitPortInfo
{
	public PortDisplayInput(ConduitType type, CellOffset offset, CellOffset? offsetFlipped = null, Color? color = null)
		: base(type, offset, offsetFlipped, input: true, color)
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
	//IL_0002: Unknown result type (might be due to invalid IL or missing references)

}
