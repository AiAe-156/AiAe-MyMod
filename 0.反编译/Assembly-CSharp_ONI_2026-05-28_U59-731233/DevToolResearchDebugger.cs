using System.Collections.Generic;
using ImGuiNET;
using UnityEngine;

public class DevToolResearchDebugger : DevTool
{
	public DevToolResearchDebugger()
	{
		RequiresGameRunning = true;
	}

	protected override void RenderTo(DevPanel panel)
	{
		TechInstance activeResearch = Research.Instance.GetActiveResearch();
		if (activeResearch == null)
		{
			ImGui.Text("No Active Research");
			return;
		}
		ImGui.Text("Active Research");
		ImGui.Text("ID: " + activeResearch.tech.Id);
		ImGui.Text("Name: " + Util.StripTextFormatting(activeResearch.tech.Name));
		ImGui.Separator();
		ImGui.Text("Active Research Inventory");
		Dictionary<string, float> dictionary = new Dictionary<string, float>(activeResearch.progressInventory.PointsByTypeID);
		foreach (KeyValuePair<string, float> item in dictionary)
		{
			if (activeResearch.tech.RequiresResearchType(item.Key))
			{
				float num = activeResearch.tech.costsByResearchTypeID[item.Key];
				float v = item.Value;
				if (ImGui.Button("Fill"))
				{
					v = num;
				}
				ImGui.SameLine();
				ImGui.SetNextItemWidth(100f);
				ImGui.InputFloat(item.Key, ref v, 1f, 10f);
				ImGui.SameLine();
				ImGui.Text($"of {num}");
				activeResearch.progressInventory.PointsByTypeID[item.Key] = Mathf.Clamp(v, 0f, num);
			}
		}
		ImGui.Separator();
		ImGui.Text("Global Points Inventory");
		ResearchPointInventory globalPointInventory = Research.Instance.globalPointInventory;
		foreach (KeyValuePair<string, float> item2 in globalPointInventory.PointsByTypeID)
		{
			ImGui.Text(item2.Key + ": " + item2.Value);
		}
	}
}
