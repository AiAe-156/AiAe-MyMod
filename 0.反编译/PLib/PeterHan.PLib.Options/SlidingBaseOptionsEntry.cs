using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry which displays a slider below it.
/// </summary>
public abstract class SlidingBaseOptionsEntry : OptionsEntry
{
	/// <summary>
	/// The margin between the slider extra row and the rest of the dialog.
	/// </summary>
	internal static readonly RectOffset ENTRY_MARGIN = new RectOffset(15, 0, 0, 5);

	/// <summary>
	/// The margin between the slider and its labels.
	/// </summary>
	internal static readonly RectOffset SLIDER_MARGIN = new RectOffset(10, 10, 0, 0);

	/// <summary>
	/// The limits allowed for the entry.
	/// </summary>
	protected readonly LimitAttribute limits;

	/// <summary>
	/// The realized slider.
	/// </summary>
	protected GameObject slider;

	protected SlidingBaseOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
		: base(field, spec)
	{
		limits = limit;
		slider = null;
	}

	public override void CreateUIEntry(PGridPanel parent, ref int row)
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		base.CreateUIEntry(parent, ref row);
		double minimum;
		double maximum;
		if (limits != null && (minimum = limits.Minimum) > -3.4028234663852886E+38 && (maximum = limits.Maximum) < 3.4028234663852886E+38 && maximum > minimum)
		{
			PSliderSingle pSliderSingle = GetSlider().AddOnRealize(OnRealizeSlider);
			PLabel pLabel = new PLabel("MinValue")
			{
				TextStyle = PUITuning.Fonts.TextLightStyle,
				Text = minimum.ToString(base.Format ?? "G3"),
				TextAlignment = (TextAnchor)5
			};
			PLabel pLabel2 = new PLabel("MaxValue")
			{
				TextStyle = PUITuning.Fonts.TextLightStyle,
				Text = maximum.ToString(base.Format ?? "G3"),
				TextAlignment = (TextAnchor)3
			};
			PRelativePanel child = new PRelativePanel("Slider Grid")
			{
				FlexSize = Vector2.right,
				DynamicSize = false
			}.AddChild(pSliderSingle).AddChild(pLabel).AddChild(pLabel2)
				.AnchorYAxis(pSliderSingle)
				.AnchorYAxis(pLabel)
				.AnchorYAxis(pLabel2)
				.SetLeftEdge(pLabel, 0f)
				.SetRightEdge(pLabel2, 1f)
				.SetLeftEdge(pSliderSingle, -1f, pLabel)
				.SetRightEdge(pSliderSingle, -1f, pLabel2)
				.SetMargin(pSliderSingle, SLIDER_MARGIN);
			parent.AddRow(new GridRowSpec());
			parent.AddChild(child, new GridComponentSpec(++row, 0)
			{
				ColumnSpan = 2,
				Margin = ENTRY_MARGIN
			});
		}
	}

	/// <summary>
	/// Gets the initialized PLib slider to be used for value display.
	/// </summary>
	/// <returns>The slider to be used.</returns>
	protected abstract PSliderSingle GetSlider();

	/// <summary>
	/// Called when the slider is realized.
	/// </summary>
	/// <param name="realized">The actual slider.</param>
	protected void OnRealizeSlider(GameObject realized)
	{
		slider = realized;
		Update();
	}

	/// <summary>
	/// Updates the displayed value.
	/// </summary>
	protected abstract void Update();
}
