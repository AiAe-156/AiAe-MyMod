using System;
using Klei.AI;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/Worker")]
public class StandardWorker : WorkerBase
{
	private State state;

	private StartWorkInfo startWorkInfo;

	private const float EARLIEST_REACT_TIME = 1f;

	[MyCmpGet]
	private Facing facing;

	[MyCmpGet]
	private IExperienceRecipient experienceRecipient;

	private float workPendingCompletionTime;

	private int onWorkChoreDisabledHandle;

	public object workCompleteData;

	private Workable.AnimInfo animInfo;

	private KAnimSynchronizer kanimSynchronizer;

	private StatusItemGroup.Entry previousStatusItem;

	private StateMachine.Instance smi;

	private bool successFullyCompleted;

	private bool surpressForceSyncOnUpdate;

	private Vector3 workAnimOffset = Vector3.zero;

	public bool usesMultiTool = true;

	public bool isFetchDrone;

	private static readonly EventSystem.IntraObjectHandler<StandardWorker> OnChoreInterruptDelegate = new EventSystem.IntraObjectHandler<StandardWorker>(delegate(StandardWorker component, object data)
	{
		component.OnChoreInterrupt(data);
	});

	private Reactable passerbyReactable;

	public override State GetState()
	{
		return state;
	}

	public override StartWorkInfo GetStartWorkInfo()
	{
		return startWorkInfo;
	}

	public override Workable GetWorkable()
	{
		if (startWorkInfo != null)
		{
			return startWorkInfo.workable;
		}
		return null;
	}

	public override KBatchedAnimController GetAnimController()
	{
		return GetComponent<KBatchedAnimController>();
	}

	public override Attributes GetAttributes()
	{
		return base.gameObject.GetAttributes();
	}

	public override AttributeConverterInstance GetAttributeConverter(string id)
	{
		return GetComponent<AttributeConverters>().GetConverter(id);
	}

	public override Guid OfferStatusItem(StatusItem item, object data = null)
	{
		return GetComponent<KSelectable>().AddStatusItem(item, data);
	}

	public override void RevokeStatusItem(Guid id)
	{
		GetComponent<KSelectable>().RemoveStatusItem(id);
	}

	public override void SetWorkCompleteData(object data)
	{
		workCompleteData = data;
	}

	public override bool UsesMultiTool()
	{
		return usesMultiTool;
	}

	public override bool IsFetchDrone()
	{
		return isFetchDrone;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		state = State.Idle;
		Subscribe(1485595942, OnChoreInterruptDelegate);
	}

	private string GetWorkableDebugString()
	{
		if (GetWorkable() == null)
		{
			return "Null";
		}
		return GetWorkable().name;
	}

	public void CompleteWork()
	{
		successFullyCompleted = false;
		state = State.Idle;
		Workable workable = GetWorkable();
		if (workable != null)
		{
			if (workable.triggerWorkReactions && workable.GetWorkTime() > 30f)
			{
				string conversationTopic = workable.GetConversationTopic();
				if (!conversationTopic.IsNullOrWhiteSpace())
				{
					CreateCompletionReactable(conversationTopic);
				}
			}
			DetachAnimOverrides();
			workable.CompleteWork(this);
			if (workable.worker != null && !(workable is Constructable) && !(workable is Deconstructable) && !(workable is Repairable) && !(workable is Disinfectable))
			{
				BonusEvent.GameplayEventData gameplayEventData = new BonusEvent.GameplayEventData();
				gameplayEventData.workable = workable;
				gameplayEventData.worker = workable.worker;
				gameplayEventData.building = workable.GetComponent<BuildingComplete>();
				gameplayEventData.eventTrigger = GameHashes.UseBuilding;
				GameplayEventManager.Instance.Trigger(1175726587, (object)gameplayEventData);
			}
		}
		InternalStopWork(workable, is_aborted: false);
	}

	protected virtual void TryPlayingIdle()
	{
		Navigator component = GetComponent<Navigator>();
		if (component != null)
		{
			NavGrid.NavTypeData navTypeData = component.NavGrid.GetNavTypeData(component.CurrentNavType);
			if (navTypeData.idleAnim.IsValid)
			{
				GetComponent<KAnimControllerBase>().Play(navTypeData.idleAnim);
			}
		}
	}

