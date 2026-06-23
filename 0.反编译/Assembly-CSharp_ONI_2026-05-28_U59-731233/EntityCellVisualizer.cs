using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using TUNING;
using UnityEngine;
using UnityEngine.UI;

public class EntityCellVisualizer : KMonoBehaviour
{
	[Flags]
	public enum Ports
	{
		PowerIn = 1,
		PowerOut = 2,
		GasIn = 4,
		GasOut = 8,
		LiquidIn = 0x10,
		LiquidOut = 0x20,
		SolidIn = 0x40,
		SolidOut = 0x80,
		HighEnergyParticleIn = 0x100,
		HighEnergyParticleOut = 0x200,
		DiseaseIn = 0x400,
		DiseaseOut = 0x800,
		HeatSource = 0x1000,
		HeatSink = 0x2000
	}

	protected class PortEntry
	{
		public Ports type;

		public CellOffset cellOffset;

		public GameObject visualizer;

		public Color connectedTint;

		public Color disconnectedTint;

		public float scale;

		public bool hideBG;

		public PortEntry(Ports type, CellOffset cellOffset, Color connectedTint, Color disconnectedTint, float scale, bool hideBG)
		{
			this.type = type;
			this.cellOffset = cellOffset;
			visualizer = null;
			this.connectedTint = connectedTint;
			this.disconnectedTint = disconnectedTint;
			this.scale = scale;
			this.hideBG = hideBG;
		}
	}

	protected List<PortEntry> ports = new List<PortEntry>();

	public Ports addedPorts = (Ports)0;

	private GameObject switchVisualizer = null;

	private GameObject wireVisualizerAlpha = null;

	private GameObject wireVisualizerBeta = null;

	public const Ports HEAT_PORTS = Ports.HeatSource | Ports.HeatSink;

	public const Ports POWER_PORTS = Ports.PowerIn | Ports.PowerOut;

	public const Ports GAS_PORTS = Ports.GasIn | Ports.GasOut;

	public const Ports LIQUID_PORTS = Ports.LiquidIn | Ports.LiquidOut;

	public const Ports SOLID_PORTS = Ports.SolidIn | Ports.SolidOut;

	public const Ports ENERGY_PARTICLES_PORTS = Ports.HighEnergyParticleIn | Ports.HighEnergyParticleOut;

	public const Ports DISEASE_PORTS = Ports.DiseaseIn | Ports.DiseaseOut;

	public const Ports MATTER_PORTS = Ports.GasIn | Ports.GasOut | Ports.LiquidIn | Ports.LiquidOut | Ports.SolidIn | Ports.SolidOut;

	protected Sprite diseaseSourceSprite;

	protected Color32 diseaseSourceColour;

	[MyCmpGet]
	private Rotatable rotatable;

	protected bool enableRaycast = true;

	protected Dictionary<GameObject, Image> icons;

	public string DiseaseCellVisName = DUPLICANTSTATS.STANDARD.Secretions.PEE_DISEASE;

	public BuildingCellVisualizerResources Resources => BuildingCellVisualizerResources.Instance();

	protected int CenterCell => Grid.PosToCell(this);

	protected virtual void DefinePorts()
	{
	}

	protected override void OnPrefabInit()
	{
		LoadDiseaseIcon();
		DefinePorts();
	}

	public void ConnectedEventWithDelay(float delay, int connectionCount, int cell, string soundName)
	{
		StartCoroutine(ConnectedDelay(delay, connectionCount, cell, soundName));
	}

	private IEnumerator ConnectedDelay(float delay, int connectionCount, int cell, string soundName)
	{
		float startTime = Time.realtimeSinceStartup;
		float currentTime = startTime;
		while (currentTime < startTime + delay)
		{
			currentTime += Time.unscaledDeltaTime;
			yield return SequenceUtil.WaitForEndOfFrame;
		}
		ConnectedEvent(cell);
		string connectedReleaseSound = GlobalAssets.GetSound(soundName);
		if (connectedReleaseSound != null)
		{
			Vector3 sound_pos = base.transform.GetPosition();
			sound_pos.z = 0f;
			EventInstance ev = SoundEvent.BeginOneShot(connectedReleaseSound, sound_pos);
			ev.setParameterByName("connectedCount", connectionCount);
			SoundEvent.EndOneShot(ev);
		}
	}

	private int ComputeCell(CellOffset cellOffset)
	{
		CellOffset offset = cellOffset;
		if (rotatable != null)
		{
			offset = rotatable.GetRotatedCellOffset(cellOffset);
		}
		return Grid.OffsetCell(Grid.PosToCell(base.gameObject), offset);
	}

	public void ConnectedEvent(int cell)
	{
		foreach (PortEntry port in ports)
		{
			if (ComputeCell(port.cellOffset) == cell && port.visualizer != null)
			{
				SizePulse pulse = port.visualizer.AddComponent<SizePulse>();
				pulse.speed = 20f;
				pulse.multiplier = 0.75f;
				pulse.updateWhenPaused = true;
				SizePulse sizePulse = pulse;
				sizePulse.onComplete = (System.Action)Delegate.Combine(sizePulse.onComplete, (System.Action)delegate
				{
					UnityEngine.Object.Destroy(pulse);
				});
			}
		}
	}

