using System;

public class RationalAi : GameStateMachine<RationalAi, RationalAi.Instance>
{
	public new class Instance : GameInstance
	{
		public Tag MinionModel;

		public Func<Instance, StateMachine.Instance>[] stateMachinesToRunWhenAlive;

		public Instance(IStateMachineTarget master, Tag minionModel)
			: base(master)
		{
			MinionModel = minionModel;
			ChoreConsumer component = GetComponent<ChoreConsumer>();
			component.AddUrge(Db.Get().Urges.EmoteHighPriority);
			component.AddUrge(Db.Get().Urges.EmoteIdle);
			component.prioritizeBrainIfNoChore = true;
		}

		public void RefreshUserMenu()
		{
			Game.Instance.userMenu.Refresh(base.master.gameObject);
		}
	}

	public State alive;

	public State dead;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = root;
		root.ToggleStateMachine((Instance smi) => new DeathMonitor.Instance(smi.master, new DeathMonitor.Def())).Enter(delegate(Instance smi)
		{
			if (smi.HasTag(GameTags.Dead))
			{
				smi.GoTo(dead);
			}
			else
			{
				smi.GoTo(alive);
			}
		});
		alive.TagTransition(GameTags.Dead, dead).Exit(IncreaseDeathCounterIfDying).ToggleStateMachineList(GetStateMachinesToRunWhenAlive);
		dead.ToggleStateMachine((Instance smi) => new FallWhenDeadMonitor.Instance(smi.master)).ToggleBrain("dead").Enter("RefreshUserMenu", delegate(Instance smi)
		{
			smi.RefreshUserMenu();
		})
			.Enter("DropStorage", delegate(Instance smi)
			{
				smi.GetComponent<Storage>().DropAll();
			});
	}

	public static Func<Instance, StateMachine.Instance>[] GetStateMachinesToRunWhenAlive(Instance smi)
	{
		return smi.stateMachinesToRunWhenAlive;
	}

	private static void IncreaseDeathCounterIfDying(Instance smi)
	{
		if (smi.HasTag(GameTags.Dead))
		{
			SaveGame.Instance.ColonyAchievementTracker.deadDupeCounter++;
		}
	}
}