	public override WorkResult Work(float dt)
	{
		if (state == State.PendingCompletion)
		{
			bool flag = Time.time - workPendingCompletionTime > 10f;
			if (GetComponent<KAnimControllerBase>().IsStopped() || flag)
			{
				TryPlayingIdle();
				if (successFullyCompleted)
				{
					CompleteWork();
					return WorkResult.Success;
				}
				StopWork();
				return WorkResult.Failed;
			}
			return WorkResult.InProgress;
		}
		if (state == State.Completing)
		{
			if (successFullyCompleted)
			{
				CompleteWork();
				return WorkResult.Success;
			}
			StopWork();
			return WorkResult.Failed;
		}
		Workable workable = GetWorkable();
		if (workable != null)
		{
			if ((bool)facing)
			{
				if (workable.ShouldFaceTargetWhenWorking())
				{
					facing.Face(workable.GetFacingTarget());
				}
				else
				{
					Rotatable component = workable.GetComponent<Rotatable>();
					bool flag2 = component != null && component.GetOrientation() == Orientation.FlipH;
					Vector3 position = facing.transform.GetPosition();
					position += (flag2 ? Vector3.left : Vector3.right);
					facing.Face(position);
				}
			}
			if (dt > 0f && Game.Instance.FastWorkersModeActive)
			{
				dt = Mathf.Min(workable.WorkTimeRemaining + 0.01f, 5f);
			}
			Klei.AI.Attribute workAttribute = workable.GetWorkAttribute();
			AttributeLevels component2 = GetComponent<AttributeLevels>();
			if (workAttribute != null && workAttribute.IsTrainable && component2 != null)
			{
				float attributeExperienceMultiplier = workable.GetAttributeExperienceMultiplier();
				component2.AddExperience(workAttribute.Id, dt, attributeExperienceMultiplier);
			}
			string skillExperienceSkillGroup = workable.GetSkillExperienceSkillGroup();
			if (experienceRecipient != null && skillExperienceSkillGroup != null)
			{
				float skillExperienceMultiplier = workable.GetSkillExperienceMultiplier();
				experienceRecipient.AddExperienceWithAptitude(skillExperienceSkillGroup, dt, skillExperienceMultiplier);
			}
			float efficiencyMultiplier = workable.GetEfficiencyMultiplier(this);
			float dt2 = dt * efficiencyMultiplier * 1f;
			if (workable.WorkTick(this, dt2) && state == State.Working)
			{
				successFullyCompleted = true;
				StartPlayingPostAnim();
				workable.OnPendingCompleteWork(this);
			}
		}
		return WorkResult.InProgress;
	}

	private void StartPlayingPostAnim()
	{
		Workable workable = GetWorkable();
		if (workable != null && !workable.alwaysShowProgressBar)
		{
			workable.ShowProgressBar(show: false);
		}
		GetComponent<KPrefabID>().AddTag(GameTags.PreventChoreInterruption);
		state = State.PendingCompletion;
		workPendingCompletionTime = Time.time;
		KAnimControllerBase component = GetComponent<KAnimControllerBase>();
		HashedString[] workPstAnims = workable.GetWorkPstAnims(this, successFullyCompleted);
		if (smi == null)
		{
			if (workPstAnims != null && workPstAnims.Length != 0)
			{
				if (workable != null && workable.synchronizeAnims)
				{
					KAnimControllerBase animController = workable.GetAnimController();
					if (animController != null)
					{
						animController.Play(workPstAnims);
					}
				}
				else
				{
					component.Play(workPstAnims);
				}
			}
			else
			{
				state = State.Completing;
			}
		}
		Trigger(-1142962013, (object)this);
	}

