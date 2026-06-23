using ImGuiNET;

public class DevToolDebugModeToggle : DevTool
{
	public DevToolDebugModeToggle()
	{
		RequiresGameRunning = true;
	}

	protected override void RenderTo(DevPanel panel)
	{
		bool v = DebugHandler.InstantBuildMode;
		if (ImGui.Checkbox("Instant Build Mode (Ctrl+F4)", ref v))
		{
			DebugHandler.ToggleInstantBuildMode();
		}
	}
}
