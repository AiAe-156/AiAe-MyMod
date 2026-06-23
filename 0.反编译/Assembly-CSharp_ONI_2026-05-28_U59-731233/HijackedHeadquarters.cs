using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

public class HijackedHeadquarters : GameStateMachine<HijackedHeadquarters, HijackedHeadquarters.Instance, IStateMachineTarget, HijackedHeadquarters.Def>
{
	public class Def : BaseDef
	{
	}

	public class OperationalStates : State
	{
		public PasscodeStates passcode;

		public State interceptPre;

		public State interceptLoop;

		public State interceptPst;

		public ReadyToPrintStates readyToPrint;

		public State printing;
	}

	public class PasscodeStates : State
	{
		public State idle_locked;

		public State unlocking;

		public State idle_unlocked;
	}

	public class ReadyToPrintStates : State
	{
		public State pre;

		public State loop;

		public State pst;
	}

	public new class Instance : GameInstance, IUserControlledCapacity
	{
		[MyCmpGet]
		private Storage m_storage;

		[Serialize]
		private bool m_introPopupSeen;

		private EventInfoData eventInfo = null;

		private Notification m_endNotification;

		private MeterController m_progressMeter;

		[Serialize]
		public Dictionary<Tag, int> printCounts = new Dictionary<Tag, int>();

		public static GameObject PrinterceptorInstance;

		private int onBuildingSelectHandle = -1;

		[Serialize]
		public float userMaxCapacity = 500f;

		float IUserControlledCapacity.UserMaxCapacity
		{
			get
			{
				return userMaxCapacity;
			}
			set
			{
				userMaxCapacity = value;
				ApplyMaxCapacity();
			}
		}

		float IUserControlledCapacity.AmountStored => m_storage.MassStored();

		float IUserControlledCapacity.MinCapacity => 0f;

		float IUserControlledCapacity.MaxCapacity => 500f;

		bool IUserControlledCapacity.WholeValues => true;

		LocString IUserControlledCapacity.CapacityUnits => DatabankHelper.NAME_PLURAL;

		bool IUserControlledCapacity.ControlEnabled()
		{
			return base.smi.sm.passcodeUnlocked.Get(base.smi);
		}

		public void ApplyMaxCapacity()
		{
			m_storage.capacityKg = userMaxCapacity;
			m_storage.GetComponent<ManualDeliveryKG>().AbortDelivery("Switching to new delivery request");
			m_storage.GetComponent<ManualDeliveryKG>().capacity = userMaxCapacity;
			m_storage.GetComponent<ManualDeliveryKG>().refillMass = userMaxCapacity;
			m_storage.GetComponent<ManualDeliveryKG>().FillToCapacity = true;
			m_storage.Trigger(-945020481, (object)this);
			if (m_storage.MassStored() > userMaxCapacity)
			{
				m_storage.DropSome(DatabankHelper.ID, m_storage.MassStored() - userMaxCapacity);
			}
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			m_progressMeter = new MeterController(GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer);
			PrinterceptorInstance = base.smi.master.gameObject;
		}

		public void ChangeUIDescriptionToCompleted()
		{
			BuildingComplete component = base.gameObject.GetComponent<BuildingComplete>();
			KSelectable component2 = base.gameObject.GetComponent<KSelectable>();
			component2.SetName(BUILDINGS.PREFABS.HIJACKEDHEADQUARTERS_COMPLETED.NAME);
			component.SetDescriptionFlavour(BUILDINGS.PREFABS.HIJACKEDHEADQUARTERS_COMPLETED.EFFECT);
			component.SetDescription(BUILDINGS.PREFABS.HIJACKEDHEADQUARTERS_COMPLETED.DESC);
		}

		public void AddLore()
		{
			if (StoryManager.Instance.IsStoryComplete(Db.Get().Stories.HijackedHeadquarters) && base.smi.master.GetComponent<LoreBearer>() == null)
			{
				LoreBearerUtil.AddLoreTo(base.smi.master.gameObject, LoreBearerUtil.UnlockSpecificEntryThenNext("story_trait_hijackheadquarters_complete", UI.USERMENUACTIONS.READLORE.SEARCH_OBJECT_SUCCESS.SEARCH6, LoreBearerUtil.UnlockNextEmail, focus: true));
			}
		}

		public void Intercept()
		{
			base.smi.sm.interceptCharges.Delta(1, base.smi);
			ImmigrantScreen.instance.ClearRejectedShuffleState();
			Immigration.Instance.EndImmigration();
			if (base.smi.sm.interceptCharges.Get(base.smi) >= 3)
			{
				base.smi.GoTo(base.smi.sm.operational.readyToPrint);
			}
			SelectTool.Instance.Select(null, skipSound: true);
		}

		public void ActivatePrintInterface()
		{
			SelectTool.Instance.Select(null, skipSound: true);
			PrinterceptorScreen.Instance.SetTarget(this);
			PrinterceptorScreen.Instance.Show();
		}

		public void UnlockPrinterceptor()
		{
			GetComponent<BuildingEnabledButton>().IsEnabled = true;
			base.smi.sm.passcodeUnlocked.Set(value: true, base.smi);
		}

		public void PrintSelectedEntity()
		{
			base.smi.sm.interceptCharges.Set(0, base.smi);
			GameObject gameObject = Util.KInstantiate(Assets.GetPrefab(PrinterceptorScreen.Instance.selectedEntityTag), Grid.CellToPosCCC(Grid.PosToCell(base.gameObject), Grid.SceneLayer.Creatures) + Vector3.up * 1.5f, Quaternion.identity);
			base.smi.master.GetComponent<Storage>().ConsumeIgnoringDisease(DatabankHelper.ID, HijackedHeadquartersConfig.GetDataBankCost(PrinterceptorScreen.Instance.selectedEntityTag, base.smi.printCounts.ContainsKey(PrinterceptorScreen.Instance.selectedEntityTag) ? base.smi.printCounts[PrinterceptorScreen.Instance.selectedEntityTag] : 0));
			gameObject.SetActive(value: true);
			if (!base.smi.printCounts.ContainsKey(PrinterceptorScreen.Instance.selectedEntityTag))
			{
				base.smi.printCounts[PrinterceptorScreen.Instance.selectedEntityTag] = 0;
			}
			base.smi.printCounts[PrinterceptorScreen.Instance.selectedEntityTag]++;
		}

		public override void StartSM()
		{
			base.StartSM();
			UpdateStatusItems();
			UpdateMeter();
			StoryManager.Instance.ForceCreateStory(Db.Get().Stories.HijackedHeadquarters, base.gameObject.GetMyWorldId());
			onBuildingSelectHandle = Subscribe(-1503271301, OnBuildingSelect);
			StoryManager.Instance.DiscoverStoryEvent(Db.Get().Stories.HijackedHeadquarters);
			if (StoryManager.Instance.IsStoryComplete(Db.Get().Stories.HijackedHeadquarters))
			{
				base.smi.AddLore();
			}
			m_storage.capacityKg = userMaxCapacity;
			ApplyMaxCapacity();
		}

		public override void StopSM(string reason)
		{
			Unsubscribe(ref onBuildingSelectHandle);
			base.StopSM(reason);
		}

		private void OnBuildingSelect(object obj)
		{
			if (((Boxed<bool>)obj).value)
			{
				if (!m_introPopupSeen)
				{
					ShowIntroNotification();
				}
				if (m_endNotification != null)
				{
					m_endNotification.customClickCallback(m_endNotification.customClickData);
				}
			}
		}

		private void UpdateStatusItems()
		{
			KSelectable component = base.gameObject.GetComponent<KSelectable>();
		}

		public void UpdateMeter()
		{
			float value = (float)base.smi.sm.interceptCharges.Get(base.smi) / 3f;
			m_progressMeter.SetPositionPercent(Mathf.Clamp01(value));
		}

		public void ShowIntroNotification()
		{
			m_introPopupSeen = true;
			EventInfoData eventInfoData = EventInfoDataHelper.GenerateStoryTraitData(CODEX.STORY_TRAITS.HIJACK_HEADQUARTERS.BEGIN_POPUP.NAME, CODEX.STORY_TRAITS.HIJACK_HEADQUARTERS.BEGIN_POPUP.DESCRIPTION, CODEX.STORY_TRAITS.CLOSE_BUTTON, "printerceptordiscovered_kanim", EventInfoDataHelper.PopupType.BEGIN);
			EventInfoScreen.ShowPopup(eventInfoData);
		}

		public void ShowCompletedNotification()
		{
			eventInfo = EventInfoDataHelper.GenerateStoryTraitData(CODEX.STORY_TRAITS.HIJACK_HEADQUARTERS.END_POPUP.NAME, CODEX.STORY_TRAITS.HIJACK_HEADQUARTERS.END_POPUP.DESCRIPTION, CODEX.STORY_TRAITS.HIJACK_HEADQUARTERS.END_POPUP.BUTTON, "printerceptorprintready_kanim", EventInfoDataHelper.PopupType.COMPLETE);
			m_endNotification = EventInfoScreen.CreateNotification(eventInfo, CompleteStory);
			base.gameObject.AddOrGet<Notifier>().Add(m_endNotification);
			base.gameObject.GetComponent<KSelectable>().AddStatusItem(Db.Get().MiscStatusItems.AttentionRequired, base.smi);
		}

		public void ClearEndNotification()
		{
			base.gameObject.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().MiscStatusItems.AttentionRequired);
			if (m_endNotification != null)
			{
				base.gameObject.AddOrGet<Notifier>().Remove(m_endNotification);
			}
			m_endNotification = null;
		}

