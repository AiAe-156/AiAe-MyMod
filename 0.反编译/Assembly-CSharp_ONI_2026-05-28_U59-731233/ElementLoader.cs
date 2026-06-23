#define UNITY_ASSERTIONS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ElementData;
using Klei;
using ProcGenGame;
using STRINGS;
using UnityEngine;

public class ElementLoader
{
	public static List<Element> elements;

	public static Dictionary<int, Element> elementTable;

	public static Dictionary<Tag, Element> elementTagTable;

	private static string path = Application.streamingAssetsPath + "/elements/";

	private static readonly Color noColour = new Color(0f, 0f, 0f, 0f);

	public static float GetMinMeltingPointAmongElements(IList<Tag> elements)
	{
		float num = float.MaxValue;
		for (int i = 0; i < elements.Count; i++)
		{
			Element element = GetElement(elements[i]);
			if (element != null)
			{
				num = Mathf.Min(num, element.highTemp);
			}
		}
		return num;
	}

	public static List<ElementEntry> CollectElementsFromYAML()
	{
		List<ElementEntry> list = new List<ElementEntry>();
		ListPool<FileHandle, ElementLoader>.PooledList pooledList = ListPool<FileHandle, ElementLoader>.Allocate();
		FileSystem.GetFiles(FileSystem.Normalize(path), "*.yaml", pooledList);
		ListPool<YamlIO.Error, ElementLoader>.PooledList errors = ListPool<YamlIO.Error, ElementLoader>.Allocate();
		foreach (FileHandle file in pooledList)
		{
			if (!Path.GetFileName(file.full_path).StartsWith(".") && KYaml.LoadFile<ElementEntryCollection>(file, out var result, delegate(string path, Exception exception)
			{
				errors.Add(new YamlIO.Error
				{
					file = file,
					message = exception.Message,
					inner_exception = exception.InnerException,
					severity = YamlIO.Error.Severity.Fatal
				});
			}))
			{
				list.AddRange(result.elements);
			}
		}
		pooledList.Recycle();
		if (Global.Instance != null && Global.Instance.modManager != null)
		{
			Global.Instance.modManager.HandleErrors(errors);
		}
		errors.Recycle();
		return list;
	}

	public static void Load(ref Hashtable substanceList, Dictionary<string, SubstanceTable> substanceTablesByDlc)
	{
		elements = new List<Element>();
		elementTable = new Dictionary<int, Element>();
		elementTagTable = new Dictionary<Tag, Element>();
		List<ElementEntry> list = CollectElementsFromYAML();
		foreach (ElementEntry item in list)
		{
			int num = Hash.SDBMLower(item.elementId);
			if (!elementTable.ContainsKey(num) && substanceTablesByDlc.ContainsKey(item.dlcId))
			{
				Element element = new Element();
				element.id = (SimHashes)num;
				element.name = Strings.Get(item.localizationID);
				element.nameUpperCase = element.name.ToUpper();
				element.description = Strings.Get(item.description);
				element.tag = TagManager.Create(item.elementId, element.name);
				CopyEntryToElement(item, element);
				elements.Add(element);
				elementTable[num] = element;
				elementTagTable[element.tag] = element;
				if (!ManifestSubstanceForElement(element, ref substanceList, substanceTablesByDlc[item.dlcId]))
				{
					Debug.LogWarning("Missing substance for element: " + element.id);
				}
			}
		}
		FinaliseElementsTable(ref substanceList);
		WorldGen.SetupDefaultElements();
	}

