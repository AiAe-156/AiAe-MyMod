using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class RemoteWorkTerminalConfig : IBuildingConfig
{
	public static string ID = "RemoteWorkTerminal";

	public static readonly Tag INPUT_MATERIAL = DatabankHelper.TAG;

	public const float INPUT_CAPACITY = 10f;

	public const float INPUT_CONSUMPTION_RATE_PER_S = 1f / 75f;

	public const float INPUT_REFILL_RATIO = 0.5f;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 3, 3, "remote_work_terminal_kanim", 30, 60f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.RAW_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER3, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER2);
		buildingDef.RequiresPowerInput = true;
		buildingDef.EnergyConsumptionWhenActive = 120f;
		buildingDef.SelfHeatKilowattsWhenActive = 2f;
		buildingDef.ExhaustKilowattsWhenActive = 0f;
		buildingDef.AddSearchTerms(SEARCH_TERMS.ROBOT);
		return buildingDef;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddComponent<RemoteWorkTerminal>().workTime = float.PositiveInfinity;
		go.AddComponent<RemoteWorkTerminalSM>();
		go.AddOrGet<Operational>();
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 100f;
		storage.showInUI = true;
		storage.SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>
		{
			Storage.StoredItemModifier.Hide,
			Storage.StoredItemModifier.Insulate
		});
		ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.RequestedItemTag = INPUT_MATERIAL;
		manualDeliveryKG.refillMass = 5f;
		manualDeliveryKG.capacity = 10f;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.ResearchFetch.IdHash;
		manualDeliveryKG.operationalRequirement = Operational.State.Functional;
		ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
		elementConverter.consumedElements = new ElementConverter.ConsumedElement[1]
		{
			new ElementConverter.ConsumedElement(INPUT_MATERIAL, 1f / 75f)
		};
		elementConverter.showDescriptors = false;
		go.AddOrGet<ElementConverterOperationalRequirement>();
		Prioritizable.AddRef(go);
	}

	public override string[] GetRequiredDlcIds()
	{
		return new string[1] { "DLC3_ID" };
	}
}
