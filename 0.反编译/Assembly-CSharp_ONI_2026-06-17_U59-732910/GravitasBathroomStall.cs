using System.Collections;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class GravitasBathroomStall : GameStateMachine<GravitasBathroomStall, GravitasBathroomStall.Instance, IStateMachineTarget, GravitasBathroomStall.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		private StoryInstance storyInstance;

		private Notification completeNotification;

		private int onBuildingSelectHandle = -1;

		private int printerceptorOperationalEventHandle = -1;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}

		public override void StartSM()
		{
			base.StartSM();
			GetComponent<Activatable>().activationCondition = () => HijackedHeadquarters.Instance.PrinterceptorInstance != null && HijackedHeadquarters.IsOperational(HijackedHeadquarters.Instance.PrinterceptorInstance.GetSMI<HijackedHeadquarters.Instance>());
			storyInstance = StoryManager.Instance.GetStoryInstance(Db.Get().Stories.HijackedHeadquarters.HashId);
			onBuildingSelectHandle = Subscribe(-1503271301, OnBuildingSelect);
		}

		public override void StopSM(string reason)
		{
			if (onBuildingSelectHandle != -1)
			{
				Unsubscribe(ref onBuildingSelectHandle);
			}
			base.StopSM(reason);
		}

		private void OnBuildingSelect(object obj)
		{
			if (((Boxed<bool>)obj).value && completeNotification != null)
			{
				completeNotification.customClickCallback(completeNotification.customClickData);
			}
		}

		public void ShowLoreUnlockedPopup()
		{
			EventInfoData eventInfoData = EventInfoDataHelper.GenerateStoryTraitData(CODEX.STORY_TRAITS.HIJACK_HEADQUARTERS.UNLOCK_POPUP.NAME, CODEX.STORY_TRAITS.HIJACK_HEADQUARTERS.UNLOCK_POPUP.DESCRIPTION, CODEX.STORY_TRAITS.HIJACK_HEADQUARTERS.UNLOCK_POPUP.BUTTON, "printerceptorcoderevealed_kanim", EventInfoDataHelper.PopupType.NORMAL, null, null, delegate
			{
				base.smi.sm.hasShownPopup.Set(value: true, base.smi);
				base.smi.master.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(Sequence(base.smi));
				base.smi.GoTo(base.smi.sm.complete);
			});
			completeNotification = EventInfoScreen.CreateNotification(eventInfoData);
			base.gameObject.AddOrGet<Notifier>().Add(completeNotification);
			base.gameObject.GetComponent<KSelectable>().AddStatusItem(Db.Get().MiscStatusItems.AttentionRequired, base.smi);
		}

		private static IEnumerator Sequence(Instance smi)
		{
			StoryManager.Instance.GetStoryInstance(Db.Get().Stories.HijackedHeadquarters.HashId);
			smi.ClearEndNotification();
			if (!HijackedHeadquarters.Instance.PrinterceptorInstance.IsNullOrDestroyed())
			{
				smi.RevealPrinterceptor();
				CameraController.Instance.FadeOut();
				yield return SequenceUtil.WaitForSecondsRealtime(1f);
				Vector3 vector = new Vector3(2f, 3f, 0f);
				GameUtil.FocusCamera(HijackedHeadquarters.Instance.PrinterceptorInstance.transform.position + vector, 10f, playSound: false);
				yield return SequenceUtil.WaitForSecondsRealtime(1f);
				if (SpeedControlScreen.Instance.IsPaused)
				{
					SpeedControlScreen.Instance.Unpause(playSound: false);
				}
				CameraController.Instance.FadeIn();
				yield return SequenceUtil.WaitForSecondsRealtime(1f);
				HijackedHeadquarters.Instance.PrinterceptorInstance.GetSMI<HijackedHeadquarters.Instance>().UnlockPrinterceptor();
			}
		}

		private void RevealPrinterceptor()
		{
			List<WorldGenSpawner.Spawnable> list = new List<WorldGenSpawner.Spawnable>();
			foreach (WorldContainer worldContainer in ClusterManager.Instance.WorldContainers)
			{
				list.AddRange(SaveGame.Instance.worldGenSpawner.GetSpawnablesWithTag("HijackedHeadquarters", worldContainer.id));
			}
			foreach (WorldGenSpawner.Spawnable item in list)
			{
				Grid.CellToXY(item.cell, out var x, out var y);
				GridVisibility.Reveal(x, y, 10, 10f);
			}
		}

		public void ClearEndNotification()
		{
			base.gameObject.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().MiscStatusItems.AttentionRequired);
			if (completeNotification != null)
			{
				base.gameObject.AddOrGet<Notifier>().Remove(completeNotification);
			}
			completeNotification = null;
		}

		public void SubscribeToPrinterceptorOperational()
		{
			UnsubscribeFromPrinterceptorOperational();
			if (HijackedHeadquarters.Instance.PrinterceptorInstance != null)
			{
				printerceptorOperationalEventHandle = HijackedHeadquarters.Instance.PrinterceptorInstance.Subscribe(-592767678, delegate
				{
					base.smi.master.GetComponent<Activatable>().CancelChore();
					base.smi.GoTo(base.smi.sm.start);
				});
			}
		}

		public void UnsubscribeFromPrinterceptorOperational()
		{
			if (printerceptorOperationalEventHandle != -1 && HijackedHeadquarters.Instance.PrinterceptorInstance != null)
			{
				HijackedHeadquarters.Instance.PrinterceptorInstance.Unsubscribe(printerceptorOperationalEventHandle);
			}
			printerceptorOperationalEventHandle = -1;
		}
	}

	public State start;

	public State branch;

	public State blinking;

	public State activated;

	public State complete;

	public BoolParameter hasBeenActivated;

	public BoolParameter hasShownPopup;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = start;
		root.DefaultState(start);
		start.PlayAnim("idle").Update(delegate(Instance smi, float dt)
		{
			if (HijackedHeadquarters.Instance.PrinterceptorInstance != null && HijackedHeadquarters.IsOperational(HijackedHeadquarters.Instance.PrinterceptorInstance.GetSMI<HijackedHeadquarters.Instance>()))
			{
				smi.sm.hasBeenActivated.Set(smi.master.GetComponent<Activatable>().IsActivated, smi);
				smi.GoTo(branch);
			}
		});
		branch.ParamTransition(hasBeenActivated, blinking, GameStateMachine<GravitasBathroomStall, Instance, IStateMachineTarget, Def>.IsFalse).ParamTransition(hasBeenActivated, activated, GameStateMachine<GravitasBathroomStall, Instance, IStateMachineTarget, Def>.IsTrue);
		blinking.PlayAnim("code_ready", KAnim.PlayMode.Loop).EventHandlerTransition(GameHashes.BuildingActivated, activated, (Instance smi, object data) => ((Boxed<bool>)data).value).Enter(delegate(Instance smi)
		{
			smi.SubscribeToPrinterceptorOperational();
		})
			.Exit(delegate(Instance smi)
			{
				smi.UnsubscribeFromPrinterceptorOperational();
			});
		activated.Enter(delegate(Instance smi)
		{
			if (!smi.sm.hasShownPopup.Get(smi))
			{
				smi.ShowLoreUnlockedPopup();
			}
			else
			{
				smi.GoTo(complete);
			}
			smi.sm.hasBeenActivated.Set(value: true, smi);
		}).PlayAnim("activated");
		complete.PlayAnim("idle");
	}
}
