using UnityEngine;

public class HarvestTool : DragTool
{
	public GameObject Placer;

	public static HarvestTool Instance;

	public Texture2D[] visualizerTextures;

	private ToolParameterMenu.ToggleData[] options;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	private bool IsOptionOn(string name)
	{
		for (int i = 0; i < options.Length; i++)
		{
			if (options[i].name == name)
			{
				return options[i].IsOn;
			}
		}
		return false;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
		options = new ToolParameterMenu.ToggleData[2]
		{
			new ToolParameterMenu.ToggleData("HARVEST_WHEN_READY", ToolParameterMenu.ToggleState.On),
			new ToolParameterMenu.ToggleData("DO_NOT_HARVEST", ToolParameterMenu.ToggleState.Off)
		};
		viewMode = OverlayModes.Harvest.ID;
	}

	protected override void OnDragTool(int cell, int distFromOrigin)
	{
		if (!Grid.IsValidCell(cell))
		{
			return;
		}
		foreach (HarvestDesignatable item in Components.HarvestDesignatables.Items)
		{
			OccupyArea area = item.area;
			if (Grid.PosToCell(item) != cell && (!(area != null) || !area.CheckIsOccupying(cell)))
			{
				continue;
			}
			if (IsOptionOn("HARVEST_WHEN_READY"))
			{
				item.SetHarvestWhenReady(state: true);
			}
			else if (IsOptionOn("DO_NOT_HARVEST"))
			{
				Harvestable component = item.GetComponent<Harvestable>();
				if (component != null)
				{
					component.Trigger(2127324410);
				}
				item.SetHarvestWhenReady(state: false);
			}
			Prioritizable component2 = item.GetComponent<Prioritizable>();
			if (component2 != null)
			{
				component2.SetMasterPriority(ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority());
			}
		}
	}

	public void Update()
	{
		MeshRenderer componentInChildren = visualizer.GetComponentInChildren<MeshRenderer>();
		if (componentInChildren != null)
		{
			if (IsOptionOn("HARVEST_WHEN_READY"))
			{
				componentInChildren.material.mainTexture = visualizerTextures[0];
			}
			else if (IsOptionOn("DO_NOT_HARVEST"))
			{
				componentInChildren.material.mainTexture = visualizerTextures[1];
			}
		}
	}

	public override void OnLeftClickUp(Vector3 cursor_pos)
	{
		base.OnLeftClickUp(cursor_pos);
	}

	protected override void OnActivateTool()
	{
		base.OnActivateTool();
		ToolMenu.Instance.PriorityScreen.Show();
		ToolMenu.Instance.toolParameterMenu.PopulateMenu(options);
	}

	protected override void OnDeactivateTool(InterfaceTool new_tool)
	{
		base.OnDeactivateTool(new_tool);
		ToolMenu.Instance.PriorityScreen.Show(show: false);
		ToolMenu.Instance.toolParameterMenu.ClearMenu();
	}
}
