using System;
using UnityEngine;

public class RecoverFromHeatChore : Chore<RecoverFromHeatChore.Instance>
{
	public class States : GameStateMachine<States, Instance, RecoverFromHeatChore>
	{
		public class CompleteStates : State
		{
			public State evaluate;

			public State fail;

			public State success;
		}

		public ApproachSubState<IApproachable> approach;

		public PreLoopPostState recover;

		public State remove_suit;

		public CompleteStates complete;

		public TargetParameter heatImmunityProvider;

		public TargetParameter entityRecovering;

		public TargetParameter locator;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = approach;
			Target(entityRecovering);
			root.Enter("CreateLocator", delegate(RecoverFromHeatChore.Instance smi)
			{
				smi.CreateLocator();
			}).Enter("UpdateImmunityProvider", delegate(RecoverFromHeatChore.Instance smi)
			{
				smi.UpdateImmunityProvider();
			}).Exit("DestroyLocator", delegate(RecoverFromHeatChore.Instance smi)
			{
				smi.DestroyLocator();
			})
				.Update("UpdateLocator", delegate(RecoverFromHeatChore.Instance smi, float dt)
				{
					smi.UpdateLocator();
				}, UpdateRate.SIM_200ms, load_balance: true)
				.Update("UpdateHeatImmunityProvider", delegate(RecoverFromHeatChore.Instance smi, float dt)
				{
					smi.UpdateImmunityProvider();
				}, UpdateRate.SIM_200ms, load_balance: true);
			approach.InitializeStates(entityRecovering, locator, recover);
			recover.OnTargetLost(heatImmunityProvider, null).Enter("AnimOverride", delegate(RecoverFromHeatChore.Instance smi)
			{
				smi.cachedAnimName = GetAnimFileName(smi);
				smi.GetComponent<KAnimControllerBase>().AddAnimOverrides(Assets.GetAnim(smi.cachedAnimName));
			}).Exit(delegate(RecoverFromHeatChore.Instance smi)
			{
				if (smi.cachedAnimName != HashedString.Invalid)
				{
					smi.GetComponent<KAnimControllerBase>().RemoveAnimOverrides(Assets.GetAnim(smi.cachedAnimName));
				}
			})
				.DefaultState(recover.pre)
				.ToggleTag(GameTags.RecoveringFromHeat);
			recover.pre.Face(heatImmunityProvider).PlayAnim((Func<RecoverFromHeatChore.Instance, string>)GetPreAnimName, KAnim.PlayMode.Once).OnAnimQueueComplete(recover.loop);
			recover.loop.PlayAnim((Func<RecoverFromHeatChore.Instance, string>)GetLoopAnimName, KAnim.PlayMode.Once).OnAnimQueueComplete(recover.pst);
			recover.pst.QueueAnim(GetPstAnimName).OnAnimQueueComplete(complete);
			complete.DefaultState(complete.evaluate);
			complete.evaluate.EnterTransition(complete.success, IsImmunityProviderStillValid).EnterTransition(complete.fail, GameStateMachine<States, RecoverFromHeatChore.Instance, RecoverFromHeatChore, object>.Not(IsImmunityProviderStillValid));
			complete.success.Enter(ApplyHeatImmunityEffect).ReturnSuccess();
			complete.fail.ReturnFailure();
		}

		public static bool IsImmunityProviderStillValid(RecoverFromHeatChore.Instance smi)
		{
			return smi.lastKnownImmunityProvider?.CanBeUsed ?? false;
		}

		public static void ApplyHeatImmunityEffect(RecoverFromHeatChore.Instance smi)
		{
			smi.lastKnownImmunityProvider?.ApplyImmunityEffect(smi.gameObject);
		}

		public static HashedString GetAnimFileName(RecoverFromHeatChore.Instance smi)
		{
			return GetAnimFromHeatImmunityProvider(smi, (HeatImmunityProvider.Instance p) => p.GetAnimFileName(smi.sm.entityRecovering.Get(smi)));
		}

