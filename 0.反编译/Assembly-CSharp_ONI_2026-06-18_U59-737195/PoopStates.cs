using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class PoopStates : GameStateMachine<PoopStates, PoopStates.Instance, IStateMachineTarget, PoopStates.Def>
{
	public class Def : BaseDef
	{
		public KAnimFile emoteAnimFile;

		public string statusItemName;

		public string statusItemTooltip;

		public bool canComplain;

		public Def(KAnimFile emoteAnimFile, string status_item_name, string status_item_tooltip, bool canComplain)
		{
			this.canComplain = canComplain;
			this.emoteAnimFile = emoteAnimFile;
			statusItemName = status_item_name;
			statusItemTooltip = status_item_tooltip;
		}
	}

	public class PoopOnStationState : State
	{
		public State approachPoopSpot;

		public PreLoopPostState pooping;

		public State end;
	}

	public class ComplainState : State
	{
		public State complain;

		public State end;
	}

	public class WildPoopState : State
	{
		public State pooping;

		public State end;
	}

	public new class Instance : GameInstance
	{
		private KPrefabID prefabID;

		private Navigator navigator;

		private float lastTimeWeAttemptedToGo = -10f;

		public bool IsInCooldown => TimePassedSinceLastAttempt < 10f;

		public float TimePassedSinceLastAttempt => Time.time - lastTimeWeAttemptedToGo;

		public GameObject PoopStationObject => base.sm.PoopStation.Get(this);

		public IPoopStation PoopStation
		{
			get
			{
				if (PoopStationObject == null)
				{
					return null;
				}
				IPoopStation component = PoopStationObject.GetComponent<IPoopStation>();
				return (component == null) ? PoopStationObject.GetSMI<IPoopStation>() : component;
			}
		}

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			prefabID = GetComponent<KPrefabID>();
			navigator = GetComponent<Navigator>();
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Poop);
			chore.AddPrecondition(IsInCooldownPrecondition, this);
		}

		public void ConsumeAttempt()
		{
			lastTimeWeAttemptedToGo = Time.time;
			base.sm.RemainingAttempts.Set(base.sm.RemainingAttempts.Get(base.smi) - 1, this);
		}

		public void ResetAttempt()
		{
			lastTimeWeAttemptedToGo = -10f;
			base.sm.RemainingAttempts.Set(3, this);
		}

		public int GetPoopStationCell()
		{
			if (PoopStationObject == null)
			{
				return Grid.InvalidCell;
			}
			return Grid.PosToCell(PoopStationObject);
		}

		public bool IsPoopStationStillValid()
		{
			IPoopStation poopStation = PoopStation;
			if (poopStation == null)
			{
				return false;
			}
			if (poopStation.IsPoopStationOperational())
			{
				return poopStation.GetCurrentPoopStationUser() == base.gameObject;
			}
			return false;
		}

		public bool AttemptToReservePoopStation()
		{
			return PoopStation?.AttemptToReservePoopStation(base.gameObject) ?? false;
		}

		public bool IsPoopStationOperational()
		{
			return PoopStation?.IsPoopStationOperational() ?? false;
		}

		public void ClearReservationFromPoopStation()
		{
			PoopStation?.ClearPoopStationUser(base.gameObject);
		}

		public void PlayAnimOnStation(string animName, KAnim.PlayMode playMode)
		{
			PoopStation?.PlayPoopStationAnim(animName, playMode);
		}

		public string GetPoopStationAnimName(int index)
		{
			IPoopStation poopStation = PoopStation;
			if (poopStation == null)
			{
				return null;
			}
			string[] poopingAnimNames = poopStation.GetPoopingAnimNames();
			if (poopingAnimNames == null || index >= poopingAnimNames.Length)
			{
				return null;
			}
			return poopingAnimNames[index];
		}

		public PoopData GetPoopData()
		{
			return PoopStation?.GetPoopData();
		}

		public void FindPoopStation()
		{
			IPoopStation poopStation = null;
			bool flag = false;
			int num = ((navigator == null) ? 32 : navigator.maxProbeRadiusX);
			int myWorldId = base.gameObject.GetMyWorldId();
			int cell_a = Grid.PosToCell(base.gameObject);
			List<IPoopStation> items = Components.PoopStations.GetItems(myWorldId);
			int num2 = -1;
			float num3 = -1f;
			foreach (IPoopStation item in items)
			{
				if (!item.IsUserCompatibleWithPoopStation(prefabID))
				{
					continue;
				}
				bool flag2 = item.IsPoopStationOperational();
				int num4 = Grid.PosToCell(item.GetPoopStationObject());
				if (Grid.GetCellDistance(cell_a, num4) > num || !flag2)
				{
					continue;
				}
				int navigationCost = navigator.GetNavigationCost(num4);
				if (navigationCost == -1)
				{
					continue;
				}
				float availablePoopCapacity = item.GetAvailablePoopCapacity();
				bool flag3 = availablePoopCapacity > num3;
				if (num2 == -1 || flag3 || (navigationCost < num2 && availablePoopCapacity == num3))
				{
					GameObject currentPoopStationUser = item.GetCurrentPoopStationUser();
					bool flag4 = currentPoopStationUser == null || currentPoopStationUser == base.gameObject;
					if (poopStation == null || !flag || flag4)
					{
						num2 = navigationCost;
						poopStation = item;
						num3 = availablePoopCapacity;
						flag = flag4;
					}
				}
			}
			base.sm.PoopStation.Set(poopStation?.GetPoopStationObject(), this);
		}
	}

	public const float POOP_LOOP_DURATION = 5f;

	public const float ATTEMPT_COOLDOWN = 10f;

	public const int ATTEMPT_TIMES = 3;

	public const string POOP_ANIM_NAME = "poop";

	public const string IDLE_ANIM_NAME = "idle_loop";

	public const string COMPLAIN_ANIM_NAME = "react_neg";

	public const string WAITING_ANIM_NAME = "idle_loop";

	public static Chore.Precondition IsInCooldownPrecondition = new Chore.Precondition
	{
		id = "IsPoopStateInCooldown",
		sortOrder = 1,
		description = DUPLICANTS.CHORES.PRECONDITIONS.IS_POOP_COOLDOWN,
		fn = delegate(ref Chore.Precondition.Context context, object data)
		{
			return !((Instance)data).IsInCooldown;
		}
	};

	public State assess;

	public ComplainState complain;

	public PoopOnStationState stationPoop;

	public WildPoopState wildPoop;

	public IntParameter RemainingAttempts = new IntParameter(3);

	public TargetParameter PoopStation;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = assess;
		assess.Enter(FindPoopStation).EnterTransition(stationPoop, AttemptToReservePoopStation).EnterTransition(complain, IsThereAPoopStationNearby)
			.EnterGoTo(wildPoop);
		complain.DefaultState(complain.complain);
		complain.complain.ParamTransition(RemainingAttempts, wildPoop, GameStateMachine<PoopStates, Instance, IStateMachineTarget, Def>.IsLTEZero_int).EnterTransition(complain.end, CanNotComplain).ToggleAnims((Instance smi) => smi.def.emoteAnimFile)
			.Enter(DisplayThoughtBubble)
			.PlayAnim("react_neg")
			.OnAnimQueueComplete(complain.end)
			.Exit(ClearThoughtBubble);
		complain.end.Enter(ConsumeAttempt).EnterGoTo(null);
		stationPoop.ParamTransition(PoopStation, wildPoop, GameStateMachine<PoopStates, Instance, IStateMachineTarget, Def>.IsNull).ToggleStatusItem("Unused", "Unused", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, category: Db.Get().StatusItemCategories.Main, render_overlay: default(HashedString), status_overlays: 129022, resolve_string_callback: (string str, Instance smi) => smi.def.statusItemName, resolve_tooltip_callback: (string str, Instance smi) => smi.def.statusItemTooltip).DefaultState(stationPoop.approachPoopSpot)
			.EventHandlerTransition(GameHashes.PoopStationUpdate, wildPoop, IsPoopStationStillValid)
			.Exit(ClearReservationFromPoopStation);
		stationPoop.approachPoopSpot.MoveTo(GetPoopStationCell, stationPoop.pooping, wildPoop);
		stationPoop.pooping.DefaultState(stationPoop.pooping.pre);
		stationPoop.pooping.pre.EnterTransition(stationPoop.pooping.loop, (Instance smi) => GetPoopStationPoop_PRE_AnimName(smi) == null).Enter(delegate(Instance smi)
		{
			PlayAnimOnStation(smi, GetPoopStationPoop_PRE_AnimName(smi), KAnim.PlayMode.Once);
		}).PlayAnim((Func<Instance, string>)GetPoopStationPoop_PRE_AnimName, KAnim.PlayMode.Once)
			.OnAnimQueueComplete(stationPoop.pooping.loop);
		stationPoop.pooping.loop.EnterTransition(stationPoop.pooping.pst, (Instance smi) => GetPoopStationPoop_LOOP_AnimName(smi) == null).Enter(delegate(Instance smi)
		{
			PlayAnimOnStation(smi, GetPoopStationPoop_LOOP_AnimName(smi), KAnim.PlayMode.Loop);
		}).PlayAnim((Func<Instance, string>)GetPoopStationPoop_LOOP_AnimName, KAnim.PlayMode.Loop)
			.ScheduleGoTo(5f, stationPoop.pooping.pst);
		stationPoop.pooping.pst.EnterTransition(stationPoop.end, (Instance smi) => GetPoopStationPoop_PST_AnimName(smi) == null).Enter(delegate(Instance smi)
		{
			PlayAnimOnStation(smi, GetPoopStationPoop_PST_AnimName(smi), KAnim.PlayMode.Once);
		}).PlayAnim((Func<Instance, string>)GetPoopStationPoop_PST_AnimName, KAnim.PlayMode.Once)
			.OnAnimQueueComplete(stationPoop.end);
		stationPoop.end.PlayAnim("idle_loop", KAnim.PlayMode.Loop).Enter(ResetAttempts).TriggerOnEnter(GameHashes.PoopStatesCompleted, GetPoopData)
			.BehaviourComplete(GameTags.Creatures.Poop);
		wildPoop.ToggleStatusItem("Unused", "Unused", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, category: Db.Get().StatusItemCategories.Main, render_overlay: default(HashedString), status_overlays: 129022, resolve_string_callback: (string str, Instance smi) => smi.def.statusItemName, resolve_tooltip_callback: (string str, Instance smi) => smi.def.statusItemTooltip).DefaultState(wildPoop.pooping);
		wildPoop.pooping.PlayAnim("poop", KAnim.PlayMode.Once).OnAnimQueueComplete(wildPoop.end);
		wildPoop.end.PlayAnim("idle_loop", KAnim.PlayMode.Loop).Enter(ResetAttempts).TriggerOnEnter(GameHashes.PoopStatesCompleted, (Instance smi) => (object)null)
			.BehaviourComplete(GameTags.Creatures.Poop);
	}

	private static void DisplayThoughtBubble(Instance smi)
	{
		Tuple<Sprite, Color> uISprite = global::Def.GetUISprite(smi.PoopStationObject);
		NameDisplayScreen.Instance.SetThoughtBubbleDisplay(smi.gameObject, bVisible: true, "", Assets.GetSprite("bubble_alert"), uISprite.first);
	}

	private static void ClearThoughtBubble(Instance smi)
	{
		NameDisplayScreen.Instance.SetThoughtBubbleDisplay(smi.gameObject, bVisible: false, null, null, null);
	}

	private static int GetPoopStationCell(Instance smi)
	{
		return smi.GetPoopStationCell();
	}

	private static bool IsThereAPoopStationNearby(Instance smi)
	{
		return smi.PoopStationObject != null;
	}

	private static bool AttemptToReservePoopStation(Instance smi)
	{
		return smi.AttemptToReservePoopStation();
	}

	private static bool IsPoopStationStillValid(Instance smi, object o)
	{
		return smi.IsPoopStationStillValid();
	}

	private static bool CanNotComplain(Instance smi)
	{
		return !smi.def.canComplain;
	}

	private static void ConsumeAttempt(Instance smi)
	{
		smi.ConsumeAttempt();
	}

	private static void ResetAttempts(Instance smi)
	{
		smi.ResetAttempt();
	}

	private static void FindPoopStation(Instance smi)
	{
		smi.FindPoopStation();
	}

	private static void PlayAnimOnStation(Instance smi, string animName, KAnim.PlayMode playmode)
	{
		smi.PlayAnimOnStation(animName, playmode);
	}

	private static void ClearReservationFromPoopStation(Instance smi)
	{
		smi.ClearReservationFromPoopStation();
	}

	private static PoopData GetPoopData(Instance smi)
	{
		return smi.GetPoopData();
	}

	private static string GetPoopStationPoop_PRE_AnimName(Instance smi)
	{
		string text = smi.GetPoopStationAnimName(0);
		if (text == null)
		{
			text = "poop";
		}
		return text;
	}

	private static string GetPoopStationPoop_LOOP_AnimName(Instance smi)
	{
		return smi.GetPoopStationAnimName(1);
	}

	private static string GetPoopStationPoop_PST_AnimName(Instance smi)
	{
		return smi.GetPoopStationAnimName(2);
	}
}
