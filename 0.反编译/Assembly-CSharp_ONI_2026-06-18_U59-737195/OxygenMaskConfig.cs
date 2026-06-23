using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class OxygenMaskConfig : IEquipmentConfig
{
	public const string ID = "Oxygen_Mask";

	public const string WORN_ID = "Worn_Oxygen_Mask";

	private const PathFinder.PotentialPath.Flags suit_flags = PathFinder.PotentialPath.Flags.HasOxygenMask;

	private AttributeModifier expertAthleticsModifier;

	public EquipmentDef CreateEquipmentDef()
	{
		List<AttributeModifier> list = new List<AttributeModifier>();
		list.Add(new AttributeModifier(TUNING.EQUIPMENT.ATTRIBUTE_MOD_IDS.ATHLETICS, TUNING.EQUIPMENT.SUITS.OXYGEN_MASK_ATHLETICS, STRINGS.EQUIPMENT.PREFABS.OXYGEN_MASK.NAME));
		expertAthleticsModifier = new AttributeModifier(TUNING.EQUIPMENT.ATTRIBUTE_MOD_IDS.ATHLETICS, -TUNING.EQUIPMENT.SUITS.OXYGEN_MASK_ATHLETICS, Db.Get().Skills.Suits1.Name);
		EquipmentDef equipmentDef = EquipmentTemplates.CreateEquipmentDef("Oxygen_Mask", TUNING.EQUIPMENT.SUITS.SLOT, SimHashes.Dirt, 15f, "oxygen_mask_kanim", "mask_oxygen", "", 6, list, null, IsBody: false, EntityTemplates.CollisionShape.CIRCLE, 0.325f, 0.325f, new Tag[3]
		{
			GameTags.Suit,
			GameTags.Clothes,
			GameTags.PedestalDisplayable
		});
		equipmentDef.wornID = "Worn_Oxygen_Mask";
		equipmentDef.RecipeDescription = STRINGS.EQUIPMENT.PREFABS.OXYGEN_MASK.RECIPE_DESC;
		equipmentDef.OnEquipCallBack = delegate(Equippable eq)
		{
			Ownables soleOwner = eq.assignee.GetSoleOwner();
			if (soleOwner != null)
			{
				GameObject targetGameObject = soleOwner.GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
				Navigator component = targetGameObject.GetComponent<Navigator>();
				if (component != null)
				{
					component.SetFlags(PathFinder.PotentialPath.Flags.HasOxygenMask);
				}
				MinionResume component2 = targetGameObject.GetComponent<MinionResume>();
				if (component2 != null && component2.HasPerk(Db.Get().SkillPerks.ExosuitExpertise.Id))
				{
					targetGameObject.GetAttributes().Get(Db.Get().Attributes.Athletics).Add(expertAthleticsModifier);
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
						targetGameObject.GetAttributes()?.Get(Db.Get().Attributes.Athletics).Remove(expertAthleticsModifier);
						Navigator component = targetGameObject.GetComponent<Navigator>();
						if (component != null)
						{
							component.ClearFlags(PathFinder.PotentialPath.Flags.HasOxygenMask);
						}
					}
				}
			}
		};
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.SuitIDs, "Oxygen_Mask");
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.SuitIDs, "Helmet");
		return equipmentDef;
	}

	public void DoPostConfigure(GameObject go)
	{
		Storage storage = go.AddComponent<Storage>();
		storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
		storage.showInUI = true;
		SuitTank suitTank = go.AddComponent<SuitTank>();
		suitTank.element = "Oxygen";
		suitTank.capacity = 20f;
		suitTank.elementTag = GameTags.Breathable;
		suitTank.SafeCellFlagsToIgnoreOnEquipped = SafeCellQuery.SafeFlags.IsBreathable;
		Durability durability = go.AddComponent<Durability>();
		durability.wornEquipmentPrefabID = "Worn_Oxygen_Mask";
		durability.durabilityLossPerCycle = TUNING.EQUIPMENT.SUITS.OXYGEN_MASK_DECAY;
		go.AddComponent<SuitDiseaseHandler>();
	}
}
