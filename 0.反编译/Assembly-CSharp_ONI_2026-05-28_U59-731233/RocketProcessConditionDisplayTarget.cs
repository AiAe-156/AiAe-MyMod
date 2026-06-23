using System;
using System.Collections.Generic;

public class RocketProcessConditionDisplayTarget : KMonoBehaviour, IProcessConditionSet, ISim1000ms
{
	private CraftModuleInterface craftModuleInterface;

	[MyCmpReq]
	private KSelectable kselectable;

	private Guid statusHandle = Guid.Empty;

	public List<ProcessCondition> GetConditionSet(ProcessCondition.ProcessConditionType conditionType)
	{
		if (craftModuleInterface == null)
		{
			craftModuleInterface = GetComponent<RocketModuleCluster>().CraftInterface;
		}
		return craftModuleInterface.GetConditionSet(conditionType);
	}

	public int PopulateConditionSet(ProcessCondition.ProcessConditionType conditionType, List<ProcessCondition> conditions)
	{
		if (craftModuleInterface == null)
		{
			craftModuleInterface = GetComponent<RocketModuleCluster>().CraftInterface;
		}
		return craftModuleInterface.PopulateConditionSet(conditionType, conditions);
	}

	public void Sim1000ms(float dt)
	{
		bool flag = false;
		List<ProcessCondition> v;
		using (ProcessCondition.ListPool.Get(out v))
		{
			PopulateConditionSet(ProcessCondition.ProcessConditionType.All, v);
			foreach (ProcessCondition item in v)
			{
				if (item.EvaluateCondition() == ProcessCondition.Status.Failure)
				{
					flag = true;
					if (statusHandle == Guid.Empty)
					{
						statusHandle = kselectable.AddStatusItem(Db.Get().BuildingStatusItems.RocketChecklistIncomplete);
					}
					break;
				}
			}
		}
		if (!flag && statusHandle != Guid.Empty)
		{
			kselectable.RemoveStatusItem(statusHandle);
		}
	}
}
