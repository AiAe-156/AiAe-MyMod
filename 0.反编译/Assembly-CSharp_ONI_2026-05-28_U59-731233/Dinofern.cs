using STRINGS;
using UnityEngine;

public class Dinofern : StateMachineComponent<Dinofern.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, Dinofern, object>.GameInstance
	{
		public StatesInstance(Dinofern master)
			: base(master)
		{
			master.growing = GetComponent<Growing>();
		}
	}

	public class States : GameStateMachine<States, StatesInstance, Dinofern>
	{
		public class AliveStates : PlantAliveSubState
		{
			public State growing;

			public State mature;

			public State wilting;
		}

		public State grow;

		public State blocked_from_growing;

		public AliveStates alive;

		public State dead;

		public override void InitializeStates(out BaseState default_state)
		{
			base.serializable = SerializeType.Both_DEPRECATED;
			default_state = grow;
			State state = dead;
			string text = CREATURES.STATUSITEMS.DEAD.NAME;
			string tooltip = CREATURES.STATUSITEMS.DEAD.TOOLTIP;
			StatusItemCategory main = Db.Get().StatusItemCategories.Main;
			state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main).Enter(delegate(StatesInstance smi)
			{
				GameUtil.KInstantiate(Assets.GetPrefab(EffectConfigs.PlantDeathId), smi.master.transform.GetPosition(), Grid.SceneLayer.FXFront).SetActive(value: true);
				smi.master.Trigger(1623392196);
				smi.master.GetComponent<KBatchedAnimController>().StopAndClear();
				Object.Destroy(smi.master.GetComponent<KBatchedAnimController>());
				smi.Schedule(0.5f, smi.master.DestroySelf);
			});
			blocked_from_growing.ToggleStatusItem(Db.Get().MiscStatusItems.RegionIsBlocked).EventTransition(GameHashes.EntombedChanged, alive, (StatesInstance smi) => alive.ForceUpdateStatus(smi.master.gameObject)).EventTransition(GameHashes.TooColdWarning, alive, (StatesInstance smi) => alive.ForceUpdateStatus(smi.master.gameObject))
				.EventTransition(GameHashes.TooHotWarning, alive, (StatesInstance smi) => alive.ForceUpdateStatus(smi.master.gameObject))
				.TagTransition(GameTags.Uprooted, dead);
			grow.Enter(delegate(StatesInstance smi)
			{
				if (smi.master.receptacleMonitor.HasReceptacle() && !alive.ForceUpdateStatus(smi.master.gameObject))
				{
					smi.GoTo(blocked_from_growing);
				}
			}).EventTransition(GameHashes.AnimQueueComplete, alive);
			alive.InitializeStates(masterTarget, dead).DefaultState(alive.growing);
			alive.growing.Transition(alive.mature, (StatesInstance smi) => smi.master.growing.IsGrown()).EventTransition(GameHashes.Wilt, alive.wilting, (StatesInstance smi) => smi.master.wiltCondition.IsWilting()).Enter(delegate(StatesInstance smi)
			{
				smi.master.elementConsumer.EnableConsumption(enabled: true);
			})
				.Exit(delegate(StatesInstance smi)
				{
					smi.master.elementConsumer.EnableConsumption(enabled: false);
				});
			alive.mature.Transition(alive.growing, (StatesInstance smi) => !smi.master.growing.IsGrown()).EventTransition(GameHashes.Wilt, alive.wilting, (StatesInstance smi) => smi.master.wiltCondition.IsWilting());
			alive.wilting.EventTransition(GameHashes.WiltRecover, alive.growing, (StatesInstance smi) => !smi.master.wiltCondition.IsWilting());
		}
	}

	[MyCmpReq]
	private WiltCondition wiltCondition;

	[MyCmpReq]
	private ElementConsumer elementConsumer;

	[MyCmpReq]
	private ReceptacleMonitor receptacleMonitor;

	private Growing growing;

	protected void DestroySelf(object callbackParam)
	{
		CreatureHelpers.DeselectCreature(base.gameObject);
		Util.KDestroyGameObject(base.gameObject);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}

	public void SetConsumptionRate()
	{
		if (receptacleMonitor.Replanted)
		{
			elementConsumer.consumptionRate = 0.09f;
		}
		else
		{
			elementConsumer.consumptionRate = 0.0225f;
		}
	}
}
