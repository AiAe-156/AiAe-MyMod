using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class WarmVestConfig : IEquipmentConfig
{
	public const string ID = "Warm_Vest";

	public static ComplexRecipe recipe;

	public EquipmentDef CreateEquipmentDef()
	{
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		dictionary.Add("BasicFabric", TUNING.EQUIPMENT.VESTS.WARM_VEST_MASS);
		ClothingWearer.ClothingInfo clothingInfo = ClothingWearer.ClothingInfo.WARM_CLOTHING;
		List<AttributeModifier> attributeModifiers = new List<AttributeModifier>();
		EquipmentDef equipmentDef = EquipmentTemplates.CreateEquipmentDef("Warm_Vest", TUNING.EQUIPMENT.CLOTHING.SLOT, SimHashes.Carbon, TUNING.EQUIPMENT.VESTS.WARM_VEST_MASS, TUNING.EQUIPMENT.VESTS.WARM_VEST_ICON0, TUNING.EQUIPMENT.VESTS.SNAPON0, TUNING.EQUIPMENT.VESTS.WARM_VEST_ANIM0, 4, attributeModifiers, TUNING.EQUIPMENT.VESTS.SNAPON1, IsBody: true, EntityTemplates.CollisionShape.RECTANGLE, 0.75f, 0.4f, new Tag[2]
		{
			GameTags.Clothes,
			GameTags.PedestalDisplayable
		});
		int decorMod = ClothingWearer.ClothingInfo.WARM_CLOTHING.decorMod;
		Descriptor item = new Descriptor($"{DUPLICANTS.ATTRIBUTES.THERMALCONDUCTIVITYBARRIER.NAME}: {GameUtil.GetFormattedDistance(ClothingWearer.ClothingInfo.WARM_CLOTHING.conductivityMod)}", $"{DUPLICANTS.ATTRIBUTES.THERMALCONDUCTIVITYBARRIER.NAME}: {GameUtil.GetFormattedDistance(ClothingWearer.ClothingInfo.WARM_CLOTHING.conductivityMod)}");
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
		equipmentDef.RecipeDescription = STRINGS.EQUIPMENT.PREFABS.WARM_VEST.RECIPE_DESC;
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
