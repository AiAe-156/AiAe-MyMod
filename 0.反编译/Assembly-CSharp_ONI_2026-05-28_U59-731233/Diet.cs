using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Diet
{
	public class Info
	{
		public enum FoodType
		{
			EatSolid,
			EatPlantDirectly,
			EatPlantStorage,
			EatPrey,
			EatButcheredPrey
		}

		public HashSet<Tag> consumedTags { get; private set; }

		public Tag producedElement { get; private set; }

		public float caloriesPerKg { get; private set; }

		public float producedConversionRate { get; private set; }

		public byte diseaseIdx { get; private set; }

		public float diseasePerKgProduced { get; private set; }

		public bool emmitDiseaseOnCell { get; private set; }

		public bool produceSolidTile { get; private set; }

		public FoodType foodType { get; private set; }

		public string[] eatAnims { get; set; }

		public Info(HashSet<Tag> consumed_tags, Tag produced_element, float calories_per_kg, float produced_conversion_rate = 1f, string disease_id = null, float disease_per_kg_produced = 0f, bool produce_solid_tile = false, FoodType food_type = FoodType.EatSolid, bool emmit_disease_on_cell = false, string[] eat_anims = null)
		{
			consumedTags = consumed_tags;
			producedElement = produced_element;
			caloriesPerKg = calories_per_kg;
			producedConversionRate = produced_conversion_rate;
			if (!string.IsNullOrEmpty(disease_id))
			{
				diseaseIdx = Db.Get().Diseases.GetIndex(disease_id);
			}
			else
			{
				diseaseIdx = byte.MaxValue;
			}
			diseasePerKgProduced = disease_per_kg_produced;
			emmitDiseaseOnCell = emmit_disease_on_cell;
			produceSolidTile = produce_solid_tile;
			foodType = food_type;
			if (eat_anims == null)
			{
				eat_anims = new string[3] { "eat_pre", "eat_loop", "eat_pst" };
			}
			eatAnims = eat_anims;
		}

		public Info(Info info)
		{
			consumedTags = new HashSet<Tag>(info.consumedTags);
			producedElement = info.producedElement;
			caloriesPerKg = info.caloriesPerKg;
			producedConversionRate = info.producedConversionRate;
			diseaseIdx = info.diseaseIdx;
			diseasePerKgProduced = info.diseasePerKgProduced;
			emmitDiseaseOnCell = info.emmitDiseaseOnCell;
			produceSolidTile = info.produceSolidTile;
			foodType = info.foodType;
			eatAnims = info.eatAnims;
		}

		public bool IsMatch(Tag tag)
		{
			return consumedTags.Contains(tag);
		}

		public bool IsMatch(HashSet<Tag> tags)
		{
			if (tags.Count < consumedTags.Count)
			{
				foreach (Tag tag in tags)
				{
					if (consumedTags.Contains(tag))
					{
						return true;
					}
				}
				return false;
			}
			foreach (Tag consumedTag in consumedTags)
			{
				if (tags.Contains(consumedTag))
				{
					return true;
				}
			}
			return false;
		}

		public float ConvertCaloriesToConsumptionMass(float calories)
		{
			return calories / caloriesPerKg;
		}

		public float ConvertConsumptionMassToCalories(float mass)
		{
			return caloriesPerKg * mass;
		}

		public float ConvertConsumptionMassToProducedMass(float consumed_mass)
		{
			return consumed_mass * producedConversionRate;
		}

		public float ConvertProducedMassToConsumptionMass(float produced_mass)
		{
			return produced_mass / producedConversionRate;
		}
	}

	public List<KeyValuePair<Tag, float>> consumedTags;

	public List<KeyValuePair<Tag, float>> producedTags;

	private Dictionary<Tag, Info> consumedTagToInfo = new Dictionary<Tag, Info>();

	public Info[] infos { get; private set; }

	public Info[] solidEdiblesInfo { get; private set; }

	public Info[] directlyEatenPlantInfos { get; private set; }

	public Info[] preyInfos { get; private set; }

	public bool CanEatAnySolid => solidEdiblesInfo != null && solidEdiblesInfo.Length != 0;

	public bool CanEatAnyPlantDirectly => directlyEatenPlantInfos != null && directlyEatenPlantInfos.Length != 0;

	public bool CanEatPreyCritter => preyInfos != null && preyInfos.Length != 0;

	public bool IsConsumedTagAbleToBeEatenDirectly(Tag tag)
	{
		if (directlyEatenPlantInfos == null && preyInfos == null)
		{
			return false;
		}
		for (int i = 0; i < directlyEatenPlantInfos.Length; i++)
		{
			if (directlyEatenPlantInfos[i].consumedTags.Contains(tag))
			{
				return true;
			}
		}
		for (int j = 0; j < preyInfos.Length; j++)
		{
			if (preyInfos[j].consumedTags.Contains(tag))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateSecondaryInfoArrays()
	{
		directlyEatenPlantInfos = ((infos == null) ? null : infos.Where((Info i) => i.foodType == Info.FoodType.EatPlantDirectly || i.foodType == Info.FoodType.EatPlantStorage).ToArray());
		solidEdiblesInfo = ((infos == null) ? null : infos.Where((Info i) => i.foodType == Info.FoodType.EatSolid).ToArray());
		preyInfos = ((infos == null) ? null : infos.Where((Info i) => i.foodType == Info.FoodType.EatPrey || i.foodType == Info.FoodType.EatButcheredPrey).ToArray());
	}

	public Diet(params Info[] infos)
	{
		this.infos = infos;
		consumedTags = new List<KeyValuePair<Tag, float>>();
		producedTags = new List<KeyValuePair<Tag, float>>();
		foreach (Info info in infos)
		{
			foreach (Tag tag in info.consumedTags)
			{
				if (-1 == consumedTags.FindIndex((KeyValuePair<Tag, float> e) => e.Key == tag))
				{
					consumedTags.Add(new KeyValuePair<Tag, float>(tag, info.caloriesPerKg));
				}
				if (consumedTagToInfo.ContainsKey(tag))
				{
					Tag tag2 = tag;
					Debug.LogError("Duplicate diet entry: " + tag2.ToString());
				}
				consumedTagToInfo[tag] = info;
			}
			if (info.producedElement != Tag.Invalid && -1 == producedTags.FindIndex((KeyValuePair<Tag, float> e) => e.Key == info.producedElement))
			{
				producedTags.Add(new KeyValuePair<Tag, float>(info.producedElement, info.producedConversionRate));
			}
		}
		UpdateSecondaryInfoArrays();
	}

	public Diet(Diet diet)
	{
		infos = new Info[diet.infos.Length];
		for (int i = 0; i < diet.infos.Length; i++)
		{
			infos[i] = new Info(diet.infos[i]);
		}
		consumedTags = new List<KeyValuePair<Tag, float>>();
		producedTags = new List<KeyValuePair<Tag, float>>();
		Info[] array = infos;
		foreach (Info info in array)
		{
			foreach (Tag tag in info.consumedTags)
			{
				if (-1 == consumedTags.FindIndex((KeyValuePair<Tag, float> e) => e.Key == tag))
				{
					consumedTags.Add(new KeyValuePair<Tag, float>(tag, info.caloriesPerKg));
				}
				if (consumedTagToInfo.ContainsKey(tag))
				{
					Tag tag2 = tag;
					Debug.LogError("Duplicate diet entry: " + tag2.ToString());
				}
				consumedTagToInfo[tag] = info;
			}
			if (info.producedElement != Tag.Invalid && -1 == producedTags.FindIndex((KeyValuePair<Tag, float> e) => e.Key == info.producedElement))
			{
				producedTags.Add(new KeyValuePair<Tag, float>(info.producedElement, info.producedConversionRate));
			}
		}
		UpdateSecondaryInfoArrays();
	}

	public Info GetDietInfo(Tag tag)
	{
		Info value = null;
		consumedTagToInfo.TryGetValue(tag, out value);
		return value;
	}

	public float AvailableCaloriesInPrey(Tag tag)
	{
		Info dietInfo = GetDietInfo(tag);
		if (dietInfo == null)
		{
			return 0f;
		}
		GameObject prefab = Assets.GetPrefab(tag);
		if (dietInfo.foodType == Info.FoodType.EatPrey)
		{
			return prefab.GetComponent<PrimaryElement>().Mass * dietInfo.caloriesPerKg;
		}
		Butcherable component = prefab.GetComponent<Butcherable>();
		float num = 0f;
		if (component == null)
		{
			return 0f;
		}
		foreach (KeyValuePair<string, float> drop in component.drops)
		{
			Info dietInfo2 = GetDietInfo(new Tag(drop.Key));
			if (dietInfo2 != null)
			{
				num += drop.Value * dietInfo2.caloriesPerKg;
			}
		}
		return num;
	}

	public void FilterDLC()
	{
		Info[] array = infos;
		foreach (Info info in array)
		{
			List<Tag> list = new List<Tag>();
			foreach (Tag consumedTag in info.consumedTags)
			{
				GameObject prefab = Assets.GetPrefab(consumedTag);
				if (prefab == null || !Game.IsCorrectDlcActiveForCurrentSave(prefab.GetComponent<KPrefabID>()))
				{
					list.Add(consumedTag);
				}
			}
			foreach (Tag invalid_tag in list)
			{
				info.consumedTags.Remove(invalid_tag);
				consumedTags.RemoveAll((KeyValuePair<Tag, float> t) => t.Key == invalid_tag);
				consumedTagToInfo.Remove(invalid_tag);
			}
			if (info.producedElement != Tag.Invalid)
			{
				GameObject prefab2 = Assets.GetPrefab(info.producedElement);
				if (prefab2 == null || !Game.IsCorrectDlcActiveForCurrentSave(prefab2.GetComponent<KPrefabID>()))
				{
					info.consumedTags.Clear();
				}
			}
		}
		infos = infos.Where((Info info2) => info2.consumedTags.Count > 0).ToArray();
		UpdateSecondaryInfoArrays();
	}
}
