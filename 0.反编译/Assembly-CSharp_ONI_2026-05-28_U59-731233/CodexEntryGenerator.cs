using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Klei;
using Klei.AI;
using ProcGen;
using ProcGenGame;
using STRINGS;
using TUNING;
using UnityEngine;

public static class CodexEntryGenerator
{
	private static string categoryPrefx = "BUILD_CATEGORY_";

	public static readonly string FOOD_CATEGORY_ID = CodexCache.FormatLinkID("FOOD");

	public static readonly string FOOD_EFFECTS_ENTRY_ID = CodexCache.FormatLinkID("id_food_effects");

	public static readonly string TABLE_SALT_ENTRY_ID = CodexCache.FormatLinkID("id_table_salt");

	public static readonly string MINION_MODIFIERS_CATEGORY_ID = CodexCache.FormatLinkID("MINION_MODIFIERS");

	public static Tag[] HiddenRoomConstrainTags = new Tag[13]
	{
		RoomConstraints.ConstraintTags.Refrigerator,
		RoomConstraints.ConstraintTags.FarmStationType,
		RoomConstraints.ConstraintTags.LuxuryBedType,
		RoomConstraints.ConstraintTags.MassageTable,
		RoomConstraints.ConstraintTags.MessTable,
		RoomConstraints.ConstraintTags.NatureReserve,
		RoomConstraints.ConstraintTags.Park,
		RoomConstraints.ConstraintTags.SpiceStation,
		RoomConstraints.ConstraintTags.DeStressingBuilding,
		RoomConstraints.ConstraintTags.MachineShopType,
		RoomConstraints.ConstraintTags.LightDutyGeneratorType,
		RoomConstraints.ConstraintTags.HeavyDutyGeneratorType,
		RoomConstraints.ConstraintTags.BionicUpkeepType
	};

	public static Dictionary<Tag, Tag> RoomConstrainTagIcons = new Dictionary<Tag, Tag>
	{
		[RoomConstraints.ConstraintTags.IndustrialMachinery] = "ManualGenerator",
		[RoomConstraints.ConstraintTags.RecBuilding] = "ArcadeMachine",
		[RoomConstraints.ConstraintTags.Clinic] = "MedicalCot",
		[RoomConstraints.ConstraintTags.WashStation] = "WashBasin",
		[RoomConstraints.ConstraintTags.AdvancedWashStation] = ShowerConfig.ID,
		[RoomConstraints.ConstraintTags.ToiletType] = "Outhouse",
		[RoomConstraints.ConstraintTags.FlushToiletType] = "FlushToilet",
		[RoomConstraints.ConstraintTags.ScienceBuilding] = "ResearchCenter",
		[GameTags.Decoration] = "Canvas",
		[GameTags.Ornament] = "artifact_RubiksCube",
		[RoomConstraints.ConstraintTags.RanchStationType] = "RanchStation",
		[RoomConstraints.ConstraintTags.BedType] = "Bed",
		[RoomConstraints.ConstraintTags.GeneratorType] = "Generator",
		[RoomConstraints.ConstraintTags.RocketInterior] = RocketControlStationConfig.ID,
		[RoomConstraints.ConstraintTags.CookTop] = "CookingStation",
		[RoomConstraints.ConstraintTags.WarmingStation] = "SpaceHeater",
		[RoomConstraints.ConstraintTags.PowerBuilding] = "Battery",
		[RoomConstraints.ConstraintTags.DiningTableType] = "DiningTable",
		[RoomConstraints.ConstraintTags.Submersible] = "UnderwaterCritterCondo"
	};

	public static Dictionary<Tag, Tag> BuildingsCategoriesTagIcons = new Dictionary<Tag, Tag>
	{
		[GameTags.CodexCategories.CreatureRelocator] = "CreatureDeliveryPoint",
		[GameTags.CodexCategories.FarmBuilding] = "FarmTile",
		[GameTags.CodexCategories.BionicBuilding] = "OilChanger"
	};

	public static Dictionary<string, CodexEntry> GenerateBuildingEntries()
	{
		Dictionary<string, CodexEntry> categoryEntries = new Dictionary<string, CodexEntry>();
		foreach (PlanScreen.PlanInfo item in TUNING.BUILDINGS.PLANORDER)
		{
			GenerateEntriesForBuildingsInCategory(item, categoryPrefx, ref categoryEntries);
		}
		if (DlcManager.FeatureClusterSpaceEnabled())
		{
			GenerateDLC1RocketryEntries();
		}
		GenerateBuildingCategoriesEntry(categoryPrefx, ref categoryEntries);
		PopulateCategoryEntries(categoryEntries);
		return categoryEntries;
	}

	private static void GenerateEntriesForBuildingsInCategory(PlanScreen.PlanInfo category, string categoryPrefx, ref Dictionary<string, CodexEntry> categoryEntries)
	{
		string text = HashCache.Get().Get(category.category);
		string text2 = CodexCache.FormatLinkID(categoryPrefx + text);
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		foreach (KeyValuePair<string, string> buildingAndSubcategoryDatum in category.buildingAndSubcategoryData)
		{
			BuildingDef buildingDef = Assets.GetBuildingDef(buildingAndSubcategoryDatum.Key);
			if (Game.IsCorrectDlcActiveForCurrentSave(buildingDef))
			{
				CodexEntry codexEntry = GenerateSingleBuildingEntry(buildingDef, text2);
				if (buildingDef.ExtendCodexEntry != null)
				{
					codexEntry = buildingDef.ExtendCodexEntry(codexEntry);
				}
				if (codexEntry != null)
				{
					dictionary.Add(codexEntry.id, codexEntry);
				}
			}
		}
		if (dictionary.Count != 0)
		{
			CategoryEntry categoryEntry = GenerateCategoryEntry(CodexCache.FormatLinkID(text2), Strings.Get("STRINGS.UI.BUILDCATEGORIES." + text.ToUpper() + ".NAME"), dictionary);
			categoryEntry.parentId = "BUILDINGS";
			categoryEntry.category = "BUILDINGS";
			categoryEntry.icon = Assets.GetSprite(PlanScreen.IconNameMap[text]);
			categoryEntries.Add(text2, categoryEntry);
		}
	}

	private static void GenerateBuildingCategoriesEntry(string categoryPrefix, ref Dictionary<string, CodexEntry> categoryEntries)
	{
		string text = "CATEGORY";
		string text2 = CodexCache.FormatLinkID(categoryPrefx + text);
		Dictionary<string, CodexEntry> dictionary = GenerateBuildingRequirementClassCategoryEntry(text2);
		Dictionary<string, CodexEntry> dictionary2 = GenerateBuildingCategoryGroupEntry(text2);
		Dictionary<string, CodexEntry> dictionary3 = new Dictionary<string, CodexEntry>(dictionary);
		foreach (string key in dictionary2.Keys)
		{
			dictionary3.Add(key, dictionary2[key]);
		}
		CategoryEntry categoryEntry = GenerateCategoryEntry(CodexCache.FormatLinkID(text2), CODEX.ROOM_REQUIREMENT_CLASS.NAME, dictionary3);
		categoryEntry.parentId = "BUILDINGS";
		categoryEntry.category = "BUILDINGS";
		categoryEntry.icon = Assets.GetSprite("icon_categories_placeholder");
		categoryEntries.Add(text2, categoryEntry);
	}

	private static Dictionary<string, CodexEntry> GenerateBuildingRequirementClassCategoryEntry(string categoryParentName)
	{
		string id_prefix = "REQUIREMENTCLASS";
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		foreach (Tag allTag in RoomConstraints.ConstraintTags.AllTags)
		{
			if (!HiddenRoomConstrainTags.Contains(allTag) && (DlcManager.FeatureClusterSpaceEnabled() || !(allTag == RoomConstraints.ConstraintTags.RocketInterior)) && (!(allTag == RoomConstraints.ConstraintTags.BionicUpkeepType) || Game.IsDlcActiveForCurrentSave("DLC3_ID")))
			{
				CodexEntry codexEntry = GenerateEntryForSpecificBuildingRequirementClass(allTag, categoryParentName, id_prefix);
				dictionary.Add(codexEntry.id, codexEntry);
			}
		}
		return dictionary;
	}

	private static Dictionary<string, CodexEntry> GenerateBuildingCategoryGroupEntry(string parentCategory)
	{
		string id_prefix = "GROUP";
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		foreach (Tag allTag in GameTags.CodexCategories.AllTags)
		{
			if (!HiddenRoomConstrainTags.Contains(allTag))
			{
				CodexEntry codexEntry = GenerateEntryForSpecificBuildingCategoryGroup(allTag, parentCategory, id_prefix);
				if (codexEntry != null)
				{
					dictionary.Add(codexEntry.id, codexEntry);
				}
			}
		}
		return dictionary;
	}

	private static CodexEntry GenerateEntryForSpecificBuildingRequirementClass(Tag requirementClassTag, string category_parentName, string id_prefix)
	{
		string text = "STRINGS.CODEX.ROOM_REQUIREMENT_CLASS." + requirementClassTag.ToString().ToUpper();
		List<ContentContainer> list = new List<ContentContainer>();
		List<ICodexWidget> list2 = new List<ICodexWidget>();
		ICodexWidget item = new CodexText(Strings.Get(text + ".TITLE"), CodexTextStyle.Title);
		list2.Add(item);
		list2.Add(new CodexDividerLine());
		list.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
		List<ICodexWidget> list3 = new List<ICodexWidget>();
		ICodexWidget item2 = new CodexText(Strings.Get(text + ".DESCRIPTION"));
		list3.Add(item2);
		list3.Add(new CodexSpacer());
		list3.Reverse();
		list.Add(new ContentContainer(list3, ContentContainer.ContentLayout.Vertical));
		List<ICodexWidget> list4 = new List<ICodexWidget>();
		List<ICodexWidget> list5 = new List<ICodexWidget>();
		BuildingDef buildingDef = null;
		List<string> list6 = new List<string>();
		foreach (PlanScreen.PlanInfo item11 in TUNING.BUILDINGS.PLANORDER)
		{
			foreach (KeyValuePair<string, string> buildingAndSubcategoryDatum in item11.buildingAndSubcategoryData)
			{
				buildingDef = Assets.GetBuildingDef(buildingAndSubcategoryDatum.Key);
				if (buildingDef.DebugOnly || buildingDef.Deprecated)
				{
					continue;
				}
				KPrefabID component = buildingDef.BuildingComplete.GetComponent<KPrefabID>();
				if (!list6.Contains(component.name))
				{
					list6.Add(component.name);
					if (component != null && component.Tags.Contains(requirementClassTag))
					{
						ICodexWidget item3 = new CodexIndentedLabelWithIcon("    • " + Strings.Get("STRINGS.BUILDINGS.PREFABS." + buildingDef.PrefabID.ToUpper() + ".NAME"), CodexTextStyle.Body, Def.GetUISprite(component.gameObject));
						list5.Add(item3);
					}
				}
			}
		}
		foreach (GameObject item12 in Assets.GetPrefabsWithTag(requirementClassTag))
		{
			KPrefabID component2 = item12.GetComponent<KPrefabID>();
			if (!list6.Contains(component2.name) && !item12.HasTag(GameTags.HideFromCodex) && component2 != null && component2.Tags.Contains(requirementClassTag))
			{
				ICodexWidget item4 = new CodexIndentedLabelWithIcon("    • " + item12.GetProperName(), CodexTextStyle.Body, Def.GetUISprite(component2.gameObject));
				list5.Add(item4);
				list6.Add(item12.name);
			}
		}
		if (list5.Count > 0)
		{
			ContentContainer contentContainer = new ContentContainer(list5, ContentContainer.ContentLayout.Vertical);
			CodexCollapsibleHeader item5 = new CodexCollapsibleHeader(CODEX.ROOM_REQUIREMENT_CLASS.SHARED.BUILDINGS_LIST_TITLE, contentContainer);
			list4.Add(item5);
			list4.Add(new CodexSpacer());
			list4.Add(new CodexSpacer());
			list4.Reverse();
			list.Add(new ContentContainer(list4, ContentContainer.ContentLayout.Vertical));
			list.Add(contentContainer);
		}
		if (Strings.TryGet(new StringKey(text + ".ROOMSREQUIRING"), out var result))
		{
			List<ICodexWidget> list7 = new List<ICodexWidget>();
			List<ICodexWidget> list8 = new List<ICodexWidget>();
			string[] array = result.String.Split('\n');
			for (int i = 0; i < array.Length; i++)
			{
				ICodexWidget item6 = new CodexText(array[i]);
				list8.Add(item6);
			}
			new CodexText(result.String);
			ContentContainer contentContainer2 = new ContentContainer(list8, ContentContainer.ContentLayout.Vertical);
			CodexCollapsibleHeader item7 = new CodexCollapsibleHeader(CODEX.ROOM_REQUIREMENT_CLASS.SHARED.ROOMS_REQUIRED_LIST_TITLE, contentContainer2);
			list7.Add(item7);
			list7.Add(new CodexSpacer());
			list7.Add(new CodexSpacer());
			list7.Reverse();
			list.Add(new ContentContainer(list7, ContentContainer.ContentLayout.Vertical));
			list.Add(contentContainer2);
		}
		if (Strings.TryGet(new StringKey(text + ".CONFLICTINGROOMS"), out var result2))
		{
			List<ICodexWidget> list9 = new List<ICodexWidget>();
			List<ICodexWidget> list10 = new List<ICodexWidget>();
			string[] array2 = result2.String.Split('\n');
			for (int j = 0; j < array2.Length; j++)
			{
				ICodexWidget item8 = new CodexText(array2[j]);
				list10.Add(item8);
			}
			ContentContainer contentContainer3 = new ContentContainer(list10, ContentContainer.ContentLayout.Vertical);
			CodexCollapsibleHeader item9 = new CodexCollapsibleHeader(CODEX.ROOM_REQUIREMENT_CLASS.SHARED.ROOMS_CONFLICT_LIST_TITLE, contentContainer3);
			list9.Add(item9);
			list9.Add(new CodexSpacer());
			list9.Add(new CodexSpacer());
			list9.Reverse();
			list.Add(new ContentContainer(list9, ContentContainer.ContentLayout.Vertical));
			list.Add(contentContainer3);
		}
		List<ICodexWidget> list11 = new List<ICodexWidget>();
		ICodexWidget item10 = new CodexText(Strings.Get(text + ".FLAVOUR"));
		list11.Add(item10);
		list.Add(new ContentContainer(list11, ContentContainer.ContentLayout.Vertical));
		CodexEntry codexEntry = new CodexEntry(category_parentName, list, RoomConstraints.ConstraintTags.GetRoomConstraintLabelText(requirementClassTag));
		codexEntry.icon = (RoomConstrainTagIcons.TryGetValue(requirementClassTag, out var value) ? Def.GetUISprite(value).first : null);
		codexEntry.parentId = category_parentName;
		Tag tag = requirementClassTag;
		CodexCache.AddEntry(CodexCache.FormatLinkID(id_prefix + tag.ToString()), codexEntry);
		return codexEntry;
	}

