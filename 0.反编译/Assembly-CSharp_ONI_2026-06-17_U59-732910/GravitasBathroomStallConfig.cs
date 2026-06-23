using STRINGS;
using TUNING;
using UnityEngine;

public class GravitasBathroomStallConfig : IBuildingConfig
{
	public const string ID = "GravitasBathroomStall";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("GravitasBathroomStall", 2, 2, "gravitas_toilet_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.RAW_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER0, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0);
		obj.Overheatable = false;
		obj.Floodable = false;
		obj.AudioCategory = "Metal";
		obj.ShowInBuildMenu = false;
		return obj;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		PrimaryElement component = go.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Granite);
		component.Temperature = 294.15f;
		BuildingTemplates.ExtendBuildingToGravitas(go);
		go.AddOrGet<Demolishable>();
		go.AddOrGetDef<GravitasBathroomStall.Def>();
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		base.ConfigureBuildingTemplate(go, prefab_tag);
		Activatable activatable = go.AddOrGet<Activatable>();
		activatable.SetWorkTime(5f);
		activatable.SetButtonTextOverride(new ButtonMenuTextOverride
		{
			Text = UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.ACTIVATE_TOILET_BUTTON,
			ToolTip = UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.ACTIVATE_TOILET_BUTTON_TOOLTIP,
			CancelText = UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.ACTIVATE_TOILET_BUTTON_CANCEL,
			CancelToolTip = UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.ACTIVATE_TOILET_BUTTON_CANCEL_TOOLTIP
		});
		activatable.Required = true;
		activatable.synchronizeAnims = true;
		activatable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_gravitas_toilet_kanim") };
	}

	public void OnPrefabInit(GameObject inst)
	{
		inst.GetComponent<OccupyArea>().objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
