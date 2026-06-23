using System;

public struct DevQuickActionInstruction
{
	public string Address;

	public System.Action Action;

	public DevQuickActionInstruction(IDevQuickAction.CommonMenusNames category, string name, System.Action action)
		: this(category.ToString() + "/" + name, action)
	{
	}

	public DevQuickActionInstruction(string address, System.Action action)
	{
		Address = address;
		Action = action;
	}
}
