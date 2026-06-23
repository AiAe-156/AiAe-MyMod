using System.Collections.Generic;
using KSerialization;

[SerializationConfig(MemberSerialization.OptIn)]
public class GridRestrictionSerializer : KMonoBehaviour, ISaveLoadable
{
	public static GridRestrictionSerializer Instance;

	private List<KeyValuePair<Tag, int>> tagToId = new List<KeyValuePair<Tag, int>>
	{
		new KeyValuePair<Tag, int>(GameTags.Minions.Models.Standard, -1),
		new KeyValuePair<Tag, int>(GameTags.Minions.Models.Bionic, -2),
		new KeyValuePair<Tag, int>(GameTags.Robot, -3),
		new KeyValuePair<Tag, int>(GameTags.Robots.Models.FetchDrone, -4),
		new KeyValuePair<Tag, int>(GameTags.Robots.Models.ScoutRover, -5),
		new KeyValuePair<Tag, int>(GameTags.Robots.Models.MorbRover, -6)
	};

	private Tag[] robotTypeTags = new Tag[3]
	{
		GameTags.Robots.Models.FetchDrone,
		GameTags.Robots.Models.ScoutRover,
		GameTags.Robots.Models.MorbRover
	};

	public Tag[] ValidRobotTypes => robotTypeTags;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
	}

	public int GetTagId(Tag gameTag)
	{
		foreach (KeyValuePair<Tag, int> item in tagToId)
		{
			if (item.Key == gameTag)
			{
				return item.Value;
			}
		}
		DebugUtil.DevAssert(test: false, "Gametag " + gameTag.Name + " has not been added to the valid list of GridRestrictionTagId's before requesting the ID");
		return 0;
	}
}