	private static CodexEntry GenerateEntryForSpecificBuildingCategoryGroup(Tag categoryGroupTag, string category_parentName, string id_prefix)
	{
		string text = "STRINGS.CODEX.CATEGORIES." + categoryGroupTag.ToString().ToUpper();
		List<ContentContainer> list = new List<ContentContainer>();
		List<ICodexWidget> list2 = new List<ICodexWidget>();
		ICodexWidget item = new CodexText(Strings.Get(text + ".TITLE"), CodexTextStyle.Title);
		list2.Add(item);
		list2.Add(new CodexDividerLine());
		list.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
		List<ICodexWidget> list3 = new List<ICodexWidget>();
		ICodexWidget item2 = new CodexText(Strings.Get(text + ".DESCRIPTION"));
		list3.Add(item2);
		list3.Add(new CodexSpacer());
		list3.Reverse();
		list.Add(new ContentContainer(list3, ContentContainer.ContentLayout.Vertical));
		List<ICodexWidget> list4 = new List<ICodexWidget>();
		List<ICodexWidget> list5 = new List<ICodexWidget>();
		BuildingDef buildingDef = null;
		foreach (PlanScreen.PlanInfo item6 in TUNING.BUILDINGS.PLANORDER)
		{
			foreach (KeyValuePair<string, string> buildingAndSubcategoryDatum in item6.buildingAndSubcategoryData)
			{
				buildingDef = Assets.GetBuildingDef(buildingAndSubcategoryDatum.Key);
				if (!buildingDef.DebugOnly && !buildingDef.Deprecated)
				{
					KPrefabID component = buildingDef.BuildingComplete.GetComponent<KPrefabID>();
					if (component != null && component.Tags.Contains(categoryGroupTag))
					{
						ICodexWidget item3 = new CodexIndentedLabelWithIcon("    • " + Strings.Get("STRINGS.BUILDINGS.PREFABS." + buildingDef.PrefabID.ToUpper() + ".NAME"), CodexTextStyle.Body, Def.GetUISprite(component.gameObject));
						list5.Add(item3);
					}
				}
			}
		}
		if (list5.Count == 0)
		{
			return null;
		}
		ContentContainer contentContainer = new ContentContainer(list5, ContentContainer.ContentLayout.Vertical);
		CodexCollapsibleHeader item4 = new CodexCollapsibleHeader(CODEX.CATEGORIES.SHARED.BUILDINGS_LIST_TITLE, contentContainer);
		list4.Add(item4);
		list4.Add(new CodexSpacer());
		list4.Add(new CodexSpacer());
		list4.Reverse();
		list.Add(new ContentContainer(list4, ContentContainer.ContentLayout.Vertical));
		list.Add(contentContainer);
		List<ICodexWidget> list6 = new List<ICodexWidget>();
		ICodexWidget item5 = new CodexText(Strings.Get(text + ".FLAVOUR"));
		list6.Add(item5);
		list.Add(new ContentContainer(list6, ContentContainer.ContentLayout.Vertical));
		CodexEntry codexEntry = new CodexEntry(category_parentName, list, GameTags.CodexCategories.GetCategoryLabelText(categoryGroupTag));
		codexEntry.icon = (BuildingsCategoriesTagIcons.TryGetValue(categoryGroupTag, out var value) ? Def.GetUISprite(value).first : null);
		codexEntry.parentId = category_parentName;
		Tag tag = categoryGroupTag;
		CodexCache.AddEntry(CodexCache.FormatLinkID(id_prefix + tag.ToString()), codexEntry);
		return codexEntry;
	}

	private static CodexEntry GenerateSingleBuildingEntry(BuildingDef def, string categoryEntryID)
	{
		if (def.DebugOnly || def.Deprecated)
		{
			return null;
		}
		List<ContentContainer> list = new List<ContentContainer>();
		List<ICodexWidget> list2 = new List<ICodexWidget>();
		list2.Add(new CodexText(def.Name, CodexTextStyle.Title));
		Tech tech = Db.Get().Techs.TryGetTechForTechItem(def.PrefabID);
		if (tech != null)
		{
			list2.Add(new CodexLabelWithIcon(tech.Name, CodexTextStyle.Body, new Tuple<Sprite, Color>(Assets.GetSprite("research_type_alpha_icon"), Color.white)));
		}
		list2.Add(new CodexDividerLine());
		list.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
		GenerateImageContainers(def.GetUISprite(), list);
		GenerateBuildingDescriptionContainers(def, list);
		GenerateFabricatorContainers(def.BuildingComplete, list);
		GenerateReceptacleContainers(def.BuildingComplete, list);
		GenerateConfigurableConsumerContainers(def.BuildingComplete, list);
		GenerateManualConversionEntries(def.BuildingComplete.PrefabID(), list);
		CodexEntry codexEntry = new CodexEntry(categoryEntryID, list, Strings.Get("STRINGS.BUILDINGS.PREFABS." + def.PrefabID.ToUpper() + ".NAME"));
		codexEntry.icon = def.GetUISprite();
		codexEntry.parentId = categoryEntryID;
		CodexCache.AddEntry(def.PrefabID, codexEntry);
		return codexEntry;
	}

	private static void GenerateDLC1RocketryEntries()
	{
		PlanScreen.PlanInfo planInfo = TUNING.BUILDINGS.PLANORDER.Find((PlanScreen.PlanInfo match) => match.category == new HashedString("Rocketry"));
		foreach (string item in SelectModuleSideScreen.moduleButtonSortOrder)
		{
			string text = HashCache.Get().Get(planInfo.category);
			string categoryEntryID = CodexCache.FormatLinkID(categoryPrefx + text);
			BuildingDef buildingDef = Assets.GetBuildingDef(item);
			if (!(buildingDef == null))
			{
				CodexEntry codexEntry = GenerateSingleBuildingEntry(buildingDef, categoryEntryID);
				List<ICodexWidget> list = new List<ICodexWidget>();
				list.Add(new CodexSpacer());
				list.Add(new CodexText(UI.CLUSTERMAP.ROCKETS.MODULE_STATS.NAME_HEADER, CodexTextStyle.Subtitle));
				list.Add(new CodexSpacer());
				list.Add(new CodexText(UI.CLUSTERMAP.ROCKETS.SPEED.TOOLTIP));
				RocketModuleCluster component = buildingDef.BuildingComplete.GetComponent<RocketModuleCluster>();
				float burden = component.performanceStats.Burden;
				float enginePower = component.performanceStats.EnginePower;
				RocketEngineCluster component2 = buildingDef.BuildingComplete.GetComponent<RocketEngineCluster>();
				if (component2 != null)
				{
					list.Add(new CodexText(string.Concat("    • ", UI.CLUSTERMAP.ROCKETS.MAX_HEIGHT.NAME_MAX_SUPPORTED, component2.maxHeight.ToString())));
				}
				list.Add(new CodexText(string.Concat("    • ", UI.CLUSTERMAP.ROCKETS.MAX_HEIGHT.NAME_RAW, buildingDef.HeightInCells.ToString())));
				if (burden != 0f)
				{
					list.Add(new CodexText(string.Concat("    • ", UI.CLUSTERMAP.ROCKETS.BURDEN_MODULE.NAME, burden.ToString())));
				}
				if (enginePower != 0f)
				{
					list.Add(new CodexText(string.Concat("    • ", UI.CLUSTERMAP.ROCKETS.POWER_MODULE.NAME, enginePower.ToString())));
				}
				ContentContainer container = new ContentContainer(list, ContentContainer.ContentLayout.Vertical);
				codexEntry.AddContentContainer(container);
			}
		}
	}

	public static void GeneratePageNotFound()
	{
		List<ContentContainer> list = new List<ContentContainer>();
		ContentContainer contentContainer = new ContentContainer();
		contentContainer.content.Add(new CodexText(CODEX.PAGENOTFOUND.TITLE, CodexTextStyle.Title));
		contentContainer.content.Add(new CodexText(CODEX.PAGENOTFOUND.SUBTITLE, CodexTextStyle.Subtitle));
		contentContainer.content.Add(new CodexDividerLine());
		contentContainer.content.Add(new CodexImage(312, 312, Assets.GetSprite("outhouseMessage")));
		list.Add(contentContainer);
		CodexEntry codexEntry = new CodexEntry("ROOT", list, CODEX.PAGENOTFOUND.TITLE);
		codexEntry.searchOnly = true;
		CodexCache.AddEntry("PageNotFound", codexEntry);
	}

	public static Dictionary<string, CodexEntry> GenerateRoomsEntries()
	{
		Dictionary<string, CodexEntry> result = new Dictionary<string, CodexEntry>();
		RoomTypes roomTypesData = Db.Get().RoomTypes;
		string parentCategoryName = "ROOMS";
		Action<RoomTypeCategory> action = delegate(RoomTypeCategory roomCategory)
		{
			bool flag = false;
			List<ContentContainer> contentContainers = new List<ContentContainer>();
			CodexEntry codexEntry = new CodexEntry(parentCategoryName, contentContainers, roomCategory.Name);
			for (int i = 0; i < roomTypesData.Count; i++)
			{
				RoomType roomType = roomTypesData[i];
				if (roomType.category.Id == roomCategory.Id)
				{
					if (!flag)
					{
						flag = true;
						codexEntry.parentId = parentCategoryName;
						codexEntry.name = roomCategory.Name;
						CodexCache.AddEntry(parentCategoryName + roomCategory.Id, codexEntry);
						result.Add(parentCategoryName + roomType.category.Id, codexEntry);
						ContentContainer container = new ContentContainer(new List<ICodexWidget>
						{
							new CodexImage(312, 312, Assets.GetSprite(roomCategory.icon))
						}, ContentContainer.ContentLayout.Vertical);
						codexEntry.AddContentContainer(container);
					}
					List<ContentContainer> list = new List<ContentContainer>();
					GenerateTitleContainers(roomType.Name, list);
					GenerateRoomTypeDescriptionContainers(roomType, list);
					GenerateRoomTypeDetailsContainers(roomType, list);
					SubEntry item = new SubEntry(roomType.Id, parentCategoryName + roomType.category.Id, list, roomType.Name)
					{
						icon = Assets.GetSprite(roomCategory.icon),
						iconColor = Color.white
					};
					codexEntry.subEntries.Add(item);
				}
			}
		};
		action(Db.Get().RoomTypeCategories.Agricultural);
		action(Db.Get().RoomTypeCategories.Bathroom);
		if (Game.IsDlcActiveForCurrentSave("DLC3_ID"))
		{
			action(Db.Get().RoomTypeCategories.Bionic);
		}
		action(Db.Get().RoomTypeCategories.Food);
		action(Db.Get().RoomTypeCategories.Hospital);
		action(Db.Get().RoomTypeCategories.Industrial);
		action(Db.Get().RoomTypeCategories.Park);
		action(Db.Get().RoomTypeCategories.Recreation);
		action(Db.Get().RoomTypeCategories.Sleep);
		action(Db.Get().RoomTypeCategories.Science);
		return result;
	}

