using System;
using System.Collections.Generic;
using UnityEngine;

public class ElectrobankDischarger : Generator
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, ElectrobankDischarger, object>.GameInstance
	{
		public StatesInstance(ElectrobankDischarger master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, ElectrobankDischarger>
	{
		public State noBattery;

		public State inoperational;

		public State discharging;

		public State discharging_pst;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = noBattery;
			root.EventTransition(GameHashes.ActiveChanged, discharging, (StatesInstance smi) => smi.GetComponent<Operational>().IsActive);
			noBattery.PlayAnim("off").EnterTransition(inoperational, (StatesInstance smi) => smi.master.storage.items.Count != 0).Enter(delegate(StatesInstance smi)
			{
				smi.master.UpdateMeter();
			});
			inoperational.PlayAnim("on").Enter(delegate(StatesInstance smi)
			{
				smi.master.UpdateMeter();
			}).EnterTransition(noBattery, (StatesInstance smi) => smi.master.storage.items.Count == 0);
			discharging.Enter(delegate(StatesInstance smi)
			{
				smi.master.UpdateMeter();
			}).EventTransition(GameHashes.ActiveChanged, inoperational, (StatesInstance smi) => !smi.GetComponent<Operational>().IsActive).QueueAnim("working_pre")
				.QueueAnim("working_loop", loop: true)
				.Update(delegate(StatesInstance smi, float dt)
				{
					smi.master.GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Power, Db.Get().BuildingStatusItems.ElectrobankJoulesAvailable, smi.master);
					smi.master.UpdateMeter();
				});
			discharging_pst.Enter(delegate(StatesInstance smi)
			{
				smi.master.UpdateMeter();
			}).PlayAnim("working_pst");
		}
	}

	public float wattageRating;

	[MyCmpReq]
	private Storage storage;

	private StatesInstance smi;

	private List<Electrobank> storedCells = new List<Electrobank>();

	private MeterController meterController;

	protected FilteredStorage filteredStorage;

	public float ElectrobankJoulesStored
	{
		get
		{
			float num = 0f;
			foreach (Electrobank storedCell in storedCells)
			{
				num += storedCell.Charge;
			}
			return num;
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		smi = new StatesInstance(this);
		smi.StartSM();
		Subscribe(-1697596308, OnStorageChange);
		RefreshCells();
		filteredStorage = new FilteredStorage(this, null, null, use_logic_meter: false, Db.Get().ChoreTypes.PowerFetch);
		filteredStorage.SetHasMeter(has_meter: false);
		filteredStorage.FilterChanged();
		Storage obj = storage;
		obj.onDestroyItemsDropped = (Action<List<GameObject>>)Delegate.Combine(obj.onDestroyItemsDropped, new Action<List<GameObject>>(OnBatteriesDroppedFromDeconstruction));
		UpdateSymbolSwap();
	}

	private void OnBatteriesDroppedFromDeconstruction(List<GameObject> items)
	{
		if (items == null)
		{
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
			GameObject gameObject = items[i];
			Electrobank component = gameObject.GetComponent<Electrobank>();
			if (component != null && component.HasTag(GameTags.ChargedPortableBattery) && !component.IsFullyCharged)
			{
				component.RemovePower(component.Charge, dropWhenEmpty: true);
			}
		}
	}

	protected override void OnCleanUp()
	{
		filteredStorage.CleanUp();
		base.OnCleanUp();
	}

	private void OnStorageChange(object data = null)
	{
		RefreshCells();
		UpdateSymbolSwap();
	}

	public void UpdateMeter()
	{
		if (meterController == null)
		{
			meterController = new MeterController(GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer);
		}
		meterController.SetPositionPercent(smi.master.ElectrobankJoulesStored / 120000f);
	}

	public void UpdateSymbolSwap()
	{
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		SymbolOverrideController component2 = component.GetComponent<SymbolOverrideController>();
		component.SetSymbolVisiblity("electrobank_l", is_visible: false);
		if (storage.items.Count > 0)
		{
			KAnim.Build.Symbol source_symbol = storage.items[0].GetComponent<KBatchedAnimController>().AnimFiles[0].GetData().build.symbols[0];
			component2.AddSymbolOverride("electrobank_s", source_symbol);
		}
		else
		{
			component2.RemoveSymbolOverride("electrobank_s");
		}
	}

	public override void EnergySim200ms(float dt)
	{
		base.EnergySim200ms(dt);
		bool value = false;
		ushort circuitID = base.CircuitID;
		operational.SetFlag(Generator.wireConnectedFlag, circuitID != ushort.MaxValue);
		if (!operational.IsOperational)
		{
			if (operational.IsActive)
			{
				operational.SetActive(value: false);
			}
			return;
		}
		float num = 0f;
		float num2 = Mathf.Min(wattageRating * dt, Capacity - JoulesAvailable);
		for (int num3 = storedCells.Count - 1; num3 >= 0; num3--)
		{
			num += storedCells[num3].RemovePower(num2 - num, dropWhenEmpty: true);
			if (num >= num2)
			{
				break;
			}
		}
		if (num > 0f)
		{
			value = true;
			GenerateJoules(num);
		}
		operational.SetActive(value);
	}

	private void RefreshCells(object data = null)
	{
		storedCells.Clear();
		foreach (GameObject item in storage.GetItems())
		{
			Electrobank component = item.GetComponent<Electrobank>();
			if (component != null)
			{
				storedCells.Add(component);
			}
		}
	}
}
