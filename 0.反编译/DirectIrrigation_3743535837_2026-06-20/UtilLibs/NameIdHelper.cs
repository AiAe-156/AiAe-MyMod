using System.Collections.Generic;
using STRINGS;
using UnityEngine;

namespace UtilLibs;

public static class NameIdHelper
{
	public static Dictionary<string, string> NamesById = new Dictionary<string, string>();

	public static bool TryGetIdFromName(string name, out string Id)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		name = name.ToLowerInvariant();
		Id = name;
		if ((Object)(object)Db.Get() == (Object)null)
		{
			SgtLogger.error("Db not initialized yet!");
			return false;
		}
		if (NamesById.TryGetValue(name, out var value))
		{
			Id = value;
			return true;
		}
		foreach (KeyValuePair<Tag, KPrefabID> item in Assets.PrefabsByTag)
		{
			string properName = KSelectableExtensions.GetProperName(((Component)item.Value).gameObject);
			properName = UI.StripLinkFormatting(properName).ToLowerInvariant();
			NamesById[properName] = ((object)item.Key/*cast due to .constrained prefix*/).ToString();
			if (properName == name)
			{
				Id = ((object)item.Key/*cast due to .constrained prefix*/).ToString();
				return true;
			}
		}
		SgtLogger.warning("could not find prefab with the name " + name);
		return false;
	}
}
