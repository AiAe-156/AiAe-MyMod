using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtilLibs.MarkdownExport;

public class MD_ComplexRecipes : IMD_Entry
{
	private static StringBuilder sb = new StringBuilder();

	private List<ComplexRecipe> recipes;

	private HashSet<string> processedRecipes = new HashSet<string>();

	public MD_ComplexRecipes(List<ComplexRecipe> recipes)
	{
		this.recipes = recipes;
	}

	public string FormatAsMarkdown()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		processedRecipes.Clear();
		if (recipes == null || !recipes.Any())
		{
			return string.Empty;
		}
		string key = ((object)recipes.First().fabricators[0]/*cast due to .constrained prefix*/).ToString();
		Dictionary<Tag, List<Tag>> value;
		bool flag = Exporter.Instance.RandomRecipeResults.TryGetValue(key, out value);
		Dictionary<Tag, List<Tag>> value2;
		bool flag2 = Exporter.Instance.RandomRecipeOccurences.TryGetValue(key, out value2);
		sb.Clear();
		if (flag)
		{
			sb.Append("|" + MD_Localization.L("RECIPE_INGREDIENTS") + "| " + MD_Localization.L("RECIPE_TIME") + " | " + MD_Localization.L("RECIPE_PRODUCTS_RANDOM"));
		}
		else
		{
			sb.Append("|" + MD_Localization.L("RECIPE_INGREDIENTS") + "| " + MD_Localization.L("RECIPE_TIME") + " | " + MD_Localization.L("RECIPE_PRODUCTS"));
		}
		if (flag2)
		{
			sb.Append("|");
			sb.Append(MD_Localization.L("RECIPE_RANDOM_OCCURENCE"));
		}
		sb.AppendLine("|");
		if (flag2)
		{
			sb.AppendLine("|-|-|-|-|");
		}
		else
		{
			sb.AppendLine("|-|-|-|");
		}
		foreach (ComplexRecipe recipe in recipes)
		{
			if (processedRecipes.Contains(recipe.id))
			{
				continue;
			}
			sb.Append("|");
			RecipeElement[] ingredients = recipe.ingredients;
			foreach (RecipeElement val in ingredients)
			{
				sb.Append(MarkdownUtil.GetFormattedMass(val.material, val.amount, (TimeSlice)0));
				sb.Append("<br>");
			}
			if (recipe.consumedHEP > 0)
			{
				sb.Append(MarkdownUtil.FormatRadbolts(recipe.consumedHEP));
				sb.Append("<br>");
			}
			sb.Append("|");
			sb.Append(GameUtil.GetFormattedTime(recipe.time, "F0"));
			sb.Append("|");
			if (flag && value.TryGetValue(recipe.ingredients[0].material, out var value3) && value3.Any())
			{
				foreach (Tag item in value3)
				{
					sb.Append(MarkdownUtil.GetTagStringWithIcon(item));
					sb.Append("<br>");
				}
			}
			else
			{
				RecipeElement[] results = recipe.results;
				foreach (RecipeElement val2 in results)
				{
					sb.Append(MarkdownUtil.GetFormattedMass(val2.material, val2.amount, (TimeSlice)0));
					sb.Append("<br>");
				}
			}
			if (flag2 && value2.TryGetValue(recipe.ingredients[0].material, out var value4))
			{
				sb.Append("|");
				foreach (Tag item2 in value4)
				{
					sb.Append(MarkdownUtil.GetTagStringWithIcon(item2));
					sb.Append("<br>");
				}
			}
			sb.AppendLine("|");
			processedRecipes.Add(recipe.id);
		}
		sb.AppendLine();
		return sb.ToString();
	}
}
