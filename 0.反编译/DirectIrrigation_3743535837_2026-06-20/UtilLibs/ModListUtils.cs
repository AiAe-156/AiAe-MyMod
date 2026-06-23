using System.Collections.Generic;
using KMod;

namespace UtilLibs;

public class ModListUtils
{
	public static Dictionary<string, string> WorkshopModIDs = new Dictionary<string, string>
	{
		{ "TrueTiles", "2815406414" },
		{ "Amorbus", "2899109675" }
	};

	public static bool ModIsActive(string modId)
	{
		Manager modManager = Global.Instance.modManager;
		foreach (Mod mod in modManager.mods)
		{
			if (!mod.IsEnabledForActiveDlc() || (!(mod.staticID == modId) && !mod.staticID.Contains(modId) && !(mod.staticID == WorkshopModIDs[modId])))
			{
				continue;
			}
			return true;
		}
		return false;
	}
}
