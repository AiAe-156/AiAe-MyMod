using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[SkipSaveFileSerialization]
[AddComponentMenu("KMonoBehaviour/scripts/SimTemperatureTransfer")]
public class SimTemperatureTransfer : KMonoBehaviour
{
	[MyCmpReq]
	public PrimaryElement pe;

	private const float SIM_FREEZE_SPAWN_ORE_PERCENT = 0.8f;

	public float deltaKJ;

	public Action<SimTemperatureTransfer> onSimRegistered;

	protected int simHandle = -1;

	protected bool forceDataSyncOnRegister;

	[SerializeField]
	protected float surfaceArea = 10f;

	[SerializeField]
	protected float thickness = 0.01f;

	[SerializeField]
	protected float groundTransferScale = 0.0625f;

	private static Dictionary<int, SimTemperatureTransfer> handleInstanceMap = new Dictionary<int, SimTemperatureTransfer>();

	private ulong cellChangedHandlerID;

	private static readonly Action<object> OnCellChangedDispatcher = delegate(object obj)
	{
		Unsafe.As<SimTemperatureTransfer>(obj).OnCellChanged();
	};

	public float SurfaceArea
	{
		get
		{
			return surfaceArea;
		}
		set
		{
			surfaceArea = value;
		}
	}

	public float Thickness
	{
		get
		{
			return thickness;
		}
		set
		{
			thickness = value;
		}
	}

	public float GroundTransferScale
	{
		get
		{
			return groundTransferScale;
		}
		set
		{
			groundTransferScale = value;
		}
	}

	public int SimHandle => simHandle;

	public static void ClearInstanceMap()
	{
		handleInstanceMap.Clear();
	}

	public static void DoOreMeltTransition(int sim_handle)
	{
		SimTemperatureTransfer value = null;
		if (!handleInstanceMap.TryGetValue(sim_handle, out value) || value == null || value.HasTag(GameTags.Sealed))
		{
			return;
		}
		PrimaryElement primaryElement = value.pe;
		Element element = primaryElement.Element;
		bool flag = primaryElement.Temperature >= element.highTemp;
		bool flag2 = primaryElement.Temperature <= element.lowTemp;
		if (!(flag || flag2) || (flag && element.highTempTransitionTarget == SimHashes.Unobtanium) || (flag2 && element.lowTempTransitionTarget == SimHashes.Unobtanium))
		{
			return;
		}
		if (primaryElement.Mass > 0f)
		{
			int gameCell = Grid.PosToCell(value.transform.GetPosition());
			float num = primaryElement.Mass;
			int num2 = primaryElement.DiseaseCount;
			SimHashes new_element = (flag ? element.highTempTransitionTarget : element.lowTempTransitionTarget);
			SimHashes simHashes = (flag ? element.highTempTransitionOreID : element.lowTempTransitionOreID);
			float num3 = (flag ? element.highTempTransitionOreMassConversion : element.lowTempTransitionOreMassConversion);
			if (simHashes != 0)
			{
				float num4 = num * num3;
				int num5 = (int)((float)num2 * num3);
				if (num4 > 0.001f)
				{
					num -= num4;
					num2 -= num5;
					Element element2 = ElementLoader.FindElementByHash(simHashes);
					if (element2.IsSolid)
					{
						GameObject gameObject = element2.substance.SpawnResource(value.transform.GetPosition(), num4, primaryElement.Temperature, primaryElement.DiseaseIdx, num5, prevent_merge: true, forceTemperature: false, manual_activation: true);
						element2.substance.ActivateSubstanceGameObject(gameObject, primaryElement.DiseaseIdx, num5);
					}
					else
					{
						SimMessages.AddRemoveSubstance(gameCell, element2.id, CellEventLogger.Instance.OreMelted, num4, primaryElement.Temperature, primaryElement.DiseaseIdx, num5);
					}
				}
			}
			SimMessages.AddRemoveSubstance(gameCell, new_element, CellEventLogger.Instance.OreMelted, num, primaryElement.Temperature, primaryElement.DiseaseIdx, num2);
		}
		value.OnCleanUp();
		Util.KDestroyGameObject(value.gameObject);
	}

	protected override void OnPrefabInit()
	{
		pe.sttOptimizationHook = this;
		pe.getTemperatureCallback = OnGetTemperature;
		pe.setTemperatureCallback = OnSetTemperature;
		PrimaryElement primaryElement = pe;
		primaryElement.onDataChanged = (Action<PrimaryElement>)Delegate.Combine(primaryElement.onDataChanged, new Action<PrimaryElement>(OnDataChanged));
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Element element = pe.Element;
		cellChangedHandlerID = Singleton<CellChangeMonitor>.Instance.RegisterCellChangedHandler(base.transform, OnCellChangedDispatcher, this, "SimTemperatureTransfer.OnSpawn");
		if (!Grid.IsValidCell(Grid.PosToCell(this)) || pe.Element.HasTag(GameTags.Special) || element.specificHeatCapacity == 0f)
		{
			base.enabled = false;
		}
		SimRegister();
	}

	protected override void OnCmpEnable()
	{
		base.OnCmpEnable();
		SimRegister();
		if (Sim.IsValidHandle(simHandle))
		{
			OnSetTemperature(pe, pe.Temperature);
		}
	}

	protected override void OnCmpDisable()
	{
		if (Sim.IsValidHandle(simHandle))
		{
			float temperature = pe.Temperature;
			pe.InternalTemperature = pe.Temperature;
			SimMessages.SetElementChunkData(simHandle, temperature, 0f);
		}
		base.OnCmpDisable();
	}

