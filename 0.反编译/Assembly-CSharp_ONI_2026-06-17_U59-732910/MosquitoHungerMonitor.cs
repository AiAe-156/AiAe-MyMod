using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class MosquitoHungerMonitor : StateMachineComponent<MosquitoHungerMonitor.Instance>
{
	public class States : GameStateMachine<States, Instance, MosquitoHungerMonitor>
	{
		public class HungryStates : State
		{
			public State lookingForVictim;

			public State chaseVictim;
		}

		public State satisfied;

		public HungryStates hungry;

		public TargetParameter victim;

		public override void InitializeStates(out BaseState default_state)
		{
			base.serializable = SerializeType.Never;
			default_state = satisfied;
			satisfied.EventTransition(GameHashes.EffectRemoved, hungry, GameStateMachine<States, MosquitoHungerMonitor.Instance, MosquitoHungerMonitor, object>.Not(IsFed)).Enter(ClearTarget);
			hungry.EventTransition(GameHashes.EffectAdded, satisfied, IsFed).DefaultState(hungry.lookingForVictim);
			hungry.lookingForVictim.ToggleStatusItem(CREATURES.STATUSITEMS.HUNGRY.NAME, CREATURES.STATUSITEMS.HUNGRY.TOOLTIP).ParamTransition(victim, hungry.chaseVictim, GameStateMachine<States, MosquitoHungerMonitor.Instance, MosquitoHungerMonitor, object>.IsNotNull).PreBrainUpdate(LookForVictim);
			hungry.chaseVictim.ParamTransition(victim, hungry.lookingForVictim, GameStateMachine<States, MosquitoHungerMonitor.Instance, MosquitoHungerMonitor, object>.IsNull).EventTransition(GameHashes.TargetLost, hungry.lookingForVictim).Enter(InitiatePokeBehaviour)
				.EventHandler(GameHashes.EntityPoked, OnVictimPoked)
				.Exit(AbortPokeBehaviour)
				.Exit(ClearTarget)
				.Target(victim)
				.EventTransition(GameHashes.TagsChanged, hungry.lookingForVictim, GameStateMachine<States, MosquitoHungerMonitor.Instance, MosquitoHungerMonitor, object>.Not(HasValidVictim))
				.EventTransition(GameHashes.EffectAdded, hungry.lookingForVictim, GameStateMachine<States, MosquitoHungerMonitor.Instance, MosquitoHungerMonitor, object>.Not(HasValidVictim));
		}
	}

	public class Instance : GameStateMachine<States, Instance, MosquitoHungerMonitor, object>.GameInstance
	{
		private Effects effects;

		public GameObject Victim => base.sm.victim.Get(this);

		public bool IsFed => effects.HasEffect("MosquitoFed");

		public Navigator navigator { get; private set; }

		public Instance(MosquitoHungerMonitor master)
			: base(master)
		{
			effects = GetComponent<Effects>();
			navigator = GetComponent<Navigator>();
		}

		public void ApplyFedEffect()
		{
			effects.Add("MosquitoFed", should_save: true);
		}
	}

	public const string DupeMosquitoBiteEffectName = "DupeMosquitoBite";

	public const string CritterMosquitoBiteEffectName = "CritterMosquitoBite";

	public const string Dupe_SUPPRESSED_MosquitoBiteEffectName = "DupeMosquitoBiteSuppressed";

	public const string Critter_SUPPRESSED_MosquitoBiteEffectName = "CritterMosquitoBiteSuppressed";

	public const string MosquitoFedEffectName = "MosquitoFed";

	public const int ReachabilityPadding = 1;

	public bool CanBiteMinions = true;

	public List<Tag> AllowedTargetTags;

	public List<Tag> ForbiddenTargetTags;

	public static string[] ImmunityEffectNames = new string[1] { "HistamineSuppression" };

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}

	private static void ClearTarget(Instance smi)
	{
		smi.sm.victim.Set(null, smi);
	}

	public static bool IsFed(Instance smi)
	{
		return smi.IsFed;
	}

	public static bool HasValidVictim(Instance smi)
	{
		return HasValidVictim(smi, smi.Victim);
	}

	public static bool HasValidVictim(Instance smi, GameObject victimParam)
	{
		if (victimParam != null)
		{
			return !IsVictimForbidden(smi, victimParam.GetComponent<KPrefabID>(), mustBeInSameCavity: true);
		}
		return false;
	}

	public static void LookForVictim(Instance smi)
	{
		CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(Grid.PosToCell(smi));
		if (cavityForCell == null)
		{
			return;
		}
		int myWorldId = smi.GetMyWorldId();
		List<KPrefabID> list = new List<KPrefabID>();
		if (smi.master.CanBiteMinions)
		{
			List<MinionIdentity> worldItems = Components.LiveMinionIdentities.GetWorldItems(myWorldId);
			for (int i = 0; i < worldItems.Count; i++)
			{
				KPrefabID component = worldItems[i].GetComponent<KPrefabID>();
				if (!IsVictimForbidden(smi, component, mustBeInSameCavity: true))
				{
					list.Add(component);
				}
			}
		}
		for (int j = 0; j < cavityForCell.creatures.Count; j++)
		{
			KPrefabID kPrefabID = cavityForCell.creatures[j];
			if (kPrefabID.HasAnyTags(smi.master.AllowedTargetTags) && !IsVictimForbidden(smi, kPrefabID))
			{
				list.Add(kPrefabID);
			}
		}
		KPrefabID value = ((list.Count > 0) ? list.GetRandom() : null);
		smi.sm.victim.Set(value, smi);
	}

	private static bool IsVictimForbidden(Instance smi, KPrefabID victim, bool mustBeInSameCavity = false)
	{
		int cell = Grid.PosToCell(victim);
		if (mustBeInSameCavity)
		{
			CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(Grid.PosToCell(smi));
			if (Game.Instance.roomProber.GetCavityForCell(cell) != cavityForCell)
			{
				return true;
			}
		}
		if (victim.HasAnyTags(smi.master.ForbiddenTargetTags))
		{
			return true;
		}
		Effects component = victim.GetComponent<Effects>();
		if (component.HasEffect("DupeMosquitoBite") || component.HasEffect("CritterMosquitoBite") || component.HasEffect("DupeMosquitoBiteSuppressed") || component.HasEffect("CritterMosquitoBiteSuppressed"))
		{
			return true;
		}
		OccupyArea component2 = victim.GetComponent<OccupyArea>();
		if (!smi.navigator.CanReach(cell, component2.OccupiedCellsOffsets))
		{
			return true;
		}
		return false;
	}

	public static void InitiatePokeBehaviour(Instance smi)
	{
		PokeMonitor.Instance sMI = smi.GetSMI<PokeMonitor.Instance>();
		CellOffset[] array = smi.Victim.GetComponent<OccupyArea>().OccupiedCellsOffsets;
		for (int i = 0; i < 1; i++)
		{
			array = array.Expand();
		}
		sMI.InitiatePoke(smi.Victim, array);
	}

	public static void AbortPokeBehaviour(Instance smi)
	{
		smi.GetSMI<PokeMonitor.Instance>()?.AbortPoke();
	}

	public static void OnVictimPoked(Instance smi, object victimOBJ)
	{
		if (victimOBJ != null)
		{
			GameObject obj = (GameObject)victimOBJ;
			Effects component = obj.GetComponent<Effects>();
			bool flag = obj.HasTag(GameTags.BaseMinion);
			bool flag2 = false;
			string[] immunityEffectNames = ImmunityEffectNames;
			foreach (string effect_id in immunityEffectNames)
			{
				flag2 = flag2 || component.HasEffect(effect_id);
			}
			if (flag)
			{
				component.Add(flag2 ? "DupeMosquitoBiteSuppressed" : "DupeMosquitoBite", should_save: true);
			}
			else
			{
				component.Add(flag2 ? "CritterMosquitoBiteSuppressed" : "CritterMosquitoBite", should_save: true);
			}
			smi.ApplyFedEffect();
		}
	}
}