	private static void CopyEntryToElement(ElementEntry entry, Element elem)
	{
		int num = Hash.SDBMLower(entry.elementId);
		UnityEngine.Debug.Assert(num == (int)elem.id);
		elem.tag = TagManager.Create(entry.elementId.ToString());
		elem.specificHeatCapacity = entry.specificHeatCapacity;
		elem.thermalConductivity = entry.thermalConductivity;
		elem.molarMass = entry.molarMass;
		elem.strength = entry.strength;
		elem.disabled = entry.isDisabled;
		elem.dlcId = entry.dlcId;
		elem.flow = entry.flow;
		elem.maxMass = entry.maxMass;
		elem.maxCompression = entry.liquidCompression;
		elem.viscosity = entry.speed;
		elem.minHorizontalFlow = entry.minHorizontalFlow;
		elem.minVerticalFlow = entry.minVerticalFlow;
		elem.solidSurfaceAreaMultiplier = entry.solidSurfaceAreaMultiplier;
		elem.liquidSurfaceAreaMultiplier = entry.liquidSurfaceAreaMultiplier;
		elem.gasSurfaceAreaMultiplier = entry.gasSurfaceAreaMultiplier;
		elem.state = entry.state;
		elem.hardness = entry.hardness;
		elem.lowTemp = (entry.lowTemp.HasValue ? entry.lowTemp.Value : 0f);
		elem.lowTempTransitionTarget = (SimHashes)Hash.SDBMLower(entry.lowTempTransitionTarget);
		elem.highTemp = (entry.highTemp.HasValue ? entry.highTemp.Value : 10000f);
		elem.highTempTransitionTarget = (SimHashes)Hash.SDBMLower(entry.highTempTransitionTarget);
		elem.highTempTransitionOreID = (SimHashes)Hash.SDBMLower(entry.highTempTransitionOreId);
		elem.highTempTransitionOreMassConversion = entry.highTempTransitionOreMassConversion;
		elem.lowTempTransitionOreID = (SimHashes)Hash.SDBMLower(entry.lowTempTransitionOreId);
		elem.lowTempTransitionOreMassConversion = entry.lowTempTransitionOreMassConversion;
		elem.refinedMetalTarget = (SimHashes)Hash.SDBMLower(entry.refinedMetalTarget);
		elem.sublimateId = (SimHashes)Hash.SDBMLower(entry.sublimateId);
		elem.convertId = (SimHashes)Hash.SDBMLower(entry.convertId);
		elem.sublimateFX = (SpawnFXHashes)Hash.SDBMLower(entry.sublimateFx);
		elem.sublimateRate = entry.sublimateRate;
		elem.sublimateEfficiency = entry.sublimateEfficiency;
		elem.sublimateProbability = entry.sublimateProbability;
		elem.offGasPercentage = entry.offGasPercentage;
		elem.lightAbsorptionFactor = entry.lightAbsorptionFactor;
		elem.radiationAbsorptionFactor = entry.radiationAbsorptionFactor;
		elem.radiationPer1000Mass = entry.radiationPer1000Mass;
		elem.toxicity = entry.toxicity;
		elem.elementComposition = entry.composition;
		Tag phaseTag = TagManager.Create(entry.state.ToString());
		elem.materialCategory = CreateMaterialCategoryTag(elem.id, phaseTag, entry.materialCategory);
		elem.oreTags = CreateOreTags(elem.materialCategory, phaseTag, entry.tags);
		elem.buildMenuSort = entry.buildMenuSort;
		Sim.PhysicsData defaultValues = new Sim.PhysicsData
		{
			temperature = entry.defaultTemperature,
			mass = entry.defaultMass,
			pressure = entry.defaultPressure
		};
		switch (entry.state)
		{
		case Element.State.Solid:
			GameTags.SolidElements.Add(elem.tag);
			break;
		case Element.State.Liquid:
			GameTags.LiquidElements.Add(elem.tag);
			break;
		case Element.State.Gas:
			GameTags.GasElements.Add(elem.tag);
			defaultValues.mass = 1f;
			elem.maxMass = 1.8f;
			break;
		}
		elem.defaultValues = defaultValues;
	}

