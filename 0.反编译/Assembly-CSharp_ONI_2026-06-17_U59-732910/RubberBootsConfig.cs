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
		EquipmentDef equipmentDef = EquipmentTemplates.CreateEquipmentDef(ID, TUNING.EQUIPMENT.SHOES.SLOT, SimHashes.Rubber, 30f, "rubber_boots_item_kanim", TUNING.EQUIPMENT.SHOES.SNAPON0, "", 6, attributeModifiers, null, IsBody: false, EntityTemplates.CollisionShape.CIRCLE, 0.28f, 0.28f, new Tag[2]
		{
			GameTags.PedestalDisplayable,
			GameTags.Clothes
		});
		equipmentDef.RecipeDescription = (DlcManager.IsContentSubscribed("DLC3_ID") ? STRINGS.EQUIPMENT.PREFABS.RUBBERBOOTS.RECIPE_DESC_DLC3 : STRINGS.EQUIPMENT.PREFABS.RUBBERBOOTS.RECIPE_DESC);
		ResourceSet<Effect> effects = Db.Get().effects;
		equipmentDef.EffectImmunites.Add(effects.Get("WetFeet"));
		equipmentDef.EffectImmunites.Add(effects.Get("RecentlySlippedTracker"));
		equipmentDef.OnEquipCallBack = delegate(Equippable eq)
		{
			Ownables soleOwner = eq.assignee.GetSoleOwner();
			if (soleOwner != null)
			{
				GameObject targetGameObject = soleOwner.GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
				if ((bool)targetGameObject)
				{
					targetGameObject.AddTag(GameTags.FeetProtection);
				}
			}
		};
		equipmentDef.OnUnequipCallBack = delegate(Equippable eq)
		{
			if (eq.assignee != null)
			{
				Ownables soleOwner = eq.assignee.GetSoleOwner();
				if (soleOwner != null)
				{
					GameObject targetGameObject = soleOwner.GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
					if ((bool)targetGameObject)
					{
						targetGameObject.RemoveTag(GameTags.FeetProtection);
					}
				}
			}
		};
		return equipmentDef;
	}

	public void DoPostConfigure(GameObject go)
	{
		go.AddOrGet<Equippable>().SetQuality(QualityLevel.Poor);
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
