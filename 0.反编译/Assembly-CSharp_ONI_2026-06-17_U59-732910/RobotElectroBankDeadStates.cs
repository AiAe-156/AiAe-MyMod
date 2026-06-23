using UnityEngine;

public class RobotElectroBankDeadStates : GameStateMachine<RobotElectroBankDeadStates, RobotElectroBankDeadStates.Instance, IStateMachineTarget, RobotElectroBankDeadStates.Def>
{
	public class Def : BaseDef
	{
	}

	public class PowerDown : State
	{
		public State pre;

		public State fall;

		public State landed;

		public State dead;
	}

	public class PowerUp : State
	{
		public State grounded;

		public State takeoff;
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.choreType.interruptPriority = Db.Get().ChoreTypes.Die.interruptPriority;
			chore.masterPriority.priority_class = PriorityScreen.PriorityClass.compulsory;
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Robots.Behaviours.NoElectroBank);
		}
	}

	public PowerDown powerdown;

	public PowerUp powerup;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = powerdown;
		powerdown.DefaultState(powerdown.pre).ToggleStatusItem(Db.Get().RobotStatusItems.DeadBatteryFlydo, (Instance smi) => smi.gameObject, Db.Get().StatusItemCategories.Main).EventTransition(GameHashes.OnStorageChange, powerup.grounded, (Instance smi) => ElectrobankDelivered(smi))
			.Exit(delegate(Instance smi)
			{
				if (GameComps.Fallers.Has(smi.gameObject))
				{
					GameComps.Fallers.Remove(smi.gameObject);
				}
			});
		powerdown.pre.PlayAnim("power_down_pre").OnAnimQueueComplete(powerdown.fall);
		powerdown.fall.PlayAnim("power_down_loop", KAnim.PlayMode.Loop).Enter(delegate(Instance smi)
		{
			if (!GameComps.Fallers.Has(smi.gameObject))
			{
				GameComps.Fallers.Add(smi.gameObject, Vector2.zero);
			}
		}).Update(delegate(Instance smi, float dt)
		{
			if (!GameComps.Gravities.Has(smi.gameObject))
			{
				smi.GoTo(powerdown.landed);
			}
		})
			.EventTransition(GameHashes.Landed, powerdown.landed);
		powerdown.landed.PlayAnim("power_down_pst").Enter(delegate(Instance smi)
		{
			smi.GetComponent<LoopingSounds>().PauseSound(GlobalAssets.GetSound("Flydo_flying_LP"), paused: true);
		}).OnAnimQueueComplete(powerdown.dead);
		powerdown.dead.PlayAnim("dead_battery").EventTransition(GameHashes.OnStorageChange, powerup.grounded, (Instance smi) => ElectrobankDelivered(smi));
		powerup.Exit(delegate(Instance smi)
		{
			smi.GetComponent<LoopingSounds>().PauseSound(GlobalAssets.GetSound("Flydo_flying_LP"), paused: false);
			smi.Get<Brain>().Resume("power up");
		});
		powerup.grounded.PlayAnim("battery_change_dead").OnAnimQueueComplete(powerup.takeoff);
		powerup.takeoff.PlayAnim("power_up").OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.BehaviourComplete(GameTags.Robots.Behaviours.NoElectroBank);
	}

	private static bool ElectrobankDelivered(Instance smi)
	{
		Storage[] components = smi.gameObject.GetComponents<Storage>();
		foreach (Storage storage in components)
		{
			if (storage.storageID == GameTags.ChargedPortableBattery)
			{
				return storage.Has(GameTags.ChargedPortableBattery);
			}
		}
		return false;
	}
}
