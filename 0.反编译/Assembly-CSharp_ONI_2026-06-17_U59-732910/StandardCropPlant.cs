using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class StandardCropPlant : StateMachineComponent<StandardCropPlant.StatesInstance>
{
	public class AnimSet
	{
		public string pre_grow;

		public string grow;

		public string grow_pst;

		public string idle_full;

		public string wilt_base;

		public string wilt_recover_base;

		public string harvest;

		public string waning;

		public KAnim.PlayMode grow_playmode = KAnim.PlayMode.Paused;

		private Dictionary<string, string[]> m_wilt = new Dictionary<string, string[]>();

		public void ClearWiltLevelCache()
		{
			m_wilt.Clear();
		}

		public string GetWiltLevel(int level)
		{
			return GetWiltAnimLevel(wilt_base, level);
		}

		public string GetWiltRecoverLevel(int level)
		{
			return GetWiltAnimLevel(wilt_recover_base, level);
		}

		private string GetWiltAnimLevel(string baseSTR, int level)
		{
			if (baseSTR == null)
			{
				return null;
			}
			if (!m_wilt.ContainsKey(baseSTR))
			{
				m_wilt[baseSTR] = new string[3];
				for (int i = 0; i < 3; i++)
				{
					m_wilt[baseSTR][i] = baseSTR + (i + 1);
				}
			}
			return m_wilt[baseSTR][level - 1];
		}

		public AnimSet()
		{
		}

		public AnimSet(AnimSet template)
		{
			pre_grow = template.pre_grow;
			grow = template.grow;
			grow_pst = template.grow_pst;
			idle_full = template.idle_full;
			wilt_base = template.wilt_base;
			wilt_recover_base = template.wilt_recover_base;
			harvest = template.harvest;
			waning = template.waning;
			grow_playmode = template.grow_playmode;
		}
	}

	public class States : GameStateMachine<States, StatesInstance, StandardCropPlant>
	{
		public class AliveStates : PlantAliveSubState
		{
			public State pre_idle;

			public State idle;

			public State pre_fruiting;

			public State fruiting_lost;

			public State barren;

			public State fruiting;

			public State wilting;

			public State wiltRecover;

			public State destroy;

			public State harvest;
		}

		public AliveStates alive;

		public State dead;

		public PlantAliveSubState blighted;

		public override void InitializeStates(out BaseState default_state)
		{
			base.serializable = SerializeType.Both_DEPRECATED;
			default_state = alive;
			dead.ToggleMainStatusItem(Db.Get().CreatureStatusItems.Dead).Enter(delegate(StatesInstance smi)
			{
				if (smi.master.rm.Replanted && !smi.master.GetComponent<KPrefabID>().HasTag(GameTags.Uprooted))
				{
					Notifier notifier = smi.master.gameObject.AddOrGet<Notifier>();
					Notification notification = smi.master.CreateDeathNotification();
					notifier.Add(notification);
				}
				GameUtil.KInstantiate(Assets.GetPrefab(EffectConfigs.PlantDeathId), smi.master.transform.GetPosition(), Grid.SceneLayer.FXFront).SetActive(value: true);
				Harvestable component = smi.master.GetComponent<Harvestable>();
				if (component != null && component.CanBeHarvested && GameScheduler.Instance != null)
				{
					GameScheduler.Instance.Schedule("SpawnFruit", 0.2f, smi.master.crop.SpawnConfiguredFruit);
				}
				smi.master.Trigger(1623392196);
				smi.master.GetComponent<KBatchedAnimController>().StopAndClear();
				UnityEngine.Object.Destroy(smi.master.GetComponent<KBatchedAnimController>());
				smi.Schedule(0.5f, smi.master.DestroySelf);
			});
			blighted.InitializeStates(masterTarget, dead).PlayAnim((StatesInstance smi) => smi.master.anims.waning).ToggleMainStatusItem(Db.Get().CreatureStatusItems.Crop_Blighted)
				.TagTransition(GameTags.Blighted, alive, on_remove: true);
			alive.InitializeStates(masterTarget, dead).DefaultState(alive.pre_idle).ToggleComponent<Growing>()
				.TagTransition(GameTags.Blighted, blighted);
			alive.pre_idle.EnterTransition(alive.idle, (StatesInstance smi) => smi.master.anims.pre_grow == null).PlayAnim((StatesInstance smi) => smi.master.anims.pre_grow).OnAnimQueueComplete(alive.idle)
				.ScheduleGoTo(8f, alive.idle);
			alive.idle.EventTransition(GameHashes.Wilt, alive.wilting, (StatesInstance smi) => smi.master.wiltCondition.IsWilting()).EventTransition(GameHashes.Grow, alive.pre_fruiting, (StatesInstance smi) => smi.master.growing.ReachedNextHarvest()).PlayAnim((StatesInstance smi) => smi.master.anims.grow, (StatesInstance smi) => smi.master.anims.grow_playmode)
				.Enter(RefreshPositionPercent)
				.Update(RefreshPositionPercent, UpdateRate.SIM_4000ms)
				.EventHandler(GameHashes.ConsumePlant, RefreshPositionPercent);
			alive.pre_fruiting.PlayAnim((StatesInstance smi) => smi.master.anims.grow_pst).TriggerOnEnter(GameHashes.BurstEmitDisease).EventTransition(GameHashes.AnimQueueComplete, alive.fruiting)
				.EventTransition(GameHashes.Wilt, alive.wilting)
				.ScheduleGoTo(8f, alive.fruiting);
			alive.fruiting_lost.Enter(delegate(StatesInstance smi)
			{
				if (smi.master.harvestable != null)
				{
					smi.master.harvestable.SetCanBeHarvested(state: false);
				}
			}).GoTo(alive.idle);
			alive.wilting.PlayAnim((Func<StatesInstance, string>)GetWiltAnim, KAnim.PlayMode.Once).EventTransition(GameHashes.WiltRecover, alive.wiltRecover, (StatesInstance smi) => !smi.master.wiltCondition.IsWilting()).EventTransition(GameHashes.Harvest, alive.harvest);
			alive.wiltRecover.EnterTransition(alive.idle, DoesNotHaveWiltRecoverAnim).PlayAnim((Func<StatesInstance, string>)GetWiltRecoverAnim, KAnim.PlayMode.Once).OnAnimQueueComplete(alive.idle);
			alive.fruiting.PlayAnim((StatesInstance smi) => smi.master.anims.idle_full, KAnim.PlayMode.Loop).ToggleTag(GameTags.FullyGrown).Enter(delegate(StatesInstance smi)
			{
				if (smi.master.harvestable != null)
				{
					smi.master.harvestable.SetCanBeHarvested(state: true);
				}
			})
				.EventHandlerTransition(GameHashes.Wilt, alive.wilting, (StatesInstance smi, object obj) => smi.master.wiltsOnReadyToHarvest)
				.EventTransition(GameHashes.Harvest, alive.harvest)
				.EventTransition(GameHashes.Grow, alive.fruiting_lost, (StatesInstance smi) => !smi.master.growing.ReachedNextHarvest());
			alive.harvest.PlayAnim((StatesInstance smi) => smi.master.anims.harvest).Enter(delegate(StatesInstance smi)
			{
				if (smi.master != null)
				{
					smi.master.crop.SpawnConfiguredFruit(null);
				}
				if (smi.master.harvestable != null)
				{
					smi.master.harvestable.SetCanBeHarvested(state: false);
				}
			}).Exit(delegate(StatesInstance smi)
			{
				smi.Trigger(113170146);
			})
				.OnAnimQueueComplete(alive.idle);
		}

		private static string GetWiltAnim(StatesInstance smi)
		{
			float growingPercentage = smi.master.growing.PercentOfCurrentHarvest();
			return GetWiltAnimFromAnimSet(smi.master.anims, growingPercentage);
		}

		private static bool DoesNotHaveWiltRecoverAnim(StatesInstance smi)
		{
			return GetWiltRecoverAnim(smi) == null;
		}

		private static string GetWiltRecoverAnim(StatesInstance smi)
		{
			float growingPercentage = smi.master.growing.PercentOfCurrentHarvest();
			return GetWiltRecoverAnimFromAnimSet(smi.master.anims, growingPercentage);
		}

		private static void RefreshPositionPercent(StatesInstance smi, float dt)
		{
			RefreshPositionPercent(smi);
		}

		private static void RefreshPositionPercent(StatesInstance smi)
		{
			if (!smi.master.preventGrowPositionUpdate)
			{
				smi.master.RefreshPositionPercent();
			}
		}
	}

	public class StatesInstance : GameStateMachine<States, StatesInstance, StandardCropPlant, object>.GameInstance
	{
		public StatesInstance(StandardCropPlant master)
			: base(master)
		{
		}
	}

	private const int WILT_LEVELS = 3;

	[MyCmpReq]
	private Crop crop;

	[MyCmpReq]
	private WiltCondition wiltCondition;

	[MyCmpReq]
	private ReceptacleMonitor rm;

	[MyCmpReq]
	private Growing growing;

	[MyCmpReq]
	private KAnimControllerBase animController;

	[MyCmpGet]
	private Harvestable harvestable;

	public bool wiltsOnReadyToHarvest;

	public bool preventGrowPositionUpdate;

	public static AnimSet defaultAnimSet = new AnimSet
	{
		pre_grow = null,
		grow = "grow",
		grow_pst = "grow_pst",
		idle_full = "idle_full",
		wilt_base = "wilt",
		wilt_recover_base = null,
		harvest = "harvest",
		waning = "waning"
	};

	public AnimSet anims = defaultAnimSet;

	public static string GetWiltAnimFromAnimSet(AnimSet set, float growingPercentage)
	{
		return GetGenericWiltAnimFromAnimSet(set, (int stg) => set.GetWiltLevel(stg), growingPercentage);
	}

	public static string GetWiltRecoverAnimFromAnimSet(AnimSet set, float growingPercentage)
	{
		return GetGenericWiltAnimFromAnimSet(set, (int stg) => set.GetWiltRecoverLevel(stg), growingPercentage);
	}

	private static string GetGenericWiltAnimFromAnimSet(AnimSet set, Func<int, string> wiltLevelFn, float growingPercentage)
	{
		int arg = ((growingPercentage < 0.75f) ? 1 : ((!(growingPercentage < 1f)) ? 3 : 2));
		return wiltLevelFn(arg);
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

	public Notification CreateDeathNotification()
	{
		return new Notification(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION, NotificationType.Bad, (List<Notification> notificationList, object data) => string.Concat(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION_TOOLTIP, notificationList.ReduceMessages(countNames: false)), "/t• " + base.gameObject.GetProperName());
	}

	public void RefreshPositionPercent()
	{
		animController.SetPositionPercent(growing.PercentOfCurrentHarvest());
	}

	private static string ToolTipResolver(List<Notification> notificationList, object data)
	{
		string text = "";
		for (int i = 0; i < notificationList.Count; i++)
		{
			Notification notification = notificationList[i];
			text += (string)notification.tooltipData;
			if (i < notificationList.Count - 1)
			{
				text += "\n";
			}
		}
		return string.Format(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION_TOOLTIP, text);
	}
}
