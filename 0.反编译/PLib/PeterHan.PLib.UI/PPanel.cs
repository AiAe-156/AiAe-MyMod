using System;
using System.Collections.Generic;
using PeterHan.PLib.Core;
using UnityEngine;

namespace PeterHan.PLib.UI;

/// <summary>
/// A custom UI panel factory which can arrange its children horizontally or vertically.
/// </summary>
public class PPanel : PContainer, IDynamicSizable, IUIComponent
{
	/// <summary>
	/// The children of this panel.
	/// </summary>
	protected readonly ICollection<IUIComponent> children;

	/// <summary>
	/// The alignment position to use for child elements if they are smaller than the
	/// required size.
	/// </summary>
	public TextAnchor Alignment { get; set; }

	/// <summary>
	/// The direction in which components will be laid out.
	/// </summary>
	public PanelDirection Direction { get; set; }

	public bool DynamicSize { get; set; }

	/// <summary>
	/// The spacing between components in pixels.
	/// </summary>
	public int Spacing { get; set; }

	public PPanel()
		: this(null)
	{
	}

	public PPanel(string name)
		: base(name ?? "Panel")
	{
		Alignment = (TextAnchor)4;
		children = new List<IUIComponent>();
		Direction = PanelDirection.Vertical;
		DynamicSize = true;
		Spacing = 0;
	}

	/// <summary>
	/// Adds a child to this panel.
	/// </summary>
	/// <param name="child">The child to add.</param>
	/// <returns>This panel for call chaining.</returns>
	public PPanel AddChild(IUIComponent child)
	{
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		children.Add(child);
		return this;
	}

	/// <summary>
	/// Adds a handler when this panel is realized.
	/// </summary>
	/// <param name="onRealize">The handler to invoke on realization.</param>
	/// <returns>This panel for call chaining.</returns>
	public PPanel AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		base.OnRealize += onRealize;
		return this;
	}

	public override GameObject Build()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return Build(default(Vector2), DynamicSize);
	}

	/// <summary>
	/// Builds this panel.
	/// </summary>
	/// <param name="size">The fixed size to use if dynamic is false.</param>
	/// <param name="dynamic">Whether to use dynamic sizing.</param>
	/// <returns>The realized panel.</returns>
	private GameObject Build(Vector2 size, bool dynamic)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(null, base.Name);
		SetImage(val);
		foreach (IUIComponent child in children)
		{
			GameObject obj = child.Build();
			obj.SetParent(val);
			PUIElements.SetAnchors(obj, PUIAnchoring.Stretch, PUIAnchoring.Stretch);
		}
		BoxLayoutGroup boxLayoutGroup = val.AddComponent<BoxLayoutGroup>();
		boxLayoutGroup.Params = new BoxLayoutParams
		{
			Direction = Direction,
			Alignment = Alignment,
			Spacing = Spacing,
			Margin = base.Margin
		};
		if (!dynamic)
		{
			boxLayoutGroup.LockLayout();
			val.SetMinUISize(size);
		}
		boxLayoutGroup.flexibleWidth = base.FlexSize.x;
		boxLayoutGroup.flexibleHeight = base.FlexSize.y;
		InvokeRealize(val);
		return val;
	}

	/// <summary>
	/// Builds this panel with a given default size.
	/// </summary>
	/// <param name="size">The fixed size to use.</param>
	/// <returns>The realized panel.</returns>
	public GameObject BuildWithFixedSize(Vector2 size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Build(size, dynamic: false);
	}

	/// <summary>
	/// Removes a child from this panel.
	/// </summary>
	/// <param name="child">The child to remove.</param>
	/// <returns>This panel for call chaining.</returns>
	public PPanel RemoveChild(IUIComponent child)
	{
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		children.Remove(child);
		return this;
	}

	/// <summary>
	/// Sets the background color to the default Klei dialog blue.
	/// </summary>
	/// <returns>This panel for call chaining.</returns>
	public PPanel SetKleiBlueColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		base.BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
		return this;
	}

	/// <summary>
	/// Sets the background color to the Klei dialog header pink.
	/// </summary>
	/// <returns>This panel for call chaining.</returns>
	public PPanel SetKleiPinkColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		base.BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
		return this;
	}

	public override string ToString()
	{
		return $"PPanel[Name={base.Name},Direction={Direction}]";
	}
}
