using System;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class UseSolidLubricantChore : Chore<UseSolidLubricantChore.Instance>
{
	public class States : GameStateMachine<States, Instance, UseSolidLubricantChore>
	{
		public class InstallState : State
		{
			public State pre;

			public State loop;

			public State pst;
		}

		public FetchSubState fetch;

		public InstallState consume;

		public State complete;

		public State lubricantLost;

		public TargetParameter dupe;

		public TargetParameter solidLubricantSource;

		public TargetParameter pickedUpSolidLubricant;

		public TargetParameter messstation;

		public FloatParameter actualunits;

		public FloatParameter amountRequested = new FloatParameter(LubricationStickConfig.MASS_PER_RECIPE);

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = fetch;
			Target(dupe);
			fetch.InitializeStates(dupe, solidLubricantSource, pickedUpSolidLubricant, amountRequested, actualunits, consume).OnTargetLost(solidLubricantSource, lubricantLost);
			consume.DefaultState(consume.pre).ToggleAnims("anim_bionic_kanim").Enter("Add Symbol Override", delegate(UseSolidLubricantChore.Instance smi)
			{
				SetOverrideAnimSymbol(smi, overriding: true);
			})
				.Exit("Revert Symbol Override", delegate(UseSolidLubricantChore.Instance smi)
				{
					SetOverrideAnimSymbol(smi, overriding: false);
				});
			consume.pre.PlayAnim("lubricate_pre", KAnim.PlayMode.Once).OnAnimQueueComplete(consume.loop).ScheduleGoTo(4.7f, consume.loop);
			consume.loop.PlayAnim("lubricate_loop", KAnim.PlayMode.Loop).ScheduleGoTo(6.666f, consume.pst);
			consume.pst.PlayAnim("lubricate_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(complete).ScheduleGoTo(3.5f, complete);
			complete.Enter(ConsumeLubricant).ReturnSuccess();
			lubricantLost.Target(dupe).ReturnFailure();
		}
	}

	public class Instance : GameStateMachine<States, Instance, UseSolidLubricantChore, object>.GameInstance
	{
		public BionicOilMonitor.Instance oilMonitor => base.sm.dupe.Get(this).GetSMI<BionicOilMonitor.Instance>();

		public Instance(UseSolidLubricantChore master, GameObject duplicant)
			: base(master)
		{
		}
	}

	public const float LOOP_LENGTH = 6.666f;

	public static readonly Precondition SolidLubricantIsNotNull = new Precondition
	{
		id = "SolidLubricantIsNotNull ",
		description = DUPLICANTS.CHORES.PRECONDITIONS.EDIBLE_IS_NOT_NULL,
		fn = delegate(ref Precondition.Context context, object data)
		{
			return null != context.consumerState.consumer.GetSMI<BionicOilMonitor.Instance>().GetClosestSolidLubricant();
		}
	};

	public UseSolidLubricantChore(IStateMachineTarget target)
		: base(Db.Get().ChoreTypes.SolidOilChange, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.personalNeeds, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new Instance(this, target.gameObject);
		AddPrecondition(ChorePreconditions.instance.IsNotRedAlert);
		AddPrecondition(SolidLubricantIsNotNull);
	}

	public override void Begin(Precondition.Context context)
	{
		if (context.consumerState.consumer == null)
		{
			Debug.LogError("ReloadElectrobankChore null context.consumer");
			return;
		}
		BionicOilMonitor.Instance sMI = context.consumerState.consumer.GetSMI<BionicOilMonitor.Instance>();
		if (sMI == null)
		{
			Debug.LogError("ReloadElectrobankChore null RationMonitor.Instance");
			return;
		}
		Pickupable closestSolidLubricant = sMI.GetClosestSolidLubricant();
		if (closestSolidLubricant == null)
		{
			Debug.LogError("ReloadElectrobankChore null electrobank.gameObject");
			return;
		}
		base.smi.sm.solidLubricantSource.Set(closestSolidLubricant.gameObject, base.smi);
		base.smi.sm.dupe.Set(context.consumerState.consumer, base.smi);
		base.Begin(context);
	}

	public static void ConsumeLubricant(Instance smi)
	{
		PrimaryElement component = smi.sm.pickedUpSolidLubricant.Get(smi).GetComponent<PrimaryElement>();
		float num = Mathf.Min(component.Mass, 200f - smi.oilMonitor.oilAmount.value);
		smi.oilMonitor.RefillOil(num);
		if (num >= component.Mass)
		{
			Util.KDestroyGameObject(component.gameObject);
			smi.sm.pickedUpSolidLubricant.Set(null, smi);
		}
		else
		{
			component.Mass -= num;
		}
		BionicOilMonitor.ApplyLubricationEffects(smi.master.GetComponent<Effects>(), component.GetComponent<PrimaryElement>().ElementID);
	}

	public static void SetOverrideAnimSymbol(Instance smi, bool overriding)
	{
		string text = "lubricant";
		KBatchedAnimController component = smi.GetComponent<KBatchedAnimController>();
		SymbolOverrideController component2 = smi.gameObject.GetComponent<SymbolOverrideController>();
		GameObject gameObject = smi.sm.pickedUpSolidLubricant.Get(smi);
		if (gameObject != null)
		{
			KBatchedAnimTracker component3 = gameObject.GetComponent<KBatchedAnimTracker>();
			if (component3 != null)
			{
				component3.enabled = !overriding;
			}
			Storage.MakeItemInvisible(gameObject, overriding, is_initializing: false);
		}
		if (!overriding)
		{
			component2.RemoveSymbolOverride(text);
			component.SetSymbolVisiblity(text, is_visible: false);
			return;
		}
		KBatchedAnimController component4 = gameObject.GetComponent<KBatchedAnimController>();
		KAnim.Build.Symbol symbolByIndex = component4.AnimFiles[0].GetData().build.GetSymbolByIndex(0u);
		component2.AddSymbolOverride(text, symbolByIndex);
		component.SetSymbolVisiblity(text, is_visible: true);
	}
}
