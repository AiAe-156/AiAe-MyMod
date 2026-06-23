using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class PowerControlStationConfig : IBuildingConfig
{
	public const string ID = "PowerControlStation";

	public static Tag MATERIAL_FOR_TINKER = GameTags.RefinedMetal;

	public static Tag TINKER_TOOLS = PowerStationToolsConfig.tag;

	public const float MASS_PER_TINKER = 5f;

	public static string ROLE_PERK = "CanPowerTinker";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("PowerControlStation", 2, 4, "electricianworkdesk_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER1, decor: TUNING.BUILDINGS.DECOR.NONE);
		obj.ViewMode = OverlayModes.Rooms.ID;
		obj.Overheatable = false;
		obj.AudioCategory = "Metal";
		obj.AudioSize = "large";
		obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		obj.AddSearchTerms(SEARCH_TERMS.POWER);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 50f;
		storage.showInUI = true;
		storage.storageFilters = new List<Tag> { MATERIAL_FOR_TINKER };
		storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
		TinkerStation tinkerstation = go.AddOrGet<TinkerStation>();
		tinkerstation.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_electricianworkdesk_kanim") };
		tinkerstation.inputMaterial = MATERIAL_FOR_TINKER;
		tinkerstation.massPerTinker = 5f;
		tinkerstation.outputPrefab = TINKER_TOOLS;
		tinkerstation.requiredSkillPerk = ROLE_PERK;
		tinkerstation.choreType = Db.Get().ChoreTypes.PowerFabricate.IdHash;
		tinkerstation.useFilteredStorage = true;
		tinkerstation.fetchChoreType = Db.Get().ChoreTypes.PowerFetch.IdHash;
		RoomTracker roomTracker = go.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.PowerPlant.Id;
		roomTracker.requirement = RoomTracker.Requirement.Required;
		Prioritizable.AddRef(go);
		go.GetComponent<KPrefabID>().prefabInitFn += delegate(GameObject game_object)
		{
			TinkerStation component = game_object.GetComponent<TinkerStation>();
			component.AttributeConverter = Db.Get().AttributeConverters.MachinerySpeed;
			component.AttributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.MOST_DAY_EXPERIENCE;
			component.SkillExperienceSkillGroup = Db.Get().SkillGroups.Technicals.Id;
			component.SkillExperienceMultiplier = SKILLS.MOST_DAY_EXPERIENCE;
			tinkerstation.toolProductionTime = 160f;
		};
	}
}
