using System;
using STRINGS;
using UnityEngine;

public class ReloadElectrobankChore : Chore<ReloadElectrobankChore.Instance>
{
	public class States : GameStateMachine<States, Instance, ReloadElectrobankChore>
	{
		public class RemoveDepletedBatteries : State
		{
			public State animate;

			public State end;

			public RemoveDepletedBatteries InitializeStates(State nextState)
			{
				DefaultState(animate).EnterTransition(nextState, (ReloadElectrobankChore.Instance smi) => !HasAnyDepletedBattery(smi));
				animate.ToggleAnims("anim_bionic_kanim").PlayAnim("discharge", KAnim.PlayMode.Once).Enter("Add Symbol Override", delegate(ReloadElectrobankChore.Instance smi)
				{
					smi.ShowElectrobankSymbol(show: true, smi.sm.depletedElectrobankSymbol);
				})
					.Exit("Revert Symbol Override", delegate(ReloadElectrobankChore.Instance smi)
					{
						smi.ShowElectrobankSymbol(show: false, smi.sm.depletedElectrobankSymbol);
					})
					.OnAnimQueueComplete(end);
				end.Enter(RemoveDepletedElectrobank).EnterTransition(animate, HasAnyDepletedBattery).GoTo(nextState);
				return this;
			}
		}

		public struct WorkerSnapshot
		{
			public bool hasHat;

			public bool hasSalt;
		}

		public interface IInstallBatteryAnim
		{
			public enum Anim
			{
				Pre,
				Idle,
				Convo,
				Pst
			}

			HashedString GetBank(ReloadElectrobankChore.Instance smi);

			string GetPrefix(ReloadElectrobankChore.Instance smi, Anim anim);

			bool ForceFacing();
		}

		public class DefaultInstallBatteryAnim : IInstallBatteryAnim
		{
			private static readonly HashedString bank = "anim_bionic_kanim";

			public HashedString GetBank(ReloadElectrobankChore.Instance _)
			{
				return bank;
			}

			public string GetPrefix(ReloadElectrobankChore.Instance _smi, IInstallBatteryAnim.Anim _anim)
			{
				return "consume";
			}

			public bool ForceFacing()
			{
				return false;
			}
		}

		public class MessStationInstallBatteryAnim : IInstallBatteryAnim
		{
			public HashedString GetBank(ReloadElectrobankChore.Instance smi)
			{
				return EatChore.ResolveDiningSeat(smi.sm.messstation.Get(smi))?.ReloadElectrobankAnim ?? MessStation.reloadElectrobankAnim;
			}

			public string GetPrefix(ReloadElectrobankChore.Instance smi, IInstallBatteryAnim.Anim anim)
			{
				bool hasHat = smi.workerSnapshot.hasHat;
				bool hasSalt = smi.workerSnapshot.hasSalt;
				if (hasSalt && hasHat)
				{
					return "salt_hat";
				}
				if (hasSalt)
				{
					return "salt";
				}
				if (hasHat)
				{
					return (anim != IInstallBatteryAnim.Anim.Idle) ? "hat" : "working";
				}
				return "working";
			}

			public bool ForceFacing()
			{
				return true;
			}
		}

		public class InstallBattery : State
		{
			public State pre;

			public State idle;

			public State idleOrConvo;

			public State convo;

			public State pst;

			private const float ANIMATION_TIMEOUT = 15f;

			private const float DINING_DURATION_MAXIMUM = 15f;

			private static WorkerSnapshot Snapshot(ReloadElectrobankChore.Instance smi)
			{
				bool hasHat = smi.Resume != null && smi.Resume.CurrentHat != null;
				GameObject messStation = smi.sm.messstation.Get(smi);
				bool hasSalt = EatChore.StatesInstance.UseGarnish(messStation);
				return new WorkerSnapshot
				{
					hasHat = hasHat,
					hasSalt = hasSalt
				};
			}

