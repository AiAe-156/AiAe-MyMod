using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class LargeImpactorCometConfig : IEntityConfig, IHasDlcRestrictions
{
	public static readonly string ID = "LargeImpactorComet";

	private const SimHashes element = SimHashes.Regolith;

	private const int ADDED_CELLS = 6;

	public string[] GetRequiredDlcIds()
	{
		return new string[1] { "DLC4_ID" };
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateEntity(ID, UI.SPACEDESTINATIONS.COMETS.ROCKCOMET.NAME);
		gameObject.AddOrGet<SaveLoadRoot>();
		gameObject.AddOrGet<LoopingSounds>();
		LargeComet largeComet = gameObject.AddOrGet<LargeComet>();
		largeComet.impactSound = "Meteor_Large_Impact";
		largeComet.flyingSoundID = 2;
		largeComet.additionalAnimFiles.Add(new KeyValuePair<string, string>("asteroid_wind_kanim", "wind_loop"));
		largeComet.additionalAnimFiles.Add(new KeyValuePair<string, string>("asteroid_flame_inner_kanim", "flame_loop"));
		largeComet.mainAnimFile = new KeyValuePair<string, string>("asteroid_001_kanim", "idle");
		PrimaryElement primaryElement = gameObject.AddOrGet<PrimaryElement>();
		primaryElement.SetElement(SimHashes.Regolith);
		primaryElement.Temperature = 20000f;
		KBatchedAnimController kBatchedAnimController = gameObject.AddComponent<KBatchedAnimController>();
		kBatchedAnimController.AnimFiles = new KAnimFile[1] { Assets.GetAnim("asteroid_flame_outer_kanim") };
		kBatchedAnimController.isMovable = true;
		kBatchedAnimController.initialAnim = "flame_loop";
		kBatchedAnimController.initialMode = KAnim.PlayMode.Loop;
		kBatchedAnimController.animScale = 0.2f;
		kBatchedAnimController.visibilityType = KAnimControllerBase.VisibilityType.OffscreenUpdate;
		gameObject.AddOrGet<KCircleCollider2D>().radius = 0.5f;
		gameObject.AddTag(GameTags.Comet);
		return gameObject;
	}

	public void OnPrefabInit(GameObject go)
	{
		LargeComet largeComet = go.AddOrGet<LargeComet>();
		largeComet.additionalAnimFiles.Add(new KeyValuePair<string, string>("asteroid_wind_kanim", "wind_loop"));
		largeComet.additionalAnimFiles.Add(new KeyValuePair<string, string>("asteroid_flame_inner_kanim", "flame_loop"));
		largeComet.mainAnimFile = new KeyValuePair<string, string>("asteroid_001_kanim", "idle");
	}

	public void OnSpawn(GameObject go)
	{
	}
}
