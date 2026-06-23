using Klei.AI;
using STRINGS;
using UnityEngine;

public class SolidConsumerMonitor : GameStateMachine<SolidConsumerMonitor, SolidConsumerMonitor.Instance, IStateMachineTarget, SolidConsumerMonitor.Def>
{
	public class Def : BaseDef
	{
		public Diet diet;

		public Vector3[] possibleEatPositionOffsets = new Vector3[1] { Vector3.zero };

		public Vector2 navigatorSize = Vector2.one;

		public bool sportHuntWhenOvercrowded;
	}

	public new class Instance : GameInstance
	{
		private const int RECALC_THRESHOLD = 4;

		public GameObject targetEdible;

		public Vector3 targetEdibleOffset;

		private int targetEdibleCost;

		[MyCmpGet]
		private Navigator navigator;

		[MyCmpGet]
		private DrowningMonitor drowningMonitor;

		public Diet diet;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			diet = DietManager.Instance.GetPrefabDiet(base.gameObject);
		}

		public bool CanSearchForPickupables(bool foodAtFeeder)
		{
			return !foodAtFeeder;
		}

		public bool IsCloserThanTargetEdible(int cost)
		{
			if (cost != -1)
			{
				if (cost >= targetEdibleCost)
				{
					return targetEdibleCost == -1;
				}
				return true;
			}
			return false;
		}

		public bool IsTargetEdibleValid()
		{
			if (targetEdible == null)
			{
				return false;
			}
			int cost = GetCost(Grid.PosToCell(targetEdible.transform.GetPosition() + targetEdibleOffset));
			if (cost == -1 || cost > targetEdibleCost + 4)
			{
				return false;
			}
			return true;
		}

		public void ClearTargetEdible()
		{
			targetEdibleCost = -1;
			targetEdible = null;
			targetEdibleOffset = Vector3.zero;
		}

		public bool ProcessEdible(GameObject edible, out bool isReachable)
		{
			int cost = GetCost(edible);
			isReachable = cost != -1;
			if (cost != -1 && (cost < targetEdibleCost || targetEdibleCost == -1))
			{
				SetTargetEdible(edible, cost);
				return true;
			}
			return false;
		}

		public void SetTargetEdible(GameObject gameObject, int cost)
		{
			if (!(targetEdible == gameObject))
			{
				targetEdibleCost = cost;
				targetEdible = gameObject;
			}
		}

		public int GetCost(GameObject edible)
		{
			return GetCost(Grid.PosToCell(edible.transform.GetPosition() + base.smi.GetBestEdibleOffset(edible)));
		}

		public int GetCost(int cell)
		{
			if (drowningMonitor != null && drowningMonitor.canDrownToDeath && !drowningMonitor.livesUnderWater && !drowningMonitor.IsCellSafe(cell))
			{
				return -1;
			}
			return navigator.GetNavigationCost(cell);
		}

