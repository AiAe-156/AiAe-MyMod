using System.Collections.Generic;

namespace Database;

public class MooSongModifiers : ResourceSet<MooSongModifier>
{
	public List<MooSongModifier> GetForTag(Tag searchTag)
	{
		List<MooSongModifier> list = new List<MooSongModifier>();
		foreach (MooSongModifier resource in resources)
		{
			if (resource.TargetTag == searchTag)
			{
				list.Add(resource);
			}
		}
		return list;
	}
}
