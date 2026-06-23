using System.Collections.Generic;
using Database;
using ImGuiNET;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class DevToolEntityDebug : DevTool
{
	private bool lockSelection;

	private GameObject lockedObject;

	private Dictionary<string, bool> expandedAttributes = new Dictionary<string, bool>();

	public DevToolEntityDebug()
	{
		RequiresGameRunning = true;
	}

	protected override void RenderTo(DevPanel panel)
	{
		if (SelectTool.Instance == null)
		{
			ImGui.Text("SelectTool not available.");
			return;
		}
		ImGui.Checkbox("Lock Selection", ref lockSelection);
		GameObject gameObject = null;
		if (lockSelection && lockedObject != null)
		{
			gameObject = lockedObject;
		}
		else
		{
			KSelectable selected = SelectTool.Instance.selected;
			if (!selected.IsNullOrDestroyed())
			{
				gameObject = selected.gameObject;
				if (lockSelection)
				{
					lockedObject = gameObject;
				}
			}
		}
		if (gameObject == null)
		{
			ImGui.Text("Nothing selected.");
			return;
		}
		Modifiers component = gameObject.GetComponent<Modifiers>();
		if (component == null)
		{
			ImGui.Text("Selected object has no Modifiers component.");
			return;
		}
		Name = "Entity Debug: " + gameObject.name;
		if (GameClock.Instance != null)
		{
			float num = GameClock.Instance.GetTime() / 600f;
			ImGui.Text($"GameTime: {num:F2} cycles");
		}
		ImGui.Separator();
		DrawAmounts(component);
		DrawEffects(component);
		DrawAttributes(component);
		DrawAttributeLevels(component);
		DrawTraits(component);
		DrawDeaths(component);
		DrawUrges(component);
		DrawDiseases(component);
		DrawSicknesses(component);
		DrawResume(component);
		DrawStomach(component);
	}

	private void DrawAmounts(Modifiers entity)
	{
		if (entity.GetAmounts() == null || !ImGui.CollapsingHeader("Amounts (Min/Max/Delta)"))
		{
			return;
		}
		List<AmountInstance> list = new List<AmountInstance>(entity.GetAmounts().ModifierList);
		list.Sort((AmountInstance x, AmountInstance y) => x.amount.Id.CompareTo(y.amount.Id));
		foreach (AmountInstance item in list)
		{
			string label = $"{item.amount.Id} ({item.GetMin()}/{item.GetMax()}/{item.GetDelta():F2})";
			float v = item.value;
			if (ImGui.DragFloat(label, ref v, 0.1f, item.GetMin(), item.GetMax()))
			{
				item.amount.DebugSetValue(item, v);
			}
		}
	}

	private void DrawEffects(Modifiers entity)
	{
		Effects component = entity.GetComponent<Effects>();
		if (component == null || !ImGui.CollapsingHeader("Effects"))
		{
			return;
		}
		List<Effect> list = new List<Effect>(Db.Get().effects.resources);
		list.Sort((Effect x, Effect y) => x.Name.CompareTo(y.Name));
		foreach (Effect item in list)
		{
			if (item == null)
			{
				continue;
			}
			bool v = component.HasEffect(item);
			if (ImGui.Checkbox(item.Name, ref v))
			{
				if (v)
				{
					component.Add(item, should_save: false);
				}
				else
				{
					component.Remove(item);
				}
			}
			if (v)
			{
				ImGui.SameLine();
				EffectInstance effectInstance = component.Get(item);
				float v2 = effectInstance.timeRemaining;
				ImGui.SetNextItemWidth(100f);
				if (ImGui.DragFloat("##time_" + item.Id, ref v2, 0.1f, 0f, float.MaxValue, "%.1f"))
				{
					effectInstance.timeRemaining = v2;
				}
			}
		}
	}

	private void DrawAttributes(Modifiers entity)
	{
		if (entity.GetAttributes() == null || !ImGui.CollapsingHeader("Attributes"))
		{
			return;
		}
		foreach (AttributeInstance attribute in entity.GetAttributes())
		{
			float totalValue = attribute.GetTotalValue();
			string fmt = $"{attribute.Attribute.Id}: {totalValue} ({totalValue * 600f}/cycle)";
			if (attribute.Modifiers.Count > 0)
			{
				bool value = false;
				expandedAttributes.TryGetValue(attribute.Attribute.Id, out value);
				if (ImGui.TreeNode(attribute.Attribute.Id, fmt))
				{
					expandedAttributes[attribute.Attribute.Id] = true;
					for (int i = 0; i < attribute.Modifiers.Count; i++)
					{
						AttributeModifier attributeModifier = attribute.Modifiers[i];
						string text = (attributeModifier.IsMultiplier ? " x " : "");
						ImGui.Text($"  {attributeModifier.GetDescription()}: {text}{attributeModifier.Value} ({attributeModifier.Value * 600f}/cycle)");
					}
					ImGui.TreePop();
				}
				else
				{
					expandedAttributes[attribute.Attribute.Id] = false;
				}
			}
			else
			{
				ImGui.Text(fmt);
			}
		}
	}

	private void DrawAttributeLevels(Modifiers entity)
	{
		AttributeLevels component = entity.GetComponent<AttributeLevels>();
		if (component == null || !ImGui.CollapsingHeader("Attribute Levels"))
		{
			return;
		}
		foreach (AttributeLevel item in component)
		{
			string text = $"{item.attribute.Attribute.Id} Lv{item.GetLevel()}";
			string text2 = $"{item.experience:F0}/{item.GetExperienceForNextLevel():F0} ({item.GetPercentComplete():F3})";
			ImGui.Text(text + ": " + text2);
			ImGui.SameLine();
			if (ImGui.SmallButton("+##" + item.attribute.Attribute.Id))
			{
				item.LevelUp(component);
			}
		}
	}

	private void DrawTraits(Modifiers entity)
	{
		Traits component = entity.GetComponent<Traits>();
		if (component == null || !ImGui.CollapsingHeader("Traits"))
		{
			return;
		}
		List<Trait> list = new List<Trait>(Db.Get().traits.resources);
		list.Sort((Trait a, Trait b) => UI.StripLinkFormatting(a.Name).CompareTo(UI.StripLinkFormatting(b.Name)));
		foreach (Trait item in list)
		{
			bool v = component.HasTrait(item);
			if (ImGui.Checkbox(UI.StripLinkFormatting(item.Name), ref v))
			{
				if (v)
				{
					component.Add(item);
				}
				else
				{
					component.Remove(item);
				}
			}
		}
	}

	private void DrawDeaths(Modifiers entity)
	{
		DeathMonitor.Instance sMI = entity.GetSMI<DeathMonitor.Instance>();
		if (sMI == null || !ImGui.CollapsingHeader("Deaths"))
		{
			return;
		}
		foreach (Death resource in Db.Get().Deaths.resources)
		{
			if (ImGui.Button(resource.Id))
			{
				sMI.Kill(resource);
			}
		}
	}

	private void DrawUrges(Modifiers entity)
	{
		ChoreConsumer component = entity.GetComponent<ChoreConsumer>();
		if (component == null || !ImGui.CollapsingHeader("Urges"))
		{
			return;
		}
		foreach (Urge resource in Db.Get().Urges.resources)
		{
			bool v = component.HasUrge(resource);
			if (ImGui.Checkbox(resource.Name, ref v))
			{
				if (v)
				{
					component.AddUrge(resource);
				}
				else
				{
					component.RemoveUrge(resource);
				}
			}
		}
	}

	private void DrawDiseases(Modifiers entity)
	{
		if (!ImGui.CollapsingHeader("Diseases"))
		{
			return;
		}
		Diseases diseases = Db.Get().Diseases;
		PrimaryElement component = entity.gameObject.GetComponent<PrimaryElement>();
		for (int i = 0; i < diseases.Count; i++)
		{
			Disease disease = diseases[i];
			int num = ((component.DiseaseIdx == i) ? component.DiseaseCount : 0);
			string arg = Util.StripTextFormatting(disease.Name);
			ImGui.Text($"{arg}: {num}");
			ImGui.SameLine();
			if (ImGui.SmallButton("Add 100##" + disease.Id))
			{
				component.AddDisease((byte)i, 100, "debug");
			}
		}
	}

	private void DrawSicknesses(Modifiers entity)
	{
		MinionModifiers component = entity.GetComponent<MinionModifiers>();
		if (component == null || !ImGui.CollapsingHeader("Sicknesses"))
		{
			return;
		}
		Database.Sicknesses sicknesses = Db.Get().Sicknesses;
		Klei.AI.Sicknesses sicknesses2 = component.sicknesses;
		for (int i = 0; i < sicknesses.Count; i++)
		{
			Sickness sickness = sicknesses[i];
			string text = Util.StripTextFormatting(sickness.Name);
			SicknessInstance sicknessInstance = sicknesses2.Get(sickness);
			bool v = sicknessInstance != null;
			if (ImGui.Checkbox(text + "##sick", ref v))
			{
				if (v)
				{
					sicknesses2.Infect(new SicknessExposureInfo(sickness.Id, "debug menu"));
				}
				else
				{
					sicknessInstance.Cure();
				}
			}
			if (v && sicknessInstance != null)
			{
				ImGui.SameLine();
				float v2 = sicknessInstance.GetPercentCured();
				ImGui.SetNextItemWidth(100f);
				if (ImGui.DragFloat("##cure_" + sickness.Id, ref v2, 0.01f, 0f, 1f, "%.2f"))
				{
					sicknessInstance.SetPercentCured(v2);
				}
			}
		}
	}

	private void DrawResume(Modifiers entity)
	{
		MinionResume component = entity.GetComponent<MinionResume>();
		if (component == null || !ImGui.CollapsingHeader("Resume"))
		{
			return;
		}
		float v = component.TotalExperienceGained;
		if (ImGui.DragFloat("Total Experience", ref v, 1f))
		{
			component.AddExperience(v - component.TotalExperienceGained);
		}
		ImGui.Text($"Next Level: {MinionResume.CalculateNextExperienceBar(component.TotalSkillPointsGained)}");
		ImGui.Text($"Total Skill Points: {component.TotalSkillPointsGained}");
		ImGui.SameLine();
		if (ImGui.SmallButton("+##skillpoint"))
		{
			component.ForceAddSkillPoint();
		}
		ImGui.Separator();
		List<Skill> list = new List<Skill>(Db.Get().Skills.resources);
		foreach (Skill item in list)
		{
			bool v2 = component.MasteryBySkillID.ContainsKey(item.Id) && component.MasteryBySkillID[item.Id];
			if (ImGui.Checkbox(UI.StripLinkFormatting(item.Name) + "##skill_" + item.Id, ref v2))
			{
				if (v2)
				{
					component.MasterSkill(item.Id);
				}
				else
				{
					component.UnmasterSkill(item.Id);
				}
			}
		}
	}

	private void DrawStomach(Modifiers entity)
	{
		CreatureCalorieMonitor.Instance sMI = entity.GetSMI<CreatureCalorieMonitor.Instance>();
		if (sMI == null || !ImGui.CollapsingHeader("Stomach"))
		{
			return;
		}
		CreatureCalorieMonitor.Stomach stomach = sMI.stomach;
		ImGui.Text($"Fullness: {stomach.GetFullness()}");
		ImGui.Text($"Hunger {sMI.calories.GetMax() * sMI.HungryRatio}: {(sMI.calories.GetMax() - sMI.calories.value) / (sMI.calories.GetMax() * (1f - sMI.HungryRatio))}");
		List<CreatureCalorieMonitor.Stomach.CaloriesConsumedEntry> calorieEntries = stomach.GetCalorieEntries();
		for (int i = 0; i < calorieEntries.Count; i++)
		{
			CreatureCalorieMonitor.Stomach.CaloriesConsumedEntry value = calorieEntries[i];
			float v = value.calories;
			if (ImGui.DragFloat(value.tag.Name, ref v, 0.1f))
			{
				value.calories = v;
				calorieEntries[i] = value;
			}
		}
	}
}
