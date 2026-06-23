using System.Collections.Generic;
using System.Text;
using ImGuiNET;
using UnityEngine;

public class DevToolFuzzy : DevTool
{
	private string searchText = "";

	private float mostRecentEditTime;

	private bool refresh;

	private const float REFRESH_DELAY = 0.4f;

	private int scoreThreshold = 79;

	private readonly List<SearchUtil.TechCache> techs = new List<SearchUtil.TechCache>();

	private readonly List<SearchUtil.BuildingDefCache> buildingDefs = new List<SearchUtil.BuildingDefCache>();

	public DevToolFuzzy()
	{
		mostRecentEditTime = Time.unscaledTime;
	}

	private void RecipesUi(StringBuilder sb, string id, List<SearchUtil.NameDescCache> recipes)
	{
		int num = 0;
		foreach (SearchUtil.NameDescCache recipe in recipes)
		{
			if (recipe.Score > num)
			{
				num = recipe.Score;
			}
		}
		if (!IsPassingScore(num))
		{
			return;
		}
		sb.Clear();
		sb.AppendFormat("[{0}] Recipes##{1}", num, id);
		if (!ImGui.CollapsingHeader(sb.ToString()))
		{
			return;
		}
		ImGui.Indent();
		foreach (SearchUtil.NameDescCache recipe2 in recipes)
		{
			if (IsPassingScore(recipe2.Score))
			{
				sb.Clear();
				sb.AppendFormat("{0}##{1}", FormatScoreDisplay(recipe2.Score, recipe2.name.text), id);
				if (ImGui.CollapsingHeader(sb.ToString()))
				{
					DisplayIfScorePasses(recipe2);
				}
			}
		}
		ImGui.Unindent();
	}

	private void TechItemUi(StringBuilder sb, string id, SearchUtil.TechItemCache techItem, SearchUtil.TechCache parentTech = null)
	{
		if (!IsPassingScore(techItem.Score))
		{
			return;
		}
		sb.Clear();
		sb.AppendFormat("{0}##TechItem{1}", FormatScoreDisplay(techItem.Score, techItem.nameDescSearchTerms.nameDesc.name.text), id);
		string text = sb.ToString();
		if (ImGui.CollapsingHeader(text))
		{
			ImGui.Indent();
			if (parentTech != null)
			{
				ImGui.LabelText("Parent Tech", parentTech.tech.nameDesc.name.text);
			}
			DisplayIfScorePasses(techItem.nameDescSearchTerms);
			RecipesUi(sb, text, techItem.recipes);
			ImGui.Unindent();
		}
	}

