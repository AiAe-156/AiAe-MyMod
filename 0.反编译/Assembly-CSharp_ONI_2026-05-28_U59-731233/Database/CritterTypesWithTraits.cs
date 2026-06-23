using System.Collections.Generic;

namespace Database;

public class CritterTypesWithTraits : ColonyAchievementRequirement, AchievementRequirementSerialization_Deprecated
{
	public Dictionary<Tag, bool> critterTypesToCheck = new Dictionary<Tag, bool>();

	private Tag trait;

	private bool hasTrait;

	private bool allRequired = true;

	private Dictionary<Tag, bool> revisedCritterTypesToCheckState = new Dictionary<Tag, bool>();

	public CritterTypesWithTraits(List<Tag> critterTypes)
		: this(critterTypes, allRequired: true)
	{
	}

	public CritterTypesWithTraits(List<Tag> critterTypes, bool allRequired)
	{
		foreach (Tag critterType in critterTypes)
		{
			if (!critterTypesToCheck.ContainsKey(critterType))
			{
				critterTypesToCheck.Add(critterType, value: false);
			}
		}
		hasTrait = false;
		this.allRequired = allRequired;
		trait = GameTags.Creatures.Wild;
	}

	public override bool Success()
	{
		HashSet<Tag> tamedCritterTypes = SaveGame.Instance.ColonyAchievementTracker.tamedCritterTypes;
		bool flag = allRequired;
		foreach (KeyValuePair<Tag, bool> item in critterTypesToCheck)
		{
			flag = ((!allRequired) ? (flag || tamedCritterTypes.Contains(item.Key)) : (flag && tamedCritterTypes.Contains(item.Key)));
		}
		UpdateSavedState();
		return flag;
	}

	public void UpdateSavedState()
	{
		revisedCritterTypesToCheckState.Clear();
		HashSet<Tag> tamedCritterTypes = SaveGame.Instance.ColonyAchievementTracker.tamedCritterTypes;
		foreach (KeyValuePair<Tag, bool> item in critterTypesToCheck)
		{
			revisedCritterTypesToCheckState.Add(item.Key, tamedCritterTypes.Contains(item.Key));
		}
		foreach (KeyValuePair<Tag, bool> item2 in revisedCritterTypesToCheckState)
		{
			critterTypesToCheck[item2.Key] = item2.Value;
		}
	}

	public void Deserialize(IReader reader)
	{
		critterTypesToCheck = new Dictionary<Tag, bool>();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string name = reader.ReadKleiString();
			bool value = reader.ReadByte() != 0;
			critterTypesToCheck.Add(new Tag(name), value);
		}
		hasTrait = reader.ReadByte() != 0;
		trait = GameTags.Creatures.Wild;
	}
}