	private void OnCellChanged()
	{
		int cell = Grid.PosToCell(this);
		if (!Grid.IsValidCell(cell))
		{
			base.enabled = false;
			return;
		}
		SimRegister();
		if (Sim.IsValidHandle(simHandle))
		{
			SimMessages.MoveElementChunk(simHandle, cell);
		}
		else
		{
			forceDataSyncOnRegister = true;
		}
	}

	protected override void OnCleanUp()
	{
		Singleton<CellChangeMonitor>.Instance.UnregisterCellChangedHandler(ref cellChangedHandlerID);
		SimUnregister();
		base.OnForcedCleanUp();
	}

	private unsafe static float OnGetTemperature(PrimaryElement primary_element)
	{
		SimTemperatureTransfer sttOptimizationHook = primary_element.sttOptimizationHook;
		float result;
		if (Sim.IsValidHandle(sttOptimizationHook.simHandle))
		{
			int handleIndex = Sim.GetHandleIndex(sttOptimizationHook.simHandle);
			result = Game.Instance.simData.elementChunks[handleIndex].temperature;
			sttOptimizationHook.deltaKJ = Game.Instance.simData.elementChunks[handleIndex].deltaKJ;
		}
		else
		{
			result = primary_element.InternalTemperature;
		}
		return result;
	}

	private unsafe static void OnSetTemperature(PrimaryElement primary_element, float temperature)
	{
		if (temperature <= 0f)
		{
			KCrashReporter.Assert(condition: false, "STT.OnSetTemperature - Tried to set <= 0 degree temperature");
			temperature = 293f;
		}
		primary_element.InternalTemperature = temperature;
		SimTemperatureTransfer sttOptimizationHook = primary_element.sttOptimizationHook;
		if (Sim.IsValidHandle(sttOptimizationHook.simHandle))
		{
			float heat_capacity = primary_element.Mass * primary_element.Element.specificHeatCapacity;
			SimMessages.SetElementChunkData(sttOptimizationHook.simHandle, temperature, heat_capacity);
			int handleIndex = Sim.GetHandleIndex(sttOptimizationHook.simHandle);
			Game.Instance.simData.elementChunks[handleIndex].temperature = temperature;
		}
	}

	private void OnDataChanged(PrimaryElement primary_element)
	{
		if (Sim.IsValidHandle(simHandle))
		{
			float heat_capacity = primary_element.Mass * primary_element.Element.specificHeatCapacity;
			SimMessages.SetElementChunkData(simHandle, primary_element.Temperature, heat_capacity);
		}
		else
		{
			forceDataSyncOnRegister = true;
		}
	}

	protected void SimRegister()
	{
		if (base.isSpawned && simHandle == -1 && base.enabled && pe.Mass > 0f && !pe.Element.IsTemperatureInsulated)
		{
			int gameCell = Grid.PosToCell(base.transform.GetPosition());
			simHandle = -2;
			HandleVector<Game.ComplexCallbackInfo<int>>.Handle handle = Game.Instance.simComponentCallbackManager.Add(OnSimRegisteredCallback, this, "SimTemperatureTransfer.SimRegister");
			float num = pe.InternalTemperature;
			if (num <= 0f)
			{
				pe.InternalTemperature = 293f;
				num = 293f;
			}
			forceDataSyncOnRegister = false;
			SimMessages.AddElementChunk(gameCell, pe.ElementID, pe.Mass, num, surfaceArea, thickness, groundTransferScale, handle.index);
		}
	}

	protected unsafe void SimUnregister()
	{
		if (simHandle != -1 && !KMonoBehaviour.isLoadingScene)
		{
			if (Sim.IsValidHandle(simHandle))
			{
				int handleIndex = Sim.GetHandleIndex(simHandle);
				pe.InternalTemperature = Game.Instance.simData.elementChunks[handleIndex].temperature;
				SimMessages.RemoveElementChunk(simHandle, -1);
				handleInstanceMap.Remove(simHandle);
			}
			simHandle = -1;
		}
	}

	private static void OnSimRegisteredCallback(int handle, object data)
	{
		((SimTemperatureTransfer)data).OnSimRegistered(handle);
	}

	private unsafe void OnSimRegistered(int handle)
	{
		if (this != null && simHandle == -2)
		{
			simHandle = handle;
			int handleIndex = Sim.GetHandleIndex(handle);
			float temperature = Game.Instance.simData.elementChunks[handleIndex].temperature;
			float internalTemperature = pe.InternalTemperature;
			if (temperature <= 0f)
			{
				KCrashReporter.Assert(condition: false, "Bad temperature");
			}
			handleInstanceMap[simHandle] = this;
			if (forceDataSyncOnRegister || Mathf.Abs(temperature - internalTemperature) > 0.1f)
			{
				float heat_capacity = pe.Mass * pe.Element.specificHeatCapacity;
				SimMessages.SetElementChunkData(simHandle, internalTemperature, heat_capacity);
				SimMessages.MoveElementChunk(simHandle, Grid.PosToCell(this));
				Game.Instance.simData.elementChunks[handleIndex].temperature = internalTemperature;
			}
			if (onSimRegistered != null)
			{
				onSimRegistered(this);
			}
			if (!base.enabled)
			{
				OnCmpDisable();
			}
		}
		else
		{
			SimMessages.RemoveElementChunk(handle, -1);
		}
	}
}
