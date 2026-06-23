using System.Collections.Generic;
using PeterHan.PLib.Core;

namespace UtilLibs;

public static class DuperyShared
{
	private const string MouthDataKey = "Dupery_CustomCheekAnims";

	public static void RegisterMouthSymbolForCustomCheek(string mouth, string anim)
	{
		Dictionary<string, string> dictionary = PRegistry.GetData<Dictionary<string, string>>("Dupery_CustomCheekAnims");
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, string>();
		}
		dictionary[mouth] = anim;
		PRegistry.PutData("Dupery_CustomCheekAnims", dictionary);
	}

	public static bool HasCustomCheekAnimForMouth(string mouth, out KAnimFile kanim)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		kanim = null;
		Dictionary<string, string> data = PRegistry.GetData<Dictionary<string, string>>("Dupery_CustomCheekAnims");
		if (data == null)
		{
			return false;
		}
		string value;
		return data.TryGetValue(mouth, out value) && Assets.TryGetAnim(HashedString.op_Implicit(value), ref kanim);
	}
}
