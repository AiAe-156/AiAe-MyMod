using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry that displays static text. Not intended to be serializable to the
/// options file, instead declare a read-only property that returns null with a type of
/// LocText, e.g:
///
/// [Option("Your text goes here", "Tool tip for the text")]
/// public LocText MyLabel =&gt; null;
///
/// Unity font formatting can be used in the text. The name of a strings table entry can
/// also be used to allow localization.
/// </summary>
public class TextBlockOptionsEntry : OptionsEntry
{
	/// <summary>
	/// A font style that looks like TextLightStyle but allows word wrapping.
	/// </summary>
	private static readonly TextStyleSetting WRAP_TEXT_STYLE;

	/// <summary>
	/// This value is not used, it only exists to satisfy the contract.
	/// </summary>
	private LocText ignore;

	public override object Value
	{
		get
		{
			return ignore;
		}
		set
		{
			LocText val = (LocText)((value is LocText) ? value : null);
			if (val != null)
			{
				ignore = val;
			}
		}
	}

	static TextBlockOptionsEntry()
	{
		WRAP_TEXT_STYLE = PUITuning.Fonts.TextLightStyle.DeriveStyle();
		WRAP_TEXT_STYLE.enableWordWrapping = true;
	}

	public TextBlockOptionsEntry(string field, IOptionSpec spec)
		: base(field, spec)
	{
	}

	public override void CreateUIEntry(PGridPanel parent, ref int row)
	{
		parent.AddChild(new PLabel(base.Field)
		{
			Text = OptionsEntry.LookInStrings(base.Title),
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			TextStyle = WRAP_TEXT_STYLE
		}, new GridComponentSpec(row, 0)
		{
			Margin = OptionsEntry.CONTROL_MARGIN,
			Alignment = (TextAnchor)4,
			ColumnSpan = 2
		});
	}

	public override GameObject GetUIComponent()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		return new GameObject("Empty");
	}
}
