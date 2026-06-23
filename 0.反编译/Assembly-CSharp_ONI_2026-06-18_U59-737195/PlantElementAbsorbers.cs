using System.Collections.Generic;

public class PlantElementAbsorbers : KCompactedVector<PlantElementAbsorber>
{
	private bool updating;

	private List<HandleVector<int>.Handle> queuedRemoves = new List<HandleVector<int>.Handle>();

	public HandleVector<int>.Handle Add(Storage storage, PlantElementAbsorber.ConsumeInfo[] consumed_elements)
	{
		if (consumed_elements == null || consumed_elements.Length == 0)
		{
			return HandleVector<int>.InvalidHandle;
		}
		if (consumed_elements.Length == 1)
		{
			return Allocate(new PlantElementAbsorber
			{
				storage = storage,
				consumedElements = null,
				localInfo = new PlantElementAbsorber.LocalInfo
				{
					tag = consumed_elements[0].tag,
					massConsumptionRate = consumed_elements[0].massConsumptionRate
				}
			});
		}
		return Allocate(new PlantElementAbsorber
		{
			storage = storage,
			consumedElements = consumed_elements,
			localInfo = new PlantElementAbsorber.LocalInfo
			{
				tag = Tag.Invalid,
				massConsumptionRate = 0f
			}
		});
	}

	public HandleVector<int>.Handle Remove(HandleVector<int>.Handle h)
	{
		if (updating)
		{
			queuedRemoves.Add(h);
		}
		else
		{
			Free(h);
		}
		return HandleVector<int>.InvalidHandle;
	}

	public void Sim200ms(float dt)
	{
		int count = data.Count;
		updating = true;
		ListPool<PlantElementAbsorber.Planner.ConsumeCommand, PlantElementAbsorbers>.PooledList pooledList = ListPool<PlantElementAbsorber.Planner.ConsumeCommand, PlantElementAbsorbers>.Allocate();
		for (int i = 0; i < count; i++)
		{
			PlantElementAbsorber value = data[i];
			pooledList.Clear();
			if (!value.PlanConsume(dt, pooledList))
			{
				continue;
			}
			foreach (PlantElementAbsorber.Planner.ConsumeCommand item in pooledList)
			{
				item.primaryElement.Mass -= item.deltaMass;
				value.storage.Trigger(-1697596308, (object)item.primaryElement.gameObject);
			}
			data[i] = value;
		}
		pooledList.Recycle();
		updating = false;
		for (int j = 0; j < queuedRemoves.Count; j++)
		{
			HandleVector<int>.Handle h = queuedRemoves[j];
			Remove(h);
		}
		queuedRemoves.Clear();
	}

	public override void Clear()
	{
		base.Clear();
		for (int i = 0; i < data.Count; i++)
		{
			data[i].Clear();
		}
		data.Clear();
		handles.Clear();
	}

	public PlantElementAbsorbers()
		: base(0)
	{
	}
}
