using System;
using System.Collections.Generic;
using UnityEngine;

public class BipedSwimTransitionLayer : TransitionDriver.OverrideLayer
{
	private Vector3 offset;

	private KBatchedAnimController animcontroller;

	private bool lerpingOffset;

	private float startOffsetY;

	private float targetOffsetY;

	private Vector3 startPos;

	private Vector3 endPos;

	public BipedSwimTransitionLayer(Navigator navigator)
		: base(navigator)
	{
		animcontroller = navigator.GetComponent<KBatchedAnimController>();
	}

	public override void BeginTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		base.BeginTransition(navigator, transition);
		lerpingOffset = false;
		int cell = Grid.CellAbove(navigator.cachedCell);
		bool flag = Grid.IsWorldValidCell(cell) && !Grid.IsLiquid(cell);
		bool num = transition.start == NavType.Swim && transition.end == NavType.Swim;
		bool flag2 = transition.x != 0 && transition.y != 0;
		bool flag3 = transition.x != 0 && transition.y == 0;
		Dictionary<HashedString, HashedString> value;
		HashedString value2;
		if (num && flag3 && flag)
		{
			transition.anim = "shallow_swim_1_0_loop";
			transition.isLooping = true;
			SetupOffsets(navigator, transition);
		}
		else if (animcontroller.currentAnim != transition.anim && SwimMonitor.transitionAnims.TryGetValue(animcontroller.currentAnim, out value) && value.TryGetValue(transition.anim, out value2))
		{
			transition.preAnim = value2;
		}
		if (num && !transition.isLooping)
		{
			if (transition.speed > 0f)
			{
				transition.animSpeed = transition.speed;
			}
			if (flag2)
			{
				transition.animSpeed *= 0.9f;
			}
		}
	}

	public override void UpdateTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		if (transition.start == NavType.Swim && transition.end == NavType.Swim && lerpingOffset)
		{
			Vector3 position = navigator.transform.GetPosition();
			float num = Vector3.Distance(startPos, endPos);
			float num2 = Vector3.Distance(startPos, position);
			float t = ((num > 0f) ? Mathf.Clamp01(num2 / num) : 1f);
			offset.y = Mathf.Lerp(startOffsetY, targetOffsetY, t);
			if (MathF.Abs(offset.y - animcontroller.Offset.y) > SwimMonitor.OffsetEpsilon)
			{
				animcontroller.Offset = offset;
			}
		}
		base.UpdateTransition(navigator, transition);
	}

	private void SetupOffsets(Navigator navigator, Navigator.ActiveTransition transition)
	{
		int cachedCell = navigator.cachedCell;
		int cell = Grid.OffsetCell(cachedCell, transition.x, transition.y);
		startOffsetY = SwimMonitor.ComputeSwimOffsetY(cachedCell);
		targetOffsetY = (IsSurfaceSwimCell(cell) ? SwimMonitor.ComputeSwimOffsetY(cell) : 0f);
		startPos = navigator.transform.GetPosition();
		endPos = Grid.CellToPosCBC(cell, Grid.SceneLayer.Move);
		lerpingOffset = true;
		offset.y = startOffsetY;
		animcontroller.Offset = offset;
	}

	private static bool IsSurfaceSwimCell(int cell)
	{
		if (!Grid.IsWorldValidCell(cell) || !Grid.IsLiquid(cell))
		{
			return false;
		}
		int cell2 = Grid.CellAbove(cell);
		if (Grid.IsWorldValidCell(cell2))
		{
			return !Grid.IsLiquid(cell2);
		}
		return false;
	}

	public override void EndTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		lerpingOffset = false;
		base.EndTransition(navigator, transition);
		if (MathF.Abs(animcontroller.Offset.y) > SwimMonitor.OffsetEpsilon)
		{
			offset = Vector3.zero;
			animcontroller.Offset = offset;
		}
	}
}
