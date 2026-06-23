using System.Collections.Generic;
using UnityEngine;

public class ReefGenerator : GameStateMachine<ReefGenerator, ReefGenerator.Instance, IStateMachineTarget, ReefGenerator.Def>
{
	public class Def : BaseDef
	{
	}

	private class OperationalState : State
	{
		public State inhale;

		public State exhale;
	}

	private class InoperationalState : State
	{
		public State withoutGeyser;

		public State withGeyser;
	}

	public new class Instance : GameInstance
	{
		private KAnimLink animLink;

		private KAnimControllerBase geyserController;

		private readonly Operational operational;

		private readonly KAnimControllerBase myController;

		private const string INOPERABLE_INSERT = "_inoperable";

		private static readonly string[] PHASE_PREFIXES = new string[2] { "inhale", "exhale" };

		public ReefGeneratorPower GeneratorPower { get; private set; }

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			operational = master.GetComponent<Operational>();
			myController = master.GetComponent<KAnimControllerBase>();
			GeneratorPower = master.GetComponent<ReefGeneratorPower>();
		}

		public void MonitorForLinkableGeyser()
		{
			if (base.sm.geyserTarget.IsNull(base.smi))
			{
				int cell = Grid.PosToCell(base.master.transform.GetPosition());
				GameObject gameObject = Grid.Objects[cell, 1];
				if (!(gameObject == null))
				{
					base.sm.geyserTarget.Set(gameObject, base.smi);
				}
			}
		}

		public bool IsOperationalIgnoringGeyser()
		{
			if (base.sm.geyserTarget.IsNull(base.smi))
			{
				return false;
			}
			foreach (KeyValuePair<Operational.Flag, bool> flag in operational.Flags)
			{
				if (flag.Key != reefGeyserEmittingFlag && !flag.Value)
				{
					return false;
				}
			}
			return true;
		}

		public void SetWattageState(bool active)
		{
			operational.SetFlag(reefGeyserEmittingFlag, active);
		}

		public void LinkToGeyser()
		{
			GameObject gameObject = base.sm.geyserTarget.Get(base.smi);
			if (!(gameObject == null))
			{
				geyserController = gameObject.GetComponent<KAnimControllerBase>();
				if (!(geyserController == null))
				{
					animLink = new KAnimLink(geyserController, myController);
					KAnimSynchronizer synchronizer = geyserController.GetSynchronizer();
					synchronizer.Add(myController, TranslateGeyserAnim);
					synchronizer.Sync(myController);
					synchronizer.IdleAnim = "off";
				}
			}
		}

		public void UnlinkFromGeyser()
		{
			if (animLink != null)
			{
				animLink.Unregister();
				animLink = null;
			}
			if (geyserController != null)
			{
				geyserController.GetSynchronizer().Remove(myController);
			}
			geyserController = null;
		}

		private string TranslateGeyserAnim(string masterAnimName)
		{
			if (IsOperationalIgnoringGeyser())
			{
				return masterAnimName;
			}
			string[] pHASE_PREFIXES = PHASE_PREFIXES;
			foreach (string text in pHASE_PREFIXES)
			{
				if (masterAnimName.StartsWith(text))
				{
					int length = text.Length;
					return text + "_inoperable" + masterAnimName.Substring(length, masterAnimName.Length - length);
				}
			}
			return masterAnimName;
		}
	}

	private const string OFF_ANIM = "off";

	private readonly InoperationalState inoperational;

	private readonly OperationalState operational;

	private readonly TargetParameter geyserTarget;

	private static readonly Operational.Flag reefGeyserEmittingFlag = new Operational.Flag("reefGeyserEmitting", Operational.Flag.Type.Requirement);

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = inoperational;
		inoperational.Enter(DisableWattage).EnterTransition(inoperational.withoutGeyser, (Instance smi) => geyserTarget.IsNull(smi)).EnterTransition(inoperational.withGeyser, (Instance smi) => !geyserTarget.IsNull(smi));
		inoperational.withoutGeyser.PlayAnim("off").Update(delegate(Instance smi, float _)
		{
			smi.MonitorForLinkableGeyser();
		}).UpdateTransition(inoperational.withGeyser, (Instance smi, float _) => !geyserTarget.IsNull(smi));
		inoperational.withGeyser.Enter(LinkToGeyser).Exit(UnlinkFromGeyser).EnterTransition(operational.inhale, (Instance smi) => smi.IsOperationalIgnoringGeyser())
			.ParamTransition(geyserTarget, inoperational.withoutGeyser, GameStateMachine<ReefGenerator, Instance, IStateMachineTarget, Def>.IsNull)
			.EventTransition(GameHashes.OperationalFlagChanged, operational.inhale, (Instance smi) => smi.IsOperationalIgnoringGeyser());
		operational.Enter(LinkToGeyser).Exit(UnlinkFromGeyser).ParamTransition(geyserTarget, inoperational.withoutGeyser, GameStateMachine<ReefGenerator, Instance, IStateMachineTarget, Def>.IsNull)
			.Target(masterTarget)
			.EventTransition(GameHashes.OperationalFlagChanged, inoperational, (Instance smi) => !smi.IsOperationalIgnoringGeyser());
		operational.inhale.Target(geyserTarget).TagTransition(GameTags.GeyserExhaling, operational.exhale).Target(masterTarget)
			.PlayAnim("off")
			.Enter(DisableWattage)
			.ToggleStatusItem(Db.Get().BuildingStatusItems.ReefGeneratorIdle);
		operational.exhale.Target(geyserTarget).TagTransition(GameTags.GeyserExhaling, operational.inhale, on_remove: true).Target(masterTarget)
			.Enter(EnableWattage)
			.ToggleStatusItem(Db.Get().BuildingStatusItems.Wattage, (Instance smi) => smi.GeneratorPower);
	}

	private static void EnableWattage(Instance smi)
	{
		smi.SetWattageState(active: true);
	}

	private static void DisableWattage(Instance smi)
	{
		smi.SetWattageState(active: false);
	}

	private static void LinkToGeyser(Instance smi)
	{
		smi.LinkToGeyser();
	}

	private static void UnlinkFromGeyser(Instance smi)
	{
		smi.UnlinkFromGeyser();
	}
}
