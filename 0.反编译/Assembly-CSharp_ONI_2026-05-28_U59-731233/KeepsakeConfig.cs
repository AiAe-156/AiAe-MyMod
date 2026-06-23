using System;
using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class KeepsakeConfig : IMultiEntityConfig
{
	public delegate void PostInitFn(GameObject gameObject);

	public List<GameObject> CreatePrefabs()
	{
		List<GameObject> list = new List<GameObject>();
		list.Add(CreateKeepsake("MegaBrain", UI.KEEPSAKES.MEGA_BRAIN.NAME, UI.KEEPSAKES.MEGA_BRAIN.DESCRIPTION, "keepsake_mega_brain_kanim"));
		list.Add(CreateKeepsake("CritterManipulator", UI.KEEPSAKES.CRITTER_MANIPULATOR.NAME, UI.KEEPSAKES.CRITTER_MANIPULATOR.DESCRIPTION, "keepsake_critter_manipulator_kanim"));
		list.Add(CreateKeepsake("LonelyMinion", UI.KEEPSAKES.LONELY_MINION.NAME, UI.KEEPSAKES.LONELY_MINION.DESCRIPTION, "keepsake_lonelyminion_kanim"));
		list.Add(CreateKeepsake("FossilHunt", UI.KEEPSAKES.FOSSIL_HUNT.NAME, UI.KEEPSAKES.FOSSIL_HUNT.DESCRIPTION, "keepsake_fossil_dig_kanim"));
		list.Add(CreateKeepsake("HijackHeadquarters", UI.KEEPSAKES.HIJACK_HEADQUARTERS.NAME, UI.KEEPSAKES.HIJACK_HEADQUARTERS.DESCRIPTION, "keepsake_hijacked_hq_kanim"));
		list.Add(CreateKeepsake("GeothermalPlant", UI.KEEPSAKES.GEOTHERMAL_PLANT.NAME, UI.KEEPSAKES.GEOTHERMAL_PLANT.DESCRIPTION, "keepsake_geothermal_vent_kanim", "idle", "ui", DlcManager.DLC2));
		GameObject gameObject = CreateKeepsake("MorbRoverMaker", UI.KEEPSAKES.MORB_ROVER_MAKER.NAME, UI.KEEPSAKES.MORB_ROVER_MAKER.DESCRIPTION, "keepsake_morb_tank_kanim");
		gameObject.AddOrGetDef<MorbRoverMakerKeepsake.Def>();
		list.Add(gameObject);
		GameObject gameObject2 = CreateKeepsake("LargeImpactor", UI.KEEPSAKES.VIEWMASTER.NAME, UI.KEEPSAKES.VIEWMASTER.DESCRIPTION, "keepsake_demolior_kanim", "idle", "ui", DlcManager.DLC4);
		if (gameObject2 != null)
		{
			gameObject2.GetComponent<KBoxCollider2D>().size = new Vector2(1f, 0.7f);
			gameObject2.AddOrGetDef<LargeImpactorKeepsake.Def>();
			list.Add(gameObject2);
		}
		list.RemoveAll((GameObject x) => x == null);
		return list;
	}

	public static GameObject CreateKeepsake(string id, string name, string desc, string animFile, string initial_anim = "idle", string ui_anim = "ui", string[] requiredDlcIds = null, string[] forbiddenDlcIds = null, PostInitFn postInitFn = null, SimHashes element = SimHashes.Creature)
	{
		if (!DlcManager.IsCorrectDlcSubscribed(requiredDlcIds, forbiddenDlcIds))
		{
			return null;
		}
		GameObject gameObject = EntityTemplates.CreateLooseEntity("keepsake_" + id.ToLower(), name, desc, 25f, unitMass: true, Assets.GetAnim(animFile), initial_anim, Grid.SceneLayer.Ore, EntityTemplates.CollisionShape.RECTANGLE, 1f, 1f, isPickupable: true, SORTORDER.KEEPSAKES, element, new List<Tag> { GameTags.MiscPickupable });
		OccupyArea occupyArea = gameObject.AddOrGet<OccupyArea>();
		occupyArea.SetCellOffsets(EntityTemplates.GenerateOffsets(1, 1));
		DecorProvider decorProvider = gameObject.AddOrGet<DecorProvider>();
		decorProvider.SetValues(DECOR.BONUS.TIER1);
		decorProvider.overrideName = gameObject.GetProperName();
		gameObject.AddOrGet<KSelectable>();
		gameObject.GetComponent<KBatchedAnimController>().initialMode = KAnim.PlayMode.Loop;
		postInitFn?.Invoke(gameObject);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.PedestalDisplayable);
		component.AddTag(GameTags.Keepsake);
		component.AddTag(GameTags.Ornament);
		KPrefabID component2 = gameObject.GetComponent<KPrefabID>();
		component2.requiredDlcIds = requiredDlcIds;
		component2.forbiddenDlcIds = forbiddenDlcIds;
		CodexEntryRedirector codexEntryRedirector = gameObject.AddOrGet<CodexEntryRedirector>();
		codexEntryRedirector.CodexID = "REQUIREMENTCLASSORNAMENT";
		return gameObject;
	}

	[Obsolete]
	public static GameObject CreateKeepsake(string id, string name, string desc, string animFile, string initial_anim = "idle", string ui_anim = "ui", string[] dlcIDs = null, PostInitFn postInitFn = null, SimHashes element = SimHashes.Creature)
	{
		DlcRestrictionsUtil.TemporaryHelperObject transientHelperObjectFromAllowList = DlcRestrictionsUtil.GetTransientHelperObjectFromAllowList(dlcIDs);
		return CreateKeepsake(id, name, desc, animFile, initial_anim, ui_anim, transientHelperObjectFromAllowList.requiredDlcIds, transientHelperObjectFromAllowList.forbiddenDlcIds, postInitFn, element);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
