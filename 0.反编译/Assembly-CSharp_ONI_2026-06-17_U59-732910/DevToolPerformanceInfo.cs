using System.Collections.Generic;
using ImGuiNET;
using UnityEngine;

public class DevToolPerformanceInfo : DevTool
{
	private PerformanceMonitor performanceMonitor;

	protected override void RenderTo(DevPanel panel)
	{
		if (Game.Instance == null)
		{
			ImGui.Text("No game loaded");
			return;
		}
		if (performanceMonitor == null)
		{
			performanceMonitor = Global.Instance.GetComponent<PerformanceMonitor>();
		}
		float fPS = performanceMonitor.FPS;
		float num = 1000f / fPS;
		ImGui.Text($"{num:0.00} ms ({fPS:0.00} fps)");
		ImGui.NewLine();
		ImGui.Separator();
		if (ImGui.CollapsingHeader("Brains") && Game.BrainScheduler.debugGetBrainGroups() != null)
		{
			List<BrainScheduler.BrainGroup> list = Game.BrainScheduler.debugGetBrainGroups();
			for (int i = 0; i < list.Count; i++)
			{
				ImGui.PushID(i);
				BrainScheduler.BrainGroup brainGroup = list[i];
				ImGui.Text(brainGroup.tag.ToString());
				ImGui.Indent();
				ImGui.Text("Brain count: " + brainGroup.BrainCount);
				ImGui.PushID(i);
				ImGui.Checkbox("Freeze AdjustLoad", ref brainGroup.debugFreezeLoadAdustment);
				ImGui.PopID();
				ImGui.SameLine();
				ImGui.Text("Max priority brain count seen: " + brainGroup.debugMaxPriorityBrainCountSeen);
				ImGui.SameLine();
				if (ImGui.Button("Reset"))
				{
					brainGroup.debugMaxPriorityBrainCountSeen = 0;
				}
				ImGui.PopID();
				ImGui.Unindent();
			}
		}
		if (!ImGui.CollapsingHeader("Camera Culling"))
		{
			return;
		}
		if (CameraController.Instance == null)
		{
			ImGui.Text("No camera instance");
			return;
		}
		GridVisibleArea visibleArea = CameraController.Instance.VisibleArea;
		ImGui.Checkbox("Freeze visible area", ref visibleArea.debugFreezeVisibleArea);
		ImGui.Checkbox("Freeze visible area extended", ref visibleArea.debugFreezeVisibleAreasExtended);
		Vector2I min = visibleArea.CurrentArea.Min;
		Vector2I max = visibleArea.CurrentArea.Max;
		Option<(Vector2, Vector2)> screenRect = new DevToolEntityTarget.ForSimCell(Grid.XYToCell(min.x, min.y)).GetScreenRect();
		Option<(Vector2, Vector2)> screenRect2 = new DevToolEntityTarget.ForSimCell(Grid.XYToCell(max.x, max.y)).GetScreenRect();
		if (screenRect.IsSome() && screenRect2.IsSome())
		{
			DevToolEntity.DrawScreenRect((cornerA: Vector2.Min(screenRect.Unwrap().Item1, Vector2.Min(screenRect.Unwrap().Item2, Vector2.Min(screenRect2.Unwrap().Item1, screenRect2.Unwrap().Item2))), cornerB: Vector2.Max(screenRect.Unwrap().Item1, Vector2.Max(screenRect.Unwrap().Item2, Vector2.Max(screenRect2.Unwrap().Item1, screenRect2.Unwrap().Item2)))), "", new Option<Color>(Color.red));
		}
		min = visibleArea.CurrentAreaExtended.Min;
		max = visibleArea.CurrentAreaExtended.Max;
		screenRect = new DevToolEntityTarget.ForSimCell(Grid.XYToCell(min.x, min.y)).GetScreenRect();
		screenRect2 = new DevToolEntityTarget.ForSimCell(Grid.XYToCell(max.x, max.y)).GetScreenRect();
		if (screenRect.IsSome() && screenRect2.IsSome())
		{
			DevToolEntity.DrawScreenRect((cornerA: Vector2.Min(screenRect.Unwrap().Item1, Vector2.Min(screenRect.Unwrap().Item2, Vector2.Min(screenRect2.Unwrap().Item1, screenRect2.Unwrap().Item2))), cornerB: Vector2.Max(screenRect.Unwrap().Item1, Vector2.Max(screenRect.Unwrap().Item2, Vector2.Max(screenRect2.Unwrap().Item1, screenRect2.Unwrap().Item2)))), "", new Option<Color>(Color.cyan));
		}
	}
}
