using System;
using UnityEngine;

namespace PeterHan.PLib.UI.Layouts;

/// <summary>
/// Maintains the status of a layout in progress.
/// </summary>
internal sealed class BoxLayoutStatus
{
	/// <summary>
	/// The current direction of flow.
	/// </summary>
	public readonly PanelDirection direction;

	/// <summary>
	/// The edge from where layout started.
	/// </summary>
	public readonly Edge edge;

	/// <summary>
	/// The next component's offset.
	/// </summary>
	public readonly float offset;

	/// <summary>
	/// The component size in that direction minus margins.
	/// </summary>
	public readonly float size;

	internal BoxLayoutStatus(PanelDirection direction, RectOffset margins, float size)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		this.direction = direction;
		switch (direction)
		{
		case PanelDirection.Horizontal:
			edge = (Edge)0;
			offset = margins.left;
			this.size = size - offset - (float)margins.right;
			break;
		case PanelDirection.Vertical:
			edge = (Edge)2;
			offset = margins.top;
			this.size = size - offset - (float)margins.bottom;
			break;
		default:
			throw new ArgumentException("direction");
		}
	}
}
