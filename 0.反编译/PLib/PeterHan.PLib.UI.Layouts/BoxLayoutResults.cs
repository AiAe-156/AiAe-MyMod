using System.Collections.Generic;

namespace PeterHan.PLib.UI.Layouts;

/// <summary>
/// A class which stores the results of a single box layout calculation pass.
/// </summary>
internal sealed class BoxLayoutResults
{
	/// <summary>
	/// The components which were laid out.
	/// </summary>
	public readonly ICollection<LayoutSizes> children;

	/// <summary>
	/// The current direction of flow.
	/// </summary>
	public readonly PanelDirection direction;

	/// <summary>
	/// Whether any spaces have been added yet for minimum size.
	/// </summary>
	private bool haveMinSpace;

	/// <summary>
	/// Whether any spaces have been added yet for preferred size.
	/// </summary>
	private bool havePrefSpace;

	/// <summary>
	/// The total sizes.
	/// </summary>
	public LayoutSizes total;

	internal BoxLayoutResults(PanelDirection direction, int presize)
	{
		children = new List<LayoutSizes>(presize);
		this.direction = direction;
		haveMinSpace = false;
		havePrefSpace = false;
		total = default(LayoutSizes);
	}

	/// <summary>
	/// Accumulates another component into the results.
	/// </summary>
	/// <param name="sizes">The size of the component to add.</param>
	/// <param name="spacing">The component spacing.</param>
	public void Accum(LayoutSizes sizes, float spacing)
	{
		float num = sizes.min;
		float num2 = sizes.preferred;
		if (num > 0f)
		{
			if (haveMinSpace)
			{
				num += spacing;
			}
			haveMinSpace = true;
		}
		total.min += num;
		if (num2 > 0f)
		{
			if (havePrefSpace)
			{
				num2 += spacing;
			}
			havePrefSpace = true;
		}
		total.preferred += num2;
		total.flexible += sizes.flexible;
	}

	/// <summary>
	/// Expands the results around another component.
	/// </summary>
	/// <param name="sizes">The size of the component to expand to.</param>
	public void Expand(LayoutSizes sizes)
	{
		float min = sizes.min;
		float preferred = sizes.preferred;
		float flexible = sizes.flexible;
		if (min > total.min)
		{
			total.min = min;
		}
		if (preferred > total.preferred)
		{
			total.preferred = preferred;
		}
		if (flexible > total.flexible)
		{
			total.flexible = flexible;
		}
	}

	public override string ToString()
	{
		string text = direction.ToString();
		LayoutSizes layoutSizes = total;
		return text + " " + layoutSizes.ToString();
	}
}
