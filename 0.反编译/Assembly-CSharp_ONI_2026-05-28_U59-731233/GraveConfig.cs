using System.Collections.Generic;
using TUNING;
using UnityEngine;

public class GraveConfig : IBuildingConfig
{
	public const string ID = "Grave";

	public const string AnimFile = "gravestone_kanim";

	private static KAnimFile[] STORAGE_OVERRIDE_ANIM_FILES;

	private static readonly HashedString[] STORAGE_WORK_ANIMS = new HashedString[1] { "working_pre" };

	private static readonly HashedString STORAGE_PST_ANIM = HashedString.Invalid;

	private static readonly List<Storage.StoredItemModifier> StorageModifiers = new List<Storage.StoredItemModifier>
	{
		Storage.StoredItemModifier.Hide,
		Storage.StoredItemModifier.Preserve
	};

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("Grave", 1, 2, "gravestone_kanim", 30, 120f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER5, MATERIALS.RAW_MINERALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.BONUS.TIER1);
		buildingDef.Overheatable = false;
		buildingDef.Floodable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.BaseTimeUntilRepair = -1f;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		STORAGE_OVERRIDE_ANIM_FILES = new KAnimFile[1] { Assets.GetAnim("anim_bury_dupe_kanim") };
		GraveStorage graveStorage = go.AddOrGet<GraveStorage>();
		graveStorage.showInUI = true;
		graveStorage.SetDefaultStoredItemModifiers(StorageModifiers);
		graveStorage.overrideAnims = STORAGE_OVERRIDE_ANIM_FILES;
		graveStorage.workAnims = STORAGE_WORK_ANIMS;
		graveStorage.workingPstComplete = new HashedString[1] { STORAGE_PST_ANIM };
		graveStorage.synchronizeAnims = false;
		graveStorage.useGunForDelivery = false;
		graveStorage.workAnimPlayMode = KAnim.PlayMode.Once;
		go.AddOrGet<Grave>();
		Prioritizable.AddRef(go);
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.prefabInitFn += OnInit;
	}

	private void OnInit(GameObject go)
	{
		GraveStorage graveStorage = go.AddOrGet<GraveStorage>();
		KAnimFile[] value = new KAnimFile[1] { Assets.GetAnim("anim_bury_dupe_kanim") };
		graveStorage.workerTypeOverrideAnims.Add(MinionConfig.ID, value);
		graveStorage.workerTypeOverrideAnims.Add(BionicMinionConfig.ID, new KAnimFile[1] { Assets.GetAnim("anim_bionic_bury_dupe_kanim") });
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
