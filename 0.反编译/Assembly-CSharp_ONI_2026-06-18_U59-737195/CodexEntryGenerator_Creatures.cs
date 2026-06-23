using System.Collections.Generic;
using System.Linq;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class CodexEntryGenerator_Creatures
{
	public const string CATEGORY_ID = "CREATURES";

	public const string GUIDE_ID = "CREATURES::GUIDE";

	public const string GUIDE_METABOLISM_ID = "CREATURES::GUIDE::METABOLISM";

	public const string GUIDE_MOOD_ID = "CREATURES::GUIDE::MOOD";

	public const string GUIDE_FERTILITY_ID = "CREATURES::GUIDE::FERTILITY";

	public const string GUIDE_DOMESTICATION_ID = "CREATURES::GUIDE::DOMESTICATION";

	public static Dictionary<string, CodexEntry> GenerateEntries()
	{
		Dictionary<string, CodexEntry> results = new Dictionary<string, CodexEntry>();
		List<GameObject> brains = Assets.GetPrefabsWithComponent<CreatureBrain>();
		List<(string, CodexEntry)> critterEntries = new List<(string, CodexEntry)>();
		AddEntry("CREATURES::GUIDE", GenerateFieldGuideEntry());
		Tag[] array = GameTags.Creatures.Species.AllSpecies_REFLECTION();
		for (int i = 0; i < array.Length; i++)
		{
			PushCritterEntry(array[i]);
		}
		PopAndAddAllCritterEntries();
		return results;
		void AddEntry(string entryId, CodexEntry entry, string parentEntryId = "CREATURES")
		{
			if (entry != null)
			{
				entry.parentId = parentEntryId;
				CodexCache.AddEntry(entryId, entry);
				results.Add(entryId, entry);
			}
		}
		void PopAndAddAllCritterEntries()
		{
			foreach (var (entryId, entry) in critterEntries.StableSort(((string, CodexEntry) pair) => UI.StripLinkFormatting(pair.Item2.name)))
			{
				AddEntry(entryId, entry);
			}
		}
		void PushCritterEntry(Tag speciesTag)
		{
			CodexEntry codexEntry = GenerateCritterEntry(speciesTag, speciesTag.ProperName(), brains);
			if (codexEntry != null)
			{
				critterEntries.Add((speciesTag.ToString(), codexEntry));
			}
		}
	}

	private static CodexEntry GenerateFieldGuideEntry()
	{
		CodexEntry generalInfoEntry = new CodexEntry("CREATURES", new List<ContentContainer>(), CODEX.CRITTERSTATUS.CRITTERSTATUS_TITLE);
		generalInfoEntry.icon = Assets.GetSprite("codex_critter_emotions");
		List<ICodexWidget> subEntryContents = null;
		SubEntry subEntry = null;
		AddSubEntry("CREATURES::GUIDE::METABOLISM", CODEX.CRITTERSTATUS.METABOLISM.TITLE);
		AddImage(Assets.GetSprite("codex_metabolism"));
		AddBody(CODEX.CRITTERSTATUS.METABOLISM.BODY.CONTAINER1);
		AddSubtitle(CODEX.CRITTERSTATUS.METABOLISM.HUNGRY.TITLE);
		AddBody(CODEX.CRITTERSTATUS.METABOLISM.HUNGRY.CONTAINER1);
		AddSubtitle(CODEX.CRITTERSTATUS.METABOLISM.STARVING.TITLE);
		AddBody(string.Format(DlcManager.IsExpansion1Active() ? CODEX.CRITTERSTATUS.METABOLISM.STARVING.CONTAINER1_DLC1 : CODEX.CRITTERSTATUS.METABOLISM.STARVING.CONTAINER1_VANILLA, 10));
		AddSpacer();
		AddSubEntry("CREATURES::GUIDE::MOOD", CODEX.CRITTERSTATUS.MOOD.TITLE);
		AddImage(Assets.GetSprite("codex_mood"));
		AddBody(CODEX.CRITTERSTATUS.MOOD.BODY.CONTAINER1);
		AddSubtitle(CODEX.CRITTERSTATUS.MOOD.HAPPY.TITLE);
		AddBody(CODEX.CRITTERSTATUS.MOOD.HAPPY.CONTAINER1);
		AddBody(CODEX.CRITTERSTATUS.MOOD.HAPPY.SUBTITLE);
		AddBulletPoint(CODEX.CRITTERSTATUS.MOOD.HAPPY.HAPPY_METABOLISM);
		AddSubtitle(CODEX.CRITTERSTATUS.MOOD.NEUTRAL.TITLE);
		AddBody(CODEX.CRITTERSTATUS.MOOD.NEUTRAL.CONTAINER1);
		AddSubtitle(CODEX.CRITTERSTATUS.MOOD.GLUM.TITLE);
		AddBody(CODEX.CRITTERSTATUS.MOOD.GLUM.CONTAINER1);
		AddBody(CODEX.CRITTERSTATUS.MOOD.GLUM.SUBTITLE);
		AddBulletPoint(CODEX.CRITTERSTATUS.MOOD.GLUM.GLUMWILD_METABOLISM);
		AddSubtitle(CODEX.CRITTERSTATUS.MOOD.MISERABLE.TITLE);
		AddBody(CODEX.CRITTERSTATUS.MOOD.MISERABLE.CONTAINER1);
		AddBody(CODEX.CRITTERSTATUS.MOOD.MISERABLE.SUBTITLE);
		AddBulletPoint(CODEX.CRITTERSTATUS.MOOD.MISERABLE.MISERABLEWILD_METABOLISM);
		AddBulletPoint(CODEX.CRITTERSTATUS.MOOD.MISERABLE.MISERABLEWILD_FERTILITY);
		AddSubtitle(CODEX.CRITTERSTATUS.MOOD.HOSTILE.TITLE);
		AddBody(DlcManager.IsExpansion1Active() ? CODEX.CRITTERSTATUS.MOOD.HOSTILE.CONTAINER1_DLC1 : CODEX.CRITTERSTATUS.MOOD.HOSTILE.CONTAINER1_VANILLA);
		AddSubtitle(CODEX.CRITTERSTATUS.MOOD.CONFINED.TITLE);
		AddBody(CODEX.CRITTERSTATUS.MOOD.CONFINED.CONTAINER1);
		AddBody(CODEX.CRITTERSTATUS.MOOD.CONFINED.SUBTITLE);
		AddBulletPoint(CODEX.CRITTERSTATUS.MOOD.CONFINED.CONFINED_FERTILITY);
		AddBulletPoint(CODEX.CRITTERSTATUS.MOOD.CONFINED.CONFINED_HAPPINESS);
		AddSubtitle(CODEX.CRITTERSTATUS.MOOD.OVERCROWDED.TITLE);
		AddBody(CODEX.CRITTERSTATUS.MOOD.OVERCROWDED.CONTAINER1);
		AddBody(CODEX.CRITTERSTATUS.MOOD.OVERCROWDED.SUBTITLE);
		AddBulletPoint(CODEX.CRITTERSTATUS.MOOD.OVERCROWDED.OVERCROWDED_HAPPY1);
		AddSpacer();
		AddSubEntry("CREATURES::GUIDE::FERTILITY", CODEX.CRITTERSTATUS.FERTILITY.TITLE);
		AddImage(Assets.GetSprite("codex_reproduction"));
		AddBody(CODEX.CRITTERSTATUS.FERTILITY.BODY.CONTAINER1);
		AddSubtitle(CODEX.CRITTERSTATUS.FERTILITY.FERTILITYRATE.TITLE);
		AddBody(CODEX.CRITTERSTATUS.FERTILITY.FERTILITYRATE.CONTAINER1);
		AddSubtitle(CODEX.CRITTERSTATUS.FERTILITY.EGGCHANCES.TITLE);
		AddBody(CODEX.CRITTERSTATUS.FERTILITY.EGGCHANCES.CONTAINER1);
		AddSubtitle(CODEX.CRITTERSTATUS.FERTILITY.FUTURE_OVERCROWDED.TITLE);
		AddBody(CODEX.CRITTERSTATUS.FERTILITY.FUTURE_OVERCROWDED.CONTAINER1);
		AddBody(CODEX.CRITTERSTATUS.FERTILITY.FUTURE_OVERCROWDED.SUBTITLE);
		AddBulletPoint(CODEX.CRITTERSTATUS.FERTILITY.FUTURE_OVERCROWDED.CRAMPED_FERTILITY);
		AddSubtitle(CODEX.CRITTERSTATUS.FERTILITY.INCUBATION.TITLE);
		AddBody(CODEX.CRITTERSTATUS.FERTILITY.INCUBATION.CONTAINER1);
		AddSubtitle(CODEX.CRITTERSTATUS.FERTILITY.MAXAGE.TITLE);
		AddBody(DlcManager.IsExpansion1Active() ? CODEX.CRITTERSTATUS.FERTILITY.MAXAGE.CONTAINER1_DLC1 : CODEX.CRITTERSTATUS.FERTILITY.MAXAGE.CONTAINER1_VANILLA);
		AddSpacer();
		AddSubEntry("CREATURES::GUIDE::DOMESTICATION", CODEX.CRITTERSTATUS.DOMESTICATION.TITLE);
		AddImage(Assets.GetSprite("codex_domestication"));
		AddBody(CODEX.CRITTERSTATUS.DOMESTICATION.BODY.CONTAINER1);
		AddSubtitle(CODEX.CRITTERSTATUS.DOMESTICATION.WILD.TITLE);
		AddBody(CODEX.CRITTERSTATUS.DOMESTICATION.WILD.CONTAINER1);
		AddBody(CODEX.CRITTERSTATUS.DOMESTICATION.WILD.SUBTITLE);
		AddBulletPoint(CODEX.CRITTERSTATUS.DOMESTICATION.WILD.WILD_METABOLISM);
		AddBulletPoint(CODEX.CRITTERSTATUS.DOMESTICATION.WILD.WILD_POOP);
		AddSubtitle(CODEX.CRITTERSTATUS.DOMESTICATION.TAME.TITLE);
		AddBody(CODEX.CRITTERSTATUS.DOMESTICATION.TAME.CONTAINER1);
		AddBody(CODEX.CRITTERSTATUS.DOMESTICATION.TAME.SUBTITLE);
		AddBulletPoint(CODEX.CRITTERSTATUS.DOMESTICATION.TAME.TAME_HAPPINESS);
		AddBulletPoint(CODEX.CRITTERSTATUS.DOMESTICATION.TAME.TAME_METABOLISM);
		return generalInfoEntry;
		void AddBody(string text)
		{
			subEntryContents.Add(new CodexText(text));
		}
		void AddBulletPoint(string text)
		{
			if (text.StartsWith("    • "))
			{
				text = text.Substring("    • ".Length);
			}
			text = "<indent=13px>•<indent=21px>" + text + "</indent></indent>";
			subEntryContents.Add(new CodexText(text));
		}
		void AddImage(Sprite sprite)
		{
			subEntryContents.Add(new CodexImage(432, 1, sprite));
		}
		void AddSpacer()
		{
			subEntryContents.Add(new CodexSpacer());
		}
		void AddSubEntry(string id, string name)
		{
			subEntryContents = new List<ICodexWidget>();
			subEntryContents.Add(new CodexText(name, CodexTextStyle.Title));
			subEntry = new SubEntry(id, "CREATURES::GUIDE", new List<ContentContainer>
			{
				new ContentContainer(subEntryContents, ContentContainer.ContentLayout.Vertical)
			}, name);
			generalInfoEntry.subEntries.Add(subEntry);
		}
		void AddSubtitle(string text)
		{
			AddSpacer();
			subEntryContents.Add(new CodexText(text, CodexTextStyle.Subtitle));
		}
	}

	private static CodexEntry GenerateCritterEntry(Tag speciesTag, string name, List<GameObject> brains)
	{
		CodexEntry codexEntry = null;
		List<ContentContainer> list = new List<ContentContainer>();
		foreach (GameObject brain in brains)
		{
			if (brain.GetDef<BabyMonitor.Def>() != null || !Game.IsCorrectDlcActiveForCurrentSave(brain.GetComponent<KPrefabID>()))
			{
				continue;
			}
			Sprite sprite = null;
			Sprite sprite2 = null;
			CreatureBrain component = brain.GetComponent<CreatureBrain>();
			if (!(component.species != speciesTag))
			{
				if (codexEntry == null)
				{
					codexEntry = new CodexEntry("CREATURES", list, name);
					codexEntry.sortString = name;
					list.Add(new ContentContainer(new List<ICodexWidget>
					{
						new CodexSpacer(),
						new CodexSpacer()
					}, ContentContainer.ContentLayout.Vertical));
				}
				List<ContentContainer> list2 = new List<ContentContainer>();
				string symbolPrefix = component.symbolPrefix;
				sprite = Def.GetUISprite(brain, symbolPrefix + "ui").first;
				GameObject gameObject = Assets.TryGetPrefab(brain.PrefabID().ToString() + "Baby");
				if (gameObject != null)
				{
					sprite2 = Def.GetUISprite(gameObject).first;
				}
				if ((bool)sprite2)
				{
					CodexEntryGenerator.GenerateImageContainers(new Sprite[2] { sprite, sprite2 }, list2, ContentContainer.ContentLayout.Horizontal);
				}
				else
				{
					CodexEntryGenerator.GenerateImageContainers(sprite, list2);
				}
				GenerateCreatureDescriptionContainers(brain, list2);
				SubEntry subEntry = new SubEntry(component.PrefabID().ToString(), speciesTag.ToString(), list2, component.GetProperName());
				subEntry.icon = sprite;
				subEntry.iconColor = Color.white;
				codexEntry.subEntries.Add(subEntry);
			}
		}
		return codexEntry;
	}

	private static void GenerateCreatureDescriptionContainers(GameObject creature, List<ContentContainer> containers)
	{
		containers.Add(new ContentContainer(new List<ICodexWidget>
		{
			new CodexText(creature.GetComponent<InfoDescription>().description)
		}, ContentContainer.ContentLayout.Vertical));
		RobotBatteryMonitor.Def def = creature.GetDef<RobotBatteryMonitor.Def>();
		if (def != null)
		{
			Amount batteryAmount = Db.Get().Amounts.Get(def.batteryAmountId);
			float value = Db.Get().traits.Get(creature.GetComponent<Modifiers>().initialTraits[0]).SelfModifiers.Find((AttributeModifier match) => match.AttributeId == batteryAmount.maxAttribute.Id).Value;
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(CODEX.HEADERS.INTERNALBATTERY, CodexTextStyle.Subtitle),
				new CodexText("    • " + string.Format(CODEX.ROBOT_DESCRIPTORS.BATTERY.CAPACITY, value))
			}, ContentContainer.ContentLayout.Vertical));
		}
		if (creature.GetDef<StorageUnloadMonitor.Def>() != null)
		{
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(CODEX.HEADERS.INTERNALSTORAGE, CodexTextStyle.Subtitle),
				new CodexText("    • " + string.Format(CODEX.ROBOT_DESCRIPTORS.STORAGE.CAPACITY, creature.GetComponents<Storage>()[1].Capacity()))
			}, ContentContainer.ContentLayout.Vertical));
		}
		List<GameObject> prefabsWithTag = Assets.GetPrefabsWithTag((creature.PrefabID().ToString() + "Egg").ToTag());
		if (prefabsWithTag != null && prefabsWithTag.Count > 0)
		{
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(CODEX.HEADERS.HATCHESFROMEGG, CodexTextStyle.Subtitle)
			}, ContentContainer.ContentLayout.Vertical));
			foreach (GameObject item3 in prefabsWithTag)
			{
				containers.Add(new ContentContainer(new List<ICodexWidget>
				{
					new CodexIndentedLabelWithIcon(item3.GetProperName(), CodexTextStyle.Body, Def.GetUISprite(item3))
				}, ContentContainer.ContentLayout.Horizontal));
			}
		}
		CritterTemperatureMonitor.Def def2 = creature.GetDef<CritterTemperatureMonitor.Def>();
		if (def2 != null)
		{
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(CODEX.HEADERS.COMFORTRANGE, CodexTextStyle.Subtitle),
				new CodexText("    • " + string.Format(CODEX.CREATURE_DESCRIPTORS.TEMPERATURE.COMFORT_RANGE, GameUtil.GetFormattedTemperature(def2.temperatureColdUncomfortable), GameUtil.GetFormattedTemperature(def2.temperatureHotUncomfortable))),
				new CodexText("    • " + string.Format(CODEX.CREATURE_DESCRIPTORS.TEMPERATURE.NON_LETHAL_RANGE, GameUtil.GetFormattedTemperature(def2.temperatureColdDeadly), GameUtil.GetFormattedTemperature(def2.temperatureHotDeadly)))
			}, ContentContainer.ContentLayout.Vertical));
		}
		Modifiers component = creature.GetComponent<Modifiers>();
		if (component != null)
		{
			Attribute maxAttribute = Db.Get().Amounts.Age.maxAttribute;
			float totalValue = AttributeInstance.GetTotalValue(maxAttribute, component.GetPreModifiers(maxAttribute));
			string text = ((!Mathf.Approximately(totalValue, 0f)) ? string.Format(CODEX.CREATURE_DESCRIPTORS.MAXAGE, maxAttribute.formatter.GetFormattedValue(totalValue, GameUtil.TimeSlice.None)) : null);
			if (text != null)
			{
				containers.Add(new ContentContainer(new List<ICodexWidget>
				{
					new CodexSpacer(),
					new CodexText(CODEX.HEADERS.CRITTERMAXAGE, CodexTextStyle.Subtitle),
					new CodexText(text)
				}, ContentContainer.ContentLayout.Vertical));
			}
		}
		OvercrowdingMonitor.Def def3 = creature.GetDef<OvercrowdingMonitor.Def>();
		if (def3 != null && def3.spaceRequiredPerCreature > 0)
		{
			bool num = creature.GetDef<FishOvercrowdingMonitor.Def>() != null;
			LocString locString = (num ? CODEX.CREATURE_DESCRIPTORS.OVERCROWDING_AQUATIC : CODEX.CREATURE_DESCRIPTORS.OVERCROWDING);
			LocString locString2 = (num ? CODEX.CREATURE_DESCRIPTORS.CONFINED_AQUATIC : CODEX.CREATURE_DESCRIPTORS.CONFINED);
			containers.Add(new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(CODEX.HEADERS.CRITTEROVERCROWDING, CodexTextStyle.Subtitle),
				new CodexText("    • " + string.Format(locString, def3.spaceRequiredPerCreature)),
				new CodexText("    • " + string.Format(locString2, def3.spaceRequiredPerCreature))
			}, ContentContainer.ContentLayout.Vertical));
		}
		string text2 = null;
		float amount = 0f;
		Tag tag = default(Tag);
		Butcherable component2 = creature.GetComponent<Butcherable>();
		if (component2 != null && component2.drops != null && component2.drops.Count > 0)
		{
			text2 = (tag.Name = component2.drops.Keys.ToList()[0]);
			amount = component2.drops[text2];
		}
		string text4 = null;
		string text5 = null;
		if (tag.IsValid)
		{
			text4 = TagManager.GetProperName(tag);
			text5 = "\t" + GameUtil.GetFormattedByTag(tag, amount);
		}
		if (!string.IsNullOrEmpty(text4) && !string.IsNullOrEmpty(text5))
		{
			ContentContainer item = new ContentContainer(new List<ICodexWidget>
			{
				new CodexSpacer(),
				new CodexText(CODEX.HEADERS.CRITTERDROPS, CodexTextStyle.Subtitle)
			}, ContentContainer.ContentLayout.Vertical);
			ContentContainer item2 = new ContentContainer(new List<ICodexWidget>
			{
				new CodexIndentedLabelWithIcon(text4, CodexTextStyle.Body, Def.GetUISprite(text2)),
				new CodexText(text5)
			}, ContentContainer.ContentLayout.Vertical);
			containers.Add(item);
			containers.Add(item2);
		}
		new List<Tag>();
		Diet prefabDiet = DietManager.Instance.GetPrefabDiet(creature);
		if (prefabDiet == null)
		{
			return;
		}
		Diet.Info[] infos = prefabDiet.infos;
		if (infos == null || infos.Length == 0)
		{
			return;
		}
		float num2 = 0f;
		foreach (AttributeModifier selfModifier in Db.Get().traits.Get(creature.GetComponent<Modifiers>().initialTraits[0]).SelfModifiers)
		{
			if (selfModifier.AttributeId == Db.Get().Amounts.Calories.deltaAttribute.Id)
			{
				num2 = selfModifier.Value;
			}
		}
		CaloriesConsumedElementProducer component3 = creature.GetComponent<CaloriesConsumedElementProducer>();
		List<ICodexWidget> list = new List<ICodexWidget>();
		Diet.Info[] array = infos;
		foreach (Diet.Info info in array)
		{
			if (info.consumedTags.Count == 0)
			{
				continue;
			}
			foreach (Tag consumedTag in info.consumedTags)
			{
				Element element = ElementLoader.FindElementByHash(ElementLoader.GetElementID(consumedTag));
				if ((element.id == SimHashes.Vacuum || element.id == SimHashes.Void) && Assets.GetPrefab(consumedTag) == null)
				{
					continue;
				}
				bool num4 = prefabDiet.IsConsumedTagAbleToBeEatenDirectly(consumedTag);
				float num5 = (0f - num2) / info.caloriesPerKg;
				float outputAmount = num5 * info.producedConversionRate;
				if (num4)
				{
					if (info.foodType == Diet.Info.FoodType.EatPlantDirectly)
					{
						list.Add(new CodexConversionPanel(consumedTag.ProperName(), consumedTag, num5, inputContinuous: true, GameUtil.GetFormattedDirectPlantConsumptionValuePerCycle, info.producedElement, outputAmount, outputContinuous: true, null, creature));
					}
					else if (info.foodType == Diet.Info.FoodType.EatPlantStorage)
					{
						list.Add(new CodexConversionPanel(consumedTag.ProperName(), consumedTag, num5, inputContinuous: true, GameUtil.GetFormattedPlantStorageConsumptionValuePerCycle, info.producedElement, outputAmount, outputContinuous: true, null, creature));
					}
					else if (info.foodType == Diet.Info.FoodType.EatPrey || info.foodType == Diet.Info.FoodType.EatButcheredPrey)
					{
						float num6 = prefabDiet.AvailableCaloriesInPrey(consumedTag);
						num5 = (0f - num2) / num6;
						outputAmount = num5 * info.producedConversionRate * num6 / info.caloriesPerKg;
						list.Add(new CodexConversionPanel(consumedTag.ProperName(), consumedTag, num5, inputContinuous: true, GameUtil.GetFormattedPreyConsumptionValuePerCycle, info.producedElement, outputAmount, outputContinuous: true, null, creature));
					}
				}
				else
				{
					list.Add(new CodexConversionPanel(consumedTag.ProperName(), consumedTag, num5, inputContinuous: true, info.producedElement, outputAmount, outputContinuous: true, creature));
				}
				if (component3 != null)
				{
					list.Add(new CodexConversionPanel(CODEX.HEADERS.CRITTER_EXTRA_DIET_PRODUCTION, consumedTag, num5, inputContinuous: true, component3.producedElement.CreateTag(), num5 * 1000f * component3.kgProducedPerKcalConsumed * 2f, outputContinuous: true, creature));
				}
			}
		}
		ContentContainer contentContainer = new ContentContainer(list, ContentContainer.ContentLayout.Vertical);
		containers.Add(new ContentContainer(new List<ICodexWidget>
		{
			new CodexSpacer(),
			new CodexCollapsibleHeader(CODEX.HEADERS.DIET, contentContainer)
		}, ContentContainer.ContentLayout.Vertical));
		containers.Add(contentContainer);
		containers.Add(new ContentContainer(new List<ICodexWidget>
		{
			new CodexSpacer(),
			new CodexSpacer()
		}, ContentContainer.ContentLayout.Vertical));
		CodexEntryGenerator_Elements.GenerateMadeAndUsedContainers(creature.PrefabID(), containers);
	}
}
