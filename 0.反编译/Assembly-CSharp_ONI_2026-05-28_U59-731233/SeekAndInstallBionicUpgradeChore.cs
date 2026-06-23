using System;
using STRINGS;
using UnityEngine;

public class SeekAndInstallBionicUpgradeChore : Chore<SeekAndInstallBionicUpgradeChore.Instance>
{
	public class States : GameStateMachine<States, Instance, SeekAndInstallBionicUpgradeChore>
	{
		public FetchSubState fetch;

		public State install;

		public State complete;

		public TargetParameter dupe;

		public TargetParameter initialUpgradeComponent;

		public TargetParameter pickedUpgrade;

		public FloatParameter actualunits;

		public FloatParameter amountRequested = new FloatParameter(1f);

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = fetch;
			Target(dupe);
			fetch.InitializeStates(dupe, initialUpgradeComponent, pickedUpgrade, amountRequested, actualunits, install).Target(initialUpgradeComponent).EventHandlerTransition(GameHashes.AssigneeChanged, null, (SeekAndInstallBionicUpgradeChore.Instance smi, object obj) => !IsBionicUpgradeAssignedTo(smi.sm.initialUpgradeComponent.Get(smi), smi.gameObject));
			install.Target(dupe).ToggleAnims("anim_bionic_booster_installation_kanim").PlayAnim("installation", KAnim.PlayMode.Once)
				.Enter(delegate(SeekAndInstallBionicUpgradeChore.Instance smi)
				{
					SetOverrideAnimSymbol(smi, overriding: true);
				})
				.Exit(delegate(SeekAndInstallBionicUpgradeChore.Instance smi)
				{
					SetOverrideAnimSymbol(smi, overriding: false);
				})
				.OnAnimQueueComplete(complete)
				.ScheduleGoTo(10f, complete)
				.Target(pickedUpgrade)
				.EventHandlerTransition(GameHashes.AssigneeChanged, null, (SeekAndInstallBionicUpgradeChore.Instance smi, object obj) => !IsBionicUpgradeAssignedTo(smi.sm.pickedUpgrade.Get(smi), smi.gameObject));
			complete.Target(dupe).Enter(InstallUpgrade).ReturnSuccess();
		}
	}

	public class Instance : GameStateMachine<States, Instance, SeekAndInstallBionicUpgradeChore, object>.GameInstance
	{
		public BionicUpgradesMonitor.Instance upgradeMonitor => base.sm.dupe.Get(this).GetSMI<BionicUpgradesMonitor.Instance>();

		public Instance(SeekAndInstallBionicUpgradeChore master, GameObject duplicant)
			: base(master)
		{
		}
	}

	private Precondition CanPickupAnyAssignedUpgrade = new Precondition
	{
		id = "CanPickupAnyAssignedUpgrade",
		description = DUPLICANTS.CHORES.PRECONDITIONS.CANPICKUPANYASSIGNEDUPGRADE,
		fn = delegate(ref Precondition.Context context, object data)
		{
			BionicUpgradesMonitor.Instance instance = (BionicUpgradesMonitor.Instance)data;
			BionicUpgradesMonitor.UpgradeComponentSlot anyReachableAssignedSlot = instance.GetAnyReachableAssignedSlot();
			return anyReachableAssignedSlot != null;
		},
		canExecuteOnAnyThread = false
	};

	public SeekAndInstallBionicUpgradeChore(IStateMachineTarget target)
		: base(Db.Get().ChoreTypes.SeekAndInstallUpgrade, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.personalNeeds, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new Instance(this, target.gameObject);
		BionicUpgradesMonitor.Instance sMI = target.gameObject.GetSMI<BionicUpgradesMonitor.Instance>();
		AddPrecondition(ChorePreconditions.instance.IsNotRedAlert);
		AddPrecondition(CanPickupAnyAssignedUpgrade, sMI);
	}

	public override void Begin(Precondition.Context context)
	{
		if (context.consumerState.consumer == null)
		{
			Debug.LogError("SeekAndInstallBionicUpgradeChore null context.consumer");
			return;
		}
		BionicUpgradesMonitor.Instance sMI = context.consumerState.consumer.GetSMI<BionicUpgradesMonitor.Instance>();
		if (sMI == null)
		{
			Debug.LogError("SeekAndInstallBionicUpgradeChore null BionicUpgradesMonitor.Instance");
			return;
		}
		BionicUpgradeComponent bionicUpgradeComponent = sMI.GetAnyReachableAssignedSlot()?.assignedUpgradeComponent;
		if (bionicUpgradeComponent == null)
		{
			Debug.LogError("SeekAndInstallBionicUpgradeChore null upgradeComponent.gameObject");
			return;
		}
		base.smi.sm.initialUpgradeComponent.Set(bionicUpgradeComponent.gameObject, base.smi);
		base.smi.sm.dupe.Set(context.consumerState.consumer, base.smi);
		base.Begin(context);
	}

	public static void SetOverrideAnimSymbol(Instance smi, bool overriding)
	{
		string text = "booster";
		KBatchedAnimController component = smi.GetComponent<KBatchedAnimController>();
		SymbolOverrideController component2 = smi.gameObject.GetComponent<SymbolOverrideController>();
		GameObject gameObject = smi.sm.pickedUpgrade.Get(smi);
		if (gameObject != null)
		{
			KBatchedAnimTracker component3 = gameObject.GetComponent<KBatchedAnimTracker>();
			component3.enabled = !overriding;
			Storage.MakeItemInvisible(gameObject, overriding, is_initializing: false);
		}
		if (!overriding)
		{
			component2.RemoveSymbolOverride(text);
			component.SetSymbolVisiblity(text, is_visible: false);
			return;
		}
		string animStateName = BionicUpgradeComponentConfig.UpgradesData[gameObject.PrefabID()].animStateName;
		KBatchedAnimController component4 = gameObject.GetComponent<KBatchedAnimController>();
		KAnim.Build.Symbol symbol = component4.AnimFiles[0].GetData().build.GetSymbol(animStateName);
		component2.AddSymbolOverride(text, symbol);
		component.SetSymbolVisiblity(text, is_visible: true);
	}

	public static bool IsBionicUpgradeAssignedTo(GameObject bionicUpgradeGameObject, GameObject ownerInQuestion)
	{
		if (bionicUpgradeGameObject == null)
		{
			return false;
		}
		BionicUpgradeComponent component = bionicUpgradeGameObject.GetComponent<BionicUpgradeComponent>();
		IAssignableIdentity component2 = ownerInQuestion.GetComponent<IAssignableIdentity>();
		return component.IsAssignedTo(component2);
	}

	public static void InstallUpgrade(Instance smi)
	{
		Storage storage = smi.gameObject.GetComponents<Storage>().FindFirst((Storage s) => s.storageID == GameTags.StoragesIds.DefaultStorage);
		GameObject gameObject = storage.FindFirst(GameTags.BionicUpgrade);
		if (gameObject != null)
		{
			BionicUpgradeComponent component = gameObject.GetComponent<BionicUpgradeComponent>();
			storage.Remove(component.gameObject);
			smi.upgradeMonitor.InstallUpgrade(component);
			if (PopFXManager.Instance != null)
			{
				PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, component.GetProperName(), smi.gameObject.transform, Vector3.up, 1.5f, track_target: true);
			}
		}
	}
}
