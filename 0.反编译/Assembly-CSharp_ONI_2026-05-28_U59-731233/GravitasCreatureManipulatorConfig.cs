using STRINGS;
using TUNING;
using UnityEngine;

public class GravitasCreatureManipulatorConfig : IBuildingConfig
{
	public static class CRITTER_LORE_UNLOCK_ID
	{
		public static string For(Tag species)
		{
			return "story_trait_critter_manipulator_" + species.ToString().ToLower();
		}
	}

	public const string ID = "GravitasCreatureManipulator";

	public const string CODEX_ENTRY_ID = "STORYTRAITCRITTERMANIPULATOR";

	public const string INITIAL_LORE_UNLOCK_ID = "story_trait_critter_manipulator_initial";

	public const string PARKING_LORE_UNLOCK_ID = "story_trait_critter_manipulator_parking";

	public const string COMPLETED_LORE_UNLOCK_ID = "story_trait_critter_manipulator_complete";

	private const int HEIGHT = 4;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("GravitasCreatureManipulator", 3, 4, "gravitas_critter_manipulator_kanim", 250, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER5, MATERIALS.REFINED_METALS, 3200f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER5, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER2);
		buildingDef.ExhaustKilowattsWhenActive = 0f;
		buildingDef.SelfHeatKilowattsWhenActive = 0f;
		buildingDef.Floodable = false;
		buildingDef.Entombable = true;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "medium";
		buildingDef.ForegroundLayer = Grid.SceneLayer.Ground;
		buildingDef.ShowInBuildMenu = false;
		return buildingDef;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		PrimaryElement component2 = go.GetComponent<PrimaryElement>();
		component2.SetElement(SimHashes.Steel);
		component2.Temperature = 294.15f;
		BuildingTemplates.ExtendBuildingToGravitas(go);
		go.AddComponent<Storage>();
		Activatable activatable = go.AddComponent<Activatable>();
		activatable.synchronizeAnims = false;
		activatable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_use_remote_kanim") };
		activatable.SetWorkTime(30f);
		GravitasCreatureManipulator.Def def = go.AddOrGetDef<GravitasCreatureManipulator.Def>();
		def.pickupOffset = new CellOffset(-1, 0);
		def.dropOffset = new CellOffset(1, 0);
		def.numSpeciesToUnlockMorphMode = 5;
		def.workingDuration = 15f;
		def.cooldownDuration = 540f;
		MakeBaseSolid.Def def2 = go.AddOrGetDef<MakeBaseSolid.Def>();
		def2.solidOffsets = new CellOffset[4];
		for (int i = 0; i < 4; i++)
		{
			def2.solidOffsets[i] = new CellOffset(0, i);
		}
		component.prefabInitFn += delegate(GameObject game_object)
		{
			Activatable component3 = game_object.GetComponent<Activatable>();
			component3.SetOffsets(OffsetGroups.LeftOrRight);
		};
		component.prefabSpawnFn += OnSpawn;
	}

	private void OnSpawn(GameObject instance)
	{
		KBatchedAnimController[] componentsInChildrenOnly = instance.GetComponentsInChildrenOnly<KBatchedAnimController>();
		foreach (KBatchedAnimController kBatchedAnimController in componentsInChildrenOnly)
		{
			if (kBatchedAnimController.name.Contains("_fg"))
			{
				kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.LiquidVisibilityLayer, isActive: false);
				kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.WaterProof, isActive: true);
			}
		}
	}

	public static Option<string> GetBodyContentForSpeciesTag(Tag species)
	{
		Option<string> nameForSpeciesTag = GetNameForSpeciesTag(species);
		Option<string> descriptionForSpeciesTag = GetDescriptionForSpeciesTag(species);
		if (nameForSpeciesTag.HasValue && descriptionForSpeciesTag.HasValue)
		{
			return GetBodyContent(nameForSpeciesTag.Value, descriptionForSpeciesTag.Value);
		}
		return Option.None;
	}

	public static string GetBodyContentForUnknownSpecies()
	{
		return GetBodyContent(CODEX.STORY_TRAITS.CRITTER_MANIPULATOR.SPECIES_ENTRIES.UNKNOWN_TITLE, CODEX.STORY_TRAITS.CRITTER_MANIPULATOR.SPECIES_ENTRIES.UNKNOWN);
	}

	public static string GetBodyContent(string name, string desc)
	{
		return "<size=125%><b>" + name + "</b></size><line-height=150%>\n</line-height>" + desc;
	}

	public static Option<string> GetNameForSpeciesTag(Tag species)
	{
		string key = "STRINGS.CREATURES.FAMILY_PLURAL." + species.ToString().ToUpper();
		if (!Strings.TryGet(key, out var result))
		{
			return Option.None;
		}
		return Option.Some((string)result);
	}

	public static Option<string> GetDescriptionForSpeciesTag(Tag species)
	{
		string key = "STRINGS.CODEX.STORY_TRAITS.CRITTER_MANIPULATOR.SPECIES_ENTRIES." + species.ToString().ToUpper().Replace("SPECIES", "");
		if (!Strings.TryGet(key, out var result))
		{
			return Option.None;
		}
		return Option.Some((string)result);
	}
}
