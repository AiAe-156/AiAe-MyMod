using System.Collections.Generic;
using UnityEngine.Pool;

public abstract class ProcessCondition
{
	public enum ProcessConditionType
	{
		RocketFlight,
		RocketPrep,
		RocketStorage,
		RocketBoard,
		All
	}

	public enum Status
	{
		Failure,
		Warning,
		Ready
	}

	protected ProcessCondition parentCondition = null;

	public static ObjectPool<List<ProcessCondition>> ListPool = new ObjectPool<List<ProcessCondition>>(() => new List<ProcessCondition>(16), null, delegate(List<ProcessCondition> list)
	{
		list.Clear();
	}, null, collectionCheck: false, 4, 4);

	public abstract Status EvaluateCondition();

	public abstract bool ShowInUI();

	public abstract string GetStatusMessage(Status status);

	public string GetStatusMessage()
	{
		return GetStatusMessage(EvaluateCondition());
	}

	public abstract string GetStatusTooltip(Status status);

	public string GetStatusTooltip()
	{
		return GetStatusTooltip(EvaluateCondition());
	}

	public virtual StatusItem GetStatusItem(Status status)
	{
		return null;
	}

	public virtual ProcessCondition GetParentCondition()
	{
		return parentCondition;
	}
}
