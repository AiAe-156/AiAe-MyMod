using System;
using System.Collections.Generic;
using FoodRehydrator;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class EatChore : Chore<EatChore.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, EatChore, object>.GameInstance
	{
		private int locatorCell;

		public KAnimFile eatAnim;

		public StatesInstance(EatChore master)
			: base(master)
		{
		}

		private static Assignable GetPreferredMessStation(GameObject diner)
		{
			Ownables soleOwner = diner.GetComponent<MinionIdentity>().GetSoleOwner();
			diner.TryGetComponent<Navigator>(out var component);
			foreach (Assignable preferredAssignable in Game.Instance.assignmentManager.GetPreferredAssignables(soleOwner, component, Db.Get().AssignableSlots.MessStation))
			{
				IDiningSeat diningSeat = ResolveDiningSeat(preferredAssignable.gameObject);
				if (diningSeat != null)
				{
					Operational operational = diningSeat.FindOperational();
					if ((!(operational != null) || operational.IsOperational) && preferredAssignable.GetComponent<Reservable>().IsReservableBy(diner))
					{
						return preferredAssignable;
					}
				}
			}
			return null;
		}

		public static Assignable ReserveMessStation(GameObject messStation, GameObject diner)
		{
			if (messStation != null)
			{
				messStation.GetComponent<Reservable>().ClearReservation();
			}
			Assignable preferredMessStation = GetPreferredMessStation(diner);
			if (preferredMessStation != null)
			{
				Reservable component = preferredMessStation.GetComponent<Reservable>();
				if (!component.Reserve(diner))
				{
					if (component.IsReservableBy(diner))
					{
						Debug.Log("Failed to reserve dining seat. We have already reserved it.");
					}
					else
					{
						Debug.LogWarning("Failed to reserve dining seat. Someone else has already reserved it!");
					}
				}
			}
			return preferredMessStation;
		}

		public void UpdateMessStation()
		{
			Assignable value = ReserveMessStation(base.sm.messstation.Get(base.smi), base.sm.eater.Get(base.smi));
			base.sm.messstation.Set(value, base.smi);
		}

		public void ClearMessStation()
		{
			GameObject gameObject = base.smi.sm.messstation.Get(base.smi);
			if (gameObject != null)
			{
				gameObject.GetComponent<Reservable>().ClearReservation();
			}
			base.sm.messstation.Set(null, base.smi);
		}

		public static bool UseGarnish(GameObject messStation)
		{
			if (messStation == null)
			{
				return false;
			}
			return ResolveDiningSeat(messStation)?.HasGarnish ?? false;
		}

		public bool UseGarnish()
		{
			if (base.smi.sm.messstation == null)
			{
				return false;
			}
			return UseGarnish(base.sm.messstation.Get(base.smi));
		}

		public static (GameObject, int) CreateLocator(Sensors sensors, Transform transform, string locatorName)
		{
			int num = sensors.GetSensor<SafeCellSensor>().GetCellQuery();
			if (num == Grid.InvalidCell)
			{
				num = Grid.PosToCell(transform.GetPosition());
			}
			Vector3 pos = Grid.CellToPosCBC(num, Grid.SceneLayer.Move);
			Grid.Reserved[num] = true;
			return (ChoreHelpers.CreateLocator(locatorName, pos), num);
		}

		public void CreateLocator()
		{
			GameObject value;
			(value, locatorCell) = CreateLocator(base.sm.eater.Get<Sensors>(base.smi), base.sm.eater.Get<Transform>(base.smi), "EatLocator");
			base.sm.locator.Set(value, this);
		}

		public void DestroyLocator()
		{
			Grid.Reserved[locatorCell] = false;
			ChoreHelpers.DestroyLocator(base.sm.locator.Get(this));
			base.sm.locator.Set(null, this);
		}

		public static KAnimFile OnEnterMessStation(GameObject messStation, GameObject diner, GameObject food, bool dinerIsBionic, float? effectDurationOverride = null)
		{
			IDiningSeat diningSeat = ResolveDiningSeat(messStation);
			if (diningSeat == null)
			{
				return null;
			}
			KAnimControllerBase component = diner.GetComponent<KAnimControllerBase>();
			KAnimFile kAnimFile = ResolveEatAnim(diningSeat, dinerIsBionic);
			component.AddAnimOverrides(kAnimFile);
			if (food != null && food.TryGetComponent<Edible>(out var component2))
			{
				component2.workLayer = Grid.SceneLayer.BuildingFront;
			}
			EffectInstance effectInstance = null;
			Effects component3 = diner.GetComponent<Effects>();
			Storage storage = diningSeat.FindStorage();
			Garnish active = Garnish.GetActive(storage);
			if (active != null)
			{
				effectInstance = active.Activate(storage, diner);
			}
			diningSeat.Diner = diner.GetComponent<KPrefabID>();
			messStation.Trigger(1356255274);
			Room roomOfGameObject = Game.Instance.roomProber.GetRoomOfGameObject(messStation);
			KPrefabID component4 = messStation.GetComponent<KPrefabID>();
			if (effectDurationOverride.HasValue)
			{
				List<EffectInstance> result = null;
				roomOfGameObject?.roomType.TriggerRoomEffects(component4, component3, out result);
				if (effectInstance != null)
				{
					if (result == null)
					{
						result = new List<EffectInstance>();
					}
					result.Add(effectInstance);
				}
				if (result != null)
				{
					foreach (EffectInstance item in result)
					{
						item.timeRemaining = effectDurationOverride.Value;
					}
				}
			}
			else
			{
				roomOfGameObject?.roomType.TriggerRoomEffects(component4, component3);
			}
			return kAnimFile;
		}

		public static void OnExitMessStation(GameObject messStation, GameObject diner, KAnimFile eatAnim)
		{
			diner.GetComponent<KAnimControllerBase>().RemoveAnimOverrides(eatAnim);
			Garnish.Deactivate(diner);
			IDiningSeat diningSeat = ResolveDiningSeat(messStation);
			if (diningSeat != null)
			{
				diningSeat.Diner = null;
			}
		}
	}

	public class States : GameStateMachine<States, StatesInstance, EatChore>
	{
		public class EatOnFloorState : State
		{
			public ApproachSubState<IApproachable> moveto;

			public State eat;
		}

		public class EatAtMessStationState : State
		{
			public ApproachSubState<IApproachable> moveto;

			public State eat;
		}

		public class RehydrateSubState : State
		{
			public TargetParameter foodpackage;

			public ObjectParameter<AccessabilityManager> rehydrator;

			public ApproachSubState<DehydratedFoodPackage> approach;

			public State work;
		}

		public TargetParameter eater;

		public TargetParameter ediblesource;

		public TargetParameter ediblechunk;

		public TargetParameter messstation;

		public FloatParameter requestedfoodunits;

		public FloatParameter actualfoodunits;

		public TargetParameter locator;

		public State chooseaction;

		public RehydrateSubState rehydrate;

		public FetchSubState fetch;

		public State choosewheretoeat;

		public EatOnFloorState eatonfloorstate;

		public EatAtMessStationState eatatmessstation;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = chooseaction;
			Target(eater);
			root.Enter("SetMessStation", delegate(StatesInstance smi)
			{
				smi.UpdateMessStation();
			}).EventHandler(GameHashes.AssignablesChanged, delegate(StatesInstance smi)
			{
				smi.UpdateMessStation();
			}).Exit(delegate(StatesInstance smi)
			{
				smi.ClearMessStation();
			});
			chooseaction.EnterTransition(rehydrate, (StatesInstance smi) => ediblesource.Get(smi).HasTag(GameTags.Dehydrated)).EnterTransition(fetch, (StatesInstance smi) => true);
			rehydrate.Enter(delegate(StatesInstance smi)
			{
				DehydratedFoodPackage component = ediblesource.Get(smi).GetComponent<Pickupable>().storage.gameObject.GetComponent<DehydratedFoodPackage>();
				rehydrate.foodpackage.Set(component, smi);
				GameObject rehydrator = component.Rehydrator;
				rehydrate.rehydrator.Set((rehydrator != null) ? component.Rehydrator.GetComponent<AccessabilityManager>() : null, smi);
				AccessabilityManager accessabilityManager = rehydrate.rehydrator.Get(smi);
				if (accessabilityManager != null)
				{
					GameObject worker = eater.Get(smi);
					if (accessabilityManager.CanAccess(worker))
					{
						accessabilityManager.Reserve(eater.Get(smi));
					}
					else
					{
						smi.GoTo((BaseState)null);
					}
				}
				else
				{
					smi.GoTo((BaseState)null);
				}
			}).Exit(delegate(StatesInstance smi)
			{
				AccessabilityManager accessabilityManager = rehydrate.rehydrator.Get(smi);
				if (accessabilityManager != null)
				{
					accessabilityManager.Unreserve();
				}
			}).DefaultState(rehydrate.approach);
			rehydrate.approach.InitializeStates(eater, rehydrate.foodpackage, rehydrate.work, null, null, NavigationTactics.ReduceTravelDistance).OnTargetLost(ediblesource, null);
			rehydrate.work.ToggleWork("Rehydrate", delegate(StatesInstance smi)
			{
				WorkerBase workerBase = eater.Get<WorkerBase>(smi);
				DehydratedFoodPackage pkg = rehydrate.foodpackage.Get<DehydratedFoodPackage>(smi);
				workerBase.StartWork(new DehydratedFoodPackage.RehydrateStartWorkItem(pkg, delegate(GameObject result)
				{
					ediblechunk.Set(result, smi);
				}));
			}, delegate(StatesInstance smi)
			{
				AccessabilityManager accessabilityManager = rehydrate.rehydrator.Get(smi);
				return !(accessabilityManager == null) && accessabilityManager.CanAccess(eater.Get<WorkerBase>(smi).gameObject);
			}, eatatmessstation, null);
			fetch.InitializeStates(eater, ediblesource, ediblechunk, requestedfoodunits, actualfoodunits, choosewheretoeat);
			choosewheretoeat.ParamTransition(messstation, eatonfloorstate, (StatesInstance smi, GameObject p) => p == null || IsMessStationNonOperational(p)).GoTo(eatatmessstation);
			eatatmessstation.DefaultState(eatatmessstation.moveto).ParamTransition(messstation, null, (StatesInstance smi, GameObject p) => p == null || IsMessStationNonOperational(p));
			eatatmessstation.moveto.InitializeStates(eater, messstation, eatatmessstation.eat);
			eatatmessstation.eat.Enter("OnEnterMessStation", delegate(StatesInstance smi)
			{
				smi.eatAnim = StatesInstance.OnEnterMessStation(messstation.Get(smi), eater.Get(smi), ediblechunk.Get(smi), dinerIsBionic: false);
			}).Transition(eatonfloorstate, (StatesInstance smi) => smi.eatAnim == null).DoEat(ediblechunk, actualfoodunits, null, null)
				.Exit(delegate(StatesInstance smi)
				{
					StatesInstance.OnExitMessStation(messstation.Get(smi), eater.Get(smi), smi.eatAnim);
				});
			eatonfloorstate.DefaultState(eatonfloorstate.moveto).Enter("CreateLocator", delegate(StatesInstance smi)
			{
				smi.CreateLocator();
			}).Exit("DestroyLocator", delegate(StatesInstance smi)
			{
				smi.DestroyLocator();
			});
			eatonfloorstate.moveto.InitializeStates(eater, locator, eatonfloorstate.eat, eatonfloorstate.eat);
			eatonfloorstate.eat.ToggleAnims((Func<StatesInstance, HashedString>)GetEatOnFloorAnim).DoEat(ediblechunk, actualfoodunits, null, null);
		}

		private HashedString GetEatOnFloorAnim(StatesInstance smi)
		{
			if (smi.GetComponent<Navigator>().CurrentNavType == NavType.Swim)
			{
				return "anim_eat_swim_kanim";
			}
			return "anim_eat_floor_kanim";
		}
	}

	public static readonly Precondition EdibleIsNotNull = new Precondition
	{
		id = "EdibleIsNotNull",
		description = DUPLICANTS.CHORES.PRECONDITIONS.EDIBLE_IS_NOT_NULL,
		fn = delegate(ref Precondition.Context context, object data)
		{
			return null != context.consumerState.consumer.GetSMI<RationMonitor.Instance>().GetEdible();
		}
	};

	public static IDiningSeat ResolveDiningSeat(GameObject messStation)
	{
		if (messStation == null)
		{
			Debug.LogWarning("messStation GameObject is null");
			return null;
		}
		if (!messStation.TryGetComponent<IDiningSeat>(out var component))
		{
			Debug.LogWarning("messStation GameObject has no IDiningSeat component");
			return null;
		}
		return component;
	}

	private static KAnimFile ResolveEatAnim(IDiningSeat diningSeat, bool dinerIsBionic)
	{
		HashedString hashedString = ((diningSeat == null) ? MessStation.eatAnim : (dinerIsBionic ? diningSeat.ReloadElectrobankAnim : diningSeat.EatAnim));
		KAnimFile anim = Assets.GetAnim(hashedString);
		if (anim == null)
		{
			Debug.LogError($"Animation asset [{hashedString}] does not exist");
			return null;
		}
		return anim;
	}

	private static KAnimFile ResolveEatAnim(GameObject messStation, bool dinerIsBionic)
	{
		return ResolveEatAnim(ResolveDiningSeat(messStation), dinerIsBionic);
	}

	public EatChore(IStateMachineTarget master)
		: base(Db.Get().ChoreTypes.Eat, master, master.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.personalNeeds, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.PersonalTime)
	{
		base.smi = new StatesInstance(this);
		showAvailabilityInHoverText = false;
		AddPrecondition(ChorePreconditions.instance.IsNotRedAlert);
		AddPrecondition(EdibleIsNotNull);
	}

	public override void Begin(Precondition.Context context)
	{
		if (context.consumerState.consumer == null)
		{
			Debug.LogError("EATCHORE null context.consumer");
			return;
		}
		RationMonitor.Instance sMI = context.consumerState.consumer.GetSMI<RationMonitor.Instance>();
		if (sMI == null)
		{
			Debug.LogError("EATCHORE null RationMonitor.Instance");
			return;
		}
		Edible edible = sMI.GetEdible();
		if (edible.gameObject == null)
		{
			Debug.LogError("EATCHORE null edible.gameObject");
			return;
		}
		if (base.smi == null)
		{
			Debug.LogError("EATCHORE null smi");
			return;
		}
		if (base.smi.sm == null)
		{
			Debug.LogError("EATCHORE null smi.sm");
			return;
		}
		if (base.smi.sm.ediblesource == null)
		{
			Debug.LogError("EATCHORE null smi.sm.ediblesource");
			return;
		}
		base.smi.sm.ediblesource.Set(edible.gameObject, base.smi);
		KCrashReporter.Assert(edible.FoodInfo.CaloriesPerUnit > 0f, edible.GetProperName() + " has invalid calories per unit. Will result in NaNs");
		AmountInstance amountInstance = Db.Get().Amounts.Calories.Lookup(gameObject);
		float num = (amountInstance.GetMax() - amountInstance.value) / edible.FoodInfo.CaloriesPerUnit;
		KCrashReporter.Assert(num > 0f, "EatChore is requesting an invalid amount of food");
		base.smi.sm.requestedfoodunits.Set(num, base.smi);
		base.smi.sm.eater.Set(context.consumerState.gameObject, base.smi);
		base.Begin(context);
	}

	public static bool IsMessStationNonOperational(GameObject messStation)
	{
		if (messStation == null)
		{
			return true;
		}
		IDiningSeat diningSeat = ResolveDiningSeat(messStation);
		if (diningSeat == null)
		{
			return true;
		}
		Operational operational = diningSeat.FindOperational();
		if (operational == null)
		{
			return true;
		}
		return !operational.IsOperational;
	}

	private static bool IsMessStationNonOperational(StatesInstance _, GameObject messStation)
	{
		return IsMessStationNonOperational(messStation);
	}
}
