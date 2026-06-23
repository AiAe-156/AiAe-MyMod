using System.Collections.Generic;
using FUtility.FUI;
using HarmonyLib;
using KMod;
using TMPro;
using TrueTiles.Settings;
using UnityEngine;

namespace TrueTiles.Patches;

public class ModsScreenPatch
{
	[HarmonyPatch(typeof(ModsScreen), "BuildDisplay")]
	public static class ModsScreen_BuildDisplay_Patch
	{
		public static void Postfix(ModsScreen __instance)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			List<Mod> mods = Global.Instance.modManager.mods;
			HierarchyReferences val2 = default(HierarchyReferences);
			foreach (DisplayedMod displayedMod in __instance.displayedMods)
			{
				if (displayedMod.mod_index > mods.Count - 1 || displayedMod.mod_index < 0)
				{
					continue;
				}
				Mod val = mods[displayedMod.mod_index];
				if (Mod.moddedPacksPaths.Contains(val.ContentPath))
				{
					PretendEnableMod(displayedMod, val);
					if (((Component)displayedMod.rect_transform).TryGetComponent<HierarchyReferences>(ref val2))
					{
						Transform transform = ((KMonoBehaviour)val2.GetReference<KButton>("ManageButton")).transform;
						Helper.MakeKButton(new Helper.ButtonInfo(LocString.op_Implicit("Texture Packs"), OpenModSettingsScreen, 12), ((Component)transform).gameObject, ((Component)transform.parent).gameObject, transform.GetSiblingIndex() - 1);
					}
				}
			}
		}

		private static void PretendEnableMod(DisplayedMod displayedMod, Mod mod)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Invalid comparison between Unknown and I4
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			HierarchyReferences val = default(HierarchyReferences);
			if ((int)mod.available_content == 0 && (int)mod.contentCompatability == 2 && ((Component)displayedMod.rect_transform).TryGetComponent<HierarchyReferences>(ref val))
			{
				((TMP_Text)val.GetReference<LocText>("Title")).text = mod.title + "\n<color=#e485e6><size=70%>Mod managed by True Tiles.</size></color>";
				((Component)val.GetReference<MultiToggle>("EnabledToggle")).gameObject.SetActive(false);
				KImage reference = val.GetReference<KImage>("BG");
				reference.defaultState = (ColorSelector)1;
				reference.ColorState = (ColorSelector)1;
			}
		}

		private static void OpenModSettingsScreen()
		{
			Helper.CreateFDialog<SettingsScreen>(ModAssets.Prefabs.settingsDialog, "TrueTilesSettings");
		}
	}
}
