using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UtilLibs.ElementUtilNamespace;

public class ElementGrouping
{
	public Tag GroupTag { get; private set; }

	public List<Element> GroupedElements { get; private set; }

	public ElementGrouping Exclude(Func<Element, bool> predicate)
	{
		GroupedElements.RemoveAll((Element e) => predicate(e));
		return this;
	}

	public ElementGrouping Include(Func<Element, bool> predicate, bool addGroupTagToElement = true)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		List<Element> list = ElementLoader.elements.Where(predicate).ToList();
		foreach (Element item in list)
		{
			if (!GroupedElements.Contains(item))
			{
				if (addGroupTagToElement && !item.oreTags.Contains(GroupTag))
				{
					item.oreTags = Util.Append<Tag>(item.oreTags, GroupTag);
				}
				GroupedElements.Add(item);
			}
		}
		return this;
	}

	public static ElementGrouping GroupAllWith(Tag groupTag)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		ElementGrouping elementGrouping = new ElementGrouping();
		elementGrouping.GroupTag = groupTag;
		elementGrouping.GroupedElements = ElementLoader.elements.Where((Element e) => e.HasTag(groupTag)).ToList();
		return elementGrouping;
	}

	public static implicit operator Tag(ElementGrouping info)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return info.GroupTag;
	}

	public static implicit operator List<Tag>(ElementGrouping info)
	{
		return info.GroupedElements.Select((Element e) => e.tag).ToList();
	}

	public static implicit operator Tag[](ElementGrouping info)
	{
		return info.GroupedElements.Select((Element e) => e.tag).ToArray();
	}

	public static implicit operator string(ElementGrouping info)
	{
		return string.Join("&", info.GroupedElements.Select((Element t) => ((object)Unsafe.As<Tag, Tag>(ref t.tag)/*cast due to .constrained prefix*/).ToString()));
	}
}
