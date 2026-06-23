using System.Collections.Generic;

public class VoidChoreProvider : ChoreProvider
{
	public static VoidChoreProvider Instance;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
	}

	public override void AddChore(Chore chore)
	{
	}

	public override void RemoveChore(Chore chore)
	{
	}

	public override void CollectChores(ChoreConsumerState consumer_state, List<Chore.Precondition.Context> succeeded, List<Chore.Precondition.Context> failed_contexts)
	{
	}
}
