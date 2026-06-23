using System.Collections.Generic;
using KMod;
using PeterHan.PLib.Core;

namespace UtilLibs.ModSyncing;

public static class ModSyncUtils
{
	public const string SyncModKey = "Sgt_Imalas_SyncModsKey";

	public static bool IsModSyncMod(string defaultStaticModID)
	{
		List<string> data = PRegistry.GetData<List<string>>("Sgt_Imalas_SyncModsKey");
		if (data == null)
		{
			return false;
		}
		return data.Contains(defaultStaticModID) || defaultStaticModID.ToLowerInvariant().Contains("modupdatedate") || defaultStaticModID.Contains("2018291283");
	}

	public static bool IsModSyncMod(Mod mod)
	{
		return IsModSyncMod(((Label)(ref mod.label)).defaultStaticID);
	}

	public static void RegisterModAsSyncMod(Mod mod)
	{
		List<string> list = PRegistry.GetData<List<string>>("Sgt_Imalas_SyncModsKey");
		if (list == null || list.Count == 0)
		{
			list = new List<string>();
		}
		list.Add(((Label)(ref mod.label)).defaultStaticID);
		PRegistry.PutData("Sgt_Imalas_SyncModsKey", list);
	}
}
