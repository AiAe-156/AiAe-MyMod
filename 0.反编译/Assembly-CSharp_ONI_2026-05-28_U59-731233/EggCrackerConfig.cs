using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

[EntityConfigOrder(2)]
public class EggCrackerConfig : IBuildingConfig
{
	public class EggData : IHasDlcRestrictions
	{
		public Tag id;

		public float mass;

		public string name;

		public string description;

		public string[] requiredDlcIds;

		public string[] forbiddenDlcIds;

		public bool hasCrackerRecipe;

		public Tuple<Tag, float>[] customOutput;

		public float customShellRatio = -1f;

		public bool isBaseMorph;

		public EggData(Tag id, string name, string description, float mass, string[] requiredDLC, string[] forbiddenDLC)
		{
			Config(id, name, description, mass, requiredDLC, forbiddenDLC);
		}

		public EggData(Tag id, string name, string description, float mass, string[] requiredDLC, string[] forbiddenDLC, bool hasCrackerRecipe = true)
		{
			Config(id, name, description, mass, requiredDLC, forbiddenDLC, hasCrackerRecipe);
		}

		public EggData(Tag id, string name, string description, float mass, string[] requiredDLC, string[] forbiddenDLC, bool hasCrackerRecipe = true, float customShellRatio = -1f)
		{
			Config(id, name, description, mass, requiredDLC, forbiddenDLC, hasCrackerRecipe, customShellRatio);
		}

		private void Config(Tag id, string name, string description, float mass, string[] requiredDLC, string[] forbiddenDLC, bool hasCrackerRecipe = true, float customShellRatio = -1f)
		{
			this.id = id;
			this.name = name;
			this.description = description;
			this.mass = mass;
			requiredDlcIds = requiredDLC;
			forbiddenDlcIds = forbiddenDLC;
			this.hasCrackerRecipe = hasCrackerRecipe;
			this.customShellRatio = customShellRatio;
		}

		public string[] GetRequiredDlcIds()
		{
			return requiredDlcIds;
		}

		public string[] GetForbiddenDlcIds()
		{
			return forbiddenDlcIds;
		}
	}

	public const string ID = "EggCracker";

	public static Dictionary<Tag, List<EggData>> EggsBySpecies = new Dictionary<Tag, List<EggData>>();

	private static List<EggData> uncategorizedEggData = new List<EggData>();

	public static void RegisterEgg(Tag eggPrefabTag, string name, string description, float mass, string[] requiredDLC, string[] forbiddenDLC)
	{
		RegisterEgg(eggPrefabTag, name, description, mass, requiredDLC, forbiddenDLC, null);
	}

	public static void RegisterEgg(Tag eggPrefabTag, string name, string description, float mass, string[] requiredDLC, string[] forbiddenDLC, Tuple<Tag, float>[] customDrops)
	{
		RegisterEgg(eggPrefabTag, name, description, mass, requiredDLC, forbiddenDLC, customDrops, 0.5f);
	}

	public static void RegisterEgg(Tag eggPrefabTag, string name, string description, float mass, string[] requiredDLC, string[] forbiddenDLC, Tuple<Tag, float>[] customDrops, float customShellRatio, bool allowCrackerRecipeCreation = true)
	{
		EggData eggData = new EggData(eggPrefabTag, name, description, mass, requiredDLC, forbiddenDLC, allowCrackerRecipeCreation, customShellRatio);
		eggData.customOutput = customDrops;
		uncategorizedEggData.Add(eggData);
	}

	public static void CategorizeEggs()
	{
		foreach (EggData uncategorizedEggDatum in uncategorizedEggData)
		{
			GameObject prefab = Assets.GetPrefab(uncategorizedEggDatum.id);
			Tag spawnedCreature = prefab.GetDef<IncubationMonitor.Def>().spawnedCreature;
			GameObject prefab2 = Assets.GetPrefab(spawnedCreature);
			CreatureBrain component = prefab2.GetComponent<CreatureBrain>();
			Tag species = component.species;
			uncategorizedEggDatum.isBaseMorph = Assets.GetPrefab(spawnedCreature).HasTag(GameTags.OriginalCreature);
			if (!EggsBySpecies.ContainsKey(species))
			{
				EggsBySpecies.Add(species, new List<EggData>());
			}
			EggsBySpecies[species].Add(uncategorizedEggDatum);
		}
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("EggCracker", 2, 2, "egg_cracker_kanim", 30, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.RAW_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0);
		buildingDef.AudioCategory = "Metal";
		buildingDef.SceneLayer = Grid.SceneLayer.Building;
		buildingDef.ForegroundLayer = Grid.SceneLayer.BuildingFront;
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		buildingDef.AddSearchTerms(SEARCH_TERMS.FOOD);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<DropAllWorkable>();
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		go.AddOrGet<KBatchedAnimController>().SetSymbolVisiblity("snapto_egg", is_visible: false);
		ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
		complexFabricator.labelByResult = false;
		complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		complexFabricator.duplicantOperated = true;
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		ComplexFabricatorWorkable complexFabricatorWorkable = go.AddOrGet<ComplexFabricatorWorkable>();
		BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
		complexFabricatorWorkable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_egg_cracker_kanim") };
		complexFabricator.outputOffset = new Vector3(1f, 1f, 0f);
		Prioritizable.AddRef(go);
		go.AddOrGet<EggCracker>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
	}