	protected override void RenderTo(DevPanel panel)
	{
		if (ImGui.InputText("Search Text", ref searchText, 30u))
		{
			refresh = true;
			mostRecentEditTime = Time.unscaledTime;
		}
		if (refresh && Time.unscaledTime - mostRecentEditTime > 0.4f)
		{
			Refresh();
			refresh = false;
		}
		ImGui.DragInt("Score Threshold", ref scoreThreshold, 0.5f, 0, 100);
		StringBuilder stringBuilder = new StringBuilder();
		if (ImGui.CollapsingHeader("Techs"))
		{
			ImGui.Indent();
			foreach (SearchUtil.TechCache tech in techs)
			{
				if (!IsPassingScore(tech.Score))
				{
					continue;
				}
				stringBuilder.Clear();
				stringBuilder.AppendFormat("{0}##Tech", FormatScoreDisplay(tech.Score, tech.tech.nameDesc.name.text));
				string text = stringBuilder.ToString();
				if (!ImGui.CollapsingHeader(text))
				{
					continue;
				}
				ImGui.Indent();
				DisplayIfScorePasses(tech.tech);
				foreach (KeyValuePair<string, SearchUtil.TechItemCache> techItem in tech.techItems)
				{
					TechItemUi(stringBuilder, text, techItem.Value);
				}
				ImGui.Unindent();
			}
			ImGui.Unindent();
		}
		if (ImGui.CollapsingHeader("TechItems"))
		{
			ImGui.Indent();
			foreach (SearchUtil.TechCache tech2 in techs)
			{
				foreach (KeyValuePair<string, SearchUtil.TechItemCache> techItem2 in tech2.techItems)
				{
					TechItemUi(stringBuilder, "TechItem", techItem2.Value, tech2);
				}
			}
			ImGui.Unindent();
		}
		if (!ImGui.CollapsingHeader("BuildingDefs"))
		{
			return;
		}
		ImGui.Indent();
		foreach (SearchUtil.BuildingDefCache buildingDef in buildingDefs)
		{
			if (IsPassingScore(buildingDef.Score))
			{
				stringBuilder.Clear();
				stringBuilder.AppendFormat("{0}##BuildingDef", FormatScoreDisplay(buildingDef.Score, buildingDef.nameDescSearchTerms.nameDesc.name.text));
				string text2 = stringBuilder.ToString();
				if (ImGui.CollapsingHeader(text2))
				{
					ImGui.Indent();
					DisplayIfScorePasses(buildingDef.nameDescSearchTerms);
					DisplayIfScorePasses("Effect", buildingDef.effect);
					RecipesUi(stringBuilder, text2, buildingDef.recipes);
					ImGui.Unindent();
				}
			}
		}
		ImGui.Unindent();
	}

	private void Refresh()
	{
		string text = searchText.ToUpper().Trim();
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (techs.Count == 0)
		{
			foreach (KeyValuePair<string, SearchUtil.TechCache> item in SearchUtil.CacheTechs())
			{
				techs.Add(item.Value);
			}
		}
		foreach (SearchUtil.TechCache tech in techs)
		{
			tech.Bind(text);
		}
		techs.Sort();
		if (buildingDefs.Count == 0)
		{
			foreach (BuildingDef buildingDef in Assets.BuildingDefs)
			{
				buildingDefs.Add(SearchUtil.MakeBuildingDefCache(buildingDef));
			}
		}
		foreach (SearchUtil.BuildingDefCache buildingDef2 in buildingDefs)
		{
			buildingDef2.Bind(text);
		}
		buildingDefs.Sort();
	}

	private bool IsPassingScore(int score)
	{
		return score >= scoreThreshold;
	}

	private static string FormatScoreDisplay(int score, string text)
	{
		return $"[{score}] {FuzzySearch.Canonicalize(text)}";
	}

	private static void DisplayScore(int score, string label, string token, string text)
	{
		ImGui.Text($"[{score}]");
		ImGui.SameLine();
		ImGui.Text(label);
		ImGui.SameLine();
		ImGui.Text($"({token})");
		ImGui.SameLine();
		ImGui.TextWrapped(text);
	}

	private static void DisplayScore(string label, SearchUtil.MatchCache match)
	{
		DisplayScore(match.Score, label, match.FuzzyMatch.token, match.text);
	}

	private void DisplayIfScorePasses(string label, SearchUtil.MatchCache match)
	{
		if (IsPassingScore(match.Score))
		{
			DisplayScore(label, match);
		}
	}

	private void DisplayIfScorePasses(SearchUtil.NameDescCache nameDesc)
	{
		DisplayIfScorePasses("Name", nameDesc.name);
		DisplayIfScorePasses("Desc", nameDesc.desc);
	}

	private void DisplayIfScorePasses(SearchUtil.NameDescSearchTermsCache nameDescSearchTerms)
	{
		DisplayIfScorePasses(nameDescSearchTerms.nameDesc);
		if (IsPassingScore(nameDescSearchTerms.SearchTermsScore.score))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendJoin(", ", nameDescSearchTerms.searchTerms);
			DisplayScore(nameDescSearchTerms.SearchTermsScore.score, "SearchTerms", nameDescSearchTerms.SearchTermsScore.token, stringBuilder.ToString());
		}
	}
}
