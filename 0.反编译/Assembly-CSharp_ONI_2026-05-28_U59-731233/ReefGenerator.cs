using System.Collections.Generic;
using UnityEngine;

public class ReefGenerator : GameStateMachine<ReefGenerator, ReefGenerator.Instance, IStateMachineTarget, ReefGenerator.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public ReefGeneratorPower generatorPower;

		public Operational operational;

		public BreathingGeyser.Instance geyserInstance;

		private KAnimLink animLink;

		private KAnimControllerBase myController;

		private KAnimControllerBase geyserController;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			operational = master.GetComponent<Operational>();
			myController = master.GetComponent<KAnimControllerBase>();
			generatorPower = master.GetComponent<ReefGeneratorPower>();
		}

		public override void StartSM()
		{
			int num = Grid.PosToCell(base.master.transform.GetPosition());
			GameObject gameObject = Grid.Objects[num, 1];
			if (gameObject != null)
			{
				geyserInstance = gameObject.GetSMI<BreathingGeyser.Instance>();
			}
			else
			{
				DebugUtil.LogArgs("No reef geyser found for reef generator at cell ", num);
			}
			if (geyserInstance != null)
			{
				geyserController = geyserInstance.GetComponent<KAnimControllerBase>();
				base.smi.sm.geyserTarget.Set(geyserInstance.gameObject, base.smi);
			}
			base.StartSM();
		}

		public bool IsOperationalIgnoringGeyser()
		{
			foreach (KeyValuePair<Operational.Flag, bool> flag in operational.Flags)
			{
				if (flag.Key == reefGeyserEmittingFlag || flag.Value)
				{
					continue;
				}
				return false;
			}
			return true;
		}

		public void SetWattageState(bool active)
		{
			operational.SetFlag(reefGeyserEmittingFlag, active);
		}

		public void LinkToGeyser()
		{
			if (!(geyserController == null))
			{
				animLink = new KAnimLink(geyserController, myController);
				KAnimSynchronizer synchronizer = geyserController.GetSynchronizer();
				synchronizer.Add(myController);
				synchronizer.Sync(myController);
				synchronizer.IdleAnim = "off";
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
		}
	}

	public State inoperational;

	public State idle;

	public State generating;

	public TargetParameter geyserTarget;

	protected static readonly Operational.Flag reefGeyserEmittingFlag = new Operational.Flag("reefGeyserEmitting", Operational.Flag.Type.Requirement);

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = idle;
		inoperational.EventTransition(GameHashes.OperationalFlagChanged, idle, (Instance smi) => smi.IsOperationalIgnoringGeyser()).PlayAnim("off").Enter(DisableWattage);
		idle.Target(geyserTarget).TagTransition(GameTags.GeyserExhaling, generating).Target(masterTarget)
			.EventTransition(GameHashes.OperationalFlagChanged, inoperational, (Instance smi) => !smi.IsOperationalIgnoringGeyser())
			.PlayAnim("off")
			.Enter(DisableWattage)
			.ToggleStatusItem(Db.Get().BuildingStatusItems.ReefGeneratorIdle);
		generating.Target(geyserTarget).TagTransition(GameTags.GeyserExhaling, idle, on_remove: true).Target(masterTarget)
			.EventTransition(GameHashes.OperationalFlagChanged, inoperational, (Instance smi) => !smi.IsOperationalIgnoringGeyser())
			.ToggleStatusItem(Db.Get().BuildingStatusItems.Wattage, (Instance smi) => smi.generatorPower)
			.Enter(EnableWattage)
			.Enter(LinkToGeyser)
			.Exit(UnlinkFromGeyser);
	}

	public static void EnableWattage(Instance smi)
	{
		smi.SetWattageState(active: true);
	}

	public static void DisableWattage(Instance smi)
	{
		smi.SetWattageState(active: false);
	}

	public static void LinkToGeyser(Instance smi)
	{
		smi.LinkToGeyser();
	}

	public static void UnlinkFromGeyser(Instance smi)
	{
		smi.UnlinkFromGeyser();
	}
}
