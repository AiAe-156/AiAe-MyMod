using System.Collections.Generic;
using UnityEngine;

public struct PlantElementAbsorber
{
	public struct ConsumeInfo
	{
		public Tag tag;

		public float massConsumptionRate;

		public ConsumeInfo(Tag tag, float mass_consumption_rate)
		{
			this.tag = tag;
			massConsumptionRate = mass_consumption_rate;
		}
	}

	public struct LocalInfo
	{
		public Tag tag;

		public float massConsumptionRate;
	}

	public readonly struct Planner
	{
		public struct ConsumeCommand
		{
			public PrimaryElement primaryElement;

			public float deltaMass;
		}

		private readonly ListPool<KPrefabID, Planner>.PooledList prefabIds;

		private readonly ListPool<PrimaryElement, Planner>.PooledList primaryElements;

		public Planner(Storage storage)
		{
			prefabIds = ListPool<KPrefabID, Planner>.Allocate();
			primaryElements = ListPool<PrimaryElement, Planner>.Allocate();
			DebugUtil.DevAssert(storage != null, "Initializing Planner with a null Storage.");
			if (storage == null)
			{
				return;
			}
			foreach (GameObject item in storage.items)
			{
				if (item.TryGetComponent<KPrefabID>(out var component) && item.TryGetComponent<PrimaryElement>(out var component2))
				{
					prefabIds.Add(component);
					primaryElements.Add(component2);
				}
			}
		}

		public bool PlanConsume(float requiredMass, Tag tag, List<ConsumeCommand> consumeCommands)
		{
			for (int i = 0; i != prefabIds.Count; i++)
			{
				if (prefabIds[i].HasTag(tag))
				{
					PrimaryElement primaryElement = primaryElements[i];
					float num = Mathf.Min(requiredMass, primaryElement.Mass);
					requiredMass -= num;
					consumeCommands?.Add(new ConsumeCommand
					{
						primaryElement = primaryElement,
						deltaMass = num
					});
					if (requiredMass <= 0f)
					{
						return true;
					}
				}
			}
			return false;
		}

		public float Sum(Tag tag)
		{
			float num = 0f;
			for (int i = 0; i != prefabIds.Count; i++)
			{
				if (prefabIds[i].HasTag(tag))
				{
					num += primaryElements[i].Mass;
				}
			}
			return num;
		}

		public void Recycle()
		{
			prefabIds.Recycle();
			primaryElements.Recycle();
		}
	}

	public Storage storage;

	public LocalInfo localInfo;

	public ConsumeInfo[] consumedElements;

	public void Clear()
	{
		storage = null;
		consumedElements = null;
	}

	private static bool CheckPreConditions(Storage storage, float dt)
	{
		if (storage == null)
		{
			return false;
		}
		if (dt <= 0f)
		{
			DebugUtil.DevLogError("A delta time of 0 will produce degenerate consumeCommands.");
			return false;
		}
		return true;
	}

	public static bool PlanConsume(Storage storage, LocalInfo localInfo, float dt, List<Planner.ConsumeCommand> consumeCommands)
	{
		if (!CheckPreConditions(storage, dt))
		{
			return false;
		}
		Planner planner = new Planner(storage);
		bool result = planner.PlanConsume(localInfo.massConsumptionRate * dt, localInfo.tag, consumeCommands);
		planner.Recycle();
		return result;
	}

	public static bool PlanConsume(Storage storage, ConsumeInfo[] consumedElements, float dt, List<Planner.ConsumeCommand> consumeCommands)
	{
		if (!CheckPreConditions(storage, dt))
		{
			return false;
		}
		Planner planner = new Planner(storage);
		for (int i = 0; i < consumedElements.Length; i++)
		{
			ConsumeInfo consumeInfo = consumedElements[i];
			consumeCommands?.Clear();
			if (planner.PlanConsume(consumeInfo.massConsumptionRate * dt, consumeInfo.tag, consumeCommands))
			{
				return OnExit(result: true);
			}
		}
		return OnExit(result: false);
		bool OnExit(bool result)
		{
			planner.Recycle();
			return result;
		}
	}

	public readonly bool PlanConsume(float dt, List<Planner.ConsumeCommand> consumeCommands)
	{
		return (consumedElements == null) ? PlanConsume(storage, localInfo, dt, consumeCommands) : PlanConsume(storage, consumedElements, dt, consumeCommands);
	}

	public static float FindLargest(Storage storage, ConsumeInfo[] consumedElements)
	{
		if (storage == null)
		{
			return 0f;
		}
		if (consumedElements.Length == 0)
		{
			return 0f;
		}
		Planner planner = new Planner(storage);
		float num = -1f;
		for (int i = 0; i < consumedElements.Length; i++)
		{
			ConsumeInfo consumeInfo = consumedElements[i];
			float num2 = planner.Sum(consumeInfo.tag);
			if (num == -1f || num2 > num)
			{
				num = num2;
			}
		}
		planner.Recycle();
		return num;
	}

	public readonly float FindLargest()
	{
		if (consumedElements == null)
		{
			if (storage == null)
			{
				return 0f;
			}
			Planner planner = new Planner(storage);
			float result = planner.Sum(localInfo.tag);
			planner.Recycle();
			return result;
		}
		return FindLargest(storage, consumedElements);
	}
}
