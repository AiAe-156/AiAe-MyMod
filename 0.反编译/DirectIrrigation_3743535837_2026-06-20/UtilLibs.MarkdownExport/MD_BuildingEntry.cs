using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UtilLibs.BuildingPortUtils;

namespace UtilLibs.MarkdownExport;

public class MD_BuildingEntry : IMD_Entry
{
	public bool IsVanillaModified = false;

	private string ID;

	public int PowerConsumption;

	public int PowerProduction;

	public int Width;

	public int Height;

	public string ResearchKey;

	public int StorageCapacity;

	private BuildingDef def;

	public List<IMD_Entry> Children;

	public List<Tuple<string, int>> Costs = new List<Tuple<string, int>>();

	private static StringBuilder sb = new StringBuilder();

	private static string FontSizeIncrease(string text, int increase = 1)
	{
		return $"<font size=\"+{increase}\">{text}</font>";
	}

	public string FormatAsMarkdown()
	{
		sb.Clear();
		sb.Append("## " + MD_Localization.Strip(MD_Localization.L("STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".NAME")));
		if (IsVanillaModified)
		{
			sb.Append(" ");
			sb.Append(MD_Localization.L("MODIFIED_SUFFIX"));
		}
		sb.AppendLine();
		sb.AppendLine(MD_Localization.Strip(MD_Localization.L("STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".DESC")));
		sb.AppendLine();
		sb.AppendLine(MD_Localization.Strip(MD_Localization.L("STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".EFFECT")));
		sb.AppendLine();
		sb.AppendLine("| | | |");
		sb.AppendLine("|-|-|-|");
		sb.Append("| ![" + ID + "](/assets/images/buildings/" + ID + ".png){height=\"100\"} {rowspan=\"3\"}");
		sb.AppendLine("|**" + MD_Localization.L("BUILDING_DIMENSIONS_LABEL") + "** | " + string.Format(MD_Localization.L("BUILDING_DIMENSIONS_INFO"), Width, Height) + "|");
		string text = MarkdownUtil.GetFormattedWattage(PowerConsumption, (WattageFormatterUnit)2);
		if (PowerConsumption <= 0)
		{
			text = " - ";
		}
		if (PowerProduction > 0)
		{
			text = MarkdownUtil.GetFormattedWattage(PowerProduction, (WattageFormatterUnit)2);
		}
		if (PowerProduction > 0)
		{
			sb.AppendLine("|**" + MD_Localization.L("BUILDING_POWER_GENERATION") + "**| " + text + " |&#8288 {: style=\"padding:0\"}|");
		}
		else
		{
			sb.AppendLine("|**" + MD_Localization.L("BUILDING_POWER_CONSUMPTION") + "**| " + text + " |&#8288 {: style=\"padding:0\"}|");
		}
		if (!Util.IsNullOrWhiteSpace(ResearchKey))
		{
			sb.AppendLine("|**" + MD_Localization.L("BUILDING_RESEARCH_REQUIREMENT") + "**| " + MD_Localization.Strip(MD_Localization.L(ResearchKey)) + "|&#8288 {: style=\"padding:0\"}| ");
		}
		else
		{
			sb.AppendLine("|**" + MD_Localization.L("BUILDING_RESEARCH_REQUIREMENT") + "**| - |&#8288 {: style=\"padding:0\"}| ");
		}
		AppendMaterialCostsTable(sb);
		if (StorageCapacity > 0)
		{
			sb.AppendLine("|**" + FontSizeIncrease(MD_Localization.L("BUILDING_STORAGE_CAPACITY")) + "**| " + MarkdownUtil.GetFormattedMass(StorageCapacity, (TimeSlice)0, (MetricMassFormat)0) + "|&#8288 {: style=\"padding:0\"}|");
		}
		AppendBuildingPortsTable(sb);
		sb.AppendLine();
		foreach (IMD_Entry child in Children)
		{
			sb.AppendLine(child.FormatAsMarkdown());
		}
		return sb.ToString();
	}

	private void AppendBuildingPortsTable(StringBuilder sb)
	{
		AppendBuildingPorts(sb, htmlTable: true);
	}

	private void AppendBuildingPorts(StringBuilder sb, bool htmlTable = false)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		if ((int)def.InputConduitType > 0)
		{
			Tag val = GameTags.Any;
			ConduitConsumer val2 = default(ConduitConsumer);
			SolidConduitConsumer val3 = default(SolidConduitConsumer);
			if (def.BuildingComplete.TryGetComponent<ConduitConsumer>(ref val2))
			{
				val = val2.capacityTag;
			}
			else if (def.BuildingComplete.TryGetComponent<SolidConduitConsumer>(ref val3))
			{
				val = val3.capacityTag;
			}
			string material = ((val == GameTags.Any) ? null : MarkdownUtil.GetTagString(val));
			list.Add(MarkdownUtil.GetPortDescription(def.InputConduitType, input: true, material));
		}
		PortConduitConsumer[] components = def.BuildingComplete.GetComponents<PortConduitConsumer>();
		if (components.Any())
		{
			PortConduitConsumer[] array = components;
			foreach (PortConduitConsumer portConduitConsumer in array)
			{
				string material2 = ((portConduitConsumer.capacityTag == GameTags.Any) ? null : MarkdownUtil.GetTagString(portConduitConsumer.capacityTag));
				list.Add(MarkdownUtil.GetPortDescription(portConduitConsumer.conduitType, input: true, material2));
			}
		}
		List<ConduitType> list3 = new List<ConduitType>(3)
		{
			(ConduitType)3,
			(ConduitType)2,
			(ConduitType)1
		};
		ISecondaryInput[] components2 = def.BuildingComplete.GetComponents<ISecondaryInput>();
		if (components2.Any())
		{
			ISecondaryInput[] array2 = components2;
			foreach (ISecondaryInput val4 in array2)
			{
				foreach (ConduitType item in list3)
				{
					if (val4.HasSecondaryConduitType(item))
					{
						list.Add(MarkdownUtil.GetPortDescription(item, input: true));
					}
				}
			}
		}
		if ((int)def.OutputConduitType > 0)
		{
			list2.Add(MarkdownUtil.GetPortDescription(def.OutputConduitType, input: false));
		}
		PortConduitDispenserBase[] components3 = def.BuildingComplete.GetComponents<PortConduitDispenserBase>();
		if (components3.Any())
		{
			PortConduitDispenserBase[] array3 = components3;
			foreach (PortConduitDispenserBase portConduitDispenserBase in array3)
			{
				Tag val5 = GameTags.Any;
				if (portConduitDispenserBase.elementFilter != null && portConduitDispenserBase.elementFilter.Any())
				{
					val5 = GameTagExtensions.CreateTag(portConduitDispenserBase.elementFilter[0]);
				}
				else if (portConduitDispenserBase.tagFilter != null && portConduitDispenserBase.tagFilter.Any())
				{
					val5 = portConduitDispenserBase.tagFilter[0];
				}
				string material3 = ((val5 == GameTags.Any) ? null : MarkdownUtil.GetTagString(val5));
				list2.Add(MarkdownUtil.GetPortDescription(portConduitDispenserBase.conduitType, input: false, material3));
			}
		}
		ISecondaryOutput[] components4 = def.BuildingComplete.GetComponents<ISecondaryOutput>();
		if (components4.Any())
		{
			ISecondaryOutput[] array4 = components4;
			foreach (ISecondaryOutput val6 in array4)
			{
				foreach (ConduitType item2 in list3)
				{
					if (val6.HasSecondaryConduitType(item2))
					{
						list2.Add(MarkdownUtil.GetPortDescription(item2, input: false));
					}
				}
			}
		}
		if (!list.Any() && !list2.Any())
		{
			return;
		}
		if (!htmlTable)
		{
			sb.AppendLine();
			sb.AppendLine("### " + MD_Localization.L("BUILDING_PORTS_HEADER"));
			sb.AppendLine("|" + MD_Localization.L("INPUTS_HEADER") + "|" + MD_Localization.L("OUTPUTS_HEADER") + "|");
			sb.AppendLine("|-|-|");
			int num = Math.Max(list.Count, list2.Count);
			for (int m = 0; m < num; m++)
			{
				string value = ((list.Count > m) ? list[m] : "-");
				string value2 = ((list2.Count > m) ? list2[m] : "-");
				sb.Append('|');
				sb.Append(value);
				sb.Append('|');
				sb.Append(value2);
				sb.AppendLine("|");
			}
			return;
		}
		sb.Append("| **<font size=\"+1\">" + MD_Localization.L("BUILDING_PORTS_HEADER") + ":</font>** |");
		sb.Append("<table>");
		sb.Append("<tr>");
		sb.Append("<th>" + MD_Localization.L("INPUTS_HEADER") + "</th>");
		sb.Append("<th>" + MD_Localization.L("OUTPUTS_HEADER") + "</th>");
		sb.Append("</tr>");
		int num2 = Math.Max(list.Count, list2.Count);
		for (int n = 0; n < num2; n++)
		{
			sb.Append("<tr>");
			sb.Append("<td>");
			sb.Append((list.Count > n) ? list[n] : "-");
			sb.Append("</td>");
			sb.Append("<td>");
			sb.Append((list2.Count > n) ? list2[n] : "-");
			sb.Append("</td>");
			sb.Append("</tr>");
		}
		sb.Append("</table>");
		sb.Append(" {colspan=\"2\"}");
		sb.Append('|');
		sb.Append("&#8288 {: style=\"padding:0\"}");
		sb.AppendLine("|");
	}

	private void AppendMaterialCostsTable(StringBuilder sb)
	{
		sb.Append("|**<font size=\"+1\">" + MD_Localization.L("BUILDING_MATERIAL_COST_HEADER") + "</font>**|");
		sb.Append("<table>");
		foreach (Tuple<string, int> cost in Costs)
		{
			sb.Append("<tr>");
			int second = cost.second;
			string[] source = cost.first.Split('&');
			string separator = " " + MD_Localization.L("SEPARATOR_OR") + " ";
			sb.Append("<td>");
			sb.Append(string.Join(separator, source.Select((string m) => MarkdownUtil.GetTagString(Tag.op_Implicit(m)))));
			sb.Append("</td>");
			sb.Append("<td>");
			sb.Append(MarkdownUtil.GetFormattedMass(second, (TimeSlice)0, (MetricMassFormat)0));
			sb.Append("</td>");
			sb.Append("</tr>");
		}
		sb.Append("</table>");
		sb.Append(" {colspan=\"2\"} |");
		sb.Append("&#8288 {: style=\"padding:0\"}");
		sb.AppendLine("|");
	}

	private void AppendMaterialCosts(StringBuilder sb)
	{
		sb.AppendLine();
		sb.AppendLine("|**<font size=\"+1\">" + MD_Localization.L("BUILDING_MATERIAL_COST_HEADER") + "</font>**| |");
		sb.AppendLine("|-|-|");
		foreach (Tuple<string, int> cost in Costs)
		{
			sb.Append('|');
			int second = cost.second;
			string[] source = cost.first.Split('&');
			string separator = " " + MD_Localization.L("SEPARATOR_OR") + " ";
			sb.Append(string.Join(separator, source.Select((string m) => MarkdownUtil.GetTagString(Tag.op_Implicit(m)))));
			sb.Append('|');
			sb.Append(MarkdownUtil.GetFormattedMass(second, (TimeSlice)0, (MetricMassFormat)0));
			sb.AppendLine("|");
		}
	}

	public MD_BuildingEntry WriteUISprite(string path)
	{
		KAnimFile kanimFile = def.AnimFiles.First();
		Exporter.WriteUISprite(path, ID, kanimFile);
		return this;
	}

	public MD_BuildingEntry(string id)
	{
		ID = id;
		CollectInfo(id);
	}

	public MD_BuildingEntry Tech(string techId)
	{
		ResearchKey = "STRINGS.RESEARCH.TECHS." + techId.ToUpperInvariant() + ".NAME";
		return this;
	}

	private void CollectInfo(string id)
	{
		BuildingDef val = (def = Assets.GetBuildingDef(id));
		if ((Object)(object)val == (Object)null)
		{
			SgtLogger.error("No BuildingDef found for " + id);
			return;
		}
		PowerConsumption = Mathf.RoundToInt(val.EnergyConsumptionWhenActive);
		PowerProduction = Mathf.RoundToInt(val.GeneratorWattageRating);
		Width = val.WidthInCells;
		Height = val.HeightInCells;
		Children = new List<IMD_Entry>();
		for (int i = 0; i < val.MaterialCategory.Length; i++)
		{
			string text = val.MaterialCategory[i];
			float num = val.Mass[i];
			Costs.Add(new Tuple<string, int>(text, (int)num));
		}
		EnergyGenerator gen = default(EnergyGenerator);
		if (val.BuildingComplete.TryGetComponent<EnergyGenerator>(ref gen))
		{
			Children.Add(new MD_Header("CONVERSION_GENERATOR_HEADER", 4));
			Children.Add(new MD_EnergyGenerator(gen));
		}
		foreach (TechItem resource in ((ResourceSet<TechItem>)(object)Db.Get().TechItems).resources)
		{
			if (((Resource)resource).Id == id)
			{
				ResearchKey = "STRINGS.RESEARCH.TECHS." + ((Resource)resource.ParentTech).Id.ToUpperInvariant() + ".NAME";
				break;
			}
		}
		ElementConverter[] components = val.BuildingComplete.GetComponents<ElementConverter>();
		if (components != null && components.Any())
		{
			Children.Add(new MD_Header("CONVERSION_ELEMENT_HEADER", 4));
			ElementConverter[] array = components;
			foreach (ElementConverter converter in array)
			{
				Children.Add(new MD_ElementConverter(converter));
			}
		}
		if ((Object)(object)val.BuildingComplete.GetComponent<ComplexFabricator>() != (Object)null)
		{
			Children.Add(new MD_Header("RECIPES_HEADER", 3));
			List<ComplexRecipe> recipes = ComplexRecipeManager.Get().recipes.FindAll((ComplexRecipe recipe) => recipe.fabricators[0] == Tag.op_Implicit(id));
			Children.Add(new MD_ComplexRecipes(recipes));
		}
		SmartReservoir val2 = default(SmartReservoir);
		TreeFilterable val3 = default(TreeFilterable);
		Storage val4 = default(Storage);
		StorageLocker val5 = default(StorageLocker);
		if ((val.BuildingComplete.TryGetComponent<SmartReservoir>(ref val2) || val.BuildingComplete.TryGetComponent<TreeFilterable>(ref val3)) && val.BuildingComplete.TryGetComponent<Storage>(ref val4) && val4.showInUI)
		{
			StorageCapacity = Mathf.RoundToInt(val4.Capacity());
		}
		else if (val.BuildingComplete.TryGetComponent<StorageLocker>(ref val5))
		{
			StorageCapacity = Mathf.RoundToInt(val5.MaxCapacity);
		}
		if (SupplyClosetUtils.TryGetCollectionFor(id, out var _))
		{
			Children.Add(new MD_Header("STRINGS.UI.UISIDESCREENS.TABS.SKIN", 3));
			Children.Add(new MD_BlueprintCollectionEntry(id, title: false));
		}
	}

	public MD_BuildingEntry VanillaModified()
	{
		IsVanillaModified = true;
		return this;
	}
}