			public InstallBattery InitializeStates(State nextState, IInstallBatteryAnim anim)
			{
				DefaultState(pre).Enter("Install Battery", delegate(ReloadElectrobankChore.Instance smi)
				{
					KAnimFile anim2 = Assets.GetAnim(anim.GetBank(smi));
					smi.AnimController.AddAnims(anim2);
					smi.AnimController.AddAnimOverrides(anim2);
					smi.StowElectrobank(stow: false);
					if (anim.ForceFacing() && smi.Facing != null)
					{
						smi.Facing.SetFacing(mirror_x: false);
					}
					smi.workerSnapshot = Snapshot(smi);
					smi.diningTimedOut = false;
				}).ScheduleAction("Dining Timeout", 15f, delegate(ReloadElectrobankChore.Instance smi)
				{
					smi.diningTimedOut = true;
				}).Exit("Exit Install Battery", delegate(ReloadElectrobankChore.Instance smi)
				{
					smi.StowElectrobank(stow: true);
					KAnimFile anim2 = Assets.GetAnim(anim.GetBank(smi));
					smi.AnimController.RemoveAnimOverrides(anim2);
					smi.workerSnapshot = default(WorkerSnapshot);
					smi.Kpid.RemoveTag(GameTags.DoNotInterruptMe);
				});
				pre.PlayAnim((ReloadElectrobankChore.Instance smi) => anim.GetPrefix(smi, IInstallBatteryAnim.Anim.Pre) + "_pre").ToggleTag(GameTags.SuppressConversation).OnAnimQueueComplete(idle)
					.ScheduleGoTo(15f, idle);
				idle.PlayAnim((ReloadElectrobankChore.Instance smi) => anim.GetPrefix(smi, IInstallBatteryAnim.Anim.Idle) + "_loop").OnAnimQueueComplete(idleOrConvo).ScheduleGoTo(15f, idleOrConvo);
				idleOrConvo.Enter("IdleOrConvo", delegate(ReloadElectrobankChore.Instance smi)
				{
					if (smi.Kpid.HasTag(GameTags.CommunalDining) && !smi.diningTimedOut)
					{
						if (smi.Kpid.HasTag(GameTags.WantsToTalk))
						{
							smi.GoTo(convo);
						}
						else
						{
							smi.GoTo(idle);
						}
					}
					else
					{
						smi.GoTo(pst);
					}
				});
				convo.Enter("Convo", delegate(ReloadElectrobankChore.Instance smi)
				{
					smi.Kpid.RemoveTag(GameTags.WantsToTalk);
					smi.AnimController.SetSymbolVisiblity(Edible.SALT_SYMBOL, smi.workerSnapshot.hasSalt);
					smi.AnimController.SetSymbolVisiblity(Edible.HAT_SYMBOL, smi.workerSnapshot.hasHat);
				}).PlayAnim((ReloadElectrobankChore.Instance _) => Edible.convoAnims[UnityEngine.Random.Range(0, Edible.convoAnims.Length)]).OnAnimQueueComplete(idleOrConvo)
					.ScheduleGoTo(15f, idleOrConvo)
					.Exit("Exit Convo", delegate(ReloadElectrobankChore.Instance smi)
					{
						smi.Kpid.RemoveTag(GameTags.DoNotInterruptMe);
						smi.AnimController.SetSymbolVisiblity(Edible.SALT_SYMBOL, is_visible: true);
						smi.AnimController.SetSymbolVisiblity(Edible.HAT_SYMBOL, is_visible: true);
					});
				pst.PlayAnim((ReloadElectrobankChore.Instance smi) => anim.GetPrefix(smi, IInstallBatteryAnim.Anim.Pst) + "_pst").OnAnimQueueComplete(nextState).ScheduleGoTo(15f, nextState);
				return this;
			}
		}

		public class InstallAtMessStation : State
		{
			public ApproachSubState<IApproachable> approach;

			public RemoveDepletedBatteries removeDepletedBatteries;

			public InstallBattery install;
		}

		public class InstallAtSafeLocation : State
		{
			public ApproachSubState<IApproachable> approach;

			public RemoveDepletedBatteries removeDepletedBatteries;

			public InstallBattery install;
		}

		public FetchSubState fetch;

		public InstallAtMessStation installAtMessStation;

		public InstallAtSafeLocation installAtSafeLocation;

		public State complete;

		public State electrobankLost;

		public TargetParameter dupe;

		public TargetParameter electrobankSource;

		public TargetParameter lastDepletedElectrobankFound;

		public TargetParameter pickedUpElectrobank;

		public TargetParameter messstation;

		public TargetParameter safeLocation;

		public FloatParameter actualunits;

		public FloatParameter amountRequested;

		public IntParameter safeCellIndex;

		public KAnim.Build.Symbol defaultElectrobankSymbol;

		public KAnim.Build.Symbol depletedElectrobankSymbol;

		private const float ROOM_EFFECT_DURATION = 1800f;

		private bool IsMessStationInvalid(GameObject messStation)
		{
			return EatChore.IsMessStationNonOperational(messStation);
		}

