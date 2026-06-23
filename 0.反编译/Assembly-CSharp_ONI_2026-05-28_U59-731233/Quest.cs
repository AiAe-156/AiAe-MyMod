public class Quest : Resource
{
	public struct ItemData
	{
		public int LocalCellId;

		public float CurrentValue;

		public Tag SatisfyingItem;

		public Tag QualifyingTag;

		public HashedString CriteriaId;

		private int valueHandle;

		public int ValueHandle
		{
			get
			{
				return valueHandle - 1;
			}
			set
			{
				valueHandle = value + 1;
			}
		}
	}

	public enum State
	{
		NotStarted,
		InProgress,
		Completed
	}

	public const string STRINGS_PREFIX = "STRINGS.CODEX.QUESTS.";

	public readonly QuestCriteria[] Criteria = null;

	public readonly string Title = null;

	public readonly string CompletionText = null;

	public Quest(string id, QuestCriteria[] criteria)
		: base(id, id)
	{
		Debug.Assert(criteria.Length != 0);
		Criteria = criteria;
		string text = "STRINGS.CODEX.QUESTS." + id.ToUpperInvariant();
		if (Strings.TryGet(text + ".NAME", out var result))
		{
			Title = result.String;
		}
		if (Strings.TryGet(text + ".COMPLETE", out result))
		{
			CompletionText = result.String;
		}
		for (int i = 0; i < Criteria.Length; i++)
		{
			Criteria[i].PopulateStrings("STRINGS.CODEX.QUESTS.");
		}
	}
}