		public void CompleteStory(object _)
		{
			if (m_endNotification != null)
			{
				base.gameObject.AddOrGet<Notifier>().Remove(m_endNotification);
			}
			UpdateStatusItems();
			ClearEndNotification();
			int cell = Grid.OffsetCell(Grid.PosToCell(base.smi), new CellOffset(0, 2));
			Vector3 target = Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore);
			StoryManager.Instance.CompleteStoryEvent(Db.Get().Stories.HijackedHeadquarters, base.gameObject.GetComponent<MonoBehaviour>(), new FocusTargetSequence.Data
			{
				WorldId = base.smi.GetMyWorldId(),
				OrthographicSize = 6f,
				TargetSize = 6f,
				Target = target,
				PopupData = eventInfo,
				CompleteCB = OnStorySequenceComplete,
				CanCompleteCB = null
			});
			AddLore();
		}

		private void OnStorySequenceComplete()
		{
			int cell = Grid.OffsetCell(Grid.PosToCell(base.smi), new CellOffset(-1, 1));
			Vector3 keepsakeSpawnPosition = Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore);
			StoryManager.Instance.CompleteStoryEvent(Db.Get().Stories.HijackedHeadquarters, keepsakeSpawnPosition);
			eventInfo = null;
		}

		protected override void OnCleanUp()
		{
			if (m_endNotification != null)
			{
				base.gameObject.AddOrGet<Notifier>().Remove(m_endNotification);
			}
		}
	}

	public IntParameter interceptCharges;

	public BoolParameter passcodeUnlocked;

	public BoolParameter hasBeenCompleted;

	public const int MAX_INTERCEPT_CHARGES = 3;

	public State inoperational;

	public OperationalStates operational;

	public static bool IsReadyToPrint(Instance smi, int charges)
	{
		return charges >= 3;
	}

	public static bool IsOperational(Instance smi)
	{
		return smi.GetComponent<Operational>().IsOperational;
	}

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = inoperational;
		base.serializable = SerializeType.ParamsOnly;
		root.Enter(delegate(Instance smi)
		{
			smi.UpdateMeter();
		}).EventHandler(GameHashes.BuildingActivated, delegate(Instance smi, object activated)
		{
			if (((Boxed<bool>)activated).value)
			{
				StoryManager.Instance.BeginStoryEvent(Db.Get().Stories.HijackedHeadquarters);
			}
		});
		inoperational.PlayAnim("inactive").EventTransition(GameHashes.OperationalChanged, operational.passcode.idle_locked, (Instance smi) => smi.GetComponent<Operational>().IsOperational);
		operational.DefaultState(operational.passcode.idle_locked).ParamTransition(interceptCharges, operational.readyToPrint.pre, IsReadyToPrint).EventTransition(GameHashes.OperationalChanged, inoperational, (Instance smi) => !smi.GetComponent<Operational>().IsOperational)
			.Update(delegate(Instance smi, float dt)
			{
				smi.UpdateMeter();
			});
		operational.passcode.idle_locked.ParamTransition(passcodeUnlocked, operational.passcode.unlocking, GameStateMachine<HijackedHeadquarters, Instance, IStateMachineTarget, Def>.IsTrue).PlayAnim("idle_locked", KAnim.PlayMode.Once);
		operational.passcode.unlocking.PlayAnim("unlocking", KAnim.PlayMode.Once).OnAnimQueueComplete(operational.passcode.idle_unlocked);
		operational.passcode.idle_unlocked.PlayAnim("idle_unlocked", KAnim.PlayMode.Loop).Enter(delegate(Instance smi)
		{
			smi.AddLore();
		}).Enter(delegate(Instance smi)
		{
			smi.ChangeUIDescriptionToCompleted();
		})
			.Update(delegate(Instance smi, float dt)
			{
				if (Immigration.Instance.ImmigrantsAvailable)
				{
					smi.GoTo(operational.interceptPre);
				}
			});
		operational.interceptPre.PlayAnim("intercept_pre").OnAnimQueueComplete(operational.interceptLoop);
		operational.interceptLoop.PlayAnim("intercept_loop", KAnim.PlayMode.Loop).Update(delegate(Instance smi, float dt)
		{
			if (!Immigration.Instance.ImmigrantsAvailable)
			{
				smi.GoTo(operational.interceptPst);
			}
		});
		operational.interceptPst.PlayAnim("intercept").OnAnimQueueComplete(operational.passcode.idle_unlocked);
		operational.readyToPrint.DefaultState(operational.readyToPrint.pre).EventTransition(GameHashes.PrinterceptorPrint, operational.readyToPrint.pst);
		operational.readyToPrint.pre.PlayAnim("print_ready_pre").OnAnimQueueComplete(operational.readyToPrint.loop);
		operational.readyToPrint.loop.QueueAnim("print_ready").QueueAnim("print_ready_loop", loop: true);
		operational.readyToPrint.pst.PlayAnim("printing").ScheduleAction("PrinterceptorPrintDelay", 1f, delegate(Instance smi)
		{
			smi.PrintSelectedEntity();
		}).Exit(delegate(Instance smi)
		{
			if (!smi.sm.hasBeenCompleted.Get(smi))
			{
				smi.sm.hasBeenCompleted.Set(value: true, smi);
				smi.ShowCompletedNotification();
			}
		})
			.OnAnimQueueComplete(operational.passcode.idle_unlocked);
	}
}
