using Klei.AI;
using STRINGS;
using UnityEngine;

public class EatStates : GameStateMachine<EatStates, EatStates.Instance, IStateMachineTarget, EatStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Element lastMealElement;

		public SolidConsumerMonitor.Instance solidConsumer;

		public string[] eatAnims = new string[3] { "eat_pre", "eat_loop", "eat_pst" };

		public GameObject Edible => base.smi.sm.target.Get(this);

		public bool IsPredator { get; private set; }

		public void OverrideEatAnims(Instance smi, string[] preLoopPstAnims)
		{
			Debug.Assert(preLoopPstAnims != null && preLoopPstAnims.Length == 3);
			smi.eatAnims = preLoopPstAnims;
		}

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.WantsToEat);
			chore.AddPrecondition(ChorePreconditions.instance.DoesntHaveTag, GameTags.Creatures.SuppressedDiet);
			IsPredator = base.gameObject.GetComponent<FactionAlignment>().Alignment == FactionManager.FactionID.Predator;
		}

		public Element GetLatestMealElement()
		{
			return lastMealElement;
		}
	}

	public class PounceState : State
	{
		public State pre;

		public State roll;

		public State hit;

		public State miss;
	}

	public class EatingState : State
	{
		public State pre;

		public State loop;

		public State pst;
	}

	private static Effect PredationStunEffect = CreatePredationStunEffect();

	public ApproachSubState<Pickupable> goingtoeat;

	public State arrivedAtEdible;

	public PounceState pounce;

	public EatingState eating;

	public State failedHunt;

	public State behaviourcomplete;

	public Vector3Parameter offset;

	public TargetParameter target;

	private static float HUNT_WILD_MIN_AGE = 0.825f;

	private static MathUtil.MinMax HUNT_WILD_PRED_RATE = new MathUtil.MinMax(0.1f, 1.1f);

	private static MathUtil.MinMax HUNT_TAME_PRED_RATE = new MathUtil.MinMax(0.4f, 1.05f);

	private static Effect CreatePredationStunEffect()
	{
		return new Effect("StunnedEat", "", "", 5f, show_in_ui: false, trigger_floating_text: false, is_bad: true, "")
		{
			tag = GameTags.Creatures.StunnedBeingEaten
		};
	}

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = goingtoeat;
		root.Enter(SetTarget).Exit(UnreserveEdible);
		State state = goingtoeat.MoveTo(GetEdibleCell, arrivedAtEdible, behaviourcomplete);
		string text = CREATURES.STATUSITEMS.HUNGRY.NAME;
		string tooltip = CREATURES.STATUSITEMS.HUNGRY.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		arrivedAtEdible.EnterTransition(pounce, (Instance smi) => smi.IsPredator).Transition(eating, (Instance smi) => !smi.IsPredator);
		State state2 = pounce.Face(target).DefaultState(pounce.pre);
		string text2 = CREATURES.STATUSITEMS.HUNTING.NAME;
		string tooltip2 = CREATURES.STATUSITEMS.HUNTING.TOOLTIP;
		main = Db.Get().StatusItemCategories.Main;
		state2.ToggleStatusItem(text2, tooltip2, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		pounce.pre.PlayAnim("pounce_pre").OnAnimQueueComplete(pounce.roll);
		pounce.roll.Enter(delegate(Instance smi)
		{
			if (CheckHuntSuccess(smi))
			{
				smi.GoTo(pounce.hit);
			}
			else
			{
				smi.GoTo(pounce.miss);
			}
		});
		pounce.hit.Enter(delegate(Instance smi)
		{
			FreezeEdible(smi);
		}).QueueAnim("pounce_hit").OnAnimQueueComplete(eating);
		pounce.miss.Enter(delegate(Instance smi)
		{
			OnPounceMiss(smi);
		}).QueueAnim("pounce_miss").OnAnimQueueComplete(failedHunt);
		failedHunt.PlayAnim("idle_loop", KAnim.PlayMode.Loop).ScheduleGoTo(5f, behaviourcomplete);
		State state3 = eating.EnterTransition(behaviourcomplete, (Instance smi) => EdibleGotAway(smi)).Face(target).DefaultState(eating.pre);
		string text3 = CREATURES.STATUSITEMS.EATING.NAME;
		string tooltip3 = CREATURES.STATUSITEMS.EATING.TOOLTIP;
		main = Db.Get().StatusItemCategories.Main;
		state3.ToggleStatusItem(text3, tooltip3, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		eating.pre.Enter(delegate(Instance smi)
		{
			FreezeEdible(smi);
		}).QueueAnim((Instance smi) => smi.eatAnims[0]).OnAnimQueueComplete(eating.loop);
		eating.loop.Enter(EatComplete).QueueAnim((Instance smi) => smi.eatAnims[1]).OnAnimQueueComplete(eating.pst);
		eating.pst.QueueAnim((Instance smi) => smi.eatAnims[2]).OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.Enter(delegate(Instance smi)
		{
			smi.solidConsumer.ClearTargetEdible();
		}).PlayAnim("idle_loop", KAnim.PlayMode.Loop).BehaviourComplete(GameTags.Creatures.WantsToEat);
	}

	private static void SetTarget(Instance smi)
	{
		smi.solidConsumer = smi.GetSMI<SolidConsumerMonitor.Instance>();
		smi.sm.target.Set(smi.solidConsumer.targetEdible, smi);
		ReserveEdible(smi);
		smi.OverrideEatAnims(smi, smi.solidConsumer.GetTargetEdibleEatAnims());
		smi.sm.offset.Set(smi.solidConsumer.targetEdibleOffset, smi);
	}

	private static void ReserveEdible(Instance smi)
	{
		GameObject gameObject = smi.sm.target.Get(smi);
		if (gameObject != null)
		{
			DebugUtil.Assert(!gameObject.HasTag(GameTags.Creatures.ReservedByCreature));
			gameObject.AddTag(GameTags.Creatures.ReservedByCreature);
		}
	}

	private static void UnreserveEdible(Instance smi)
	{
		GameObject gameObject = smi.sm.target.Get(smi);
		if (gameObject != null)
		{
			if (gameObject.HasTag(GameTags.Creatures.ReservedByCreature))
			{
				gameObject.RemoveTag(GameTags.Creatures.ReservedByCreature);
				return;
			}
			Debug.LogWarningFormat(smi.gameObject, "{0} UnreserveEdible but it wasn't reserved: {1}", smi.gameObject, gameObject);
		}
	}

	private static void EatComplete(Instance smi)
	{
		PrimaryElement primaryElement = smi.sm.target.Get<PrimaryElement>(smi);
		if (primaryElement != null)
		{
			smi.lastMealElement = primaryElement.Element;
		}
		smi.Trigger(1386391852, (object)smi.sm.target.Get<KPrefabID>(smi));
	}

	private static bool EdibleGotAway(Instance smi)
	{
		int edibleCell = GetEdibleCell(smi);
		return Grid.PosToCell(smi) != edibleCell;
	}

	private static void FreezeEdible(Instance smi)
	{
		if (smi.IsPredator)
		{
			GameObject gameObject = smi.sm.target.Get(smi);
			Effects component = gameObject.GetComponent<Effects>();
			if (component != null)
			{
				component.Add(PredationStunEffect, should_save: false);
			}
			Brain component2 = gameObject.GetComponent<Brain>();
			if (component2 != null)
			{
				Game.BrainScheduler.PrioritizeBrain(component2);
			}
		}
	}

	private static void OnPounceMiss(Instance smi)
	{
		smi.GetComponent<Effects>().Add("PredatorFailedHunt", should_save: true);
		GameObject gameObject = smi.sm.target.Get(smi);
		if (gameObject != null)
		{
			gameObject.Trigger(-787691065, (object)smi.GetComponent<FactionAlignment>());
		}
	}

	private static bool HuntPredicateWild(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		AmountInstance amountInstance = Db.Get().Amounts.Age.Lookup(obj);
		if (amountInstance == null)
		{
			return true;
		}
		float num = amountInstance.value / amountInstance.GetMax();
		if (num < HUNT_WILD_MIN_AGE)
		{
			return false;
		}
		return Random.Range(0f, 1f) < HUNT_WILD_PRED_RATE.Lerp(num);
	}

	private static bool HuntPredicateTame(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		AmountInstance amountInstance = Db.Get().Amounts.Age.Lookup(obj);
		if (amountInstance == null)
		{
			return true;
		}
		float t = amountInstance.value / amountInstance.GetMax();
		return Random.Range(0f, 1f) < HUNT_TAME_PRED_RATE.Lerp(t);
	}

	private static bool CheckHuntSuccess(Instance smi)
	{
		WildnessMonitor.Instance sMI = smi.gameObject.GetSMI<WildnessMonitor.Instance>();
		GameObject gameObject = smi.sm.target.Get(smi);
		WildnessMonitor.Instance instance = ((gameObject != null) ? gameObject.GetSMI<WildnessMonitor.Instance>() : null);
		bool num = sMI?.IsWild() ?? false;
		bool flag = instance?.IsWild() ?? false;
		if (num && flag)
		{
			return HuntPredicateWild(gameObject);
		}
		return HuntPredicateTame(gameObject);
	}

	private static int GetEdibleCell(Instance smi)
	{
		if (smi.Edible == null)
		{
			return Grid.InvalidCell;
		}
		return Grid.PosToCell(smi.Edible.transform.GetPosition() + smi.sm.offset.Get(smi));
	}
}
