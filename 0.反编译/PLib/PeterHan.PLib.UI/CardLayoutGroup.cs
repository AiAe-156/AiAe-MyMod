using System;
using System.Collections.Generic;
using PeterHan.PLib.UI.Layouts;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

/// <summary>
/// A freezable layout manager that displays one of its contained objects at a time.
/// Unlike other layout groups, even inactive children are considered for sizing.
/// </summary>
public sealed class CardLayoutGroup : AbstractLayoutGroup
{
	/// <summary>
	/// Results from the horizontal calculation pass.
	/// </summary>
	private CardLayoutResults horizontal;

	/// <summary>
	/// The margin around the components as a whole.
	/// </summary>
	[SerializeField]
	private RectOffset margin;

	/// <summary>
	/// Results from the vertical calculation pass.
	/// </summary>
	private CardLayoutResults vertical;

	/// <summary>
	/// The margin around the components as a whole.
	/// </summary>
	public RectOffset Margin
	{
		get
		{
			return margin;
		}
		set
		{
			margin = value;
		}
	}

	/// <summary>
	/// Calculates the size of the card layout container.
	/// </summary>
	/// <param name="obj">The container to lay out.</param>
	/// <param name="args">The parameters to use for layout.</param>
	/// <param name="direction">The direction which is being calculated.</param>
	/// <returns>The minimum and preferred box layout size.</returns>
	private static CardLayoutResults Calc(GameObject obj, PanelDirection direction)
	{
		RectTransform val = EntityTemplateExtensions.AddOrGet<RectTransform>(obj);
		int childCount = ((Transform)val).childCount;
		CardLayoutResults cardLayoutResults = new CardLayoutResults(direction, childCount);
		PooledList<Component, BoxLayoutGroup> val2 = ListPool<Component, BoxLayoutGroup>.Allocate();
		for (int i = 0; i < childCount; i++)
		{
			Transform child = ((Transform)val).GetChild(i);
			GameObject val3 = ((child != null) ? ((Component)child).gameObject : null);
			if ((Object)(object)val3 != (Object)null)
			{
				bool activeInHierarchy = val3.activeInHierarchy;
				((List<Component>)(object)val2).Clear();
				val3.GetComponents<Component>((List<Component>)(object)val2);
				val3.SetActive(true);
				LayoutSizes layoutSizes = PUIUtils.CalcSizes(val3, direction, (IEnumerable<Component>)val2);
				if (!layoutSizes.ignore)
				{
					cardLayoutResults.Expand(layoutSizes);
					cardLayoutResults.children.Add(layoutSizes);
				}
				val3.SetActive(activeInHierarchy);
			}
		}
		val2.Recycle();
		return cardLayoutResults;
	}

	/// <summary>
	/// Lays out components in the card layout container.
	/// </summary>
	/// <param name="margin">The margin to allow around the components.</param>
	/// <param name="required">The calculated minimum and preferred sizes.</param>
	/// <param name="size">The total available size in this dimension.</param>
	private static void DoLayout(RectOffset margin, CardLayoutResults required, float size)
	{
		if (required == null)
		{
			throw new ArgumentNullException("required");
		}
		PanelDirection direction = required.direction;
		PooledList<ILayoutController, BoxLayoutGroup> val = ListPool<ILayoutController, BoxLayoutGroup>.Allocate();
		size = ((direction != PanelDirection.Horizontal) ? (size - (float)(margin.top + margin.bottom)) : (size - (float)(margin.left + margin.right)));
		foreach (LayoutSizes child in required.children)
		{
			GameObject source = child.source;
			if (!((Object)(object)source != (Object)null))
			{
				continue;
			}
			float properSize = PUIUtils.GetProperSize(child, size);
			RectTransform val2 = EntityTemplateExtensions.AddOrGet<RectTransform>(source);
			if (direction == PanelDirection.Horizontal)
			{
				val2.SetInsetAndSizeFromParentEdge((Edge)0, (float)margin.left, properSize);
			}
			else
			{
				val2.SetInsetAndSizeFromParentEdge((Edge)2, (float)margin.top, properSize);
			}
			((List<ILayoutController>)(object)val).Clear();
			source.GetComponents<ILayoutController>((List<ILayoutController>)(object)val);
			foreach (ILayoutController item in (List<ILayoutController>)(object)val)
			{
				if (direction == PanelDirection.Horizontal)
				{
					item.SetLayoutHorizontal();
				}
				else
				{
					item.SetLayoutVertical();
				}
			}
		}
		val.Recycle();
	}

	internal CardLayoutGroup()
	{
		horizontal = null;
		base.layoutPriority = 1;
		vertical = null;
	}

	public override void CalculateLayoutInputHorizontal()
	{
		if (!locked)
		{
			RectOffset val = Margin;
			float num = ((val == null) ? 0f : ((float)(val.left + val.right)));
			horizontal = Calc(((Component)this).gameObject, PanelDirection.Horizontal);
			LayoutSizes total = horizontal.total;
			base.minWidth = total.min + num;
			base.preferredWidth = total.preferred + num;
		}
	}

	public override void CalculateLayoutInputVertical()
	{
		if (!locked)
		{
			RectOffset val = Margin;
			float num = ((val == null) ? 0f : ((float)(val.top + val.bottom)));
			vertical = Calc(((Component)this).gameObject, PanelDirection.Vertical);
			LayoutSizes total = vertical.total;
			base.minHeight = total.min + num;
			base.preferredHeight = total.preferred + num;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		horizontal = null;
		vertical = null;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		horizontal = null;
		vertical = null;
	}

	/// <summary>
	/// Switches the active card.
	/// </summary>
	/// <param name="card">The child to make active, or null to inactivate all children.</param>
	public void SetActiveCard(GameObject card)
	{
		int childCount = ((Component)this).transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = ((Component)this).transform.GetChild(i);
			GameObject val = ((child != null) ? ((Component)child).gameObject : null);
			if ((Object)(object)val != (Object)null)
			{
				val.SetActive((Object)(object)val == (Object)(object)card);
			}
		}
	}

	/// <summary>
	/// Switches the active card.
	/// </summary>
	/// <param name="index">The child index to make active, or -1 to inactivate all children.</param>
	public void SetActiveCard(int index)
	{
		int childCount = ((Component)this).transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = ((Component)this).transform.GetChild(i);
			GameObject val = ((child != null) ? ((Component)child).gameObject : null);
			if ((Object)(object)val != (Object)null)
			{
				val.SetActive(i == index);
			}
		}
	}

	public override void SetLayoutHorizontal()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (horizontal != null && !locked)
		{
			object obj = ((object)Margin) ?? ((object)new RectOffset());
			CardLayoutResults required = horizontal;
			Rect rect = base.rectTransform.rect;
			DoLayout((RectOffset)obj, required, ((Rect)(ref rect)).width);
		}
	}

	public override void SetLayoutVertical()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (vertical != null && !locked)
		{
			object obj = ((object)Margin) ?? ((object)new RectOffset());
			CardLayoutResults required = vertical;
			Rect rect = base.rectTransform.rect;
			DoLayout((RectOffset)obj, required, ((Rect)(ref rect)).height);
		}
	}
}
