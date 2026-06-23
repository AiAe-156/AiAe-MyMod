using System;

public class SymbolicConsumableItem : IConsumableUIItem
{
	private string id;

	private string name;

	private int majorOrder;

	private int minorOrder;

	private bool display;

	private string overrideSpriteName;

	private Func<bool> revealTest;

	string IConsumableUIItem.ConsumableId => id;

	string IConsumableUIItem.ConsumableName => name;

	int IConsumableUIItem.MajorOrder => majorOrder;

	int IConsumableUIItem.MinorOrder => minorOrder;

	bool IConsumableUIItem.Display => display;

	public SymbolicConsumableItem(string id, string name, int majorOrder, int minorOrder, bool display, string overrideSpriteName, Func<bool> revealTest)
	{
		this.id = id;
		this.name = name;
		this.majorOrder = majorOrder;
		this.minorOrder = minorOrder;
		this.display = display;
		this.overrideSpriteName = overrideSpriteName;
		this.revealTest = revealTest;
	}

	string IConsumableUIItem.OverrideSpriteName()
	{
		return overrideSpriteName;
	}

	bool IConsumableUIItem.RevealTest()
	{
		return revealTest();
	}
}
