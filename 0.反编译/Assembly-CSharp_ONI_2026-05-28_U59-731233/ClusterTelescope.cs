using System;
using System.Collections.Generic;
using Database;
using KSerialization;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class ClusterTelescope : GameStateMachine<ClusterTelescope, ClusterTelescope.Instance, IStateMachineTarget, ClusterTelescope.Def>
{
	public class Def : BaseDef
	{
		public int clearScanCellRadius = 15;

		public int analyzeClusterRadius = 3;

		public KAnimFile[] workableOverrideAnims;

		public bool providesOxygen = false;

		public SkyVisibilityInfo skyVisibilityInfo;
	}

	public class WorkStates : State
	{
		public State decide;

		public State identifyMeteorShower;

		public State revealTile;
	}

	public class ReadyStates : State
	{
		public State no_visibility;

		public WorkStates ready_to_work;
	}

	public new class Instance : GameInstance, ICheckboxControl, BuildingStatusItems.ISkyVisInfo
	{
		private float m_percentClear = 0f;

		[Serialize]
		public bool allowMeteorIdentification = true;

		[Serialize]
		private bool m_hasAnalyzeTarget;

		[Serialize]
		private AxialI m_analyzeTarget;

		[MyCmpAdd]
		private ClusterTelescopeWorkable m_workable;

		[MyCmpAdd]
		private ClusterTelescopeIdentifyMeteorWorkable m_identifyMeteorWorkable;

		public KAnimFile[] workableOverrideAnims;

		public bool providesOxygen;

		public float PercentClear => m_percentClear;

		private bool hasMeteorShowerTarget => meteorShowerTarget != null;

		private ClusterMapMeteorShower.Instance meteorShowerTarget => base.sm.meteorShowerTarget.Get(this)?.GetSMI<ClusterMapMeteorShower.Instance>();

		public string CheckboxTitleKey => "STRINGS.UI.UISIDESCREENS.CLUSTERTELESCOPESIDESCREEN.TITLE";

		public string CheckboxLabel => UI.UISIDESCREENS.CLUSTERTELESCOPESIDESCREEN.CHECKBOX_METEORS;

		public string CheckboxTooltip => UI.UISIDESCREENS.CLUSTERTELESCOPESIDESCREEN.CHECKBOX_TOOLTIP_METEORS;

		float BuildingStatusItems.ISkyVisInfo.GetPercentVisible01()
		{
			return m_percentClear;
		}

		public Instance(IStateMachineTarget smi, Def def)
			: base(smi, def)
		{
			workableOverrideAnims = def.workableOverrideAnims;
			providesOxygen = def.providesOxygen;
		}

		public bool ShouldBeWorkingOnRevealingTile()
		{
			return CheckHasAnalyzeTarget() && (!allowMeteorIdentification || !CheckHasValidMeteorTarget());
		}

		public bool ShouldBeWorkingOnMeteorIdentification()
		{
			return allowMeteorIdentification && CheckHasValidMeteorTarget();
		}

		public bool IsAnyAvailableWorkToBeDone()
		{
			return CheckHasAnalyzeTarget() || ShouldBeWorkingOnMeteorIdentification();
		}

		public bool CheckHasValidMeteorTarget()
		{
			ClusterFogOfWarManager.Instance sMI = SaveGame.Instance.GetSMI<ClusterFogOfWarManager.Instance>();
			if (HasValidMeteor())
			{
				return true;
			}
			ClusterMapMeteorShower.Instance result = null;
			AxialI myWorldLocation = this.GetMyWorldLocation();
			ClusterGrid.Instance.GetVisibleUnidentifiedMeteorShowerWithinRadius(myWorldLocation, base.def.analyzeClusterRadius, out result);
			base.sm.meteorShowerTarget.Set(result?.gameObject, this);
			return result != null;
		}

		public bool CheckHasAnalyzeTarget()
		{
			ClusterFogOfWarManager.Instance sMI = SaveGame.Instance.GetSMI<ClusterFogOfWarManager.Instance>();
			if (m_hasAnalyzeTarget && !sMI.IsLocationRevealed(m_analyzeTarget))
			{
				return true;
			}
			AxialI myWorldLocation = this.GetMyWorldLocation();
			m_hasAnalyzeTarget = sMI.GetUnrevealedLocationWithinRadius(myWorldLocation, base.def.analyzeClusterRadius, out m_analyzeTarget);
			return m_hasAnalyzeTarget;
		}

		private bool HasValidMeteor()
		{
			if (!hasMeteorShowerTarget)
			{
				return false;
			}
			AxialI myWorldLocation = this.GetMyWorldLocation();
			bool flag = ClusterGrid.Instance.IsInRange(meteorShowerTarget.ClusterGridPosition(), myWorldLocation, base.def.analyzeClusterRadius);
			bool flag2 = SaveGame.Instance.GetSMI<ClusterFogOfWarManager.Instance>().IsLocationRevealed(meteorShowerTarget.ClusterGridPosition());
			bool hasBeenIdentified = meteorShowerTarget.HasBeenIdentified;
			if (!flag || !flag2 || hasBeenIdentified)
			{
				return false;
			}
			return true;
		}

		public Chore CreateRevealTileChore()
		{
			WorkChore<ClusterTelescopeWorkable> workChore = new WorkChore<ClusterTelescopeWorkable>(Db.Get().ChoreTypes.Research, m_workable);
			if (providesOxygen)
			{
				workChore.AddPrecondition(Telescope.ContainsOxygen);
			}
			return workChore;
		}

		public Chore CreateIdentifyMeteorChore()
		{
			WorkChore<ClusterTelescopeIdentifyMeteorWorkable> workChore = new WorkChore<ClusterTelescopeIdentifyMeteorWorkable>(Db.Get().ChoreTypes.Research, m_identifyMeteorWorkable);
			if (providesOxygen)
			{
				workChore.AddPrecondition(Telescope.ContainsOxygen);
			}
			return workChore;
		}

		public ClusterMapMeteorShower.Instance GetMeteorTarget()
		{
			return meteorShowerTarget;
		}

		public AxialI GetAnalyzeTarget()
		{
			Debug.Assert(m_hasAnalyzeTarget, "GetAnalyzeTarget called but this telescope has no target assigned.");
			return m_analyzeTarget;
		}

		public bool HasSkyVisibility()
		{
			(bool isAnyVisible, float percentVisible01) visibilityOf = base.def.skyVisibilityInfo.GetVisibilityOf(base.gameObject);
			bool item = visibilityOf.isAnyVisible;
			float item2 = visibilityOf.percentVisible01;
			m_percentClear = item2;
			return item;
		}

		public bool GetCheckboxValue()
		{
			return allowMeteorIdentification;
		}

		public void SetCheckboxValue(bool value)
		{
			allowMeteorIdentification = value;
			base.sm.MeteorIdenificationPriorityChangeSignal.Trigger(this);
		}
	}

	public class ClusterTelescopeWorkable : Workable, OxygenBreather.IGasProvider
	{
		[MySmiReq]
		private Instance m_telescope;

		private ClusterFogOfWarManager.Instance m_fowManager = null;

		private GameObject telescopeTargetMarker;

		private AxialI currentTarget;

		[MyCmpGet]
		private Storage storage;

		private AttributeModifier radiationShielding;

		private float checkMarkerTimer = 0f;

		private float checkMarkerFrequency = 1f;

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			attributeConverter = Db.Get().AttributeConverters.ResearchSpeed;
			attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.ALL_DAY_EXPERIENCE;
			skillExperienceSkillGroup = Db.Get().SkillGroups.Research.Id;
			skillExperienceMultiplier = SKILLS.ALL_DAY_EXPERIENCE;
			requiredSkillPerk = Db.Get().SkillPerks.CanUseClusterTelescope.Id;
			workLayer = Grid.SceneLayer.BuildingUse;
			radiationShielding = new AttributeModifier(Db.Get().Attributes.RadiationResistance.Id, FIXEDTRAITS.COSMICRADIATION.TELESCOPE_RADIATION_SHIELDING, STRINGS.BUILDINGS.PREFABS.CLUSTERTELESCOPEENCLOSED.NAME);
		}

		protected override void OnCleanUp()
		{
			if (telescopeTargetMarker != null)
			{
				Util.KDestroyGameObject(telescopeTargetMarker);
			}
			base.OnCleanUp();
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			OnWorkableEventCB = (Action<Workable, WorkableEvent>)Delegate.Combine(OnWorkableEventCB, new Action<Workable, WorkableEvent>(OnWorkableEvent));
			m_fowManager = SaveGame.Instance.GetSMI<ClusterFogOfWarManager.Instance>();
			SetWorkTime(float.PositiveInfinity);
			overrideAnims = m_telescope.workableOverrideAnims;
		}

		private void OnWorkableEvent(Workable workable, WorkableEvent ev)
		{
			WorkerBase workerBase = base.worker;
			if (workerBase == null)
			{
				return;
			}
			KPrefabID component = workerBase.GetComponent<KPrefabID>();
			OxygenBreather component2 = workerBase.GetComponent<OxygenBreather>();
			Klei.AI.Attributes attributes = workerBase.GetAttributes();
			KSelectable component3 = GetComponent<KSelectable>();
			switch (ev)
			{
			case WorkableEvent.WorkStarted:
				ShowProgressBar(show: true);
				progressBar.SetUpdateFunc(() => m_fowManager.GetRevealCompleteFraction(currentTarget));
				currentTarget = m_telescope.GetAnalyzeTarget();
				if (!ClusterGrid.Instance.GetEntityOfLayerAtCell(currentTarget, EntityLayer.Telescope))
				{
					telescopeTargetMarker = GameUtil.KInstantiate(Assets.GetPrefab("TelescopeTarget"), Grid.SceneLayer.Background);
					telescopeTargetMarker.SetActive(value: true);
					telescopeTargetMarker.GetComponent<TelescopeTarget>().Init(currentTarget);
					telescopeTargetMarker.GetComponent<TelescopeTarget>().SetTargetMeteorShower(null);
				}
				if (m_telescope.providesOxygen)
				{
					attributes.Add(radiationShielding);
					if (component2 != null)
					{
						component2.AddGasProvider(this);
					}
					workerBase.GetComponent<CreatureSimTemperatureTransfer>().enabled = false;
					component.AddTag(GameTags.Shaded);
				}
				GetComponent<Operational>().SetActive(value: true);
				checkMarkerFrequency = UnityEngine.Random.Range(2f, 5f);
				component3.AddStatusItem(Db.Get().BuildingStatusItems.TelescopeWorking, this);
				break;
			case WorkableEvent.WorkStopped:
				if (m_telescope.providesOxygen)
				{
					attributes.Remove(radiationShielding);
					if (component2 != null)
					{
						component2.RemoveGasProvider(this);
					}
					workerBase.GetComponent<CreatureSimTemperatureTransfer>().enabled = true;
					component.RemoveTag(GameTags.Shaded);
				}
				GetComponent<Operational>().SetActive(value: false);
				if (telescopeTargetMarker != null)
				{
					Util.KDestroyGameObject(telescopeTargetMarker);
				}
				ShowProgressBar(show: false);
				component3.RemoveStatusItem(Db.Get().BuildingStatusItems.TelescopeWorking, this);
				break;
			}
		}

		public override List<Descriptor> GetDescriptors(GameObject go)
		{
			List<Descriptor> descriptors = base.GetDescriptors(go);
			Element element = ElementLoader.FindElementByHash(SimHashes.Oxygen);
			Descriptor item = default(Descriptor);
			item.SetupDescriptor(element.tag.ProperName(), string.Format(STRINGS.BUILDINGS.PREFABS.TELESCOPE.REQUIREMENT_TOOLTIP, element.tag.ProperName()), Descriptor.DescriptorType.Requirement);
			descriptors.Add(item);
			Descriptor item2 = default(Descriptor);
			item2.SetupDescriptor(string.Format(UI.BUILDINGEFFECTS.PRODUCESDATABANKS, 3), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.PRODUCESDATABANKS, 3));
			descriptors.Add(item2);
			return descriptors;
		}

		public override float GetEfficiencyMultiplier(WorkerBase worker)
		{
			return base.GetEfficiencyMultiplier(worker) * Mathf.Clamp01(m_telescope.PercentClear);
		}

		protected override bool OnWorkTick(WorkerBase worker, float dt)
		{
			AxialI analyzeTarget = m_telescope.GetAnalyzeTarget();
			bool flag = false;
			if (analyzeTarget != currentTarget)
			{
				if ((bool)telescopeTargetMarker)
				{
					telescopeTargetMarker.GetComponent<TelescopeTarget>().Init(analyzeTarget);
				}
				currentTarget = analyzeTarget;
				flag = true;
			}
			if (!flag && checkMarkerTimer > checkMarkerFrequency)
			{
				checkMarkerTimer = 0f;
				if (!telescopeTargetMarker && !ClusterGrid.Instance.GetEntityOfLayerAtCell(currentTarget, EntityLayer.Telescope))
				{
					telescopeTargetMarker = GameUtil.KInstantiate(Assets.GetPrefab("TelescopeTarget"), Grid.SceneLayer.Background);
					telescopeTargetMarker.SetActive(value: true);
					telescopeTargetMarker.GetComponent<TelescopeTarget>().Init(currentTarget);
				}
			}
			checkMarkerTimer += dt;
			float num = ROCKETRY.CLUSTER_FOW.POINTS_TO_REVEAL / ROCKETRY.CLUSTER_FOW.DEFAULT_CYCLES_PER_REVEAL;
			float num2 = num / 600f;
			float points = dt * num2;
			if (m_fowManager.EarnRevealPointsForLocation(currentTarget, points))
			{
				for (int i = 0; i < 3; i++)
				{
					Util.KInstantiate(Assets.GetPrefab(DatabankHelper.ID), base.transform.position + Vector3.up * 2f + Vector3.right, Quaternion.identity, null, null, initialize_id: true, 23).SetActive(value: true);
				}
				PopFXManager.Instance.SpawnFX(global::Def.GetUISprite(DatabankHelper.ID).first, DatabankHelper.NAME_PLURAL, base.transform, Vector3.up * 2f, 1.5f, track_target: true);
			}
			return base.OnWorkTick(worker, dt);
		}

		protected override void OnCompleteWork(WorkerBase worker)
		{
			base.OnCompleteWork(worker);
		}

		public void OnSetOxygenBreather(OxygenBreather oxygen_breather)
		{
		}

		public void OnClearOxygenBreather(OxygenBreather oxygen_breather)
		{
		}

		public bool ShouldEmitCO2()
		{
			return false;
		}

		public bool ShouldStoreCO2()
		{
			return false;
		}

		public bool ConsumeGas(OxygenBreather oxygen_breather, float amount)
		{
			if (storage.items.Count <= 0)
			{
				return false;
			}
			GameObject gameObject = storage.items[0];
			if (gameObject == null)
			{
				return false;
			}
			PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
			float mass = component.Mass;
			float amount_consumed = 0f;
			float aggregate_temperature = 0f;
			SimHashes mostRelevantItemElement = SimHashes.Vacuum;
			storage.ConsumeAndGetDisease(GameTags.Breathable, amount, out amount_consumed, out var disease_info, out aggregate_temperature, out mostRelevantItemElement);
			bool result = amount_consumed >= amount;
			OxygenBreather.BreathableGasConsumed(oxygen_breather, mostRelevantItemElement, amount_consumed, aggregate_temperature, disease_info.idx, disease_info.count);
			return result;
		}

		public bool IsLowOxygen()
		{
			if (storage.items.Count <= 0)
			{
				return true;
			}
			PrimaryElement primaryElement = storage.FindFirstWithMass(GameTags.Breathable);
			return primaryElement == null || primaryElement.Mass == 0f;
		}

		public bool HasOxygen()
		{
			if (storage.items.Count <= 0)
			{
				return false;
			}
			PrimaryElement primaryElement = storage.FindFirstWithMass(GameTags.Breathable);
			return primaryElement != null && primaryElement.Mass > 0f;
		}

		public bool IsBlocked()
		{
			return false;
		}
	}

	public class ClusterTelescopeIdentifyMeteorWorkable : Workable, OxygenBreather.IGasProvider
	{
		[MySmiReq]
		private Instance m_telescope;

		private ClusterFogOfWarManager.Instance m_fowManager = null;

		private GameObject telescopeTargetMarker;

		private ClusterMapMeteorShower.Instance currentTarget;

		[MyCmpGet]
		private Storage storage;

		private AttributeModifier radiationShielding;

		private float checkMarkerTimer = 0f;

		private float checkMarkerFrequency = 1f;

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			attributeConverter = Db.Get().AttributeConverters.ResearchSpeed;
			attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.ALL_DAY_EXPERIENCE;
			skillExperienceSkillGroup = Db.Get().SkillGroups.Research.Id;
			skillExperienceMultiplier = SKILLS.ALL_DAY_EXPERIENCE;
			requiredSkillPerk = Db.Get().SkillPerks.CanUseClusterTelescope.Id;
			workLayer = Grid.SceneLayer.BuildingUse;
			radiationShielding = new AttributeModifier(Db.Get().Attributes.RadiationResistance.Id, FIXEDTRAITS.COSMICRADIATION.TELESCOPE_RADIATION_SHIELDING, STRINGS.BUILDINGS.PREFABS.CLUSTERTELESCOPEENCLOSED.NAME);
		}

		protected override void OnCleanUp()
		{
			if (telescopeTargetMarker != null)
			{
				Util.KDestroyGameObject(telescopeTargetMarker);
			}
			base.OnCleanUp();
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			OnWorkableEventCB = (Action<Workable, WorkableEvent>)Delegate.Combine(OnWorkableEventCB, new Action<Workable, WorkableEvent>(OnWorkableEvent));
			m_fowManager = SaveGame.Instance.GetSMI<ClusterFogOfWarManager.Instance>();
			SetWorkTime(float.PositiveInfinity);
			overrideAnims = m_telescope.workableOverrideAnims;
		}

		private void OnWorkableEvent(Workable workable, WorkableEvent ev)
		{
			WorkerBase workerBase = base.worker;
			if (workerBase == null)
			{
				return;
			}
			KPrefabID component = workerBase.GetComponent<KPrefabID>();
			OxygenBreather component2 = workerBase.GetComponent<OxygenBreather>();
			Klei.AI.Attributes attributes = workerBase.GetAttributes();
			KSelectable component3 = GetComponent<KSelectable>();
			switch (ev)
			{
			case WorkableEvent.WorkStarted:
			{
				ShowProgressBar(show: true);
				progressBar.SetUpdateFunc(() => (currentTarget == null) ? 0f : currentTarget.IdentifyingProgress);
				currentTarget = m_telescope.GetMeteorTarget();
				AxialI axialI = currentTarget.ClusterGridPosition();
				if (!ClusterGrid.Instance.GetEntityOfLayerAtCell(axialI, EntityLayer.Telescope))
				{
					telescopeTargetMarker = GameUtil.KInstantiate(Assets.GetPrefab("TelescopeTarget"), Grid.SceneLayer.Background);
					telescopeTargetMarker.SetActive(value: true);
					TelescopeTarget component4 = telescopeTargetMarker.GetComponent<TelescopeTarget>();
					component4.Init(axialI);
					component4.SetTargetMeteorShower(currentTarget);
				}
				if (m_telescope.providesOxygen)
				{
					attributes.Add(radiationShielding);
					component2.AddGasProvider(this);
					component2.GetComponent<CreatureSimTemperatureTransfer>().enabled = false;
					component.AddTag(GameTags.Shaded);
				}
				GetComponent<Operational>().SetActive(value: true);
				checkMarkerFrequency = UnityEngine.Random.Range(2f, 5f);
				component3.AddStatusItem(Db.Get().BuildingStatusItems.ClusterTelescopeMeteorWorking, this);
				break;
			}
			case WorkableEvent.WorkStopped:
				if (m_telescope.providesOxygen)
				{
					attributes.Remove(radiationShielding);
					component2.RemoveGasProvider(this);
					component2.GetComponent<CreatureSimTemperatureTransfer>().enabled = true;
					component.RemoveTag(GameTags.Shaded);
				}
				GetComponent<Operational>().SetActive(value: false);
				if (telescopeTargetMarker != null)
				{
					Util.KDestroyGameObject(telescopeTargetMarker);
				}
				ShowProgressBar(show: false);
				component3.RemoveStatusItem(Db.Get().BuildingStatusItems.ClusterTelescopeMeteorWorking, this);
				break;
			}
		}

		protected override bool OnWorkTick(WorkerBase worker, float dt)
		{
			ClusterMapMeteorShower.Instance meteorTarget = m_telescope.GetMeteorTarget();
			AxialI axialI = meteorTarget.ClusterGridPosition();
			bool flag = false;
			if (meteorTarget != currentTarget)
			{
				if ((bool)telescopeTargetMarker)
				{
					TelescopeTarget component = telescopeTargetMarker.GetComponent<TelescopeTarget>();
					component.Init(axialI);
					component.SetTargetMeteorShower(meteorTarget);
				}
				currentTarget = meteorTarget;
				flag = true;
			}
			if (!flag && checkMarkerTimer > checkMarkerFrequency)
			{
				checkMarkerTimer = 0f;
				if (!telescopeTargetMarker && !ClusterGrid.Instance.GetEntityOfLayerAtCell(axialI, EntityLayer.Telescope))
				{
					telescopeTargetMarker = GameUtil.KInstantiate(Assets.GetPrefab("TelescopeTarget"), Grid.SceneLayer.Background);
					telescopeTargetMarker.SetActive(value: true);
					telescopeTargetMarker.GetComponent<TelescopeTarget>().Init(axialI);
				}
			}
			checkMarkerTimer += dt;
			float num = 20f;
			float points = dt / num;
			currentTarget.ProgressIdentifiction(points);
			return base.OnWorkTick(worker, dt);
		}

		public void OnSetOxygenBreather(OxygenBreather oxygen_breather)
		{
		}

		public void OnClearOxygenBreather(OxygenBreather oxygen_breather)
		{
		}

		public bool ShouldEmitCO2()
		{
			return false;
		}

		public bool ShouldStoreCO2()
		{
			return false;
		}

		public bool ConsumeGas(OxygenBreather oxygen_breather, float amount)
		{
			if (storage.items.Count <= 0)
			{
				return false;
			}
			GameObject gameObject = storage.items[0];
			if (gameObject == null)
			{
				return false;
			}
			PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
			float mass = component.Mass;
			float amount_consumed = 0f;
			float aggregate_temperature = 0f;
			SimHashes mostRelevantItemElement = SimHashes.Vacuum;
			storage.ConsumeAndGetDisease(GameTags.Breathable, amount, out amount_consumed, out var disease_info, out aggregate_temperature, out mostRelevantItemElement);
			bool result = amount_consumed >= amount;
			OxygenBreather.BreathableGasConsumed(oxygen_breather, mostRelevantItemElement, amount_consumed, aggregate_temperature, disease_info.idx, disease_info.count);
			return result;
		}

		public bool IsLowOxygen()
		{
			if (storage.items.Count <= 0)
			{
				return true;
			}
			GameObject gameObject = storage.items[0];
			if (gameObject == null)
			{
				return false;
			}
			PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
			return component.Mass > 0f;
		}

		public bool HasOxygen()
		{
			if (storage.items.Count <= 0)
			{
				return false;
			}
			GameObject gameObject = storage.items[0];
			if (gameObject == null)
			{
				return false;
			}
			PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
			return component.Mass > 0f;
		}

		public bool IsBlocked()
		{
			return false;
		}
	}

	public State all_work_complete;

	public ReadyStates ready;

	public TargetParameter meteorShowerTarget;

	public Signal MeteorIdenificationPriorityChangeSignal;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = ready.no_visibility;
		root.Update(delegate(Instance smi, float dt)
		{
			KSelectable component = smi.GetComponent<KSelectable>();
			bool flag = Mathf.Approximately(0f, smi.PercentClear) || smi.PercentClear < 0f;
			bool flag2 = Mathf.Approximately(1f, smi.PercentClear) || smi.PercentClear > 1f;
			component.ToggleStatusItem(Db.Get().BuildingStatusItems.SkyVisNone, flag, smi);
			component.ToggleStatusItem(Db.Get().BuildingStatusItems.SkyVisLimited, !flag && !flag2, smi);
		});
		ready.DoNothing();
		ready.no_visibility.UpdateTransition(ready.ready_to_work, (Instance smi, float dt) => smi.HasSkyVisibility());
		ready.ready_to_work.UpdateTransition(ready.no_visibility, (Instance smi, float dt) => !smi.HasSkyVisibility()).DefaultState(ready.ready_to_work.decide);
		ready.ready_to_work.decide.EnterTransition(ready.ready_to_work.identifyMeteorShower, (Instance smi) => smi.ShouldBeWorkingOnMeteorIdentification()).EnterTransition(ready.ready_to_work.revealTile, (Instance smi) => smi.ShouldBeWorkingOnRevealingTile()).EnterTransition(all_work_complete, (Instance smi) => !smi.IsAnyAvailableWorkToBeDone());
		ready.ready_to_work.identifyMeteorShower.OnSignal(MeteorIdenificationPriorityChangeSignal, ready.ready_to_work.decide, (Instance smi, SignalParameter param) => !smi.ShouldBeWorkingOnMeteorIdentification()).ParamTransition(meteorShowerTarget, ready.ready_to_work.decide, GameStateMachine<ClusterTelescope, Instance, IStateMachineTarget, Def>.IsNull).EventTransition(GameHashes.ClusterMapMeteorShowerIdentified, (Instance smi) => Game.Instance, ready.ready_to_work.decide, (Instance smi) => !smi.ShouldBeWorkingOnMeteorIdentification())
			.EventTransition(GameHashes.ClusterMapMeteorShowerMoved, (Instance smi) => Game.Instance, ready.ready_to_work.decide, (Instance smi) => !smi.ShouldBeWorkingOnMeteorIdentification())
			.ToggleChore((Instance smi) => smi.CreateIdentifyMeteorChore(), ready.no_visibility);
		ready.ready_to_work.revealTile.OnSignal(MeteorIdenificationPriorityChangeSignal, ready.ready_to_work.decide, (Instance smi, SignalParameter param) => smi.ShouldBeWorkingOnMeteorIdentification()).EventTransition(GameHashes.ClusterFogOfWarRevealed, (Instance smi) => Game.Instance, ready.ready_to_work.decide, (Instance smi) => !smi.ShouldBeWorkingOnRevealingTile()).EventTransition(GameHashes.ClusterMapMeteorShowerMoved, (Instance smi) => Game.Instance, ready.ready_to_work.decide, (Instance smi) => smi.ShouldBeWorkingOnMeteorIdentification())
			.ToggleChore((Instance smi) => smi.CreateRevealTileChore(), ready.no_visibility);
		all_work_complete.OnSignal(MeteorIdenificationPriorityChangeSignal, ready.no_visibility, (Instance smi, SignalParameter param) => smi.IsAnyAvailableWorkToBeDone()).ToggleMainStatusItem(Db.Get().BuildingStatusItems.ClusterTelescopeAllWorkComplete).EventTransition(GameHashes.ClusterLocationChanged, (Instance smi) => Game.Instance, ready.no_visibility, (Instance smi) => smi.IsAnyAvailableWorkToBeDone())
			.EventTransition(GameHashes.ClusterMapMeteorShowerMoved, (Instance smi) => Game.Instance, ready.no_visibility, (Instance smi) => smi.ShouldBeWorkingOnMeteorIdentification());
	}
}
