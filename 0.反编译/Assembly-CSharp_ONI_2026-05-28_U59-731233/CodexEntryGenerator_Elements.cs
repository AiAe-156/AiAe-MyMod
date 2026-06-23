using System;
using System.Collections.Generic;
using System.Linq;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class CodexEntryGenerator_Elements
{
	public class ConversionEntry
	{
		public string title;

		public GameObject prefab;

		public HashSet<ElementUsage> inSet = new HashSet<ElementUsage>();

		public HashSet<ElementUsage> outSet = new HashSet<ElementUsage>();

		public CodexConversionPanel.IconSettings aidIcon1;
	}

	public class CodexElementMap
	{
		public Dictionary<Tag, List<ConversionEntry>> map = new Dictionary<Tag, List<ConversionEntry>>();

		public void Add(Tag t, ConversionEntry ce)
		{
			if (map.TryGetValue(t, out var value))
			{
				value.Add(ce);
				return;
			}
			map[t] = new List<ConversionEntry> { ce };
		}
	}

	public class ElementEntryContext
	{
		public CodexElementMap madeMap = new CodexElementMap();

		public CodexElementMap usedMap = new CodexElementMap();
	}

	public static string ELEMENTS_ID = CodexCache.FormatLinkID("ELEMENTS");

	public static string ELEMENTS_SOLIDS_ID = CodexCache.FormatLinkID("ELEMENTS_SOLID");

	public static string ELEMENTS_LIQUIDS_ID = CodexCache.FormatLinkID("ELEMENTS_LIQUID");

	public static string ELEMENTS_GASES_ID = CodexCache.FormatLinkID("ELEMENTS_GAS");

	public static string ELEMENTS_OTHER_ID = CodexCache.FormatLinkID("ELEMENTS_OTHER");

	public static string ELEMENT_TYPES = CodexCache.FormatLinkID("ELEMENTTYPES");

	private static ElementEntryContext contextInstance;

	private static Tag WaterTag => ElementLoader.FindElementByHash(SimHashes.Water).tag;

	private static Tag DirtyWaterTag => ElementLoader.FindElementByHash(SimHashes.DirtyWater).tag;

	public static Dictionary<string, CodexEntry> GenerateEntries()
	{
		Dictionary<string, CodexEntry> entriesElements = new Dictionary<string, CodexEntry>();
		Dictionary<string, CodexEntry> dictionary = new Dictionary<string, CodexEntry>();
		Dictionary<string, CodexEntry> dictionary2 = new Dictionary<string, CodexEntry>();
		Dictionary<string, CodexEntry> dictionary3 = new Dictionary<string, CodexEntry>();
		Dictionary<string, CodexEntry> dictionary4 = new Dictionary<string, CodexEntry>();
		Dictionary<string, CodexEntry> entries = new Dictionary<string, CodexEntry>();
		AddCategoryEntry(ELEMENTS_SOLIDS_ID, UI.CODEX.CATEGORYNAMES.ELEMENTSSOLID, Assets.GetSprite("ui_elements-solid"), dictionary);
		AddCategoryEntry(ELEMENTS_LIQUIDS_ID, UI.CODEX.CATEGORYNAMES.ELEMENTSLIQUID, Assets.GetSprite("ui_elements-liquids"), dictionary2);
		AddCategoryEntry(ELEMENTS_GASES_ID, UI.CODEX.CATEGORYNAMES.ELEMENTSGAS, Assets.GetSprite("ui_elements-gases"), dictionary3);
		AddCategoryEntry(ELEMENTS_OTHER_ID, UI.CODEX.CATEGORYNAMES.ELEMENTSOTHER, Assets.GetSprite("ui_elements-other"), dictionary4);
		AddCategoryEntry(ELEMENT_TYPES, UI.CODEX.CATEGORYNAMES.ELEMENTTYPES, Assets.GetSprite("ui_element_poperties"), entries);
		foreach (Element element in ElementLoader.elements)
		{
			if (element.disabled)
			{
				continue;
			}
			bool flag = false;
			Tag[] oreTags = element.oreTags;
			foreach (Tag tag in oreTags)
			{
				if (tag == GameTags.HideFromCodex)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			Tuple<Sprite, Color> tuple = Def.GetUISprite(element);
			if (tuple.first == null)
			{
				if (element.id == SimHashes.Void)
				{
					tuple = new Tuple<Sprite, Color>(Assets.GetSprite("ui_elements-void"), Color.white);
				}
				else if (element.id == SimHashes.Vacuum)
				{
					tuple = new Tuple<Sprite, Color>(Assets.GetSprite("ui_elements-vacuum"), Color.white);
				}
			}
			List<ContentContainer> list = new List<ContentContainer>();
			CodexEntryGenerator.GenerateTitleContainers(element.name, list);
			CodexEntryGenerator.GenerateImageContainers(new Tuple<Sprite, Color>[1] { tuple }, list, ContentContainer.ContentLayout.Horizontal);
			GenerateElementDescriptionContainers(element, list);
			string text;
			Dictionary<string, CodexEntry> dictionary5;
			if (element.IsSolid)
			{
				text = ELEMENTS_SOLIDS_ID;
				dictionary5 = dictionary;
			}
			else if (element.IsLiquid)
			{
				text = ELEMENTS_LIQUIDS_ID;
				dictionary5 = dictionary2;
			}
			else if (element.IsGas)
			{
				text = ELEMENTS_GASES_ID;
				dictionary5 = dictionary3;
			}
			else
			{
				text = ELEMENTS_OTHER_ID;
				dictionary5 = dictionary4;
			}
			string text2 = element.id.ToString();
			CodexEntry codexEntry = new CodexEntry(text, list, element.name);
			codexEntry.parentId = text;
			codexEntry.icon = tuple.first;
			codexEntry.iconColor = tuple.second;
			CodexCache.AddEntry(text2, codexEntry);
			dictionary5.Add(text2, codexEntry);
		}
		string text3 = "IceBellyPoop";
		GameObject gameObject = Assets.TryGetPrefab(text3);
		if (gameObject != null)
		{
			string eLEMENTS_SOLIDS_ID = ELEMENTS_SOLIDS_ID;
			Dictionary<string, CodexEntry> dictionary6 = dictionary;
			KPrefabID component = gameObject.GetComponent<KPrefabID>();
			InfoDescription component2 = gameObject.GetComponent<InfoDescription>();
			string properName = gameObject.GetProperName();
			string description = component2.description;
			Tuple<Sprite, Color> uISprite = Def.GetUISprite(gameObject);
			List<ContentContainer> list2 = new List<ContentContainer>();
			CodexEntryGenerator.GenerateTitleContainers(properName, list2);
			CodexEntryGenerator.GenerateImageContainers(new Tuple<Sprite, Color>[1] { uISprite }, list2, ContentContainer.ContentLayout.Horizontal);
			GenerateMadeAndUsedContainers(component.PrefabTag, list2);
			list2.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(description),
				new CodexSpacer()
			}, ContentContainer.ContentLayout.Vertical));
			CodexEntry codexEntry2 = new CodexEntry(eLEMENTS_SOLIDS_ID, list2, properName);
			codexEntry2.parentId = eLEMENTS_SOLIDS_ID;
			codexEntry2.icon = uISprite.first;
			codexEntry2.iconColor = uISprite.second;
			CodexCache.AddEntry(text3, codexEntry2);
			dictionary6.Add(text3, codexEntry2);
		}
		CodexEntryGenerator.PopulateCategoryEntries(entriesElements);
		return entriesElements;
		void AddCategoryEntry(string categoryId, string name, Sprite icon, Dictionary<string, CodexEntry> entries2)
		{
			CodexEntry codexEntry3 = CodexEntryGenerator.GenerateCategoryEntry(categoryId, name, entries2, icon);
			codexEntry3.parentId = ELEMENTS_ID;
			codexEntry3.category = ELEMENTS_ID;
			entriesElements.Add(categoryId, codexEntry3);
		}
	}

	public static void GenerateElementDescriptionContainers(Element element, List<ContentContainer> containers)
	{
		List<ICodexWidget> list = new List<ICodexWidget>();
		List<ICodexWidget> list2 = new List<ICodexWidget>();
		if (element.sublimateId != 0 || element.HasTag(GameTags.Sublimating))
		{
			list.Add(new CodexTemperatureTransitionPanel(element, (element.offGasPercentage != 0f) ? CodexTemperatureTransitionPanel.TransitionType.OFFGASS : CodexTemperatureTransitionPanel.TransitionType.SUBLIMATE));
		}
		if (element.highTempTransition != null)
		{
			list.Add(new CodexTemperatureTransitionPanel(element, CodexTemperatureTransitionPanel.TransitionType.HEAT));
		}
		if (element.lowTempTransition != null)
		{
			list.Add(new CodexTemperatureTransitionPanel(element, CodexTemperatureTransitionPanel.TransitionType.COOL));
		}
		foreach (Element element2 in ElementLoader.elements)
		{
			if (element2.disabled)
			{
				continue;
			}
			if (element2.highTempTransition == element || ElementLoader.FindElementByHash(element2.highTempTransitionOreID) == element)
			{
				list2.Add(new CodexTemperatureTransitionPanel(element2, CodexTemperatureTransitionPanel.TransitionType.HEAT));
			}
			if (element2.lowTempTransition == element || ElementLoader.FindElementByHash(element2.lowTempTransitionOreID) == element)
			{
				list2.Add(new CodexTemperatureTransitionPanel(element2, CodexTemperatureTransitionPanel.TransitionType.COOL));
			}
			if (element2.sublimateId != element.id && !element2.HasTag(GameTags.Sublimating))
			{
				continue;
			}
			bool flag = element2.sublimateId == element.id;
			if (element2.sublimateId != element.id)
			{
				GameObject prefab = Assets.GetPrefab(element2.id.CreateTag());
				if (prefab != null)
				{
					Sublimates component = prefab.GetComponent<Sublimates>();
					flag = component != null && component.info.sublimatedElement == element.id;
				}
			}
			if (flag)
			{
				list2.Add(new CodexTemperatureTransitionPanel(element2, (element2.offGasPercentage != 0f) ? CodexTemperatureTransitionPanel.TransitionType.OFFGASS : CodexTemperatureTransitionPanel.TransitionType.SUBLIMATE));
			}
		}
		if (list.Count > 0)
		{
			ContentContainer contentContainer = new ContentContainer(list, ContentContainer.ContentLayout.Vertical);
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexCollapsibleHeader(CODEX.HEADERS.ELEMENTTRANSITIONSTO, contentContainer)
			}, ContentContainer.ContentLayout.Vertical));
			containers.Add(contentContainer);
		}
		if (list2.Count > 0)
		{
			ContentContainer contentContainer2 = new ContentContainer(list2, ContentContainer.ContentLayout.Vertical);
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexCollapsibleHeader(CODEX.HEADERS.ELEMENTTRANSITIONSFROM, contentContainer2)
			}, ContentContainer.ContentLayout.Vertical));
			containers.Add(contentContainer2);
		}
		GenerateMadeAndUsedContainers(element.tag, containers);
		containers.Add(new ContentContainer(new List<ICodexWidget>
		{
			new CodexSpacer(),
			new CodexText(element.FullDescription()),
			new CodexSpacer()
		}, ContentContainer.ContentLayout.Vertical));
	}

	public static void GenerateMadeAndUsedContainers(Tag tag, List<ContentContainer> containers)
	{
		List<ICodexWidget> used = new List<ICodexWidget>();
		List<ICodexWidget> made = new List<ICodexWidget>();
		foreach (ComplexRecipe recipe in ComplexRecipeManager.Get().recipes)
		{
			if (Game.IsCorrectDlcActiveForCurrentSave(recipe) && !recipe.IsAnyProductDeprecated())
			{
				if (recipe.ingredients.Any((ComplexRecipe.RecipeElement i) => i.material == tag))
				{
					used.Add(new CodexRecipePanel(recipe));
				}
				if (recipe.results.Any((ComplexRecipe.RecipeElement i) => i.material == tag))
				{
					made.Add(new CodexRecipePanel(recipe, shouldUseFabricatorForTitle: true));
				}
			}
		}
		if (GetElementEntryContext().usedMap.map.TryGetValue(tag, out var value))
		{
			foreach (ConversionEntry item in value)
			{
				used.Add(new CodexConversionPanel(item.title, item.inSet.ToArray(), item.outSet.ToArray(), item.prefab, item.aidIcon1));
			}
		}
		if (GetElementEntryContext().madeMap.map.TryGetValue(tag, out var value2))
		{
			foreach (ConversionEntry item2 in value2)
			{
				made.Add(new CodexConversionPanel(item2.title, item2.inSet.ToArray(), item2.outSet.ToArray(), item2.prefab, item2.aidIcon1));
			}
		}
		ManualCodexConversionRegistry.GetConversionsForGivenConverter(tag)?.ForEach(delegate(ManualCodexConversionRegistry.ManualConversionEntry ce)
		{
			used.Add(new CodexConversionPanel(ce.headerDescription, (ce.input == null) ? null : new ElementUsage[1]
			{
				new ElementUsage(ce.input.first, ce.input.second, continuous: false, ce.inputCustomFormating)
			}, (ce.output == null) ? null : new ElementUsage[1]
			{
				new ElementUsage(ce.output.first, ce.output.second, continuous: false, ce.outputCustomFormating)
			}, Assets.GetPrefab(tag)));
		});
		ManualCodexConversionRegistry.GetProducersForGivenOutput(tag)?.ForEach(delegate(ManualCodexConversionRegistry.ManualConversionEntry ce)
		{
			made.Add(new CodexConversionPanel(ce.headerDescription, (ce.input == null) ? null : new ElementUsage[1]
			{
				new ElementUsage(ce.input.first, ce.input.second, continuous: false, ce.inputCustomFormating)
			}, (ce.output == null) ? null : new ElementUsage[1]
			{
				new ElementUsage(ce.output.first, ce.output.second, continuous: false, ce.outputCustomFormating)
			}, Assets.GetPrefab(ce.converter.first)));
		});
		ManualCodexConversionRegistry.GetConsumersForGivenInput(tag)?.ForEach(delegate(ManualCodexConversionRegistry.ManualConversionEntry ce)
		{
			used.Add(new CodexConversionPanel(ce.headerDescription, (ce.input == null) ? null : new ElementUsage[1]
			{
				new ElementUsage(ce.input.first, ce.input.second, continuous: false, ce.inputCustomFormating)
			}, (ce.output == null) ? null : new ElementUsage[1]
			{
				new ElementUsage(ce.output.first, ce.output.second, continuous: false, ce.outputCustomFormating)
			}, Assets.GetPrefab(ce.converter.first)));
		});
		ContentContainer contentContainer = new ContentContainer(used, ContentContainer.ContentLayout.Vertical);
		ContentContainer contentContainer2 = new ContentContainer(made, ContentContainer.ContentLayout.Vertical);
		if (used.Count > 0)
		{
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexCollapsibleHeader(CODEX.HEADERS.ELEMENTCONSUMEDBY, contentContainer)
			}, ContentContainer.ContentLayout.Vertical));
			containers.Add(contentContainer);
		}
		if (made.Count > 0)
		{
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexCollapsibleHeader(CODEX.HEADERS.ELEMENTPRODUCEDBY, contentContainer2)
			}, ContentContainer.ContentLayout.Vertical));
			containers.Add(contentContainer2);
		}
	}

	private static void AddPlantFiberInfo(ref HashSet<ElementUsage> inSet, CodexElementMap usedMap, CodexElementMap madeMap, GameObject prefabOfProducer, GameObject prefabForPlayerFacing, Crop crop, Func<Tag, float, bool, string> customFormatting = null)
	{
		if (prefabOfProducer.TryGetComponent<PlantFiberProducer>(out var component))
		{
			ConversionEntry conversionEntry = new ConversionEntry();
			conversionEntry.title = prefabForPlayerFacing.GetProperName();
			conversionEntry.prefab = prefabForPlayerFacing;
			conversionEntry.inSet = inSet;
			conversionEntry.outSet.Add(new ElementUsage("PlantFiber", component.amount / crop.cropVal.cropDuration, continuous: true, customFormatting));
			conversionEntry.aidIcon1 = new CodexConversionPanel.IconSettings
			{
				spriteName = "skillbadge_role_farming3",
				tooltip = CODEX.MISC.TIP_ICON.FARMING3_SKILL.TOOLTIP,
				onClickActions = delegate
				{
					ManagementMenu.Instance.OpenSkills(null);
				}
			};
			usedMap.Add(prefabForPlayerFacing.PrefabID(), conversionEntry);
			madeMap.Add("PlantFiber", conversionEntry);
		}
	}

	private static void CheckPrefab(GameObject prefab, CodexElementMap usedMap, CodexElementMap madeMap)
	{
		HashSet<ElementUsage> inSet = new HashSet<ElementUsage>();
		HashSet<ElementUsage> outSet = new HashSet<ElementUsage>();
		List<ElementConverter> outputOnlyConverters = new List<ElementConverter>();
		List<ElementConverter> withInputsConverters = new List<ElementConverter>();
		List<ElementConverter> categoryConverters = new List<ElementConverter>();
		PartitionElementConverters(prefab, outputOnlyConverters, withInputsConverters, categoryConverters);
		CollectSharedConversionIO(prefab, inSet, outSet, outputOnlyConverters);
		RegisterConversionEntries(prefab, inSet, outSet, usedMap, madeMap, withInputsConverters);
		AddIndependentConversionEntries(prefab, inSet, usedMap, madeMap, categoryConverters);
	}

	private static void PartitionElementConverters(GameObject prefab, List<ElementConverter> outputOnlyConverters, List<ElementConverter> withInputsConverters, List<ElementConverter> categoryConverters)
	{
		IEnumerable<ElementConverter> components = prefab.GetComponents<ElementConverter>();
		foreach (ElementConverter item in components ?? Enumerable.Empty<ElementConverter>())
		{
			if (item.inputIsCategory)
			{
				categoryConverters.Add(item);
			}
			else if (item.consumedElements != null && item.consumedElements.Length != 0)
			{
				withInputsConverters.Add(item);
			}
			else
			{
				outputOnlyConverters.Add(item);
			}
		}
	}

	private static void VerifyClaimedByproduct(GameObject prefab, List<ElementConverter> withInputsConverters, List<ElementConverter> categoryConverters)
	{
		IConverterByproduct defImplementingInterface = prefab.GetDefImplementingInterface<IConverterByproduct>();
		if (defImplementingInterface == null || defImplementingInterface.ByproductRate <= 0f)
		{
			return;
		}
		bool flag = false;
		ElementUsage usage;
		foreach (ElementConverter withInputsConverter in withInputsConverters)
		{
			if (TryGetByproductUsage(defImplementingInterface, withInputsConverter, out usage))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			foreach (ElementConverter categoryConverter in categoryConverters)
			{
				if (TryGetByproductUsage(defImplementingInterface, categoryConverter, out usage))
				{
					flag = true;
					break;
				}
			}
		}
		DebugUtil.DevAssert(flag, "IConverterByproduct has no associated ElementConverter");
	}

	private static void RegisterConversionEntries(GameObject prefab, HashSet<ElementUsage> inSet, HashSet<ElementUsage> outSet, CodexElementMap usedMap, CodexElementMap madeMap, List<ElementConverter> withInputsConverters)
	{
		IrrigationMonitor.Def def = prefab.GetDef<IrrigationMonitor.Def>();
		IConverterByproduct defImplementingInterface = prefab.GetDefImplementingInterface<IConverterByproduct>();
		if (withInputsConverters.Count == 0)
		{
			RegisterIrrigationOrSingleEntry(prefab, inSet, outSet, usedMap, madeMap, def);
			return;
		}
		foreach (ElementConverter withInputsConverter in withInputsConverters)
		{
			HashSet<ElementUsage> hashSet = new HashSet<ElementUsage>(inSet);
			ElementConverter.ConsumedElement[] consumedElements = withInputsConverter.consumedElements;
			for (int i = 0; i < consumedElements.Length; i++)
			{
				ElementConverter.ConsumedElement consumedElement = consumedElements[i];
				hashSet.Add(new ElementUsage(consumedElement.Tag, consumedElement.MassConsumptionRate, continuous: true));
			}
			HashSet<ElementUsage> hashSet2 = new HashSet<ElementUsage>(outSet);
			IEnumerable<ElementConverter.OutputElement> outputElements = withInputsConverter.outputElements;
			foreach (ElementConverter.OutputElement item in outputElements ?? Enumerable.Empty<ElementConverter.OutputElement>())
			{
				Tag tag = ElementLoader.FindElementByHash(item.elementHash).tag;
				hashSet2.Add(new ElementUsage(tag, item.massGenerationRate, continuous: true));
			}
			if (TryGetByproductUsage(defImplementingInterface, withInputsConverter, out var usage))
			{
				hashSet2.Add(usage);
			}
			RegisterIrrigationOrSingleEntry(prefab, hashSet, hashSet2, usedMap, madeMap, def);
		}
	}

	private static void RegisterIrrigationOrSingleEntry(GameObject prefab, HashSet<ElementUsage> inSet, HashSet<ElementUsage> outSet, CodexElementMap usedMap, CodexElementMap madeMap, IrrigationMonitor.Def irrigation)
	{
		if (irrigation != null)
		{
			PlantElementAbsorber.ConsumeInfo[] consumedElements = irrigation.consumedElements;
			for (int i = 0; i < consumedElements.Length; i++)
			{
				PlantElementAbsorber.ConsumeInfo consumeInfo = consumedElements[i];
				HashSet<ElementUsage> hashSet = new HashSet<ElementUsage>(inSet);
				hashSet.Add(new ElementUsage(consumeInfo.tag, consumeInfo.massConsumptionRate, continuous: true));
				RegisterSingleEntry(prefab, hashSet, outSet, usedMap, madeMap);
			}
		}
		else
		{
			RegisterSingleEntry(prefab, inSet, outSet, usedMap, madeMap);
		}
	}

	private static void RegisterSingleEntry(GameObject prefab, HashSet<ElementUsage> inSet, HashSet<ElementUsage> outSet, CodexElementMap usedMap, CodexElementMap madeMap)
	{
		ConversionEntry conversionEntry = new ConversionEntry();
		conversionEntry.title = prefab.GetProperName();
		conversionEntry.prefab = prefab;
		conversionEntry.inSet = inSet;
		conversionEntry.outSet = outSet;
		if (inSet.Count > 0 && outSet.Count > 0)
		{
			usedMap.Add(prefab.PrefabID(), conversionEntry);
		}
		foreach (ElementUsage item in inSet)
		{
			usedMap.Add(item.tag, conversionEntry);
		}
		foreach (ElementUsage item2 in outSet)
		{
			madeMap.Add(item2.tag, conversionEntry);
		}
		Crop component = prefab.GetComponent<Crop>();
		if (component != null && prefab.GetComponent<IPlantConsumeEntities>() == null)
		{
			AddPlantFiberInfo(ref inSet, usedMap, madeMap, prefab, prefab, component);
		}
	}

	private static bool TryGetByproductUsage(IConverterByproduct byproduct, ElementConverter conv, out ElementUsage usage)
	{
		usage = null;
		if (byproduct == null || byproduct.ByproductRate <= 0f)
		{
			return false;
		}
		IEnumerable<ElementConverter.ConsumedElement> consumedElements = conv.consumedElements;
		foreach (ElementConverter.ConsumedElement item in consumedElements ?? Enumerable.Empty<ElementConverter.ConsumedElement>())
		{
			if (item.Tag == byproduct.ByproductAssociatedInputTag)
			{
				usage = new ElementUsage(byproduct.ByproductTag, byproduct.ByproductRate, byproduct.ByproductIsContinuous);
				return true;
			}
		}
		return false;
	}

	private static void CollectSharedConversionIO(GameObject prefab, HashSet<ElementUsage> inSet, HashSet<ElementUsage> outSet, List<ElementConverter> outputOnlyConverters)
	{
		EnergyGenerator component = prefab.GetComponent<EnergyGenerator>();
		if ((bool)component)
		{
			IEnumerable<EnergyGenerator.InputItem> inputs = component.formula.inputs;
			foreach (EnergyGenerator.InputItem item in inputs ?? Enumerable.Empty<EnergyGenerator.InputItem>())
			{
				inSet.Add(new ElementUsage(item.tag, item.consumptionRate, continuous: true));
			}
			IEnumerable<EnergyGenerator.OutputItem> outputs = component.formula.outputs;
			foreach (EnergyGenerator.OutputItem item2 in outputs ?? Enumerable.Empty<EnergyGenerator.OutputItem>())
			{
				Tag tag = ElementLoader.FindElementByHash(item2.element).tag;
				outSet.Add(new ElementUsage(tag, item2.creationRate, continuous: true));
			}
		}
		foreach (ElementConverter outputOnlyConverter in outputOnlyConverters)
		{
			IEnumerable<ElementConverter.OutputElement> outputElements = outputOnlyConverter.outputElements;
			foreach (ElementConverter.OutputElement item3 in outputElements ?? Enumerable.Empty<ElementConverter.OutputElement>())
			{
				Tag tag2 = ElementLoader.FindElementByHash(item3.elementHash).tag;
				outSet.Add(new ElementUsage(tag2, item3.massGenerationRate, continuous: true));
			}
		}
		ElementConsumer[] components = prefab.GetComponents<ElementConsumer>();
		IEnumerable<ElementConsumer> enumerable = components;
		foreach (ElementConsumer item4 in enumerable ?? Enumerable.Empty<ElementConsumer>())
		{
			if (!item4.storeOnConsume)
			{
				Tag tag3 = ElementLoader.FindElementByHash(item4.elementToConsume).tag;
				inSet.Add(new ElementUsage(tag3, item4.consumptionRate, continuous: true));
			}
		}
		FertilizationMonitor.Def def = prefab.GetDef<FertilizationMonitor.Def>();
		if (def != null)
		{
			PlantElementAbsorber.ConsumeInfo[] consumedElements = def.consumedElements;
			for (int i = 0; i < consumedElements.Length; i++)
			{
				PlantElementAbsorber.ConsumeInfo consumeInfo = consumedElements[i];
				inSet.Add(new ElementUsage(consumeInfo.tag, consumeInfo.massConsumptionRate, continuous: true));
			}
		}
		Crop component2 = prefab.GetComponent<Crop>();
		if (component2 != null && prefab.GetComponent<IPlantConsumeEntities>() == null)
		{
			outSet.Add(new ElementUsage(component2.cropId, (float)component2.cropVal.numProduced / component2.cropVal.cropDuration, continuous: true));
		}
		FlushToilet component3 = prefab.GetComponent<FlushToilet>();
		if ((bool)component3)
		{
			inSet.Add(new ElementUsage(WaterTag, component3.massConsumedPerUse, continuous: false));
			outSet.Add(new ElementUsage(DirtyWaterTag, component3.massEmittedPerUse, continuous: false));
		}
		HandSanitizer component4 = prefab.GetComponent<HandSanitizer>();
		if ((bool)component4)
		{
			Tag tag4 = ElementLoader.FindElementByHash(component4.consumedElement).tag;
			inSet.Add(new ElementUsage(tag4, component4.massConsumedPerUse, continuous: false));
			if (component4.outputElement != SimHashes.Vacuum)
			{
				Tag tag5 = ElementLoader.FindElementByHash(component4.outputElement).tag;
				outSet.Add(new ElementUsage(tag5, component4.massConsumedPerUse, continuous: false));
			}
		}
	}

	private static void AddIndependentConversionEntries(GameObject prefab, HashSet<ElementUsage> inSet, CodexElementMap usedMap, CodexElementMap madeMap, List<ElementConverter> categoryConverters)
	{
		Crop component = prefab.GetComponent<Crop>();
		IPlantConsumeEntities component2 = prefab.GetComponent<IPlantConsumeEntities>();
		foreach (ElementConverter categoryConverter in categoryConverters)
		{
			List<ConversionEntry> list = new List<ConversionEntry>();
			IEnumerable<ElementConverter.ConsumedElement> consumedElements = categoryConverter.consumedElements;
			foreach (ElementConverter.ConsumedElement c in consumedElements ?? Enumerable.Empty<ElementConverter.ConsumedElement>())
			{
				List<Element> list2 = ElementLoader.FindElements((Element e) => e.HasTag(c.Tag));
				foreach (Element item4 in list2)
				{
					ConversionEntry conversionEntry = new ConversionEntry();
					conversionEntry.title = prefab.GetProperName();
					conversionEntry.prefab = prefab;
					conversionEntry.inSet.Add(new ElementUsage(item4.tag, c.MassConsumptionRate, continuous: true));
					list.Add(conversionEntry);
				}
			}
			IEnumerable<ElementConverter.OutputElement> outputElements = categoryConverter.outputElements;
			foreach (ElementConverter.OutputElement item5 in outputElements ?? Enumerable.Empty<ElementConverter.OutputElement>())
			{
				Tag tag = ElementLoader.FindElementByHash(item5.elementHash).tag;
				ElementUsage item = new ElementUsage(tag, item5.massGenerationRate, continuous: true);
				foreach (ConversionEntry item6 in list)
				{
					item6.outSet.Add(item);
				}
			}
			IConverterByproduct defImplementingInterface = prefab.GetDefImplementingInterface<IConverterByproduct>();
			if (TryGetByproductUsage(defImplementingInterface, categoryConverter, out var usage))
			{
				foreach (ConversionEntry item7 in list)
				{
					item7.outSet.Add(usage);
				}
			}
			foreach (ConversionEntry item8 in list)
			{
				if (item8.inSet.Count > 0 && item8.outSet.Count > 0)
				{
					usedMap.Add(prefab.PrefabID(), item8);
				}
				foreach (ElementUsage item9 in item8.inSet)
				{
					usedMap.Add(item9.tag, item8);
				}
				foreach (ElementUsage item10 in item8.outSet)
				{
					madeMap.Add(item10.tag, item8);
				}
			}
		}
		IPlantBranchGrower defImplementingInterface2 = prefab.GetDefImplementingInterface<IPlantBranchGrower>();
		if (defImplementingInterface2 != null)
		{
			GameObject prefab2 = Assets.GetPrefab(defImplementingInterface2.GetPlantBranchPrefabName());
			if (prefab2 != null)
			{
				Crop component3 = prefab2.GetComponent<Crop>();
				if (component3 != null && (component == null || component3.cropId != component.cropId || component3.cropVal.numProduced != component.cropVal.numProduced))
				{
					ConversionEntry conversionEntry2 = new ConversionEntry();
					conversionEntry2.title = prefab2.GetProperName();
					conversionEntry2.prefab = prefab;
					usedMap.Add(prefab.PrefabID(), conversionEntry2);
					conversionEntry2.inSet = new HashSet<ElementUsage>();
					IrrigationMonitor.Def def = prefab.GetDef<IrrigationMonitor.Def>();
					if (def != null)
					{
						PlantElementAbsorber.ConsumeInfo[] consumedElements2 = def.consumedElements;
						for (int num = 0; num < consumedElements2.Length; num++)
						{
							PlantElementAbsorber.ConsumeInfo consumeInfo = consumedElements2[num];
							conversionEntry2.inSet.Add(new ElementUsage(consumeInfo.tag, consumeInfo.massConsumptionRate, continuous: true));
						}
					}
					FertilizationMonitor.Def def2 = prefab.GetDef<FertilizationMonitor.Def>();
					if (def2 != null)
					{
						PlantElementAbsorber.ConsumeInfo[] consumedElements3 = def2.consumedElements;
						for (int num2 = 0; num2 < consumedElements3.Length; num2++)
						{
							PlantElementAbsorber.ConsumeInfo consumeInfo2 = consumedElements3[num2];
							conversionEntry2.inSet.Add(new ElementUsage(consumeInfo2.tag, consumeInfo2.massConsumptionRate, continuous: true));
						}
					}
					conversionEntry2.outSet = new HashSet<ElementUsage>();
					int branchCount = defImplementingInterface2.GetMaxBranchCount();
					conversionEntry2.outSet.Add(new ElementUsage(component3.cropId, (float)component3.cropVal.numProduced / component3.cropVal.cropDuration, continuous: true, (Tag t, float a, bool b) => GameUtil.GetFormattedBranchGrowerPlantProductionValuePerCycle(t, a, branchCount)));
					madeMap.Add(component3.cropId, conversionEntry2);
					AddPlantFiberInfo(ref inSet, usedMap, madeMap, prefab2, prefab, component3, (Tag t, float a, bool b) => GameUtil.GetFormattedBranchGrowerPlantPlantFiberProductionValuePerCycle(t, a, branchCount));
				}
			}
		}
		if (component2 != null)
		{
			List<KPrefabID> prefabsOfPossiblePrey = component2.GetPrefabsOfPossiblePrey();
			List<string> list3 = new List<string>();
			foreach (KPrefabID item11 in prefabsOfPossiblePrey)
			{
				CreatureBrain component4 = item11.GetComponent<CreatureBrain>();
				Tag tag2 = ((component4 == null) ? item11.PrefabID() : component4.species);
				string text = tag2.ProperName();
				if (!list3.Contains(text))
				{
					ConversionEntry conversionEntry3 = new ConversionEntry();
					conversionEntry3.title = component2.GetConsumableEntitiesCategoryName() + ": " + text;
					conversionEntry3.prefab = prefab;
					conversionEntry3.inSet.Add(new ElementUsage(tag2, (component == null) ? 1f : (1f / component.cropVal.cropDuration), component != null, (Tag t, float units, bool flag) => GameUtil.GetFormattedUnits(units, flag ? GameUtil.TimeSlice.PerCycle : GameUtil.TimeSlice.None)));
					if (component != null)
					{
						conversionEntry3.outSet.Add(new ElementUsage(component.cropId, (float)component.cropVal.numProduced / component.cropVal.cropDuration, continuous: true));
						madeMap.Add(component.cropId, conversionEntry3);
					}
					usedMap.Add(prefab.PrefabID(), conversionEntry3);
					list3.Add(text);
				}
			}
		}
		ScaleGrowthMonitor.Def def3 = prefab.GetDef<ScaleGrowthMonitor.Def>();
		if (def3 != null)
		{
			ConversionEntry conversionEntry4 = new ConversionEntry();
			GameObject prefab3 = Assets.GetPrefab("ShearingStation");
			conversionEntry4.title = prefab3.GetProperName();
			conversionEntry4.prefab = prefab3;
			conversionEntry4.inSet = new HashSet<ElementUsage>();
			conversionEntry4.inSet.Add(new ElementUsage(prefab.PrefabID(), 1f, continuous: false));
			usedMap.Add(prefab.PrefabID(), conversionEntry4);
			usedMap.Add(prefab3.PrefabID(), conversionEntry4);
			conversionEntry4.outSet = new HashSet<ElementUsage>();
			conversionEntry4.outSet.Add(new ElementUsage(def3.itemDroppedOnShear, def3.dropMass, continuous: false));
			madeMap.Add(def3.itemDroppedOnShear, conversionEntry4);
		}
		WellFedShearable.Def def4 = prefab.GetDef<WellFedShearable.Def>();
		if (def4 != null)
		{
			ConversionEntry conversionEntry5 = new ConversionEntry();
			GameObject prefab4 = Assets.GetPrefab("ShearingStation");
			conversionEntry5.title = prefab4.GetProperName();
			conversionEntry5.prefab = prefab4;
			conversionEntry5.inSet = new HashSet<ElementUsage>();
			conversionEntry5.inSet.Add(new ElementUsage(prefab.PrefabID(), 1f, continuous: false));
			usedMap.Add(prefab.PrefabID(), conversionEntry5);
			usedMap.Add(prefab4.PrefabID(), conversionEntry5);
			conversionEntry5.outSet = new HashSet<ElementUsage>();
			conversionEntry5.outSet.Add(new ElementUsage(def4.itemDroppedOnShear, def4.dropMass, continuous: false));
			madeMap.Add(def4.itemDroppedOnShear, conversionEntry5);
		}
		FertilityShearable.Def def5 = prefab.GetDef<FertilityShearable.Def>();
		if (def5 != null)
		{
			ConversionEntry conversionEntry6 = new ConversionEntry();
			GameObject prefab5 = Assets.GetPrefab("UnderwaterMilkingStation");
			conversionEntry6.title = prefab5.GetProperName();
			conversionEntry6.prefab = prefab5;
			conversionEntry6.inSet = new HashSet<ElementUsage>
			{
				new ElementUsage(prefab.PrefabID(), 1f, continuous: false)
			};
			usedMap.Add(prefab.PrefabID(), conversionEntry6);
			usedMap.Add(prefab5.PrefabID(), conversionEntry6);
			Tag tag3 = def5.milkElement.CreateTag();
			conversionEntry6.outSet = new HashSet<ElementUsage>
			{
				new ElementUsage(tag3, def5.dropMass, continuous: false)
			};
			madeMap.Add(tag3, conversionEntry6);
		}
		MilkProductionMonitor.Def def6 = prefab.GetDef<MilkProductionMonitor.Def>();
		if (def6 != null)
		{
			string text2 = (prefab.GetComponent<KPrefabID>().HasTag(GameTags.SwimmingCreature) ? "UnderwaterMilkingStation" : "MilkingStation");
			ConversionEntry conversionEntry7 = new ConversionEntry();
			GameObject prefab6 = Assets.GetPrefab(text2);
			conversionEntry7.title = prefab6.GetProperName();
			conversionEntry7.prefab = prefab6;
			conversionEntry7.inSet = new HashSet<ElementUsage>();
			conversionEntry7.inSet.Add(new ElementUsage(prefab.PrefabID(), 1f, continuous: false));
			usedMap.Add(prefab.PrefabID(), conversionEntry7);
			usedMap.Add(prefab6.PrefabID(), conversionEntry7);
			conversionEntry7.outSet = new HashSet<ElementUsage>();
			conversionEntry7.outSet.Add(new ElementUsage(def6.element.CreateTag(), def6.Capacity, continuous: false));
			madeMap.Add(def6.element.CreateTag(), conversionEntry7);
		}
		MoistureMonitor.Def def7 = prefab.GetDef<MoistureMonitor.Def>();
		if (def7 != null)
		{
			string title = CODEX.HEADERS.SECRETED.Replace("{Creature}", prefab.GetProperName());
			ConversionEntry conversionEntry8 = SimpleConversionBase(usedMap, prefab, title);
			string id = Db.Get().Amounts.Mucus.deltaAttribute.Id;
			float num3 = 0f;
			Trait trait = Db.Get().traits.Get(prefab.GetComponent<Modifiers>().initialTraits[0]);
			foreach (AttributeModifier selfModifier in trait.SelfModifiers)
			{
				if (selfModifier.AttributeId == id)
				{
					num3 = selfModifier.Value;
					break;
				}
			}
			float amount = num3 + def7.GetMaxModification();
			ElementUsage item2 = new ElementUsage(def7.lubricant.CreateTag(), amount, continuous: true)
			{
				customFormating = (Tag tag4, float mass, bool continous) => string.Format(CODEX.FORMAT_STRINGS.SECRETED, GameUtil.GetFormattedMass(mass, GameUtil.TimeSlice.PerCycle))
			};
			conversionEntry8.outSet.Add(item2);
			madeMap.Add(def7.lubricant.CreateTag(), conversionEntry8);
		}
		MoltDropperMonitor.Def def8 = prefab.GetDef<MoltDropperMonitor.Def>();
		if (def8 != null)
		{
			ConversionEntry conversionEntry9 = SimpleConversionBase(usedMap, prefab, CODEX.HEADERS.MOLTED.Replace("{Creature}", prefab.GetProperName()));
			ElementUsage item3 = new ElementUsage(def8.onGrowDropID, def8.massToDrop / 600f, continuous: true)
			{
				customFormating = (Tag tag4, float mass, bool continous) => CODEX.FORMAT_STRINGS.MOLTED.Replace("{Amount}", GameUtil.GetFormattedMass(mass, GameUtil.TimeSlice.PerCycle))
			};
			conversionEntry9.outSet.Add(item3);
			madeMap.Add(def8.onGrowDropID, conversionEntry9);
		}
		Butcherable component5 = prefab.GetComponent<Butcherable>();
		if (!(component5 != null))
		{
			return;
		}
		ConversionEntry conversionEntry10 = new ConversionEntry();
		conversionEntry10.title = prefab.GetProperName();
		conversionEntry10.prefab = prefab;
		usedMap.Add(prefab.PrefabID(), conversionEntry10);
		conversionEntry10.outSet = new HashSet<ElementUsage>();
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		foreach (KeyValuePair<string, float> drop in component5.drops)
		{
			dictionary.TryGetValue(drop.Key, out var value);
			dictionary[drop.Key] = value + Assets.GetPrefab(drop.Key).GetComponent<PrimaryElement>().Mass * drop.Value;
		}
		foreach (var (text4, amount2) in dictionary)
		{
			conversionEntry10.outSet.Add(new ElementUsage(text4, amount2, continuous: false));
			madeMap.Add(text4, conversionEntry10);
		}
	}

	private static void AddDietConversions(GameObject prefab, CodexElementMap usedMap, CodexElementMap madeMap)
	{
		Diet diet = null;
		CreatureCalorieMonitor.Def def = prefab.GetDef<CreatureCalorieMonitor.Def>();
		if (def != null)
		{
			diet = def.diet;
		}
		else
		{
			BeehiveCalorieMonitor.Def def2 = prefab.GetDef<BeehiveCalorieMonitor.Def>();
			if (def2 != null)
			{
				diet = def2.diet;
			}
		}
		if (diet == null)
		{
			return;
		}
		float num = 0f;
		Trait trait = Db.Get().traits.Get(prefab.GetComponent<Modifiers>().initialTraits[0]);
		foreach (AttributeModifier selfModifier in trait.SelfModifiers)
		{
			if (selfModifier.AttributeId == Db.Get().Amounts.Calories.deltaAttribute.Id)
			{
				num = selfModifier.Value;
			}
		}
		Diet.Info[] infos = diet.infos;
		foreach (Diet.Info info in infos)
		{
			foreach (Tag consumedTag in info.consumedTags)
			{
				float num2 = (0f - num) / info.caloriesPerKg;
				float amount = num2 * info.producedConversionRate;
				bool flag = diet.IsConsumedTagAbleToBeEatenDirectly(consumedTag);
				ElementUsage item = null;
				if (flag)
				{
					if (info.foodType == Diet.Info.FoodType.EatPlantDirectly)
					{
						item = new ElementUsage(consumedTag, num2, continuous: true, GameUtil.GetFormattedDirectPlantConsumptionValuePerCycle);
					}
					else if (info.foodType == Diet.Info.FoodType.EatPlantStorage)
					{
						item = new ElementUsage(consumedTag, num2, continuous: true, GameUtil.GetFormattedPlantStorageConsumptionValuePerCycle);
					}
					else if (info.foodType == Diet.Info.FoodType.EatPrey || info.foodType == Diet.Info.FoodType.EatButcheredPrey)
					{
						float num3 = diet.AvailableCaloriesInPrey(consumedTag);
						num2 = (0f - num) / num3;
						amount = num2 * info.producedConversionRate * num3 / info.caloriesPerKg;
						item = new ElementUsage(consumedTag, num2, continuous: true, GameUtil.GetFormattedPreyConsumptionValuePerCycle);
					}
				}
				else
				{
					item = new ElementUsage(consumedTag, num2, continuous: true);
				}
				ConversionEntry conversionEntry = new ConversionEntry();
				conversionEntry.title = prefab.GetProperName();
				conversionEntry.prefab = prefab;
				conversionEntry.inSet.Add(item);
				conversionEntry.outSet.Add(new ElementUsage(info.producedElement, amount, continuous: true));
				usedMap.Add(consumedTag, conversionEntry);
				madeMap.Add(info.producedElement, conversionEntry);
			}
		}
	}

	public static ElementEntryContext GetElementEntryContext()
	{
		if (contextInstance != null)
		{
			return contextInstance;
		}
		CodexElementMap usedMap = new CodexElementMap();
		CodexElementMap madeMap = new CodexElementMap();
		foreach (PlanScreen.PlanInfo item in TUNING.BUILDINGS.PLANORDER)
		{
			foreach (KeyValuePair<string, string> buildingAndSubcategoryDatum in item.buildingAndSubcategoryData)
			{
				BuildingDef buildingDef = Assets.GetBuildingDef(buildingAndSubcategoryDatum.Key);
				if (buildingDef == null)
				{
					Debug.LogError("Building def for id " + buildingAndSubcategoryDatum.Key + " is null");
				}
				if (!buildingDef.Deprecated && !buildingDef.BuildingComplete.HasTag(GameTags.DevBuilding))
				{
					CheckPrefab(buildingDef.BuildingComplete, usedMap, madeMap);
				}
			}
		}
		HashSet<GameObject> hashSet = new HashSet<GameObject>(Assets.GetPrefabsWithComponent<Harvestable>());
		foreach (GameObject item2 in Assets.GetPrefabsWithComponent<WiltCondition>())
		{
			hashSet.Add(item2);
		}
		foreach (GameObject item3 in hashSet)
		{
			if (!item3.HasTag(GameTags.HideFromCodex))
			{
				CheckPrefab(item3, usedMap, madeMap);
			}
		}
		List<GameObject> prefabsWithComponent = Assets.GetPrefabsWithComponent<CreatureBrain>();
		foreach (GameObject item4 in prefabsWithComponent)
		{
			if (item4.GetDef<BabyMonitor.Def>() == null)
			{
				CheckPrefab(item4, usedMap, madeMap);
			}
		}
		foreach (GameObject item5 in prefabsWithComponent)
		{
			if (item5.GetDef<BabyMonitor.Def>() == null)
			{
				AddDietConversions(item5, usedMap, madeMap);
			}
		}
		contextInstance = new ElementEntryContext
		{
			usedMap = usedMap,
			madeMap = madeMap
		};
		return contextInstance;
	}

	private static ConversionEntry SimpleConversionBase(CodexElementMap usedMap, GameObject prefab, string title = null)
	{
		ConversionEntry conversionEntry = new ConversionEntry
		{
			title = ((title == null) ? prefab.GetProperName() : title),
			prefab = prefab,
			inSet = new HashSet<ElementUsage>()
		};
		usedMap.Add(prefab.PrefabID(), conversionEntry);
		return conversionEntry;
	}
}
