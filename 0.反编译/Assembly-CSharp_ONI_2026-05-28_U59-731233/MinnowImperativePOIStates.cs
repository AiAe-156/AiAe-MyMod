using System.Collections;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class MinnowImperativePOIStates : GameStateMachine<MinnowImperativePOIStates, MinnowImperativePOIStates.Instance, IStateMachineTarget, MinnowImperativePOIStates.Def>
{
	public enum MinnowPOIIdentity
	{
		POI_A,
		POI_B,
		POI_C
	}

	public class OnStates : State
	{
		public State enter;

		public State waiting;

		public State working;
	}

	public class Def : BaseDef
	{
		public Tag requestedTag;

		public float requiredMass;

		public MinnowPOIIdentity minnowPOIIdentity;
	}

	public new class Instance : GameInstance, ISidescreenButtonControl
	{
		private int onSelectHandle = -1;

		private Notification completedNotification;

		private static readonly string[] MinnowPOIPrefabIDs = new string[3] { "MinnowImperativePOIA", "MinnowImperativePOIB", "MinnowImperativePOIC" };

		public bool HasUserEverClicked => base.sm.hasShownQuestPopup.Get(this);

		public bool WasCompletedAndAcknowledged => base.smi.sm.isCompleted.Get(base.smi) && base.smi.sm.hasShownCompletedPopup.Get(base.smi);

		public string SidescreenButtonText
		{
			get
			{
				ManualDeliveryKG component = base.gameObject.GetComponent<ManualDeliveryKG>();
				if (component != null && component.enabled)
				{
					return UI.USERMENUACTIONS.MANUAL_DELIVERY.NAME;
				}
				return UI.USERMENUACTIONS.MANUAL_DELIVERY.NAME_OFF;
			}
		}

		public string SidescreenButtonTooltip
		{
			get
			{
				ManualDeliveryKG component = base.gameObject.GetComponent<ManualDeliveryKG>();
				if (component != null && component.enabled)
				{
					return UI.USERMENUACTIONS.MANUAL_DELIVERY.TOOLTIP;
				}
				return UI.USERMENUACTIONS.MANUAL_DELIVERY.TOOLTIP_OFF;
			}
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}

		public override void StartSM()
		{
			base.StartSM();
			Components.MinnowImperativePOIs.Add(this);
			onSelectHandle = Subscribe(-1503271301, OnObjectSelected);
		}

		public override void StopSM(string reason)
		{
			if (onSelectHandle != -1)
			{
				Unsubscribe(ref onSelectHandle);
			}
			ClearCompletedNotification();
			Components.MinnowImperativePOIs.Remove(this);
			base.StopSM(reason);
		}

		public void SetSelectable(bool selectable)
		{
			KSelectable component = base.gameObject.GetComponent<KSelectable>();
			if (component != null)
			{
				component.IsSelectable = selectable;
			}
		}

		private bool IsInPopupEligibleState()
		{
			return base.smi.IsInsideState(base.smi.sm.on);
		}

		private void OnObjectSelected(object data)
		{
			if (((Boxed<bool>)data).value)
			{
				if (completedNotification != null)
				{
					completedNotification.customClickCallback?.Invoke(completedNotification);
				}
				else if (IsInPopupEligibleState() && !base.smi.sm.hasShownQuestPopup.Get(base.smi))
				{
					ShowQuestPopup();
				}
			}
		}

		private string GetStartPopupTitle(MinnowPOIIdentity identity)
		{
			string text = "";
			switch (identity)
			{
			case MinnowPOIIdentity.POI_A:
				text = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_START_POI_A_TITLE;
				break;
			case MinnowPOIIdentity.POI_B:
				text = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_START_POI_B_TITLE;
				break;
			case MinnowPOIIdentity.POI_C:
				text = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_START_POI_C_TITLE;
				break;
			}
			text = text.Replace("{0}", (GetPOIStartedCount() + 1).ToString());
			return text.Replace("{1}", 3.ToString());
		}

		private string GetStartPopupDescription(MinnowPOIIdentity identity)
		{
			string result = "";
			switch (identity)
			{
			case MinnowPOIIdentity.POI_A:
				result = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_START_POI_A_DESCRIPTION;
				result = result.Replace("[AMOUNT2]", GameUtil.GetFormattedMass(200f)).Replace("[AMOUNT1]", "1");
				break;
			case MinnowPOIIdentity.POI_B:
				result = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_START_POI_B_DESCRIPTION;
				result = result.Replace("[AMOUNT1]", GameUtil.GetFormattedMass(20f)).Replace("[AMOUNT2]", "1");
				break;
			case MinnowPOIIdentity.POI_C:
				result = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_START_POI_C_DESCRIPTION;
				result = result.Replace("[AMOUNT1]", GameUtil.GetFormattedMass(10f)).Replace("[AMOUNT2]", "1");
				break;
			}
			return result;
		}

		private string GetCompletePopupTitle(MinnowPOIIdentity identity)
		{
			string result = "";
			switch (identity)
			{
			case MinnowPOIIdentity.POI_A:
				result = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_COMPLETE_POI_A_TITLE;
				break;
			case MinnowPOIIdentity.POI_B:
				result = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_COMPLETE_POI_B_TITLE;
				break;
			case MinnowPOIIdentity.POI_C:
				result = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_COMPLETE_POI_C_TITLE;
				break;
			}
			return result;
		}

		private string GetCompletePopupDescription(MinnowPOIIdentity identity)
		{
			string result = "";
			switch (identity)
			{
			case MinnowPOIIdentity.POI_A:
				result = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_COMPLETE_POI_A_DESCRIPTION;
				result = result.Replace("[AMOUNT1]", GameUtil.GetFormattedMass(200f)).Replace("[AMOUNT2]", "1");
				break;
			case MinnowPOIIdentity.POI_B:
				result = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_COMPLETE_POI_B_DESCRIPTION;
				result = result.Replace("[AMOUNT1]", GameUtil.GetFormattedMass(20f)).Replace("[AMOUNT2]", "1");
				break;
			case MinnowPOIIdentity.POI_C:
				result = COLONY_ACHIEVEMENTS.FINDING_MINNOW.POPUPS.POPUP_COMPLETE_POI_C_DESCRIPTION;
				result = result.Replace("[AMOUNT1]", GameUtil.GetFormattedMass(10f)).Replace("[AMOUNT2]", "1");
				break;
			}
			return result;
		}

		private string GetStartPopupImage(MinnowPOIIdentity identity)
		{
			string result = "";
			switch (identity)
			{
			case MinnowPOIIdentity.POI_A:
				result = "MinnowDiscoveryA_kanim";
				break;
			case MinnowPOIIdentity.POI_B:
				result = "MinnowDiscoveryB_kanim";
				break;
			case MinnowPOIIdentity.POI_C:
				result = "MinnowDiscoveryC_kanim";
				break;
			}
			return result;
		}

		private string GetCompletedPopupImage(MinnowPOIIdentity identity)
		{
			string result = "";
			switch (identity)
			{
			case MinnowPOIIdentity.POI_A:
				result = "MinnowCompleteA_kanim";
				break;
			case MinnowPOIIdentity.POI_B:
				result = "MinnowCompleteB_kanim";
				break;
			case MinnowPOIIdentity.POI_C:
				result = "MinnowCompleteC_kanim";
				break;
			}
			return result;
		}

		public void ShowCompletedNotification()
		{
			EventInfoData eventInfoData = EventInfoDataHelper.GenerateStoryTraitData(GetCompletePopupTitle(base.smi.def.minnowPOIIdentity), GetCompletePopupDescription(base.smi.def.minnowPOIIdentity), UI.TOOLTIPS.CLOSETOOLTIP, GetCompletedPopupImage(base.smi.def.minnowPOIIdentity), (!AllPOIsCompleted()) ? EventInfoDataHelper.PopupType.NORMAL : EventInfoDataHelper.PopupType.COMPLETE, null, null, OnCompletionPopupAcknowledged);
			completedNotification = EventInfoScreen.CreateNotification(eventInfoData);
			base.gameObject.AddOrGet<Notifier>().Add(completedNotification);
		}

		private void OnCompletionPopupAcknowledged()
		{
			ClearCompletedNotification();
			SpawnReward(base.smi);
			int pOICompletedCount = GetPOICompletedCount();
			SaveGame.Instance.ColonyAchievementTracker.minnowQuestsCompleted = Mathf.Max(SaveGame.Instance.ColonyAchievementTracker.minnowQuestsCompleted, pOICompletedCount);
			bool wasLastPOI = pOICompletedCount >= 3;
			Game.Instance.StartCoroutine(CompletionCameraSequence(wasLastPOI));
		}

		private IEnumerator CompletionCameraSequence(bool wasLastPOI)
		{
			Vector3 cameraStartPos = CameraController.Instance.transform.position;
			Vector3 currentPos = base.transform.GetPosition();
			if (!SpeedControlScreen.Instance.IsPaused)
			{
				SpeedControlScreen.Instance.Pause(playSound: false);
			}
			RootMenu.Instance.canTogglePauseScreen = false;
			CameraController.Instance.DisableUserCameraControl = true;
			CameraController.Instance.SetWorldInteractive(state: false);
			ManagementMenu.Instance.CloseAll();
			StoryMessageScreen.HideInterface(hide: true);
			OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID, allowSound: false);
			CameraController.Instance.SetOverrideZoomSpeed(10f);
			CameraController.Instance.SetTargetPos(currentPos, 8f, playSound: false);
			yield return SequenceUtil.WaitForSecondsRealtime(0.5f);
			base.smi.sm.hasShownCompletedPopup.Set(value: true, base.smi);
			if (SpeedControlScreen.Instance.IsPaused)
			{
				SpeedControlScreen.Instance.Unpause(playSound: false);
			}
			SpeedControlScreen.Instance.SetSpeed(0);
			yield return SequenceUtil.WaitForSecondsRealtime(2.5f);
			if (!wasLastPOI)
			{
				if (FindNextUncompletedPOIPosition(out var nextPOIPos))
				{
					Grid.CellToXY(Grid.PosToCell(nextPOIPos), out var x, out var y);
					GridVisibility.Reveal(x, y, 16, 16f);
					yield return null;
					CameraController.Instance.SetOverrideZoomSpeed(2f);
					CameraController.Instance.SetTargetPos(nextPOIPos, 8f, playSound: false);
					yield return SequenceUtil.WaitForSecondsRealtime(2.5f);
				}
			}
			else if (!MinnowAlreadyExists())
			{
				UnlockWinAchievement(base.smi);
				SpawnMinnow();
				base.smi.GoTo(base.smi.sm.off_poi_completed);
			}
			CameraController.Instance.SetOverrideZoomSpeed(1f);
			CameraController.Instance.SetWorldInteractive(state: true);
			CameraController.Instance.DisableUserCameraControl = false;
			RootMenu.Instance.canTogglePauseScreen = true;
			StoryMessageScreen.HideInterface(hide: false);
			NotificationScreen_TemporaryActions.Instance.CreateCameraReturnActionButton(cameraStartPos);
		}

		private static bool FindNextUncompletedPOIPosition(out Vector3 position)
		{
			for (int i = 0; i < 3; i++)
			{
				MinnowPOIIdentity minnowPOIIdentity = (MinnowPOIIdentity)i;
				foreach (Instance item in Components.MinnowImperativePOIs.Items)
				{
					if (item.def.minnowPOIIdentity == minnowPOIIdentity && !item.sm.isCompleted.Get(item))
					{
						position = item.transform.GetPosition();
						return true;
					}
				}
				string name = MinnowPOIPrefabIDs[i];
				List<WorldGenSpawner.Spawnable> spawnablesWithTag = SaveGame.Instance.worldGenSpawner.GetSpawnablesWithTag(false, new Tag(name));
				if (spawnablesWithTag.Count > 0)
				{
					position = Grid.CellToPosCCC(spawnablesWithTag[0].cell, Grid.SceneLayer.Creatures);
					return true;
				}
			}
			position = Vector3.zero;
			return false;
		}

		public void ClearCompletedNotification()
		{
			if (completedNotification != null)
			{
				base.gameObject.AddOrGet<Notifier>().Remove(completedNotification);
				completedNotification = null;
			}
		}

		public static bool AllPOIsCompleted()
		{
			int pOICompletedCount = GetPOICompletedCount();
			return pOICompletedCount >= 3;
		}

		private static bool MinnowAlreadyExists()
		{
			foreach (MinionIdentity item in Components.LiveMinionIdentities.Items)
			{
				if (item.personalityResourceId == "MINNOW")
				{
					return true;
				}
			}
			return false;
		}

		private void SpawnMinnow()
		{
			Personality personality = Db.Get().Personalities.Get("MINNOW");
			MinionStartingStats minionStartingStats = new MinionStartingStats(personality);
			GameObject prefab = Assets.GetPrefab(BaseMinionConfig.GetMinionIDForModel(minionStartingStats.personality.model));
			GameObject gameObject = Util.KInstantiate(prefab);
			gameObject.name = prefab.name;
			Immigration.Instance.ApplyDefaultPersonalPriorities(gameObject);
			Vector3 position = Grid.CellToPosCBC(Grid.PosToCell(base.gameObject), Grid.SceneLayer.Move);
			gameObject.transform.SetLocalPosition(position);
			gameObject.SetActive(value: true);
			minionStartingStats.Apply(gameObject);
			gameObject.GetComponent<MinionIdentity>().arrivalTime = GameClock.Instance.GetCycle();
			gameObject.GetMyWorld().SetDupeVisited();
		}

		private void ShowQuestPopup()
		{
			EventInfoData eventInfoData = EventInfoDataHelper.GenerateStoryTraitData(GetStartPopupTitle(base.smi.def.minnowPOIIdentity), GetStartPopupDescription(base.smi.def.minnowPOIIdentity), UI.TOOLTIPS.CLOSETOOLTIP, GetStartPopupImage(base.smi.def.minnowPOIIdentity), EventInfoDataHelper.PopupType.BEGIN, null, null, delegate
			{
				base.smi.sm.hasShownQuestPopup.Set(value: true, base.smi);
				KSelectable component = base.gameObject.GetComponent<KSelectable>();
				SelectTool.Instance.Select(null);
				SelectTool.Instance.Select(component);
			});
			EventInfoScreen.ShowPopup(eventInfoData);
		}

		public void SetButtonTextOverride(ButtonMenuTextOverride textOverride)
		{
		}

		public bool SidescreenEnabled()
		{
			return IsInPopupEligibleState() && base.smi.sm.hasShownQuestPopup.Get(base.smi);
		}

		public bool SidescreenButtonInteractable()
		{
			return true;
		}

		public void OnSidescreenButtonPressed()
		{
			bool flag = base.smi.sm.hasClickedSideScreen.Get(base.smi);
			base.smi.sm.hasClickedSideScreen.Set(!flag, base.smi);
		}

		public int HorizontalGroupID()
		{
			return -1;
		}

		public int ButtonSideScreenSortOrder()
		{
			return 20;
		}
	}

	public const int TOTALPOICOUNT = 3;

	public State off;

	public OnStates on;

	public State disabling;

	public State poi_completed_pending;

	public State poi_completed_acknowledged;

	public State off_poi_completed;

	public BoolParameter hasShownQuestPopup;

	public BoolParameter hasShownCompletedPopup;

	public BoolParameter isCompleted;

	public BoolParameter hasClickedSideScreen;

	public const int GASKET_REWARD_COUNT = 1;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = off;
		off.PlayAnim("off").ParamTransition(hasShownCompletedPopup, off_poi_completed, GameStateMachine<MinnowImperativePOIStates, Instance, IStateMachineTarget, Def>.IsTrue).ParamTransition(isCompleted, poi_completed_pending, GameStateMachine<MinnowImperativePOIStates, Instance, IStateMachineTarget, Def>.IsTrue)
			.Transition(on.working, (Instance smi) => HasLiquid(smi) && smi.sm.hasClickedSideScreen.Get(smi))
			.Transition(on.enter, (Instance smi) => HasLiquid(smi) && IsVisibleOnCamera(smi));
		on.DoNothing().Transition(disabling, (Instance smi) => !HasLiquid(smi), UpdateRate.SIM_1000ms);
		disabling.PlayAnim("exit").OnAnimQueueComplete(off);
		on.enter.PlayAnim("enter").OnAnimQueueComplete(on.waiting);
		on.waiting.PlayAnim("on", KAnim.PlayMode.Loop).ParamTransition(hasClickedSideScreen, on.working, GameStateMachine<MinnowImperativePOIStates, Instance, IStateMachineTarget, Def>.IsTrue);
		on.working.PlayAnim("on", KAnim.PlayMode.Loop).ToggleComponent<ManualDeliveryKG>().Enter(delegate(Instance smi)
		{
			smi.GetComponent<ManualDeliveryKG>().Pause(pause: false, "Delivery enabled");
		})
			.ParamTransition(hasClickedSideScreen, on.waiting, GameStateMachine<MinnowImperativePOIStates, Instance, IStateMachineTarget, Def>.IsFalse)
			.Transition(poi_completed_pending, HasEnoughMass, UpdateRate.SIM_1000ms)
			.Exit(delegate(Instance smi)
			{
				smi.GetComponent<ManualDeliveryKG>().Pause(pause: true, "Delivery disabled");
			});
		poi_completed_pending.ParamTransition(hasShownCompletedPopup, poi_completed_acknowledged, GameStateMachine<MinnowImperativePOIStates, Instance, IStateMachineTarget, Def>.IsTrue).PlayAnim("on", KAnim.PlayMode.Loop).Enter(delegate(Instance smi)
		{
			isCompleted.Set(value: true, smi);
			smi.ShowCompletedNotification();
		});
		poi_completed_acknowledged.PlayAnim((Instance smi) => Instance.AllPOIsCompleted() ? "victory" : "exit").Enter(delegate
		{
			if (Instance.AllPOIsCompleted())
			{
				MusicManager.instance.PlaySong("Stinger_NewDuplicant");
			}
		}).OnAnimQueueComplete(off_poi_completed)
			.Toggle("Toggle selectable", MakeItUnselectable, MakeItSelectable);
		off_poi_completed.PlayAnim("off").Toggle("Toggle selectable", MakeItUnselectable, MakeItSelectable);
	}

	private static void MakeItUnselectable(Instance smi)
	{
		smi.SetSelectable(selectable: false);
	}

	private static void MakeItSelectable(Instance smi)
	{
		smi.SetSelectable(selectable: true);
	}

	private static bool IsVisibleOnCamera(Instance smi)
	{
		return CameraController.Instance != null && CameraController.Instance.IsVisiblePos(smi.transform.GetPosition());
	}

	private static bool HasEnoughMass(Instance smi)
	{
		return smi.GetComponent<Storage>().GetMassAvailable(smi.def.requestedTag) >= smi.def.requiredMass;
	}

	private static bool HasLiquid(Instance smi)
	{
		int num = Grid.CellAbove(Grid.PosToCell(smi.transform.GetPosition()));
		return Grid.IsValidCell(num) && Grid.Element[num].IsLiquid;
	}

	public static int GetPOIStartedCount()
	{
		int num = 0;
		foreach (Instance item in Components.MinnowImperativePOIs.Items)
		{
			if (item.sm.hasShownQuestPopup.Get(item))
			{
				num++;
			}
		}
		return num;
	}

	public static int GetPOICompletedCount()
	{
		int num = 0;
		foreach (Instance item in Components.MinnowImperativePOIs.Items)
		{
			if (item.sm.isCompleted.Get(item))
			{
				num++;
			}
		}
		return num;
	}

	private static void UnlockWinAchievement(Instance smi)
	{
		SaveGame.Instance.ColonyAchievementTracker.allMinnowQuestsCompleted = true;
	}

	private static void SpawnReward(Instance smi)
	{
		Vector3 vector = Grid.CellToPosCBC(Grid.PosToCell(smi.gameObject), Grid.SceneLayer.Ore);
		switch (smi.def.minnowPOIIdentity)
		{
		case MinnowPOIIdentity.POI_A:
			Util.KInstantiate(Assets.GetPrefab("PlasticGasket"), vector).SetActive(value: true);
			Util.KInstantiate(Assets.GetPrefab("OxyCoralSeed"), vector + Vector3.right).SetActive(value: true);
			Util.KInstantiate(Assets.GetPrefab("OxyCoralSeed"), vector + Vector3.left).SetActive(value: true);
			break;
		case MinnowPOIIdentity.POI_B:
			Util.KInstantiate(Assets.GetPrefab("PlasticGasket"), vector).SetActive(value: true);
			Util.KInstantiate(Assets.GetPrefab(DewPalmConfig.SEED_ID), vector + Vector3.right).SetActive(value: true);
			Util.KInstantiate(Assets.GetPrefab(DewPalmConfig.SEED_ID), vector + Vector3.left).SetActive(value: true);
			break;
		case MinnowPOIIdentity.POI_C:
			Util.KInstantiate(Assets.GetPrefab("PlasticGasket"), vector).SetActive(value: true);
			break;
		}
	}
}
