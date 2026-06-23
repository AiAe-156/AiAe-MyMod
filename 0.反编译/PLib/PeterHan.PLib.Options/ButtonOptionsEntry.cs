using System;
using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry that displays a button. Not intended to be serializable to the
/// options file, instead declare a read-only property that returns a handler method as
/// an Action in the settings class, e.g:
///
/// [Option("Click Here!", "Button tool tip")]
/// public System.Action&lt;object&gt; MyButton =&gt; Handler;
///
/// public void Handler() {
///     // ...
/// }
/// </summary>
public class ButtonOptionsEntry : OptionsEntry
{
	/// <summary>
	/// The action to invoke when the button is pushed.
	/// </summary>
	private Action<object> value;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value is Action<object> action)
			{
				this.value = action;
			}
		}
	}

	public ButtonOptionsEntry(string field, IOptionSpec spec)
		: base(field, spec)
	{
	}

	public override void CreateUIEntry(PGridPanel parent, ref int row)
	{
		parent.AddChild(new PButton(base.Field)
		{
			Text = OptionsEntry.LookInStrings(base.Title),
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			OnClick = OnButtonClicked
		}.SetKleiPinkStyle(), new GridComponentSpec(row, 0)
		{
			Margin = OptionsEntry.CONTROL_MARGIN,
			Alignment = (TextAnchor)4,
			ColumnSpan = 2
		});
	}

	private void OnButtonClicked(GameObject _)
	{
		value?.Invoke(null);
	}

	public override GameObject GetUIComponent()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		return new GameObject("Empty");
	}
}
