using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI.Layouts;

/// <summary>
/// A helper class for RelativeLayout.
/// </summary>
internal static class RelativeLayoutUtil
{
	/// <summary>
	/// Initializes and computes horizontal sizes for the components in this relative
	/// layout.
	/// </summary>
	/// <param name="children">The location to store information about these components.</param>
	/// <param name="all">The components to lay out.</param>
	/// <param name="constraints">The constraints defined for these components.</param>
	internal static void CalcX(this ICollection<RelativeLayoutResults> children, RectTransform all, IDictionary<GameObject, RelativeLayoutParams> constraints)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Component, RelativeLayoutGroup> val = ListPool<Component, RelativeLayoutGroup>.Allocate();
		PooledDictionary<GameObject, RelativeLayoutResults, RelativeLayoutGroup> val2 = DictionaryPool<GameObject, RelativeLayoutResults, RelativeLayoutGroup>.Allocate();
		int childCount = ((Transform)all).childCount;
		children.Clear();
		for (int i = 0; i < childCount; i++)
		{
			Transform child = ((Transform)all).GetChild(i);
			GameObject val3 = ((child != null) ? ((Component)child).gameObject : null);
			if (!((Object)(object)val3 != (Object)null))
			{
				continue;
			}
			((List<Component>)(object)val).Clear();
			val3.GetComponents<Component>((List<Component>)(object)val);
			LayoutSizes layoutSizes = PUIUtils.CalcSizes(val3, PanelDirection.Horizontal, (IEnumerable<Component>)val);
			if (layoutSizes.ignore)
			{
				continue;
			}
			float preferredWidth = layoutSizes.preferred;
			RelativeLayoutResults relativeLayoutResults;
			if (constraints.TryGetValue(val3, out var value))
			{
				relativeLayoutResults = new RelativeLayoutResults(Util.rectTransform(val3), value);
				Vector2 overrideSize = relativeLayoutResults.OverrideSize;
				if (overrideSize.x > 0f)
				{
					preferredWidth = overrideSize.x;
				}
				((Dictionary<GameObject, RelativeLayoutResults>)(object)val2)[val3] = relativeLayoutResults;
			}
			else
			{
				relativeLayoutResults = new RelativeLayoutResults(Util.rectTransform(val3), null);
			}
			relativeLayoutResults.PreferredWidth = preferredWidth;
			children.Add(relativeLayoutResults);
		}
		foreach (RelativeLayoutResults child2 in children)
		{
			child2.TopParams = InitResolve(child2.TopEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
			child2.BottomParams = InitResolve(child2.BottomEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
			child2.LeftParams = InitResolve(child2.LeftEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
			child2.RightParams = InitResolve(child2.RightEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
		}
		val2.Recycle();
		val.Recycle();
	}

	/// <summary>
	/// Computes vertical sizes for the components in this relative layout.
	/// </summary>
	/// <param name="children">The location to store information about these components.</param>
	internal static void CalcY(this ICollection<RelativeLayoutResults> children)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Component, RelativeLayoutGroup> val = ListPool<Component, RelativeLayoutGroup>.Allocate();
		foreach (RelativeLayoutResults child in children)
		{
			GameObject gameObject = ((Component)child.Transform).gameObject;
			Vector2 overrideSize = child.OverrideSize;
			((List<Component>)(object)val).Clear();
			gameObject.gameObject.GetComponents<Component>((List<Component>)(object)val);
			float preferredHeight = PUIUtils.CalcSizes(gameObject, PanelDirection.Vertical, (IEnumerable<Component>)val).preferred;
			if (overrideSize.y > 0f)
			{
				preferredHeight = overrideSize.y;
			}
			child.PreferredHeight = preferredHeight;
		}
		val.Recycle();
	}

	/// <summary>
	/// Calculates the minimum size the component must be to support a specific child
	/// component.
	/// </summary>
	/// <param name="min">The lower edge constraint.</param>
	/// <param name="max">The upper edge constraint.</param>
	/// <param name="effective">The component size in that dimension plus margins.</param>
	/// <returns>The minimum parent component size to fit the child.</returns>
	internal static float ElbowRoom(RelativeLayoutParamsBase<GameObject>.EdgeStatus min, RelativeLayoutParamsBase<GameObject>.EdgeStatus max, float effective)
	{
		float fromAnchor = min.FromAnchor;
		float fromAnchor2 = max.FromAnchor;
		float offset = min.Offset;
		float offset2 = max.Offset;
		if (fromAnchor2 > fromAnchor)
		{
			return (effective + offset - offset2) / (fromAnchor2 - fromAnchor);
		}
		return Math.Max(effective, Math.Max(Math.Abs(offset), Math.Abs(offset2)));
	}

	/// <summary>
	/// Executes the horizontal layout.
	/// </summary>
	/// <param name="children">The components to lay out.</param>
	/// <param name="scratch">The location where components will be temporarily stored.</param>
	/// <param name="mLeft">The left margin.</param>
	/// <param name="mRight">The right margin.</param>
	internal static void ExecuteX(this IEnumerable<RelativeLayoutResults> children, List<ILayoutController> scratch, float mLeft = 0f, float mRight = 0f)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		foreach (RelativeLayoutResults child in children)
		{
			RectTransform transform = child.Transform;
			RectOffset insets = child.Insets;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus leftEdge = child.LeftEdge;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus rightEdge = child.RightEdge;
			transform.anchorMin = new Vector2(leftEdge.FromAnchor, 0f);
			transform.anchorMax = new Vector2(rightEdge.FromAnchor, 1f);
			if (child.UseSizeDeltaX)
			{
				transform.SetSizeWithCurrentAnchors((Axis)0, child.PreferredWidth);
			}
			else
			{
				transform.offsetMin = new Vector2(leftEdge.Offset + (float)insets.left + ((leftEdge.FromAnchor <= 0f) ? mLeft : 0f), transform.offsetMin.y);
				transform.offsetMax = new Vector2(rightEdge.Offset - (float)insets.right - ((rightEdge.FromAnchor >= 1f) ? mRight : 0f), transform.offsetMax.y);
			}
			scratch.Clear();
			((Component)transform).gameObject.GetComponents<ILayoutController>(scratch);
			foreach (ILayoutController item in scratch)
			{
				item.SetLayoutHorizontal();
			}
		}
	}

	/// <summary>
	/// Executes the vertical layout.
	/// </summary>
	/// <param name="children">The components to lay out.</param>
	/// <param name="scratch">The location where components will be temporarily stored.</param>
	/// <param name="mBottom">The bottom margin.</param>
	/// <param name="mTop">The top margin.</param>
	internal static void ExecuteY(this IEnumerable<RelativeLayoutResults> children, List<ILayoutController> scratch, float mBottom = 0f, float mTop = 0f)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		foreach (RelativeLayoutResults child in children)
		{
			RectTransform transform = child.Transform;
			RectOffset insets = child.Insets;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus topEdge = child.TopEdge;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus bottomEdge = child.BottomEdge;
			transform.anchorMin = new Vector2(transform.anchorMin.x, bottomEdge.FromAnchor);
			transform.anchorMax = new Vector2(transform.anchorMax.x, topEdge.FromAnchor);
			if (child.UseSizeDeltaY)
			{
				transform.SetSizeWithCurrentAnchors((Axis)1, child.PreferredHeight);
			}
			else
			{
				transform.offsetMin = new Vector2(transform.offsetMin.x, bottomEdge.Offset + (float)insets.bottom + ((bottomEdge.FromAnchor <= 0f) ? mBottom : 0f));
				transform.offsetMax = new Vector2(transform.offsetMax.x, topEdge.Offset - (float)insets.top - ((topEdge.FromAnchor >= 1f) ? mTop : 0f));
			}
			scratch.Clear();
			((Component)transform).gameObject.GetComponents<ILayoutController>(scratch);
			foreach (ILayoutController item in scratch)
			{
				item.SetLayoutVertical();
			}
		}
	}

