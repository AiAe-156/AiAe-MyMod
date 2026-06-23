using STRINGS;
using UnityEngine;

public class IridiumCometConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "IridiumComet";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC4;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateEntity(ID, UI.SPACEDESTINATIONS.COMETS.IRIDIUMCOMET.NAME);
		gameObject.AddOrGet<SaveLoadRoot>();
		gameObject.AddOrGet<LoopingSounds>();
		Comet comet = gameObject.AddOrGet<Comet>();
		comet.massRange = new Vector2(10f, 100f);
		comet.temperatureRange = new Vector2(473.15f, 548.15f);
		comet.explosionTemperatureRange = comet.temperatureRange;
		comet.explosionOreCount = new Vector2I(2, 4);
		comet.impactSound = "Meteor_copper_Impact";
		comet.flyingSoundID = 1;
		comet.EXHAUST_ELEMENT = SimHashes.CarbonDioxide;
		comet.explosionEffectHash = SpawnFXHashes.MeteorImpactMetal;
		comet.entityDamage = 15;
		comet.totalTileDamage = 0.5f;
		comet.splashRadius = 1;
		PrimaryElement primaryElement = gameObject.AddOrGet<PrimaryElement>();
		primaryElement.SetElement(SimHashes.Iridium);
		primaryElement.Temperature = (comet.temperatureRange.x + comet.temperatureRange.y) / 2f;
		KBatchedAnimController kBatchedAnimController = gameObject.AddOrGet<KBatchedAnimController>();
		kBatchedAnimController.AnimFiles = new KAnimFile[1] { Assets.GetAnim("meteor_iridium_kanim") };
		kBatchedAnimController.isMovable = true;
		kBatchedAnimController.initialAnim = "fall_loop";
		kBatchedAnimController.initialMode = KAnim.PlayMode.Loop;
		kBatchedAnimController.visibilityType = KAnimControllerBase.VisibilityType.OffscreenUpdate;
		KCircleCollider2D kCircleCollider2D = gameObject.AddOrGet<KCircleCollider2D>();
		kCircleCollider2D.radius = 0.5f;
		gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
		gameObject.AddTag(GameTags.Comet);
		return gameObject;
	}

	public void OnPrefabInit(GameObject go)
	{
	}

	public void OnSpawn(GameObject go)
	{
	}
}
