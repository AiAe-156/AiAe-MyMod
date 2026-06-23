using STRINGS;
using TUNING;
using UnityEngine;

public class MinnowImperativePOIBConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "MinnowImperativePOIB";

	public const float RequiredDeliveryMass = 20f;

	public Tag RequiredDeliveryTag => "Maki".ToTag();

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("MinnowImperativePOIB", STRINGS.BUILDINGS.PREFABS.MINNOW_IMPERATIVE_POI_B.NAME, STRINGS.BUILDINGS.PREFABS.MINNOW_IMPERATIVE_POI_B.DESC, 30f, Assets.GetAnim("minnow_imperative_poi_b_kanim"), "off", Grid.SceneLayer.Creatures, 3, 3, TUNING.BUILDINGS.DECOR.BONUS.TIER0, NOISE_POLLUTION.NONE);
		Storage storage = gameObject.AddOrGet<Storage>();
		storage.capacityKg = 20f;
		storage.showInUI = true;
		ManualDeliveryKG manualDeliveryKG = gameObject.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.RequestedItemTag = RequiredDeliveryTag;
		manualDeliveryKG.capacity = 20f;
		manualDeliveryKG.refillMass = 20f;
		manualDeliveryKG.MinimumMass = 20f;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.Fetch.IdHash;
		manualDeliveryKG.enabled = false;
		gameObject.AddOrGet<Prioritizable>();
		Prioritizable.AddRef(gameObject);
		MinnowImperativePOIStates.Def def = gameObject.AddOrGetDef<MinnowImperativePOIStates.Def>();
		def.requestedTag = RequiredDeliveryTag;
		def.requiredMass = 20f;
		def.minnowPOIIdentity = MinnowImperativePOIStates.MinnowPOIIdentity.POI_B;
		KBatchedAnimController component = gameObject.GetComponent<KBatchedAnimController>();
		component.materialType = KAnimBatchGroup.MaterialType.DefaultInsideVistas;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