	/// <summary>
	/// Calculates the minimum size in the X direction.
	/// </summary>
	/// <param name="children">The components to lay out.</param>
	/// <returns>The minimum horizontal size.</returns>
	internal static float GetMinSizeX(this IEnumerable<RelativeLayoutResults> children)
	{
		float num = 0f;
		foreach (RelativeLayoutResults child in children)
		{
			float num2 = ElbowRoom(child.LeftEdge, child.RightEdge, child.EffectiveWidth);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	/// <summary>
	/// Calculates the minimum size in the Y direction.
	/// </summary>
	/// <param name="children">The components to lay out.</param>
	/// <returns>The minimum vertical size.</returns>
	internal static float GetMinSizeY(this IEnumerable<RelativeLayoutResults> children)
	{
		float num = 0f;
		foreach (RelativeLayoutResults child in children)
		{
			float num2 = ElbowRoom(child.BottomEdge, child.TopEdge, child.EffectiveHeight);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	/// <summary>
	/// Resolves a component reference if needed.
	/// </summary>
	/// <param name="edge">The edge to resolve.</param>
	/// <param name="lookup">The location where the component can be looked up.</param>
	/// <returns>The linked parameters for that edge if needed.</returns>
	private static RelativeLayoutResults InitResolve(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, IDictionary<GameObject, RelativeLayoutResults> lookup)
	{
		RelativeLayoutResults value = null;
		if (edge.Constraint == RelativeConstraintType.ToComponent && !lookup.TryGetValue(edge.FromComponent, out value))
		{
			edge.Constraint = RelativeConstraintType.Unconstrained;
		}
		return value;
	}

	/// <summary>
	/// Locks both edges if they are constrained to the same anchor.
	/// </summary>
	/// <param name="edge">The edge to check.</param>
	/// <param name="otherEdge">The other edge to check.</param>
	/// <returns>true if it was able to lock, or false otherwise.</returns>
	private static bool LockEdgeAnchor(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, RelativeLayoutParamsBase<GameObject>.EdgeStatus otherEdge)
	{
		int num;
		if (edge.Constraint == RelativeConstraintType.ToAnchor && otherEdge.Constraint == RelativeConstraintType.ToAnchor)
		{
			num = ((edge.FromAnchor == otherEdge.FromAnchor) ? 1 : 0);
			if (num != 0)
			{
				edge.Constraint = RelativeConstraintType.Locked;
				otherEdge.Constraint = RelativeConstraintType.Locked;
				edge.Offset = 0f;
				otherEdge.Offset = 0f;
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	/// <summary>
	/// Locks an edge if it is constrained to an anchor.
	/// </summary>
	/// <param name="edge">The edge to check.</param>
	private static void LockEdgeAnchor(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge)
	{
		if (edge.Constraint == RelativeConstraintType.ToAnchor)
		{
			edge.Constraint = RelativeConstraintType.Locked;
			edge.Offset = 0f;
		}
	}

	/// <summary>
	/// Locks an edge if it can be determined from another component.
	/// </summary>
	/// <param name="edge">The edge to check.</param>
	/// <param name="offset">The component's offset in that direction.</param>
	/// <param name="otherEdge">The opposing edge of the referenced component.</param>
	private static void LockEdgeComponent(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, RelativeLayoutParamsBase<GameObject>.EdgeStatus otherEdge)
	{
		if (edge.Constraint == RelativeConstraintType.ToComponent && otherEdge.Locked)
		{
			edge.Constraint = RelativeConstraintType.Locked;
			edge.FromAnchor = otherEdge.FromAnchor;
			edge.Offset = otherEdge.Offset;
		}
	}

	/// <summary>
	/// Locks an edge if it can be determined from the other edge.
	/// </summary>
	/// <param name="edge">The edge to check.</param>
	/// <param name="size">The component's effective size in that direction.</param>
	/// <param name="opposing">The component's other edge.</param>
	private static void LockEdgeRelative(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, float size, RelativeLayoutParamsBase<GameObject>.EdgeStatus opposing)
	{
		if (edge.Constraint == RelativeConstraintType.Unconstrained)
		{
			if (opposing.Locked)
			{
				edge.Constraint = RelativeConstraintType.Locked;
				edge.FromAnchor = opposing.FromAnchor;
				edge.Offset = opposing.Offset + size;
			}
			else if (opposing.Constraint == RelativeConstraintType.Unconstrained)
			{
				edge.Constraint = RelativeConstraintType.Locked;
				edge.FromAnchor = 0f;
				edge.Offset = 0f;
				opposing.Constraint = RelativeConstraintType.Locked;
				opposing.FromAnchor = 1f;
				opposing.Offset = 0f;
			}
		}
	}

	/// <summary>
	/// Runs a layout pass in the X direction, resolving edges that can be resolved.
	/// </summary>
	/// <param name="children">The children to resolve.</param>
	/// <returns>true if all children have all X edges constrained, or false otherwise.</returns>
	internal static bool RunPassX(this IEnumerable<RelativeLayoutResults> children)
	{
		bool result = true;
		foreach (RelativeLayoutResults child in children)
		{
			float effectiveWidth = child.EffectiveWidth;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus leftEdge = child.LeftEdge;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus rightEdge = child.RightEdge;
			if (LockEdgeAnchor(leftEdge, rightEdge))
			{
				child.UseSizeDeltaX = true;
			}
			LockEdgeAnchor(leftEdge);
			LockEdgeAnchor(rightEdge);
			LockEdgeRelative(leftEdge, 0f - effectiveWidth, rightEdge);
			LockEdgeRelative(rightEdge, effectiveWidth, leftEdge);
			if (child.LeftParams != null)
			{
				LockEdgeComponent(leftEdge, child.LeftParams.RightEdge);
			}
			if (child.RightParams != null)
			{
				LockEdgeComponent(rightEdge, child.RightParams.LeftEdge);
			}
			if (!leftEdge.Locked || !rightEdge.Locked)
			{
				result = false;
			}
		}
		return result;
	}

	/// <summary>
	/// Runs a layout pass in the Y direction, resolving edges that can be resolved.
	/// </summary>
	/// <param name="children">The children to resolve.</param>
	/// <returns>true if all children have all Y edges constrained, or false otherwise.</returns>
	internal static bool RunPassY(this IEnumerable<RelativeLayoutResults> children)
	{
		bool result = true;
		foreach (RelativeLayoutResults child in children)
		{
			float effectiveHeight = child.EffectiveHeight;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus topEdge = child.TopEdge;
			RelativeLayoutParamsBase<GameObject>.EdgeStatus bottomEdge = child.BottomEdge;
			if (LockEdgeAnchor(topEdge, bottomEdge))
			{
				child.UseSizeDeltaY = true;
			}
			LockEdgeAnchor(bottomEdge);
			LockEdgeAnchor(topEdge);
			LockEdgeRelative(bottomEdge, 0f - effectiveHeight, topEdge);
			LockEdgeRelative(topEdge, effectiveHeight, bottomEdge);
			if (child.BottomParams != null)
			{
				LockEdgeComponent(bottomEdge, child.BottomParams.TopEdge);
			}
			if (child.TopParams != null)
			{
				LockEdgeComponent(topEdge, child.TopParams.BottomEdge);
			}
			if (!topEdge.Locked || !bottomEdge.Locked)
			{
				result = false;
			}
		}
		return result;
	}

	/// <summary>
	/// Throws an error when resolution fails.
	/// </summary>
	/// <param name="children">The children, some of which failed to resolve.</param>
	/// <param name="limit">The number of passes executed before failing.</param>
	/// <param name="direction">The direction that failed.</param>
	internal static void ThrowUnresolvable(this IEnumerable<RelativeLayoutResults> children, int limit, PanelDirection direction)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.Append("After ").Append(limit);
		stringBuilder.Append(" passes, unable to complete resolution of RelativeLayout:");
		foreach (RelativeLayoutResults child in children)
		{
			string name = ((Object)((Component)child.Transform).gameObject).name;
			if (direction == PanelDirection.Horizontal)
			{
				if (!child.LeftEdge.Locked)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(name);
					stringBuilder.Append(".Left");
				}
				if (!child.RightEdge.Locked)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(name);
					stringBuilder.Append(".Right");
				}
			}
			else
			{
				if (!child.BottomEdge.Locked)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(name);
					stringBuilder.Append(".Bottom");
				}
				if (!child.TopEdge.Locked)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(name);
					stringBuilder.Append(".Top");
				}
			}
		}
		throw new InvalidOperationException(stringBuilder.ToString());
	}
}