	protected virtual void InternalStopWork(Workable target_workable, bool is_aborted)
	{
		state = State.Idle;
		base.gameObject.RemoveTag(GameTags.PerformingWorkRequest);
		GetComponent<KAnimControllerBase>().Offset -= workAnimOffset;
		workAnimOffset = Vector3.zero;
		GetComponent<KPrefabID>().RemoveTag(GameTags.PreventChoreInterruption);
		DetachAnimOverrides();
		ClearPasserbyReactable();
		AnimEventHandler component = GetComponent<AnimEventHandler>();
		if ((bool)component)
		{
			component.ClearContext();
		}
		if (previousStatusItem.item != null)
		{
			GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, previousStatusItem.item, previousStatusItem.data);
		}
		else
		{
			GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, null);
		}
		if (target_workable != null)
		{
			target_workable.Unsubscribe(onWorkChoreDisabledHandle);
			target_workable.StopWork(this, is_aborted);
		}
		if (smi != null)
		{
			smi.StopSM("stopping work");
			smi = null;
		}
		Vector3 position = base.transform.GetPosition();
		position.z = Grid.GetLayerZ(Grid.SceneLayer.Move);
		base.transform.SetPosition(position);
		startWorkInfo = null;
	}

	private void OnChoreInterrupt(object data)
	{
		if (state == State.Working)
		{
			successFullyCompleted = false;
			StartPlayingPostAnim();
		}
	}

	private void OnWorkChoreDisabled(object data)
	{
		string text = data as string;
		ChoreConsumer component = GetComponent<ChoreConsumer>();
		if (component != null && component.choreDriver != null)
		{
			component.choreDriver.GetCurrentChore()?.Fail((text != null) ? text : "WorkChoreDisabled");
		}
	}

	public override void StopWork()
	{
		Workable workable = GetWorkable();
		if (state == State.PendingCompletion || state == State.Completing)
		{
			state = State.Idle;
			if (successFullyCompleted)
			{
				CompleteWork();
				Trigger(1705586602, (object)this);
			}
			else
			{
				Trigger(-993481695, (object)this);
				InternalStopWork(workable, is_aborted: true);
			}
		}
		else if (state == State.Working)
		{
			if (workable != null && workable.synchronizeAnims)
			{
				KAnimControllerBase animController = workable.GetAnimController();
				if (animController != null)
				{
					HashedString[] workPstAnims = workable.GetWorkPstAnims(this, successfully_completed: false);
					if (workPstAnims != null && workPstAnims.Length != 0)
					{
						animController.Play(workPstAnims);
						animController.SetPositionPercent(1f);
					}
				}
			}
			Trigger(-993481695, (object)this);
			InternalStopWork(workable, is_aborted: true);
		}
		Trigger(2027193395, (object)this);
	}

	public override void StartWork(StartWorkInfo start_work_info)
	{
		startWorkInfo = start_work_info;
		Game.Instance.StartedWork();
		Workable workable = GetWorkable();
		surpressForceSyncOnUpdate = false;
		if (state != State.Idle)
		{
			string text = "";
			if (workable != null)
			{
				text = workable.name;
			}
			Debug.LogError(base.name + "." + text + ".state should be idle but instead it's:" + state);
		}
		string text2 = workable.GetType().Name;
		try
		{
			base.gameObject.AddTag(GameTags.PerformingWorkRequest);
			state = State.Working;
			base.gameObject.Trigger(1568504979, (object)this);
			if (workable != null)
			{
				animInfo = workable.GetAnim(this);
				Vector3 position = base.transform.GetPosition();
				position.z = Grid.GetLayerZ(workable.workLayer);
				base.transform.SetPosition(position);
				if (animInfo.smi != null)
				{
					smi = animInfo.smi;
					smi.StartSM();
				}
				KAnimControllerBase component = GetComponent<KAnimControllerBase>();
				if (animInfo.smi == null)
				{
					AttachOverrideAnims(component);
				}
				surpressForceSyncOnUpdate = workable.surpressWorkerForceSync;
				HashedString[] workAnims = workable.GetWorkAnims(this);
				KAnim.PlayMode workAnimPlayMode = workable.GetWorkAnimPlayMode();
				Vector3 vector = (workAnimOffset = workable.GetWorkOffset());
				component.Offset += vector;
				if (usesMultiTool && animInfo.smi == null && workAnims != null && workAnims.Length != 0 && experienceRecipient != null)
				{
					if (workable.synchronizeAnims)
					{
						KAnimControllerBase animController = workable.GetAnimController();
						if (animController != null)
						{
							kanimSynchronizer = animController.GetSynchronizer();
							if (kanimSynchronizer != null)
							{
								kanimSynchronizer.Add(component);
							}
						}
						animController.Play(workAnims, workAnimPlayMode);
					}
					else
					{
						component.Play(workAnims, workAnimPlayMode);
					}
				}
			}
			workable.StartWork(this);
			if (workable == null)
			{
				Debug.LogWarning("Stopped work as soon as I started. This is usually a sign that a chore is open when it shouldn't be or that it's preconditions are wrong.");
				return;
			}
			onWorkChoreDisabledHandle = workable.Subscribe(2108245096, OnWorkChoreDisabled);
			if (workable.triggerWorkReactions && workable.WorkTimeRemaining > 10f)
			{
				CreatePasserbyReactable();
			}
			KSelectable component2 = GetComponent<KSelectable>();
			previousStatusItem = component2.GetStatusItem(Db.Get().StatusItemCategories.Main);
			component2.SetStatusItem(Db.Get().StatusItemCategories.Main, workable.GetWorkerStatusItem(), workable);
		}
		catch (Exception ex)
		{
			string text3 = "Exception in: Worker.StartWork(" + text2 + ")";
			DebugUtil.LogErrorArgs(this, text3 + "\n" + ex.ToString());
			throw;
		}
	}

	private void Update()
	{
		if (state == State.Working && !surpressForceSyncOnUpdate)
		{
			ForceSyncAnims();
		}
	}

	private void ForceSyncAnims()
	{
		if (Time.deltaTime > 0f && kanimSynchronizer != null)
		{
			kanimSynchronizer.SyncTime();
		}
	}

	public override bool InstantlyFinish()
	{
		Workable workable = GetWorkable();
		if (workable != null)
		{
			return workable.InstantlyFinish(this);
		}
		return false;
	}

	private void AttachOverrideAnims(KAnimControllerBase worker_controller)
	{
		if (animInfo.overrideAnims != null && animInfo.overrideAnims.Length != 0)
		{
			for (int i = 0; i < animInfo.overrideAnims.Length; i++)
			{
				worker_controller.AddAnimOverrides(animInfo.overrideAnims[i]);
			}
		}
	}

	private void DetachAnimOverrides()
	{
		KAnimControllerBase component = GetComponent<KAnimControllerBase>();
		if (kanimSynchronizer != null)
		{
			kanimSynchronizer.RemoveWithoutIdleAnim(component);
			kanimSynchronizer = null;
		}
		if (animInfo.overrideAnims != null)
		{
			for (int i = 0; i < animInfo.overrideAnims.Length; i++)
			{
				component.RemoveAnimOverrides(animInfo.overrideAnims[i]);
			}
			animInfo.overrideAnims = null;
		}
	}

	private void CreateCompletionReactable(string topic)
	{
		if (!(GameClock.Instance.GetTime() / 600f < 1f))
		{
			EmoteReactable emoteReactable = OneshotReactableLocator.CreateOneshotReactable(base.gameObject, 3f, "WorkCompleteAcknowledgement", Db.Get().ChoreTypes.Emote, 9, 5, 100f);
			Emote clapCheer = Db.Get().Emotes.Minion.ClapCheer;
			emoteReactable.SetEmote(clapCheer);
			emoteReactable.RegisterEmoteStepCallbacks("clapcheer_pre", GetReactionEffect, null).RegisterEmoteStepCallbacks("clapcheer_pst", null, delegate(GameObject r)
			{
				r.Trigger(937885943, (object)topic);
			});
			Tuple<Sprite, Color> tuple = null;
			tuple = Def.GetUISprite(topic, "ui", centered: true);
			if (tuple != null)
			{
				Thought thought = new Thought("Completion_" + topic, null, tuple.first, "mode_satisfaction", "conversation_short", "bubble_conversation", SpeechMonitor.PREFIX_HAPPY, "", show_immediately: true);
				emoteReactable.SetThought(thought);
			}
		}
	}

	private void CreatePasserbyReactable()
	{
		if (!(GameClock.Instance.GetTime() / 600f < 1f) && passerbyReactable == null)
		{
			EmoteReactable emoteReactable = new EmoteReactable(base.gameObject, "WorkPasserbyAcknowledgement", Db.Get().ChoreTypes.Emote, 5, 5, 30f, 720f * TuningData<DupeGreetingManager.Tuning>.Get().greetingDelayMultiplier);
			Emote thumbsUp = Db.Get().Emotes.Minion.ThumbsUp;
			emoteReactable.SetEmote(thumbsUp).SetThought(Db.Get().Thoughts.Encourage).AddPrecondition(ReactorIsOnFloor)
				.AddPrecondition(ReactorIsFacingMe)
				.AddPrecondition(ReactorIsntPartying);
			emoteReactable.RegisterEmoteStepCallbacks("react", GetReactionEffect, null);
			passerbyReactable = emoteReactable;
		}
	}

	private void GetReactionEffect(GameObject reactor)
	{
		Effects component = GetComponent<Effects>();
		if (component != null)
		{
			component.Add("WorkEncouraged", should_save: true);
		}
	}

	private bool ReactorIsOnFloor(GameObject reactor, Navigator.ActiveTransition transition)
	{
		return transition.end == NavType.Floor;
	}

	private bool ReactorIsFacingMe(GameObject reactor, Navigator.ActiveTransition transition)
	{
		Facing component = reactor.GetComponent<Facing>();
		return base.transform.GetPosition().x < reactor.transform.GetPosition().x == component.GetFacing();
	}

	private bool ReactorIsntPartying(GameObject reactor, Navigator.ActiveTransition transition)
	{
		ChoreConsumer component = reactor.GetComponent<ChoreConsumer>();
		if (component.choreDriver.HasChore())
		{
			return component.choreDriver.GetCurrentChore().choreType != Db.Get().ChoreTypes.Party;
		}
		return false;
	}

	private void ClearPasserbyReactable()
	{
		if (passerbyReactable != null)
		{
			passerbyReactable.Cleanup();
			passerbyReactable = null;
		}
	}
}
