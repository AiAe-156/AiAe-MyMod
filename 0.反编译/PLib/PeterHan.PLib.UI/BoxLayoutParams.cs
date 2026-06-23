using System;
using UnityEngine;

namespace PeterHan.PLib.UI;

/// <summary>
/// The parameters used for laying out a box layout.
/// </summary>
[Serializable]
public sealed class BoxLayoutParams
{
	/// <summary>
	/// The alignment to use for components that are not big enough to fit and have no
	/// flexible width.
	/// </summary>
	public TextAnchor Alignment { get; set; }

	/// <summary>
	/// The direction of layout.
	/// </summary>
	public PanelDirection Direction { get; set; }

	/// <summary>
	/// The margin between the children and the component edge.
	/// </summary>
	public RectOffset Margin { get; set; }

	/// <summary>
	/// The spacing between components.
	/// </summary>
	public float Spacing { get; set; }

	public BoxLayoutParams()
	{
		Alignment = (TextAnchor)4;
		Direction = PanelDirection.Horizontal;
		Margin = null;
		Spacing = 0f;
	}

	public override string ToString()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return $"BoxLayoutParams[Alignment={Alignment},Direction={Direction},Spacing={Spacing:F2}]";
	}
}
