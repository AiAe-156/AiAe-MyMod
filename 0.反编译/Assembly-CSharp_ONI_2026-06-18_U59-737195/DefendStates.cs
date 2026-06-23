using System;
using STRINGS;
using UnityEngine;

public class DefendStates : GameStateMachine<DefendStates, DefendStates.Instance, IStateMachineTarget, DefendStates.Def>
{
	public class Def : BaseDef
	{
		public string preAnim = "slap_pre";

		public string attackAnim = "slap";

		public string pstAnim = "slap_pst";

		public Action<GameObject, GameObject> specialAttackAction;
	}

	public new class Instance : GameInstance
	{
		[MyCmpGet]
		public KBatchedAnimController animcontroller;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Defend);
		}
	}

	public class ProtectStates : State
	{
		public ApproachSubState<AttackableBase> moveToThreat;

		public PreLoopPostState attackThreat;
	}

	public TargetParameter target;

	public ProtectStates protectEntity;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = protectEntity.moveToThreat;
		State state = root.Enter("SetTarget", delegate(Instance smi)
		{
			target.Set(smi.GetSMI<ThreatMonitor.Instance>().MainThreat, smi);
		});
		string text = CREATURES.STATUSITEMS.ATTACKINGENTITY.NAME;
		string tooltip = CREATURES.STATUSITEMS.ATTACKINGENTITY.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		protectEntity.moveToThreat.InitializeStates(masterTarget, target, protectEntity.attackThreat, null, CrabTuning.DEFEND_OFFSETS);
		protectEntity.attackThreat.OnTargetLost(target, behaviourcomplete).DefaultState(protectEntity.attackThreat.pre).Face(target);
		protectEntity.attackThreat.pre.PlayAnim((Instance smi) => smi.def.preAnim).OnAnimQueueComplete(protectEntity.attackThreat.loop);
		protectEntity.attackThreat.loop.PlayAnim((Instance smi) => smi.def.attackAnim).OnAnimQueueComplete(protectEntity.attackThreat.pst);
		protectEntity.attackThreat.pst.Enter(UseWeapon).PlayAnim((Instance smi) => smi.def.pstAnim).OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.BehaviourComplete(GameTags.Creatures.Defend);
	}

	private static void UseWeapon(Instance smi)
	{
		smi.GetComponent<Weapon>().AttackTarget(smi.sm.target.Get(smi));
		smi.def.specialAttackAction?.Invoke(smi.gameObject, smi.sm.target.Get(smi));
	}
}