	public static Dictionary<string, CodexEntry> GeneratePlantEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		List<GameObject> prefabsWithComponent = Assets.GetPrefabsWithComponent<Harvestable>();
		prefabsWithComponent.AddRange(Assets.GetPrefabsWithComponent<WiltCondition>());
		foreach (GameObject item6 in prefabsWithComponent)
		{
			KPrefabID component = item6.GetComponent<KPrefabID>();
			SeedProducer component2 = item6.GetComponent<SeedProducer>();
			KPrefabID kPrefabID = ((component2 == null) ? null : Assets.GetPrefab(component2.seedInfo.seedId).GetComponent<KPrefabID>());
			bool flag = kPrefabID != null && kPrefabID.HasAnyTags(new List<Tag>
			{
				GameTags.Seed,
				GameTags.CropSeed
			});
			if (dictionary.ContainsKey(component.PrefabID().ToString()) || !Game.IsCorrectDlcActiveForCurrentSave(component) || component.HasTag(GameTags.DeprecatedContent) || component.HasTag(GameTags.HideFromCodex))
			{
				continue;
			}
			List<ContentContainer> list = new List<ContentContainer>();
			Sprite first = Def.GetUISprite(item6).first;
			GenerateImageContainers(first, list);
			GeneratePlantDescriptionContainers(item6, list);
			CodexEntryGenerator_Elements.GenerateMadeAndUsedContainers(item6.PrefabID(), list);
			CodexEntry codexEntry = new CodexEntry("PLANTS", list, item6.GetProperName());
			if (flag)
			{
				List<ContentContainer> list2 = new List<ContentContainer>();
				List<ICodexWidget> list3 = new List<ICodexWidget>();
				InfoDescription component3 = kPrefabID.GetComponent<InfoDescription>();
				CodexText item = new CodexText(CODEX.HEADERS.GROWNFROMSEED, CodexTextStyle.Subtitle);
				CodexText item2 = new CodexText(kPrefabID.GetProperName(), CodexTextStyle.Title);
				CodexText item3 = new CodexText(component3.description);
				list3.Add(new CodexLargeSpacer());
				list3.Add(item);
				list3.Add(new CodexDividerLine());
				list3.Add(item2);
				list3.Add(item3);
				ContentContainer item4 = new ContentContainer(list3, ContentContainer.ContentLayout.Vertical);
				list2.Add(item4);
				Sprite first2 = Def.GetUISprite(component2.seedInfo.seedId).first;
				GenerateImageContainers(first2, list2);
				string text = kPrefabID.PrefabID().ToString();
				SubEntry subEntry = CodexCache.FindSubEntry(text);
				if (subEntry != null)
				{
					text = component.ToString() + "_" + text;
				}
				CodexEntryGenerator_Elements.GenerateMadeAndUsedContainers(kPrefabID.PrefabID(), list2);
				SubEntry item5 = new SubEntry(text, item6.PrefabID().ToString(), list2, kPrefabID.GetProperName());
				codexEntry.subEntries.Add(item5);
			}
			codexEntry.parentId = "PLANTS";
			codexEntry.icon = first;
			CodexCache.AddEntry(item6.PrefabID().ToString(), codexEntry);
			dictionary.Add(item6.PrefabID().ToString(), codexEntry);
		}
		return dictionary;
	}

	public static Dictionary<string, CodexEntry> GenerateFoodEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		foreach (EdiblesManager.FoodInfo allFoodType in EdiblesManager.GetAllFoodTypes())
		{
			GameObject prefab = Assets.GetPrefab(allFoodType.Id);
			DebugUtil.DevAssert(prefab != null, "Food prefab is null: " + allFoodType.Id);
			if (!(prefab == null))
			{
				KPrefabID component = prefab.GetComponent<KPrefabID>();
				if (!component.HasTag(GameTags.DeprecatedContent) && !component.HasTag(GameTags.IncubatableEgg) && !component.HasTag(GameTags.Seed) && !component.HasTag(GameTags.CropSeed))
				{
					List<ContentContainer> list = new List<ContentContainer>();
					GenerateTitleContainers(allFoodType.Name, list);
					Sprite first = Def.GetUISprite(allFoodType.ConsumableId).first;
					GenerateImageContainers(first, list);
					GenerateFoodDescriptionContainers(allFoodType, list);
					GenerateRecipeContainers(allFoodType.ConsumableId.ToTag(), list);
					CodexEntryGenerator_Elements.GenerateMadeAndUsedContainers(allFoodType.ConsumableId.ToTag(), list);
					CodexEntry codexEntry = new CodexEntry(FOOD_CATEGORY_ID, list, allFoodType.Name);
					codexEntry.icon = first;
					codexEntry.parentId = FOOD_CATEGORY_ID;
					CodexCache.AddEntry(allFoodType.Id, codexEntry);
					dictionary.Add(allFoodType.Id, codexEntry);
				}
			}
		}
		CodexEntry codexEntry2 = GenerateFoodEffectEntry();
		CodexCache.AddEntry(FOOD_EFFECTS_ENTRY_ID, codexEntry2);
		dictionary.Add(FOOD_EFFECTS_ENTRY_ID, codexEntry2);
		CodexEntry codexEntry3 = GenerateTabelSaltEntry();
		CodexCache.AddEntry(TABLE_SALT_ENTRY_ID, codexEntry3);
		dictionary.Add(TABLE_SALT_ENTRY_ID, codexEntry3);
		return dictionary;
	}

	private static CodexEntry GenerateFoodEffectEntry()
	{
		List<ICodexWidget> content = new List<ICodexWidget>();
		CodexEntry codexEntry = new CodexEntry(FOOD_CATEGORY_ID, new List<ContentContainer>
		{
			new ContentContainer(content, ContentContainer.ContentLayout.Vertical)
		}, CODEX.HEADERS.FOODEFFECTS);
		codexEntry.parentId = FOOD_CATEGORY_ID;
		codexEntry.icon = Assets.GetSprite("icon_category_food");
		Dictionary<string, List<EdiblesManager.FoodInfo>> dictionary = new Dictionary<string, List<EdiblesManager.FoodInfo>>();
		foreach (EdiblesManager.FoodInfo allFoodType in EdiblesManager.GetAllFoodTypes())
		{
			foreach (string effect2 in allFoodType.Effects)
			{
				if (!dictionary.TryGetValue(effect2, out var value))
				{
					value = (dictionary[effect2] = new List<EdiblesManager.FoodInfo>());
				}
				value.Add(allFoodType);
			}
		}
		foreach (KeyValuePair<string, List<EdiblesManager.FoodInfo>> item2 in dictionary)
		{
			item2.Deconstruct(out var key, out var value2);
			string text = key;
			List<EdiblesManager.FoodInfo> list2 = value2;
			Effect effect = Db.Get().effects.Get(text);
			string id = FOOD_EFFECTS_ENTRY_ID + "::" + text.ToUpper();
			string text2 = Strings.Get("STRINGS.DUPLICANTS.MODIFIERS." + text.ToUpper() + ".NAME");
			string text3 = Strings.Get("STRINGS.DUPLICANTS.MODIFIERS." + text.ToUpper() + ".DESCRIPTION");
			List<ICodexWidget> list3 = new List<ICodexWidget>();
			list3.Add(new CodexText(text2, CodexTextStyle.Title));
			SubEntry item = new SubEntry(id, FOOD_EFFECTS_ENTRY_ID, new List<ContentContainer>
			{
				new ContentContainer(list3, ContentContainer.ContentLayout.Vertical)
			}, text2);
			codexEntry.subEntries.Add(item);
			list3.Add(new CodexText(text3));
			foreach (AttributeModifier selfModifier in effect.SelfModifiers)
			{
				string text4 = Strings.Get("STRINGS.DUPLICANTS.ATTRIBUTES." + selfModifier.AttributeId.ToUpper() + ".NAME");
				string tooltip = Strings.Get("STRINGS.DUPLICANTS.ATTRIBUTES." + selfModifier.AttributeId.ToUpper() + ".DESC");
				list3.Add(new CodexTextWithTooltip("    • " + text4 + ": " + selfModifier.GetFormattedString(), tooltip));
			}
			list3.Add(new CodexText(string.Concat(CODEX.HEADERS.FOODSWITHEFFECT, ": ")));
			foreach (EdiblesManager.FoodInfo item3 in list2)
			{
				list3.Add(new CodexTextWithTooltip("    • " + item3.Name, item3.Description));
			}
			list3.Add(new CodexSpacer());
		}
		return codexEntry;
	}

	public static Dictionary<string, CodexEntry> GenerateDuplicantEntries()
	{
		string text = "DUPLICANTS";
		CodexEntry codexEntry = new CodexEntry("DUPLICANTSCATEGORY", new List<ContentContainer>(), CODEX.DUPLICANT.SPECIES_TITLE);
		codexEntry.icon = Assets.GetSprite("codexIconDupes");
		codexEntry.parentId = "DUPLICANTSCATEGORY";
		CodexCache.AddEntry(text, codexEntry);
		CategoryEntry categoryEntry = GenerateCategoryEntry("ROLES", UI.CODEX.CATEGORYNAMES.ROLES, GenerateRoleEntries(), Assets.GetSprite("codexIconSkills"));
		categoryEntry.parentId = codexEntry.parentId;
		Dictionary<string, CodexEntry> result = new Dictionary<string, CodexEntry>
		{
			{ text, codexEntry },
			{ "ROLES", categoryEntry }
		};
		PopulateCategoryEntries(new List<CategoryEntry> { categoryEntry });
		List<ContentContainer> contentContainers = new List<ContentContainer>
		{
			new ContentContainer(new List<ICodexWidget>
			{
				new CodexText(CODEX.STANDARD.TITLE, CodexTextStyle.Title),
				new CodexText(CODEX.STANDARD.SUBTITLE, CodexTextStyle.Subtitle),
				new CodexDividerLine()
			}, ContentContainer.ContentLayout.Vertical),
			new ContentContainer(new List<ICodexWidget>
			{
				new CodexText(CODEX.STANDARD.HEADER_1, CodexTextStyle.Subtitle),
				new CodexText(CODEX.STANDARD.PARAGRAPH_1.Replace("{time}", GameUtil.GetFormattedCycles(100f / DUPLICANTSTATS.STANDARD.BaseStats.BLADDER_INCREASE_PER_SECOND)).Replace("{O2gperSec}", GameUtil.GetFormattedMass(DUPLICANTSTATS.STANDARD.BaseStats.OXYGEN_USED_PER_SECOND, GameUtil.TimeSlice.PerSecond)).Replace("{CO2gperSec}", GameUtil.GetFormattedMass(DUPLICANTSTATS.STANDARD.BaseStats.OXYGEN_USED_PER_SECOND * DUPLICANTSTATS.STANDARD.BaseStats.OXYGEN_TO_CO2_CONVERSION, GameUtil.TimeSlice.PerSecond, GameUtil.MetricMassFormat.Gram))
					.Replace("{caloriesrequired}", GameUtil.GetFormattedCalories(DUPLICANTSTATS.STANDARD.BaseStats.CALORIES_BURNED_PER_CYCLE * -1f))),
				new CodexText(CODEX.STANDARD.HEADER_2, CodexTextStyle.Subtitle),
				new CodexText(CODEX.STANDARD.PARAGRAPH_2)
			}, ContentContainer.ContentLayout.Vertical)
		};
		SubEntry item = new SubEntry("DuplicantStandard", text, contentContainers, CODEX.STANDARD.TITLE);
		CodexCache.FindEntry(text).subEntries.Add(item);
		List<ContentContainer> contentContainers2 = new List<ContentContainer>
		{
			new ContentContainer(new List<ICodexWidget>
			{
				new CodexText(CODEX.BIONIC.TITLE, CodexTextStyle.Title),
				new CodexText(CODEX.BIONIC.SUBTITLE, CodexTextStyle.Subtitle),
				new CodexDividerLine()
			}, ContentContainer.ContentLayout.Vertical),
			new ContentContainer(new List<ICodexWidget>
			{
				new CodexText(CODEX.BIONIC.HEADER_1, CodexTextStyle.Subtitle),
				new CodexText(CODEX.BIONIC.PARAGRAPH_1.Replace("{time}", GameUtil.GetFormattedCycles(GunkMonitor.GUNK_CAPACITY / (1f / 30f))).Replace("{number}", GameUtil.GetFormattedCycles(BionicOxygenTankMonitor.OXYGEN_TANK_CAPACITY_KG / DUPLICANTSTATS.BIONICS.BaseStats.OXYGEN_USED_PER_SECOND))),
				new CodexText(CODEX.BIONIC.HEADER_2, CodexTextStyle.Subtitle),
				new CodexText(CODEX.BIONIC.PARAGRAPH_2)
			}, ContentContainer.ContentLayout.Vertical)
		};
		SubEntry item2 = new SubEntry("DuplicantBionic", text, contentContainers2, CODEX.BIONIC.TITLE);
		CodexCache.FindEntry(text).subEntries.Add(item2);
		return result;
	}

	private static CodexEntry GenerateTabelSaltEntry()
	{
		LocString nAME = STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.TABLE_SALT.NAME;
		LocString dESC = STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.TABLE_SALT.DESC;
		Sprite sprite = Assets.GetSprite("ui_food_table_salt");
		List<ContentContainer> list = new List<ContentContainer>();
		GenerateImageContainers(sprite, list);
		list.Add(new ContentContainer(new List<ICodexWidget>
		{
			new CodexText(nAME, CodexTextStyle.Title),
			new CodexText(dESC)
		}, ContentContainer.ContentLayout.Vertical));
		CodexEntry codexEntry = new CodexEntry(FOOD_CATEGORY_ID, list, nAME);
		codexEntry.parentId = FOOD_CATEGORY_ID;
		codexEntry.icon = sprite;
		return codexEntry;
	}

	public static Dictionary<string, CodexEntry> GenerateMinionModifierEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		foreach (Effect resource in Db.Get().effects.resources)
		{
			if (!resource.triggerFloatingText && resource.showInUI)
			{
				continue;
			}
			string id = resource.Id;
			string text = "AVOID_COLLISIONS_" + id;
			if (!Strings.TryGet("STRINGS.DUPLICANTS.MODIFIERS." + id.ToUpper() + ".NAME", out var result) || (!Strings.TryGet("STRINGS.DUPLICANTS.MODIFIERS." + id.ToUpper() + ".DESCRIPTION", out var result2) && !Strings.TryGet("STRINGS.DUPLICANTS.MODIFIERS." + id.ToUpper() + ".TOOLTIP", out result2)))
			{
				continue;
			}
			string text2 = result.String;
			string text3 = result2.String;
			List<ContentContainer> list = new List<ContentContainer>();
			ContentContainer contentContainer = new ContentContainer();
			List<ICodexWidget> content = contentContainer.content;
			content.Add(new CodexText(resource.Name, CodexTextStyle.Title));
			content.Add(new CodexText(resource.description));
			foreach (AttributeModifier selfModifier in resource.SelfModifiers)
			{
				string text4 = Strings.Get("STRINGS.DUPLICANTS.ATTRIBUTES." + selfModifier.AttributeId.ToUpper() + ".NAME");
				string tooltip = Strings.Get("STRINGS.DUPLICANTS.ATTRIBUTES." + selfModifier.AttributeId.ToUpper() + ".DESC");
				content.Add(new CodexTextWithTooltip("    • " + text4 + ": " + selfModifier.GetFormattedString(), tooltip));
			}
			content.Add(new CodexSpacer());
			list.Add(contentContainer);
			CodexEntry codexEntry = new CodexEntry(MINION_MODIFIERS_CATEGORY_ID, list, resource.Name);
			codexEntry.icon = Assets.GetSprite(resource.customIcon);
			codexEntry.parentId = MINION_MODIFIERS_CATEGORY_ID;
			CodexCache.AddEntry(text, codexEntry);
			dictionary.Add(text, codexEntry);
		}
		return dictionary;
	}

	public static Dictionary<string, CodexEntry> GenerateTechEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		foreach (Tech resource in Db.Get().Techs.resources)
		{
			List<ContentContainer> list = new List<ContentContainer>();
			GenerateTitleContainers(resource.Name, list);
			GenerateTechDescriptionContainers(resource, list);
			GeneratePrerequisiteTechContainers(resource, list);
			GenerateUnlockContainers(resource, list);
			CodexEntry codexEntry = new CodexEntry("TECH", list, resource.Name);
			codexEntry.icon = ((resource.unlockedItems.Count != 0) ? resource.unlockedItems[0] : null)?.getUISprite("ui", arg2: false);
			codexEntry.parentId = "TECH";
			CodexCache.AddEntry(resource.Id, codexEntry);
			dictionary.Add(resource.Id, codexEntry);
		}
		return dictionary;
	}

	public static Dictionary<string, CodexEntry> GenerateRoleEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		foreach (Skill resource in Db.Get().Skills.resources)
		{
			if (!resource.deprecated && Game.IsCorrectDlcActiveForCurrentSave(resource))
			{
				List<ContentContainer> list = new List<ContentContainer>();
				Sprite sprite = null;
				sprite = Assets.GetSprite(resource.hat);
				GenerateTitleContainers(resource.Name, list);
				GenerateImageContainers(sprite, list);
				GenerateGenericDescriptionContainers(resource.description, list);
				GenerateSkillRequirementsAndPerksContainers(resource, list);
				GenerateRelatedSkillContainers(resource, list);
				CodexEntry codexEntry = new CodexEntry("ROLES", list, resource.Name);
				codexEntry.parentId = "ROLES";
				codexEntry.icon = sprite;
				CodexCache.AddEntry(resource.Id, codexEntry);
				dictionary.Add(resource.Id, codexEntry);
			}
		}
		return dictionary;
	}

	public static Dictionary<string, CodexEntry> GenerateGeyserEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		List<GameObject> prefabsWithTag = Assets.GetPrefabsWithTag(GameTags.GeyserFeature);
		foreach (GameObject item in prefabsWithTag)
		{
			KPrefabID component = item.GetComponent<KPrefabID>();
			if (!component.HasTag(GameTags.DeprecatedContent) && Game.IsCorrectDlcActiveForCurrentSave(component))
			{
				List<ContentContainer> list = new List<ContentContainer>();
				GenerateTitleContainers(item.GetProperName(), list);
				Sprite first = Def.GetUISprite(item).first;
				GenerateImageContainers(first, list);
				List<ICodexWidget> list2 = new List<ICodexWidget>();
				string text = item.PrefabID().ToString();
				string text2 = text.ToUpper();
				if (text2.StartsWith("GEYSERGENERIC_"))
				{
					text2 = text2.Substring("GEYSERGENERIC_".Length);
				}
				list2.Add(new CodexText(Strings.Get("STRINGS.CREATURES.SPECIES.GEYSER." + text2 + ".DESC")));
				list2.Add(new CodexText(UI.CODEX.GEYSERS.DESC));
				list.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
				CodexEntry codexEntry = new CodexEntry("GEYSERS", list, item.GetProperName());
				codexEntry.icon = first;
				codexEntry.parentId = "GEYSERS";
				codexEntry.id = text;
				CodexCache.AddEntry(codexEntry.id, codexEntry);
				dictionary.Add(codexEntry.id, codexEntry);
			}
		}
		return dictionary;
	}

	public static Dictionary<string, CodexEntry> GenerateEquipmentEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		List<GameObject> prefabsWithComponent = Assets.GetPrefabsWithComponent<Equippable>();
		if (prefabsWithComponent != null)
		{
			foreach (GameObject item in prefabsWithComponent)
			{
				if (!Game.IsCorrectDlcActiveForCurrentSave(item.GetComponent<KPrefabID>()))
				{
					continue;
				}
				bool flag = false;
				Equippable component = item.GetComponent<Equippable>();
				if (component.def.AdditionalTags != null)
				{
					Tag[] additionalTags = component.def.AdditionalTags;
					foreach (Tag tag in additionalTags)
					{
						if (tag == GameTags.DeprecatedContent)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag || component.hideInCodex)
				{
					continue;
				}
				List<ContentContainer> list = new List<ContentContainer>();
				GenerateTitleContainers(item.GetProperName(), list);
				Sprite first = Def.GetUISprite(item).first;
				GenerateImageContainers(first, list);
				List<ICodexWidget> list2 = new List<ICodexWidget>();
				string text = item.PrefabID().ToString();
				if (component.def.Id == "SleepClinicPajamas")
				{
					list2.Add(new CodexText(Strings.Get("STRINGS.EQUIPMENT.PREFABS." + text.ToUpper() + ".DESC")));
					list2.Add(new CodexText(Strings.Get("STRINGS.EQUIPMENT.PREFABS." + text.ToUpper() + ".EFFECT")));
				}
				else
				{
					list2.Add(new CodexText(Strings.Get("STRINGS.EQUIPMENT.PREFABS." + text.ToUpper() + ".RECIPE_DESC")));
				}
				if (component.def.AttributeModifiers.Count > 0 || component.def.additionalDescriptors.Count > 0)
				{
					list2.Add(new CodexSpacer());
					list2.Add(new CodexText(CODEX.HEADERS.EQUIPMENTEFFECTS, CodexTextStyle.Subtitle));
				}
				foreach (AttributeModifier attributeModifier in component.def.AttributeModifiers)
				{
					list2.Add(new CodexTextWithTooltip("    • " + string.Format(DUPLICANTS.MODIFIERS.MODIFIER_FORMAT, attributeModifier.GetName(), attributeModifier.GetFormattedString()), Db.Get().Attributes.Get(attributeModifier.AttributeId).Description));
				}
				foreach (Descriptor additionalDescriptor in component.def.additionalDescriptors)
				{
					list2.Add(new CodexTextWithTooltip("    • " + additionalDescriptor.text, additionalDescriptor.tooltipText));
				}
				list.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
				CodexEntry codexEntry = new CodexEntry("EQUIPMENT", list, item.GetProperName());
				codexEntry.icon = first;
				codexEntry.parentId = "EQUIPMENT";
				codexEntry.id = item.PrefabID().ToString();
				CodexCache.AddEntry(codexEntry.id, codexEntry);
				dictionary.Add(codexEntry.id, codexEntry);
			}
		}
		return dictionary;
	}

	public static void GenerateElectrobankEntries()
	{
		CodexEntry codexEntry = new CodexEntry("ROOT", new List<ContentContainer>(), CODEX.ELECTROBANK.TITLE);
		codexEntry.id = "ELECTROBANKS";
		codexEntry.icon = Assets.GetSprite("upgrade_disc");
		codexEntry.parentId = "EQUIPMENT";
		CodexCache.AddEntry(codexEntry.id, codexEntry);
		List<GameObject> prefabsWithComponent = Assets.GetPrefabsWithComponent<Electrobank>();
		foreach (GameObject item in prefabsWithComponent)
		{
			if (!item.HasTag(GameTags.DeprecatedContent))
			{
				List<ContentContainer> list = new List<ContentContainer>();
				GenerateTitleContainers(item.GetProperName(), list);
				Sprite first = Def.GetUISprite(item).first;
				GenerateImageContainers(first, list);
				List<ICodexWidget> list2 = new List<ICodexWidget>();
				list2.Add(new CodexText(item.GetComponent<InfoDescription>().description));
				list2.Add(new CodexSpacer());
				list.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
				string id = UI.ExtractLinkID(item.GetProperName());
				if (CodexCache.FindEntry("ELECTROBANKS").subEntries.Find((SubEntry x) => x.id == id) == null)
				{
					SubEntry subEntry = new SubEntry(id, "ELECTROBANKS", list, item.GetProperName());
					subEntry.icon = first;
					CodexCache.FindEntry("ELECTROBANKS").subEntries.Add(subEntry);
				}
			}
		}
	}

	public static void GenerateBionicUpgradeEntries()
	{
		CodexEntry codexEntry = new CodexEntry("ROOT", new List<ContentContainer>(), CODEX.BIONICBOOSTER.TITLE);
		codexEntry.id = "BOOSTER";
		codexEntry.icon = Assets.GetSprite("upgrade_disc");
		codexEntry.parentId = "EQUIPMENT";
		CodexCache.AddEntry(codexEntry.id, codexEntry);
		List<GameObject> prefabsWithComponent = Assets.GetPrefabsWithComponent<BionicUpgradeComponent>();
		foreach (GameObject item in prefabsWithComponent)
		{
			BionicUpgradeComponent component = item.GetComponent<BionicUpgradeComponent>();
			List<ContentContainer> list = new List<ContentContainer>();
			GenerateTitleContainers(item.GetProperName(), list);
			Sprite first = Def.GetUISprite(item).first;
			GenerateImageContainers(first, list);
			List<ICodexWidget> list2 = new List<ICodexWidget>();
			foreach (Descriptor descriptor in component.GetDescriptors(item))
			{
				list2.Add(new CodexText(descriptor.text));
			}
			list2.Add(new CodexSpacer());
			list.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
			string id = UI.ExtractLinkID(item.GetProperName());
			if (CodexCache.FindEntry("BOOSTER").subEntries.Find((SubEntry x) => x.id == id) == null)
			{
				SubEntry subEntry = new SubEntry(id, "BOOSTER", list, item.GetProperName());
				subEntry.icon = first;
				CodexCache.FindEntry("BOOSTER").subEntries.Add(subEntry);
			}
		}
	}

	public static Dictionary<string, CodexEntry> GenerateBiomeEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		ListPool<YamlIO.Error, WorldGen>.PooledList pooledList = ListPool<YamlIO.Error, WorldGen>.Allocate();
		string text = Application.streamingAssetsPath + "/worldgen/worlds/";
		string text2 = Application.streamingAssetsPath + "/worldgen/biomes/";
		string text3 = Application.streamingAssetsPath + "/worldgen/subworlds/";
		WorldGen.LoadSettings();
		Dictionary<string, List<WeightedSubworldName>> dictionary2 = new Dictionary<string, List<WeightedSubworldName>>();
		foreach (KeyValuePair<string, ClusterLayout> item5 in SettingsCache.clusterLayouts.clusterCache)
		{
			ClusterLayout value = item5.Value;
			string filePath = value.filePath;
			foreach (WorldPlacement worldPlacement in value.worldPlacements)
			{
				ProcGen.World worldData = SettingsCache.worlds.GetWorldData(worldPlacement.world);
				foreach (WeightedSubworldName subworldFile in worldData.subworldFiles)
				{
					string text4 = subworldFile.name.Substring(subworldFile.name.LastIndexOf("/"));
					string text5 = subworldFile.name.Substring(0, subworldFile.name.Length - text4.Length);
					text5 = text5.Substring(text5.LastIndexOf("/") + 1);
					if (!(text5 == "subworlds"))
					{
						if (!dictionary2.ContainsKey(text5))
						{
							dictionary2.Add(text5, new List<WeightedSubworldName> { subworldFile });
						}
						else
						{
							dictionary2[text5].Add(subworldFile);
						}
					}
				}
			}
		}
		foreach (KeyValuePair<string, List<WeightedSubworldName>> item6 in dictionary2)
		{
			string text6 = CodexCache.FormatLinkID(item6.Key);
			Tuple<Sprite, Color> tuple = null;
			string text7 = Strings.Get("STRINGS.SUBWORLDS." + text6.ToUpper() + ".NAME");
			if (text7.Contains("MISSING"))
			{
				text7 = text6 + " (missing string key)";
			}
			List<ContentContainer> list = new List<ContentContainer>();
			GenerateTitleContainers(text7, list);
			string text8 = "biomeIcon" + char.ToUpper(text6[0]) + text6.Substring(1).ToLower();
			Sprite sprite = Assets.GetSprite(text8);
			if (sprite != null)
			{
				tuple = new Tuple<Sprite, Color>(sprite, Color.white);
			}
			else
			{
				Debug.LogWarning("Missing codex biome icon: " + text8);
			}
			string text9 = Strings.Get("STRINGS.SUBWORLDS." + text6.ToUpper() + ".DESC");
			string text10 = Strings.Get("STRINGS.SUBWORLDS." + text6.ToUpper() + ".UTILITY");
			ContentContainer item = new ContentContainer(new List<ICodexWidget>
			{
				new CodexText(string.IsNullOrEmpty(text9) ? "Basic description of the biome." : text9),
				new CodexSpacer(),
				new CodexText(string.IsNullOrEmpty(text10) ? "Description of the biomes utility." : text10),
				new CodexSpacer()
			}, ContentContainer.ContentLayout.Vertical);
			list.Add(item);
			Dictionary<string, float> dictionary3 = new Dictionary<string, float>();
			ContentContainer item2 = new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(UI.CODEX.SUBWORLDS.ELEMENTS, CodexTextStyle.Subtitle),
				new CodexSpacer()
			}, ContentContainer.ContentLayout.Vertical);
			list.Add(item2);
			ContentContainer contentContainer = new ContentContainer();
			contentContainer.contentLayout = ContentContainer.ContentLayout.Vertical;
			contentContainer.content = new List<ICodexWidget>();
			list.Add(contentContainer);
			foreach (WeightedSubworldName item7 in item6.Value)
			{
				SubWorld subWorld = SettingsCache.subworlds[item7.name];
				foreach (WeightedBiome biome in SettingsCache.subworlds[item7.name].biomes)
				{
					ElementBandConfiguration elementBandConfiguration = SettingsCache.biomes.BiomeBackgroundElementBandConfigurations[biome.name];
					foreach (ElementGradient item8 in elementBandConfiguration)
					{
						if (dictionary3.ContainsKey(item8.content))
						{
							dictionary3[item8.content] = dictionary3[item8.content] + item8.bandSize;
							continue;
						}
						if (ElementLoader.FindElementByName(item8.content) == null)
						{
							Debug.LogError("Biome " + biome.name + " contains non-existent element " + item8.content);
						}
						dictionary3.Add(item8.content, item8.bandSize);
					}
				}
				foreach (Feature feature in subWorld.features)
				{
					FeatureSettings cachedFeature = SettingsCache.GetCachedFeature(feature.type);
					foreach (KeyValuePair<string, ElementChoiceGroup<WeightedSimHash>> elementChoiceGroup in cachedFeature.ElementChoiceGroups)
					{
						foreach (WeightedSimHash choice in elementChoiceGroup.Value.choices)
						{
							if (dictionary3.ContainsKey(choice.element))
							{
								dictionary3[choice.element] = dictionary3[choice.element] + 1f;
							}
							else
							{
								dictionary3.Add(choice.element, 1f);
							}
						}
					}
				}
			}
			IOrderedEnumerable<KeyValuePair<string, float>> orderedEnumerable = dictionary3.OrderBy(delegate(KeyValuePair<string, float> pair)
			{
				KeyValuePair<string, float> keyValuePair = pair;
				return keyValuePair.Value;
			});
			foreach (KeyValuePair<string, float> item9 in orderedEnumerable)
			{
				Element element = ElementLoader.FindElementByName(item9.Key);
				if (tuple == null)
				{
					tuple = Def.GetUISprite(element.substance);
				}
				contentContainer.content.Add(new CodexIndentedLabelWithIcon(element.name, CodexTextStyle.Body, Def.GetUISprite(element.substance)));
			}
			List<Tag> list2 = new List<Tag>();
			ContentContainer item3 = new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(UI.CODEX.SUBWORLDS.PLANTS, CodexTextStyle.Subtitle),
				new CodexSpacer()
			}, ContentContainer.ContentLayout.Vertical);
			list.Add(item3);
			ContentContainer contentContainer2 = new ContentContainer();
			contentContainer2.contentLayout = ContentContainer.ContentLayout.Vertical;
			contentContainer2.content = new List<ICodexWidget>();
			list.Add(contentContainer2);
			foreach (WeightedSubworldName item10 in item6.Value)
			{
				foreach (WeightedBiome biome2 in SettingsCache.subworlds[item10.name].biomes)
				{
					if (biome2.tags == null)
					{
						continue;
					}
					foreach (string tag2 in biome2.tags)
					{
						if (!list2.Contains(tag2))
						{
							GameObject gameObject = Assets.TryGetPrefab(tag2);
							if (gameObject != null && (gameObject.GetComponent<Harvestable>() != null || gameObject.GetComponent<SeedProducer>() != null))
							{
								list2.Add(tag2);
								contentContainer2.content.Add(new CodexIndentedLabelWithIcon(gameObject.GetProperName(), CodexTextStyle.Body, Def.GetUISprite(gameObject)));
							}
						}
					}
				}
				foreach (Feature feature2 in SettingsCache.subworlds[item10.name].features)
				{
					FeatureSettings cachedFeature2 = SettingsCache.GetCachedFeature(feature2.type);
					foreach (MobReference internalMob in cachedFeature2.internalMobs)
					{
						Tag tag = internalMob.type.ToTag();
						if (!list2.Contains(tag))
						{
							GameObject gameObject2 = Assets.TryGetPrefab(tag);
							if (gameObject2 != null && (gameObject2.GetComponent<Harvestable>() != null || gameObject2.GetComponent<SeedProducer>() != null))
							{
								list2.Add(tag);
								contentContainer2.content.Add(new CodexIndentedLabelWithIcon(gameObject2.GetProperName(), CodexTextStyle.Body, Def.GetUISprite(gameObject2)));
							}
						}
					}
				}
			}
			if (list2.Count == 0)
			{
				contentContainer2.content.Add(new CodexIndentedLabelWithIcon(UI.CODEX.SUBWORLDS.NONE, CodexTextStyle.Body, new Tuple<Sprite, Color>(Assets.GetSprite("inspectorUI_cannot_build"), Color.red)));
			}
			ContentContainer item4 = new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(UI.CODEX.SUBWORLDS.CRITTERS, CodexTextStyle.Subtitle),
				new CodexSpacer()
			}, ContentContainer.ContentLayout.Vertical);
			list.Add(item4);
			DictionaryPool<Tag, GameObject, CodexEntry>.PooledDictionary pooledDictionary = DictionaryPool<Tag, GameObject, CodexEntry>.Allocate();
			CollectCritterTypes(item6.Value, pooledDictionary);
			ContentContainer contentContainer3 = new ContentContainer();
			contentContainer3.contentLayout = ContentContainer.ContentLayout.Vertical;
			contentContainer3.content = new List<ICodexWidget>();
			list.Add(contentContainer3);
			if (pooledDictionary.Count == 0)
			{
				contentContainer3.content.Add(new CodexIndentedLabelWithIcon(UI.CODEX.SUBWORLDS.NONE, CodexTextStyle.Body, new Tuple<Sprite, Color>(Assets.GetSprite("inspectorUI_cannot_build"), Color.red)));
			}
			else
			{
				foreach (KeyValuePair<Tag, GameObject> item11 in pooledDictionary)
				{
					contentContainer3.content.Add(new CodexIndentedLabelWithIcon(item11.Value.GetProperName(), CodexTextStyle.Body, Def.GetUISprite(item11.Value)));
				}
			}
			pooledDictionary.Recycle();
			string text11 = "BIOME" + text6;
			CodexEntry codexEntry = new CodexEntry("BIOMES", list, text11);
			codexEntry.name = text7;
			codexEntry.parentId = "BIOMES";
			codexEntry.icon = tuple.first;
			codexEntry.iconColor = tuple.second;
			CodexCache.AddEntry(text11, codexEntry);
			dictionary.Add(text11, codexEntry);
		}
		if (Application.isPlaying)
		{
			Global.Instance.modManager.HandleErrors(pooledList);
		}
		else
		{
			foreach (YamlIO.Error item12 in pooledList)
			{
				YamlIO.LogError(item12, force_log_as_warning: false);
			}
		}
		pooledList.Recycle();
		return dictionary;
	}

	private static void CollectCritterTypes(IReadOnlyList<WeightedSubworldName> subworlds, Dictionary<Tag, GameObject> critterTypes)
	{
		for (int i = 0; i != subworlds.Count; i++)
		{
			WeightedSubworldName weightedSubworldName = subworlds[i];
			foreach (WeightedBiome biome in SettingsCache.subworlds[weightedSubworldName.name].biomes)
			{
				if (biome.tags == null)
				{
					continue;
				}
				foreach (string tag in biome.tags)
				{
					TryAddCritterType(tag, critterTypes);
				}
			}
			foreach (Feature feature in SettingsCache.subworlds[weightedSubworldName.name].features)
			{
				FeatureSettings cachedFeature = SettingsCache.GetCachedFeature(feature.type);
				foreach (MobReference internalMob in cachedFeature.internalMobs)
				{
					TryAddCritterType(internalMob.type.ToTag(), critterTypes);
				}
			}
		}
		static void TryAddCritterType(Tag tag, Dictionary<Tag, GameObject> _critterTypes)
		{
			if (!_critterTypes.ContainsKey(tag))
			{
				GameObject gameObject = Assets.TryGetPrefab(tag);
				if (!(gameObject == null) && gameObject.HasTag(GameTags.Creature))
				{
					_critterTypes.Add(tag, gameObject);
				}
			}
		}
	}

	public static void CollectCritterTypes(Dictionary<Tag, GameObject> critterTypes)
	{
		foreach (KeyValuePair<string, ClusterLayout> item in SettingsCache.clusterLayouts.clusterCache)
		{
			foreach (WorldPlacement worldPlacement in item.Value.worldPlacements)
			{
				ProcGen.World worldData = SettingsCache.worlds.GetWorldData(worldPlacement.world);
				CollectCritterTypes(worldData.subworldFiles, critterTypes);
			}
		}
	}

	public static Dictionary<string, CodexEntry> GenerateConstructionMaterialEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		Dictionary<Tag, List<BuildingDef>> dictionary2 = new Dictionary<Tag, List<BuildingDef>>();
		foreach (BuildingDef buildingDef in Assets.BuildingDefs)
		{
			if (buildingDef.Deprecated || buildingDef.DebugOnly || !Game.IsCorrectDlcActiveForCurrentSave(buildingDef) || (!buildingDef.ShowInBuildMenu && !buildingDef.BuildingComplete.HasTag(GameTags.RocketModule)))
			{
				continue;
			}
			string[] materialCategory = buildingDef.MaterialCategory;
			foreach (string text in materialCategory)
			{
				string[] array = text.Split('&');
				foreach (string name in array)
				{
					Tag key = new Tag(name);
					if (!dictionary2.ContainsKey(key))
					{
						dictionary2.Add(key, new List<BuildingDef>());
					}
					dictionary2[key].Add(buildingDef);
				}
			}
		}
		foreach (Tag key2 in dictionary2.Keys)
		{
			if (ElementLoader.GetElement(key2) != null)
			{
				continue;
			}
			string text2 = key2.ToString();
			string name2 = Strings.Get("STRINGS.MISC.TAGS." + text2.ToUpper());
			List<ContentContainer> list = new List<ContentContainer>();
			GenerateTitleContainers(name2, list);
			list.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(Strings.Get("STRINGS.MISC.TAGS." + text2.ToUpper() + "_DESC")),
				new CodexSpacer()
			}, ContentContainer.ContentLayout.Vertical));
			List<ICodexWidget> list2 = new List<ICodexWidget>();
			List<Tag> validMaterials = MaterialSelector.GetValidMaterials(key2, omitDisabledElements: true);
			foreach (Tag item in validMaterials)
			{
				list2.Add(new CodexIndentedLabelWithIcon(item.ProperName(), CodexTextStyle.Body, Def.GetUISprite(item)));
			}
			list.Add(new ContentContainer(list2, ContentContainer.ContentLayout.GridTwoColumn));
			list.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(CODEX.HEADERS.MATERIALUSEDTOCONSTRUCT, CodexTextStyle.Title),
				new CodexDividerLine()
			}, ContentContainer.ContentLayout.Vertical));
			List<ICodexWidget> list3 = new List<ICodexWidget>();
			foreach (BuildingDef item2 in dictionary2[key2])
			{
				list3.Add(new CodexIndentedLabelWithIcon(item2.Name, CodexTextStyle.Body, Def.GetUISprite(item2.Tag)));
			}
			list.Add(new ContentContainer(list3, ContentContainer.ContentLayout.GridTwoColumn));
			CodexEntry codexEntry = new CodexEntry("BUILDINGMATERIALCLASSES", list, name2);
			codexEntry.parentId = codexEntry.category;
			codexEntry.icon = Assets.GetSprite("ui_" + key2.Name.ToLower()) ?? ((validMaterials.Count != 0) ? Def.GetUISprite(validMaterials[0]).first : null) ?? Assets.GetSprite("ui_elements_classes");
			if (key2 == GameTags.BuildableAny)
			{
				codexEntry.icon = Assets.GetSprite("ui_elements_classes");
			}
			CodexCache.AddEntry(CodexCache.FormatLinkID(text2), codexEntry);
			dictionary.Add(text2, codexEntry);
		}
		return dictionary;
	}

	public static Dictionary<string, CodexEntry> GenerateDiseaseEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		foreach (Disease resource in Db.Get().Diseases.resources)
		{
			if (!resource.Disabled)
			{
				List<ContentContainer> list = new List<ContentContainer>();
				GenerateTitleContainers(resource.Name, list);
				GenerateDiseaseDescriptionContainers(resource, list);
				CodexEntry codexEntry = new CodexEntry("DISEASE", list, resource.Name);
				codexEntry.parentId = "DISEASE";
				dictionary.Add(resource.Id, codexEntry);
				codexEntry.icon = Assets.GetSprite("overlay_disease");
				CodexCache.AddEntry(resource.Id, codexEntry);
			}
		}
		return dictionary;
	}

	public static CategoryEntry GenerateCategoryEntry(string id, string name, Dictionary<string, CodexEntry> entries, Sprite icon = null, bool largeFormat = true, bool sort = true, string overrideHeader = null)
	{
		List<ContentContainer> list = new List<ContentContainer>();
		GenerateTitleContainers((overrideHeader == null) ? name : overrideHeader, list);
		List<CodexEntry> list2 = new List<CodexEntry>();
		foreach (KeyValuePair<string, CodexEntry> entry in entries)
		{
			list2.Add(entry.Value);
			if (icon == null)
			{
				icon = entry.Value.icon;
			}
		}
		CategoryEntry categoryEntry = new CategoryEntry("Root", list, name, list2, largeFormat, sort);
		categoryEntry.icon = icon;
		CodexCache.AddEntry(id, categoryEntry);
		return categoryEntry;
	}

	public static Dictionary<string, CodexEntry> GenerateTutorialNotificationEntries()
	{
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		Dictionary<string, CodexEntry> dictionary2 = new Dictionary<string, CodexEntry>();
		Dictionary<string, CodexEntry> dictionary3 = new Dictionary<string, CodexEntry>();
		Dictionary<string, CodexEntry> entries = new Dictionary<string, CodexEntry>();
		for (int i = 0; i < 24; i++)
		{
			TutorialMessage tutorialMessage = (TutorialMessage)Tutorial.Instance.TutorialMessage((Tutorial.TutorialMessages)i, queueMessage: false);
			if (tutorialMessage != null && Game.IsCorrectDlcActiveForCurrentSave(tutorialMessage))
			{
				if (!string.IsNullOrEmpty(tutorialMessage.videoClipId))
				{
					List<ContentContainer> list = new List<ContentContainer>();
					GenerateTitleContainers(tutorialMessage.GetTitle(), list);
					CodexVideo codexVideo = new CodexVideo();
					codexVideo.videoName = tutorialMessage.videoClipId;
					codexVideo.overlayName = tutorialMessage.videoOverlayName;
					codexVideo.overlayTexts = new List<string>
					{
						tutorialMessage.videoTitleText,
						VIDEOS.TUTORIAL_HEADER
					};
					list.Add(new ContentContainer(new List<ICodexWidget> { codexVideo }, ContentContainer.ContentLayout.Vertical));
					list.Add(new ContentContainer(new List<ICodexWidget>
					{
						new CodexText(tutorialMessage.GetMessageBody(), CodexTextStyle.Body, tutorialMessage.GetTitle())
					}, ContentContainer.ContentLayout.Vertical));
					CodexEntry codexEntry = new CodexEntry("VIDEOTUTORIALS", list, UI.FormatAsLink(tutorialMessage.GetTitle(), "videos_" + i));
					codexEntry.parentId = "VIDEOTUTORIALS";
					codexEntry.icon = Assets.GetSprite("codexVideo");
					CodexCache.AddEntry("videos_" + i, codexEntry);
					dictionary2.Add(codexEntry.id, codexEntry);
				}
				else
				{
					List<ContentContainer> list2 = new List<ContentContainer>();
					GenerateTitleContainers(tutorialMessage.GetTitle(), list2);
					list2.Add(new ContentContainer(new List<ICodexWidget>
					{
						new CodexText(tutorialMessage.GetMessageBody(), CodexTextStyle.Body, tutorialMessage.GetTitle())
					}, ContentContainer.ContentLayout.Vertical));
					list2.Add(new ContentContainer(new List<ICodexWidget>
					{
						new CodexSpacer(),
						new CodexSpacer()
					}, ContentContainer.ContentLayout.Vertical));
					CodexEntry codexEntry2 = new CodexEntry("MISCELLANEOUSTIPS", list2, tutorialMessage.GetTitle());
					codexEntry2.parentId = "MISCELLANEOUSTIPS";
					CodexCache.AddEntry("MISCELLANEOUSTIPS_" + i, codexEntry2);
					dictionary3.Add(codexEntry2.id, codexEntry2);
				}
			}
		}
		CategoryEntry categoryEntry = GenerateCategoryEntry("VIDEOTUTORIALS", Strings.Get("STRINGS.UI.CODEX.CATEGORYNAMES.VIDEOTUTORIALS"), dictionary2, Assets.GetSprite("codexVideo"), largeFormat: true, sort: false, Strings.Get("STRINGS.UI.CODEX.CATEGORYNAMES.VIDEOTUTORIALS"));
		CategoryEntry categoryEntry2 = GenerateCategoryEntry("SYSTEMSTUTORIALS", Strings.Get("STRINGS.UI.CODEX.CATEGORYNAMES.SYSTEMSTUTORIALS"), entries, Assets.GetSprite("ui_systems"), largeFormat: true, sort: false, Strings.Get("STRINGS.UI.CODEX.CATEGORYNAMES.SYSTEMSTUTORIALS"));
		CategoryEntry categoryEntry3 = GenerateCategoryEntry("MISCELLANEOUSTIPS", Strings.Get("STRINGS.UI.CODEX.CATEGORYNAMES.MISCELLANEOUSTIPS"), dictionary3, Assets.GetSprite("ui_tips"), largeFormat: false, sort: false, Strings.Get("STRINGS.UI.CODEX.CATEGORYNAMES.MISCELLANEOUSTIPS"));
		categoryEntry.parentId = "LESSONS";
		categoryEntry2.parentId = "LESSONS";
		categoryEntry3.parentId = "LESSONS";
		dictionary.Add("VIDEOTUTORIALS", categoryEntry);
		dictionary.Add("SYSTEMSTUTORIALS", categoryEntry2);
		dictionary.Add("MISCELLANEOUSTIPS", categoryEntry3);
		foreach (KeyValuePair<string, CodexEntry> item in dictionary)
		{
			CodexCache.categoriesForPostYAMLPopulation.Add(item.Value as CategoryEntry);
		}
		return dictionary;
	}

	public static void PopulateCategoryEntries(Dictionary<string, CodexEntry> categoryEntries)
	{
		List<CategoryEntry> list = new List<CategoryEntry>();
		foreach (KeyValuePair<string, CodexEntry> categoryEntry in categoryEntries)
		{
			list.Add(categoryEntry.Value as CategoryEntry);
		}
		PopulateCategoryEntries(list);
	}

	public static void PopulateCategoryEntries(List<CategoryEntry> categoryEntries, Comparison<CodexEntry> comparison = null)
	{
		foreach (CategoryEntry categoryEntry in categoryEntries)
		{
			List<ContentContainer> contentContainers = categoryEntry.contentContainers;
			List<CodexEntry> list = new List<CodexEntry>();
			foreach (CodexEntry item3 in categoryEntry.entriesInCategory)
			{
				list.Add(item3);
			}
			if (categoryEntry.sort)
			{
				if (comparison == null)
				{
					list.Sort((CodexEntry a, CodexEntry b) => UI.StripLinkFormatting(a.name).CompareTo(UI.StripLinkFormatting(b.name)));
				}
				else
				{
					list.Sort(comparison);
				}
			}
			if (categoryEntry.largeFormat)
			{
				ContentContainer contentContainer = new ContentContainer(new List<ICodexWidget>(), ContentContainer.ContentLayout.Grid);
				foreach (CodexEntry item4 in list)
				{
					contentContainer.content.Add(new CodexLabelWithLargeIcon(item4.name, CodexTextStyle.BodyWhite, new Tuple<Sprite, Color>((item4.icon != null) ? item4.icon : Assets.GetSprite("unknown"), item4.iconColor), item4.id));
				}
				if (categoryEntry.showBeforeGeneratedCategoryLinks)
				{
					contentContainers.Add(contentContainer);
					continue;
				}
				ContentContainer item = contentContainers[contentContainers.Count - 1];
				contentContainers.RemoveAt(contentContainers.Count - 1);
				contentContainers.Insert(0, item);
				contentContainers.Insert(1, contentContainer);
				contentContainers.Insert(2, new ContentContainer(new List<ICodexWidget>
				{
					new CodexSpacer()
				}, ContentContainer.ContentLayout.Vertical));
				continue;
			}
			ContentContainer contentContainer2 = new ContentContainer(new List<ICodexWidget>(), ContentContainer.ContentLayout.Vertical);
			foreach (CodexEntry item5 in list)
			{
				if (item5.icon == null)
				{
					contentContainer2.content.Add(new CodexText(UI.FormatAsLink(Util.StripTextFormatting(item5.name), item5.id)));
				}
				else
				{
					contentContainer2.content.Add(new CodexLabelWithIcon(UI.FormatAsLink(Util.StripTextFormatting(item5.name), item5.id), CodexTextStyle.Body, new Tuple<Sprite, Color>(item5.icon, item5.iconColor), 64, 48));
				}
			}
			if (categoryEntry.showBeforeGeneratedCategoryLinks)
			{
				contentContainers.Add(contentContainer2);
				continue;
			}
			ContentContainer item2 = contentContainers[contentContainers.Count - 1];
			contentContainers.RemoveAt(contentContainers.Count - 1);
			contentContainers.Insert(0, item2);
			contentContainers.Insert(1, contentContainer2);
		}
	}

	public static void GenerateTitleContainers(string name, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		list.Add(new CodexText(name, CodexTextStyle.Title));
		list.Add(new CodexDividerLine());
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
	}

	private static void GeneratePrerequisiteTechContainers(Tech tech, List<ContentContainer> containers)
	{
		if (tech.requiredTech == null || tech.requiredTech.Count == 0)
		{
			return;
		}
		List<ICodexWidget> list = new List<ICodexWidget>();
		list.Add(new CodexText(CODEX.HEADERS.PREREQUISITE_TECH, CodexTextStyle.Subtitle));
		list.Add(new CodexDividerLine());
		list.Add(new CodexSpacer());
		foreach (Tech item in tech.requiredTech)
		{
			list.Add(new CodexText(item.Name));
		}
		list.Add(new CodexSpacer());
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
	}

	private static void GenerateSkillRequirementsAndPerksContainers(Skill skill, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		string text = CODEX.HEADERS.ROLE_PERKS;
		string text2 = CODEX.HEADERS.ROLE_PERKS_DESC;
		if (DlcManager.DlcListContains(skill.GetRequiredDlcIds(), "DLC3_ID"))
		{
			text = CODEX.HEADERS.ROLE_PERKS_BIONIC;
			text2 = CODEX.HEADERS.ROLE_PERKS_BIONIC_DESC;
		}
		CodexText item = new CodexText(text, CodexTextStyle.Subtitle);
		CodexText item2 = new CodexText(text2);
		list.Add(item);
		list.Add(new CodexDividerLine());
		list.Add(item2);
		list.Add(new CodexSpacer());
		foreach (SkillPerk perk in skill.perks)
		{
			if (Game.IsCorrectDlcActiveForCurrentSave(perk))
			{
				CodexText item3 = new CodexText(SkillPerk.GetDescription(perk.Id));
				list.Add(item3);
			}
		}
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
		list.Add(new CodexSpacer());
	}

	private static void GenerateRelatedSkillContainers(Skill skill, List<ContentContainer> containers)
	{
		bool flag = false;
		List<ICodexWidget> list = new List<ICodexWidget>();
		CodexText item = new CodexText(CODEX.HEADERS.PREREQUISITE_ROLES, CodexTextStyle.Subtitle);
		list.Add(item);
		list.Add(new CodexDividerLine());
		list.Add(new CodexSpacer());
		foreach (string priorSkill in skill.priorSkills)
		{
			CodexText item2 = new CodexText(Db.Get().Skills.Get(priorSkill).Name);
			list.Add(item2);
			flag = true;
		}
		if (flag)
		{
			list.Add(new CodexSpacer());
			containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
		}
		bool flag2 = false;
		List<ICodexWidget> list2 = new List<ICodexWidget>();
		string text = CODEX.HEADERS.UNLOCK_ROLES;
		string text2 = CODEX.HEADERS.UNLOCK_ROLES_DESC;
		if (DlcManager.DlcListContains(skill.GetRequiredDlcIds(), "DLC3_ID"))
		{
			text = CODEX.HEADERS.UNLOCK_ROLES_BIONIC;
			text2 = CODEX.HEADERS.UNLOCK_ROLES_BIONIC_DESC;
		}
		CodexText item3 = new CodexText(text, CodexTextStyle.Subtitle);
		CodexText item4 = new CodexText(text2);
		list2.Add(item3);
		list2.Add(new CodexDividerLine());
		list2.Add(item4);
		list2.Add(new CodexSpacer());
		foreach (Skill resource in Db.Get().Skills.resources)
		{
			if (resource.deprecated)
			{
				continue;
			}
			foreach (string priorSkill2 in resource.priorSkills)
			{
				if (priorSkill2 == skill.Id)
				{
					CodexText item5 = new CodexText(resource.Name);
					list2.Add(item5);
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			list2.Add(new CodexSpacer());
			containers.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
		}
	}

	private static void GenerateUnlockContainers(Tech tech, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		CodexText item = new CodexText(CODEX.HEADERS.TECH_UNLOCKS, CodexTextStyle.Subtitle);
		list.Add(item);
		list.Add(new CodexDividerLine());
		list.Add(new CodexSpacer());
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
		foreach (TechItem unlockedItem in tech.unlockedItems)
		{
			List<ICodexWidget> list2 = new List<ICodexWidget>();
			CodexImage item2 = new CodexImage(64, 64, unlockedItem.getUISprite("ui", arg2: false));
			list2.Add(item2);
			CodexText item3 = new CodexText(unlockedItem.Name);
			list2.Add(item3);
			containers.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Horizontal));
		}
	}

	private static void GenerateRecipeContainers(Tag prefabID, List<ContentContainer> containers)
	{
		Recipe recipe = null;
		foreach (Recipe recipe2 in RecipeManager.Get().recipes)
		{
			if (recipe2.Result == prefabID)
			{
				recipe = recipe2;
				break;
			}
		}
		if (recipe == null)
		{
			return;
		}
		containers.Add(new ContentContainer(new List<ICodexWidget>
		{
			new CodexText(CODEX.HEADERS.RECIPE, CodexTextStyle.Subtitle),
			new CodexSpacer(),
			new CodexDividerLine()
		}, ContentContainer.ContentLayout.Vertical));
		Func<Recipe, List<ContentContainer>> func = delegate(Recipe rec)
		{
			List<ContentContainer> list = new List<ContentContainer>();
			foreach (Recipe.Ingredient ingredient in rec.Ingredients)
			{
				GameObject prefab = Assets.GetPrefab(ingredient.tag);
				if (prefab != null)
				{
					list.Add(new ContentContainer(new List<ICodexWidget>
					{
						new CodexImage(64, 64, Def.GetUISprite(prefab)),
						new CodexText(GameUtil.SafeStringFormat(UI.CODEX.RECIPE_ITEM, Assets.GetPrefab(ingredient.tag).GetProperName(), ingredient.amount, (ElementLoader.GetElement(ingredient.tag) == null) ? "" : UI.UNITSUFFIXES.MASS.KILOGRAM.text))
					}, ContentContainer.ContentLayout.Horizontal));
				}
			}
			return list;
		};
		containers.AddRange(func(recipe));
		GameObject gameObject = ((recipe.fabricators == null) ? null : Assets.GetPrefab(recipe.fabricators[0]));
		if (gameObject != null)
		{
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexText(UI.CODEX.RECIPE_FABRICATOR_HEADER, CodexTextStyle.Subtitle),
				new CodexDividerLine()
			}, ContentContainer.ContentLayout.Vertical));
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexImage(64, 64, Def.GetUISpriteFromMultiObjectAnim(gameObject.GetComponent<KBatchedAnimController>().AnimFiles[0])),
				new CodexText(string.Format(UI.CODEX.RECIPE_FABRICATOR, recipe.FabricationTime, gameObject.GetProperName()))
			}, ContentContainer.ContentLayout.Horizontal));
		}
	}

	private static void GenerateRoomTypeDetailsContainers(RoomType roomType, List<ContentContainer> containers)
	{
		ICodexWidget item = new CodexText(UI.CODEX.DETAILS, CodexTextStyle.Subtitle);
		ICodexWidget item2 = new CodexDividerLine();
		ContentContainer item3 = new ContentContainer(new List<ICodexWidget> { item, item2 }, ContentContainer.ContentLayout.Vertical);
		containers.Add(item3);
		List<ICodexWidget> list = new List<ICodexWidget>();
		if (!string.IsNullOrEmpty(roomType.effect))
		{
			string roomEffectsString = roomType.GetRoomEffectsString();
			list.Add(new CodexText(roomEffectsString));
			list.Add(new CodexSpacer());
		}
		if (roomType.primary_constraint != null || roomType.additional_constraints != null)
		{
			list.Add(new CodexText(ROOMS.CRITERIA.HEADER));
			string text = "";
			if (roomType.primary_constraint != null)
			{
				text = text + "    • " + roomType.primary_constraint.name;
			}
			if (roomType.additional_constraints != null)
			{
				for (int i = 0; i < roomType.additional_constraints.Length; i++)
				{
					text = text + "\n    • " + roomType.additional_constraints[i].name;
				}
			}
			list.Add(new CodexText(text));
		}
		ContentContainer item4 = new ContentContainer(list, ContentContainer.ContentLayout.Vertical);
		containers.Add(item4);
	}

	private static void GenerateRoomTypeDescriptionContainers(RoomType roomType, List<ContentContainer> containers)
	{
		ContentContainer item = new ContentContainer(new List<ICodexWidget>
		{
			new CodexText(roomType.description),
			new CodexSpacer()
		}, ContentContainer.ContentLayout.Vertical);
		containers.Add(item);
	}

	private static void GeneratePlantDescriptionContainers(GameObject plant, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		list.Add(new CodexSpacer());
		list.Add(new CodexText(UI.CODEX.DETAILS, CodexTextStyle.Subtitle));
		list.Add(new CodexDividerLine());
		InfoDescription component = Assets.GetPrefab(plant.PrefabID()).GetComponent<InfoDescription>();
		if (component != null)
		{
			list.Add(new CodexText(component.description));
		}
		string text = "";
		List<Descriptor> plantRequirementDescriptors = GameUtil.GetPlantRequirementDescriptors(plant);
		if (plantRequirementDescriptors.Count > 0)
		{
			text += plantRequirementDescriptors[0].text;
			for (int i = 1; i < plantRequirementDescriptors.Count; i++)
			{
				text = text + "\n    • " + plantRequirementDescriptors[i].text;
			}
			list.Add(new CodexText(text));
			list.Add(new CodexSpacer());
		}
		text = "";
		List<Descriptor> plantEffectDescriptors = GameUtil.GetPlantEffectDescriptors(plant);
		if (plantEffectDescriptors.Count > 0)
		{
			text += plantEffectDescriptors[0].text;
			for (int j = 1; j < plantEffectDescriptors.Count; j++)
			{
				text = text + "\n    • " + plantEffectDescriptors[j].text;
			}
			CodexText item = new CodexText(text);
			list.Add(item);
			list.Add(new CodexSpacer());
		}
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
	}

	private static ICodexWidget GetIconWidget(object entity)
	{
		return new CodexImage(32, 32, Def.GetUISprite(entity));
	}

	public static void GenerateManualConversionEntries(Tag tag, List<ContentContainer> containers)
	{
		CodexEntryGenerator_Elements.GenerateMadeAndUsedContainers(tag, containers);
	}

	private static void GenerateDiseaseDescriptionContainers(Disease disease, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		list.Add(new CodexSpacer());
		StringEntry result = null;
		if (Strings.TryGet("STRINGS.DUPLICANTS.DISEASES." + disease.Id.ToUpper() + ".DESC", out result))
		{
			list.Add(new CodexText(result.String));
			list.Add(new CodexSpacer());
		}
		foreach (Descriptor quantitativeDescriptor in disease.GetQuantitativeDescriptors())
		{
			list.Add(new CodexText(quantitativeDescriptor.text));
		}
		list.Add(new CodexSpacer());
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
	}

	private static void GenerateFoodDescriptionContainers(EdiblesManager.FoodInfo food, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		list.Add(new CodexText(food.Description));
		list.Add(new CodexSpacer());
		list.Add(new CodexText(GameUtil.SafeStringFormat(UI.CODEX.FOOD.QUALITY, GameUtil.GetFormattedFoodQuality(food.Quality))));
		list.Add(new CodexText(GameUtil.SafeStringFormat(UI.CODEX.FOOD.CALORIES, GameUtil.GetFormattedCalories(food.CaloriesPerUnit))));
		list.Add(new CodexSpacer());
		list.Add(new CodexText(food.CanRot ? GameUtil.SafeStringFormat(UI.CODEX.FOOD.SPOILPROPERTIES, GameUtil.GetFormattedTemperature(food.RotTemperature), GameUtil.GetFormattedTemperature(food.PreserveTemperature), GameUtil.GetFormattedCycles(food.SpoilTime)) : UI.CODEX.FOOD.NON_PERISHABLE.ToString()));
		list.Add(new CodexSpacer());
		List<ICodexWidget> list2 = list;
		if (food.Effects.Count > 0)
		{
			list2.Add(new CodexText(string.Concat(CODEX.HEADERS.FOODEFFECTS, ":")));
			foreach (string effect2 in food.Effects)
			{
				Effect effect = Db.Get().effects.Get(effect2);
				string text = Strings.Get("STRINGS.DUPLICANTS.MODIFIERS." + effect2.ToUpper() + ".NAME");
				string text2 = Strings.Get("STRINGS.DUPLICANTS.MODIFIERS." + effect2.ToUpper() + ".DESCRIPTION");
				string text3 = "";
				foreach (AttributeModifier selfModifier in effect.SelfModifiers)
				{
					text3 = string.Concat(text3, "\n    • ", Strings.Get("STRINGS.DUPLICANTS.ATTRIBUTES." + selfModifier.AttributeId.ToUpper() + ".NAME"), ": ", selfModifier.GetFormattedString());
				}
				text2 += text3;
				text = UI.FormatAsLink(text, FOOD_EFFECTS_ENTRY_ID + "::" + effect2.ToUpper());
				list2.Add(new CodexTextWithTooltip("    • " + text, text2));
			}
			list2.Add(new CodexSpacer());
		}
		containers.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
	}

	private static void GenerateTechDescriptionContainers(Tech tech, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		CodexText item = new CodexText(Strings.Get("STRINGS.RESEARCH.TECHS." + tech.Id.ToUpper() + ".DESC"));
		list.Add(item);
		list.Add(new CodexSpacer());
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
	}

	private static void GenerateGenericDescriptionContainers(string description, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		CodexText item = new CodexText(description);
		list.Add(item);
		list.Add(new CodexSpacer());
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
	}

	private static void AddDescriptorWidgets(List<ICodexWidget> widgets, string title, List<Descriptor> descriptors)
	{
		if (descriptors == null || descriptors.Count == 0)
		{
			return;
		}
		widgets.Add(new CodexText(title, CodexTextStyle.Subtitle));
		foreach (Descriptor descriptor in descriptors)
		{
			widgets.Add(new CodexTextWithTooltip("    " + descriptor.text, descriptor.tooltipText));
		}
		widgets.Add(new CodexSpacer());
	}

	private static void AddRequirementDescriptors(List<ICodexWidget> widgets, List<Descriptor> nonConverterReqs, List<(ElementConverter converter, List<Descriptor> descriptors)> converterDescCache, bool hasConverterReqs)
	{
		if (nonConverterReqs.Count <= 0 && converterDescCache.Count <= 0)
		{
			return;
		}
		widgets.Add(new CodexText(CODEX.HEADERS.BUILDINGREQUIREMENTS, CodexTextStyle.Subtitle));
		foreach (Descriptor nonConverterReq in nonConverterReqs)
		{
			widgets.Add(new CodexTextWithTooltip("    " + nonConverterReq.text, nonConverterReq.tooltipText));
		}
		if (hasConverterReqs)
		{
			widgets.Add(new CodexText("    " + UI.BUILDINGEFFECTS.OPERATIONINPUTS));
		}
		foreach (var (elementConverter, list) in converterDescCache)
		{
			foreach (Descriptor item in list)
			{
				if (item.type == Descriptor.DescriptorType.Requirement)
				{
					widgets.Add(new CodexTextWithTooltip("        • " + item.text, item.tooltipText));
				}
			}
		}
		widgets.Add(new CodexSpacer());
	}

	private static void AddConstructionPropertyDescriptors(List<ICodexWidget> widgets, BuildingDef def)
	{
		if (def.MaterialCategory.Length != def.Mass.Length)
		{
			Debug.LogWarningFormat("{0} Required Materials({1}) and Masses({2}) mismatch!", def.name, string.Join(", ", def.MaterialCategory), string.Join(", ", def.Mass));
		}
		if (def.MaterialCategory.Length + def.Mass.Length != 0)
		{
			widgets.Add(new CodexText(CODEX.HEADERS.BUILDINGCONSTRUCTIONPROPS, CodexTextStyle.Subtitle));
			widgets.Add(new CodexText("    " + string.Format(CODEX.FORMAT_STRINGS.BUILDING_SIZE, def.WidthInCells, def.HeightInCells)));
			widgets.Add(new CodexText("    " + string.Format(CODEX.FORMAT_STRINGS.CONSTRUCTION_TIME, def.ConstructionTime)));
			List<string> list = new List<string>();
			for (int i = 0; i < Math.Min(def.MaterialCategory.Length, def.Mass.Length); i++)
			{
				list.Add(string.Format(CODEX.FORMAT_STRINGS.MATERIAL_MASS, MATERIALS.GetMaterialString(def.MaterialCategory[i]), GameUtil.GetFormattedMass(def.Mass[i])));
			}
			widgets.Add(new CodexText(string.Concat("    ", CODEX.HEADERS.BUILDINGCONSTRUCTIONMATERIALS, string.Join(", ", list))));
			widgets.Add(new CodexSpacer());
		}
	}

	private static void AddEffectDescriptors(List<ICodexWidget> widgets, List<Descriptor> nonConverterEffects, List<(ElementConverter converter, List<Descriptor> descriptors)> converterDescCache)
	{
		if (nonConverterEffects.Count <= 0 && converterDescCache.Count <= 0)
		{
			return;
		}
		widgets.Add(new CodexText(CODEX.HEADERS.BUILDINGEFFECTS, CodexTextStyle.Subtitle));
		foreach (Descriptor nonConverterEffect in nonConverterEffects)
		{
			widgets.Add(new CodexTextWithTooltip("    " + nonConverterEffect.text, nonConverterEffect.tooltipText));
		}
		foreach (var item3 in converterDescCache)
		{
			ElementConverter item = item3.converter;
			List<Descriptor> item2 = item3.descriptors;
			string text = item.consumedElements[0].Name;
			for (int i = 1; i < item.consumedElements.Length; i++)
			{
				text = text + ", " + item.consumedElements[i].Name;
			}
			widgets.Add(new CodexText("    " + text + ":"));
			foreach (Descriptor item4 in item2)
			{
				if (item4.type != Descriptor.DescriptorType.Requirement)
				{
					widgets.Add(new CodexTextWithTooltip("        " + item4.text, item4.tooltipText));
				}
			}
		}
		widgets.Add(new CodexSpacer());
	}

	private static void GenerateBuildingDescriptionContainers(BuildingDef def, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>
		{
			new CodexText(Strings.Get("STRINGS.BUILDINGS.PREFABS." + def.PrefabID.ToUpper() + ".EFFECT")),
			new CodexSpacer()
		};
		ListPool<(ElementConverter, List<Descriptor>), BuildingDef>.PooledList pooledList = ListPool<(ElementConverter, List<Descriptor>), BuildingDef>.Allocate();
		ListPool<Descriptor, BuildingDef>.PooledList pooledList2 = ListPool<Descriptor, BuildingDef>.Allocate();
		ListPool<Descriptor, BuildingDef>.PooledList pooledList3 = ListPool<Descriptor, BuildingDef>.Allocate();
		GameUtil.PartitionBuildingDescriptors(def.BuildingComplete, simpleInfoScreen: false, out var _, pooledList, pooledList2, pooledList3, out var hasConverterReqs);
		AddRequirementDescriptors(list, pooledList2, pooledList, hasConverterReqs);
		pooledList2.Recycle();
		AddConstructionPropertyDescriptors(list, def);
		AddEffectDescriptors(list, pooledList3, pooledList);
		pooledList3.Recycle();
		pooledList.Recycle();
		string[] roomClassForObject = GetRoomClassForObject(def.BuildingComplete);
		string[] categoriesForObject = GetCategoriesForObject(def.BuildingComplete);
		bool flag = roomClassForObject != null || categoriesForObject != null;
		if (flag)
		{
			list.Add(new CodexText(CODEX.HEADERS.BUILDINGTYPE, CodexTextStyle.Subtitle));
		}
		if (roomClassForObject != null)
		{
			string[] array = roomClassForObject;
			foreach (string text in array)
			{
				list.Add(new CodexText("    " + text));
			}
		}
		if (categoriesForObject != null)
		{
			string[] array2 = categoriesForObject;
			foreach (string text2 in array2)
			{
				list.Add(new CodexText("    " + text2));
			}
		}
		if (flag)
		{
			list.Add(new CodexSpacer());
		}
		list.Add(new CodexText(string.Concat("<i>", Strings.Get("STRINGS.BUILDINGS.PREFABS." + def.PrefabID.ToUpper() + ".DESC"), "</i>")));
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
	}

	public static string[] GetCategoriesForObject(GameObject obj)
	{
		List<string> list = new List<string>();
		KPrefabID component = obj.GetComponent<KPrefabID>();
		if (component != null)
		{
			foreach (Tag tag in component.Tags)
			{
				if (GameTags.CodexCategories.AllTags.Contains(tag))
				{
					list.Add(GameTags.CodexCategories.GetCategoryLabelText(tag));
				}
			}
		}
		return (list.Count > 0) ? list.ToArray() : null;
	}

	public static string[] GetRoomClassForObject(GameObject obj)
	{
		List<string> list = new List<string>();
		KPrefabID component = obj.GetComponent<KPrefabID>();
		if (component != null)
		{
			foreach (Tag tag in component.Tags)
			{
				if (RoomConstraints.ConstraintTags.AllTags.Contains(tag))
				{
					list.Add(RoomConstraints.ConstraintTags.GetRoomConstraintLabelText(tag));
				}
			}
		}
		return (list.Count > 0) ? list.ToArray() : null;
	}

	public static void GenerateImageContainers(Sprite[] sprites, List<ContentContainer> containers, ContentContainer.ContentLayout layout)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		foreach (Sprite sprite in sprites)
		{
			if (!(sprite == null))
			{
				CodexImage item = new CodexImage(128, 128, sprite);
				list.Add(item);
			}
		}
		containers.Add(new ContentContainer(list, layout));
	}

	public static void GenerateImageContainers(Tuple<Sprite, Color>[] sprites, List<ContentContainer> containers, ContentContainer.ContentLayout layout)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		foreach (Tuple<Sprite, Color> tuple in sprites)
		{
			if (tuple != null)
			{
				CodexImage item = new CodexImage(128, 128, tuple);
				list.Add(item);
			}
		}
		containers.Add(new ContentContainer(list, layout));
	}

	public static void GenerateImageContainers(Sprite sprite, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		CodexImage item = new CodexImage(128, 128, sprite);
		list.Add(item);
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
	}

	public static void CreateUnlockablesContentContainer(SubEntry subentry)
	{
		ContentContainer contentContainer = new ContentContainer(new List<ICodexWidget>
		{
			new CodexText(CODEX.HEADERS.SECTION_UNLOCKABLES, CodexTextStyle.Subtitle),
			new CodexDividerLine()
		}, ContentContainer.ContentLayout.Vertical);
		contentContainer.showBeforeGeneratedContent = false;
		subentry.lockedContentContainer = contentContainer;
	}

	private static void GenerateFabricatorContainers(GameObject entity, List<ContentContainer> containers)
	{
		ComplexFabricator component = entity.GetComponent<ComplexFabricator>();
		if (!(component == null))
		{
			List<ICodexWidget> list = new List<ICodexWidget>();
			list.Add(new CodexSpacer());
			list.Add(new CodexText(Strings.Get("STRINGS.CODEX.HEADERS.FABRICATIONS"), CodexTextStyle.Subtitle));
			list.Add(new CodexDividerLine());
			containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
			List<ICodexWidget> list2 = new List<ICodexWidget>();
			ComplexRecipe[] recipes = component.GetRecipes();
			foreach (ComplexRecipe recipe in recipes)
			{
				list2.Add(new CodexRecipePanel(recipe));
			}
			containers.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
		}
	}

	private static void GenerateConfigurableConsumerContainers(GameObject buildingComplete, List<ContentContainer> containers)
	{
		IConfigurableConsumer component = buildingComplete.GetComponent<IConfigurableConsumer>();
		if (component != null)
		{
			List<ICodexWidget> list = new List<ICodexWidget>();
			list.Add(new CodexSpacer());
			list.Add(new CodexText(Strings.Get("STRINGS.CODEX.HEADERS.FABRICATIONS"), CodexTextStyle.Subtitle));
			list.Add(new CodexDividerLine());
			containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
			List<ICodexWidget> list2 = new List<ICodexWidget>();
			IConfigurableConsumerOption[] settingOptions = component.GetSettingOptions();
			foreach (IConfigurableConsumerOption data in settingOptions)
			{
				list2.Add(new CodexConfigurableConsumerRecipePanel(data));
			}
			containers.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Vertical));
		}
	}

	private static void GenerateReceptacleContainers(GameObject entity, List<ContentContainer> containers)
	{
		SingleEntityReceptacle plot = entity.GetComponent<SingleEntityReceptacle>();
		if (plot == null)
		{
			return;
		}
		List<ICodexWidget> list = new List<ICodexWidget>();
		list.Add(new CodexText(Strings.Get("STRINGS.CODEX.HEADERS.RECEPTACLE"), CodexTextStyle.Subtitle));
		list.Add(new CodexDividerLine());
		containers.Add(new ContentContainer(list, ContentContainer.ContentLayout.Vertical));
		foreach (Tag possibleDepositObjectTag in plot.possibleDepositObjectTags)
		{
			List<GameObject> prefabsWithTag = Assets.GetPrefabsWithTag(possibleDepositObjectTag);
			if (plot.rotatable == null)
			{
				prefabsWithTag.RemoveAll(delegate(GameObject go)
				{
					IReceptacleDirection component = go.GetComponent<IReceptacleDirection>();
					return component != null && component.Direction != plot.Direction;
				});
			}
			foreach (GameObject item in prefabsWithTag)
			{
				List<ICodexWidget> list2 = new List<ICodexWidget>();
				list2.Add(new CodexImage(64, 64, Def.GetUISprite(item).first));
				list2.Add(new CodexText(item.GetProperName()));
				containers.Add(new ContentContainer(list2, ContentContainer.ContentLayout.Horizontal));
			}
		}
	}
}
