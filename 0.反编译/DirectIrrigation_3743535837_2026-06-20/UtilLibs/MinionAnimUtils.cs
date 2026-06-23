using System.Collections.Generic;
using Database;

namespace UtilLibs;

public static class MinionAnimUtils
{
	public static string[] symbolOverridesToRemove = new string[9] { "snapTo_hat", "snapTo_hat_hair", "snapTo_neck", "snapTo_goggles", "snapTo_headfx", "snapTo_chest", "snapTo_pivot", "skirt", "necklace" };

	public static void ApplyNewAccessories(KBatchedAnimController kbac, SymbolOverrideController soc, List<KeyValuePair<string, string>> accessories)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		soc.RemoveAllSymbolOverrides(0);
		string[] array = symbolOverridesToRemove;
		foreach (string text in array)
		{
			soc.RemoveSymbolOverride(HashedString.op_Implicit(text), 0);
			((KAnimControllerBase)kbac).SetSymbolVisiblity(KAnimHashedString.op_Implicit(text), false);
		}
		Db val = Db.Get();
		Accessories accessories2 = val.Accessories;
		AccessorySlots accessorySlots = val.AccessorySlots;
		foreach (KeyValuePair<string, string> accessory in accessories)
		{
			if (((ResourceSet<Accessory>)(object)accessories2).Exists(accessory.Value))
			{
				Symbol symbol = ((ResourceSet<Accessory>)(object)accessories2).Get(accessory.Value).symbol;
				AccessorySlot val2 = ((ResourceSet<AccessorySlot>)(object)accessorySlots).Get(accessory.Key);
				soc.AddSymbolOverride(HashedString.op_Implicit(val2.targetSymbolId), symbol, 0);
				((KAnimControllerBase)kbac).SetSymbolVisiblity(KAnimHashedString.op_Implicit(accessory.Key), true);
			}
		}
		soc.ApplyOverrides();
	}
}
