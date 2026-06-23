using STRINGS;
using UnityEngine;

public class EvilFlower : StateMachineComponent<EvilFlower.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, EvilFlower, object>.GameInstance
	{
		public StatesInstance(EvilFlower smi)
			: base(smi)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, EvilFlower>
	{
		public class AliveStates : PlantAliveSubState
		{
			public State idle;

			public WiltingState wilting;
		}

		public class WiltingState : State
		{
			public State wilting_pre;

			public State wilting;

			public State wilting_pst;
		}

		public State grow;

		public State blocked_from_growing;

		public AliveStates alive;

		public State dead;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = grow;
			base.serializable = SerializeType.Both_DEPRECATED;
			State state = dead;
			string text = CREATURES.STATUSITEMS.DEAD.NAME;
			string tooltip = CREATURES.STATUSITEMS.DEAD.TOOLTIP;
			StatusItemCategory main = Db.Get().StatusItemCategories.Main;
			state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main).TriggerOnEnter(GameHashes.BurstEmitDisease).ToggleTag(GameTags.PreventEmittingDisease)
				.Enter(delegate(StatesInstance smi)
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
				if (smi.master.replanted && !alive.ForceUpdateStatus(smi.master.gameObject))
				{
					smi.GoTo(blocked_from_growing);
				}
			}).PlayAnim("grow_seed", KAnim.PlayMode.Once).EventTransition(GameHashes.AnimQueueComplete, alive);
			State state2 = alive.InitializeStates(masterTarget, dead).DefaultState(alive.idle);
			string text2 = CREATURES.STATUSITEMS.IDLE.NAME;
			string tooltip2 = CREATURES.STATUSITEMS.IDLE.TOOLTIP;
			main = Db.Get().StatusItemCategories.Main;
			state2.ToggleStatusItem(text2, tooltip2, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
			alive.idle.EventTransition(GameHashes.Wilt, alive.wilting, (StatesInstance smi) => smi.master.wiltCondition.IsWilting()).PlayAnim("idle", KAnim.PlayMode.Loop).Enter(delegate(StatesInstance smi)
			{
				smi.master.GetComponent<DecorProvider>().SetValues(smi.master.positive_decor_effect);
				smi.master.GetComponent<DecorProvider>().Refresh();
			});
			alive.wilting.PlayAnim("wilt1", KAnim.PlayMode.Loop).EventTransition(GameHashes.WiltRecover, alive.idle).ToggleTag(GameTags.PreventEmittingDisease)
				.Enter(delegate(StatesInstance smi)
				{
					smi.master.GetComponent<DecorProvider>().SetValues(smi.master.negative_decor_effect);
					smi.master.GetComponent<DecorProvider>().Refresh();
				});
		}
	}

	[MyCmpReq]
	private WiltCondition wiltCondition;

	[MyCmpReq]
	private EntombVulnerable entombVulnerable;

	public bool replanted = false;

	public EffectorValues positive_decor_effect = new EffectorValues
	{
		amount = 1,
		radius = 5
	};

	public EffectorValues negative_decor_effect = new EffectorValues
	{
		amount = -1,
		radius = 5
	};

	private static readonly EventSystem.IntraObjectHandler<EvilFlower> SetReplantedTrueDelegate = new EventSystem.IntraObjectHandler<EvilFlower>(delegate(EvilFlower component, object data)
	{
		component.replanted = true;
	});

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Subscribe(1309017699, SetReplantedTrueDelegate);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}

	protected void DestroySelf(object callbackParam)
	{
		CreatureHelpers.DeselectCreature(base.gameObject);
		Util.KDestroyGameObject(base.gameObject);
	}
}
