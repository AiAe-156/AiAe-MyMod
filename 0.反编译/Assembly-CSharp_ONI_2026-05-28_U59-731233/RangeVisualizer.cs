using System;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/RangeVisualizer")]
public class RangeVisualizer : KMonoBehaviour
{
	public Vector2I OriginOffset;

	public Vector2I RangeMin;

	public Vector2I RangeMax;

	public Vector2I TexSize = new Vector2I(64, 64);

	public bool TestLineOfSight = true;

	public bool BlockingTileVisible = false;

	public Func<int, bool> BlockingVisibleCb = null;

	public Func<int, bool> BlockingCb = Grid.IsSolidCell;

	public bool AllowLineOfSightInvalidCells = false;
}
