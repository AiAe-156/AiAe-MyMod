using UnityEngine;

public class SpaceTreeSeededComet : Comet
{
	protected override void DepositTiles(int cell, Element element, int world, int prev_cell, float temperature)
	{
		int depthOfElement = GetDepthOfElement(cell, element, world);
		float num = 1f;
		float num2 = (float)(depthOfElement - addTilesMinHeight) / (float)(addTilesMaxHeight - addTilesMinHeight);
		if (!float.IsNaN(num2))
		{
			num -= num2;
		}
		int num3 = Mathf.Min(addTiles, Mathf.Clamp(Mathf.RoundToInt((float)addTiles * num), 1, addTiles));
		ListPool<int, Comet>.PooledList pooledList = ListPool<int, Comet>.Allocate();
		FloodFill.BreadthCollect(prev_cell, condition, pooledList, 11);
		float mass = ((num3 > 0) ? (addTileMass / (float)addTiles) : 1f);
		int disease_count = addDiseaseCount / num3;
		float value = Random.value;
		float num4 = ((num3 == 0) ? (-1f) : (1f / (float)num3));
		float num5 = 0f;
		bool flag = false;
		foreach (int viable_cell in pooledList)
		{
			if (num3 <= 0)
			{
				break;
			}
			num5 += num4;
			bool flag2 = !flag && num4 >= 0f && value <= num5;
			int callbackIdx = (flag2 ? Game.Instance.callbackManager.Add(new Game.CallbackInfo(delegate
			{
				PlantTreeOnSolidTileCreated(viable_cell, addTilesMaxHeight);
			})).index : (-1));
			SimMessages.AddRemoveSubstance(viable_cell, element.id, CellEventLogger.Instance.ElementEmitted, mass, temperature, diseaseIdx, disease_count, do_vertical_solid_displacement: true, callbackIdx);
			num3--;
			flag = flag || flag2;
		}
		pooledList.Recycle();
		FloodFill.BoundaryCheckResult condition(int num6)
		{
			if (!Grid.IsValidCellInWorld(num6, world) || Grid.Solid[num6])
			{
				return FloodFill.BoundaryCheckResult.Halt;
			}
			return FloodFill.BoundaryCheckResult.Continue;
		}
	}

	private static void PlantTreeOnSolidTileCreated(int cell, int tileMaxHeight)
	{
		byte worldIdx = Grid.WorldIdx[cell];
		int num = 2;
		int num2 = Grid.OffsetCell(cell, new CellOffset(0, tileMaxHeight));
		int num3 = num2;
		bool flag = false;
		if (!Grid.IsValidCell(cell))
		{
			return;
		}
		do
		{
			num2 = num3;
			num3 = Grid.OffsetCell(num2, 0, -1);
			if (!Grid.IsValidCell(num3))
			{
				return;
			}
			if (Grid.Solid[num3] && CanGrowOnCell(num2, worldIdx))
			{
				flag = true;
			}
			num--;
		}
		while (!flag && num > 0);
		if (flag)
		{
			GameObject prefab = Assets.GetPrefab("SpaceTree");
			KBatchedAnimController component = prefab.GetComponent<KBatchedAnimController>();
			Vector3 position = Grid.CellToPosCBC(num2, component.sceneLayer);
			Util.KInstantiate(prefab, position).SetActive(value: true);
		}
	}

	public static bool CanGrowOnCell(int spawnCell, byte worldIdx)
	{
		CellOffset[] occupiedCellsOffsets = Assets.GetPrefab("SpaceTree").GetComponent<OccupyArea>().OccupiedCellsOffsets;
		bool flag = true;
		int num = 0;
		while (flag && num < occupiedCellsOffsets.Length)
		{
			int num2 = Grid.OffsetCell(spawnCell, occupiedCellsOffsets[num]);
			flag = flag && Grid.IsValidCellInWorld(num2, worldIdx) && (!Grid.IsSolidCell(num2) || Grid.Element[num2].HasTag(GameTags.Unstable)) && Grid.Objects[num2, 1] == null && Grid.Objects[num2, 5] == null && !Grid.Foundation[num2];
			num++;
		}
		return flag;
	}
}
