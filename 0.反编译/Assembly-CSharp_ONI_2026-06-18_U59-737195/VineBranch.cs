using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class VineBranch : PlantBranchGrowerBase<VineBranch, VineBranch.Instance, IStateMachineTarget, VineBranch.Def>
{
	public class AnimSet
	{
		public string suffix;

		private const int WILT_LEVELS = 3;

		public string pre_grow => suffix + "pre_grow";

		public string grow => suffix + "grow";

		public string grow_pst => suffix + "grow_pst";

		public string idle => suffix + "idle";

		public string meter_target => suffix + "meter_target";

		public string meter => suffix + "meter";

		public string meter_wilted => suffix + "meter_wilted";

		public string meter_harvest => suffix + "meter_harvest";

		public string meter_harvest_ready => suffix + "meter_harvest_ready";

		private string wilted => suffix + "wilted";

		public int GetWiltLevel(float growthPercentage)
		{
			if (growthPercentage < 0.75f)
			{
				return 1;
			}
			if (growthPercentage < 1f)
			{
				return 2;
			}
			return 3;
		}

		public string GetBaseWiltAnim(int level)
		{
			return GetWiltAnim(wilted, level);
		}

		public string GetMeterWiltAnim(int level)
		{
			return GetWiltAnim(meter_wilted, level);
		}

		private string GetWiltAnim(string wiltName, int level)
		{
			return wiltName + level;
		}

		public AnimSet(string suffix)
		{
			this.suffix = suffix;
		}
	}

	public enum ShapeCategory
	{
		Line,
		InCorner,
		OutCorner,
		DeadEnd
	}

	public enum Shape
	{
		Top,
		Bottom,
		Left,
		Right,
		InCornerTopLeft,
		InCornerTopRight,
		InCornerBottomLeft,
		InCornerBottomRight,
		OutCornerTopLeft,
		OutCornerTopRight,
		OutCornerBottomLeft,
		OutCornerBottomRight,
		TopEnd,
		BottomEnd,
		LeftEnd,
		RightEnd
	}

	public class Def : PlantBranchGrowerBaseDef
	{
		public float GROWTH_RATE = 0.0016666667f;

		public float WILD_GROWTH_RATE = 0.00041666668f;
	}

	public class GrowingSpeedState : State
	{
		public State wild;

		public State domestic;
	}

	public class BranchAliveSubstate : PlantAliveSubState
	{
		public State InitializeStates(TargetParameter plant, TargetParameter mother, State death_state, Signal dieSignal)
		{
			InitializeStates(plant, death_state);
			base.root.Target(plant).OnSignal(dieSignal, death_state).OnTargetLost(mother, death_state)
				.Target(mother)
				.EventHandler(GameHashes.Wilt, OnMotherWilted)
				.EventHandler(GameHashes.WiltRecover, OnMotherRecovered)
				.Target(plant);
			return this;
		}
	}

	public class GrowingStates : BranchAliveSubstate
	{
		public GrowingSpeedState growing;

		public State wilted;
	}

	public class FruitGrowingStates : State
	{
		public GrowingSpeedState growing;

		public State wilted;

		public State harvestReady;

		public State selfHarvestFromOld;

		public State harvest;
	}

	public class GrownStates : BranchAliveSubstate
	{
		public FruitGrowingStates healthy;

		public State wilted;
	}

	public new class Instance : GameInstance, IManageGrowingStates, IWiltCause
	{
		private bool isSpawningNextBranch;

		public bool IsNewGameSpawned;

		public AttributeModifier baseGrowingRate;

		public AttributeModifier wildGrowingRate;

		public AttributeModifier baseFruitGrowingRate;

		public AttributeModifier wildFruitGrowingRate;

		public AttributeModifier getOldRate;

		public KBatchedAnimController AnimController;

		private AmountInstance maturity;

		private AmountInstance fruitMaturity;

		private AmountInstance oldAge;

		private WiltCondition wiltCondition;

		private VineMother.Instance MotherSMI;

		private UprootedMonitor uprootMonitor;

		private Harvestable harvestable;

		private MeterController fruitMeter;

		private HandleVector<int>.Handle solidPartitionerEntry = HandleVector<int>.InvalidHandle;

		private HandleVector<int>.Handle buildingsPartitionerEntry = HandleVector<int>.InvalidHandle;

		private HandleVector<int>.Handle plantsPartitionerEntry = HandleVector<int>.InvalidHandle;

		private HandleVector<int>.Handle liquidsPartitionerEntry = HandleVector<int>.InvalidHandle;

		private bool wasMarkedForDeadBeforeStartSM;

		public GameObject Mother => base.sm.Mother.Get(this);

		public GameObject Branch => base.sm.Branch.Get(this);

		public Instance BranchSMI
		{
			get
			{
				if (!(Branch == null))
				{
					return Branch.GetSMI<Instance>();
				}
				return null;
			}
		}

		public int MyBranchNumber => base.sm.BranchNumber.Get(this);

		public bool IsGrowingClockwise => base.sm.GrowingClockwise.Get(this);

		public bool IsWild => base.sm.WildPlanted.Get(this);

		public bool MaxBranchNumberReached => MyBranchNumber >= 12;

		public bool CanChangeShape
		{
			get
			{
				if (!isSpawningNextBranch && Branch == null)
				{
					return !MaxBranchNumberReached;
				}
				return false;
			}
		}

		public Shape MyShape => (Shape)base.sm.BranchShape.Get(this);

		public ShapeCategory MyShapeCategory => GetShapeCategory[MyShape];

		public Shape RootShape => (Shape)base.sm.RootShape.Get(this);

		public Direction RootDirection => (Direction)base.sm.RootDirection.Get(this);

		public AnimSet Anims => GetAnimSetByShapeCategory[MyShapeCategory];

		public bool IsOld => oldAge.value >= oldAge.GetMax();

		private bool IsMotherWilting
		{
			get
			{
				if (MotherSMI != null)
				{
					return MotherSMI.IsWilting;
				}
				return false;
			}
		}

		public bool IsWilting
		{
			get
			{
				if (!wiltCondition.IsWilting())
				{
					return IsMotherWilting;
				}
				return true;
			}
		}

		public bool IsGrown => GrowthPercentage >= 1f;

		public float GrowthPercentage => maturity.value / maturity.GetMax();

		public bool IsReadyForHarvest => FruitGrowthPercentage >= 1f;

		public float FruitGrowthPercentage => fruitMaturity.value / fruitMaturity.GetMax();

		public string WiltStateString => "    • " + DUPLICANTS.STATS.VINEMOTHERHEALTH.NAME;

		public WiltCondition.Condition[] Conditions => new WiltCondition.Condition[1] { WiltCondition.Condition.UnhealthyRoot };

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			Amounts amounts = base.gameObject.GetAmounts();
			maturity = amounts.Get(Db.Get().Amounts.Maturity);
			fruitMaturity = amounts.Get(Db.Get().Amounts.Maturity2);
			baseGrowingRate = new AttributeModifier(maturity.deltaAttribute.Id, def.GROWTH_RATE, CREATURES.STATS.MATURITY.GROWING);
			wildGrowingRate = new AttributeModifier(maturity.deltaAttribute.Id, def.WILD_GROWTH_RATE, CREATURES.STATS.MATURITY.GROWINGWILD);
			baseFruitGrowingRate = new AttributeModifier(fruitMaturity.deltaAttribute.Id, def.GROWTH_RATE, CREATURES.STATS.MATURITY.GROWING);
			wildFruitGrowingRate = new AttributeModifier(fruitMaturity.deltaAttribute.Id, def.WILD_GROWTH_RATE, CREATURES.STATS.MATURITY.GROWINGWILD);
			oldAge = amounts.Add(new AmountInstance(Db.Get().Amounts.OldAge, base.gameObject));
			oldAge.maxAttribute.ClearModifiers();
			oldAge.maxAttribute.Add(new AttributeModifier(Db.Get().Amounts.OldAge.maxAttribute.Id, 2400f));
			getOldRate = new AttributeModifier(oldAge.deltaAttribute.Id, 1f);
			wiltCondition = GetComponent<WiltCondition>();
			AnimController = GetComponent<KBatchedAnimController>();
			uprootMonitor = GetComponent<UprootedMonitor>();
			harvestable = GetComponent<Harvestable>();
			uprootMonitor.customFoundationCheckFn = IsCellFoundation;
			SetCellRegistrationAsPlant(doRegister: true);
			Subscribe(1119167081, OnSpawnedByDiscovery);
		}

		public override void StartSM()
		{
			wasMarkedForDeadBeforeStartSM = base.sm.MarkedForDeath.Get(this);
			base.master.gameObject.AddTag(GameTags.GrowingPlant);
			base.StartSM();
			SetAnimOrientation(MyShape, IsGrowingClockwise);
			Schedule(1f, DelayedResetUprootMonitor);
		}

		public override void PostParamsInitialized()
		{
			base.PostParamsInitialized();
			MotherSMI = ((Mother == null) ? null : Mother.GetSMI<VineMother.Instance>());
			if (wasMarkedForDeadBeforeStartSM)
			{
				base.sm.MarkedForDeath.Set(value: true, this);
			}
			HideAllFruitSymbols();
		}

		protected override void OnCleanUp()
		{
			DestroyFruitMeter();
			KillForwardBranch();
			SetCellRegistrationAsPlant(doRegister: false);
			base.OnCleanUp();
		}

		public void DestroySelf(object o)
		{
			CreatureHelpers.DeselectCreature(base.gameObject);
			Util.KDestroyGameObject(base.gameObject);
		}

		public void SetCellRegistrationAsPlant(bool doRegister)
		{
			int cell = Grid.PosToCell(this);
			if (doRegister && Grid.Objects[cell, 5] == null)
			{
				Grid.Objects[cell, 5] = base.gameObject;
			}
			else if (!doRegister && Grid.Objects[cell, 5] == base.gameObject)
			{
				Grid.Objects[cell, 5] = null;
			}
		}

		public void SetHarvestableState(bool canBeHarvested)
		{
			harvestable.SetCanBeHarvested(canBeHarvested);
		}

		public void SetAutoHarvestInChainReaction(bool autoharvest)
		{
			HarvestDesignatable component = GetComponent<HarvestDesignatable>();
			if (component != null)
			{
				component.SetHarvestWhenReady(autoharvest);
				if (BranchSMI != null)
				{
					BranchSMI.SetAutoHarvestInChainReaction(autoharvest);
				}
			}
		}

		public void ForceCancelHarvest()
		{
			harvestable.ForceCancelHarvest(true);
		}

		public void ResetOldAge()
		{
			oldAge.SetValue(0f);
		}

		private void OnSpawnedByDiscovery(object o)
		{
			float num = 1f - (float)MyBranchNumber / 12f;
			float num2 = ((UnityEngine.Random.Range(0f, 1f) <= num) ? 1f : UnityEngine.Random.Range(0f, 1f));
			maturity.SetValue(maturity.maxAttribute.GetTotalValue() * num2);
			if (IsGrown)
			{
				IsNewGameSpawned = true;
				fruitMaturity.SetValue(fruitMaturity.maxAttribute.GetTotalValue() * UnityEngine.Random.Range(0f, 1f));
			}
		}

		public void ResetFruitGrowProgress()
		{
			fruitMaturity.SetValue(0f);
		}

		public void SpawnHarvestedFruit()
		{
			GetComponent<Crop>().SpawnConfiguredFruit(null);
		}

		public void HideAllFruitSymbols()
		{
			foreach (ShapeCategory key in GetAnimSetByShapeCategory.Keys)
			{
				AnimSet animSet = GetAnimSetByShapeCategory[key];
				AnimController.SetSymbolVisiblity(animSet.meter_target, is_visible: false);
			}
		}

		public void CreateFruitMeter()
		{
			DestroyFruitMeter();
			fruitMeter = new MeterController(AnimController, Anims.meter_target, Anims.meter, Meter.Offset.NoChange, Grid.SceneLayer.Building);
			base.sm.Fruit.Set(fruitMeter.gameObject, this);
		}

		private void DestroyFruitMeter()
		{
			if (fruitMeter != null)
			{
				fruitMeter.Unlink();
				Util.KDestroyGameObject(fruitMeter.gameObject);
				fruitMeter = null;
				base.sm.Fruit.Set(null, this);
			}
		}

		public void PlayAnimOnFruitMeter(string animName, KAnim.PlayMode playMode)
		{
			if (fruitMeter != null)
			{
				fruitMeter.meterController.Play(animName, playMode);
			}
		}

		public void UpdateFruitGrowMeterPosition()
		{
			if (fruitMeter != null)
			{
				if (fruitMeter.meterController.currentAnim != Anims.meter)
				{
					PlayAnimOnFruitMeter(Anims.meter, KAnim.PlayMode.Paused);
				}
				fruitMeter.SetPositionPercent(FruitGrowthPercentage);
			}
		}

		private void KillForwardBranch()
		{
			if (Branch != null)
			{
				Instance sMI = Branch.GetSMI<Instance>();
				if (sMI != null)
				{
					sMI.sm.DieSignal.Trigger(sMI);
					sMI.sm.MarkedForDeath.Set(value: true, sMI);
				}
				base.sm.Branch.Set(null, this);
			}
		}

		public void SetupRootInformation(VineMother.Instance mother)
		{
			CellOffset cellOffsetDirection = Grid.GetCellOffsetDirection(Grid.PosToCell(this), Grid.PosToCell(mother));
			Direction value = ((cellOffsetDirection == CellOffset.left) ? Direction.Left : ((cellOffsetDirection == CellOffset.right) ? Direction.Right : ((!(cellOffsetDirection == CellOffset.up)) ? Direction.Down : Direction.Up)));
			base.sm.RootDirection.Set((int)value, this);
			base.sm.RootShape.Set(1, this);
			base.sm.BranchNumber.Set(1, this);
			base.sm.WildPlanted.Set(mother.IsWild, this);
			base.sm.Mother.Set(mother.gameObject, this);
			MotherSMI = ((Mother == null) ? null : Mother.GetSMI<VineMother.Instance>());
			HarvestDesignatable component = mother.GetComponent<HarvestDesignatable>();
			GetComponent<HarvestDesignatable>().SetHarvestWhenReady(component.HarvestWhenReady);
		}

		public void SetupRootInformation(Instance root)
		{
			CellOffset cellOffsetDirection = Grid.GetCellOffsetDirection(Grid.PosToCell(this), Grid.PosToCell(root));
			Direction value = ((cellOffsetDirection == CellOffset.left) ? Direction.Left : ((cellOffsetDirection == CellOffset.right) ? Direction.Right : ((!(cellOffsetDirection == CellOffset.up)) ? Direction.Down : Direction.Up)));
			base.sm.RootDirection.Set((int)value, this);
			base.sm.RootShape.Set((int)root.MyShape, this);
			base.sm.BranchNumber.Set(root.MyBranchNumber + 1, this);
			base.sm.WildPlanted.Set(root.IsWild, this);
			base.sm.Mother.Set(root.Mother, this);
			MotherSMI = ((Mother == null) ? null : Mother.GetSMI<VineMother.Instance>());
			HarvestDesignatable component = root.GetComponent<HarvestDesignatable>();
			GetComponent<HarvestDesignatable>().SetHarvestWhenReady(component.HarvestWhenReady);
		}

		public void AttemptToSpawnBranch()
		{
			if (CanSpawnBranch())
			{
				isSpawningNextBranch = true;
				int cellToSpawnBranch = GetCellToSpawnBranch();
				GameObject gameObject = SpawnBranchOnCell(cellToSpawnBranch);
				base.sm.Branch.Set(gameObject, this);
				isSpawningNextBranch = false;
				if (IsNewGameSpawned)
				{
					gameObject.Trigger(1119167081);
				}
				ResetUprootMonitor();
			}
			if (IsNewGameSpawned)
			{
				IsNewGameSpawned = false;
			}
		}

		private GameObject SpawnBranchOnCell(int cell)
		{
			Vector3 position = Grid.CellToPosCBC(cell, Grid.SceneLayer.BuildingFront);
			GameObject obj = Util.KInstantiate(Assets.GetPrefab(base.def.BRANCH_PREFAB_NAME), position);
			obj.SetActive(value: true);
			obj.GetSMI<Instance>().SetupRootInformation(this);
			return obj;
		}

		private bool IsMotherCellFoundation(int cell)
		{
			if (MotherSMI != null && MotherSMI.IsOnPlanterBox)
			{
				return MotherSMI.PlanterboxCell == cell;
			}
			return false;
		}

		private bool IsCellFoundation(int cell)
		{
			if (!VineBranch.IsCellFoundation(cell))
			{
				return IsMotherCellFoundation(cell);
			}
			return true;
		}

		private bool IsCellAvailable(int cell)
		{
			bool flag = VineBranch.IsCellAvailable(base.gameObject, cell, (Func<int, bool>)IsCellFoundation);
			if (flag && IsNewGameSpawned)
			{
				flag = SaveGame.Instance.worldGenSpawner.GetSpawnableInCell(cell) == null;
			}
			return flag;
		}

		public bool CanSpawnBranch()
		{
			bool flag = Branch == null && !MaxBranchNumberReached && IsGrown;
			if (flag)
			{
				int cellToSpawnBranch = GetCellToSpawnBranch();
				flag = flag && cellToSpawnBranch != Grid.InvalidCell && IsCellAvailable(cellToSpawnBranch);
			}
			return flag;
		}

		public int GetCellToSpawnBranch()
		{
			int cell = Grid.PosToCell(base.gameObject);
			switch (MyShape)
			{
			case Shape.Top:
			case Shape.Bottom:
				if (RootDirection != Direction.Left)
				{
					return Grid.OffsetCell(cell, -1, 0);
				}
				return Grid.OffsetCell(cell, 1, 0);
			case Shape.Left:
			case Shape.Right:
				if (RootDirection != Direction.Up)
				{
					return Grid.OffsetCell(cell, 0, 1);
				}
				return Grid.OffsetCell(cell, 0, -1);
			case Shape.InCornerTopLeft:
				if (RootDirection != Direction.Down)
				{
					return Grid.OffsetCell(cell, 0, -1);
				}
				return Grid.OffsetCell(cell, 1, 0);
			case Shape.InCornerTopRight:
				if (RootDirection != Direction.Down)
				{
					return Grid.OffsetCell(cell, 0, -1);
				}
				return Grid.OffsetCell(cell, -1, 0);
			case Shape.InCornerBottomLeft:
				if (RootDirection != Direction.Up)
				{
					return Grid.OffsetCell(cell, 0, 1);
				}
				return Grid.OffsetCell(cell, 1, 0);
			case Shape.InCornerBottomRight:
				if (RootDirection != Direction.Up)
				{
					return Grid.OffsetCell(cell, 0, 1);
				}
				return Grid.OffsetCell(cell, -1, 0);
			case Shape.OutCornerTopLeft:
				if (RootDirection != Direction.Up)
				{
					return Grid.OffsetCell(cell, 0, 1);
				}
				return Grid.OffsetCell(cell, -1, 0);
			case Shape.OutCornerTopRight:
				if (RootDirection != Direction.Up)
				{
					return Grid.OffsetCell(cell, 0, 1);
				}
				return Grid.OffsetCell(cell, 1, 0);
			case Shape.OutCornerBottomLeft:
				if (RootDirection != Direction.Down)
				{
					return Grid.OffsetCell(cell, 0, -1);
				}
				return Grid.OffsetCell(cell, -1, 0);
			case Shape.OutCornerBottomRight:
				if (RootDirection != Direction.Down)
				{
					return Grid.OffsetCell(cell, 0, -1);
				}
				return Grid.OffsetCell(cell, 1, 0);
			default:
				return Grid.InvalidCell;
			}
		}

		public void SubscribeSurroundingSolidChangesListeners()
		{
			KPrefabID component = base.gameObject.GetComponent<KPrefabID>();
			UnSubscribreSurroundingSolidChangesListeners();
			CellOffset[] offsets = new CellOffset[8]
			{
				new CellOffset(-1, -1),
				new CellOffset(0, -1),
				new CellOffset(1, -1),
				new CellOffset(-1, 0),
				new CellOffset(1, 0),
				new CellOffset(-1, 1),
				new CellOffset(0, 1),
				new CellOffset(1, 1)
			};
			Extents extents = new Extents(Grid.PosToCell(base.gameObject), offsets);
			solidPartitionerEntry = GameScenePartitioner.Instance.Add("VineBranchSurroundingListenerSolids", base.gameObject, extents, GameScenePartitioner.Instance.solidChangedLayer, OnSurroundingCellsBlockageChangedDetected);
			buildingsPartitionerEntry = GameScenePartitioner.Instance.Add("VineBranchSurroundingListenerBuildings", base.gameObject, extents, GameScenePartitioner.Instance.objectLayers[1], OnSurroundingCellsBlockageChangedDetected);
			plantsPartitionerEntry = GameScenePartitioner.Instance.Add("VineBranchSurroundingListenerPlants", component, extents, GameScenePartitioner.Instance.plantsChangedLayer, OnSurroundingCellsBlockageChangedDetected);
			liquidsPartitionerEntry = GameScenePartitioner.Instance.Add("VineBranchSurroundingListenerLiquids", base.gameObject, extents, GameScenePartitioner.Instance.liquidChangedLayer, OnSurroundingCellsBlockageChangedDetected);
		}

		public void UnSubscribreSurroundingSolidChangesListeners()
		{
			GameScenePartitioner.Instance.Free(ref solidPartitionerEntry);
			GameScenePartitioner.Instance.Free(ref buildingsPartitionerEntry);
			GameScenePartitioner.Instance.Free(ref plantsPartitionerEntry);
			GameScenePartitioner.Instance.Free(ref liquidsPartitionerEntry);
			solidPartitionerEntry = HandleVector<int>.InvalidHandle;
			buildingsPartitionerEntry = HandleVector<int>.InvalidHandle;
			plantsPartitionerEntry = HandleVector<int>.InvalidHandle;
			liquidsPartitionerEntry = HandleVector<int>.InvalidHandle;
		}

		private void OnSurroundingCellsBlockageChangedDetected(object o)
		{
			if (CanChangeShape)
			{
				RecalculateMyShape();
			}
		}

		private void SetShape(Shape shape, bool clockwise)
		{
			base.sm.BranchShape.Set((int)shape, this);
			base.sm.GrowingClockwise.Set(clockwise, this);
			SetAnimOrientation(shape, clockwise);
			Trigger(838747413);
		}

		public void RecalculateMyShape()
		{
			Shape shape = Shape.TopEnd;
			bool clockwise = false;
			switch (RootDirection)
			{
			case Direction.Left:
				switch (RootShape)
				{
				case Shape.Top:
				case Shape.InCornerTopLeft:
				case Shape.OutCornerTopRight:
					shape = ChooseCompatibleShape(new Shape[4]
					{
						Shape.RightEnd,
						Shape.InCornerTopRight,
						Shape.OutCornerTopLeft,
						Shape.Top
					});
					clockwise = shape != Shape.OutCornerTopLeft;
					break;
				case Shape.Bottom:
				case Shape.InCornerBottomLeft:
				case Shape.OutCornerBottomRight:
					shape = ChooseCompatibleShape(new Shape[4]
					{
						Shape.RightEnd,
						Shape.InCornerBottomRight,
						Shape.OutCornerBottomLeft,
						Shape.Bottom
					});
					clockwise = shape == Shape.OutCornerBottomLeft;
					break;
				}
				break;
			case Direction.Up:
				switch (base.smi.RootShape)
				{
				case Shape.Right:
				case Shape.InCornerTopRight:
				case Shape.OutCornerBottomRight:
					shape = ChooseCompatibleShape(new Shape[4]
					{
						Shape.BottomEnd,
						Shape.InCornerBottomRight,
						Shape.OutCornerTopRight,
						Shape.Right
					});
					clockwise = shape != Shape.OutCornerTopRight;
					break;
				case Shape.Left:
				case Shape.InCornerTopLeft:
				case Shape.OutCornerBottomLeft:
					shape = ChooseCompatibleShape(new Shape[4]
					{
						Shape.BottomEnd,
						Shape.InCornerBottomLeft,
						Shape.OutCornerTopLeft,
						Shape.Left
					});
					clockwise = shape == Shape.OutCornerTopLeft;
					break;
				}
				break;
			case Direction.Right:
				switch (base.smi.RootShape)
				{
				case Shape.Top:
				case Shape.InCornerTopRight:
				case Shape.OutCornerTopLeft:
					shape = ChooseCompatibleShape(new Shape[4]
					{
						Shape.LeftEnd,
						Shape.InCornerTopLeft,
						Shape.OutCornerTopRight,
						Shape.Top
					});
					clockwise = shape == Shape.OutCornerTopRight;
					break;
				case Shape.Bottom:
				case Shape.InCornerBottomRight:
				case Shape.OutCornerBottomLeft:
					shape = ChooseCompatibleShape(new Shape[4]
					{
						Shape.LeftEnd,
						Shape.InCornerBottomLeft,
						Shape.OutCornerBottomRight,
						Shape.Bottom
					});
					clockwise = shape != Shape.OutCornerBottomRight;
					break;
				}
				break;
			case Direction.Down:
				switch (base.smi.RootShape)
				{
				case Shape.Left:
				case Shape.InCornerBottomLeft:
				case Shape.OutCornerTopLeft:
					shape = ChooseCompatibleShape(new Shape[4]
					{
						Shape.TopEnd,
						Shape.InCornerTopLeft,
						Shape.OutCornerBottomLeft,
						Shape.Left
					});
					clockwise = shape != Shape.OutCornerBottomLeft;
					break;
				case Shape.Right:
				case Shape.InCornerBottomRight:
				case Shape.OutCornerTopRight:
					shape = ChooseCompatibleShape(new Shape[4]
					{
						Shape.TopEnd,
						Shape.InCornerTopRight,
						Shape.OutCornerBottomRight,
						Shape.Right
					});
					clockwise = shape == Shape.OutCornerBottomRight;
					break;
				}
				break;
			}
			base.smi.SetShape(shape, clockwise);
		}

		private void SetAnimOrientation(Shape shape, bool clockwise)
		{
			AnimController.FlipX = false;
			AnimController.FlipY = false;
			AnimController.Rotation = 0f;
			switch (shape)
			{
			case Shape.Top:
				AnimController.FlipY = clockwise;
				AnimController.Rotation = ((!clockwise) ? 180 : 0);
				break;
			case Shape.Bottom:
				AnimController.FlipX = clockwise;
				break;
			case Shape.Left:
				AnimController.FlipX = clockwise;
				AnimController.Rotation = 90f;
				break;
			case Shape.Right:
				AnimController.FlipX = clockwise;
				AnimController.Rotation = 270f;
				break;
			case Shape.InCornerTopLeft:
				AnimController.FlipX = clockwise;
				AnimController.Rotation = (clockwise ? 90 : 180);
				break;
			case Shape.InCornerTopRight:
				AnimController.FlipY = clockwise;
				AnimController.Rotation = ((!clockwise) ? 270 : 0);
				break;
			case Shape.InCornerBottomLeft:
				AnimController.FlipX = clockwise;
				AnimController.Rotation = ((!clockwise) ? 90 : 0);
				break;
			case Shape.InCornerBottomRight:
				AnimController.FlipY = clockwise;
				AnimController.Rotation = (clockwise ? 90 : 0);
				break;
			case Shape.OutCornerTopLeft:
				AnimController.FlipY = clockwise;
				AnimController.Rotation = (clockwise ? 90 : 0);
				break;
			case Shape.OutCornerTopRight:
				AnimController.FlipX = clockwise;
				AnimController.Rotation = ((!clockwise) ? 90 : 0);
				break;
			case Shape.OutCornerBottomLeft:
				AnimController.FlipY = clockwise;
				AnimController.Rotation = ((!clockwise) ? 270 : 0);
				break;
			case Shape.OutCornerBottomRight:
				AnimController.FlipX = clockwise;
				AnimController.Rotation = (clockwise ? 90 : 180);
				break;
			case Shape.TopEnd:
				AnimController.FlipX = clockwise;
				AnimController.Rotation = (clockwise ? 90 : 270);
				break;
			case Shape.BottomEnd:
				AnimController.FlipX = clockwise;
				AnimController.Rotation = (clockwise ? 270 : 90);
				break;
			case Shape.LeftEnd:
				AnimController.FlipX = clockwise;
				AnimController.Rotation = ((!clockwise) ? 180 : 0);
				break;
			case Shape.RightEnd:
				AnimController.FlipX = clockwise;
				AnimController.Rotation = (clockwise ? 180 : 0);
				break;
			}
			AnimController.Rotation *= -1f;
			AnimController.Offset = new Vector3(0f, AnimController.FlipY ? 1 : 0, 0f);
			bool flag = AnimController.FlipY && Mathf.Abs(AnimController.Rotation) == 90f;
			AnimController.Pivot = new Vector3(0f, flag ? (-0.5f) : 0.5f, 0f);
		}

		private Shape ChooseCompatibleShape(Shape[] possibleShapesOrderedByPriority)
		{
			bool flag = false;
			Shape result = Shape.TopEnd;
			int cell = Grid.PosToCell(base.gameObject);
			int cell2 = Grid.OffsetCell(cell, -1, 0);
			int cell3 = Grid.OffsetCell(cell, 1, 0);
			int cell4 = Grid.OffsetCell(cell, 0, 1);
			int cell5 = Grid.OffsetCell(cell, 0, -1);
			int cell6 = Grid.OffsetCell(cell, -1, 1);
			int cell7 = Grid.OffsetCell(cell, 1, 1);
			int cell8 = Grid.OffsetCell(cell, -1, -1);
			int cell9 = Grid.OffsetCell(cell, 1, -1);
			foreach (Shape shape in possibleShapesOrderedByPriority)
			{
				ShapeCategory shapeCategory = GetShapeCategory[shape];
				if (shapeCategory == ShapeCategory.DeadEnd)
				{
					result = shape;
				}
				if (!MaxBranchNumberReached || shapeCategory == ShapeCategory.DeadEnd)
				{
					switch (shape)
					{
					case Shape.TopEnd:
						flag = !IsCellAvailable(cell2) && !IsCellAvailable(cell4) && !IsCellAvailable(cell3);
						break;
					case Shape.BottomEnd:
						flag = !IsCellAvailable(cell2) && !IsCellAvailable(cell5) && !IsCellAvailable(cell3);
						break;
					case Shape.LeftEnd:
						flag = !IsCellAvailable(cell4) && !IsCellAvailable(cell5) && !IsCellAvailable(cell2);
						break;
					case Shape.RightEnd:
						flag = !IsCellAvailable(cell4) && !IsCellAvailable(cell5) && !IsCellAvailable(cell3);
						break;
					case Shape.InCornerTopLeft:
						flag = IsCellFoundation(cell4) && IsCellFoundation(cell2);
						break;
					case Shape.InCornerTopRight:
						flag = IsCellFoundation(cell4) && IsCellFoundation(cell3);
						break;
					case Shape.InCornerBottomLeft:
						flag = IsCellFoundation(cell5) && IsCellFoundation(cell2);
						break;
					case Shape.InCornerBottomRight:
						flag = IsCellFoundation(cell5) && IsCellFoundation(cell3);
						break;
					case Shape.OutCornerTopLeft:
						flag = (IsCellAvailable(cell4) || IsCellAvailable(cell2)) && IsCellFoundation(cell6);
						break;
					case Shape.OutCornerTopRight:
						flag = (IsCellAvailable(cell4) || IsCellAvailable(cell3)) && IsCellFoundation(cell7);
						break;
					case Shape.OutCornerBottomLeft:
						flag = (IsCellAvailable(cell5) || IsCellAvailable(cell2)) && IsCellFoundation(cell8);
						break;
					case Shape.OutCornerBottomRight:
						flag = (IsCellAvailable(cell5) || IsCellAvailable(cell3)) && IsCellFoundation(cell9);
						break;
					case Shape.Top:
						flag = IsCellFoundation(cell4) && (IsCellAvailable(cell2) || IsCellAvailable(cell3));
						break;
					case Shape.Bottom:
						flag = IsCellFoundation(cell5) && (IsCellAvailable(cell2) || IsCellAvailable(cell3));
						break;
					case Shape.Left:
						flag = IsCellFoundation(cell2) && (IsCellAvailable(cell4) || IsCellAvailable(cell5));
						break;
					case Shape.Right:
						flag = IsCellFoundation(cell3) && (IsCellAvailable(cell4) || IsCellAvailable(cell5));
						break;
					}
					if (flag)
					{
						return shape;
					}
				}
			}
			return result;
		}

		private void DelayedResetUprootMonitor(object o)
		{
			ResetUprootMonitor();
		}

		public void ResetUprootMonitor()
		{
			CellOffset[] newMonitorCells = new CellOffset[0];
			if (!CanChangeShape && !MaxBranchNumberReached)
			{
				switch (MyShape)
				{
				case Shape.Top:
					newMonitorCells = new CellOffset[1] { CellOffset.up };
					break;
				case Shape.Bottom:
					newMonitorCells = new CellOffset[1] { CellOffset.down };
					break;
				case Shape.Left:
					newMonitorCells = new CellOffset[1] { CellOffset.left };
					break;
				case Shape.Right:
					newMonitorCells = new CellOffset[1] { CellOffset.right };
					break;
				case Shape.InCornerTopLeft:
					newMonitorCells = new CellOffset[2]
					{
						CellOffset.up,
						CellOffset.left
					};
					break;
				case Shape.InCornerTopRight:
					newMonitorCells = new CellOffset[2]
					{
						CellOffset.up,
						CellOffset.right
					};
					break;
				case Shape.InCornerBottomLeft:
					newMonitorCells = new CellOffset[2]
					{
						CellOffset.down,
						CellOffset.left
					};
					break;
				case Shape.InCornerBottomRight:
					newMonitorCells = new CellOffset[2]
					{
						CellOffset.down,
						CellOffset.right
					};
					break;
				case Shape.OutCornerTopLeft:
					newMonitorCells = new CellOffset[1]
					{
						new CellOffset(-1, 1)
					};
					break;
				case Shape.OutCornerTopRight:
					newMonitorCells = new CellOffset[1]
					{
						new CellOffset(1, 1)
					};
					break;
				case Shape.OutCornerBottomLeft:
					newMonitorCells = new CellOffset[1]
					{
						new CellOffset(-1, -1)
					};
					break;
				case Shape.OutCornerBottomRight:
					newMonitorCells = new CellOffset[1]
					{
						new CellOffset(1, -1)
					};
					break;
				case Shape.TopEnd:
					newMonitorCells = new CellOffset[1] { IsGrowingClockwise ? CellOffset.left : CellOffset.right };
					break;
				case Shape.BottomEnd:
					newMonitorCells = new CellOffset[1] { IsGrowingClockwise ? CellOffset.right : CellOffset.left };
					break;
				case Shape.LeftEnd:
					newMonitorCells = new CellOffset[1] { IsGrowingClockwise ? CellOffset.down : CellOffset.up };
					break;
				case Shape.RightEnd:
					newMonitorCells = new CellOffset[1] { IsGrowingClockwise ? CellOffset.up : CellOffset.down };
					break;
				}
			}
			uprootMonitor.SetNewMonitorCells(newMonitorCells);
		}

		public float TimeUntilNextHarvest()
		{
			float num = ((maturity.GetDelta() <= 0f) ? 0f : ((maturity.GetMax() - maturity.value) / maturity.GetDelta()));
			float num2 = ((fruitMaturity.GetDelta() <= 0f) ? 0f : ((fruitMaturity.GetMax() - fruitMaturity.value) / fruitMaturity.GetDelta()));
			return num + num2;
		}

		public float GetCurrentGrowthPercentage()
		{
			if (!IsGrown)
			{
				return GrowthPercentage;
			}
			return FruitGrowthPercentage;
		}

		public float PercentGrown()
		{
			return GetCurrentGrowthPercentage();
		}

		public Crop GetCropComponent()
		{
			return GetComponent<Crop>();
		}

		public float DomesticGrowthTime()
		{
			return maturity.GetMax() / baseGrowingRate.Value;
		}

		public float WildGrowthTime()
		{
			return maturity.GetMax() / wildGrowingRate.Value;
		}

		public void OverrideMaturityLevel(float percent)
		{
			float value = maturity.GetMax() * percent;
			maturity.SetValue(value);
		}

		public bool IsWildPlanted()
		{
			return IsWild;
		}
	}

	private static Dictionary<Shape, ShapeCategory> GetShapeCategory = new Dictionary<Shape, ShapeCategory>
	{
		[Shape.Top] = ShapeCategory.Line,
		[Shape.Bottom] = ShapeCategory.Line,
		[Shape.Left] = ShapeCategory.Line,
		[Shape.Right] = ShapeCategory.Line,
		[Shape.InCornerTopLeft] = ShapeCategory.InCorner,
		[Shape.InCornerTopRight] = ShapeCategory.InCorner,
		[Shape.InCornerBottomLeft] = ShapeCategory.InCorner,
		[Shape.InCornerBottomRight] = ShapeCategory.InCorner,
		[Shape.OutCornerTopLeft] = ShapeCategory.OutCorner,
		[Shape.OutCornerTopRight] = ShapeCategory.OutCorner,
		[Shape.OutCornerBottomLeft] = ShapeCategory.OutCorner,
		[Shape.OutCornerBottomRight] = ShapeCategory.OutCorner,
		[Shape.TopEnd] = ShapeCategory.DeadEnd,
		[Shape.BottomEnd] = ShapeCategory.DeadEnd,
		[Shape.LeftEnd] = ShapeCategory.DeadEnd,
		[Shape.RightEnd] = ShapeCategory.DeadEnd
	};

	private static Dictionary<ShapeCategory, AnimSet> GetAnimSetByShapeCategory = new Dictionary<ShapeCategory, AnimSet>
	{
		[ShapeCategory.Line] = new AnimSet("line_"),
		[ShapeCategory.InCorner] = new AnimSet("incorner_"),
		[ShapeCategory.OutCorner] = new AnimSet("outcorner_"),
		[ShapeCategory.DeadEnd] = new AnimSet("end_")
	};

	public TargetParameter Fruit;

	public TargetParameter Mother;

	public TargetParameter Branch;

	public IntParameter BranchNumber;

	public IntParameter BranchShape;

	public BoolParameter GrowingClockwise;

	public IntParameter RootShape;

	public IntParameter RootDirection;

	public BoolParameter WildPlanted;

	public BoolParameter MarkedForDeath;

	public Signal DieSignal;

	public Signal OnShapeChangedSignal;

	public GrowingStates undevelopedBranch;

	public GrownStates mature;

	public State dead;

	public static bool IsCellFoundation(int cell)
	{
		if (!Grid.IsSolidCell(cell))
		{
			return Grid.HasDoor[cell];
		}
		return true;
	}

	public static bool IsCellAvailable(GameObject questionerObj, int cell, Func<int, bool> foundationCheckFunction = null)
	{
		int num = Grid.PosToCell(questionerObj);
		int num2 = Grid.WorldIdx[num];
		if (cell != Grid.InvalidCell && Grid.WorldIdx[cell] == num2 && ((foundationCheckFunction == null) ? (!IsCellFoundation(cell)) : (!foundationCheckFunction(cell))) && !Grid.IsLiquid(cell) && Grid.Objects[cell, 1] == null)
		{
			return Grid.Objects[cell, 5] == null;
		}
		return false;
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = undevelopedBranch;
		undevelopedBranch.InitializeStates(masterTarget, Mother, dead, DieSignal).ParamTransition(MarkedForDeath, dead, GameStateMachine<VineBranch, Instance, IStateMachineTarget, Def>.IsTrue).ParamTransition(Mother, dead, GameStateMachine<VineBranch, Instance, IStateMachineTarget, Def>.IsNull)
			.EventTransition(GameHashes.Grow, mature, (Instance smi) => smi.IsGrown)
			.UpdateTransition(mature, (Instance smi, float dt) => smi.IsGrown, UpdateRate.SIM_4000ms)
			.Enter(RecalculateMyShape)
			.Enter(SubscribreSurroundingCellChangeListeners)
			.Exit(UnSubscribreSurroundingSolidChangesListeners)
			.DefaultState(undevelopedBranch.growing);
		undevelopedBranch.wilted.PlayAnim((Func<Instance, string>)GetWiltAnim, KAnim.PlayMode.Loop).EventHandler(GameHashes.BranchShapeChanged, delegate(Instance smi)
		{
			RefreshAnim(smi, GetWiltAnim(smi), KAnim.PlayMode.Loop);
		}).EventTransition(GameHashes.WiltRecover, undevelopedBranch.growing, (Instance smi) => !smi.IsWilting);
		undevelopedBranch.growing.EventTransition(GameHashes.Wilt, undevelopedBranch.wilted, (Instance smi) => smi.IsWilting).PlayAnim((Instance smi) => smi.Anims.grow, KAnim.PlayMode.Paused).EventHandler(GameHashes.BranchShapeChanged, delegate(Instance smi)
		{
			RefreshAnim(smi, smi.Anims.grow, KAnim.PlayMode.Paused);
		})
			.ToggleStatusItem(Db.Get().CreatureStatusItems.Growing, (Instance smi) => smi)
			.Enter(RefreshPositionPercent)
			.Update(RefreshPositionPercent, UpdateRate.SIM_4000ms)
			.EventHandler(GameHashes.ConsumePlant, RefreshPositionPercent)
			.DefaultState(undevelopedBranch.growing.wild);
		undevelopedBranch.growing.wild.ParamTransition(WildPlanted, undevelopedBranch.growing.domestic, GameStateMachine<VineBranch, Instance, IStateMachineTarget, Def>.IsFalse).ToggleAttributeModifier("Growing", (Instance smi) => smi.wildGrowingRate);
		undevelopedBranch.growing.domestic.ParamTransition(WildPlanted, undevelopedBranch.growing.wild, GameStateMachine<VineBranch, Instance, IStateMachineTarget, Def>.IsTrue).ToggleAttributeModifier("Growing", (Instance smi) => smi.baseGrowingRate);
		mature.InitializeStates(masterTarget, Mother, dead, DieSignal).ParamTransition(MarkedForDeath, dead, GameStateMachine<VineBranch, Instance, IStateMachineTarget, Def>.IsTrue).ParamTransition(Mother, dead, GameStateMachine<VineBranch, Instance, IStateMachineTarget, Def>.IsNull)
			.Enter(RecalculateShapeAndSpawnBranchesIfSpawnedByDiscovery)
			.Enter(SetupFruitMeter)
			.Enter(SubscribreSurroundingCellChangeListeners)
			.Exit(UnSubscribreSurroundingSolidChangesListeners)
			.Update(SpawnBranchIfPossible, UpdateRate.SIM_4000ms)
			.DefaultState(mature.healthy);
		mature.healthy.PlayAnim((Instance smi) => smi.Anims.idle, KAnim.PlayMode.Loop).EventHandler(GameHashes.BranchShapeChanged, delegate(Instance smi)
		{
			RefreshAnim(smi, smi.Anims.idle, KAnim.PlayMode.Loop);
		}).DefaultState(mature.healthy.growing);
		mature.healthy.growing.EventTransition(GameHashes.Grow, mature.healthy.harvestReady, (Instance smi) => smi.IsReadyForHarvest).UpdateTransition(mature.healthy.harvestReady, (Instance smi, float dt) => smi.IsReadyForHarvest, UpdateRate.SIM_4000ms).EventTransition(GameHashes.Wilt, mature.wilted, (Instance smi) => smi.IsWilting)
			.EventHandler(GameHashes.BranchShapeChanged, RecreateFruitMeter)
			.EventHandler(GameHashes.BranchShapeChanged, UpdateFruitMeterGrowAnimations)
			.ToggleStatusItem(Db.Get().CreatureStatusItems.GrowingFruit, (Instance smi) => smi)
			.Enter(UpdateFruitMeterGrowAnimations)
			.Update(UpdateFruitMeterGrowAnimations)
			.DefaultState(mature.healthy.growing.wild);
		mature.healthy.growing.wild.ParamTransition(WildPlanted, mature.healthy.growing.domestic, GameStateMachine<VineBranch, Instance, IStateMachineTarget, Def>.IsFalse).ToggleAttributeModifier("Fruit Growing", (Instance smi) => smi.wildFruitGrowingRate);
		mature.healthy.growing.domestic.ParamTransition(WildPlanted, mature.healthy.growing.wild, GameStateMachine<VineBranch, Instance, IStateMachineTarget, Def>.IsTrue).ToggleAttributeModifier("Fruit Growing", (Instance smi) => smi.baseFruitGrowingRate);
		mature.healthy.harvestReady.ToggleTag(GameTags.FullyGrown).EventTransition(GameHashes.Harvest, mature.healthy.harvest).EventHandler(GameHashes.BranchShapeChanged, RecreateFruitMeter)
			.EventHandler(GameHashes.BranchShapeChanged, delegate(Instance smi)
			{
				PlayAnimsOnFruit(smi, smi.Anims.meter_harvest_ready, KAnim.PlayMode.Loop);
			})
			.Enter(MakeItHarvestable)
			.Exit(ResetOldAge)
			.ToggleAttributeModifier("GetOld", (Instance smi) => smi.getOldRate)
			.Enter(delegate(Instance smi)
			{
				PlayAnimsOnFruit(smi, smi.Anims.meter_harvest_ready, KAnim.PlayMode.Loop);
			})
			.UpdateTransition(mature.healthy.selfHarvestFromOld, ShouldSelfHarvestFromOldAge, UpdateRate.SIM_4000ms);
		mature.healthy.harvest.Target(Fruit).OnAnimQueueComplete(mature.healthy.growing).Target(masterTarget)
			.Enter(delegate(Instance smi)
			{
				PlayAnimsOnFruit(smi, smi.Anims.meter_harvest, KAnim.PlayMode.Once);
			})
			.Enter(MakeItNotHarvestable)
			.Enter(ResetFruitGrowProgress)
			.Enter(SpawnHarvestedFruit)
			.TriggerOnExit(GameHashes.HarvestComplete)
			.ScheduleGoTo(3f, mature.healthy.growing);
		mature.healthy.selfHarvestFromOld.Target(Fruit).OnAnimQueueComplete(mature.healthy.growing).Target(masterTarget)
			.Enter(delegate(Instance smi)
			{
				PlayAnimsOnFruit(smi, smi.Anims.meter_harvest, KAnim.PlayMode.Once);
			})
			.Enter(ForceCancelHarvest)
			.Enter(MakeItNotHarvestable)
			.Enter(ResetOldAge)
			.Enter(ResetFruitGrowProgress)
			.Enter(SpawnHarvestedFruit)
			.TriggerOnExit(GameHashes.HarvestComplete)
			.ScheduleGoTo(3f, mature.healthy.growing);
		mature.wilted.PlayAnim((Func<Instance, string>)GetWiltAnim, KAnim.PlayMode.Loop).Enter(delegate(Instance smi)
		{
			PlayAnimsOnFruit(smi, GetFruitWiltAnim(smi), KAnim.PlayMode.Loop);
		}).EventHandler(GameHashes.BranchShapeChanged, delegate(Instance smi)
		{
			RefreshAnim(smi, GetWiltAnim(smi), KAnim.PlayMode.Loop);
		})
			.EventHandler(GameHashes.BranchShapeChanged, RecreateFruitMeter)
			.EventHandler(GameHashes.BranchShapeChanged, delegate(Instance smi)
			{
				PlayAnimsOnFruit(smi, smi.Anims.meter_wilted, KAnim.PlayMode.Loop);
			})
			.EventTransition(GameHashes.WiltRecover, mature.healthy, (Instance smi) => !smi.IsWilting)
			.EventTransition(GameHashes.Harvest, mature.healthy.harvest);
		dead.Target(masterTarget).ToggleMainStatusItem(Db.Get().CreatureStatusItems.Dead).Enter(HarvestOnDeath)
			.Enter(delegate(Instance smi)
			{
				if (!smi.gameObject.GetComponent<KPrefabID>().HasTag(GameTags.Uprooted) && !smi.IsWild)
				{
					Notifier notifier = smi.gameObject.AddOrGet<Notifier>();
					Notification notification = CreateDeathNotification(smi);
					notifier.Add(notification);
				}
				GameUtil.KInstantiate(Assets.GetPrefab(EffectConfigs.PlantDeathId), smi.transform.GetPosition(), Grid.SceneLayer.FXFront).SetActive(value: true);
				smi.Trigger(1623392196);
				smi.DestroySelf(null);
			});
	}

	private static bool ShouldSelfHarvestFromOldAge(Instance smi, float dt)
	{
		return smi.IsOld;
	}

	private static string GetWiltAnim(Instance smi)
	{
		return smi.Anims.GetBaseWiltAnim(smi.Anims.GetWiltLevel(smi.GrowthPercentage));
	}

	private static string GetFruitWiltAnim(Instance smi)
	{
		return smi.Anims.GetMeterWiltAnim(smi.Anims.GetWiltLevel(smi.FruitGrowthPercentage));
	}

	private static void PlayAnimsOnFruit(Instance smi, string animName, KAnim.PlayMode playmode)
	{
		smi.PlayAnimOnFruitMeter(animName, playmode);
	}

	private static void UpdateFruitMeterGrowAnimations(Instance smi, float dt)
	{
		UpdateFruitMeterGrowAnimations(smi);
	}

	private static void UpdateFruitMeterGrowAnimations(Instance smi)
	{
		smi.UpdateFruitGrowMeterPosition();
	}

	private static void RecreateFruitMeter(Instance smi)
	{
		smi.CreateFruitMeter();
	}

	private static void SetupFruitMeter(Instance smi)
	{
		smi.CreateFruitMeter();
	}

	private static void SpawnBranchIfPossible(Instance smi, float dt)
	{
		smi.AttemptToSpawnBranch();
	}

	private static void MakeItHarvestable(Instance smi)
	{
		smi.SetHarvestableState(canBeHarvested: true);
	}

	private static void ForceCancelHarvest(Instance smi)
	{
		smi.ForceCancelHarvest();
	}

	private static void MakeItNotHarvestable(Instance smi)
	{
		smi.SetHarvestableState(canBeHarvested: false);
	}

	private static void RefreshPositionPercent(Instance smi, float dt)
	{
		RefreshPositionPercent(smi);
	}

	private static void RefreshPositionPercent(Instance smi)
	{
		smi.AnimController.SetPositionPercent(smi.GrowthPercentage);
	}

	private static void SubscribreSurroundingCellChangeListeners(Instance smi)
	{
		smi.SubscribeSurroundingSolidChangesListeners();
	}

	private static void UnSubscribreSurroundingSolidChangesListeners(Instance smi)
	{
		smi.UnSubscribreSurroundingSolidChangesListeners();
	}

	private static void ResetFruitGrowProgress(Instance smi)
	{
		smi.ResetFruitGrowProgress();
	}

	private static void ResetOldAge(Instance smi)
	{
		smi.ResetOldAge();
	}

	private static void SpawnHarvestedFruit(Instance smi)
	{
		smi.SpawnHarvestedFruit();
	}

	private static void RecalculateMyShape(Instance smi)
	{
		smi.RecalculateMyShape();
	}

	private static void OnMotherRecovered(Instance smi)
	{
		smi.BoxingTrigger(912965142, data: true);
	}

	private static void OnMotherWilted(Instance smi)
	{
		smi.BoxingTrigger(912965142, data: false);
	}

	private static void RecalculateShapeAndSpawnBranchesIfSpawnedByDiscovery(Instance smi)
	{
		if (smi.IsNewGameSpawned)
		{
			smi.RecalculateMyShape();
			SpawnBranchIfPossible(smi, 0f);
		}
	}

	public static void HarvestOnDeath(Instance smi)
	{
		if (smi.IsReadyForHarvest)
		{
			SpawnHarvestedFruit(smi);
		}
	}

	private static void RefreshAnim(Instance smi, string animName, KAnim.PlayMode playmode)
	{
		float elapsedTime = smi.AnimController.GetElapsedTime();
		smi.AnimController.Play(animName, playmode);
		smi.AnimController.SetElapsedTime(elapsedTime);
	}

	private static Notification CreateDeathNotification(Instance smi)
	{
		return new Notification(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION, NotificationType.Bad, (List<Notification> notificationList, object data) => string.Concat(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION_TOOLTIP, notificationList.ReduceMessages(countNames: false)), "/t• " + smi.gameObject.GetProperName());
	}
}
