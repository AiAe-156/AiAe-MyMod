using System;
using UnityEngine;

namespace PeterHan.PLib.UI.Layouts;

/// <summary>
/// Parameters used to store the dynamic data of an object during a relative layout.
/// </summary>
internal sealed class RelativeLayoutResults : RelativeLayoutParamsBase<GameObject>
{
	/// <summary>
	/// A set of insets that are always zero.
	/// </summary>
	private static readonly RectOffset ZERO = new RectOffset();

	/// <summary>
	/// The preferred size of this component.
	/// </summary>
	private Vector2 prefSize;

	/// <summary>
	/// The instance parameters of the bottom edge's component.
	/// </summary>
	internal RelativeLayoutResults BottomParams { get; set; }

	/// <summary>
	/// The height of the component plus its margin box.
	/// </summary>
	internal float EffectiveHeight { get; private set; }

	/// <summary>
	/// The width of the component plus its margin box.
	/// </summary>
	internal float EffectiveWidth { get; private set; }

	/// <summary>
	/// The instance parameters of the left edge's component.
	/// </summary>
	internal RelativeLayoutResults LeftParams { get; set; }

	/// <summary>
	/// The preferred height at which this component will be laid out, unless both
	/// edges are constrained.
	/// </summary>
	internal float PreferredHeight
	{
		get
		{
			return prefSize.y;
		}
		set
		{
			prefSize.y = value;
			EffectiveHeight = value + (float)base.Insets.top + (float)base.Insets.bottom;
		}
	}

	/// <summary>
	/// The preferred width at which this component will be laid out, unless both
	/// edges are constrained.
	/// </summary>
	internal float PreferredWidth
	{
		get
		{
			return prefSize.x;
		}
		set
		{
			prefSize.x = value;
			EffectiveWidth = value + (float)base.Insets.left + (float)base.Insets.right;
		}
	}

	/// <summary>
	/// The instance parameters of the right edge's component.
	/// </summary>
	internal RelativeLayoutResults RightParams { get; set; }

	/// <summary>
	/// The instance parameters of the top edge's component.
	/// </summary>
	internal RelativeLayoutResults TopParams { get; set; }

	/// <summary>
	/// The object to lay out.
	/// </summary>
	internal RectTransform Transform { get; set; }

	/// <summary>
	/// Whether the size delta should be used in the X direction (as opposed to offsets).
	/// </summary>
	internal bool UseSizeDeltaX { get; set; }

	/// <summary>
	/// Whether the size delta should be used in the Y direction (as opposed to offsets).
	/// </summary>
	internal bool UseSizeDeltaY { get; set; }

	internal RelativeLayoutResults(RectTransform transform, RelativeLayoutParams other)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Transform = transform ?? throw new ArgumentNullException("transform");
		if (other != null)
		{
			base.BottomEdge.CopyFrom(other.BottomEdge);
			base.TopEdge.CopyFrom(other.TopEdge);
			base.RightEdge.CopyFrom(other.RightEdge);
			base.LeftEdge.CopyFrom(other.LeftEdge);
			base.Insets = other.Insets ?? ZERO;
			base.OverrideSize = other.OverrideSize;
		}
		else
		{
			base.Insets = ZERO;
		}
		BottomParams = (LeftParams = (TopParams = (RightParams = null)));
		PreferredWidth = (PreferredHeight = 0f);
		UseSizeDeltaX = (UseSizeDeltaY = false);
	}

	public override string ToString()
	{
		GameObject gameObject = ((Component)Transform).gameObject;
		return string.Format("component={0} {1:F2}x{2:F2}", ((Object)(object)gameObject == (Object)null) ? "null" : ((Object)gameObject).name, prefSize.x, prefSize.y);
	}
}
