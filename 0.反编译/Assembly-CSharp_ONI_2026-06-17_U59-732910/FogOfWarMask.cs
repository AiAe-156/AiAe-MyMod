using System;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/FogOfWarMask")]
public class FogOfWarMask : KMonoBehaviour
{
	private struct ThresholdVisitor : FloodFill.IVisitor
	{
		private int threshold;

		public readonly bool EarlyOut => threshold <= 0;

		public ThresholdVisitor(int threshold)
		{
			this.threshold = threshold;
		}

		public void VisitCell(int cell)
		{
			threshold--;
		}

		public readonly void VisitBoundary(int cell)
		{
		}
	}

	private static readonly Func<int, FloodFill.BoundaryCheckResult> revealFogOfWarMask = RevealFogOfWarMask;

	protected override void OnSpawn()
	{
		Debug.Assert(condition: false, "Unmaintained, presumed dead, code is being invoked!");
	}

	protected override void OnCmpEnable()
	{
		Debug.Assert(condition: false, "Unmaintained, presumed dead, code is being invoked!");
	}

	public static void ClearMask(int cell)
	{
		FloodFill.BreadthTraverse(cell, new FloodFill.PredicateCondition(revealFogOfWarMask), FloodFill.HashSetVisitTracker.Default(), default(FloodFill.NoMaxDepth), new ThresholdVisitor(300));
	}

	public static FloodFill.BoundaryCheckResult RevealFogOfWarMask(int cell)
	{
		if (Grid.PreventFogOfWarReveal[cell])
		{
			Grid.PreventFogOfWarReveal[cell] = false;
			Grid.Reveal(cell);
			return FloodFill.BoundaryCheckResult.Continue;
		}
		return FloodFill.BoundaryCheckResult.Halt;
	}
}
