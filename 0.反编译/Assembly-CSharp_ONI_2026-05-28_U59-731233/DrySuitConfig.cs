using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class DrySuitConfig : IEquipmentConfig, IHasDlcRestrictions
{
	public const string ID = "DrySuit";

	public static ComplexRecipe recipe;

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public EquipmentDef CreateEquipmentDef()
	{
		ClothingWearer.ClothingInfo clothingInfo = ClothingWearer.ClothingInfo.DRY_SUIT;
		List<AttributeModifier> attributeModifiers = new List<AttributeModifier>();
		EquipmentDef equipmentDef = EquipmentTemplates.CreateEquipmentDef("DrySuit", TUNING.EQUIPMENT.CLOTHING.SLOT, SimHashes.Carbon, TUNING.EQUIPMENT.SUITS.DRY_SUIT_MASS, "wetsuit_item_kanim", TUNING.EQUIPMENT.VESTS.SNAPON0, "body_wetsuit_kanim", 4, attributeModifiers, TUNING.EQUIPMENT.VESTS.SNAPON1, IsBody: true, EntityTemplates.CollisionShape.RECTANGLE, 0.75f, 0.4f, new Tag[2]
		{
			GameTags.Clothes,
			GameTags.PedestalDisplayable
		});
		int decorMod = ClothingWearer.ClothingInfo.DRY_SUIT.decorMod;
		Descriptor item = new Descriptor($"{DUPLICANTS.ATTRIBUTES.THERMALCONDUCTIVITYBARRIER.NAME}: {GameUtil.GetFormattedDistance(ClothingWearer.ClothingInfo.DRY_SUIT.conductivityMod)}", $"{DUPLICANTS.ATTRIBUTES.THERMALCONDUCTIVITYBARRIER.NAME}: {GameUtil.GetFormattedDistance(ClothingWearer.ClothingInfo.DRY_SUIT.conductivityMod)}");
		Descriptor item2 = new Descriptor($"{DUPLICANTS.ATTRIBUTES.DECOR.NAME}: {decorMod}", $"{DUPLICANTS.ATTRIBUTES.DECOR.NAME}: {decorMod}");
		equipmentDef.additionalDescriptors.Add(item);
		if (decorMod != 0)
		{
			equipmentDef.additionalDescriptors.Add(item2);
		}
		equipmentDef.OnEquipCallBack = delegate(Equippable eq)
		{
			ClothingWearer.ClothingInfo.OnEquipVest(eq, clothingInfo);
		};
		equipmentDef.OnUnequipCallBack = ClothingWearer.ClothingInfo.OnUnequipVest;
		equipmentDef.RecipeDescription = STRINGS.EQUIPMENT.PREFABS.DRYSUIT.RECIPE_DESC;
		ResourceSet<Effect> effects = Db.Get().effects;
		equipmentDef.EffectImmunites.Add(effects.Get("WetFeet"));
		equipmentDef.EffectImmunites.Add(effects.Get("SoakingWet"));
		return equipmentDef;
	}

	public static void SetupVest(GameObject go)
	{
		Equippable equippable = go.GetComponent<Equippable>();
		if (equippable == null)
		{
			equippable = go.AddComponent<Equippable>();
		}
		equippable.SetQuality(QualityLevel.Poor);
		go.GetComponent<KBatchedAnimController>().sceneLayer = Grid.SceneLayer.BuildingBack;
	}

	public void DoPostConfigure(GameObject go)
	{
		SetupVest(go);
	}
}
