using System;
using System.Collections.Generic;
using System.Linq;
using STRINGS;
using UnityEngine;

namespace UtilLibs;

public class RecipeBuilder
{
	private string techRequirement;

	private string fabricator;

	private float time;

	private RecipeNameDisplay nameDisplay;

	private string description;

	private string name;

	private string spritePrefabId;

	private int sortOrder = 0;

	private int hepConsumed = 0;

	private int hepProduced = 0;

	private List<RecipeElement> inputs;

	private List<RecipeElement> outputs;

	private Dictionary<RecipeElement, Tag> GroupDescriptors = new Dictionary<RecipeElement, Tag>();

	public RecipeElement FirstIngredient()
	{
		if (inputs == null || !inputs.Any())
		{
			return null;
		}
		return inputs.First();
	}

	public static RecipeBuilder Create(string fabricatorID, string description, float time)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return new RecipeBuilder
		{
			fabricator = fabricatorID,
			description = description,
			time = time,
			nameDisplay = (RecipeNameDisplay)2,
			inputs = new List<RecipeElement>(),
			outputs = new List<RecipeElement>()
		};
	}

	public static RecipeBuilder Create(string fabricatorID, float time)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return new RecipeBuilder
		{
			fabricator = fabricatorID,
			time = time,
			nameDisplay = (RecipeNameDisplay)2,
			inputs = new List<RecipeElement>(),
			outputs = new List<RecipeElement>()
		};
	}

	public RecipeBuilder Description(string desc)
	{
		if (desc != null)
		{
			description = desc;
		}
		return this;
	}

	public RecipeBuilder DescriptionFunc(Func<RecipeElement[], RecipeElement[], string> descriptionAction)
	{
		if (descriptionAction != null)
		{
			description = descriptionAction(inputs.ToArray(), outputs.ToArray());
		}
		return this;
	}

	public RecipeBuilder AppendExtraDescription(string extraDescription)
	{
		description += extraDescription;
		return this;
	}

	public RecipeBuilder Description(string ToFormat, int inputCount, int outputCount)
	{
		if (inputs.Count < inputCount || outputs.Count < outputCount)
		{
			throw new InvalidOperationException($"Recipe must have at least {inputCount} inputs and {outputCount} outputs to use Description.");
		}
		object[] formatArgs = GetFormatArgs(inputCount, outputCount);
		description = string.Format(ToFormat, formatArgs);
		return this;
	}

	public string[] GetFormatArgs(int inputCount, int outputCount)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		if (inputCount > inputs.Count || outputCount > outputs.Count)
		{
			throw new InvalidOperationException($"Recipe must have at least {inputCount} inputs and {outputCount} outputs to use GetFormatArgs.");
		}
		List<string> list = new List<string>();
		for (int i = 0; i < inputCount; i++)
		{
			RecipeElement val = inputs[i];
			if (val == null)
			{
				throw new InvalidOperationException($"Input {i} is null in GetFormatArgs.");
			}
			Tag val2 = val.material;
			if (GroupDescriptors.TryGetValue(val, out var value))
			{
				val2 = value;
			}
			GameObject val3 = Assets.TryGetPrefab(val2);
			string item = ((val3 != null) ? KSelectableExtensions.GetProperName(val3) : null) ?? GameTagExtensions.ProperName(val2);
			list.Add(item);
		}
		for (int j = 0; j < outputCount; j++)
		{
			RecipeElement val4 = outputs[j];
			if (val4 == null)
			{
				throw new InvalidOperationException($"Output {j} is null in GetFormatArgs.");
			}
			Tag val5 = val4.material;
			if (GroupDescriptors.TryGetValue(val4, out var value2))
			{
				val5 = value2;
			}
			GameObject val6 = Assets.TryGetPrefab(val5);
			string item2 = ((val6 != null) ? KSelectableExtensions.GetProperName(val6) : null) ?? GameTagExtensions.ProperName(val5);
			list.Add(item2);
		}
		return list.ToArray();
	}

	public RecipeBuilder Description1I1O(string ToFormat)
	{
		return Description(ToFormat, 1, 1);
	}

	public RecipeBuilder Description1I2O(string ToFormat)
	{
		return Description(ToFormat, 1, 2);
	}

	public RecipeBuilder Description1I3O(string ToFormat)
	{
		return Description(ToFormat, 1, 3);
	}

	public RecipeBuilder Description1I4O(string ToFormat)
	{
		return Description(ToFormat, 1, 4);
	}

	public RecipeBuilder Description2I1O(string ToFormat)
	{
		return Description(ToFormat, 2, 1);
	}

	public RecipeBuilder Description2I2O(string ToFormat)
	{
		return Description(ToFormat, 2, 2);
	}

	public RecipeBuilder Description3I2O(string ToFormat)
	{
		return Description(ToFormat, 3, 2);
	}

	public RecipeBuilder RequiresTech(string techId)
	{
		techRequirement = techId;
		return this;
	}

	public RecipeBuilder NameDisplay(RecipeNameDisplay nameDisplay)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		this.nameDisplay = nameDisplay;
		return this;
	}

	public RecipeBuilder InputHEP(int hep)
	{
		hepConsumed = hep;
		return this;
	}

	public RecipeBuilder OutputHEP(int hep)
	{
		hepProduced = hep;
		return this;
	}

	public RecipeBuilder NameOverride(string name)
	{
		this.name = name;
		return this;
	}

	public RecipeBuilder NameOverrideFormat(string name, object f1)
	{
		this.name = string.Format(name, f1);
		return this;
	}

	public RecipeBuilder NameOverrideFormatIngredient(string name, int ingredientIndex = 0)
	{
		string[] formatArgs = GetFormatArgs(inputs.Count(), outputs.Count());
		if (ingredientIndex < 0 || ingredientIndex >= formatArgs.Length)
		{
			throw new ArgumentOutOfRangeException("ingredientIndex", "Ingredient index is out of range.");
		}
		this.name = string.Format(name, formatArgs[ingredientIndex]);
		return this;
	}

	public RecipeBuilder NameOverrideFormatFromTo(int ingredientIndex = 0, int resultIndex = 0)
	{
		string[] formatArgs = GetFormatArgs(inputs.Count(), outputs.Count());
		if (ingredientIndex < 0 || ingredientIndex >= formatArgs.Length)
		{
			throw new ArgumentOutOfRangeException("ingredientIndex", "Ingredient index is out of range.");
		}
		int num = inputs.Count();
		resultIndex += num;
		if (resultIndex < num || resultIndex >= formatArgs.Length)
		{
			throw new ArgumentOutOfRangeException("ingredientIndex", "Result index is out of range.");
		}
		name = string.Format(LocString.op_Implicit(REFINERYSIDESCREEN.RECIPE_FROM_TO), formatArgs[ingredientIndex], formatArgs[resultIndex]);
		return this;
	}

	public RecipeBuilder IconPrefabIngredient(int index)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (index < 0 || index >= inputs.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Index is out of range for inputs.");
		}
		Tag material = inputs[index].material;
		return IconPrefabOverride(material);
	}

	public RecipeBuilder IconPrefabResult(int index)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (index < 0 || index >= outputs.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Index is out of range for outputs.");
		}
		Tag material = outputs[index].material;
		return IconPrefabOverride(material);
	}

	public unsafe RecipeBuilder IconPrefabOverride(Tag prefabId)
	{
		return IconPrefabOverride(((object)(*(Tag*)(&prefabId))/*cast due to .constrained prefix*/).ToString());
	}

	public RecipeBuilder IconPrefabOverride(string prefabId)
	{
		spritePrefabId = prefabId;
		return this;
	}

	public RecipeBuilder SortOrder(int sortOrder)
	{
		this.sortOrder = sortOrder;
		return this;
	}

	public RecipeBuilder Input(RecipeElement element)
	{
		inputs.Add(element);
		return this;
	}

	public RecipeBuilder Input(Tag tag, float amount, bool inheritElement = false, bool doNotConsume = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		RecipeElement val = new RecipeElement(tag, amount, inheritElement);
		val.doNotConsume = doNotConsume;
		inputs.Add(val);
		return this;
	}

	public RecipeBuilder Input(IEnumerable<Tag> tags, float amount, Tag descriptor = default(Tag))
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		tags = tags.Where((Tag tag) => (Object)(object)Assets.GetPrefab(tag) != (Object)null);
		RecipeElement val = new RecipeElement(tags.ToArray(), amount);
		inputs.Add(val);
		if (descriptor != default(Tag))
		{
			GroupDescriptors.Add(val, descriptor);
		}
		return this;
	}

	public RecipeBuilder Input(IEnumerable<SimHashes> tags, float amount, Tag descriptor = default(Tag))
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		RecipeElement val = new RecipeElement(tags.Select((SimHashes simhash) => GameTagExtensions.CreateTag(simhash)).ToArray(), amount);
		inputs.Add(val);
		if (descriptor != default(Tag))
		{
			GroupDescriptors.Add(val, descriptor);
		}
		return this;
	}

	public RecipeBuilder Input(IEnumerable<SimHashes> tags, float[] amounts, Tag descriptor = default(Tag))
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		RecipeElement val = new RecipeElement(tags.Select((SimHashes simhash) => GameTagExtensions.CreateTag(simhash)).ToArray(), amounts);
		inputs.Add(val);
		if (descriptor != default(Tag))
		{
			GroupDescriptors.Add(val, descriptor);
		}
		return this;
	}

	public RecipeBuilder InputSO(SimHashes simHashes, float amount, bool inheritElement = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return InputConditional(simHashes, amount, (Func<bool>)DlcManager.IsExpansion1Active, inheritElement);
	}

	public RecipeBuilder InputBase(SimHashes simHashes, float amount, bool inheritElement = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return InputConditional(simHashes, amount, (Func<bool>)DlcManager.IsPureVanilla, inheritElement);
	}

	public RecipeBuilder InputDlcDependent(SimHashes basegame, SimHashes spacedout, float amount, bool inheritElement = false)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return Input(DlcManager.IsPureVanilla() ? basegame : spacedout, amount, inheritElement);
	}

	public RecipeBuilder InputConditional(SimHashes simhash, float amount, Func<bool> condition, bool inheritElement = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return InputConditional(GameTagExtensions.CreateTag(simhash), amount, condition(), inheritElement);
	}

	public RecipeBuilder InputConditional(SimHashes simhash, float amount, bool condition, bool inheritElement = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return InputConditional(GameTagExtensions.CreateTag(simhash), amount, condition, inheritElement);
	}

	public RecipeBuilder InputConditional(Tag tag, float amount, bool condition, bool inheritElement = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (condition)
		{
			return Input(tag, amount, inheritElement);
		}
		return this;
	}

	public RecipeBuilder InputConditional(IEnumerable<SimHashes> hashes, float amount, Func<bool> condition)
	{
		return InputConditional(hashes, amount, condition());
	}

	public RecipeBuilder InputConditional(IEnumerable<SimHashes> hashes, float amount, bool condition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (condition)
		{
			return Input(hashes, amount);
		}
		return this;
	}

	public RecipeBuilder AltInput(SimHashes simHashes, float amount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return AltInput(GameTagExtensions.CreateTag(simHashes), amount);
	}

	public RecipeBuilder AltInput(Tag tag, float amount)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		RecipeElement val = inputs.Last();
		if (val == null)
		{
			throw new InvalidOperationException("Cannot add alt input when there is no previous ingredient!");
		}
		inputs.Remove(val);
		Tag[] possibleMaterials = val.possibleMaterials;
		float[] array = ((!(val.amount > 0f)) ? val.possibleMaterialAmounts : new float[1] { val.amount });
		possibleMaterials = Util.Concat<Tag>(possibleMaterials, (Tag[])(object)new Tag[1] { tag });
		array = Util.Concat<float>(array, new float[1] { amount });
		RecipeElement val2 = new RecipeElement(possibleMaterials, array);
		val2.inheritElement = val.inheritElement;
		return this;
	}

	public RecipeBuilder Input(SimHashes simhash, float amount, bool inheritElement = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		inputs.Add(new RecipeElement(GameTagExtensions.CreateTag(simhash), amount, inheritElement));
		return this;
	}

	public RecipeBuilder Output(Tag tag, float amount, TemperatureOperation tempOp = (TemperatureOperation)0, bool storeElement = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		outputs.Add(new RecipeElement(tag, amount, tempOp, storeElement));
		return this;
	}

	public RecipeBuilder Output(RecipeElement element)
	{
		outputs.Add(element);
		return this;
	}

	public RecipeBuilder Output(SimHashes simhash, float amount, TemperatureOperation tempOp = (TemperatureOperation)0, bool storeElement = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		outputs.Add(new RecipeElement(GameTagExtensions.CreateTag(simhash), amount, tempOp, storeElement));
		return this;
	}

	public RecipeBuilder OutputConditional(SimHashes hashes, float amount, Func<bool> condition, TemperatureOperation tempOp = (TemperatureOperation)0, bool storeElement = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return OutputConditional(hashes, amount, condition(), tempOp, storeElement);
	}

	public RecipeBuilder OutputConditional(SimHashes hashes, float amount, bool condition, TemperatureOperation tempOp = (TemperatureOperation)0, bool storeElement = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (condition)
		{
			return Output(hashes, amount, (TemperatureOperation)0);
		}
		return this;
	}

	public RecipeBuilder FacadeOutput(Tag tag, float amount, string facadeID = "", bool storeElement = false, TemperatureOperation tempOp = (TemperatureOperation)0)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		outputs.Add(new RecipeElement(tag, amount, tempOp, facadeID, storeElement));
		return this;
	}

	public ComplexRecipe Build(string facadeID = "")
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		RecipeElement[] array = inputs.ToArray();
		RecipeElement[] array2 = outputs.ToArray();
		string text = (Util.IsNullOrWhiteSpace(facadeID) ? ComplexRecipeManager.MakeRecipeID(fabricator, (IList<RecipeElement>)array, (IList<RecipeElement>)array2) : ComplexRecipeManager.MakeRecipeID(fabricator, (IList<RecipeElement>)array, (IList<RecipeElement>)array2, facadeID));
		ComplexRecipe val = new ComplexRecipe(text, array, array2, hepConsumed, hepProduced)
		{
			time = time,
			description = description,
			customName = name,
			nameDisplay = nameDisplay,
			fabricators = new List<Tag> { Tag.op_Implicit(fabricator) }
		};
		if (sortOrder > 0)
		{
			val.sortOrder = sortOrder;
		}
		if (!Util.IsNullOrWhiteSpace(spritePrefabId))
		{
			val.customSpritePrefabID = spritePrefabId;
		}
		if (!Util.IsNullOrWhiteSpace(techRequirement))
		{
			val.requiredTech = techRequirement;
		}
		return val;
	}
}