		public override void InitializeStates(out BaseState default_state)
		{
			defaultElectrobankSymbol = Assets.GetPrefab("Electrobank").GetComponent<KBatchedAnimController>().AnimFiles[0].GetData().build.GetSymbolByIndex(0u);
			depletedElectrobankSymbol = Assets.GetPrefab("EmptyElectrobank").GetComponent<KBatchedAnimController>().AnimFiles[0].GetData().build.GetSymbolByIndex(0u);
			default_state = fetch;
			Target(dupe);
			root.Enter("SetMessStation", delegate(ReloadElectrobankChore.Instance smi)
			{
				smi.UpdateMessStation();
			}).EventHandler(GameHashes.AssignablesChanged, delegate(ReloadElectrobankChore.Instance smi)
			{
				smi.UpdateMessStation();
			}).Exit(delegate(ReloadElectrobankChore.Instance smi)
			{
				smi.ClearMessStation();
			});
			fetch.InitializeStates(dupe, electrobankSource, pickedUpElectrobank, amountRequested, actualunits, installAtMessStation).OnTargetLost(electrobankSource, electrobankLost);
			installAtMessStation.EnterTransition(installAtSafeLocation, (ReloadElectrobankChore.Instance smi) => IsMessStationInvalid(messstation.Get(smi))).DefaultState(installAtMessStation.approach).ParamTransition(messstation, installAtSafeLocation, (ReloadElectrobankChore.Instance _, GameObject messStation) => IsMessStationInvalid(messStation));
			installAtMessStation.approach.InitializeStates(dupe, messstation, installAtMessStation.removeDepletedBatteries, installAtSafeLocation);
			installAtMessStation.removeDepletedBatteries.InitializeStates(installAtMessStation.install);
			installAtMessStation.install.InitializeStates(complete, new MessStationInstallBatteryAnim()).Enter(delegate(ReloadElectrobankChore.Instance smi)
			{
				GameObject gameObject = dupe.Get(smi);
				smi.eatAnim = EatChore.StatesInstance.OnEnterMessStation(messstation.Get(smi), gameObject, pickedUpElectrobank.Get(smi), dinerIsBionic: true, 1800f);
				SetZ(gameObject, Grid.GetLayerZ(Grid.SceneLayer.BuildingFront));
			}).Transition(installAtSafeLocation, (ReloadElectrobankChore.Instance smi) => smi.eatAnim == null)
				.Exit(delegate(ReloadElectrobankChore.Instance smi)
				{
					GameObject gameObject = dupe.Get(smi);
					EatChore.StatesInstance.OnExitMessStation(messstation.Get(smi), gameObject, smi.eatAnim);
					SetZ(gameObject, Grid.GetLayerZ(Grid.SceneLayer.Move));
				});
			installAtSafeLocation.Enter("CreateSafeLocation", delegate(ReloadElectrobankChore.Instance smi)
			{
				var (value, value2) = EatChore.StatesInstance.CreateLocator(dupe.Get<Sensors>(smi), dupe.Get<Transform>(smi), "ReloadElectrobankLocator");
				safeLocation.Set(value, smi);
				safeCellIndex.Set(value2, smi);
			}).Exit("DestroySafeLocation", delegate(ReloadElectrobankChore.Instance smi)
			{
				Grid.Reserved[safeCellIndex.Get(smi)] = false;
				ChoreHelpers.DestroyLocator(safeLocation.Get(smi));
				safeLocation.Set(null, smi);
			}).DefaultState(installAtSafeLocation.approach);
			installAtSafeLocation.approach.InitializeStates(dupe, safeLocation, installAtSafeLocation.removeDepletedBatteries, installAtSafeLocation.removeDepletedBatteries);
			installAtSafeLocation.removeDepletedBatteries.InitializeStates(installAtSafeLocation.install);
			installAtSafeLocation.install.InitializeStates(complete, new DefaultInstallBatteryAnim());
			complete.Enter(InstallElectrobank).ReturnSuccess();
			electrobankLost.Target(dupe).TriggerOnEnter(GameHashes.TargetElectrobankLost).ReturnFailure();
		}
	}

	public class Instance : GameStateMachine<States, Instance, ReloadElectrobankChore, object>.GameInstance
	{
		public States.WorkerSnapshot workerSnapshot = default(States.WorkerSnapshot);

		public bool diningTimedOut = false;

		public KAnimFile eatAnim;

		private static readonly HashedString SYMBOL_NAME = "object";

		public BionicBatteryMonitor.Instance batteryMonitor => base.sm.dupe.Get(this).GetSMI<BionicBatteryMonitor.Instance>();

		public KPrefabID Kpid { get; private set; }

		public KBatchedAnimController AnimController { get; private set; }

		public SymbolOverrideController SymbolOverrideController { get; private set; }

		public Facing Facing { get; private set; }

		public Storage[] Storages { get; private set; }

		public MinionResume Resume { get; private set; }

		public Instance(ReloadElectrobankChore master, GameObject duplicant)
			: base(master)
		{
			Kpid = master.GetComponent<KPrefabID>();
			AnimController = master.GetComponent<KBatchedAnimController>();
			SymbolOverrideController = master.GetComponent<SymbolOverrideController>();
			Facing = master.GetComponent<Facing>();
			Storages = master.gameObject.GetComponents<Storage>();
			Resume = master.GetComponent<MinionResume>();
		}