	public override void ConfigurePost(BuildingDef def)
	{
		base.ConfigurePost(def);
		MakeRecipes();
	}

	public void MakeRecipes()
	{
		CategorizeEggs();
		foreach (KeyValuePair<Tag, List<EggData>> eggsBySpecy in EggsBySpecies)
		{
			Tag[] array = new Tag[eggsBySpecy.Value.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = eggsBySpecy.Value[i].id;
			}
			EggData eggData = eggsBySpecy.Value[0];
			if (!eggData.hasCrackerRecipe)
			{
				continue;
			}
			string arg = string.Format(STRINGS.BUILDINGS.PREFABS.EGGCRACKER.RESULT_DESCRIPTION, eggData.name);
			ComplexRecipe.RecipeElement recipeElement = new ComplexRecipe.RecipeElement(array, 1f);
			recipeElement.material = array[0];
			ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1] { recipeElement };
			float num = ((eggData.customShellRatio < 0f) ? 0.5f : eggData.customShellRatio);
			float num2 = 1f - num;
			List<ComplexRecipe.RecipeElement> list = new List<ComplexRecipe.RecipeElement>();
			ComplexRecipe.RecipeElement recipeElement2 = null;
			ComplexRecipe.RecipeElement recipeElement3 = null;
			if (num > 0f)
			{
				list.Add(recipeElement2 = new ComplexRecipe.RecipeElement("EggShell", num * eggData.mass, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature));
			}
			if (num2 > 0f)
			{
				list.Add(recipeElement3 = new ComplexRecipe.RecipeElement("RawEgg", num2 * eggData.mass, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature));
			}
			if (eggData.customOutput != null)
			{
				Tuple<Tag, float>[] customOutput = eggData.customOutput;
				foreach (Tuple<Tag, float> tuple in customOutput)
				{
					if (tuple.first == "EggShell")
					{
						recipeElement2 = ((recipeElement3 != null) ? recipeElement2 : new ComplexRecipe.RecipeElement("EggShell", 0f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature));
						recipeElement2.amount += tuple.second;
					}
					else if (tuple.first == "RawEgg")
					{
						recipeElement3 = ((recipeElement3 != null) ? recipeElement3 : new ComplexRecipe.RecipeElement("RawEgg", 0f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature));
						recipeElement3.amount += tuple.second;
					}
					else
					{
						list.Add(new ComplexRecipe.RecipeElement(tuple.first, tuple.second, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature));
					}
				}
			}
			ComplexRecipe.RecipeElement[] array3 = list.ToArray();
			string obsolete_id = ComplexRecipeManager.MakeObsoleteRecipeID("EggCracker", "RawEgg");
			string text = ComplexRecipeManager.MakeRecipeID("EggCracker", array2, array3);
			ComplexRecipe complexRecipe = new ComplexRecipe(text, array2, array3, eggData.requiredDlcIds, eggData.forbiddenDlcIds)
			{
				description = string.Format(STRINGS.BUILDINGS.PREFABS.EGGCRACKER.RECIPE_DESCRIPTION, eggData.name, arg),
				fabricators = new List<Tag> { "EggCracker" },
				time = 5f,
				nameDisplay = ComplexRecipe.RecipeNameDisplay.Custom,
				customName = eggsBySpecy.Key.ProperName(),
				customSpritePrefabID = ((array2[0].material != null) ? array2[0].material.Name : array2[0].possibleMaterials[0].Name)
			};
			ComplexRecipeManager.Get().AddObsoleteIDMapping(obsolete_id, text);
		}
	}
}
