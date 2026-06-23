using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace UtilLibs.MarkdownExport;

public class MD_SubstanceTable : IMD_Entry
{
	private HashSet<Element> elements;

	private static StringBuilder sb = new StringBuilder();

	public MD_SubstanceTable(HashSet<Substance> substances)
	{
		elements = substances.Select((Substance s) => ElementLoader.FindElementByHash(s.elementID)).ToHashSet();
	}

	public MD_SubstanceTable(HashSet<SimHashes> substances)
	{
		elements = substances.Select((SimHashes s) => ElementLoader.FindElementByHash(s)).ToHashSet();
	}

	public MD_SubstanceTable(HashSet<Element> substances)
	{
		elements = substances;
	}

	public MD_SubstanceTable Add(SimHashes ele)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		elements.Add(ElementLoader.FindElementByHash(ele));
		return this;
	}

	public MD_SubstanceTable AddEnabled(SimHashes ele)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		elements.Add(ElementLoader.FindElementByHash(ele));
		return this;
	}

	public string FormatAsMarkdown()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		sb.Clear();
		foreach (Element item in from s in elements.Distinct()
			orderby MarkdownUtil.GetTagString(GameTagExtensions.CreateTag(s.id))
			select s)
		{
			string text = ((object)Unsafe.As<SimHashes, SimHashes>(ref item.id)/*cast due to .constrained prefix*/).ToString();
			Element element = ElementLoader.GetElement(item.tag);
			sb.AppendLine();
			sb.Append("#### ");
			sb.AppendLine(MarkdownUtil.GetTagString(GameTagExtensions.CreateTag(element.id)));
			sb.AppendLine();
			sb.AppendLine(MD_Localization.FormatLineBreaks(MarkdownUtil.GetTagString(GameTagExtensions.CreateTag(element.id), desc: true)));
			sb.AppendLine();
			sb.AppendLine("| |<font size=\"+1\">" + MD_Localization.L("ELEMENT_PROPERTIES") + "</font>| |");
			sb.AppendLine("|-|-|-|");
			sb.Append("| ![" + text + "](/assets/images/elements/" + text + ".png){width=\"200\"} |");
			sb.Append(MarkdownUtil.GetElementTransitionProperties(element));
			sb.Append("|");
			sb.Append(MarkdownUtil.GetElementPhysicalProperties(element));
			sb.AppendLine("|");
		}
		return sb.ToString();
	}
}
