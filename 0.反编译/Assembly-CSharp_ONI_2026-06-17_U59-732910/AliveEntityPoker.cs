using UnityEngine;

public class AliveEntityPoker : GameStateMachine<AliveEntityPoker, AliveEntityPoker.Instance, IStateMachineTarget, AliveEntityPoker.Def>
{
	public class Def : BaseDef
	{
		public string PokeAnimFileName;

		public string PokeAnim_Pre;

		public string PokeAnim_Loop;

		public string PokeAnim_Pst;

		public string statusItemSTR_goingToPoke;

		public string statusItemSTR_poking;
	}

	public class PokeStates : State
	{
		public State pre;

		public State loop;

		public State pst;
	}

	public new class Instance : GameInstance
	{
		public CellOffset[] VictimPokeOffsets;

		public GameObject CurrentVictim => base.sm.victim.Get(this);

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.UrgeToPoke);
			base.sm.poker.Set(base.smi.gameObject, base.smi);
		}
	}

	public static readonly Tag BehaviourTag = GameTags.Creatures.UrgeToPoke;

	public ApproachSubState<Pickupable> approach;

	public PokeStates poke;

	public State complete;

	public State failed;

	public TargetParameter poker;

	public TargetParameter victim;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.Never;
		default_state = approach;
		root.Enter(RefreshTarget).TagTransition(BehaviourTag, null, on_remove: true);
		approach.InitializeStates(poker, victim, (Instance smi) => smi.VictimPokeOffsets, poke, failed).ToggleMainStatusItem(GetGoingToPokeStatusItem);
		poke.ToggleAnims((Instance smi) => smi.def.PokeAnimFileName).OnTargetLost(victim, null).DefaultState(poke.pre)
			.ToggleMainStatusItem(GetPokingStatusItem);
		poke.pre.PlayAnim((Instance smi) => smi.def.PokeAnim_Pre).OnAnimQueueComplete(poke.loop);
		poke.loop.PlayAnim((Instance smi) => smi.def.PokeAnim_Loop).OnAnimQueueComplete(poke.pst);
		poke.pst.PlayAnim((Instance smi) => smi.def.PokeAnim_Pst).OnAnimQueueComplete(complete);
		complete.TriggerOnEnter(GameHashes.EntityPoked, (Instance smi) => smi.CurrentVictim).BehaviourComplete(BehaviourTag);
		failed.Target(poker).TriggerOnEnter(GameHashes.TargetLost).EnterGoTo(null);
	}

	public static StatusItem GetGoingToPokeStatusItem(Instance smi)
	{
		return GetStatusItem(smi, smi.def.statusItemSTR_goingToPoke);
	}

	public static StatusItem GetPokingStatusItem(Instance smi)
	{
		return GetStatusItem(smi, smi.def.statusItemSTR_poking);
	}

	private static StatusItem GetStatusItem(Instance smi, string address)
	{
		string text = Strings.Get(address + ".NAME");
		string tooltip = Strings.Get(address + ".TOOLTIP");
		return new StatusItem(smi.GetCurrentState().longName, text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString));
	}

	public static void ClearPreviousVictim(Instance smi)
	{
		smi.sm.victim.Set(null, smi);
	}

	public static void RefreshTarget(Instance smi)
	{
		PokeMonitor.Instance sMI = smi.GetSMI<PokeMonitor.Instance>();
		smi.sm.victim.Set(sMI.Target, smi);
		smi.VictimPokeOffsets = sMI.TargetOffsets;
	}
}
