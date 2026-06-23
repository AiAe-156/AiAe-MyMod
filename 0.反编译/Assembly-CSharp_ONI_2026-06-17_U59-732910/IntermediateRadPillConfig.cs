using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class IntermediateRadPillConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "IntermediateRadPill";

	public static ComplexRecipe recipe;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity("IntermediateRadPill", STRINGS.ITEMS.PILLS.INTERMEDIATERADPILL.NAME, STRINGS.ITEMS.PILLS.INTERMEDIATERADPILL.DESC, 1f, unitMass: true, Assets.GetAnim("vial_radiation_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		gameObject.GetComponent<KPrefabID>().AddTag(GameTags.DeprecatedContent);
		EntityTemplates.ExtendEntityToMedicine(gameObject, MEDICINE.INTERMEDIATERADPILL);
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Carbon", 1f)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("IntermediateRadPill".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		recipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("AdvancedApothecary", array, array2), array, array2)
		{
			time = 50f,
			description = STRINGS.ITEMS.PILLS.INTERMEDIATERADPILL.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "AdvancedApothecary" },
			sortOrder = 21
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
