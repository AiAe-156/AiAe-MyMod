using System;
using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

public class BaggableCritterCapacityTracker : KMonoBehaviour, ISim1000ms, IUserControlledCapacity
{
	public int maximumCreatures = 40;

	public bool requireLiquidOffset = false;

	public CellOffset cavityOffset;

	public bool filteredCount;

	public System.Action onCountChanged;

	private int cavityCell;

	[MyCmpReq]
	private TreeFilterable filter;

	[MyCmpGet]
	private Operational operational;

	private static readonly Operational.Flag isInLiquid = new Operational.Flag("isInLiquid", Operational.Flag.Type.Requirement);

	[MyCmpGet]
	private KSelectable selectable;

	private static StatusItem capacityStatusItem;

	private HandleVector<int>.Handle partitionerEntry;

	[Serialize]
	public int creatureLimit { get; set; } = 20;

	public int storedCreatureCount { get; private set; }

	float IUserControlledCapacity.UserMaxCapacity
	{
		get
		{
			return creatureLimit;
		}
		set
		{
			creatureLimit = Mathf.RoundToInt(value);
			if (onCountChanged != null)
			{
				onCountChanged();
			}
		}
	}

	float IUserControlledCapacity.AmountStored => storedCreatureCount;

	float IUserControlledCapacity.MinCapacity => 0f;

	float IUserControlledCapacity.MaxCapacity => maximumCreatures;

	bool IUserControlledCapacity.WholeValues => true;

	LocString IUserControlledCapacity.CapacityUnits => UI.UISIDESCREENS.CAPTURE_POINT_SIDE_SCREEN.UNITS_SUFFIX;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		int cell = Grid.PosToCell(this);
		cavityCell = Grid.OffsetCell(cell, cavityOffset);
		filter = GetComponent<TreeFilterable>();
		TreeFilterable treeFilterable = filter;
		treeFilterable.OnFilterChanged = (Action<HashSet<Tag>>)Delegate.Combine(treeFilterable.OnFilterChanged, new Action<HashSet<Tag>>(RefreshCreatureCount));
		Subscribe(-905833192, OnCopySettings);
		if (requireLiquidOffset)
		{
			partitionerEntry = GameScenePartitioner.Instance.Add("BaggableCritterCapacityTracker.OnSpawn", base.gameObject, new Extents(cavityCell, new CellOffset[1]
			{
				new CellOffset(0, 0)
			}), GameScenePartitioner.Instance.liquidChangedLayer, OnLiquidChanged);
			OnLiquidChanged(null);
		}
		else
		{
			Subscribe(144050788, RefreshCreatureCount);
		}
	}

	private void OnBuildingStrawChanged(object o)
	{
		BuildingPointStraw buildingPointStraw = (BuildingPointStraw)o;
		UpdateCavityCell(buildingPointStraw.GetBottomCellOffset());
	}

	public void UpdateCavityCell(CellOffset newOffset)
	{
		cavityOffset = newOffset;
		int cell = Grid.PosToCell(this);
		cavityCell = Grid.OffsetCell(cell, cavityOffset);
		if (requireLiquidOffset)
		{
			GameScenePartitioner.Instance.Free(ref partitionerEntry);
			partitionerEntry = GameScenePartitioner.Instance.Add("BaggableCritterCapacityTracker.UpdateCavityCell", base.gameObject, new Extents(cavityCell, new CellOffset[1]
			{
				new CellOffset(0, 0)
			}), GameScenePartitioner.Instance.liquidChangedLayer, OnLiquidChanged);
			OnLiquidChanged(null);
		}
		RefreshCreatureCount();
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		if (capacityStatusItem == null)
		{
			capacityStatusItem = new StatusItem("CritterCapacity", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
			capacityStatusItem.resolveStringCallback = delegate(string str, object data)
			{
				IUserControlledCapacity userControlledCapacity = (IUserControlledCapacity)data;
				string newValue = Util.FormatWholeNumber(Mathf.Floor(userControlledCapacity.AmountStored));
				float userMaxCapacity = userControlledCapacity.UserMaxCapacity;
				string newValue2 = Util.FormatWholeNumber(userMaxCapacity);
				str = str.Replace("{Stored}", newValue).Replace("{StoredUnits}", ((int)userControlledCapacity.AmountStored == 1) ? BUILDING.STATUSITEMS.CRITTERCAPACITY.UNIT : BUILDING.STATUSITEMS.CRITTERCAPACITY.UNITS).Replace("{Capacity}", newValue2)
					.Replace("{CapacityUnits}", ((int)userControlledCapacity.UserMaxCapacity == 1) ? BUILDING.STATUSITEMS.CRITTERCAPACITY.UNIT : BUILDING.STATUSITEMS.CRITTERCAPACITY.UNITS);
				return str;
			};
		}
		selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, capacityStatusItem, this);
		Subscribe(360192579, OnBuildingStrawChanged);
	}

	protected override void OnCleanUp()
	{
		if (requireLiquidOffset)
		{
			GameScenePartitioner.Instance.Free(ref partitionerEntry);
		}
		TreeFilterable treeFilterable = filter;
		treeFilterable.OnFilterChanged = (Action<HashSet<Tag>>)Delegate.Remove(treeFilterable.OnFilterChanged, new Action<HashSet<Tag>>(RefreshCreatureCount));
		Unsubscribe(144050788);
		base.OnCleanUp();
	}

	private void OnLiquidChanged(object data)
	{
		if (requireLiquidOffset)
		{
			bool flag = Grid.IsLiquid(cavityCell);
			if (flag)
			{
				RefreshCreatureCount();
			}
			operational.SetFlag(isInLiquid, flag);
			selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NotSubmerged, !flag, this);
			selectable.ToggleStatusItem(capacityStatusItem, flag, this);
		}
	}

	private void OnCopySettings(object data)
	{
		GameObject gameObject = (GameObject)data;
		if (!(gameObject == null))
		{
			BaggableCritterCapacityTracker component = gameObject.GetComponent<BaggableCritterCapacityTracker>();
			if (!(component == null))
			{
				creatureLimit = component.creatureLimit;
			}
		}
	}

	public void RefreshCreatureCount(object data = null)
	{
		int num = storedCreatureCount;
		if (requireLiquidOffset)
		{
			storedCreatureCount = RefreshSwimmingCreatureCount();
		}
		else
		{
			storedCreatureCount = RefreshOtherCreatureCount();
		}
		if (onCountChanged != null && storedCreatureCount != num)
		{
			onCountChanged();
		}
	}

	private int RefreshOtherCreatureCount()
	{
		int num = 0;
		CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(cavityCell);
		if (cavityForCell != null)
		{
			foreach (KPrefabID creature in cavityForCell.creatures)
			{
				if (!creature.HasTag(GameTags.Creatures.Bagged) && !creature.HasTag(GameTags.Trapped) && (!filteredCount || filter.AcceptedTags.Contains(creature.PrefabTag)))
				{
					num++;
				}
			}
		}
		return num;
	}

	private int RefreshSwimmingCreatureCount()
	{
		return FishOvercrowingManager.Instance.GetFishInPondCount(cavityCell, filter.AcceptedTags);
	}

	public void Sim1000ms(float dt)
	{
		RefreshCreatureCount();
	}
}