	private static bool ManifestSubstanceForElement(Element elem, ref Hashtable substanceList, SubstanceTable substanceTable)
	{
		elem.substance = null;
		if (substanceList.ContainsKey(elem.id))
		{
			elem.substance = substanceList[elem.id] as Substance;
			return false;
		}
		if (substanceTable != null)
		{
			elem.substance = substanceTable.GetSubstance(elem.id);
		}
		if (elem.substance == null)
		{
			elem.substance = new Substance();
			substanceTable.GetList().Add(elem.substance);
		}
		elem.substance.elementID = elem.id;
		elem.substance.renderedByWorld = elem.IsSolid;
		elem.substance.idx = substanceList.Count;
		if (elem.substance.uiColour == noColour)
		{
			int count = elements.Count;
			int idx = elem.substance.idx;
			elem.substance.uiColour = Color.HSVToRGB((float)idx / (float)count, 1f, 1f);
		}
		string name = UI.StripLinkFormatting(elem.name);
		elem.substance.name = name;
		elem.substance.nameTag = elem.tag;
		elem.substance.audioConfig = ElementsAudio.Instance.GetConfigForElement(elem.id);
		substanceList.Add(elem.id, elem.substance);
		return true;
	}

	public static Element FindElementByName(string name)
	{
		return FindElementByHash((SimHashes)Hash.SDBMLower(name));
	}

	public static Element FindElementByTag(Tag tag)
	{
		return GetElement(tag);
	}

	public static List<Element> FindElements(Func<Element, bool> filter)
	{
		List<Element> list = new List<Element>();
		foreach (int key in elementTable.Keys)
		{
			Element element = elementTable[key];
			if (filter(element))
			{
				list.Add(element);
			}
		}
		return list;
	}

	public static Element FindElementByHash(SimHashes hash)
	{
		Element value = null;
		elementTable.TryGetValue((int)hash, out value);
		return value;
	}

	public static ushort GetElementIndex(SimHashes hash)
	{
		Element value = null;
		elementTable.TryGetValue((int)hash, out value);
		return value?.idx ?? ushort.MaxValue;
	}

	public static Element GetElement(Tag tag)
	{
		elementTagTable.TryGetValue(tag, out var value);
		return value;
	}

	public static SimHashes GetElementID(Tag tag)
	{
		elementTagTable.TryGetValue(tag, out var value);
		return value?.id ?? SimHashes.Vacuum;
	}

	private static SimHashes GetID(int column, int row, string[,] grid, SimHashes defaultValue = SimHashes.Vacuum)
	{
		if (column >= grid.GetLength(0) || row > grid.GetLength(1))
		{
			Debug.LogError($"Could not find element at loc [{column},{row}] grid is only [{grid.GetLength(0)},{grid.GetLength(1)}]");
			return defaultValue;
		}
		string text = grid[column, row];
		if (text == null || text == "")
		{
			return defaultValue;
		}
		object obj = null;
		try
		{
			obj = Enum.Parse(typeof(SimHashes), text);
		}
		catch (Exception ex)
		{
			Debug.LogError($"Could not find element {text}: {ex.ToString()}");
			return defaultValue;
		}
		return (SimHashes)obj;
	}

	private static SpawnFXHashes GetSpawnFX(int column, int row, string[,] grid)
	{
		if (column >= grid.GetLength(0) || row > grid.GetLength(1))
		{
			Debug.LogError($"Could not find SpawnFXHashes at loc [{column},{row}] grid is only [{grid.GetLength(0)},{grid.GetLength(1)}]");
			return SpawnFXHashes.None;
		}
		string text = grid[column, row];
		if (text == null || text == "")
		{
			return SpawnFXHashes.None;
		}
		object obj = null;
		try
		{
			obj = Enum.Parse(typeof(SpawnFXHashes), text);
		}
		catch (Exception ex)
		{
			Debug.LogError($"Could not find FX {text}: {ex.ToString()}");
			return SpawnFXHashes.None;
		}
		return (SpawnFXHashes)obj;
	}

	private static Tag CreateMaterialCategoryTag(SimHashes element_id, Tag phaseTag, string materialCategoryField)
	{
		if (!string.IsNullOrEmpty(materialCategoryField))
		{
			Tag tag = TagManager.Create(materialCategoryField);
			if (!GameTags.MaterialCategories.Contains(tag) && !GameTags.IgnoredMaterialCategories.Contains(tag))
			{
				Debug.LogWarningFormat("Element {0} has category {1}, but that isn't in GameTags.MaterialCategores!", element_id, materialCategoryField);
			}
			return tag;
		}
		return phaseTag;
	}

