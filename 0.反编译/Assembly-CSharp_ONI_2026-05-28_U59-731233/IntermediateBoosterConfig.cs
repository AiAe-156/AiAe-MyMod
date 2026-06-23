using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class IntermediateBoosterConfig : IEntityConfig
{
	public const string ID = "IntermediateBooster";

	public static ComplexRecipe recipe;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity("IntermediateBooster", STRINGS.ITEMS.PILLS.INTERMEDIATEBOOSTER.NAME, STRINGS.ITEMS.PILLS.INTERMEDIATEBOOSTER.DESC, 1f, unitMass: true, Assets.GetAnim("pill_3_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true, 0, SimHashes.Creature, new List<Tag> { GameTags.PedestalDisplayable });
		EntityTemplates.ExtendEntityToMedicine(gameObject, MEDICINE.INTERMEDIATEBOOSTER);
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(SpiceNutConfig.ID, 1f)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("IntermediateBooster", 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id = ComplexRecipeManager.MakeRecipeID("Apothecary", array, array2);
		recipe = new ComplexRecipe(id, array, array2)
		{
			time = 100f,
			description = STRINGS.ITEMS.PILLS.INTERMEDIATEBOOSTER.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "Apothecary" },
			sortOrder = 5
		};
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
