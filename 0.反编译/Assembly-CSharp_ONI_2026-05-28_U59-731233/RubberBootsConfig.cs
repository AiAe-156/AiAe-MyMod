using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class RubberBootsConfig : IEquipmentConfig, IHasDlcRestrictions
{
	public static readonly string ID = "RubberBoots";

	public EquipmentDef CreateEquipmentDef()
	{
		List<AttributeModifier> attributeModifiers = new List<AttributeModifier>();
		EquipmentDef equipmentDef = EquipmentTemplates.CreateEquipmentDef(ID, TUNING.EQUIPMENT.SHOES.SLOT, SimHashes.Rubber, 30f, "rubber_boots_item_kanim", TUNING.EQUIPMENT.SHOES.SNAPON0, "", 6, attributeModifiers, null, IsBody: false, EntityTemplates.CollisionShape.CIRCLE, 0.325f, 0.325f, new Tag[2]
		{
			GameTags.PedestalDisplayable,
			GameTags.Clothes
		});
		equipmentDef.RecipeDescription = STRINGS.EQUIPMENT.PREFABS.RUBBERBOOTS.EFFECT;
		ResourceSet<Effect> effects = Db.Get().effects;
		equipmentDef.EffectImmunites.Add(effects.Get("WetFeet"));
		equipmentDef.EffectImmunites.Add(effects.Get("RecentlySlippedTracker"));
		return equipmentDef;
	}

	public void DoPostConfigure(GameObject go)
	{
		Equippable equippable = go.AddOrGet<Equippable>();
		equippable.SetQuality(QualityLevel.Poor);
		if (go.TryGetComponent<KBatchedAnimController>(out var component))
		{
			component.sceneLayer = Grid.SceneLayer.BuildingBack;
		}
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}
}