	private static Tag[] CreateOreTags(Tag materialCategory, Tag phaseTag, string[] ore_tags_split)
	{
		List<Tag> list = new List<Tag>();
		if (ore_tags_split != null)
		{
			foreach (string text in ore_tags_split)
			{
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(TagManager.Create(text));
				}
			}
		}
		list.Add(phaseTag);
		if (materialCategory.IsValid && !list.Contains(materialCategory))
		{
			list.Add(materialCategory);
		}
		return list.ToArray();
	}

	private static void FinaliseElementsTable(ref Hashtable substanceList)
	{
		foreach (Element element3 in elements)
		{
			if (element3 == null)
			{
				continue;
			}
			if (element3.substance == null)
			{
				Debug.LogWarning("Skipping finalise for missing element: " + element3.id);
				continue;
			}
			Debug.Assert(element3.substance.nameTag.IsValid);
			if (element3.thermalConductivity == 0f)
			{
				element3.state |= Element.State.TemperatureInsulated;
			}
			if (element3.strength == 0f)
			{
				element3.state |= Element.State.Unbreakable;
			}
			if (element3.IsSolid || element3.IsLiquid || element3.IsGas)
			{
				Element element = FindElementByHash(element3.highTempTransitionTarget);
				if (element != null)
				{
					element3.highTempTransition = element;
				}
				Element element2 = FindElementByHash(element3.lowTempTransitionTarget);
				if (element2 != null)
				{
					element3.lowTempTransition = element2;
				}
			}
		}
		IOrderedEnumerable<Element> source = from e in elements
			orderby (int)(e.state & Element.State.Solid) descending, e.id
			select e;
		elements = source.ToList();
		for (int num = 0; num < elements.Count; num++)
		{
			if (elements[num].substance != null)
			{
				elements[num].substance.idx = num;
			}
			elements[num].idx = (ushort)num;
		}
	}

	private static void ValidateElements()
	{
		Debug.Log("------ Start Validating Elements ------");
		foreach (Element element in elements)
		{
			string text = $"{element.tag.ProperNameStripLink()} ({element.state})";
			if (element.IsLiquid && element.sublimateId != 0)
			{
				Debug.Assert(element.sublimateRate == 0f, text + ": Liquids don't use sublimateRate, use offGasPercentage instead.");
				Debug.Assert(element.offGasPercentage > 0f, text + ": Missing offGasPercentage");
			}
			if (element.IsSolid && element.sublimateId != 0)
			{
				Debug.Assert(element.offGasPercentage == 0f, text + ": Solids don't use offGasPercentage, use sublimateRate instead.");
				Debug.Assert(element.sublimateRate > 0f, text + ": Missing sublimationRate");
				Debug.Assert(element.sublimateRate * element.sublimateEfficiency > 0.001f, text + ": Sublimation rate and efficiency will result in gas that will be obliterated because its less than 1g. Increase these values and use sublimateProbability if you want a low amount of sublimation");
			}
			if (element.highTempTransition != null && element.highTempTransition.lowTempTransition == element)
			{
				Debug.Assert(element.highTemp >= element.highTempTransition.lowTemp, text + ": highTemp is higher than transition element's (" + element.highTempTransition.tag.ProperNameStripLink() + ") lowTemp");
			}
			Debug.Assert(element.defaultValues.mass <= element.maxMass, text + ": Default mass should be less than max mass");
			if (false)
			{
				if (element.IsSolid && element.highTempTransition != null && element.highTempTransition.IsLiquid && element.defaultValues.mass > element.highTempTransition.maxMass)
				{
					Debug.LogWarning($"{text} defaultMass {element.defaultValues.mass} > {element.highTempTransition.tag.ProperNameStripLink()}: maxMass {element.highTempTransition.maxMass}");
				}
				if (element.defaultValues.mass < element.maxMass && element.IsLiquid)
				{
					Debug.LogWarning($"{element.tag.ProperNameStripLink()} has defaultMass: {element.defaultValues.mass} and maxMass {element.maxMass}");
				}
			}
		}
		Debug.Log("------ End Validating Elements ------");
	}
}
