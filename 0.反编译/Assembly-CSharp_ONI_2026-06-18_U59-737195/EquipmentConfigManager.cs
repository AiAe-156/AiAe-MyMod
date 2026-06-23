using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/EquipmentConfigManager")]
public class EquipmentConfigManager : KMonoBehaviour
{
	public static EquipmentConfigManager Instance;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
	}

	public void RegisterEquipment(IEquipmentConfig config)
	{
		string[] requiredDlcIds = null;
		string[] forbiddenDlcIds = null;
		if (config.GetDlcIds() != null)
		{
			DlcManager.ConvertAvailableToRequireAndForbidden(config.GetDlcIds(), out requiredDlcIds, out forbiddenDlcIds);
			DebugUtil.DevLogError($"{config.GetType()} implements GetDlcIds, which is obsolete.");
		}
		else if (config is IHasDlcRestrictions hasDlcRestrictions)
		{
			requiredDlcIds = hasDlcRestrictions.GetRequiredDlcIds();
			forbiddenDlcIds = hasDlcRestrictions.GetForbiddenDlcIds();
		}
		if (!DlcManager.IsCorrectDlcSubscribed(requiredDlcIds, forbiddenDlcIds))
		{
			return;
		}
		EquipmentDef equipmentDef = config.CreateEquipmentDef();
		GameObject gameObject = EntityTemplates.CreateLooseEntity(equipmentDef.Id, equipmentDef.Name, equipmentDef.RecipeDescription, equipmentDef.Mass, unitMass: true, equipmentDef.Anim, "object", Grid.SceneLayer.Ore, equipmentDef.CollisionShape, equipmentDef.width, equipmentDef.height, isPickupable: true, 0, equipmentDef.OutputElement);
		Equippable equippable = gameObject.AddComponent<Equippable>();
		equippable.def = equipmentDef;
		Debug.Assert(equippable.def != null);
		equippable.slotID = equipmentDef.Slot;
		Debug.Assert(equippable.slot != null);
		config.DoPostConfigure(gameObject);
		Tag[] additionalTags = equipmentDef.AdditionalTags;
		foreach (Tag tag in additionalTags)
		{
			gameObject.GetComponent<KPrefabID>().AddTag(tag);
		}
		Assets.AddPrefab(gameObject.GetComponent<KPrefabID>());
		if (equipmentDef.wornID != null)
		{
			GameObject gameObject2 = EntityTemplates.CreateLooseEntity(equipmentDef.wornID, equipmentDef.WornName, equipmentDef.WornDesc, equipmentDef.Mass, unitMass: true, equipmentDef.Anim, "worn_out", Grid.SceneLayer.Ore, equipmentDef.CollisionShape, equipmentDef.width, equipmentDef.height, isPickupable: true);
			RepairableEquipment repairableEquipment = gameObject2.AddComponent<RepairableEquipment>();
			repairableEquipment.def = equipmentDef;
			Debug.Assert(repairableEquipment.def != null);
			SymbolOverrideControllerUtil.AddToPrefab(gameObject2);
			additionalTags = equipmentDef.AdditionalTags;
			foreach (Tag tag2 in additionalTags)
			{
				gameObject2.GetComponent<KPrefabID>().AddTag(tag2);
			}
			Assets.AddPrefab(gameObject2.GetComponent<KPrefabID>());
		}
	}

	private void LoadRecipe(EquipmentDef def, Equippable equippable)
	{
		Recipe recipe = new Recipe(def.Id, 1f, (SimHashes)0, null, def.RecipeDescription);
		recipe.SetFabricator(def.FabricatorId, def.FabricationTime);
		recipe.TechUnlock = def.RecipeTechUnlock;
		foreach (KeyValuePair<string, float> item in def.InputElementMassMap)
		{
			recipe.AddIngredient(new Recipe.Ingredient(item.Key, item.Value));
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void ValidateEquipmentConfig(IEquipmentConfig equipmentConfig)
	{
		if (equipmentConfig == null)
		{
			throw new ArgumentNullException("equipmentConfig");
		}
		Type type = equipmentConfig.GetType();
		Type typeFromHandle = typeof(IHasDlcRestrictions);
		bool num = type.GetMethod("GetRequiredDlcIds", Type.EmptyTypes) != null;
		bool flag = type.GetMethod("GetForbiddenDlcIds", Type.EmptyTypes) != null;
		bool flag2 = typeFromHandle.IsAssignableFrom(type);
		if ((num || flag) && !flag2)
		{
			DebugUtil.LogErrorArgs(type.Name + " is an IEquipmentConfig and has GetRequiredDlcIds or GetForbiddenDlcIds but does not implement IHasDlcRestrictions.");
		}
	}
}
