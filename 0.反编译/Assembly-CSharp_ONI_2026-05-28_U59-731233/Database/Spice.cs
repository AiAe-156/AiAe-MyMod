using Klei.AI;
using UnityEngine;

namespace Database;

public class Spice : Resource, IHasDlcRestrictions
{
	public class Ingredient : IConfigurableConsumerIngredient
	{
		public Tag[] IngredientSet = null;

		public float AmountKG = 0f;

		public float GetAmount()
		{
			return AmountKG;
		}

		public Tag[] GetIDSets()
		{
			return IngredientSet;
		}
	}

	public readonly Ingredient[] Ingredients;

	public readonly float TotalKG;

	public AttributeModifier StatBonus { get; private set; }

	public AttributeModifier FoodModifier { get; private set; }

	public AttributeModifier CalorieModifier { get; private set; }

	public Color PrimaryColor { get; private set; }

	public Color SecondaryColor { get; private set; }

	public string Image { get; private set; }

	public string[] requiredDlcIds { get; private set; }

	public Spice(ResourceSet parent, string id, Ingredient[] ingredients, Color primaryColor, Color secondaryColor, AttributeModifier foodMod = null, AttributeModifier statBonus = null, string imageName = "unknown", string[] dlcID = null)
		: base(id, parent)
	{
		requiredDlcIds = requiredDlcIds;
		StatBonus = statBonus;
		FoodModifier = foodMod;
		Ingredients = ingredients;
		Image = imageName;
		PrimaryColor = primaryColor;
		SecondaryColor = secondaryColor;
		for (int i = 0; i < Ingredients.Length; i++)
		{
			TotalKG += Ingredients[i].AmountKG;
		}
	}

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}
}
