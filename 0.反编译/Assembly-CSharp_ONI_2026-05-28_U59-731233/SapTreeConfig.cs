using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SapTreeConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SapTree";

	public static readonly EffectorValues POSITIVE_DECOR_EFFECT = DECOR.BONUS.TIER5;

	private const int WIDTH = 5;

	private const int HEIGHT = 5;

	private const int ATTACK_RADIUS = 2;

	public const float MASS_EAT_RATE = 0.05f;

	public const float KCAL_TO_KG_RATIO = 0.005f;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		string name = STRINGS.CREATURES.SPECIES.SAPTREE.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SAPTREE.DESC;
		EffectorValues pOSITIVE_DECOR_EFFECT = POSITIVE_DECOR_EFFECT;
		KAnimFile anim = Assets.GetAnim("gravitas_sap_tree_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.Decoration };
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("SapTree", name, desc, 1f, anim, "idle", Grid.SceneLayer.BuildingFront, 5, 5, pOSITIVE_DECOR_EFFECT, default(EffectorValues), SimHashes.Creature, additionalTags);
		SapTree.Def def = gameObject.AddOrGetDef<SapTree.Def>();
		def.foodSenseArea = new Vector2I(5, 1);
		def.massEatRate = 0.05f;
		def.kcalorieToKGConversionRatio = 0.005f;
		def.stomachSize = 5f;
		def.oozeRate = 2f;
		def.oozeOffsets = new List<Vector3>
		{
			new Vector3(-2f, 2f),
			new Vector3(2f, 1f)
		};
		def.attackSenseArea = new Vector2I(5, 5);
		def.attackCooldown = 5f;
		gameObject.AddOrGet<Storage>();
		FactionAlignment factionAlignment = gameObject.AddOrGet<FactionAlignment>();
		factionAlignment.Alignment = FactionManager.FactionID.Hostile;
		factionAlignment.canBePlayerTargeted = false;
		gameObject.AddOrGet<RangedAttackable>();
		gameObject.AddWeapon(5f, 5f, AttackProperties.DamageType.Standard, AttackProperties.TargetType.AreaOfEffect, 1, 2f);
		gameObject.AddOrGet<WiltCondition>();
		TemperatureVulnerable temperatureVulnerable = gameObject.AddOrGet<TemperatureVulnerable>();
		temperatureVulnerable.Configure(173.15f, 0f, 373.15f, 1023.15f);
		gameObject.AddOrGet<EntombVulnerable>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<OccupyArea>().objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