		public static string GetPreAnimName(RecoverFromHeatChore.Instance smi)
		{
			return GetAnimFromHeatImmunityProvider(smi, (HeatImmunityProvider.Instance p) => p.PreAnimName);
		}

		public static string GetLoopAnimName(RecoverFromHeatChore.Instance smi)
		{
			return GetAnimFromHeatImmunityProvider(smi, (HeatImmunityProvider.Instance p) => p.LoopAnimName);
		}

		public static string GetPstAnimName(RecoverFromHeatChore.Instance smi)
		{
			return GetAnimFromHeatImmunityProvider(smi, (HeatImmunityProvider.Instance p) => p.PstAnimName);
		}

		public static string GetAnimFromHeatImmunityProvider(RecoverFromHeatChore.Instance smi, Func<HeatImmunityProvider.Instance, string> getCallback)
		{
			HeatImmunityProvider.Instance lastKnownImmunityProvider = smi.lastKnownImmunityProvider;
			if (lastKnownImmunityProvider != null)
			{
				return getCallback(lastKnownImmunityProvider);
			}
			return null;
		}
	}

	public class Instance : GameStateMachine<States, Instance, RecoverFromHeatChore, object>.GameInstance
	{
		private int targetCell;

		public HashedString cachedAnimName;

		public HeatImmunityProvider.Instance lastKnownImmunityProvider => (base.sm.heatImmunityProvider.Get(this) == null) ? null : base.sm.heatImmunityProvider.Get(this).GetSMI<HeatImmunityProvider.Instance>();

		public HeatImmunityMonitor.Instance heatImmunityMonitor => base.sm.entityRecovering.Get(this).GetSMI<HeatImmunityMonitor.Instance>();

		public Instance(RecoverFromHeatChore master, GameObject entityRecovering)
			: base(master)
		{
			base.sm.entityRecovering.Set(entityRecovering, this);
			HeatImmunityMonitor.Instance instance = heatImmunityMonitor;
			if (instance.NearestImmunityProvider != null && !instance.NearestImmunityProvider.isMasterNull)
			{
				base.sm.heatImmunityProvider.Set(instance.NearestImmunityProvider.gameObject, this);
			}
		}

		public void CreateLocator()
		{
			GameObject value = ChoreHelpers.CreateLocator("RecoverWarmthLocator", Vector3.zero);
			base.sm.locator.Set(value, this);
			UpdateLocator();
		}

		public void UpdateImmunityProvider()
		{
			HeatImmunityProvider.Instance nearestImmunityProvider = heatImmunityMonitor.NearestImmunityProvider;
			base.sm.heatImmunityProvider.Set((nearestImmunityProvider == null || nearestImmunityProvider.isMasterNull) ? null : nearestImmunityProvider.gameObject, this);
		}

		public void UpdateLocator()
		{
			int num = heatImmunityMonitor.ShelterCell;
			if (num == Grid.InvalidCell)
			{
				num = Grid.PosToCell(base.sm.entityRecovering.Get<Transform>(base.smi).GetPosition());
				DestroyLocator();
			}
			else
			{
				Vector3 position = Grid.CellToPosCBC(num, Grid.SceneLayer.Move);
				base.sm.locator.Get<Transform>(base.smi).SetPosition(position);
			}
			targetCell = num;
		}

		public void DestroyLocator()
		{
			ChoreHelpers.DestroyLocator(base.sm.locator.Get(this));
			base.sm.locator.Set(null, this);
		}
	}

	public RecoverFromHeatChore(IStateMachineTarget target)
		: base(Db.Get().ChoreTypes.RecoverFromHeat, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.personalNeeds, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new Instance(this, target.gameObject);
		HeatImmunityMonitor.Instance chillyBones = target.gameObject.GetSMI<HeatImmunityMonitor.Instance>();
		Func<int> data = () => chillyBones.ShelterCell;
		AddPrecondition(ChorePreconditions.instance.CanMoveToDynamicCell, data);
		AddPrecondition(ChorePreconditions.instance.IsNotRedAlert);
	}
}