		public void UpdateMessStation()
		{
			Assignable value = EatChore.StatesInstance.ReserveMessStation(base.sm.messstation.Get(base.smi), base.sm.dupe.Get(base.smi));
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

		public void ShowElectrobankSymbol(bool show, KAnim.Build.Symbol symbol)
		{
			if (show)
			{
				SymbolOverrideController.AddSymbolOverride(SYMBOL_NAME, symbol);
			}
			else
			{
				SymbolOverrideController.RemoveSymbolOverride(SYMBOL_NAME);
			}
			AnimController.SetSymbolVisiblity(SYMBOL_NAME, show);
		}

		public void StowElectrobank(bool stow)
		{
			GameObject gameObject = base.sm.pickedUpElectrobank.Get(this);
			SetStoredItemVisibility(gameObject, stow);
			KAnim.Build.Symbol symbol = ((gameObject != null) ? gameObject.GetComponent<KBatchedAnimController>().AnimFiles[0].GetData().build.GetSymbolByIndex(0u) : base.sm.defaultElectrobankSymbol);
			ShowElectrobankSymbol(!stow, symbol);
		}
	}

	public static readonly Precondition ElectrobankIsNotNull = new Precondition
	{
		id = "ElectrobankIsNotNull",
		description = DUPLICANTS.CHORES.PRECONDITIONS.EDIBLE_IS_NOT_NULL,
		fn = delegate(ref Precondition.Context context, object data)
		{
			return null != context.consumerState.consumer.GetSMI<BionicBatteryMonitor.Instance>().GetClosestElectrobank();
		}
	};

	public ReloadElectrobankChore(IStateMachineTarget target)
		: base(Db.Get().ChoreTypes.ReloadElectrobank, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.personalNeeds, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new Instance(this, target.gameObject);
		AddPrecondition(ChorePreconditions.instance.IsNotRedAlert);
		AddPrecondition(ElectrobankIsNotNull);
	}

	public override void Begin(Precondition.Context context)
	{
		if (context.consumerState.consumer == null)
		{
			Debug.LogError("ReloadElectrobankChore null context.consumer");
			return;
		}
		BionicBatteryMonitor.Instance sMI = context.consumerState.consumer.GetSMI<BionicBatteryMonitor.Instance>();
		if (sMI == null)
		{
			Debug.LogError("ReloadElectrobankChore null BionicBatteryMonitor.Instance");
			return;
		}
		Electrobank closestElectrobank = sMI.GetClosestElectrobank();
		if (closestElectrobank == null)
		{
			Debug.LogError("ReloadElectrobankChore null electrobank.gameObject");
			return;
		}
		base.smi.sm.electrobankSource.Set(closestElectrobank.gameObject, base.smi);
		base.smi.sm.amountRequested.Set(closestElectrobank.GetComponent<PrimaryElement>().Mass, base.smi);
		base.smi.sm.dupe.Set(context.consumerState.consumer, base.smi);
		base.Begin(context);
	}

	private static void SetZ(GameObject go, float z)
	{
		Vector3 position = go.transform.GetPosition();
		position.z = z;
		go.transform.SetPosition(position);
	}

	public bool IsInstallingAtMessStation()
	{
		return base.smi.IsInsideState(base.smi.sm.installAtMessStation.install);
	}

	public static bool HasAnyDepletedBattery(Instance smi)
	{
		return GetAnyEmptyBattery(smi) != null;
	}

	public static GameObject GetAnyEmptyBattery(Instance smi)
	{
		return smi.batteryMonitor.storage.FindFirst(GameTags.EmptyPortableBattery);
	}

	public static void RemoveDepletedElectrobank(Instance smi)
	{
		GameObject anyEmptyBattery = GetAnyEmptyBattery(smi);
		if (anyEmptyBattery != null)
		{
			smi.batteryMonitor.storage.Drop(anyEmptyBattery);
		}
	}

	public static void InstallElectrobank(Instance smi)
	{
		Storage[] storages = smi.Storages;
		for (int i = 0; i < storages.Length; i++)
		{
			if (storages[i] != smi.batteryMonitor.storage)
			{
				GameObject gameObject = storages[i].FindFirst(GameTags.ChargedPortableBattery);
				if (gameObject != null)
				{
					storages[i].Transfer(smi.batteryMonitor.storage);
					break;
				}
			}
		}
		Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_BionicBattery);
	}

	private static void SetStoredItemVisibility(GameObject item, bool visible)
	{
		if (!(item == null))
		{
			if (item.TryGetComponent<KBatchedAnimTracker>(out var component))
			{
				component.enabled = visible;
			}
			Storage.MakeItemInvisible(item, !visible, is_initializing: false);
		}
	}
}
