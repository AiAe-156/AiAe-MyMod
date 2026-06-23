using System;
using UnityEngine;

public class FilteredDragTool : DragTool
{
	protected ToolParameterMenu.ToggleData[] currentFilters = new ToolParameterMenu.ToggleData[0];

	private ToolParameterMenu.ToggleData[] userSelectedFilters;

	private bool active;

	private HashedString lastAppliedOverlay;

	private bool isOverlayDriven;

	protected bool IsActive => active;

	private bool IsFilterOn(string name)
	{
		for (int i = 0; i < currentFilters.Length; i++)
		{
			if (currentFilters[i].name == name)
			{
				return currentFilters[i].IsOn;
			}
		}
		return false;
	}

	public bool IsActiveLayer(string layer)
	{
		if (!IsFilterOn(ToolParameterMenu.FILTERLAYERS.ALL))
		{
			return IsFilterOn(layer.ToUpper());
		}
		return true;
	}

	public bool IsActiveLayer(ObjectLayer layer)
	{
		if (IsFilterOn(ToolParameterMenu.FILTERLAYERS.ALL))
		{
			return true;
		}
		for (int i = 0; i < currentFilters.Length; i++)
		{
			if (currentFilters[i].IsOn && GetObjectLayerFromFilterLayer(currentFilters[i].name) == layer)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void GetDefaultFilters(out ToolParameterMenu.ToggleData[] filters)
	{
		filters = new ToolParameterMenu.ToggleData[8]
		{
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.ALL, ToolParameterMenu.ToggleState.On),
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.WIRES, ToolParameterMenu.ToggleState.Off),
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.LIQUIDCONDUIT, ToolParameterMenu.ToggleState.Off),
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.GASCONDUIT, ToolParameterMenu.ToggleState.Off),
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.SOLIDCONDUIT, ToolParameterMenu.ToggleState.Off),
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.BUILDINGS, ToolParameterMenu.ToggleState.Off),
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.LOGIC, ToolParameterMenu.ToggleState.Off),
			new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.BACKWALL, ToolParameterMenu.ToggleState.Off)
		};
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		ResetFilter();
		userSelectedFilters = CloneFilters(currentFilters);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		OverlayScreen instance = OverlayScreen.Instance;
		instance.OnOverlayChanged = (Action<HashedString>)Delegate.Combine(instance.OnOverlayChanged, new Action<HashedString>(OnOverlayChanged));
	}

	protected override void OnCleanUp()
	{
		OverlayScreen instance = OverlayScreen.Instance;
		instance.OnOverlayChanged = (Action<HashedString>)Delegate.Remove(instance.OnOverlayChanged, new Action<HashedString>(OnOverlayChanged));
		base.OnCleanUp();
	}

	public void ResetFilter()
	{
		GetDefaultFilters(out currentFilters);
	}

	private ToolParameterMenu.ToggleData[] CloneFilters(ToolParameterMenu.ToggleData[] source)
	{
		ToolParameterMenu.ToggleData[] array = new ToolParameterMenu.ToggleData[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			array[i] = new ToolParameterMenu.ToggleData(source[i].name, source[i].state, source[i].isToggleInclusive);
		}
		return array;
	}

	private void SaveUserFilters()
	{
		userSelectedFilters = CloneFilters(currentFilters);
	}

	private void RestoreUserFilters()
	{
		if (userSelectedFilters != null)
		{
			currentFilters = CloneFilters(userSelectedFilters);
		}
		else
		{
			ResetFilter();
		}
	}

	private void OnParametersChanged()
	{
		if (!isOverlayDriven)
		{
			SaveUserFilters();
		}
	}

	protected override void OnActivateTool()
	{
		active = true;
		base.OnActivateTool();
		ToolMenu.Instance.toolParameterMenu.onParametersChanged += OnParametersChanged;
		HashedString hashedString = OverlayScreen.Instance.mode;
		if (hashedString != lastAppliedOverlay)
		{
			OnOverlayChanged(hashedString);
		}
		else
		{
			ToolMenu.Instance.toolParameterMenu.PopulateMenu(currentFilters);
		}
	}

	protected override void OnDeactivateTool(InterfaceTool new_tool)
	{
		active = false;
		ToolMenu.Instance.toolParameterMenu.onParametersChanged -= OnParametersChanged;
		ToolMenu.Instance.toolParameterMenu.ClearMenu();
		base.OnDeactivateTool(new_tool);
	}

	public virtual string GetFilterLayerFromGameObject(GameObject input)
	{
		BuildingComplete component = input.GetComponent<BuildingComplete>();
		BuildingUnderConstruction component2 = input.GetComponent<BuildingUnderConstruction>();
		if ((bool)component)
		{
			return GetFilterLayerFromObjectLayer(component.Def.ObjectLayer);
		}
		if ((bool)component2)
		{
			return GetFilterLayerFromObjectLayer(component2.Def.ObjectLayer);
		}
		if (input.GetComponent<Clearable>() != null || input.GetComponent<Moppable>() != null)
		{
			return "CleanAndClear";
		}
		if (input.GetComponent<Diggable>() != null)
		{
			return "DigPlacer";
		}
		return "Default";
	}

	public string GetFilterLayerFromObjectLayer(ObjectLayer gamer_layer)
	{
		switch (gamer_layer)
		{
		case ObjectLayer.Building:
		case ObjectLayer.Gantry:
			return "Buildings";
		case ObjectLayer.Wire:
		case ObjectLayer.WireConnectors:
			return "Wires";
		case ObjectLayer.LiquidConduit:
		case ObjectLayer.LiquidConduitConnection:
			return "LiquidPipes";
		case ObjectLayer.GasConduit:
		case ObjectLayer.GasConduitConnection:
			return "GasPipes";
		case ObjectLayer.SolidConduit:
		case ObjectLayer.SolidConduitConnection:
			return "SolidConduits";
		case ObjectLayer.FoundationTile:
			return "Tiles";
		case ObjectLayer.LogicGate:
		case ObjectLayer.LogicWire:
			return "Logic";
		case ObjectLayer.Backwall:
			return "BackWall";
		default:
			return "Default";
		}
	}

	private ObjectLayer GetObjectLayerFromFilterLayer(string filter_layer)
	{
		ObjectLayer objectLayer = ObjectLayer.NumLayers;
		return filter_layer.ToLower() switch
		{
			"buildings" => ObjectLayer.Building, 
			"wires" => ObjectLayer.Wire, 
			"liquidpipes" => ObjectLayer.LiquidConduit, 
			"gaspipes" => ObjectLayer.GasConduit, 
			"solidconduits" => ObjectLayer.SolidConduit, 
			"tiles" => ObjectLayer.FoundationTile, 
			"logic" => ObjectLayer.LogicWire, 
			"backwall" => ObjectLayer.Backwall, 
			_ => throw new ArgumentException("Invalid filter layer: " + filter_layer), 
		};
	}

	protected virtual void OnOverlayChanged(HashedString overlay)
	{
		if (!active || GameUtil.IsCapturingTimeLapse())
		{
			return;
		}
		lastAppliedOverlay = overlay;
		string text = null;
		if (overlay == OverlayModes.Power.ID)
		{
			text = ToolParameterMenu.FILTERLAYERS.WIRES;
		}
		else if (overlay == OverlayModes.LiquidConduits.ID)
		{
			text = ToolParameterMenu.FILTERLAYERS.LIQUIDCONDUIT;
		}
		else if (overlay == OverlayModes.GasConduits.ID)
		{
			text = ToolParameterMenu.FILTERLAYERS.GASCONDUIT;
		}
		else if (overlay == OverlayModes.SolidConveyor.ID)
		{
			text = ToolParameterMenu.FILTERLAYERS.SOLIDCONDUIT;
		}
		else if (overlay == OverlayModes.Logic.ID)
		{
			text = ToolParameterMenu.FILTERLAYERS.LOGIC;
		}
		if (text != null)
		{
			if (!isOverlayDriven)
			{
				SaveUserFilters();
			}
			isOverlayDriven = true;
			GetDefaultFilters(out currentFilters);
			for (int i = 0; i < currentFilters.Length; i++)
			{
				currentFilters[i].state = ((!(currentFilters[i].name == text)) ? ToolParameterMenu.ToggleState.Disabled : ToolParameterMenu.ToggleState.On);
			}
		}
		else if (isOverlayDriven)
		{
			RestoreUserFilters();
			isOverlayDriven = false;
		}
		ToolMenu.Instance.toolParameterMenu.PopulateMenu(currentFilters);
	}
}
