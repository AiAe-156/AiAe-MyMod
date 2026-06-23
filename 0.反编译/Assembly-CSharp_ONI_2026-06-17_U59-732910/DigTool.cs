using UnityEngine;

public class DigTool : FilteredDragTool
{
	public static DigTool Instance;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
	}

	protected override void GetDefaultFilters(out ToolParameterMenu.ToggleData[] filters)
	{
		filters = new ToolParameterMenu.ToggleData[3]
		{
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.TILES, ToolParameterMenu.ToggleState.On, isToggleInclusive: true),
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.NATURALBACKWALL, ToolParameterMenu.ToggleState.Off, isToggleInclusive: true),
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.UPROOTPLANTS, ToolParameterMenu.ToggleState.On, isToggleInclusive: true)
		};
	}

	protected override void OnOverlayChanged(HashedString overlay)
	{
		if (base.IsActive)
		{
			ToolMenu.Instance.toolParameterMenu.PopulateMenu(currentFilters);
		}
	}

	protected override void OnDragTool(int cell, int distFromOrigin)
	{
		if (IsActiveLayer(ToolParameterMenu.FILTERLAYERS.UPROOTPLANTS))
		{
			InterfaceTool.ActiveConfig.DigAction.Uproot(cell);
		}
		InterfaceTool.ActiveConfig.DigAction.Dig(cell, distFromOrigin);
	}

	public static GameObject PlaceDig(int cell, int animationDelay = 0)
	{
		bool flag = Instance.IsActiveLayer(ToolParameterMenu.FILTERLAYERS.TILES);
		bool flag2 = Instance.IsActiveLayer(ToolParameterMenu.FILTERLAYERS.NATURALBACKWALL);
		bool flag3 = Grid.Solid[cell] && !Grid.Foundation[cell];
		bool flag4 = !Grid.Solid[cell] && BackwallManager.HasBackwall(cell) && !Grid.Foundation[cell];
		if (Grid.Objects[cell, 7] == null && ((flag3 && flag) || (flag4 && flag2)))
		{
			for (int i = 0; i < 45; i++)
			{
				if (Grid.Objects[cell, i] != null && Grid.Objects[cell, i].GetComponent<Constructable>() != null)
				{
					return null;
				}
			}
			GameObject gameObject = Util.KInstantiate(Assets.GetPrefab(new Tag("DigPlacer")));
			gameObject.GetComponent<Diggable>().digTypeFlags = (flag ? 1 : 0) | (flag2 ? 2 : 0);
			gameObject.SetActive(value: true);
			Grid.Objects[cell, 7] = gameObject;
			Vector3 position = Grid.CellToPosCBC(cell, Instance.visualizerLayer);
			float num = -0.15f;
			position.z += num;
			gameObject.transform.SetPosition(position);
			gameObject.GetComponentInChildren<EasingAnimations>().PlayAnimation("ScaleUp", Mathf.Max(0f, (float)animationDelay * 0.02f));
			return gameObject;
		}
		if (Grid.Objects[cell, 7] != null)
		{
			return Grid.Objects[cell, 7];
		}
		return null;
	}

	protected override void OnActivateTool()
	{
		base.OnActivateTool();
		ToolMenu.Instance.PriorityScreen.Show();
	}

	protected override void OnDeactivateTool(InterfaceTool new_tool)
	{
		base.OnDeactivateTool(new_tool);
		ToolMenu.Instance.PriorityScreen.Show(show: false);
	}
}
