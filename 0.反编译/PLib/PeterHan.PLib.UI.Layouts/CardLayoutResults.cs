using System.Collections.Generic;

namespace PeterHan.PLib.UI.Layouts;

/// <summary>
/// A class which stores the results of a single card layout calculation pass.
/// </summary>
internal sealed class CardLayoutResults
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
	/// The total sizes.
	/// </summary>
	public LayoutSizes total;

	internal CardLayoutResults(PanelDirection direction, int presize)
	{
		children = new List<LayoutSizes>(presize);
		this.direction = direction;
		total = default(LayoutSizes);
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