	public virtual void AddPort(Ports type, CellOffset cell)
	{
		AddPort(type, cell, Color.white);
	}

	public virtual void AddPort(Ports type, CellOffset cell, Color tint)
	{
		AddPort(type, cell, tint, tint);
	}

	public virtual void AddPort(Ports type, CellOffset cell, Color connectedTint, Color disconnectedTint, float scale = 1.5f, bool hideBG = false)
	{
		ports.Add(new PortEntry(type, cell, connectedTint, disconnectedTint, scale, hideBG));
		addedPorts |= type;
	}

	protected override void OnCleanUp()
	{
		foreach (PortEntry port in ports)
		{
			if (port.visualizer != null)
			{
				UnityEngine.Object.Destroy(port.visualizer);
			}
		}
		GameObject[] array = new GameObject[3] { switchVisualizer, wireVisualizerAlpha, wireVisualizerBeta };
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		base.OnCleanUp();
	}

	protected override void OnCmpEnable()
	{
		base.OnCmpEnable();
		if (icons == null)
		{
			icons = new Dictionary<GameObject, Image>();
		}
		Components.EntityCellVisualizers.Add(this);
	}

	protected override void OnCmpDisable()
	{
		base.OnCmpDisable();
		Components.EntityCellVisualizers.Remove(this);
	}

	public void DrawIcons(HashedString mode)
	{
		Ports ports = (Ports)0;
		if (base.gameObject.GetMyWorldId() != ClusterManager.Instance.activeWorldId)
		{
			ports = (Ports)0;
		}
		else if (mode == OverlayModes.Power.ID)
		{
			ports = Ports.PowerIn | Ports.PowerOut;
		}
		else if (mode == OverlayModes.GasConduits.ID)
		{
			ports = Ports.GasIn | Ports.GasOut;
		}
		else if (mode == OverlayModes.LiquidConduits.ID)
		{
			ports = Ports.LiquidIn | Ports.LiquidOut;
		}
		else if (mode == OverlayModes.SolidConveyor.ID)
		{
			ports = Ports.SolidIn | Ports.SolidOut;
		}
		else if (mode == OverlayModes.Radiation.ID)
		{
			ports = Ports.HighEnergyParticleIn | Ports.HighEnergyParticleOut;
		}
		else if (mode == OverlayModes.Disease.ID)
		{
			ports = Ports.DiseaseIn | Ports.DiseaseOut;
		}
		else if (mode == OverlayModes.Temperature.ID || mode == OverlayModes.HeatFlow.ID)
		{
			ports = Ports.HeatSource | Ports.HeatSink;
		}
		bool flag = false;
		foreach (PortEntry port in this.ports)
		{
			if ((port.type & ports) == port.type)
			{
				DrawUtilityIcon(port);
				flag = true;
			}
			else if (port.visualizer != null && port.visualizer.activeInHierarchy)
			{
				port.visualizer.SetActive(value: false);
			}
		}
		if (mode == OverlayModes.Power.ID)
		{
			if (flag)
			{
				return;
			}
			Switch component = GetComponent<Switch>();
			if (component != null)
			{
				int cell = Grid.PosToCell(base.transform.GetPosition());
				Color32 color = (component.IsHandlerOn() ? Resources.switchColor : Resources.switchOffColor);
				DrawUtilityIcon(cell, Resources.switchIcon, ref switchVisualizer, color, 1f);
				return;
			}
			WireUtilityNetworkLink component2 = GetComponent<WireUtilityNetworkLink>();
			if (component2 != null)
			{
				component2.GetCells(out var linked_cell, out var linked_cell2);
				DrawUtilityIcon(linked_cell, (Game.Instance.circuitManager.GetCircuitID(linked_cell) == ushort.MaxValue) ? Resources.electricityBridgeIcon : Resources.electricityConnectedIcon, ref wireVisualizerAlpha, Resources.electricityInputColor, 1f);
				DrawUtilityIcon(linked_cell2, (Game.Instance.circuitManager.GetCircuitID(linked_cell2) == ushort.MaxValue) ? Resources.electricityBridgeIcon : Resources.electricityConnectedIcon, ref wireVisualizerBeta, Resources.electricityInputColor, 1f);
			}
			return;
		}
		GameObject[] array = new GameObject[3] { switchVisualizer, wireVisualizerAlpha, wireVisualizerBeta };
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject != null && gameObject.activeInHierarchy)
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	private Sprite GetSpriteForPortType(Ports type, bool connected)
	{
		return type switch
		{
			Ports.PowerIn => connected ? Resources.electricityBridgeConnectedIcon : Resources.electricityInputIcon, 
			Ports.PowerOut => connected ? Resources.electricityBridgeConnectedIcon : Resources.electricityOutputIcon, 
			Ports.GasIn => Resources.gasInputIcon, 
			Ports.GasOut => Resources.gasOutputIcon, 
			Ports.LiquidIn => Resources.liquidInputIcon, 
			Ports.LiquidOut => Resources.liquidOutputIcon, 
			Ports.SolidIn => Resources.liquidInputIcon, 
			Ports.SolidOut => Resources.liquidOutputIcon, 
			Ports.HighEnergyParticleIn => Resources.highEnergyParticleInputIcon, 
			Ports.HighEnergyParticleOut => GetIconForHighEnergyOutput(), 
			Ports.DiseaseIn => diseaseSourceSprite, 
			Ports.DiseaseOut => diseaseSourceSprite, 
			Ports.HeatSource => Resources.heatSourceIcon, 
			Ports.HeatSink => Resources.heatSinkIcon, 
			_ => null, 
		};
	}

