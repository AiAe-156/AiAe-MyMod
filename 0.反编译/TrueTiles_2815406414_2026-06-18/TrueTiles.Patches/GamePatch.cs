using System;
using System.Reflection;
using HarmonyLib;

namespace TrueTiles.Patches;

public class GamePatch
{
	public class Game_OnSpawn_Patch
	{
		public static void Patch(Harmony harmony)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			MethodInfo methodInfo = AccessTools.Method("Game, Assembly-CSharp:OnSpawn", (Type[])null, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(Game_OnSpawn_Patch), "Postfix", (Type[])null, (Type[])null);
			harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo2), (HarmonyMethod)null, (HarmonyMethod)null);
		}

		public static void Postfix()
		{
			ElementGrid.Initialize();
		}
	}
}
