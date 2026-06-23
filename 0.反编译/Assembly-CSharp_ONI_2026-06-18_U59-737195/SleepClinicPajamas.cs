using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class SleepClinicPajamas : IEquipmentConfig
{
	public const string ID = "SleepClinicPajamas";

	public const string EFFECT_ID = "SleepClinic";

	public EquipmentDef CreateEquipmentDef()
	{
		ClothingWearer.ClothingInfo clothingInfo = ClothingWearer.ClothingInfo.FANCY_CLOTHING;
		List<AttributeModifier> attributeModifiers = new List<AttributeModifier>();
		EquipmentDef equipmentDef = EquipmentTemplates.CreateEquipmentDef("SleepClinicPajamas", TUNING.EQUIPMENT.CLOTHING.SLOT, SimHashes.Carbon, TUNING.EQUIPMENT.VESTS.FUNKY_VEST_MASS, "pajamas_kanim", TUNING.EQUIPMENT.VESTS.SNAPON0, "body_pajamas_kanim", 4, attributeModifiers, TUNING.EQUIPMENT.VESTS.SNAPON1, IsBody: true, EntityTemplates.CollisionShape.RECTANGLE, 0.75f, 0.4f, new Tag[2]
		{
			GameTags.Clothes,
			GameTags.PedestalDisplayable
		});
		equipmentDef.RecipeDescription = string.Concat(STRINGS.EQUIPMENT.PREFABS.SLEEPCLINICPAJAMAS.DESC, "\n\n", STRINGS.EQUIPMENT.PREFABS.SLEEPCLINICPAJAMAS.EFFECT);
		Descriptor item = new Descriptor($"{DUPLICANTS.ATTRIBUTES.THERMALCONDUCTIVITYBARRIER.NAME}: {GameUtil.GetFormattedDistance(ClothingWearer.ClothingInfo.FANCY_CLOTHING.conductivityMod)}", $"{DUPLICANTS.ATTRIBUTES.THERMALCONDUCTIVITYBARRIER.NAME}: {GameUtil.GetFormattedDistance(ClothingWearer.ClothingInfo.FANCY_CLOTHING.conductivityMod)}");
		Descriptor item2 = new Descriptor($"{DUPLICANTS.ATTRIBUTES.DECOR.NAME}: {ClothingWearer.ClothingInfo.FANCY_CLOTHING.decorMod}", $"{DUPLICANTS.ATTRIBUTES.DECOR.NAME}: {ClothingWearer.ClothingInfo.FANCY_CLOTHING.decorMod}");
		equipmentDef.additionalDescriptors.Add(item);
		equipmentDef.additionalDescriptors.Add(item2);
		Effect.AddModifierDescriptions(null, equipmentDef.additionalDescriptors, "SleepClinic");
		equipmentDef.OnEquipCallBack = delegate(Equippable eq)
		{
			ClothingWearer.ClothingInfo.OnEquipVest(eq, clothingInfo);
		};
		equipmentDef.OnUnequipCallBack = delegate(Equippable eq)
		{
			ClothingWearer.ClothingInfo.OnUnequipVest(eq);
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, STRINGS.EQUIPMENT.PREFABS.SLEEPCLINICPAJAMAS.DESTROY_TOAST, eq.transform);
			eq.gameObject.DeleteObject();
		};
		return equipmentDef;
	}

	public void DoPostConfigure(GameObject go)
	{
		go.AddOrGet<ClinicDreamable>().workTime = 300f;
		go.AddOrGet<Equippable>().SetQuality(QualityLevel.Poor);
		go.GetComponent<KBatchedAnimController>().sceneLayer = Grid.SceneLayer.BuildingFront;
	}
}
