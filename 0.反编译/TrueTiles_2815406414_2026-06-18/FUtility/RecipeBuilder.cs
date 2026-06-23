using System.Collections.Generic;
using System.Linq;

namespace FUtility;

public class RecipeBuilder
{
	private string fabricator;

	private float time;

	private RecipeNameDisplay nameDisplay;

	private string description;

	private string name;

	private string requiredTech;

	private int sortOrder;

	private List<RecipeElement> inputs;

	private List<RecipeElement> outputs;

	private string[] requiredDlcIds;

	public static RecipeBuilder Create(string fabricatorID, string description, float time)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
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

	public RecipeBuilder Tech(string tech)
	{
		requiredTech = tech;
		return this;
	}

	public RecipeBuilder NameDisplay(RecipeNameDisplay nameDisplay)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		this.nameDisplay = nameDisplay;
		return this;
	}

	public RecipeBuilder NameOverride(string name)
	{
		this.name = name;
		return this;
	}

	public RecipeBuilder SortOrder(int sortOrder)
	{
		this.sortOrder = sortOrder;
		return this;
	}

	public RecipeBuilder Input(Tag tag, float amount, bool inheritElement = true)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		inputs.Add(new RecipeElement(tag, amount, inheritElement));
		return this;
	}

	public RecipeBuilder Input(Tag[] tags, float amountEach = 1f, bool inheritElement = true)
	{
		return Input(tags, Enumerable.Repeat(amountEach, tags.Length).ToArray(), inheritElement);
	}

	public RecipeBuilder Input(Tag[] tags, float[] amounts, bool inheritElement = true)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		inputs.Add(new RecipeElement(tags, amounts)
		{
			inheritElement = inheritElement
		});
		return this;
	}

	public RecipeBuilder Output(Tag tag, float amount, TemperatureOperation tempOp = (TemperatureOperation)0, bool storeElement = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		outputs.Add(new RecipeElement(tag, amount, tempOp, storeElement));
		return this;
	}

	public RecipeBuilder FacadeOutput(Tag tag, float amount, string facadeID = "", bool storeElement = false, TemperatureOperation tempOp = (TemperatureOperation)0)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		outputs.Add(new RecipeElement(tag, amount, tempOp, facadeID, storeElement));
		return this;
	}

	public ComplexRecipe Build(string facadeID = "")
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		RecipeElement[] array = inputs.ToArray();
		RecipeElement[] array2 = outputs.ToArray();
		ComplexRecipe val = new ComplexRecipe(Util.IsNullOrWhiteSpace(facadeID) ? ComplexRecipeManager.MakeRecipeID(fabricator, (IList<RecipeElement>)array, (IList<RecipeElement>)array2) : ComplexRecipeManager.MakeRecipeID(fabricator, (IList<RecipeElement>)array, (IList<RecipeElement>)array2, facadeID), array, array2)
		{
			time = time,
			description = description,
			customName = name,
			nameDisplay = nameDisplay,
			fabricators = new List<Tag> { Tag.op_Implicit(fabricator) },
			requiredTech = requiredTech
		};
		if (requiredDlcIds != null)
		{
			val.SetDLCRestrictions(requiredDlcIds, (string[])null);
		}
		return val;
	}

	public RecipeBuilder RequireDlcs(string[] dlcIds)
	{
		requiredDlcIds = dlcIds;
		return this;
	}
}
