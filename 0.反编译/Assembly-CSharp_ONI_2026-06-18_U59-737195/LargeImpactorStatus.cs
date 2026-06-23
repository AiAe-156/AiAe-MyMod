using System;
using UnityEngine;

public class LargeImpactorStatus : GameStateMachine<LargeImpactorStatus, LargeImpactorStatus.Instance, IStateMachineTarget, LargeImpactorStatus.Def>
{
	public class Def : BaseDef
	{
		public int MAX_HEALTH;

		public string EventID;
	}

	public new class Instance : GameInstance
	{
		public Action<int> OnDamaged;

		private ClusterTraveler clusterTraveler;

		private GameplayEventInstance eventInstance;

		public int Health => base.sm.Health.Get(this);

		public float ArrivalTime
		{
			get
			{
				if (!(clusterTraveler == null))
				{
					return ArrivalTime_SO;
				}
				return ArrivalTime_Vanilla;
			}
		}

		public float TimeRemainingBeforeCollision
		{
			get
			{
				if (!(clusterTraveler == null))
				{
					return TimeRemainingBeforeCollision_SO;
				}
				return TimeRemainingBeforeCollision_Vanilla;
			}
		}

		private float ArrivalTime_Vanilla => eventInstance.eventStartTime * 600f + LargeImpactorEvent.GetImpactTime();

		private float TimeRemainingBeforeCollision_Vanilla => Mathf.Clamp(ArrivalTime_Vanilla - GameUtil.GetCurrentTimeInCycles() * 600f, 0f, float.MaxValue);

		private float ArrivalTime_SO => GameUtil.GetCurrentTimeInCycles() * 600f + TimeRemainingBeforeCollision_SO;

		private float TimeRemainingBeforeCollision_SO => Mathf.Clamp(clusterTraveler.EstimatedTimeToReachDestination(), 0f, float.MaxValue);

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			base.sm.Health.Set(def.MAX_HEALTH, base.smi);
		}

		public override void StartSM()
		{
			clusterTraveler = GetComponent<ClusterTraveler>();
			eventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(base.def.EventID);
			base.StartSM();
		}

		public void DealDamage(int damage)
		{
			int value = Mathf.Clamp(Health - damage, 0, base.def.MAX_HEALTH);
			base.sm.Health.Set(value, this);
			OnDamaged?.Invoke(Health);
		}
	}

	public IntParameter Health;

	public BoolParameter HasArrived;

	public State alive;

	public State landing;

	public State destroyed;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = alive;
		alive.ParamTransition(HasArrived, landing, GameStateMachine<LargeImpactorStatus, Instance, IStateMachineTarget, Def>.IsTrue).ParamTransition(Health, destroyed, GameStateMachine<LargeImpactorStatus, Instance, IStateMachineTarget, Def>.IsZero_Int).EventHandler(GameHashes.MissileDamageEncountered, HandleIncommingDamage)
			.ToggleStatusItem(Db.Get().MiscStatusItems.ImpactorHealth)
			.EventTransition(GameHashes.ClusterDestinationReached, landing)
			.UpdateTransition(landing, CheckArrivalUpdate);
		landing.Enter(SetHasArrived).TriggerOnEnter(GameHashes.LargeImpactorArrived);
		destroyed.TriggerOnEnter(GameHashes.Died);
	}

	private static void HandleIncommingDamage(Instance smi, object obj)
	{
		DealDamage(smi, (obj as MissileLongRangeConfig.DamageEventPayload).damage);
	}

	private static void SetHasArrived(Instance smi)
	{
		smi.sm.HasArrived.Set(value: true, smi);
	}

	private static void DealDamage(Instance smi, int damage)
	{
		smi.DealDamage(damage);
	}

	private static void DeleteObject(Instance smi)
	{
		smi.gameObject.DeleteObject();
	}

	private static bool CheckArrivalUpdate(Instance smi, float dt)
	{
		return smi.TimeRemainingBeforeCollision <= 0f;
	}
}
