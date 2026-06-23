using System;

[Serializable]
public struct ButtonMenuTextOverride
{
	public LocString Text;

	public LocString CancelText;

	public LocString ToolTip;

	public LocString CancelToolTip;

	public bool IsValid => !string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(ToolTip);

	public bool HasCancelText => !string.IsNullOrEmpty(CancelText) && !string.IsNullOrEmpty(CancelToolTip);
}
