using System;
using System.Collections.Generic;
using System.IO;
using Klei;
using STRINGS;
using UnityEngine;

public static class CodexCache
{
	private static string baseEntryPath = null;

	public static Dictionary<string, CodexEntry> entries = null;

	public static Dictionary<string, SubEntry> subEntries = null;

	private static Dictionary<string, List<string>> unlockedEntryLookup;

	private static List<Tuple<string, Type>> widgetTagMappings;

	public static List<CategoryEntry> categoriesForPostYAMLPopulation = new List<CategoryEntry>();

	public static string FormatLinkID(string linkID)
	{
		linkID = linkID.ToUpper();
		linkID = linkID.Replace("_", "");
		return linkID;
	}

	public static void CodexCacheInit()
	{
		entries = new Dictionary<string, CodexEntry>();
		subEntries = new Dictionary<string, SubEntry>();
		unlockedEntryLookup = new Dictionary<string, List<string>>();
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		if (widgetTagMappings == null)
		{
			widgetTagMappings = new List<Tuple<string, Type>>
			{
				new Tuple<string, Type>("!CodexText", typeof(CodexText)),
				new Tuple<string, Type>("!CodexImage", typeof(CodexImage)),
				new Tuple<string, Type>("!CodexDividerLine", typeof(CodexDividerLine)),
				new Tuple<string, Type>("!CodexSpacer", typeof(CodexSpacer)),
				new Tuple<string, Type>("!CodexLabelWithIcon", typeof(CodexLabelWithIcon)),
				new Tuple<string, Type>("!CodexLabelWithLargeIcon", typeof(CodexLabelWithLargeIcon)),
				new Tuple<string, Type>("!CodexContentLockedIndicator", typeof(CodexContentLockedIndicator)),
				new Tuple<string, Type>("!CodexLargeSpacer", typeof(CodexLargeSpacer)),
				new Tuple<string, Type>("!CodexVideo", typeof(CodexVideo)),
				new Tuple<string, Type>("!CodexElementCategoryList", typeof(CodexElementCategoryList))
			};
		}
		string text = "";
		text = FormatLinkID("DUPLICANTSCATEGORY");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.DUPLICANTS, CodexEntryGenerator.GenerateDuplicantEntries(), Assets.GetSprite("codexIconDupes"), largeFormat: true, sort: false, UI.CODEX.CATEGORYNAMES.DUPLICANTS));
		text = FormatLinkID("LESSONS");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.TUTORIALS, CodexEntryGenerator.GenerateTutorialNotificationEntries(), Assets.GetSprite("codexIconLessons"), largeFormat: true, sort: true, UI.CODEX.CATEGORYNAMES.TUTORIALS));
		text = FormatLinkID("creatures");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.CREATURES, CodexEntryGenerator_Creatures.GenerateEntries(), Assets.GetSprite("codexIconCritters"), largeFormat: true, sort: false));
		DebugUtil.DevAssert(text == "CREATURES", string.Empty);
		text = FormatLinkID("plants");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.PLANTS, CodexEntryGenerator.GeneratePlantEntries(), Assets.GetSprite("codexIconPlants")));
		text = FormatLinkID("food");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.FOOD, CodexEntryGenerator.GenerateFoodEntries(), Assets.GetSprite("codexIconFood")));
		text = FormatLinkID("buildings");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.BUILDINGS, CodexEntryGenerator.GenerateBuildingEntries(), Assets.GetSprite("codexIconBuildings")));
		text = FormatLinkID("tech");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.TECH, CodexEntryGenerator.GenerateTechEntries(), Assets.GetSprite("codexIconResearch")));
		text = FormatLinkID("disease");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.DISEASE, CodexEntryGenerator.GenerateDiseaseEntries(), Assets.GetSprite("codexIconDisease"), largeFormat: false));
		text = FormatLinkID("elements");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.ELEMENTS, CodexEntryGenerator_Elements.GenerateEntries(), Assets.GetSprite("codexIconElements"), largeFormat: true, sort: false));
		text = FormatLinkID("BUILDINGMATERIALCLASSES");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.BUILDINGMATERIALCLASSES, CodexEntryGenerator.GenerateConstructionMaterialEntries(), Assets.GetSprite("ui_elements_classes"), largeFormat: true, sort: false));
		text = FormatLinkID("geysers");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.GEYSERS, CodexEntryGenerator.GenerateGeyserEntries(), Assets.GetSprite("codexIconGeysers")));
		text = FormatLinkID("equipment");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.EQUIPMENT, CodexEntryGenerator.GenerateEquipmentEntries(), Assets.GetSprite("codexIconEquipment")));
		text = FormatLinkID("biomes");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.BIOMES, CodexEntryGenerator.GenerateBiomeEntries(), Assets.GetSprite("codexIconGeysers")));
		text = FormatLinkID("rooms");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.ROOMS, CodexEntryGenerator.GenerateRoomsEntries(), Assets.GetSprite("codexIconRooms")));
		text = FormatLinkID("STORYTRAITS");
		dictionary.Add(text, CodexEntryGenerator.GenerateCategoryEntry(text, UI.CODEX.CATEGORYNAMES.STORYTRAITS, new Dictionary<string, CodexEntry>(), Assets.GetSprite("codexIconStoryTraits")));
		if (Game.IsDlcActiveForCurrentSave("DLC3_ID"))
		{
			CodexEntryGenerator.GenerateBionicUpgradeEntries();
			CodexEntryGenerator.GenerateElectrobankEntries();
		}
		CategoryEntry item = CodexEntryGenerator.GenerateCategoryEntry(FormatLinkID("HOME"), UI.CODEX.CATEGORYNAMES.ROOT, dictionary);
		CodexEntryGenerator.GeneratePageNotFound();
		List<CategoryEntry> list = new List<CategoryEntry>();
		foreach (KeyValuePair<string, CodexEntry> item2 in dictionary)
		{
			list.Add(item2.Value as CategoryEntry);
		}
		CollectYAMLEntries(list);
		CollectYAMLSubEntries(list);
		CheckUnlockableContent();
		list.Add(item);
		Comparison<SubEntry> comparison = (SubEntry a, SubEntry b) => a.layoutPriority.CompareTo(b.layoutPriority);
		List<ContentContainer> list2 = new List<ContentContainer>();
		foreach (KeyValuePair<string, CodexEntry> entry in entries)
		{
			CodexEntry value = entry.Value;
			list2.Clear();
			list2.AddRange(value.contentContainers);
			foreach (CodexEntry_MadeAndUsed item3 in value.contentMadeAndUsed)
			{
				Element element = ElementLoader.GetElement(item3.tag);
				if (element != null)
				{
					CodexEntryGenerator_Elements.GenerateElementDescriptionContainers(element, list2);
				}
				else
				{
					CodexEntryGenerator_Elements.GenerateMadeAndUsedContainers(item3.tag, list2);
				}
			}
			value.subEntries.Sort(comparison);
			if (value.icon == null)
			{
				foreach (SubEntry subEntry in value.subEntries)
				{
					if (subEntry.icon != null)
					{
						value.icon = subEntry.icon;
						value.iconColor = subEntry.iconColor;
						break;
					}
				}
			}
			if (value.subEntries.Count > 1)
			{
				int num = 0;
				foreach (SubEntry subEntry2 in value.subEntries)
				{
					if (subEntry2.lockID != null && !Game.Instance.unlocks.IsUnlocked(subEntry2.lockID))
					{
						num++;
					}
				}
				List<ICodexWidget> list3 = new List<ICodexWidget>();
				list3.Add(new CodexSpacer());
				list3.Add(new CodexText(string.Format(CODEX.HEADERS.SUBENTRIES, value.subEntries.Count - num, value.subEntries.Count), CodexTextStyle.Subtitle));
				foreach (SubEntry subEntry3 in value.subEntries)
				{
					if (subEntry3.lockID != null && !Game.Instance.unlocks.IsUnlocked(subEntry3.lockID))
					{
						list3.Add(new CodexText(UI.FormatAsLink(CODEX.HEADERS.CONTENTLOCKED, UI.ExtractLinkID(subEntry3.name))));
						continue;
					}
					string text2 = UI.StripLinkFormatting((subEntry3.name == null) ? ((string)Strings.Get(subEntry3.title)) : subEntry3.name);
					text2 = UI.FormatAsLink(text2, subEntry3.id);
					list3.Add(new CodexText(text2));
				}
				list3.Add(new CodexSpacer());
				list2.Add(new ContentContainer(list3, ContentContainer.ContentLayout.Vertical));
			}
			foreach (SubEntry subEntry4 in value.subEntries)
			{
				list2.AddRange(subEntry4.contentContainers);
				foreach (CodexEntry_MadeAndUsed item4 in subEntry4.contentMadeAndUsed)
				{
					Element element2 = ElementLoader.GetElement(item4.tag);
					if (element2 != null)
					{
						CodexEntryGenerator_Elements.GenerateElementDescriptionContainers(element2, list2);
					}
					else
					{
						CodexEntryGenerator_Elements.GenerateMadeAndUsedContainers(item4.tag, list2);
					}
				}
			}
			value.contentContainers.Clear();
			value.AddContentContainerRange(list2);
		}
		CodexEntryGenerator.PopulateCategoryEntries(list, delegate(CodexEntry a, CodexEntry b)
		{
			if (a.name == UI.CODEX.CATEGORYNAMES.TIPS)
			{
				return -1;
			}
			return (b.name == UI.CODEX.CATEGORYNAMES.TIPS) ? 1 : UI.StripLinkFormatting(a.name).CompareTo(UI.StripLinkFormatting(b.name));
		});
		CodexEntryGenerator.PopulateCategoryEntries(categoriesForPostYAMLPopulation);
		categoriesForPostYAMLPopulation.Clear();
	}

	public static CodexEntry FindEntry(string id)
	{
		if (entries == null)
		{
			Debug.LogWarning("Can't search Codex cache while it's stil null");
			return null;
		}
		if (entries.ContainsKey(id))
		{
			return entries[id];
		}
		Debug.LogWarning("Could not find codex entry with id: " + id);
		return null;
	}

	public static SubEntry FindSubEntry(string id)
	{
		foreach (KeyValuePair<string, CodexEntry> entry in entries)
		{
			foreach (SubEntry subEntry in entry.Value.subEntries)
			{
				if (subEntry.id.ToUpper() == id.ToUpper())
				{
					return subEntry;
				}
			}
		}
		return null;
	}

	private static void CheckUnlockableContent()
	{
		foreach (KeyValuePair<string, CodexEntry> entry in entries)
		{
			foreach (SubEntry subEntry in entry.Value.subEntries)
			{
				if (subEntry.lockedContentContainer != null)
				{
					subEntry.lockedContentContainer.content.Clear();
					subEntry.contentContainers.Remove(subEntry.lockedContentContainer);
				}
			}
		}
	}

	private static void CollectYAMLEntries(List<CategoryEntry> categories)
	{
		baseEntryPath = Application.streamingAssetsPath + "/codex";
		foreach (CodexEntry item in CollectEntries(""))
		{
			if (item != null && item.id != null && item.contentContainers != null && Game.IsCorrectDlcActiveForCurrentSave(item))
			{
				if (entries.ContainsKey(FormatLinkID(item.id)))
				{
					MergeEntry(item.id, item);
					continue;
				}
				AddEntry(item.id, item, categories);
				item.customContentLength = item.contentContainers.Count;
			}
		}
		string[] directories = Directory.GetDirectories(baseEntryPath);
		for (int i = 0; i < directories.Length; i++)
		{
			foreach (CodexEntry item2 in CollectEntries(Path.GetFileNameWithoutExtension(directories[i])))
			{
				if (item2 != null && item2.id != null && item2.contentContainers != null && Game.IsCorrectDlcActiveForCurrentSave(item2))
				{
					if (entries.ContainsKey(FormatLinkID(item2.id)))
					{
						MergeEntry(item2.id, item2);
						continue;
					}
					AddEntry(item2.id, item2, categories);
					item2.customContentLength = item2.contentContainers.Count;
				}
			}
		}
	}

	private static void CollectYAMLSubEntries(List<CategoryEntry> categories)
	{
		baseEntryPath = Application.streamingAssetsPath + "/codex";
		foreach (SubEntry v in CollectSubEntries(""))
		{
			if (v.parentEntryID == null || v.id == null || !Game.IsCorrectDlcActiveForCurrentSave(v))
			{
				continue;
			}
			if (entries.ContainsKey(v.parentEntryID.ToUpper()))
			{
				SubEntry subEntry = entries[v.parentEntryID.ToUpper()].subEntries.Find((SubEntry match) => match.id == v.id);
				if (!string.IsNullOrEmpty(v.lockID))
				{
					foreach (ContentContainer contentContainer in v.contentContainers)
					{
						contentContainer.lockID = v.lockID;
					}
				}
				if (subEntry != null)
				{
					if (!string.IsNullOrEmpty(v.lockID))
					{
						foreach (ContentContainer contentContainer2 in subEntry.contentContainers)
						{
							contentContainer2.lockID = v.lockID;
						}
						subEntry.lockID = v.lockID;
					}
					for (int num = 0; num < v.contentContainers.Count; num++)
					{
						if (Game.IsCorrectDlcActiveForCurrentSave(v.contentContainers[num]))
						{
							if (!string.IsNullOrEmpty(v.contentContainers[num].lockID))
							{
								int num2 = subEntry.contentContainers.IndexOf(subEntry.lockedContentContainer);
								subEntry.contentContainers.Insert(num2 + 1, v.contentContainers[num]);
							}
							else if (v.contentContainers[num].showBeforeGeneratedContent)
							{
								subEntry.contentContainers.Insert(0, v.contentContainers[num]);
							}
							else
							{
								subEntry.contentContainers.Add(v.contentContainers[num]);
							}
						}
					}
					subEntry.contentContainers.Add(new ContentContainer(new List<ICodexWidget>
					{
						new CodexLargeSpacer()
					}, ContentContainer.ContentLayout.Vertical));
					subEntry.layoutPriority = v.layoutPriority;
					if (v.contentMadeAndUsed.Count > 0)
					{
						subEntry.contentMadeAndUsed.AddRange(v.contentMadeAndUsed);
					}
				}
				else
				{
					entries[v.parentEntryID.ToUpper()].subEntries.Add(v);
				}
			}
			else
			{
				Debug.LogWarningFormat("Codex SubEntry {0} cannot find parent codex entry with id {1}", v.name, v.parentEntryID);
			}
		}
	}

	private static void AddLockLookup(string lockId, string articleId)
	{
		if (!unlockedEntryLookup.ContainsKey(lockId))
		{
			unlockedEntryLookup[lockId] = new List<string>();
		}
		unlockedEntryLookup[lockId].Add(articleId);
	}

	public static string GetEntryForLock(string lockId)
	{
		if (unlockedEntryLookup == null)
		{
			Debug.LogWarningFormat("Trying to get lock entry {0} before codex cache has been initialized.", lockId);
			return null;
		}
		if (string.IsNullOrEmpty(lockId))
		{
			return null;
		}
		if (unlockedEntryLookup.ContainsKey(lockId) && unlockedEntryLookup[lockId] != null && unlockedEntryLookup[lockId].Count > 0)
		{
			return unlockedEntryLookup[lockId][0];
		}
		return null;
	}

	public static void AddEntry(string id, CodexEntry entry, List<CategoryEntry> categoryEntries = null)
	{
		id = FormatLinkID(id);
		if (entries.ContainsKey(id))
		{
			Debug.LogError("Tried to add " + id + " to the Codex screen multiple times");
		}
		entries.Add(id, entry);
		entry.id = id;
		if (entry.name == null)
		{
			entry.name = Strings.Get(entry.title);
		}
		if (!string.IsNullOrEmpty(entry.iconAssetName))
		{
			try
			{
				entry.icon = Assets.GetSprite(entry.iconAssetName);
				if (!entry.iconLockID.IsNullOrWhiteSpace())
				{
					entry.iconColor = (Game.Instance.unlocks.IsUnlocked(entry.iconLockID) ? Color.white : Color.black);
				}
			}
			catch
			{
				Debug.LogWarningFormat("Unable to get icon for asset name {0}", entry.iconAssetName);
			}
		}
		else if (!string.IsNullOrEmpty(entry.iconPrefabID))
		{
			try
			{
				entry.icon = Def.GetUISpriteFromMultiObjectAnim(Assets.GetPrefab(entry.iconPrefabID).GetComponent<KBatchedAnimController>().AnimFiles[0]);
				if (!entry.iconLockID.IsNullOrWhiteSpace())
				{
					entry.iconColor = (Game.Instance.unlocks.IsUnlocked(entry.iconLockID) ? Color.white : Color.black);
				}
			}
			catch
			{
				Debug.LogWarningFormat("Unable to get icon for prefabID {0}", entry.iconPrefabID);
			}
		}
		if (!entry.parentId.IsNullOrWhiteSpace() && entries.ContainsKey(entry.parentId))
		{
			(entries[entry.parentId] as CategoryEntry).entriesInCategory.Add(entry);
		}
		foreach (ContentContainer contentContainer in entry.contentContainers)
		{
			if (contentContainer.lockID != null)
			{
				AddLockLookup(contentContainer.lockID, entry.id);
			}
		}
		entry.contentContainers.RemoveAll((ContentContainer x) => !Game.IsCorrectDlcActiveForCurrentSave(x));
	}

	public static void AddSubEntry(string id, SubEntry entry)
	{
	}

	public static void MergeSubEntry(string id, SubEntry entry)
	{
	}

	public static void MergeEntry(string id, CodexEntry entry)
	{
		id = FormatLinkID(entry.id);
		entry.id = id;
		CodexEntry codexEntry = entries[id];
		if (codexEntry.GetRequiredDlcIds() != null && entry.GetForbiddenDlcIds() != null)
		{
			DebugUtil.DevLogError("Codex Entry with id=" + id + " defines requiredDlcIds but the existing entry also specifies requiredDlcIds. This is currently not handled, please investigate.");
		}
		if (codexEntry.GetRequiredDlcIds() != null && entry.GetForbiddenDlcIds() != null)
		{
			DebugUtil.DevLogError("Codex Entry with id=" + id + " defines forbiddenDlcIds but the existing entry also specifies forbiddenDlcIds. This is currently not handled, please investigate.");
		}
		if (entry.requiredDlcIds != null)
		{
			codexEntry.requiredDlcIds = entry.requiredDlcIds;
		}
		if (entry.forbiddenDlcIds != null)
		{
			codexEntry.forbiddenDlcIds = entry.forbiddenDlcIds;
		}
		for (int i = 0; i < entry.log.modificationRecords.Count; i++)
		{
		}
		codexEntry.customContentLength = entry.contentContainers.Count;
		for (int num = entry.contentContainers.Count - 1; num >= 0; num--)
		{
			codexEntry.InsertContentContainer(entry.insertMergeContentAtBottom ? (codexEntry.contentContainers.Count - 1) : 0, entry.contentContainers[num]);
		}
		if (entry.disabled)
		{
			codexEntry.disabled = entry.disabled;
		}
		codexEntry.showBeforeGeneratedCategoryLinks = entry.showBeforeGeneratedCategoryLinks;
		if (!string.IsNullOrEmpty(entry.category))
		{
			codexEntry.category = entry.category;
		}
		if (!string.IsNullOrEmpty(entry.parentId))
		{
			codexEntry.parentId = entry.parentId;
		}
		foreach (ContentContainer contentContainer in entry.contentContainers)
		{
			if (contentContainer.lockID != null)
			{
				AddLockLookup(contentContainer.lockID, entry.id);
			}
		}
		if (entry.contentMadeAndUsed.Count > 0)
		{
			codexEntry.contentMadeAndUsed.AddRange(entry.contentMadeAndUsed);
		}
	}

	public static void Clear()
	{
		entries = null;
		baseEntryPath = null;
	}

	public static string GetEntryPath()
	{
		return baseEntryPath;
	}

	public static CodexEntry GetTemplate(string templatePath)
	{
		if (!entries.ContainsKey(templatePath))
		{
			entries.Add(templatePath, null);
		}
		if (entries[templatePath] == null)
		{
			string text = Path.Combine(baseEntryPath, templatePath);
			CodexEntry codexEntry = YamlIO.LoadFile<CodexEntry>(text + ".yaml", null, widgetTagMappings);
			if (codexEntry == null)
			{
				Debug.LogWarning("Missing template [" + text + ".yaml]");
			}
			entries[templatePath] = codexEntry;
		}
		return entries[templatePath];
	}

	private static void YamlParseErrorCB(YamlIO.Error error, bool force_log_as_warning)
	{
		throw new Exception($"{error.severity} parse error in {error.file.full_path}\n{error.message}", error.inner_exception);
	}

	public static List<CodexEntry> CollectEntries(string folder)
	{
		List<CodexEntry> list = new List<CodexEntry>();
		string path = ((folder == "") ? baseEntryPath : Path.Combine(baseEntryPath, folder));
		string[] array = new string[0];
		try
		{
			array = Directory.GetFiles(path, "*.yaml");
		}
		catch (UnauthorizedAccessException obj)
		{
			Debug.LogWarning(obj);
		}
		string category = folder.ToUpper();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (IsSubEntryAtPath(text))
			{
				continue;
			}
			try
			{
				CodexEntry codexEntry = YamlIO.LoadFile<CodexEntry>(text, YamlParseErrorCB, widgetTagMappings);
				if (codexEntry != null)
				{
					codexEntry.category = category;
					list.Add(codexEntry);
				}
			}
			catch (Exception ex)
			{
				DebugUtil.DevLogErrorFormat("CodexCache.CollectEntries failed to load [{0}]: {1}", text, ex.ToString());
			}
		}
		foreach (CodexEntry item in list)
		{
			if (string.IsNullOrEmpty(item.sortString))
			{
				item.sortString = Strings.Get(item.title);
			}
		}
		list.Sort((CodexEntry x, CodexEntry y) => x.sortString.CompareTo(y.sortString));
		return list;
	}

	public static List<SubEntry> CollectSubEntries(string folder)
	{
		List<SubEntry> list = new List<SubEntry>();
		string path = ((folder == "") ? baseEntryPath : Path.Combine(baseEntryPath, folder));
		string[] array = new string[0];
		try
		{
			array = Directory.GetFiles(path, "*.yaml", SearchOption.AllDirectories);
		}
		catch (UnauthorizedAccessException obj)
		{
			Debug.LogWarning(obj);
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!IsSubEntryAtPath(text))
			{
				continue;
			}
			try
			{
				SubEntry subEntry = YamlIO.LoadFile<SubEntry>(text, YamlParseErrorCB, widgetTagMappings);
				if (subEntry != null)
				{
					list.Add(subEntry);
				}
			}
			catch (Exception ex)
			{
				DebugUtil.DevLogErrorFormat("CodexCache.CollectSubEntries failed to load [{0}]: {1}", text, ex.ToString());
			}
		}
		list.Sort((SubEntry x, SubEntry y) => x.title.CompareTo(y.title));
		return list;
	}

	public static bool IsSubEntryAtPath(string path)
	{
		return Path.GetFileName(path).Contains("SubEntry");
	}
}
