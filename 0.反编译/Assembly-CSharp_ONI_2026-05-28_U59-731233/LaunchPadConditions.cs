using System.Collections.Generic;

public class LaunchPadConditions : KMonoBehaviour, IProcessConditionSet
{
	private List<ProcessCondition> conditions;

	public List<ProcessCondition> GetConditionSet(ProcessCondition.ProcessConditionType conditionType)
	{
		return (conditionType == ProcessCondition.ProcessConditionType.RocketStorage) ? conditions : null;
	}

	public int PopulateConditionSet(ProcessCondition.ProcessConditionType conditionType, List<ProcessCondition> conditions)
	{
		int num = 0;
		if (conditionType == ProcessCondition.ProcessConditionType.RocketStorage)
		{
			conditions.AddRange(this.conditions);
			num += this.conditions.Count;
		}
		return num;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		conditions = new List<ProcessCondition>();
		conditions.Add(new TransferCargoCompleteCondition(base.gameObject));
	}
}
