using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class FlushToilet : StateMachineComponent<FlushToilet.SMInstance>, IUsable, IGameObjectEffectDescriptor, IBasicBuilding
{
	public class SMInstance : GameStateMachine<States, SMInstance, FlushToilet, object>.GameInstance
	{
		public List<Chore> activeUseChores;

		private Chore cleanChore = null;

		public bool IsClogged => base.sm.isClogged.Get(this);

		public SMInstance(FlushToilet master)
			: base(master)
		{
			activeUseChores = new List<Chore>();
			UpdateFullnessState();
			UpdateDirtyState();
		}

		public void CreateCleanChore()
		{
			if (cleanChore != null)
			{
				cleanChore.Cancel("dupe");
			}
			ToiletWorkableClean component = GetComponent<ToiletWorkableClean>();
			component.SetIsCloggedByGunk(IsClogged);
			cleanChore = new WorkChore<ToiletWorkableClean>(Db.Get().ChoreTypes.CleanToilet, component, null, run_until_complete: true, OnCleanComplete, null, null, allow_in_red_alert: true, null, ignore_schedule_block: false, only_when_operational: true, null, is_preemptable: false, allow_in_context_menu: true, allow_prioritization: true, PriorityScreen.PriorityClass.basic, 5, ignore_building_assignment: true);
		}

		public void CancelCleanChore()
		{
			if (cleanChore != null)
			{
				cleanChore.Cancel("Cancelled");
				cleanChore = null;
			}
		}

		private void OnCleanComplete(object o)
		{
			base.sm.isClogged.Set(value: false, this);
		}

		public bool HasValidConnections()
		{
			return Game.Instance.liquidConduitFlow.HasConduit(base.master.inputCell) && (!base.master.requireOutput || Game.Instance.liquidConduitFlow.HasConduit(base.master.outputCell));
		}

		public bool UpdateFullnessState()
		{
			float num = 0f;
			ListPool<GameObject, FlushToilet>.PooledList pooledList = ListPool<GameObject, FlushToilet>.Allocate();
			base.master.storage.Find(WaterTag, pooledList);
			foreach (GameObject item in pooledList)
			{
				PrimaryElement component = item.GetComponent<PrimaryElement>();
				num += component.Mass;
			}
			pooledList.Recycle();
			bool flag = num >= base.master.massConsumedPerUse;
			base.master.conduitConsumer.enabled = !flag;
			float positionPercent = Mathf.Clamp01(num / base.master.massConsumedPerUse);
			base.master.fillMeter.SetPositionPercent(positionPercent);
			return flag;
		}

		public void SetDirtyStatesForClogged()
		{
			ToiletWorkableUse component = GetComponent<ToiletWorkableUse>();
			bool flag = component.last_user_id == BionicMinionConfig.ID;
			SetDirtyStateMeterPercentage((!flag) ? 1 : 0, flag ? 1 : 0);
		}

		public void SetDirtyStateMeterPercentage(float contaminationPercentage, float gunkPercentage)
		{
			base.master.contaminationMeter.SetPositionPercent(contaminationPercentage);
			base.master.gunkMeter.SetPositionPercent(gunkPercentage);
		}

		public void UpdateDirtyState()
		{
			ToiletWorkableUse component = GetComponent<ToiletWorkableUse>();
			float percentComplete = component.GetPercentComplete();
			bool flag = component.last_user_id == BionicMinionConfig.ID;
			SetDirtyStateMeterPercentage(flag ? 0f : percentComplete, flag ? percentComplete : 0f);
		}

		public void AddDisseaseToWorker()
		{
			WorkerBase worker = base.master.GetComponent<ToiletWorkableUse>().worker;
			base.master.AddDisseaseToWorker(worker);
		}

		public void Flush()
		{
			ToiletWorkableUse component = GetComponent<ToiletWorkableUse>();
			bool flag = component.last_user_id == BionicMinionConfig.ID;
			base.master.fillMeter.SetPositionPercent(0f);
			base.master.contaminationMeter.SetPositionPercent(flag ? 0f : 1f);
			base.master.gunkMeter.SetPositionPercent(flag ? 1f : 0f);
			base.smi.ShowFillMeter();
			WorkerBase worker = base.master.GetComponent<ToiletWorkableUse>().worker;
			base.master.Flush(worker);
		}

		public void ShowFillMeter()
		{
			base.master.fillMeter.gameObject.SetActive(value: true);
			base.master.contaminationMeter.gameObject.SetActive(value: false);
			base.master.gunkMeter.gameObject.SetActive(value: false);
		}

		public bool HasContaminatedMass()
		{
			foreach (GameObject item in GetComponent<Storage>().items)
			{
				PrimaryElement component = item.GetComponent<PrimaryElement>();
				if (component == null || (component.ElementID != SimHashes.DirtyWater && component.ElementID != GunkMonitor.GunkElement) || !(component.Mass > 0f))
				{
					continue;
				}
				return true;
			}
			return false;
		}

		public void ShowContaminatedMeter()
		{
			ToiletWorkableUse component = GetComponent<ToiletWorkableUse>();
			bool flag = component.last_user_id == BionicMinionConfig.ID;
			base.master.fillMeter.gameObject.SetActive(value: false);
			base.master.contaminationMeter.gameObject.SetActive(!flag);
			base.master.gunkMeter.gameObject.SetActive(flag);
		}
	}

	public class States : GameStateMachine<States, SMInstance, FlushToilet>
	{
		public class ReadyStates : State
		{
			public State idle;

			public State inuse;

			public State completed;
		}

		public State disconnected;

		public State backedup;

		public ReadyStates ready;

		public State fillingInactive;

		public State filling;

		public State clogged;

		public State unclogged;

		public State flushing;

		public State flushed;

		public BoolParameter outputBlocked;

		public BoolParameter isClogged;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = disconnected;
			base.serializable = SerializeType.ParamsOnly;
			disconnected.PlayAnim("off").EventTransition(GameHashes.ConduitConnectionChanged, backedup, (SMInstance smi) => smi.HasValidConnections()).Enter(delegate(SMInstance smi)
			{
				smi.GetComponent<Operational>().SetActive(value: false);
			});
			backedup.PlayAnim("off").ToggleStatusItem(Db.Get().BuildingStatusItems.OutputPipeFull).EventTransition(GameHashes.ConduitConnectionChanged, disconnected, (SMInstance smi) => !smi.HasValidConnections())
				.ParamTransition(outputBlocked, fillingInactive, GameStateMachine<States, SMInstance, FlushToilet, object>.IsFalse)
				.Enter(delegate(SMInstance smi)
				{
					smi.GetComponent<Operational>().SetActive(value: false);
				});
			filling.PlayAnim("on").Enter(delegate(SMInstance smi)
			{
				smi.GetComponent<Operational>().SetActive(value: true);
			}).EventTransition(GameHashes.ConduitConnectionChanged, disconnected, (SMInstance smi) => !smi.HasValidConnections())
				.ParamTransition(outputBlocked, backedup, GameStateMachine<States, SMInstance, FlushToilet, object>.IsTrue)
				.EventTransition(GameHashes.OnStorageChange, ready, (SMInstance smi) => smi.UpdateFullnessState())
				.EventTransition(GameHashes.OperationalChanged, fillingInactive, (SMInstance smi) => !smi.GetComponent<Operational>().IsOperational);
			fillingInactive.PlayAnim("on").Enter(delegate(SMInstance smi)
			{
				smi.GetComponent<Operational>().SetActive(value: false);
			}).EventTransition(GameHashes.OperationalChanged, filling, (SMInstance smi) => smi.GetComponent<Operational>().IsOperational)
				.ParamTransition(outputBlocked, backedup, GameStateMachine<States, SMInstance, FlushToilet, object>.IsTrue);
			ready.DefaultState(ready.idle).ToggleTag(GameTags.Usable).Enter(delegate(SMInstance smi)
			{
				smi.master.fillMeter.SetPositionPercent(1f);
				smi.master.contaminationMeter.SetPositionPercent(0f);
				smi.master.gunkMeter.SetPositionPercent(0f);
			})
				.PlayAnim("on")
				.EventHandler(GameHashes.FlushGunk, OnFlushedGunk)
				.EventTransition(GameHashes.ConduitConnectionChanged, disconnected, (SMInstance smi) => !smi.HasValidConnections())
				.ParamTransition(outputBlocked, backedup, GameStateMachine<States, SMInstance, FlushToilet, object>.IsTrue)
				.ToggleRecurringChore(CreateUrgentUseChore)
				.ToggleRecurringChore(CreateBreakUseChore);
			ready.idle.ParamTransition(isClogged, clogged, GameStateMachine<States, SMInstance, FlushToilet, object>.IsTrue).Enter(delegate(SMInstance smi)
			{
				smi.GetComponent<Operational>().SetActive(value: false);
			}).ToggleMainStatusItem(Db.Get().BuildingStatusItems.FlushToilet)
				.WorkableStartTransition((SMInstance smi) => smi.master.GetComponent<ToiletWorkableUse>(), ready.inuse);
			ready.inuse.Enter(delegate(SMInstance smi)
			{
				smi.ShowContaminatedMeter();
			}).ToggleMainStatusItem(Db.Get().BuildingStatusItems.FlushToiletInUse).Update(delegate(SMInstance smi, float dt)
			{
				smi.UpdateDirtyState();
			})
				.WorkableCompleteTransition((SMInstance smi) => smi.master.GetComponent<ToiletWorkableUse>(), ready.completed)
				.WorkableStopTransition((SMInstance smi) => smi.master.GetComponent<ToiletWorkableUse>(), flushed);
			ready.completed.Enter(delegate(SMInstance smi)
			{
				smi.AddDisseaseToWorker();
			}).EnterTransition(clogged, (SMInstance smi) => smi.IsClogged).EnterGoTo(flushing);
			clogged.PlayAnims((SMInstance smi) => CLOGGED_ANIMS).Enter(delegate(SMInstance smi)
			{
				smi.ShowContaminatedMeter();
			}).Enter(SetDirtyStatesForClogged)
				.Enter(CreateCleanChore)
				.Exit(CancelCleanChore)
				.ParamTransition(isClogged, unclogged, GameStateMachine<States, SMInstance, FlushToilet, object>.IsFalse);
			unclogged.PlayAnim("full_gunk_pst").OnAnimQueueComplete(flushing);
			flushing.Enter(delegate(SMInstance smi)
			{
				smi.Flush();
			}).PlayAnim("flush").OnAnimQueueComplete(flushed);
			flushed.EventTransition(GameHashes.OnStorageChange, fillingInactive, (SMInstance smi) => !smi.HasContaminatedMass()).ParamTransition(outputBlocked, backedup, GameStateMachine<States, SMInstance, FlushToilet, object>.IsTrue).PlayAnim("on");
		}

		public void OnFlushedGunk(SMInstance smi, object o)
		{
			smi.sm.isClogged.Set(value: true, smi);
		}

		public void SetDirtyStatesForClogged(SMInstance smi)
		{
			smi.SetDirtyStatesForClogged();
		}

		public void CreateCleanChore(SMInstance smi)
		{
			smi.CreateCleanChore();
		}

		public void CancelCleanChore(SMInstance smi)
		{
			smi.CancelCleanChore();
		}

		private Chore CreateUrgentUseChore(SMInstance smi)
		{
			Chore chore = CreateUseChore(smi, Db.Get().ChoreTypes.Pee);
			chore.AddPrecondition(ChorePreconditions.instance.IsBladderFull);
			chore.AddPrecondition(ChorePreconditions.instance.NotCurrentlyPeeing);
			return chore;
		}

		private Chore CreateBreakUseChore(SMInstance smi)
		{
			Chore chore = CreateUseChore(smi, Db.Get().ChoreTypes.BreakPee);
			chore.AddPrecondition(ChorePreconditions.instance.IsBladderNotFull);
			return chore;
		}

		private Chore CreateUseChore(SMInstance smi, ChoreType choreType)
		{
			WorkChore<ToiletWorkableUse> workChore = new WorkChore<ToiletWorkableUse>(choreType, smi.master, null, run_until_complete: true, null, null, null, allow_in_red_alert: false, null, ignore_schedule_block: true, only_when_operational: true, null, is_preemptable: false, allow_in_context_menu: true, allow_prioritization: false, PriorityScreen.PriorityClass.personalNeeds, 5, ignore_building_assignment: false, add_to_daily_report: false);
			smi.activeUseChores.Add(workChore);
			workChore.onExit = (Action<Chore>)Delegate.Combine(workChore.onExit, (Action<Chore>)delegate(Chore exiting_chore)
			{
				smi.activeUseChores.Remove(exiting_chore);
			});
			workChore.AddPrecondition(ChorePreconditions.instance.IsPreferredAssignableOrUrgentBladder, smi.master.GetComponent<Assignable>());
			workChore.AddPrecondition(ChorePreconditions.instance.IsExclusivelyAvailableWithOtherChores, smi.activeUseChores);
			return workChore;
		}
	}

	private static readonly HashedString[] CLOGGED_ANIMS = new HashedString[2] { "full_gunk_pre", "full_gunk" };

	private const string UNCLOG_ANIM = "full_gunk_pst";

	private MeterController fillMeter;

	private MeterController contaminationMeter;

	private MeterController gunkMeter;

	public Meter.Offset meterOffset = Meter.Offset.Behind;

	[SerializeField]
	public float massConsumedPerUse = 5f;

	[SerializeField]
	public float massEmittedPerUse = 5f;

	[SerializeField]
	public float newPeeTemperature;

	[SerializeField]
	public string diseaseId;

	[SerializeField]
	public int diseasePerFlush;

	[SerializeField]
	public int diseaseOnDupePerFlush;

	[SerializeField]
	public bool requireOutput = true;

	[MyCmpGet]
	private ConduitConsumer conduitConsumer;

	[MyCmpGet]
	private Storage storage;

	public static readonly Tag WaterTag = GameTagExtensions.Create(SimHashes.Water);

	private int inputCell;

	private int outputCell;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Building component = GetComponent<Building>();
		inputCell = component.GetUtilityInputCell();
		outputCell = component.GetUtilityOutputCell();
		ConduitFlow liquidConduitFlow = Game.Instance.liquidConduitFlow;
		liquidConduitFlow.onConduitsRebuilt += OnConduitsRebuilt;
		liquidConduitFlow.AddConduitUpdater(OnConduitUpdate);
		KBatchedAnimController component2 = GetComponent<KBatchedAnimController>();
		fillMeter = new MeterController(component2, "meter_target", "meter", meterOffset, Grid.SceneLayer.NoLayer, new Vector3(0.4f, 3.2f, 0.1f));
		contaminationMeter = new MeterController(component2, "meter_target", "meter_dirty", meterOffset, Grid.SceneLayer.NoLayer, new Vector3(0.4f, 3.2f, 0.1f));
		gunkMeter = new MeterController(component2, "meter_target", "meter_gunky", meterOffset, Grid.SceneLayer.NoLayer, new Vector3(0.4f, 3.2f, 0.1f));
		Components.Toilets.Add(this);
		Components.BasicBuildings.Add(this);
		base.smi.StartSM();
		base.smi.ShowFillMeter();
	}

	protected override void OnCleanUp()
	{
		ConduitFlow liquidConduitFlow = Game.Instance.liquidConduitFlow;
		liquidConduitFlow.onConduitsRebuilt -= OnConduitsRebuilt;
		Components.BasicBuildings.Remove(this);
		Components.Toilets.Remove(this);
		base.OnCleanUp();
	}

	private void OnConduitsRebuilt()
	{
		Trigger(-2094018600, (object)BoxedBools.False);
	}

	public bool IsUsable()
	{
		return base.smi.HasTag(GameTags.Usable);
	}

	private void AddDisseaseToWorker(WorkerBase worker)
	{
		if (worker != null)
		{
			byte index = Db.Get().Diseases.GetIndex(diseaseId);
			PrimaryElement component = worker.GetComponent<PrimaryElement>();
			component.AddDisease(index, diseaseOnDupePerFlush, "FlushToilet.Flush");
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Resource, string.Format(DUPLICANTS.DISEASES.ADDED_POPFX, Db.Get().Diseases[index].Name, diseasePerFlush + diseaseOnDupePerFlush), base.transform, Vector3.up);
			Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_LotsOfGerms);
		}
		else
		{
			DebugUtil.LogWarningArgs("Tried to add disease on toilet use but worker was null");
		}
	}

	private void Flush(WorkerBase worker)
	{
		ToiletWorkableUse component = GetComponent<ToiletWorkableUse>();
		ListPool<GameObject, Storage>.PooledList pooledList = ListPool<GameObject, Storage>.Allocate();
		storage.Find(WaterTag, pooledList);
		float num = 0f;
		float num2 = massConsumedPerUse;
		foreach (GameObject item in pooledList)
		{
			PrimaryElement component2 = item.GetComponent<PrimaryElement>();
			float num3 = Mathf.Min(component2.Mass, num2);
			component2.Mass -= num3;
			num2 -= num3;
			num += num3 * component2.Temperature;
		}
		pooledList.Recycle();
		float lastAmountOfWasteMassRemovedFromDupe = component.lastAmountOfWasteMassRemovedFromDupe;
		num += lastAmountOfWasteMassRemovedFromDupe * newPeeTemperature;
		float num4 = massConsumedPerUse + lastAmountOfWasteMassRemovedFromDupe;
		float temperature = num / num4;
		byte index = Db.Get().Diseases.GetIndex(diseaseId);
		storage.AddLiquid(component.lastElementRemovedFromDupe, num4, temperature, index, diseasePerFlush);
	}

	public List<Descriptor> RequirementDescriptors()
	{
		List<Descriptor> list = new List<Descriptor>();
		Element element = ElementLoader.FindElementByHash(SimHashes.Water);
		string arg = element.tag.ProperName();
		list.Add(new Descriptor(string.Format(UI.BUILDINGEFFECTS.ELEMENTCONSUMEDPERUSE, arg, GameUtil.GetFormattedMass(massConsumedPerUse, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.UseThreshold, includeSuffix: true, "{0:0.##}")), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.ELEMENTCONSUMEDPERUSE, arg, GameUtil.GetFormattedMass(massConsumedPerUse, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.UseThreshold, includeSuffix: true, "{0:0.##}")), Descriptor.DescriptorType.Requirement));
		return list;
	}

	public List<Descriptor> EffectDescriptors()
	{
		List<Descriptor> list = new List<Descriptor>();
		Element element = ElementLoader.FindElementByHash(SimHashes.DirtyWater);
		string arg = element.tag.ProperName();
		list.Add(new Descriptor(string.Format(UI.BUILDINGEFFECTS.ELEMENTEMITTED_TOILET, arg, GameUtil.GetFormattedMass(massEmittedPerUse, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.UseThreshold, includeSuffix: true, "{0:0.##}"), GameUtil.GetFormattedTemperature(newPeeTemperature)), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.ELEMENTEMITTED_TOILET, arg, GameUtil.GetFormattedMass(massEmittedPerUse, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.UseThreshold, includeSuffix: true, "{0:0.##}"), GameUtil.GetFormattedTemperature(newPeeTemperature))));
		Disease disease = Db.Get().Diseases.Get(diseaseId);
		int units = diseasePerFlush + diseaseOnDupePerFlush;
		list.Add(new Descriptor(string.Format(UI.BUILDINGEFFECTS.DISEASEEMITTEDPERUSE, disease.Name, GameUtil.GetFormattedDiseaseAmount(units)), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.DISEASEEMITTEDPERUSE, disease.Name, GameUtil.GetFormattedDiseaseAmount(units)), Descriptor.DescriptorType.DiseaseSource));
		return list;
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		list.AddRange(RequirementDescriptors());
		list.AddRange(EffectDescriptors());
		return list;
	}

	private void OnConduitUpdate(float dt)
	{
		if (GetSMI() != null)
		{
			ConduitFlow liquidConduitFlow = Game.Instance.liquidConduitFlow;
			bool value = base.smi.master.requireOutput && liquidConduitFlow.GetContents(outputCell).mass > 0f && base.smi.HasContaminatedMass();
			base.smi.sm.outputBlocked.Set(value, base.smi);
		}
	}
}