		public void OnEatSolidComplete(object data)
		{
			KPrefabID kPrefabID = data as KPrefabID;
			if (kPrefabID == null)
			{
				return;
			}
			PrimaryElement component = kPrefabID.GetComponent<PrimaryElement>();
			if (component == null)
			{
				return;
			}
			Diet.Info dietInfo = diet.GetDietInfo(kPrefabID.PrefabTag);
			if (dietInfo == null)
			{
				return;
			}
			AmountInstance amountInstance = Db.Get().Amounts.Calories.Lookup(base.smi.gameObject);
			string properName = kPrefabID.GetProperName();
			PopFXManager.Instance.SpawnFX(global::Def.GetUISprite(kPrefabID.gameObject).first, PopFXManager.Instance.sprite_Negative, properName, kPrefabID.transform, Vector3.zero);
			float num = amountInstance.GetMax() - amountInstance.value;
			float num2 = dietInfo.ConvertCaloriesToConsumptionMass(num);
			IPlantConsumptionInstructions plantConsumptionInstructions = null;
			IPlantConsumptionInstructions[] plantConsumptionInstructions2 = GameUtil.GetPlantConsumptionInstructions(kPrefabID.gameObject);
			foreach (IPlantConsumptionInstructions plantConsumptionInstructions3 in plantConsumptionInstructions2)
			{
				if (dietInfo.foodType == plantConsumptionInstructions3.GetDietFoodType())
				{
					plantConsumptionInstructions = plantConsumptionInstructions3;
				}
			}
			float num3 = 0f;
			if (plantConsumptionInstructions != null)
			{
				num2 = plantConsumptionInstructions.ConsumePlant(num2);
				num3 = dietInfo.ConvertConsumptionMassToCalories(num2);
			}
			else if (dietInfo.foodType == Diet.Info.FoodType.EatPrey || dietInfo.foodType == Diet.Info.FoodType.EatButcheredPrey)
			{
				float num4 = diet.AvailableCaloriesInPrey(kPrefabID.PrefabTag);
				float num5 = Mathf.Clamp(1f - num / num4, 0f, 1f);
				if (num5 > 0f)
				{
					Butcherable component2 = kPrefabID.GetComponent<Butcherable>();
					if (component2 != null)
					{
						component2.CreateDrops(num5);
					}
				}
				component.Mass = 0f;
				num3 = Mathf.Min(num, num4);
				if (IsInsideState(base.sm.sportHunting))
				{
					PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, CREATURES.STATUSITEMS.SPORTHUNT.POPFX, base.gameObject.transform);
				}
			}
			else
			{
				num2 = Mathf.Min(num2, component.Mass);
				component.Mass -= num2;
				Pickupable component3 = component.GetComponent<Pickupable>();
				if (component3.storage != null)
				{
					component3.storage.Trigger(-1452790913, (object)base.gameObject);
					component3.storage.Trigger(-1697596308, (object)base.gameObject);
				}
				num3 = dietInfo.ConvertConsumptionMassToCalories(num2);
			}
			Boxed<CreatureCalorieMonitor.CaloriesConsumedEvent> boxed = Boxed<CreatureCalorieMonitor.CaloriesConsumedEvent>.Get(new CreatureCalorieMonitor.CaloriesConsumedEvent
			{
				tag = kPrefabID.PrefabTag,
				calories = num3
			});
			Trigger(-2038961714, (object)boxed);
			boxed.Release();
			targetEdible = null;
		}

		public string[] GetTargetEdibleEatAnims()
		{
			return diet.GetDietInfo(targetEdible.PrefabID()).eatAnims;
		}

		public Vector3 GetBestEdibleOffset(GameObject edible)
		{
			int num = int.MaxValue;
			Vector3 result = Vector3.zero;
			Vector3[] possibleEatPositionOffsets = base.def.possibleEatPositionOffsets;
			for (int i = 0; i < possibleEatPositionOffsets.Length; i++)
			{
				Vector3 vector = possibleEatPositionOffsets[i];
				Vector3 pos = edible.transform.position + vector;
				if (vector.x > 0f)
				{
					pos += new Vector3(base.def.navigatorSize.x / 2f, 0f, 0f);
				}
				else if (vector.x < 0f)
				{
					pos -= new Vector3(base.def.navigatorSize.x / 2f, 0f, 0f);
				}
				if (vector.y > 0f)
				{
					pos += new Vector3(0f, base.def.navigatorSize.y / 2f, 0f);
				}
				else if (vector.y < 0f)
				{
					pos -= new Vector3(0f, base.def.navigatorSize.y / 2f, 0f);
				}
				int navigationCost = navigator.GetNavigationCost(Grid.PosToCell(pos));
				if (navigationCost != -1 && navigationCost < num)
				{
					num = navigationCost;
					result = vector;
				}
			}
			return result;
		}
	}

	public static Vector3 PLANT_ON_FLOOR_VESSEL_OFFSET = Vector3.down;

	private State satisfied;

	private State lookingforfood;

	private State sportHunting;

	private static Tag[] creatureTags = new Tag[2]
	{
		GameTags.Creatures.ReservedByCreature,
		GameTags.CreatureBrain
	};

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = satisfied;
		root.EventHandler(GameHashes.EatSolidComplete, delegate(Instance smi, object data)
		{
			smi.OnEatSolidComplete(data);
		}).ToggleBehaviour(GameTags.Creatures.WantsToEat, (Instance smi) => smi.targetEdible != null && !smi.targetEdible.HasTag(GameTags.Creatures.ReservedByCreature));
		satisfied.TagTransition(GameTags.Creatures.Hungry, lookingforfood).Transition(sportHunting, IsSportHuntingConditionMet);
		lookingforfood.TagTransition(GameTags.Creatures.Hungry, satisfied, on_remove: true).PreBrainUpdate(FindFood);
		sportHunting.TagTransition(GameTags.Creatures.Hungry, lookingforfood).Transition(satisfied, (Instance smi) => !IsSportHuntingConditionMet(smi)).PreBrainUpdate(FindSportHuntPrey);
	}

	private static bool IsSportHuntingConditionMet(Instance smi)
	{
		if (smi.def.sportHuntWhenOvercrowded && smi.diet.CanEatPreyCritter && smi.gameObject.HasTag(GameTags.Creatures.Overcrowded))
		{
			return !smi.gameObject.HasTag(GameTags.Creatures.Hungry);
		}
		return false;
	}

	private static void FindSportHuntPrey(Instance smi)
	{
		if (!smi.IsTargetEdibleValid())
		{
			smi.ClearTargetEdible();
			FindPreyInCavity(smi);
		}
	}

	private static bool FindPreyInCavity(Instance smi)
	{
		Diet diet = smi.diet;
		CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(Grid.PosToCell(smi));
		KPrefabID kPrefabID = null;
		int num = int.MaxValue;
		if (cavityForCell != null)
		{
			foreach (KPrefabID creature in cavityForCell.creatures)
			{
				if (!creature.HasTag(GameTags.Creatures.ReservedByCreature) && diet.GetDietInfo(creature.PrefabTag) != null)
				{
					int cost = smi.GetCost(creature.gameObject);
					if (cost != -1 && (cost < num || num == -1))
					{
						kPrefabID = creature;
						num = cost;
					}
				}
				if (kPrefabID != null && num < 3)
				{
					break;
				}
			}
		}
		if (kPrefabID != null)
		{
			smi.SetTargetEdible(kPrefabID.gameObject, num);
			smi.targetEdibleOffset = smi.GetBestEdibleOffset(kPrefabID.gameObject);
			return true;
		}
		return false;
	}

	private static void FindFood(Instance smi)
	{
		if (smi.IsTargetEdibleValid())
		{
			return;
		}
		smi.ClearTargetEdible();
		Diet diet = smi.diet;
		int x = 0;
		int y = 0;
		Grid.PosToXY(smi.gameObject.transform.GetPosition(), out x, out y);
		x -= 8;
		y -= 8;
		bool flag = false;
		if (diet.CanEatPreyCritter)
		{
			flag = FindPreyInCavity(smi);
		}
		bool flag2 = false;
		if (!flag && diet.CanEatAnySolid)
		{
			int num = 32;
			foreach (CreatureFeeder item in Components.CreatureFeeders.GetItems(smi.GetMyWorldId()))
			{
				Vector2I targetFeederCell = item.GetTargetFeederCell();
				if (targetFeederCell.x < x || targetFeederCell.x > x + num || targetFeederCell.y < y || targetFeederCell.y > y + num || item.StoragesAreEmpty())
				{
					continue;
				}
				int cost = smi.GetCost(Grid.XYToCell(targetFeederCell.x, targetFeederCell.y));
				if (!smi.IsCloserThanTargetEdible(cost))
				{
					continue;
				}
				Storage[] storages = item.storages;
				foreach (Storage storage in storages)
				{
					if (storage == null || storage.IsEmpty() || smi.GetCost(Grid.PosToCell(storage.items[0])) == -1)
					{
						continue;
					}
					foreach (GameObject item2 in storage.items)
					{
						if (!(item2 == null))
						{
							KPrefabID component = item2.GetComponent<KPrefabID>();
							if (!component.HasAnyTags(creatureTags) && diet.GetDietInfo(component.PrefabTag) != null)
							{
								smi.SetTargetEdible(item2, cost);
								smi.targetEdibleOffset = Vector3.zero;
								flag2 = true;
								break;
							}
						}
					}
					if (flag2)
					{
						break;
					}
				}
			}
		}
		bool flag3 = false;
		if (!flag && !flag2 && diet.CanEatAnyPlantDirectly)
		{
			ListPool<ScenePartitionerEntry, GameScenePartitioner>.PooledList pooledList = ListPool<ScenePartitionerEntry, GameScenePartitioner>.Allocate();
			GameScenePartitioner.Instance.GatherEntries(x, y, 16, 16, GameScenePartitioner.Instance.plants, pooledList);
			foreach (ScenePartitionerEntry item3 in pooledList)
			{
				KPrefabID kPrefabID = (KPrefabID)item3.obj;
				Diet.Info dietInfo = diet.GetDietInfo(kPrefabID.PrefabTag);
				Vector3 position = kPrefabID.transform.GetPosition();
				bool flag4 = kPrefabID.HasTag(GameTags.PlantedOnFloorVessel);
				if (flag4)
				{
					position += PLANT_ON_FLOOR_VESSEL_OFFSET;
				}
				int num2 = smi.GetCost(Grid.PosToCell(position));
				Vector3 vector = Vector3.zero;
				if (!smi.IsCloserThanTargetEdible(num2) || kPrefabID.HasAnyTags(creatureTags) || dietInfo == null)
				{
					continue;
				}
				if (kPrefabID.HasTag(GameTags.Plant))
				{
					IPlantConsumptionInstructions[] plantConsumptionInstructions = GameUtil.GetPlantConsumptionInstructions(kPrefabID.gameObject);
					if (plantConsumptionInstructions == null || plantConsumptionInstructions.Length == 0)
					{
						continue;
					}
					bool flag5 = false;
					IPlantConsumptionInstructions[] array = plantConsumptionInstructions;
					foreach (IPlantConsumptionInstructions plantConsumptionInstructions2 in array)
					{
						if (!plantConsumptionInstructions2.CanPlantBeEaten() || dietInfo.foodType != plantConsumptionInstructions2.GetDietFoodType())
						{
							continue;
						}
						CellOffset[] allowedOffsets = plantConsumptionInstructions2.GetAllowedOffsets();
						if (allowedOffsets != null)
						{
							num2 = -1;
							CellOffset[] array2 = allowedOffsets;
							for (int j = 0; j < array2.Length; j++)
							{
								CellOffset offset = array2[j];
								int cost2 = smi.GetCost(Grid.OffsetCell(Grid.PosToCell(position), offset));
								if (cost2 != -1 && (num2 == -1 || cost2 < num2))
								{
									num2 = cost2;
									vector = offset.ToVector3();
								}
							}
							if (num2 != -1)
							{
								flag5 = true;
								break;
							}
						}
						else
						{
							flag5 = true;
						}
					}
					if (!flag5)
					{
						continue;
					}
				}
				smi.SetTargetEdible(kPrefabID.gameObject, num2);
				smi.targetEdibleOffset = vector + (flag4 ? PLANT_ON_FLOOR_VESSEL_OFFSET : Vector3.zero);
				flag3 = true;
			}
			pooledList.Recycle();
		}
		if (flag || flag2 || flag3 || !diet.CanEatAnySolid)
		{
			return;
		}
		bool flag6 = false;
		ListPool<ScenePartitionerEntry, GameScenePartitioner>.PooledList pooledList2 = ListPool<ScenePartitionerEntry, GameScenePartitioner>.Allocate();
		GameScenePartitioner.Instance.GatherEntries(x, y, 16, 16, GameScenePartitioner.Instance.pickupablesLayer, pooledList2);
		foreach (ScenePartitionerEntry item4 in pooledList2)
		{
			Pickupable pickupable = (Pickupable)item4.obj;
			KPrefabID kPrefabID2 = pickupable.KPrefabID;
			if (!kPrefabID2.HasAnyTags(creatureTags) && diet.GetDietInfo(kPrefabID2.PrefabTag) != null)
			{
				smi.ProcessEdible(pickupable.gameObject, out var isReachable);
				smi.targetEdibleOffset = Vector3.zero;
				flag6 = flag6 || isReachable;
			}
		}
		pooledList2.Recycle();
	}
}
