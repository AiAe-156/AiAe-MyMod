using System.Collections.Generic;
using TUNING;
using UnityEngine;

public class LonelyMinionMailboxConfig : IBuildingConfig
{
	public const string ID = "LonelyMailBox";

	public static readonly HashedString IdHash = "LonelyMailBox";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("LonelyMailBox", 2, 2, "parcel_delivery_kanim", 10, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.ALL_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.SceneLayer = Grid.SceneLayer.BuildingBack;
		buildingDef.DefaultAnimState = "idle";
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.ShowInBuildMenu = false;
		buildingDef.ViewMode = OverlayModes.None.ID;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "small";
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		SingleEntityReceptacle singleEntityReceptacle = go.AddComponent<SingleEntityReceptacle>();
		singleEntityReceptacle.AddDepositTag(GameTags.Edible);
		singleEntityReceptacle.enabled = false;
		go.AddComponent<LonelyMinionMailbox>();
		go.GetComponent<Deconstructable>().allowDeconstruction = false;
		Storage storage = go.AddOrGet<Storage>();
		storage.allowItemRemoval = false;
		storage.SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>
		{
			Storage.StoredItemModifier.Seal,
			Storage.StoredItemModifier.Preserve
		});
		Prioritizable.AddRef(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
