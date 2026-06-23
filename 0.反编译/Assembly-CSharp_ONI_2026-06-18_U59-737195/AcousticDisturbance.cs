using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class AcousticDisturbance
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct CellCollector : FloodFill.IVisitor
	{
		public bool EarlyOut => false;

		public void VisitCell(int cell)
		{
			cellsInRange.Add(cell);
		}

		public void VisitBoundary(int cell)
		{
		}
	}

	private static readonly HashedString[] PreAnims = new HashedString[2] { "grid_pre", "grid_loop" };

	private static readonly HashedString PostAnim = "grid_pst";

	private static float distanceDelay = 0.25f;

	private static float duration = 3f;

	private static readonly Func<int, FloodFill.BoundaryCheckResult> notSolid = (int cell) => Grid.Solid[cell] ? FloodFill.BoundaryCheckResult.Halt : FloodFill.BoundaryCheckResult.Continue;

	private static readonly HybridListHashSet<int> cellsInRange = new HybridListHashSet<int>();

	public static void Emit(object data, int EmissionRadius)
	{
		GameObject gameObject = (GameObject)data;
		Components.Cmps<MinionIdentity> liveMinionIdentities = Components.LiveMinionIdentities;
		Vector2 vector = gameObject.transform.GetPosition();
		int num = Grid.PosToCell(vector);
		int num2 = EmissionRadius * EmissionRadius;
		cellsInRange.Clear();
		FloodFill.DepthTraverse(num, new FloodFill.PredicateCondition(notSolid), FloodFill.HashSetVisitTracker.Default(), new FloodFill.MaxDepth(EmissionRadius), default(CellCollector));
		DrawVisualEffect(num, cellsInRange);
		for (int i = 0; i < liveMinionIdentities.Count; i++)
		{
			MinionIdentity minionIdentity = liveMinionIdentities[i];
			if (minionIdentity.gameObject == gameObject.gameObject)
			{
				continue;
			}
			Vector2 vector2 = minionIdentity.transform.GetPosition();
			if (Vector2.SqrMagnitude(vector - vector2) > (float)num2)
			{
				continue;
			}
			int data2 = Grid.PosToCell(vector2);
			if (cellsInRange.Contains(data2))
			{
				StaminaMonitor.Instance sMI = minionIdentity.GetSMI<StaminaMonitor.Instance>();
				if (sMI != null && sMI.IsSleeping())
				{
					minionIdentity.Trigger(-527751701, data);
					minionIdentity.Trigger(1621815900, data);
				}
			}
		}
	}

	private static void DrawVisualEffect(int center_cell, HybridListHashSet<int> cells)
	{
		SoundEvent.PlayOneShot(GlobalResources.Instance().AcousticDisturbanceSound, Grid.CellToPos(center_cell));
		for (int i = 0; i != cells.Count; i++)
		{
			int num = cells[i];
			int gridDistance = GetGridDistance(num, center_cell);
			GameScheduler.Instance.Schedule("radialgrid_pre", distanceDelay * (float)gridDistance, SpawnEffect, num);
		}
	}

	private static void SpawnEffect(object data)
	{
		Grid.SceneLayer layer = Grid.SceneLayer.InteriorWall;
		int cell = (int)data;
		KBatchedAnimController kBatchedAnimController = FXHelpers.CreateEffect("radialgrid_kanim", Grid.CellToPosCCC(cell, layer), null, update_looping_sounds_position: false, layer);
		kBatchedAnimController.destroyOnAnimComplete = false;
		kBatchedAnimController.Play(PreAnims, KAnim.PlayMode.Loop);
		GameScheduler.Instance.Schedule("radialgrid_loop", duration, DestroyEffect, kBatchedAnimController);
	}

	private static void DestroyEffect(object data)
	{
		KBatchedAnimController obj = (KBatchedAnimController)data;
		obj.destroyOnAnimComplete = true;
		obj.Play(PostAnim);
	}

	private static int GetGridDistance(int cell, int center_cell)
	{
		Vector2I vector2I = Grid.CellToXY(cell);
		Vector2I vector2I2 = Grid.CellToXY(center_cell);
		Vector2I vector2I3 = vector2I - vector2I2;
		return Math.Abs(vector2I3.x) + Math.Abs(vector2I3.y);
	}
}
