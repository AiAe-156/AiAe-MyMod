using System;
using UnityEngine;

namespace PeterHan.PLib.UI.Layouts;

/// <summary>
/// Stores constraints applied to an object in a relative layout.
/// </summary>
/// <typeparam name="T">The type of the target object.</typeparam>
internal class RelativeLayoutParamsBase<T>
{
	/// <summary>
	/// The edge position determined for a component.
	/// </summary>
	[Serializable]
	internal sealed class EdgeStatus
	{
		/// <summary>
		/// The type of constraint to use for this relative layout.
		/// </summary>
		internal RelativeConstraintType Constraint;

		/// <summary>
		/// The anchor position in the component that sets the relative anchor.
		///
		/// 0.0f is the bottom/left, 1.0f is the top/right.
		/// </summary>
		internal float FromAnchor;

		/// <summary>
		/// The component to which this edge is anchored.
		/// </summary>
		internal T FromComponent;

		/// <summary>
		/// The offset in pixels from the anchor. + is upwards/rightwards, - is downwards/
		/// leftwards.
		/// </summary>
		internal float Offset;

		/// <summary>
		/// True if the position has been locked down in the code.
		/// Locked should only be set by the layout manager, crashes may occur otherwise.
		/// </summary>
		public bool Locked => Constraint == RelativeConstraintType.Locked;

		/// <summary>
		/// True if the position is not constrained to anything.
		/// </summary>
		public bool Unconstrained => Constraint == RelativeConstraintType.Unconstrained;

		internal EdgeStatus()
		{
			FromAnchor = 0f;
			FromComponent = default(T);
			Offset = 0f;
			Constraint = RelativeConstraintType.Unconstrained;
		}

		/// <summary>
		/// Copies data from another edge status object.
		/// </summary>
		/// <param name="other">The object to copy.</param>
		internal void CopyFrom(EdgeStatus other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			switch (Constraint = other.Constraint)
			{
			case RelativeConstraintType.ToComponent:
				FromComponent = other.FromComponent;
				break;
			case RelativeConstraintType.ToAnchor:
				FromAnchor = other.FromAnchor;
				break;
			case RelativeConstraintType.Locked:
				FromAnchor = other.FromAnchor;
				Offset = other.Offset;
				break;
			case RelativeConstraintType.Unconstrained:
				break;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is EdgeStatus edgeStatus && edgeStatus.FromAnchor == FromAnchor && edgeStatus.Offset == Offset)
			{
				return Constraint == edgeStatus.Constraint;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 37 * (37 * FromAnchor.GetHashCode() + Offset.GetHashCode()) + Constraint.GetHashCode();
		}

		/// <summary>
		/// Resets these offsets to unlocked.
		/// </summary>
		public void Reset()
		{
			Constraint = RelativeConstraintType.Unconstrained;
			Offset = 0f;
			FromAnchor = 0f;
			FromComponent = default(T);
		}

		public override string ToString()
		{
			return string.Format("EdgeStatus[Constraints={2},Anchor={0:F2},Offset={1:F2}]", FromAnchor, Offset, Constraint);
		}
	}

	/// <summary>
	/// The anchored position of the bottom edge.
	/// </summary>
	internal EdgeStatus BottomEdge { get; }

	/// <summary>
	/// The insets. If null, insets are all zero.
	/// </summary>
	internal RectOffset Insets { get; set; }

	/// <summary>
	/// The anchored position of the left edge.
	/// </summary>
	internal EdgeStatus LeftEdge { get; }

	/// <summary>
	/// Overrides the size of the component if set.
	/// </summary>
	internal Vector2 OverrideSize { get; set; }

	/// <summary>
	/// The anchored position of the right edge.
	/// </summary>
	internal EdgeStatus RightEdge { get; }

	/// <summary>
	/// The anchored position of the top edge.
	/// </summary>
	internal EdgeStatus TopEdge { get; }

	internal RelativeLayoutParamsBase()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Insets = null;
		BottomEdge = new EdgeStatus();
		LeftEdge = new EdgeStatus();
		RightEdge = new EdgeStatus();
		TopEdge = new EdgeStatus();
		OverrideSize = Vector2.zero;
	}
}
