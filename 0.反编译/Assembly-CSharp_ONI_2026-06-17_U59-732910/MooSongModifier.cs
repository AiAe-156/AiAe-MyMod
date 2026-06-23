using System;

public class MooSongModifier : Resource
{
	public delegate void MooSongModFn(BeckoningMonitor.Instance inst, Tag meteorTag);

	public string Description;

	public Tag TargetTag;

	public Func<string, string> TooltipCB;

	public MooSongModFn ApplyFunction;

	public MooSongModifier(string id, Tag targetTag, string name, string description, Func<string, string> tooltipCB, MooSongModFn applyFunction)
		: base(id, name)
	{
		Description = description;
		TargetTag = targetTag;
		TooltipCB = tooltipCB;
		ApplyFunction = applyFunction;
	}

	public string GetTooltip()
	{
		if (TooltipCB != null)
		{
			return TooltipCB(Description);
		}
		return Description;
	}
}