	protected virtual void DrawUtilityIcon(PortEntry port)
	{
		int cell = ComputeCell(port.cellOffset);
		bool flag = true;
		bool connected = true;
		switch (port.type)
		{
		case Ports.PowerIn:
		case Ports.PowerOut:
		{
			Building component = GetComponent<Building>();
			bool flag2 = component as BuildingPreview != null;
			BuildingEnabledButton component2 = GetComponent<BuildingEnabledButton>();
			connected = !flag2 && Game.Instance.circuitManager.GetCircuitID(cell) != ushort.MaxValue;
			flag = flag2 || component2 == null || component2.IsEnabled;
			break;
		}
		case Ports.LiquidIn:
		case Ports.LiquidOut:
			flag = null != Grid.Objects[cell, 16];
			break;
		case Ports.GasIn:
		case Ports.GasOut:
			flag = null != Grid.Objects[cell, 12];
			break;
		case Ports.SolidIn:
		case Ports.SolidOut:
			flag = null != Grid.Objects[cell, 20];
			break;
		}
		DrawUtilityIcon(cell, GetSpriteForPortType(port.type, connected), ref port.visualizer, flag ? port.connectedTint : port.disconnectedTint, port.scale, port.hideBG);
	}

	protected virtual void LoadDiseaseIcon()
	{
		DiseaseVisualization.Info info = Assets.instance.DiseaseVisualization.GetInfo(DiseaseCellVisName);
		if (info.name != null)
		{
			diseaseSourceSprite = Assets.instance.DiseaseVisualization.overlaySprite;
			diseaseSourceColour = GlobalAssets.Instance.colorSet.GetColorByName(info.overlayColourName);
		}
	}

	protected virtual Sprite GetIconForHighEnergyOutput()
	{
		IHighEnergyParticleDirection component = GetComponent<IHighEnergyParticleDirection>();
		Sprite result = Resources.highEnergyParticleOutputIcons[0];
		if (component != null)
		{
			int directionIndex = EightDirectionUtil.GetDirectionIndex(component.Direction);
			result = Resources.highEnergyParticleOutputIcons[directionIndex];
		}
		return result;
	}

	private void DrawUtilityIcon(int cell, Sprite icon_img, ref GameObject visualizerObj, Color tint, float scaleMultiplier = 1.5f, bool hideBG = false)
	{
		Vector3 position = Grid.CellToPosCCC(cell, Grid.SceneLayer.Building);
		if (visualizerObj == null)
		{
			visualizerObj = Util.KInstantiate(Assets.UIPrefabs.ResourceVisualizer, GameScreenManager.Instance.worldSpaceCanvas);
			visualizerObj.transform.SetAsFirstSibling();
			icons.Add(visualizerObj, visualizerObj.transform.GetChild(0).GetComponent<Image>());
		}
		if (!visualizerObj.gameObject.activeInHierarchy)
		{
			visualizerObj.gameObject.SetActive(value: true);
		}
		Image component = visualizerObj.GetComponent<Image>();
		component.enabled = !hideBG;
		icons[visualizerObj].raycastTarget = enableRaycast;
		icons[visualizerObj].sprite = icon_img;
		Transform child = visualizerObj.transform.GetChild(0);
		component = child.gameObject.GetComponent<Image>();
		component.color = tint;
		visualizerObj.transform.SetPosition(position);
		if (visualizerObj.GetComponent<SizePulse>() == null)
		{
			visualizerObj.transform.localScale = Vector3.one * scaleMultiplier;
		}
	}

	public Image GetPowerOutputIcon()
	{
		foreach (PortEntry port in ports)
		{
			if (port.type == Ports.PowerOut)
			{
				return (port.visualizer != null) ? port.visualizer.transform.GetChild(0).GetComponent<Image>() : null;
			}
		}
		return null;
	}

	public Image GetPowerInputIcon()
	{
		foreach (PortEntry port in ports)
		{
			if (port.type == Ports.PowerIn)
			{
				return (port.visualizer != null) ? port.visualizer.transform.GetChild(0).GetComponent<Image>() : null;
			}
		}
		return null;
	}
}
