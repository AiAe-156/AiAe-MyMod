using System.Collections.Generic;
using Klei.AI;
using TUNING;
using UnityEngine;

public static class BaseLightBugConfig
{
	public const string EMOTION_FILE_NAME = "lightbug_emotes_kanim";

	public static GameObject BaseLightBug(string id, string name, string desc, string anim_file, string traitId, Color lightColor, EffectorValues decor, bool is_baby, string symbolOverridePrefix = null, string onDeathDropID = "", float onDeathDropCount = 0f)
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 5f, decor: decor, anim: Assets.GetAnim(is_baby ? anim_file : "lightbug_build_kanim"), initialAnim: "idle_loop", sceneLayer: Grid.SceneLayer.Creatures, width: 1, height: 1);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "lightbug_build_kanim", symbolOverridePrefix, FactionManager.FactionID.Prey, traitId, "FlyerNavGrid1x1", NavType.Hover, 32, 2f, onDeathDropID, onDeathDropCount, drownVulnerable: true, entombVulnerable: true, 283.15f, 313.15f, 173.15f, 373.15f);
		EggConfig.CUSTOM_EGG_OUTPUTS.Add(id + "Baby", new List<Tuple<Tag, float>>
		{
			new Tuple<Tag, float>(SimHashes.NaturalResin.CreateTag(), 5f)
		});
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = CREATURES.SORTING.CRITTER_ORDER["LightBug"];
		pickupable.sortOrder = sortOrder;
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.Creatures.Flyer);
		component.prefabInitFn += delegate(GameObject inst)
		{
			inst.GetAttributes().Add(Db.Get().Attributes.MaxUnderwaterTravelCost);
		};
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<Trappable>();
		LureableMonitor.Def def = gameObject.AddOrGetDef<LureableMonitor.Def>();
		def.lures = new Tag[2]
		{
			GameTags.Phosphorite,
			GameTags.Creatures.FlyersLure
		};
		gameObject.AddOrGetDef<ThreatMonitor.Def>();
		gameObject.AddOrGetDef<SubmergedMonitor.Def>();
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		if (DlcManager.FeatureRadiationEnabled())
		{
			RadiationEmitter radiationEmitter = gameObject.AddOrGet<RadiationEmitter>();
			radiationEmitter.emitType = RadiationEmitter.RadiationEmitterType.Constant;
			radiationEmitter.radiusProportionalToRads = false;
			radiationEmitter.emitRadiusX = 6;
			radiationEmitter.emitRadiusY = radiationEmitter.emitRadiusX;
			radiationEmitter.emitRads = 60f;
			radiationEmitter.emissionOffset = new Vector3(0f, 0f, 0f);
			component.prefabSpawnFn += delegate(GameObject inst)
			{
				inst.GetComponent<RadiationEmitter>().SetEmitting(emitting: true);
			};
		}
		if (lightColor != Color.black)
		{
			Light2D light2D = gameObject.AddOrGet<Light2D>();
			light2D.Color = lightColor;
			light2D.overlayColour = LIGHT2D.LIGHTBUG_OVERLAYCOLOR;
			light2D.Range = 5f;
			light2D.Angle = 0f;
			light2D.Direction = LIGHT2D.LIGHTBUG_DIRECTION;
			light2D.Offset = LIGHT2D.LIGHTBUG_OFFSET;
			light2D.shape = LightShape.Circle;
			light2D.drawOverlay = true;
			light2D.Lux = 1800;
			gameObject.AddOrGet<LightSymbolTracker>().targetSymbol = "snapTo_light_locator";
			gameObject.AddOrGetDef<CreatureLightToggleController.Def>();
		}
		KAnimFile anim = Assets.GetAnim("lightbug_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new GrowUpStates.Def(), is_baby)
			.Add(new IncubatingStates.Def(), is_baby)
			.Add(new TrappedStates.Def())
			.Add(new BaggedStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new DrowningStates.Def())
			.PushInterruptGroup()
			.Add(new CreatureSleepStates.Def())
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def(), !is_baby)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def
			{
				shouldBeBehindMilkTank = true
			})
			.Add(new MoveToLureStates.Def())
			.Add(new CallAdultStates.Def(), is_baby)
			.Add(new CritterCondoStates.Def
			{
				working_anim = "cc_working_shinebug"
			}, !is_baby)
			.Add(new CritterEmoteStates.Def(anim), !is_baby)
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.LightBugSpecies, symbolOverridePrefix);
		CritterCondoInteractMontior.Def def2 = gameObject.AddOrGetDef<CritterCondoInteractMontior.Def>();
		def2.condoPrefabTag = "AirBorneCritterCondo";
		return gameObject;
	}

	public static GameObject SetupDiet(GameObject prefab, HashSet<Tag> consumed_tags, Tag producedTag, float caloriesPerKg)
	{
		Diet.Info[] infos = new Diet.Info[1]
		{
			new Diet.Info(consumed_tags, producedTag, caloriesPerKg)
		};
		Diet diet = new Diet(infos);
		CreatureCalorieMonitor.Def def = prefab.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def.diet = diet;
		SolidConsumerMonitor.Def def2 = prefab.AddOrGetDef<SolidConsumerMonitor.Def>();
		def2.diet = diet;
		return prefab;
	}

	public static void SetupLoopingSounds(GameObject inst)
	{
		LoopingSounds component = inst.GetComponent<LoopingSounds>();
		component.StartSound(GlobalAssets.GetSound("ShineBug_wings_LP"));
	}
}
