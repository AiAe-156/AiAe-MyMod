using System;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

public class HijackedHeadquartersConfig : IBuildingConfig
{
	public const string ID = "HijackedHeadquarters";

	private const int WIDTH = 5;

	private const int HEIGHT = 5;

	public const int DEFAULT_DATABANK_PRINT_COST = 25;

	public const int COST_INCREASE_PER_PRINT = 25;

	public const int MAX_COST_INCREASES_PER_PRINT = 10;

	private static Dictionary<Tag, int> PrintableCostOverrides = new Dictionary<Tag, int>();

	public static int GetDataBankCost(Tag printableTag, int printCount = 0)
	{
		if (PrintableCostOverrides.ContainsKey(printableTag))
		{
			return PrintableCostOverrides[printableTag];
		}
		return 25 + Math.Min(printCount, 10) * 25;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("HijackedHeadquarters", 5, 5, "hijacked_hq_kanim", 250, 120f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER5, new string[1] { SimHashes.Steel.ToString() }, 3200f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER5, decor: BUILDINGS.DECOR.NONE);
		obj.ExhaustKilowattsWhenActive = 0f;
		obj.SelfHeatKilowattsWhenActive = 0f;
		obj.Floodable = false;
		obj.Entombable = true;
		obj.Overheatable = false;
		obj.ShowInBuildMenu = false;
		obj.AudioCategory = "Metal";
		obj.AudioSize = "medium";
		obj.ForegroundLayer = Grid.SceneLayer.Ground;
		return obj;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		PrimaryElement component = go.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Steel);
		component.Temperature = 294.15f;
		BuildingTemplates.ExtendBuildingToGravitas(go);
		Storage storage = go.AddComponent<Storage>();
		storage.capacityKg = 275f;
		Activatable activatable = go.AddComponent<Activatable>();
		activatable.synchronizeAnims = false;
		activatable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_use_remote_kanim") };
		activatable.SetWorkTime(30f);
		go.AddOrGetDef<HijackedHeadquarters.Def>();
		ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.RequestedItemTag = DatabankHelper.ID;
		manualDeliveryKG.MinimumMass = 0f;
		manualDeliveryKG.refillMass = 25f;
		manualDeliveryKG.capacity = storage.capacityKg;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.Fetch.IdHash;
		manualDeliveryKG.operationalRequirement = Operational.State.Operational;
		manualDeliveryKG.ShowStatusItem = false;
		manualDeliveryKG.RoundFetchAmountToInt = true;
		manualDeliveryKG.FillToCapacity = true;
		go.AddComponent<DropToUserCapacity>();
		go.GetComponent<KPrefabID>().prefabInitFn += delegate(GameObject game_object)
		{
			game_object.GetComponent<Activatable>().SetOffsets(OffsetGroups.LeftOrRight);
			StoryManager.Instance.ForceCreateStory(Db.Get().Stories.HijackedHeadquarters, game_object.GetMyWorldId());
		};
	}
}
