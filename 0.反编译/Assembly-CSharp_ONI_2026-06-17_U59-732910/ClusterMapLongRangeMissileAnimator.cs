using System;
using UnityEngine;

public class ClusterMapLongRangeMissileAnimator : GameStateMachine<ClusterMapLongRangeMissileAnimator, ClusterMapLongRangeMissileAnimator.StatesInstance, ClusterMapVisualizer>
{
	public class ExplodingStates : State
	{
		public State pre;

		public State animating;

		public State post;
	}

	public class StatesInstance : GameInstance
	{
		public ClusterGridEntity entity;

		private int animCompleteHandle = -1;

		private GameObject animCompleteSubscriber;

		public StatesInstance(ClusterMapVisualizer master, ClusterGridEntity entity)
			: base(master)
		{
			this.entity = entity;
			base.sm.entityTarget.Set(entity, this);
		}

		public void PlayVisAnim(string animName, KAnim.PlayMode playMode)
		{
			GetComponent<ClusterMapVisualizer>().PlayAnim(animName, playMode);
		}

		public void ToggleVisAnim(bool on)
		{
			ClusterMapVisualizer component = GetComponent<ClusterMapVisualizer>();
			if (!on)
			{
				component.GetFirstAnimController().Play("grounded");
			}
		}

		public void SubscribeOnVisAnimComplete(Action<object> action)
		{
			ClusterMapVisualizer component = GetComponent<ClusterMapVisualizer>();
			UnsubscribeOnVisAnimComplete();
			animCompleteSubscriber = component.GetFirstAnimController().gameObject;
			animCompleteHandle = animCompleteSubscriber.Subscribe(-1061186183, action);
		}

		public void UnsubscribeOnVisAnimComplete()
		{
			if (animCompleteHandle != -1)
			{
				DebugUtil.DevAssert(animCompleteSubscriber != null, "ClustermapBallisticAnimator animCompleteSubscriber GameObject is null. Whatever the previous gameObject in this variable was, it may not have unsubscribed from an event properly");
				animCompleteSubscriber.Unsubscribe(animCompleteHandle);
				animCompleteHandle = -1;
			}
		}

		protected override void OnCleanUp()
		{
			base.OnCleanUp();
			UnsubscribeOnVisAnimComplete();
		}
	}

	public TargetParameter entityTarget;

	public State moving;

	public State idle;

	public ExplodingStates exploding;

	public override void InitializeStates(out BaseState defaultState)
	{
		defaultState = moving;
		root.OnTargetLost(entityTarget, null).Target(entityTarget).TagTransition(GameTags.LongRangeMissileMoving, moving)
			.TagTransition(GameTags.LongRangeMissileIdle, idle)
			.TagTransition(GameTags.LongRangeMissileExploding, exploding);
		moving.Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("inflight_loop", KAnim.PlayMode.Loop);
		});
		idle.Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("idle_loop", KAnim.PlayMode.Loop);
		});
		exploding.DefaultState(exploding.pre);
		exploding.pre.ScheduleGoTo(10f, exploding.animating).EventTransition(GameHashes.ClusterMapTravelAnimatorMoveComplete, exploding.animating);
		exploding.animating.Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("explode", KAnim.PlayMode.Once);
			smi.SubscribeOnVisAnimComplete(delegate
			{
				smi.GoTo(exploding.post);
			});
		});
		exploding.post.Enter(delegate(StatesInstance smi)
		{
			if (smi.entity != null)
			{
				smi.entity.Trigger(-1311384361);
			}
			smi.GoTo((BaseState)null);
		});
	}
}
