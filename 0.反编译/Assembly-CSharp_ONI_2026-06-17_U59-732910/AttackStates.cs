using STRINGS;
using UnityEngine;

public class AttackStates : GameStateMachine<AttackStates, AttackStates.Instance, IStateMachineTarget, AttackStates.Def>
{
	public class Def : BaseDef
	{
		public string preAnim;

		public string pstAnim;

		public CellOffset[] cellOffsets = new CellOffset[5]
		{
			new CellOffset(0, 0),
			new CellOffset(1, 0),
			new CellOffset(-1, 0),
			new CellOffset(1, 1),
			new CellOffset(-1, 1)
		};

		public Def(string pre_anim = "eat_pre", string pst_anim = "eat_pst", CellOffset[] cell_offsets = null)
		{
			preAnim = pre_anim;
			pstAnim = pst_anim;
			if (cell_offsets != null)
			{
				cellOffsets = cell_offsets;
			}
		}
	}

	public class AttackingStates : State
	{
		public State pre;

		public State pst;
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Attack);
		}
	}

	public TargetParameter target;

	public ApproachSubState<AttackableBase> approach;

	public CellOffset[] cellOffsets;

	public State waitBeforeAttack;

	public AttackingStates attack;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = waitBeforeAttack;
		root.Enter("SetTarget", delegate(Instance smi)
		{
			target.Set(smi.GetSMI<ThreatMonitor.Instance>().MainThreat, smi);
			cellOffsets = smi.def.cellOffsets;
		});
		waitBeforeAttack.ScheduleGoTo((Instance smi) => Random.Range(0f, 4f), approach);
		State state = approach.InitializeStates(masterTarget, target, attack, null, cellOffsets);
		string text = CREATURES.STATUSITEMS.ATTACK_APPROACH.NAME;
		string tooltip = CREATURES.STATUSITEMS.ATTACK_APPROACH.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		State state2 = attack.DefaultState(attack.pre);
		string text2 = CREATURES.STATUSITEMS.ATTACK.NAME;
		string tooltip2 = CREATURES.STATUSITEMS.ATTACK.TOOLTIP;
		main = Db.Get().StatusItemCategories.Main;
		state2.ToggleStatusItem(text2, tooltip2, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		attack.pre.PlayAnim((Instance smi) => smi.def.preAnim).Exit(delegate(Instance smi)
		{
			smi.GetComponent<Weapon>().AttackTarget(target.Get(smi));
		}).OnAnimQueueComplete(attack.pst);
		attack.pst.PlayAnim((Instance smi) => smi.def.pstAnim).OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.BehaviourComplete(GameTags.Creatures.Attack);
	}
}
