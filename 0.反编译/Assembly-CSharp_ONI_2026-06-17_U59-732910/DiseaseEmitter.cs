using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using KSerialization;
using Klei.AI;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/DiseaseEmitter")]
public class DiseaseEmitter : KMonoBehaviour
{
	private struct EmitterRegistration
	{
		public DiseaseEmitter emitter;

		public int emitterIndex;
	}

	[Serialize]
	public float emitRate = 1f;

	[Serialize]
	public byte emitRange;

	[Serialize]
	public int emitCount;

	[Serialize]
	public byte[] emitDiseases;

	public int[] simHandles;

	[Serialize]
	protected bool enableEmitter = true;

	private ulong cellChangedHandlerID;

	private static readonly Action<object> OnCellChangedDispatcher = delegate(object obj)
	{
		Unsafe.As<DiseaseEmitter>(obj).OnCellChanged();
	};

	public float EmitRate => emitRate;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (emitDiseases != null)
		{
			simHandles = new int[emitDiseases.Length];
			for (int i = 0; i < simHandles.Length; i++)
			{
				simHandles[i] = -1;
			}
		}
		SimRegister();
	}

	protected override void OnCleanUp()
	{
		SimUnregister();
		base.OnCleanUp();
	}

	public void SetEnable(bool enable)
	{
		if (enableEmitter != enable)
		{
			enableEmitter = enable;
			if (enableEmitter)
			{
				SimRegister();
			}
			else
			{
				SimUnregister();
			}
		}
	}

	private void SimModifyDiseaseEmitter(int emitterIndex, int cell)
	{
		SimMessages.ModifyDiseaseEmitter(simHandles[emitterIndex], cell, emitRange, emitDiseases[emitterIndex], emitRate, emitCount);
	}

	protected void OnCellChanged()
	{
		DebugUtil.DevAssert(simHandles != null, "DiseaseEmitter received cell change notification but has not been Spawned?!");
		if (simHandles == null || !enableEmitter)
		{
			return;
		}
		int cell = Grid.PosToCell(this);
		if (!Grid.IsValidCell(cell))
		{
			return;
		}
		for (int i = 0; i < emitDiseases.Length; i++)
		{
			if (Sim.IsValidHandle(simHandles[i]))
			{
				SimModifyDiseaseEmitter(i, cell);
			}
		}
	}

	private void SimRegister()
	{
		DebugUtil.DevAssert(simHandles != null, "DiseaseEmitter.SimRegister invoked but has not been Spawned?!");
		if (simHandles == null || !enableEmitter)
		{
			return;
		}
		if (cellChangedHandlerID != 0L)
		{
			cellChangedHandlerID = Singleton<CellChangeMonitor>.Instance.RegisterCellChangedHandler(base.transform, OnCellChangedDispatcher, this);
		}
		for (int i = 0; i < simHandles.Length; i++)
		{
			if (simHandles[i] == -1)
			{
				simHandles[i] = -2;
				SimMessages.AddDiseaseEmitter(Game.Instance.simComponentCallbackManager.Add(OnSimRegisteredCallback, new EmitterRegistration
				{
					emitter = this,
					emitterIndex = i
				}, "DiseaseEmitter").index);
			}
		}
	}

	private void SimUnregister()
	{
		DebugUtil.DevAssert(simHandles != null, "DiseaseEmitter.SimUnregister invoked but has not been Spawned?!");
		if (simHandles == null)
		{
			return;
		}
		for (int i = 0; i < simHandles.Length; i++)
		{
			if (Sim.IsValidHandle(simHandles[i]))
			{
				SimMessages.RemoveDiseaseEmitter(-1, simHandles[i]);
			}
			simHandles[i] = -1;
		}
		Singleton<CellChangeMonitor>.Instance.UnregisterCellChangedHandler(ref cellChangedHandlerID);
	}

	private static void OnSimRegisteredCallback(int handle, object data)
	{
		EmitterRegistration emitterRegistration = (EmitterRegistration)data;
		emitterRegistration.emitter.OnSimRegistered(handle, emitterRegistration.emitterIndex);
	}

	private void OnSimRegistered(int handle, int emitterIndex)
	{
		if (this.IsNullOrDestroyed())
		{
			SimMessages.RemoveDiseaseEmitter(-1, handle);
			return;
		}
		simHandles[emitterIndex] = handle;
		int cell = Grid.PosToCell(this);
		DebugUtil.DevAssert(Grid.IsValidCell(cell), "Failed to initialize DiseaseEmitter because it is on an invalid cell");
		if (Grid.IsValidCell(cell))
		{
			SimModifyDiseaseEmitter(emitterIndex, cell);
		}
	}

	public void SetDiseases(List<Disease> diseases)
	{
		emitDiseases = new byte[diseases.Count];
		for (int i = 0; i < diseases.Count; i++)
		{
			emitDiseases[i] = Db.Get().Diseases.GetIndex(diseases[i].id);
		}
	}
}
