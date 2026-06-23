using System.Collections.Generic;
using System.IO;
using FUtility;
using FUtility.FUI;
using TMPro;
using TrueTiles.Cmps;
using TrueTiles.Settings.Unity_UI_Extensions.Scripts.Controls.ReorderableList;
using UnityEngine;
using UnityEngine.UI;

namespace TrueTiles.Settings;

public class SettingsScreen : FScreen
{
	private PackEntry entryPrefab;

	private Transform entryParent;

	private FToggle2 saveExternally;

	private List<PackEntry> entries;

	public override void SetObjects()
	{
		base.SetObjects();
		saveExternally = Util.FindOrAddComponent<FToggle2>((Component)(object)((KMonoBehaviour)this).transform.Find("Buttons/ExternalSaveConfirm"));
		saveExternally.mark = ((Component)((KMonoBehaviour)saveExternally).transform.Find("Background/Checkmark")).GetComponent<Image>();
		Helper.AddSimpleToolTip(((Component)saveExternally).gameObject, LocString.op_Implicit(STRINGS.UI.SETTINGSDIALOG.BUTTONS.EXTERNALSAVECONFIRM.TOOLTIP));
		entryParent = ((KMonoBehaviour)this).transform.Find("ScrollView/Viewport/Content");
		entryPrefab = ((Component)entryParent.Find("Entry")).gameObject.AddComponent<PackEntry>();
		((Component)entryPrefab).gameObject.SetActive(false);
		ReorderableList reorderableList = Util.FindOrAddComponent<ReorderableList>((Component)(object)entryParent);
		reorderableList.ContentLayout = (LayoutGroup)(object)Util.FindComponent<VerticalLayoutGroup>((Component)(object)entryParent);
		reorderableList.IsDraggable = true;
		reorderableList.IsDropable = true;
		((TMP_Text)((Component)((KMonoBehaviour)this).transform.Find("VersionLabel")).GetComponent<LocText>()).text = "v" + Log.GetVersion();
	}

	public override void ShowDialog()
	{
		saveExternally.On = Mod.Settings.SaveExternally;
		base.ShowDialog();
		entries = new List<PackEntry>();
		foreach (PackData pack in TexturePacksManager.Instance.packs)
		{
			if (pack.IsValid)
			{
				PackEntry packEntry = Object.Instantiate<PackEntry>(entryPrefab, entryParent);
				((Component)packEntry).gameObject.SetActive(true);
				packEntry.SetTitle(pack.Name);
				packEntry.SetIcon(pack.Icon);
				packEntry.SetDescription(pack.Author, pack.TextureCount);
				packEntry.SetEnabled(pack.Enabled);
				packEntry.SetFolder(pack.Root);
				packEntry.SetTooltip(pack.Description);
				Util.FindOrAddComponent<ReorderableListElement>((Component)(object)packEntry);
				packEntry.Id = pack.Id;
				entries.Add(packEntry);
			}
		}
	}

	public override void OnClickApply()
	{
		if (!saveExternally.On && Directory.Exists(Mod.GetExternalSavePath()))
		{
			Directory.Delete(Mod.GetExternalSavePath(), recursive: true);
		}
		SaveAll();
	}

	private void SaveAll()
	{
		foreach (PackData pack in TexturePacksManager.Instance.packs)
		{
			PackEntry packEntry = entries.Find((PackEntry e) => e.Id == pack.Id);
			if ((Object)(object)packEntry != (Object)null)
			{
				pack.Enabled = packEntry.IsEnabled();
				pack.Order = ((Component)packEntry).GetComponent<Transform>().GetSiblingIndex();
			}
		}
		Mod.Settings.SaveExternally = saveExternally.On;
		Mod.SaveConfig();
		string root = (saveExternally.On ? Mod.GetExternalSavePath() : Mod.GetLocalSavePath());
		TexturePacksManager.Instance.SavePacks(root);
		TileAssetLoader.Instance.ReloadAssets();
		((KScreen)this).Deactivate();
	}
}
