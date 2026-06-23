using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class LargeImpactorNotificationMonitor : GameStateMachine<LargeImpactorNotificationMonitor, LargeImpactorNotificationMonitor.Instance, IStateMachineTarget, LargeImpactorNotificationMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public class NotificationStates : State
	{
		public State delayEntry;

		public State running;
	}

	public class DiscoveredStates : State
	{
		public State sequence;

		public NotificationStates notification;
	}

	public new class Instance : GameInstance
	{
		private Notifier notifier;

		private Coroutine sequenceCoroutine;

		private LargeImpactorSequenceUIReticle sequenceReticle;

		public bool HasRevealSequencePlayed => base.sm.SequenceCompleted.Get(this);

		public Notification notification { get; private set; }

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			notifier = base.gameObject.AddOrGet<Notifier>();
			LargeImpactorStatus.Instance sMI = base.smi.GetSMI<LargeImpactorStatus.Instance>();
			string title = MISC.NOTIFICATIONS.INCOMINGPREHISTORICASTEROIDNOTIFICATION.NAME;
			object tooltip_data = sMI;
			notification = new Notification(title, NotificationType.Custom, ResolveNotificationTooltip, tooltip_data, expires: false);
			notification.customNotificationID = "LargeImpactNotification";
		}

		private string ResolveNotificationTooltip(List<Notification> not, object data)
		{
			LargeImpactorStatus.Instance instance = (LargeImpactorStatus.Instance)data;
			return GameUtil.SafeStringFormat(MISC.NOTIFICATIONS.INCOMINGPREHISTORICASTEROIDNOTIFICATION.TOOLTIP, GameUtil.GetFormattedInt(instance.Health), GameUtil.GetFormattedInt(instance.def.MAX_HEALTH), GameUtil.GetFormattedCycles(instance.TimeRemainingBeforeCollision));
		}

		public void RevealSurface()
		{
			GameplayEventInstance gameplayEventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.Id);
			if (gameplayEventInstance != null)
			{
				WorldContainer world = ClusterManager.Instance.GetWorld(gameplayEventInstance.worldId);
				if (world != null && !world.IsSurfaceRevealed)
				{
					world.RevealSurface();
				}
			}
		}

		public void SetNotificationVisibility(bool visible)
		{
			if (visible)
			{
				notifier.Add(notification);
			}
			else
			{
				notifier.Remove(notification);
			}
		}

		public void PlaySequence()
		{
			AbortSequenceCoroutine();
			CreateReticleForSequence();
			GameplayEventInstance gameplayEventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.Id);
			if (gameplayEventInstance != null)
			{
				WorldContainer world = ClusterManager.Instance.GetWorld(gameplayEventInstance.worldId);
				sequenceCoroutine = LargeImpactorRevealSequence.Start(notifier, sequenceReticle, world);
			}
		}

		private void CreateReticleForSequence()
		{
			DeleteReticleObject();
			sequenceReticle = Util.KInstantiateUI<LargeImpactorSequenceUIReticle>(ScreenPrefabs.Instance.largeImpactorSequenceReticlePrefab.gameObject, GameScreenManager.Instance.ssOverlayCanvas.gameObject, force_active: true);
			LargeImpactorStatus.Instance sMI = base.gameObject.GetSMI<LargeImpactorStatus.Instance>();
			sequenceReticle.SetTarget(sMI);
		}

		private void DeleteReticleObject()
		{
			if (sequenceReticle != null)
			{
				sequenceReticle.gameObject.DeleteObject();
			}
		}

		private void AbortSequenceCoroutine()
		{
			if (sequenceCoroutine != null)
			{
				notifier.StopCoroutine(sequenceCoroutine);
				sequenceCoroutine = null;
			}
		}

		protected override void OnCleanUp()
		{
			AbortSequenceCoroutine();
			DeleteReticleObject();
			base.OnCleanUp();
		}
	}

	public const string NOTIFICATION_PREFAB_ID = "LargeImpactNotification";

	public State undiscovered;

	public DiscoveredStates discovered;

	public BoolParameter HasBeenDiscovered;

	public BoolParameter SequenceCompleted;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = undiscovered;
		undiscovered.ParamTransition(HasBeenDiscovered, discovered, GameStateMachine<LargeImpactorNotificationMonitor, Instance, IStateMachineTarget, Def>.IsTrue).EventHandler(GameHashes.DiscoveredSpace, (Instance smi) => Game.Instance, OnDuplicantReachedSpace).EventHandler(GameHashes.DLCPOICompleted, (Instance smi) => Game.Instance, OnPOIActivated);
		discovered.DefaultState(discovered.sequence);
		discovered.sequence.ParamTransition(SequenceCompleted, discovered.notification, GameStateMachine<LargeImpactorNotificationMonitor, Instance, IStateMachineTarget, Def>.IsTrue).Enter(RevealSurface).Enter(PlaySequence)
			.EventHandler(GameHashes.SequenceCompleted, CompleteSequence);
		discovered.notification.DefaultState(discovered.notification.delayEntry);
		discovered.notification.delayEntry.ScheduleGoTo(3f, discovered.notification.running);
		discovered.notification.running.Enter(PlayNotificationEnterSound).Enter(SetLandingZoneVisualizationToActive).ScheduleAction("Toggle off the visualization after a delay", 2f, FoldTheVisualization)
			.ToggleNotification((Instance smi) => smi.notification);
	}

	public static void CompleteSequence(Instance smi)
	{
		smi.sm.SequenceCompleted.Set(value: true, smi);
	}

	public static void Discover(Instance smi)
	{
		smi.sm.HasBeenDiscovered.Set(value: true, smi);
	}

	public static void RevealSurface(Instance smi)
	{
		smi.RevealSurface();
	}

	public static void PlayNotificationEnterSound(Instance smi)
	{
		KFMOD.PlayUISound(GlobalAssets.GetSound("Notification_Imperative"));
	}

	public static void SetLandingZoneVisualizationToActive(Instance smi)
	{
		smi.GetComponent<LargeImpactorVisualizer>().Active = true;
	}

	public static void FoldTheVisualization(Instance smi)
	{
		LargeImpactorVisualizer component = smi.GetComponent<LargeImpactorVisualizer>();
		if (!component.Folded)
		{
			component.SetFoldedState(shouldBeFolded: true);
		}
	}

	public static void OnPOIActivated(Instance smi, object obj)
	{
		if (((GameObject)obj).PrefabID() == "POIDlc4TechUnlock")
		{
			Discover(smi);
		}
	}

	public static void OnDuplicantReachedSpace(Instance smi, object obj)
	{
		int myWorldId = ((GameObject)obj).GetMyWorldId();
		int myWorldId2 = smi.gameObject.GetMyWorldId();
		if (myWorldId == myWorldId2)
		{
			Discover(smi);
		}
	}

	public static void PlaySequence(Instance smi)
	{
		smi.PlaySequence();
	}
}
