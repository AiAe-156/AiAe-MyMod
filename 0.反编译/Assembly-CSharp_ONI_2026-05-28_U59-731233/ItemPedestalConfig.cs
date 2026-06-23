using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ItemPedestalConfig : IBuildingConfig
{
	public const string ID = "ItemPedestal";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("ItemPedestal", 1, 2, "pedestal_kanim", 10, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.RAW_MINERALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0);
		buildingDef.DefaultAnimState = "pedestal";
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.ViewMode = OverlayModes.Decor.ID;
		buildingDef.AudioCategory = "Glass";
		buildingDef.AudioSize = "small";
		buildingDef.AddSearchTerms(SEARCH_TERMS.MORALE);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Storage storage = go.AddOrGet<Storage>();
		storage.SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>(new Storage.StoredItemModifier[2]
		{
			Storage.StoredItemModifier.Seal,
			Storage.StoredItemModifier.Preserve
		}));
		Prioritizable.AddRef(go);
		OrnamentReceptacle ornamentReceptacle = go.AddOrGet<OrnamentReceptacle>();
		ornamentReceptacle.AddDepositTag(GameTags.Ornament);
		ornamentReceptacle.AddDepositTag(GameTags.Suit);
		ornamentReceptacle.AddDepositTag(GameTags.Clothes);
		ornamentReceptacle.AddDepositTag(GameTags.Egg);
		ornamentReceptacle.AddDepositTag(GameTags.Seed);
		ornamentReceptacle.AddDepositTag(GameTags.Edible);
		ornamentReceptacle.AddDepositTag(GameTags.BionicUpgrade);
		ornamentReceptacle.AddDepositTag(GameTags.Solid);
		ornamentReceptacle.AddDepositTag(GameTags.Liquid);
		ornamentReceptacle.AddDepositTag(GameTags.Gas);
		ornamentReceptacle.AddDepositTag(GameTags.PedestalDisplayable);
		ornamentReceptacle.occupyingObjectRelativePosition = new Vector3(0f, 1.2f, -1f);
		go.AddOrGet<DecorProvider>();
		go.AddOrGet<ItemPedestal>();
		go.GetComponent<KPrefabID>().AddTag(GameTags.Decoration);
		go.GetComponent<KPrefabID>().AddTag(GameTags.OrnamentDisplayer);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
