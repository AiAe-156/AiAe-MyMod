public class BuildingPointStraw : KMonoBehaviour
{
	public const string SYMBOL_PREFIX = "straw";

	public const string ANIM_PREFIX = "on";

	public bool canControlAnimStates;

	public bool usesSymbols;

	public bool isInLiquid;

	public int maxDepth = 4;

	private int depthAvailable = -1;

	private HandleVector<int>.Handle partitionerEntry_solids;

	private HandleVector<int>.Handle partitionerEntry_buildings;

	private HandleVector<int>.Handle partitionerEntry_liquid;

	public int currentDepth => depthAvailable;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		RefreshDepthAvailable();
		RegisterListenersToCellChanges();
	}

	protected override void OnCleanUp()
	{
		UnregisterListenersToCellChanges();
		base.OnCleanUp();
	}

	public int GetDepthOffset()
	{
		if (depthAvailable <= 0)
		{
			return -1;
		}
		return -depthAvailable;
	}

	public string GetSymbolSuffix()
	{
		if (depthAvailable <= 0)
		{
			return "1";
		}
		return depthAvailable.ToString();
	}

	public string GetAnimSuffix()
	{
		if (depthAvailable <= 0)
		{
			return "_1";
		}
		return "_" + depthAvailable;
	}

	public CellOffset GetBottomCellOffset()
	{
		int depthOffset = GetDepthOffset();
		return new CellOffset(0, depthOffset);
	}

	public int GetStrawCell()
	{
		return Grid.OffsetCell(Grid.PosToCell(base.gameObject), GetBottomCellOffset());
	}

	private int GetDepthAvailable()
	{
		int cell = Grid.PosToCell(this);
		int result = 0;
		bool flag = false;
		for (int i = 1; i <= maxDepth; i++)
		{
			int num = Grid.OffsetCell(cell, 0, -i);
			if (!Grid.IsValidCell(num) || Grid.Solid[num] || (Grid.ObjectLayers[1].ContainsKey(num) && Grid.ObjectLayers[1][num] != null && Grid.ObjectLayers[1][num] != base.gameObject))
			{
				break;
			}
			result = i;
			if (Grid.IsLiquid(num))
			{
				if (flag)
				{
					break;
				}
				flag = true;
			}
		}
		return result;
	}

	private void RefreshDepthAvailable()
	{
		int num = GetDepthAvailable();
		bool flag = num != depthAvailable;
		depthAvailable = num;
		bool flag2 = Grid.IsLiquid(GetStrawCell());
		bool num2 = isInLiquid != flag2;
		isInLiquid = flag2;
		if (num2 || flag)
		{
			RefreshAnims();
			Trigger(360192579, (object)this);
		}
	}

	private void RefreshAnims()
	{
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		if (usesSymbols)
		{
			for (int i = 1; i <= maxDepth; i++)
			{
				string text = "straw" + i;
				bool is_visible = i <= depthAvailable;
				component.SetSymbolVisiblity(text, is_visible);
			}
		}
		else if (canControlAnimStates)
		{
			string text2 = ((depthAvailable > 0) ? ("_" + depthAvailable) : "_1");
			string text3 = "on" + text2;
			component.Play(text3);
		}
	}

	private void OnCellChanged(object data)
	{
		RefreshDepthAvailable();
	}

	private void RegisterListenersToCellChanges()
	{
		CellOffset[] array = new CellOffset[maxDepth];
		for (int i = 0; i < maxDepth; i++)
		{
			array[i] = new CellOffset(0, -(i + 1));
		}
		Extents extents = new Extents(Grid.PosToCell(base.transform.GetPosition()), array);
		partitionerEntry_solids = GameScenePartitioner.Instance.Add("FishDeliveryPointStraw", base.gameObject, extents, GameScenePartitioner.Instance.solidChangedLayer, OnCellChanged);
		partitionerEntry_buildings = GameScenePartitioner.Instance.Add("FishDeliveryPointStraw", base.gameObject, extents, GameScenePartitioner.Instance.objectLayers[1], OnCellChanged);
		partitionerEntry_liquid = GameScenePartitioner.Instance.Add("FishDeliveryPointStraw", base.gameObject, extents, GameScenePartitioner.Instance.liquidChangedLayer, OnCellChanged);
	}

	private void UnregisterListenersToCellChanges()
	{
		GameScenePartitioner.Instance.Free(ref partitionerEntry_solids);
		GameScenePartitioner.Instance.Free(ref partitionerEntry_buildings);
		GameScenePartitioner.Instance.Free(ref partitionerEntry_liquid);
	}
}
