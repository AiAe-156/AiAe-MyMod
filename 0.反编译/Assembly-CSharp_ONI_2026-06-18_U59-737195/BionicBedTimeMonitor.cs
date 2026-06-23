using UnityEngine;

public class BionicBedTimeMonitor : GameStateMachine<BionicBedTimeMonitor, BionicBedTimeMonitor.Instance, IStateMachineTarget, BionicBedTimeMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public class DefragmentingStates : State
	{
		public State traveling;

		public State defragmenting;
	}

	public class ChoreStates : State
	{
		public State notStarted;

		public DefragmentingStates running;
	}

	public class BedTimeStates : State
	{
		public ChoreStates runChore;

		public State choreEnded;
	}

	public new class Instance : GameInstance
	{
		private Light2D light;

		private LightSymbolTracker lightSymbolTracker;

		private BionicBatteryMonitor.Instance batteryMonitor;

		private Schedulable schedulable;

		private KPrefabID prefabID;

		public bool IsOnline
		{
			get
			{
				if (batteryMonitor != null)
				{
					return batteryMonitor.IsOnline;
				}
				return false;
			}
		}

		public bool IsBedTimeChoreRunning => prefabID.HasTag(GameTags.BionicBedTime);

		public bool IsScheduleInBedTime => schedulable.IsAllowed(Db.Get().ScheduleBlockTypes.Sleep);

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			batteryMonitor = base.gameObject.GetSMI<BionicBatteryMonitor.Instance>();
			prefabID = GetComponent<KPrefabID>();
			schedulable = GetComponent<Schedulable>();
		}

		public void EnableLight()
		{
			lightSymbolTracker = base.gameObject.AddOrGet<LightSymbolTracker>();
			lightSymbolTracker.targetSymbol = "snapTo_mouth";
			lightSymbolTracker.enabled = true;
			light = base.gameObject.AddOrGet<Light2D>();
			light.Lux = 1800;
			light.Range = 3f;
			light.enabled = true;
			light.drawOverlay = true;
			light.Color = new Color(0f, 16f / 51f, 1f, 1f);
			light.overlayColour = new Color(1f, 1f, 1f, 1f);
			light.FullRefresh();
		}

		public void DisableLight()
		{
			if (light != null)
			{
				light.enabled = false;
			}
			if (lightSymbolTracker != null)
			{
				lightSymbolTracker.enabled = false;
			}
		}
	}

	private const float LIGHT_RADIUS = 3f;

	private const int LIGHT_LUX = 1800;

	public State notAllowed;

	public BedTimeStates bedTime;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = notAllowed;
		notAllowed.ScheduleChange(bedTime, CanGoToBedTime).EventTransition(GameHashes.BionicOnline, bedTime, CanGoToBedTime);
		bedTime.DefaultState(bedTime.runChore);
		bedTime.runChore.ToggleChore((Instance smi) => new BionicBedTimeModeChore(smi.master), bedTime.choreEnded, bedTime.choreEnded).DefaultState(bedTime.runChore.notStarted);
		bedTime.runChore.notStarted.EventTransition(GameHashes.BeginChore, bedTime.runChore.running, ChoreIsRunning).ScheduleChange(notAllowed, GameStateMachine<BionicBedTimeMonitor, Instance, IStateMachineTarget, Def>.Not(CanGoToBedTime)).EventTransition(GameHashes.BionicOffline, notAllowed);
		bedTime.runChore.running.EventTransition(GameHashes.EndChore, bedTime.runChore.notStarted, GameStateMachine<BionicBedTimeMonitor, Instance, IStateMachineTarget, Def>.Not(ChoreIsRunning)).DefaultState(bedTime.runChore.running.traveling);
		bedTime.runChore.running.traveling.TagTransition(GameTags.BionicBedTime, bedTime.runChore.running.defragmenting);
		bedTime.runChore.running.defragmenting.TagTransition(GameTags.BionicBedTime, bedTime.runChore.running.traveling, on_remove: true).Enter(EnableLight).Exit(DisableLight);
		bedTime.choreEnded.ScheduleChange(notAllowed, GameStateMachine<BionicBedTimeMonitor, Instance, IStateMachineTarget, Def>.Not(CanGoToBedTime)).EventTransition(GameHashes.BionicOffline, notAllowed).GoTo(bedTime.runChore);
	}

	public static bool CanGoToBedTime(Instance smi)
	{
		if (IsOnline(smi))
		{
			return ScheduleIsInBedTime(smi);
		}
		return false;
	}

	private static void EnableLight(Instance smi)
	{
		smi.EnableLight();
	}

	private static void DisableLight(Instance smi)
	{
		smi.DisableLight();
	}

	private static bool IsOnline(Instance smi)
	{
		return smi.IsOnline;
	}

	private static bool ScheduleIsInBedTime(Instance smi)
	{
		return smi.IsScheduleInBedTime;
	}

	public static bool ChoreIsRunning(Instance smi)
	{
		ChoreDriver component = smi.GetComponent<ChoreDriver>();
		Chore chore = ((component == null) ? null : component.GetCurrentChore());
		if (chore == null)
		{
			return false;
		}
		return chore.choreType == Db.Get().ChoreTypes.BionicBedtimeMode;
	}
}
