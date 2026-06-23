using System.Collections.Generic;
using STRINGS;
using UnityEngine;

namespace TUNING;

public static class GARNISHES
{
	public static readonly List<GarnishInfo> AllGarnishes = new List<GarnishInfo>
	{
		new GarnishInfo
		{
			itemTag = TableSaltConfig.TAG,
			effectId = "MessTableSalt",
			consumeRate = TableSaltTuning.CONSUMABLE_RATE,
			storageCapacity = TableSaltTuning.SALTSHAKERSTORAGEMASS,
			priority = 0,
			descriptor = new Descriptor(string.Format(UI.BUILDINGEFFECTS.MESS_TABLE_SALT, TableSaltTuning.MORALE_MODIFIER), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.MESS_TABLE_SALT, TableSaltTuning.MORALE_MODIFIER))
		},
		new GarnishInfo
		{
			itemTag = CaviarConfig.TAG,
			effectId = "MessCaviar",
			consumeRate = CaviarTuning.CONSUMABLE_RATE,
			storageCapacity = CaviarTuning.STORAGEMASS,
			priority = 10,
			descriptor = new Descriptor(string.Format(UI.BUILDINGEFFECTS.MESS_CAVIAR, CaviarTuning.MORALE_MODIFIER, CaviarTuning.STRESS_MODIFIER), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.MESS_CAVIAR, CaviarTuning.MORALE_MODIFIER, CaviarTuning.STRESS_MODIFIER)),
			overrideAnimName = "caviarshaker_kanim",
			overrideSymbolName = "object",
			fxTintColor = Color.black
		}
	};

	public static GarnishInfo GetActiveGarnish(Storage storage)
	{
		if (storage == null)
		{
			return null;
		}
		GarnishInfo garnishInfo = null;
		foreach (GarnishInfo allGarnish in AllGarnishes)
		{
			if (!(storage.GetMassAvailable(allGarnish.itemTag) < allGarnish.consumeRate) && (garnishInfo == null || allGarnish.priority > garnishInfo.priority))
			{
				garnishInfo = allGarnish;
			}
		}
		return garnishInfo;
	}

	public static bool HasAnyGarnish(Storage storage)
	{
		return GetActiveGarnish(storage) != null;
	}
}
