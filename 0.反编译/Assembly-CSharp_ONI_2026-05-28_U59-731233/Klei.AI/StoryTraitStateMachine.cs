using System;
using Database;
using UnityEngine;

namespace Klei.AI;

public abstract class StoryTraitStateMachine<TStateMachine, TInstance, TDef> : GameStateMachine<TStateMachine, TInstance, StateMachineController, TDef> where TStateMachine : StoryTraitStateMachine<TStateMachine, TInstance, TDef> where TInstance : StoryTraitStateMachine<TStateMachine, TInstance, TDef>.TraitInstance where TDef : StoryTraitStateMachine<TStateMachine, TInstance, TDef>.TraitDef
{
	public class TraitDef : BaseDef
	{
		public string InitalLoreId = null;

		public string CompleteLoreId = null;

		public Story Story = null;

		public StoryCompleteData CompletionData;

		public StoryManager.PopupInfo EventIntroInfo = new StoryManager.PopupInfo
		{
			PopupType = EventInfoDataHelper.PopupType.NONE
		};

		public StoryManager.PopupInfo EventCompleteInfo = new StoryManager.PopupInfo
		{
			PopupType = EventInfoDataHelper.PopupType.NONE
		};
	}

	public class TraitInstance : GameInstance
	{
		protected int buildingActivatedHandle = -1;

		private int onObjectSelectedHandle = -1;

		protected Notifier notifier = null;

		protected KSelectable selectable = null;

		public TraitInstance(StateMachineController master)
			: base(master)
		{
			StoryManager.Instance.ForceCreateStory(base.def.Story, base.gameObject.GetMyWorldId());
			buildingActivatedHandle = master.Subscribe(-1909216579, OnBuildingActivated);
		}

		public TraitInstance(StateMachineController master, TDef def)
			: base(master, def)
		{
			StoryManager.Instance.ForceCreateStory(def.Story, base.gameObject.GetMyWorldId());
			buildingActivatedHandle = master.Subscribe(-1909216579, OnBuildingActivated);
		}

		public override void StartSM()
		{
			selectable = GetComponent<KSelectable>();
			notifier = base.gameObject.AddOrGet<Notifier>();
			base.StartSM();
			onObjectSelectedHandle = Subscribe(-1503271301, OnObjectSelect);
			if (buildingActivatedHandle == -1)
			{
				buildingActivatedHandle = base.master.Subscribe(-1909216579, OnBuildingActivated);
			}
			TriggerStoryEvent(StoryInstance.State.DISCOVERED);
		}

		public override void StopSM(string reason)
		{
			base.StopSM(reason);
			Unsubscribe(ref onObjectSelectedHandle);
			Unsubscribe(ref buildingActivatedHandle);
		}

		public void TriggerStoryEvent(StoryInstance.State storyEvent)
		{
			switch (storyEvent)
			{
			case StoryInstance.State.DISCOVERED:
				StoryManager.Instance.DiscoverStoryEvent(base.def.Story);
				break;
			case StoryInstance.State.IN_PROGRESS:
				StoryManager.Instance.BeginStoryEvent(base.def.Story);
				break;
			case StoryInstance.State.COMPLETE:
			{
				int cell = Grid.OffsetCell(Grid.PosToCell(base.master), base.def.CompletionData.KeepSakeSpawnOffset);
				Vector3 keepsakeSpawnPosition = Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore);
				StoryManager.Instance.CompleteStoryEvent(base.def.Story, keepsakeSpawnPosition);
				break;
			}
			case StoryInstance.State.RETROFITTED:
				break;
			case StoryInstance.State.NOT_STARTED:
				break;
			default:
				throw new NotImplementedException(storyEvent.ToString());
			}
		}

		protected virtual void OnBuildingActivated(object activated)
		{
			if (((Boxed<bool>)activated).value)
			{
				TriggerStoryEvent(StoryInstance.State.IN_PROGRESS);
			}
		}

		protected virtual void OnObjectSelect(object clicked)
		{
			if (((Boxed<bool>)clicked).value)
			{
				StoryInstance storyInstance = StoryManager.Instance.GetStoryInstance(base.def.Story.HashId);
				if (storyInstance != null && storyInstance.PendingType != EventInfoDataHelper.PopupType.NONE)
				{
					OnNotificationClicked();
				}
				else if (!StoryManager.Instance.HasDisplayedPopup(base.def.Story, EventInfoDataHelper.PopupType.BEGIN))
				{
					DisplayPopup(base.def.EventIntroInfo);
				}
			}
		}

		public virtual void CompleteEvent()
		{
			StoryInstance storyInstance = StoryManager.Instance.GetStoryInstance(base.def.Story.HashId);
			if (storyInstance != null && storyInstance.CurrentState != StoryInstance.State.COMPLETE)
			{
				DisplayPopup(base.def.EventCompleteInfo);
			}
		}

		public virtual void OnCompleteStorySequence()
		{
			TriggerStoryEvent(StoryInstance.State.COMPLETE);
		}

		protected void DisplayPopup(StoryManager.PopupInfo info)
		{
			if (info.PopupType != EventInfoDataHelper.PopupType.NONE)
			{
				StoryInstance storyInstance = StoryManager.Instance.DisplayPopup(base.def.Story, info, OnPopupClosed, OnNotificationClicked);
				if (storyInstance != null && !info.DisplayImmediate)
				{
					selectable.AddStatusItem(Db.Get().MiscStatusItems.AttentionRequired, base.smi);
					notifier.Add(storyInstance.Notification);
				}
			}
		}

		public void OnNotificationClicked(object data = null)
		{
			StoryInstance storyInstance = StoryManager.Instance.GetStoryInstance(base.def.Story.HashId);
			if (storyInstance != null)
			{
				selectable.RemoveStatusItem(Db.Get().MiscStatusItems.AttentionRequired);
				notifier.Remove(storyInstance.Notification);
				if (storyInstance.PendingType == EventInfoDataHelper.PopupType.COMPLETE)
				{
					ShowEventCompleteUI();
				}
				else if (storyInstance.PendingType == EventInfoDataHelper.PopupType.NORMAL)
				{
					ShowEventNormalUI();
				}
				else
				{
					ShowEventBeginUI();
				}
			}
		}

		public virtual void OnPopupClosed()
		{
			StoryInstance storyInstance = StoryManager.Instance.GetStoryInstance(base.def.Story.HashId);
			if (storyInstance != null)
			{
				if (storyInstance.HasDisplayedPopup(EventInfoDataHelper.PopupType.COMPLETE))
				{
					Game.Instance.unlocks.Unlock(base.def.CompleteLoreId);
				}
				else
				{
					Game.Instance.unlocks.Unlock(base.def.InitalLoreId);
				}
			}
		}

		protected virtual void ShowEventBeginUI()
		{
		}

		protected virtual void ShowEventNormalUI()
		{
		}

		protected virtual void ShowEventCompleteUI()
		{
			StoryInstance storyInstance = StoryManager.Instance.GetStoryInstance(base.def.Story.HashId);
			if (storyInstance != null)
			{
				int cell = Grid.OffsetCell(Grid.PosToCell(base.master), base.def.CompletionData.CameraTargetOffset);
				Vector3 target = Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore);
				StoryManager.Instance.CompleteStoryEvent(base.def.Story, base.master, new FocusTargetSequence.Data
				{
					WorldId = base.master.GetMyWorldId(),
					OrthographicSize = 6f,
					TargetSize = 6f,
					Target = target,
					PopupData = storyInstance.EventInfo,
					CompleteCB = OnCompleteStorySequence,
					CanCompleteCB = null
				});
			}
		}
	}
}
