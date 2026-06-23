using System.Collections.Generic;
using System.Linq;
using PeterHan.PLib.Core;
using UnityEngine;

namespace UtilLibs.BuildingPortUtils;

[SkipSaveFileSerialization]
public class PortDisplayController : KMonoBehaviour
{
	public struct VanillaPortInfo
	{
		public string portDesc;

		public Sprite sprite;

		public Color color;

		public VanillaPortInfo(string portDesc, Sprite sprite, Color color)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			this.portDesc = portDesc;
			this.sprite = sprite;
			this.color = color;
		}
	}

	public struct ActivePortInfo
	{
		public string portDesc;

		public PortDisplay2 port;

		public int cell;

		public ActivePortInfo(int cell, string portDesc, PortDisplay2 port)
		{
			this.portDesc = portDesc;
			this.port = port;
			this.cell = cell;
		}
	}

	[SerializeField]
	private HashedString lastMode = None.ID;

	[SerializeField]
	private List<PortDisplay2> gasOverlay = new List<PortDisplay2>();

	[SerializeField]
	private List<PortDisplay2> liquidOverlay = new List<PortDisplay2>();

	[SerializeField]
	private List<PortDisplay2> solidOverlay = new List<PortDisplay2>();

	private static Dictionary<PortDisplay2, int> activePortCells = new Dictionary<PortDisplay2, int>();

	private static Dictionary<int, ActivePortInfo> activePortInfo = new Dictionary<int, ActivePortInfo>();

	private static Dictionary<int, VanillaPortInfo> vanillaPortInfo = new Dictionary<int, VanillaPortInfo>();

	private bool isCompletedBuilding = false;

	private static int lastCell = -1;

	private static HashSet<Ports> ValidPortTypes = new HashSet<Ports>
	{
		(Ports)4,
		(Ports)8,
		(Ports)16,
		(Ports)32,
		(Ports)64,
		(Ports)128
	};

	public static bool? VanillaPortsHandled = null;

	private static readonly string PLib_Registry_VanillaPorts = "UtilLibs_BuildingPortUtils_VanillaPortsHandled";

	public static bool TryGetActivePortDesc(int utilityCell, out string portDesc, out Sprite sprite, out Color color)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		portDesc = null;
		sprite = null;
		color = Color.white;
		if (activePortInfo.TryGetValue(utilityCell, out var value))
		{
			portDesc = value.portDesc;
			sprite = value.port.sprite;
			color = Color32.op_Implicit(value.port.color);
			return true;
		}
		if (vanillaPortInfo.TryGetValue(utilityCell, out var value2))
		{
			portDesc = value2.portDesc;
			sprite = value2.sprite;
			color = value2.color;
			return true;
		}
		return false;
	}

	public void AssignPort(GameObject go, DisplayConduitPortInfo port)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected I4, but got Unknown
		PortDisplay2 portDisplay = go.AddComponent<PortDisplay2>();
		portDisplay.AssignPort(port);
		ConduitType type = port.type;
		ConduitType val = type;
		switch (val - 1)
		{
		case 0:
			gasOverlay.Add(portDisplay);
			break;
		case 1:
			liquidOverlay.Add(portDisplay);
			break;
		case 2:
			solidOverlay.Add(portDisplay);
			break;
		}
	}

	public List<PortDisplay2> GetAllPorts()
	{
		return gasOverlay.Concat(liquidOverlay).Concat(solidOverlay).ToList();
	}

	public void Init(GameObject go)
	{
		string name = ((Tag)(ref go.GetComponent<KPrefabID>().PrefabTag)).Name;
		EntityTemplateExtensions.AddOrGet<BuildingCellVisualizer>(go);
		ConduitDisplayPortPatching.AddBuilding(name);
	}

	public override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		BuildingComplete val = default(BuildingComplete);
		isCompletedBuilding = ((Component)this).gameObject.TryGetComponent<BuildingComplete>(ref val);
	}

	public void Draw(BuildingCellVisualizer __instance, HashedString mode, GameObject go)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = mode != lastMode;
		if (flag)
		{
			ClearPorts();
			lastMode = mode;
		}
		foreach (PortDisplay2 port in GetPorts(mode))
		{
			int utilityCell = port.GetUtilityCell(__instance.building);
			if (isCompletedBuilding)
			{
				activePortInfo[utilityCell] = new ActivePortInfo(utilityCell, GetPortFilterDesc(port), port);
				activePortCells[port] = utilityCell;
			}
			port.Draw(go, __instance, flag);
		}
	}

	private string GetPortFilterDesc(PortDisplay2 port)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)port == (Object)null)
		{
			return null;
		}
		if (port.input)
		{
			PortConduitConsumer[] components = ((Component)this).GetComponents<PortConduitConsumer>();
			foreach (PortConduitConsumer portConduitConsumer in components)
			{
				if (portConduitConsumer.conduitType == port.type && portConduitConsumer.conduitOffset == port.offset && portConduitConsumer.conduitOffsetFlipped == port.offsetFlipped)
				{
					return SharedConduitUtils.GetFilteredPortTooltip(port.type, port.input, (Tag[])(object)new Tag[1] { portConduitConsumer.capacityTag });
				}
			}
		}
		else
		{
			PortConduitDispenserBase[] components2 = ((Component)this).GetComponents<PortConduitDispenserBase>();
			foreach (PortConduitDispenserBase portConduitDispenserBase in components2)
			{
				if (portConduitDispenserBase.conduitType == port.type && portConduitDispenserBase.conduitOffset == port.offset && portConduitDispenserBase.conduitOffsetFlipped == port.offsetFlipped)
				{
					return SharedConduitUtils.GetFilteredPortTooltip(port.type, port.input, portConduitDispenserBase.tagFilter, portConduitDispenserBase.elementFilter, portConduitDispenserBase.invertElementFilter);
				}
			}
		}
		return null;
	}

	private void ClearPorts()
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		foreach (PortDisplay2 port in GetPorts(lastMode))
		{
			port.DisableIcons();
			if (isCompletedBuilding)
			{
				activePortInfo.Remove(activePortCells[port]);
				activePortCells.Remove(port);
			}
		}
	}

	private List<PortDisplay2> GetPorts(HashedString mode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (mode == GasConduits.ID)
		{
			return gasOverlay;
		}
		if (mode == LiquidConduits.ID)
		{
			return liquidOverlay;
		}
		if (mode == SolidConveyor.ID)
		{
			return solidOverlay;
		}
		return new List<PortDisplay2>();
	}

	internal static void HandleVanillaPortInfo(BuildingCellVisualizer instance, HashedString mode)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Invalid comparison between Unknown and I4
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Invalid comparison between Unknown and I4
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Invalid comparison between Unknown and I4
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Invalid comparison between Unknown and I4
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Invalid comparison between Unknown and I4
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Invalid comparison between Unknown and I4
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Invalid comparison between Unknown and I4
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Invalid comparison between Unknown and I4
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Invalid comparison between Unknown and I4
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		if (!VanillaPortsHandled.HasValue)
		{
			if (!PRegistry.GetData<bool>(PLib_Registry_VanillaPorts))
			{
				VanillaPortsHandled = false;
				PRegistry.PutData(PLib_Registry_VanillaPorts, true);
			}
			else
			{
				VanillaPortsHandled = true;
			}
		}
		if (VanillaPortsHandled.Value)
		{
			return;
		}
		foreach (PortEntry port in ((EntityCellVisualizer)instance).ports)
		{
			if (!ValidPortTypes.Contains(port.type))
			{
				continue;
			}
			int num = ((EntityCellVisualizer)instance).ComputeCell(port.cellOffset);
			if ((Object)(object)port.visualizer == (Object)null || ((Object)(object)port.visualizer != (Object)null && !port.visualizer.activeInHierarchy))
			{
				vanillaPortInfo.Remove(num);
			}
			else
			{
				if (!((Object)(object)port.visualizer != (Object)null))
				{
					continue;
				}
				Sprite sprite = Assets.GetSprite(HashedString.op_Implicit("unknown"));
				Color connectedTint = port.connectedTint;
				bool flag = false;
				ConduitType val = (ConduitType)0;
				Ports type = port.type;
				Ports val2 = type;
				if ((int)val2 <= 16)
				{
					if ((int)val2 != 4)
					{
						if ((int)val2 != 8)
						{
							if ((int)val2 == 16)
							{
								val = (ConduitType)2;
								flag = true;
							}
						}
						else
						{
							val = (ConduitType)1;
						}
					}
					else
					{
						val = (ConduitType)1;
						flag = true;
					}
				}
				else if ((int)val2 != 32)
				{
					if ((int)val2 != 64)
					{
						if ((int)val2 == 128)
						{
							val = (ConduitType)3;
						}
					}
					else
					{
						flag = true;
						val = (ConduitType)3;
					}
				}
				else
				{
					val = (ConduitType)2;
				}
				sprite = SharedConduitUtils.GetSprite(flag, val);
				if (flag)
				{
					if ((int)val != 3)
					{
						ConduitConsumer[] components = ((Component)instance).GetComponents<ConduitConsumer>();
						foreach (ConduitConsumer val3 in components)
						{
							if (val3.conduitType == val && val3.GetInputCell(val3.GetConduitManager().conduitType) == num)
							{
								vanillaPortInfo[num] = new VanillaPortInfo(SharedConduitUtils.GetFilteredPortTooltip(val, flag, (Tag[])(object)new Tag[1] { val3.capacityTag }), sprite, connectedTint);
								break;
							}
						}
						continue;
					}
					SolidConduitConsumer[] components2 = ((Component)instance).GetComponents<SolidConduitConsumer>();
					foreach (SolidConduitConsumer val4 in components2)
					{
						if (val4.GetInputCell() == num)
						{
							vanillaPortInfo[num] = new VanillaPortInfo(SharedConduitUtils.GetFilteredPortTooltip(val, flag, (Tag[])(object)new Tag[1] { val4.capacityTag }), sprite, connectedTint);
							break;
						}
					}
					continue;
				}
				if ((int)val != 3)
				{
					ConduitDispenser[] components3 = ((Component)instance).GetComponents<ConduitDispenser>();
					foreach (ConduitDispenser val5 in components3)
					{
						if (val5.conduitType == val && val5.GetOutputCell(val5.conduitType) == num)
						{
							vanillaPortInfo[num] = new VanillaPortInfo(SharedConduitUtils.GetFilteredPortTooltip(val, flag, null, val5.elementFilter, val5.invertElementFilter), sprite, connectedTint);
							break;
						}
					}
					continue;
				}
				SolidConduitDispenser[] components4 = ((Component)instance).GetComponents<SolidConduitDispenser>();
				foreach (SolidConduitDispenser val6 in components4)
				{
					if (val6.GetOutputCell() == num)
					{
						vanillaPortInfo[num] = new VanillaPortInfo(SharedConduitUtils.GetFilteredPortTooltip(val, flag, null, val6.elementFilter, val6.invertElementFilter), sprite, connectedTint);
						break;
					}
				}
			}
		}
	}
}
