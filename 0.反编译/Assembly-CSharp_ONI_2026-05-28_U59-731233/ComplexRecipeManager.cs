using System.Collections.Generic;
using System.Text;

public class ComplexRecipeManager
{
	private static ComplexRecipeManager _Instance;

	public HashSet<ComplexRecipe> preProcessRecipes = new HashSet<ComplexRecipe>();

	public List<ComplexRecipe> recipes = new List<ComplexRecipe>();

	private Dictionary<string, string> obsoleteIDMapping = new Dictionary<string, string>();

	public bool IsPostProcessing { get; private set; } = false;

	public static ComplexRecipeManager Get()
	{
		if (_Instance == null)
		{
			_Instance = new ComplexRecipeManager();
		}
		return _Instance;
	}

	public static void DestroyInstance()
	{
		_Instance = null;
	}

	public void PostProcess()
	{
		IsPostProcessing = true;
		foreach (ComplexRecipe preProcessRecipe in preProcessRecipes)
		{
			Get().Add(preProcessRecipe, real: true);
		}
		IsPostProcessing = false;
	}

	public static string MakeObsoleteRecipeID(string fabricator, Tag signatureElement)
	{
		Tag tag = signatureElement;
		return fabricator + "_" + tag.ToString();
	}

	public static string MakeRecipeCategoryID(string fabricator, string categoryName, string productID)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(fabricator);
		stringBuilder.Append("_");
		stringBuilder.Append(categoryName);
		stringBuilder.Append("_");
		stringBuilder.Append(productID);
		return stringBuilder.ToString();
	}

	public static string MakeRecipeID(string fabricator, IList<ComplexRecipe.RecipeElement> inputs, IList<ComplexRecipe.RecipeElement> outputs)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(fabricator);
		stringBuilder.Append("_I");
		foreach (ComplexRecipe.RecipeElement input in inputs)
		{
			stringBuilder.Append("_");
			stringBuilder.Append(input.material.ToString());
		}
		stringBuilder.Append("_O");
		foreach (ComplexRecipe.RecipeElement output in outputs)
		{
			stringBuilder.Append("_");
			stringBuilder.Append(output.material.ToString());
		}
		return stringBuilder.ToString();
	}

	public static string MakeRecipeID(string fabricator, IList<ComplexRecipe.RecipeElement> inputs, IList<ComplexRecipe.RecipeElement> outputs, string facadeID)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(fabricator);
		stringBuilder.Append("_I");
		foreach (ComplexRecipe.RecipeElement input in inputs)
		{
			stringBuilder.Append("_");
			stringBuilder.Append(input.material.ToString());
		}
		stringBuilder.Append("_O");
		foreach (ComplexRecipe.RecipeElement output in outputs)
		{
			stringBuilder.Append("_");
			stringBuilder.Append(output.material.ToString());
		}
		if (!string.IsNullOrEmpty(facadeID))
		{
			stringBuilder.Append("_" + facadeID);
		}
		return stringBuilder.ToString();
	}

	public void Add(ComplexRecipe recipe, bool real)
	{
		recipes.AddRange(DeriveRecipiesFromSource(recipe));
	}

	private List<ComplexRecipe> DeriveRecipiesFromSource(ComplexRecipe sourceRecipe)
	{
		ComplexRecipe.RecipeElement[] ingredients = sourceRecipe.ingredients;
		foreach (ComplexRecipe.RecipeElement recipeElement in ingredients)
		{
			ListPool<Tag, RecipeManager>.PooledList pooledList = ListPool<Tag, RecipeManager>.Allocate();
			ListPool<float, RecipeManager>.PooledList pooledList2 = ListPool<float, RecipeManager>.Allocate();
			for (int j = 0; j < recipeElement.possibleMaterials.Length; j++)
			{
				if (Assets.TryGetPrefab(recipeElement.possibleMaterials[j]) != null)
				{
					pooledList.Add(recipeElement.possibleMaterials[j]);
					pooledList2.Add((recipeElement.possibleMaterialAmounts == null) ? recipeElement.amount : recipeElement.possibleMaterialAmounts[j]);
				}
			}
			recipeElement.possibleMaterials = pooledList.ToArray();
			recipeElement.possibleMaterialAmounts = pooledList2.ToArray();
			pooledList.Recycle();
			pooledList2.Recycle();
		}
		List<ComplexRecipe> list = new List<ComplexRecipe>();
		List<ComplexRecipe.RecipeElement.IngredientDataSet> list2 = new List<ComplexRecipe.RecipeElement.IngredientDataSet>();
		int num = sourceRecipe.ingredients.Length;
		for (int k = 0; k < sourceRecipe.ingredients[0].possibleMaterials.Length; k++)
		{
			list2.Add(new ComplexRecipe.RecipeElement.IngredientDataSet(new Tag[1] { sourceRecipe.ingredients[0].possibleMaterials[k] }, new float[1] { (sourceRecipe.ingredients[0].possibleMaterialAmounts == null) ? sourceRecipe.ingredients[0].amount : sourceRecipe.ingredients[0].possibleMaterialAmounts[k] }));
		}
		for (int l = 1; l < num; l++)
		{
			ComplexRecipe.RecipeElement.IngredientDataSet multiplyAgainst = new ComplexRecipe.RecipeElement.IngredientDataSet(sourceRecipe.ingredients[l].possibleMaterials, sourceRecipe.ingredients[l].possibleMaterialAmounts);
			list2 = MultiplyIngredientDataSets(list2, multiplyAgainst);
		}
		for (int m = 0; m < list2.Count; m++)
		{
			ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[sourceRecipe.ingredients.Length];
			for (int n = 0; n < array.Length; n++)
			{
				array[n] = new ComplexRecipe.RecipeElement(sourceRecipe.ingredients[n].possibleMaterials, sourceRecipe.ingredients[n].possibleMaterialAmounts, sourceRecipe.ingredients[n].temperatureOperation, sourceRecipe.ingredients[n].facadeID, sourceRecipe.ingredients[n].storeElement, sourceRecipe.ingredients[n].inheritElement, sourceRecipe.ingredients[n].doNotConsume);
			}
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				array[num2].possibleMaterials = new Tag[1] { list2[m].substitutionOptions[num2] };
				array[num2].possibleMaterialAmounts = new float[1] { list2[m].amounts[num2] };
				array[num2].material = array[num2].possibleMaterials[0];
				array[num2].amount = array[num2].possibleMaterialAmounts[0];
			}
			string id = MakeRecipeID(sourceRecipe.id.Substring(0, sourceRecipe.id.IndexOf("_")), array, sourceRecipe.results, sourceRecipe.results[0].facadeID);
			ComplexRecipe complexRecipe = new ComplexRecipe(id, array, sourceRecipe.results);
			complexRecipe.consumedHEP = sourceRecipe.consumedHEP;
			complexRecipe.producedHEP = sourceRecipe.producedHEP;
			complexRecipe.requiredTech = sourceRecipe.requiredTech;
			complexRecipe.SetDLCRestrictions(sourceRecipe.GetRequiredDlcIds(), sourceRecipe.GetForbiddenDlcIds());
			complexRecipe.time = sourceRecipe.time;
			complexRecipe.description = sourceRecipe.description;
			complexRecipe.nameDisplay = sourceRecipe.nameDisplay;
			complexRecipe.fabricators = sourceRecipe.fabricators;
			complexRecipe.requiredTech = sourceRecipe.requiredTech;
			complexRecipe.sortOrder = sourceRecipe.sortOrder;
			complexRecipe.runTimeDescription = sourceRecipe.runTimeDescription;
			complexRecipe.customName = sourceRecipe.customName;
			complexRecipe.customSpritePrefabID = sourceRecipe.customSpritePrefabID;
			complexRecipe.ProductHasFacade = sourceRecipe.ProductHasFacade;
			complexRecipe.recipeCategoryID = sourceRecipe.recipeCategoryID;
			complexRecipe.FabricationVisualizer = sourceRecipe.FabricationVisualizer;
			list.Add(complexRecipe);
		}
		return list;
	}

	private List<ComplexRecipe.RecipeElement.IngredientDataSet> MultiplyIngredientDataSets(List<ComplexRecipe.RecipeElement.IngredientDataSet> inputList, ComplexRecipe.RecipeElement.IngredientDataSet multiplyAgainst)
	{
		List<ComplexRecipe.RecipeElement.IngredientDataSet> list = new List<ComplexRecipe.RecipeElement.IngredientDataSet>();
		foreach (ComplexRecipe.RecipeElement.IngredientDataSet input in inputList)
		{
			for (int i = 0; i < multiplyAgainst.substitutionOptions.Length; i++)
			{
				Tag[] array = new Tag[input.substitutionOptions.Length + 1];
				float[] array2 = new float[input.amounts.Length + 1];
				input.substitutionOptions.CopyTo(array, 0);
				input.amounts.CopyTo(array2, 0);
				array[^1] = multiplyAgainst.substitutionOptions[i];
				array2[^1] = multiplyAgainst.amounts[i];
				list.Add(new ComplexRecipe.RecipeElement.IngredientDataSet(array, array2));
			}
		}
		return list;
	}

	public ComplexRecipe GetRecipe(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		ComplexRecipe complexRecipe = recipes.Find((ComplexRecipe r) => r.id == id);
		if (complexRecipe == null)
		{
			foreach (ComplexRecipe preProcessRecipe in preProcessRecipes)
			{
				if (preProcessRecipe.id == id)
				{
					complexRecipe = preProcessRecipe;
				}
			}
		}
		return complexRecipe;
	}

	public List<ComplexRecipe> GetRecipesInCategory(string categoryID)
	{
		return recipes.FindAll((ComplexRecipe r) => r.recipeCategoryID == categoryID);
	}

	public void AddObsoleteIDMapping(string obsolete_id, string new_id)
	{
		obsoleteIDMapping[obsolete_id] = new_id;
	}

	public ComplexRecipe GetObsoleteRecipe(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		ComplexRecipe result = null;
		string value = null;
		if (obsoleteIDMapping.TryGetValue(id, out value))
		{
			result = GetRecipe(value);
		}
		return result;
	}
}
