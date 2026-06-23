using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry which represents bool and displays a check box.
/// </summary>
public class CheckboxOptionsEntry : OptionsEntry
{
	/// <summary>
	/// true if it is checked, or false otherwise
	/// </summary>
	private bool check;

	/// <summary>
	/// The realized item checkbox.
	/// </summary>
	private GameObject checkbox;

	public override object Value
	{
		get
		{
			return check;
		}
		set
		{
			if (value is bool flag)
			{
				check = flag;
				Update();
			}
		}
	}

	public CheckboxOptionsEntry(string field, IOptionSpec spec)
		: base(field, spec)
	{
		check = false;
		checkbox = null;
	}

	public override GameObject GetUIComponent()
	{
		checkbox = new PCheckBox
		{
			OnChecked = delegate(GameObject source, int state)
			{
				check = state == 0;
				Update();
			},
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip)
		}.SetKleiBlueStyle().Build();
		Update();
		return checkbox;
	}

	private void Update()
	{
		PCheckBox.SetCheckState(checkbox, check ? 1 : 0);
	}
}
