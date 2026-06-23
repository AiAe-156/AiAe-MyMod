using System;
using System.Collections.Generic;
using Klei.AI;
using TUNING;
using UnityEngine;

public class EggConfig
{
	public static Dictionary<Tag, List<Tuple<Tag, float>>> CUSTOM_EGG_OUTPUTS = new Dictionary<Tag, List<Tuple<Tag, float>>>();

	[Obsolete("Mod compatibility: Use CreateEgg with requiredDlcIds and forbiddenDlcIds")]
	public static GameObject CreateEgg(string id, string name, string desc, Tag creature_id, string anim, float mass, int egg_sort_order, float base_incubation_rate)
	{
		return CreateEgg(id, name, desc, creature_id, anim, mass, egg_sort_order, base_incubation_rate, null, null);
	}

	[Obsolete("Mod compatibility: Use CreateEgg with requiredDlcIds and forbiddenDlcIds")]
	public static GameObject CreateEgg(string id, string name, string desc, Tag creature_id, string anim, float mass, int egg_sort_order, float base_incubation_rate, string[] dlcIds)
	{
		DlcManager.ConvertAvailableToRequireAndForbidden(dlcIds, out var requiredDlcIds, out var forbiddenDlcIds);
		return CreateEgg(id, name, desc, creature_id, anim, mass, egg_sort_order, base_incubation_rate, requiredDlcIds, forbiddenDlcIds);
	}

	public static GameObject CreateEgg(string id, string name, string desc, Tag creature_id, string anim, float mass, int egg_sort_order, float base_incubation_rate, string[] requiredDlcIds, string[] forbiddenDlcIds)
	{
		return CreateEgg(id, name, desc, creature_id, anim, mass, egg_sort_order, base_incubation_rate, requiredDlcIds, forbiddenDlcIds, preventEggDrops: false);
	}

	public static GameObject CreateEgg(string id, string name, string desc, Tag creature_id, string anim, float mass, int egg_sort_order, float base_incubation_rate, string[] requiredDlcIds, string[] forbiddenDlcIds, bool preventEggDrops)
	{
		return CreateEgg(id, name, desc, creature_id, anim, mass, egg_sort_order, base_incubation_rate, requiredDlcIds, forbiddenDlcIds, preventEggDrops, mass);
	}

	public static GameObject CreateEgg(string id, string name, string desc, Tag creature_id, string anim, float mass, int egg_sort_order, float base_incubation_rate, string[] requiredDlcIds, string[] forbiddenDlcIds, bool preventEggDrops, float eggMassToDrop)
	{
		return CreateEgg(id, name, desc, creature_id, anim, mass, egg_sort_order, base_incubation_rate, requiredDlcIds, forbiddenDlcIds, preventEggDrops, eggMassToDrop, 0.5f);
	}

	public static GameObject CreateEgg(string id, string name, string desc, Tag creature_id, string anim, float mass, int egg_sort_order, float base_incubation_rate, string[] requiredDlcIds, string[] forbiddenDlcIds, bool preventEggDrops, float eggMassToDrop, float customEggShellRatio, bool allowCrackerRecipeCreation = true)
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity(id, name, desc, mass, unitMass: true, Assets.GetAnim(anim), "idle", Grid.SceneLayer.Ore, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.8f, isPickupable: true);
		gameObject.AddOrGet<KBoxCollider2D>().offset = new Vector2f(0f, 0.36f);
		gameObject.AddOrGet<Pickupable>().sortOrder = SORTORDER.EGGS + egg_sort_order;
		gameObject.AddOrGet<Effects>();
		KPrefabID kPrefabID = gameObject.AddOrGet<KPrefabID>();
		kPrefabID.AddTag(GameTags.Egg);
		kPrefabID.AddTag(GameTags.IncubatableEgg);
		kPrefabID.AddTag(GameTags.PedestalDisplayable);
		kPrefabID.requiredDlcIds = requiredDlcIds;
		kPrefabID.forbiddenDlcIds = forbiddenDlcIds;
		IncubationMonitor.Def def = gameObject.AddOrGetDef<IncubationMonitor.Def>();
		def.preventEggDrops = preventEggDrops;
		def.eggShellRatio = customEggShellRatio;
		def.spawnedCreature = creature_id;
		def.baseIncubationRate = base_incubation_rate;
		gameObject.AddOrGetDef<OvercrowdingMonitor.Def>().spaceRequiredPerCreature = 0;
		UnityEngine.Object.Destroy(gameObject.GetComponent<EntitySplitter>());
		Assets.AddPrefab(gameObject.GetComponent<KPrefabID>());
		EggCrackerConfig.RegisterEgg(id, name, desc, eggMassToDrop, requiredDlcIds, forbiddenDlcIds, CUSTOM_EGG_OUTPUTS.ContainsKey(creature_id) ? CUSTOM_EGG_OUTPUTS[creature_id].ToArray() : null, customEggShellRatio, allowCrackerRecipeCreation);
		return gameObject;
	}
}
