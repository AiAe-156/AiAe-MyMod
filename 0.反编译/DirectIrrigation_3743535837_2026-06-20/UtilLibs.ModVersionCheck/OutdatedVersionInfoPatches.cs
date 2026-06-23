using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UtilLibs.ModVersionCheck;

internal class OutdatedVersionInfoPatches
{
	public static class MainMenuMissingModsContainerInit
	{
		public static void InitMainMenuInfoPatch()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Expected O, but got Unknown
			Harmony val = new Harmony("SGT_IMALAS_VERSION_INFO");
			if (VersionChecker.OlderVersion)
			{
				return;
			}
			Type type = AccessTools.TypeByName("MainMenu");
			if (type == null)
			{
				SgtLogger.warning("MainMenu was null");
				return;
			}
			SgtLogger.l("patching MainMenu.OnPrefabInit");
			MethodInfo methodInfo = AccessTools.Method(type, "OnPrefabInit", (Type[])null, (Type[])null);
			if (methodInfo == null)
			{
				SgtLogger.warning("MainMenu.OnPrefabInit was null!");
				return;
			}
			if (VersionChecker.HasPatch(out var harmonyID))
			{
				val.Unpatch((MethodBase)methodInfo, (HarmonyPatchType)2, harmonyID);
			}
			MethodInfo methodInfo2 = AccessTools.Method(typeof(MainMenuMissingModsContainerInit), "Postfix", (Type[])null, (Type[])null);
			val.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo2, 300, (string[])null, (string[])null, (bool?)null), (HarmonyMethod)null, (HarmonyMethod)null);
			VersionChecker.SetIsPatched(val.Id);
		}

		public static void Postfix(MainMenu __instance)
		{
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
			if (VersionChecker.OlderVersion || VersionChecker.UI_Built())
			{
				return;
			}
			string assemblyOverride = Assembly.GetExecutingAssembly().GetName().Name + "/Sgt_Imalas-VersionChecker";
			VersionChecker.SetUIConstructed(constructed: true);
			SgtLogger.l("Current UI handler version: " + PRegistry.GetData<int>("Sgt_Imalas_UI_VersionData"), assemblyOverride);
			if (!VersionChecker.ModsOutOfDate(50, out var missingModsInfo, out var linecount))
			{
				SgtLogger.l("no mods out of date", assemblyOverride);
				return;
			}
			SgtLogger.l("grabbing ref.", assemblyOverride);
			OptionsMenuScreen val = Util.KInstantiateUI<OptionsMenuScreen>(((Component)ScreenPrefabs.Instance.OptionsScreen).gameObject, (GameObject)null, false);
			SgtLogger.Assert("options", val);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			FeedbackScreen val2 = Util.KInstantiateUI<FeedbackScreen>(((Component)val.feedbackScreenPrefab).gameObject, (GameObject)null, false);
			SgtLogger.Assert("feedbackClone", val2);
			if ((Object)(object)val2 == (Object)null)
			{
				return;
			}
			GameObject val3 = Util.KInstantiateUI(((Component)((KMonoBehaviour)val2).transform.Find("Content/GameObject/InfoBox")).gameObject, (GameObject)null, false);
			SgtLogger.Assert("_infoBox", val3);
			Object.Destroy((Object)(object)((Component)val).gameObject);
			Object.Destroy((Object)(object)((Component)val2).gameObject);
			if ((Object)(object)val3 == (Object)null)
			{
				return;
			}
			GameObject val4 = null;
			GameObject val5 = null;
			int num = 75 + linecount * 20;
			Edge val6 = (Edge)2;
			Edge val7 = (Edge)0;
			int num2 = 250;
			int num3 = 25;
			if ((Object)(object)((KMonoBehaviour)__instance).transform.Find("MainMenuMenubar/BottomRow") != (Object)null)
			{
				val4 = ((Component)__instance).gameObject;
				val5 = Util.KInstantiateUI(val3, val4, true);
				val6 = (Edge)3;
				val7 = (Edge)1;
				num2 = 245;
				num3 = 25;
			}
			else if ((Object)(object)((KMonoBehaviour)__instance).transform.Find("UI Group") != (Object)null)
			{
				val4 = ((Component)((KMonoBehaviour)__instance).transform.Find("UI Group")).gameObject;
				val5 = Util.KInstantiateUI(val3, val4, true);
				val6 = (Edge)2;
				val7 = (Edge)0;
				num2 = 25;
				num3 = 325;
			}
			if ((Object)(object)val5 != (Object)null)
			{
				((Object)val5).name = "SGT_IMALAS_VERSION_INFO";
				LocText val8 = default(LocText);
				if (((Component)val5.transform.Find("Header")).gameObject.TryGetComponent<LocText>(ref val8))
				{
					((TMP_Text)val8).text = UIUtils.ColorText("Outdated Mods:", UIUtils.rgb(237f, 89f, 92f));
				}
				Transform val9 = val5.transform.Find("Description");
				LocText val10 = default(LocText);
				if (((Component)val9).gameObject.TryGetComponent<LocText>(ref val10))
				{
					((TMP_Text)val10).text = missingModsInfo;
				}
				if (Object.op_Implicit((Object)(object)val5.transform.Find("BG")))
				{
					Util.FindOrAddComponent<Outline>((Component)(object)val5.transform.Find("BG"));
				}
				RectTransform val11 = Util.rectTransform(val5);
				val11.SetInsetAndSizeFromParentEdge(val7, (float)num3, 298f);
				val11.SetInsetAndSizeFromParentEdge(val6, (float)num2, (float)num);
			}
		}
	}

	public const string UICMPName = "SGT_IMALAS_VERSION_INFO";
}
