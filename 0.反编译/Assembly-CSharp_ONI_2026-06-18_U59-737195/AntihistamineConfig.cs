using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class AntihistamineConfig : IEntityConfig
{
	public const string ID = "Antihistamine";

	public static List<ComplexRecipe> recipes = new List<ComplexRecipe>();

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity("Antihistamine", STRINGS.ITEMS.PILLS.ANTIHISTAMINE.NAME, STRINGS.ITEMS.PILLS.ANTIHISTAMINE.DESC, 1f, unitMass: true, Assets.GetAnim("pill_allergies_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		EntityTemplates.ExtendEntityToMedicine(gameObject, MEDICINE.ANTIHISTAMINE);
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Antihistamine", 10f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				"PrickleFlowerSeed",
				KelpConfig.ID
			}, new float[2] { 1f, 10f }),
			new ComplexRecipe.RecipeElement(SimHashes.Dirt.CreateTag(), 1f)
		};
		string recipeID = ComplexRecipeManager.MakeRecipeID("Apothecary", array2, array);
		recipes.Add(CreateComplexRecipe(recipeID, array2, array));
		return gameObject;
	}

	public ComplexRecipe CreateComplexRecipe(string recipeID, ComplexRecipe.RecipeElement[] input, ComplexRecipe.RecipeElement[] output)
	{
		return new ComplexRecipe(recipeID, input, output)
		{
			time = 100f,
			description = STRINGS.ITEMS.PILLS.ANTIHISTAMINE.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "Apothecary" },
			sortOrder = 10
		};
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
