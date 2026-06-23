#define DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class TransitionDriver
{
	public class OverrideLayer
	{
		public OverrideLayer(Navigator navigator)
		{
		}

		public virtual void Destroy()
		{
		}

		public virtual void BeginTransition(Navigator navigator, Navigator.ActiveTransition transition)
		{
		}

		public virtual void UpdateTransition(Navigator navigator, Navigator.ActiveTransition transition)
		{
		}

		public virtual void EndTransition(Navigator navigator, Navigator.ActiveTransition transition)
		{
		}
	}

	public class InterruptOverrideLayer : OverrideLayer
	{
		protected Navigator.ActiveTransition originalTransition = null;

		protected TransitionDriver driver = null;

		protected bool InterruptInProgress => originalTransition != null;

		public InterruptOverrideLayer(Navigator navigator)
			: base(navigator)
		{
			driver = navigator.transitionDriver;
		}

		public override void BeginTransition(Navigator navigator, Navigator.ActiveTransition transition)
		{
			driver.interruptOverrideStack.Push(this);
			originalTransition = SwapTransitionWithEmpty(transition);
		}

		public override void UpdateTransition(Navigator navigator, Navigator.ActiveTransition transition)
		{
			if (IsOverrideComplete())
			{
				driver.interruptOverrideStack.Pop();
				transition.Copy(originalTransition);
				DevOnly.Assert(originalTransition != null, "Releasing a null transition to the pool");
				TransitionPool.Release(originalTransition);
				originalTransition = null;
				EndTransition(navigator, transition);
				driver.BeginTransition(navigator, transition);
			}
		}

		public override void EndTransition(Navigator navigator, Navigator.ActiveTransition transition)
		{
			base.EndTransition(navigator, transition);
			if (originalTransition != null)
			{
				TransitionPool.Release(originalTransition);
				originalTransition = null;
			}
		}

		protected virtual bool IsOverrideComplete()
		{
			if (originalTransition == null)
			{
				return false;
			}
			return driver.interruptOverrideStack.Count != 0 && driver.interruptOverrideStack.Peek() == this;
		}
	}

	private static Navigator.ActiveTransition emptyTransition = new Navigator.ActiveTransition();

	public static ObjectPool<Navigator.ActiveTransition> TransitionPool = new ObjectPool<Navigator.ActiveTransition>(() => new Navigator.ActiveTransition(), null, null, null, collectionCheck: false, 128);

	private Stack<InterruptOverrideLayer> interruptOverrideStack = new Stack<InterruptOverrideLayer>(8);

	private Navigator.ActiveTransition transition;

	private Navigator navigator;

	private Vector3 targetPos;

	private bool isComplete = false;

	private Brain brain;

	public List<OverrideLayer> overrideLayers = new List<OverrideLayer>();

	private LoggerFS log;

	private Action<object> onAnimComplete_ = null;

	private int onAnimCompleteHandle = -1;

	private Action<object> onAnimCompleteBinding
	{
		get
		{
			if (onAnimComplete_ == null)
			{
				onAnimComplete_ = OnAnimComplete;
			}
			return onAnimComplete_;
		}
	}

	public Navigator.ActiveTransition GetTransition => transition;

	public TransitionDriver(Navigator navigator)
	{
		log = new LoggerFS("TransitionDriver");
	}

	public void BeginTransition(Navigator navigator, NavGrid.Transition transition, float defaultSpeed)
	{
		Navigator.ActiveTransition activeTransition = TransitionPool.Get();
		activeTransition.Init(transition, defaultSpeed);
		BeginTransition(navigator, activeTransition);
	}

	private void BeginTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		bool flag = interruptOverrideStack.Count != 0;
		foreach (OverrideLayer overrideLayer in overrideLayers)
		{
			if (!flag || !(overrideLayer is InterruptOverrideLayer))
			{
				overrideLayer.BeginTransition(navigator, transition);
			}
		}
		this.navigator = navigator;
		this.transition = transition;
		isComplete = false;
		Grid.SceneLayer sceneLayer = navigator.sceneLayer;
		if (transition.navGridTransition.start == NavType.Tube || transition.navGridTransition.end == NavType.Tube)
		{
			sceneLayer = Grid.SceneLayer.BuildingUse;
		}
		else if (transition.navGridTransition.start == NavType.Solid && transition.navGridTransition.end == NavType.Solid)
		{
			sceneLayer = Grid.SceneLayer.FXFront;
			navigator.animController.SetSceneLayer(sceneLayer);
		}
		else if (transition.navGridTransition.start == NavType.Solid || transition.navGridTransition.end == NavType.Solid)
		{
			navigator.animController.SetSceneLayer(sceneLayer);
		}
		int cell = Grid.PosToCell(navigator);
		int target_cell = Grid.OffsetCell(cell, transition.x, transition.y);
		targetPos = GetTargetPosition(transition.navGridTransition, target_cell, sceneLayer);
		KAnimControllerBase animController = navigator.animController;
		animController.PlaySpeedMultiplier = transition.animSpeed;
		if (transition.isLooping)
		{
			bool flag2 = transition.preAnim != "";
			bool flag3 = animController.CurrentAnim != null && animController.CurrentAnim.name == transition.anim;
			if (flag2 && animController.CurrentAnim != null && animController.CurrentAnim.name == transition.preAnim)
			{
				animController.ClearQueue();
				animController.Queue(transition.anim, KAnim.PlayMode.Loop);
			}
			else if (flag3)
			{
				if (animController.PlayMode != KAnim.PlayMode.Loop)
				{
					animController.ClearQueue();
					animController.Queue(transition.anim, KAnim.PlayMode.Loop);
				}
			}
			else if (flag2)
			{
				animController.Play(transition.preAnim);
				animController.Queue(transition.anim, KAnim.PlayMode.Loop);
			}
			else
			{
				animController.Play(transition.anim, KAnim.PlayMode.Loop);
			}
		}
		else if (transition.anim != null)
		{
			if (transition.preAnim != null)
			{
				animController.Play(transition.preAnim);
				animController.Queue(transition.anim);
			}
			else
			{
				animController.Play(transition.anim);
			}
			navigator.Unsubscribe(onAnimCompleteHandle);
			onAnimCompleteHandle = navigator.Subscribe(-1061186183, onAnimCompleteBinding);
		}
		if (transition.navGridTransition.y != 0)
		{
			if (transition.navGridTransition.start == NavType.RightWall)
			{
				navigator.facing.SetFacing(transition.navGridTransition.y < 0);
			}
			else if (transition.navGridTransition.start == NavType.LeftWall)
			{
				navigator.facing.SetFacing(transition.navGridTransition.y > 0);
			}
		}
		if (transition.navGridTransition.x != 0)
		{
			if (transition.navGridTransition.start == NavType.Ceiling)
			{
				navigator.facing.SetFacing(transition.navGridTransition.x > 0);
			}
			else if (transition.navGridTransition.start != NavType.LeftWall && transition.navGridTransition.start != NavType.RightWall)
			{
				navigator.facing.SetFacing(transition.navGridTransition.x < 0);
			}
		}
		brain = navigator.GetComponent<Brain>();
	}

	private Vector3 GetTargetPosition(NavGrid.Transition trans, int target_cell, Grid.SceneLayer layer)
	{
		if (trans.useXOffset)
		{
			if (trans.x < 0)
			{
				return Grid.CellToPosRBC(target_cell, layer);
			}
			if (trans.x > 0)
			{
				return Grid.CellToPosLBC(target_cell, layer);
			}
		}
		return Grid.CellToPosCBC(target_cell, layer);
	}

	public void UpdateTransition(float dt)
	{
		if (this.navigator == null)
		{
			return;
		}
		foreach (OverrideLayer overrideLayer in overrideLayers)
		{
			bool flag = interruptOverrideStack.Count != 0;
			bool flag2 = overrideLayer is InterruptOverrideLayer;
			if (!(flag && flag2) || interruptOverrideStack.Peek() == overrideLayer)
			{
				overrideLayer.UpdateTransition(this.navigator, transition);
			}
		}
		if (!(brain != null) || isComplete)
		{
		}
		if (transition.isLooping)
		{
			float speed = transition.speed;
			Vector3 position = this.navigator.transform.GetPosition();
			int num = Grid.PosToCell(position);
			if (transition.x > 0)
			{
				position.x += dt * speed;
				if (position.x > targetPos.x)
				{
					isComplete = true;
				}
			}
			else if (transition.x < 0)
			{
				position.x -= dt * speed;
				if (position.x < targetPos.x)
				{
					isComplete = true;
				}
			}
			else
			{
				position.x = targetPos.x;
			}
			if (transition.y > 0)
			{
				position.y += dt * speed;
				if (position.y > targetPos.y)
				{
					isComplete = true;
				}
			}
			else if (transition.y < 0)
			{
				position.y -= dt * speed;
				if (position.y < targetPos.y)
				{
					isComplete = true;
				}
			}
			else
			{
				position.y = targetPos.y;
			}
			this.navigator.transform.SetPosition(position);
			int num2 = Grid.PosToCell(position);
			if (num2 != num)
			{
				this.navigator.BoxingTrigger(915392638, num2);
			}
		}
		if (isComplete)
		{
			isComplete = false;
			Navigator navigator = this.navigator;
			navigator.SetCurrentNavType(transition.end);
			navigator.transform.SetPosition(targetPos);
			EndTransition();
			navigator.AdvancePath();
		}
	}

	public void EndTransition()
	{
		if (!(navigator != null))
		{
			return;
		}
		interruptOverrideStack.Clear();
		foreach (OverrideLayer overrideLayer in overrideLayers)
		{
			overrideLayer.EndTransition(navigator, transition);
		}
		navigator.animController.PlaySpeedMultiplier = 1f;
		navigator.Unsubscribe(ref onAnimCompleteHandle);
		if (brain != null)
		{
			brain.Resume("move_handler");
		}
		DevOnly.Assert(transition != null, "Releasing a null ActiveTransition to the pool");
		TransitionPool.Release(transition);
		transition = null;
		navigator = null;
		brain = null;
	}

	private void OnAnimComplete(object data)
	{
		if (navigator != null)
		{
			navigator.Unsubscribe(ref onAnimCompleteHandle);
		}
		isComplete = true;
	}

	public static Navigator.ActiveTransition SwapTransitionWithEmpty(Navigator.ActiveTransition src)
	{
		Navigator.ActiveTransition activeTransition = TransitionPool.Get();
		activeTransition.Copy(src);
		src.Copy(emptyTransition);
		return activeTransition;
	}
}
