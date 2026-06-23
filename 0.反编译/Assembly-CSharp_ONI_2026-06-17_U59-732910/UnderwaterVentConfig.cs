using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class UnderwaterVentConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "UnderwaterVent";

	public static readonly UnderwaterVent.Data Data = new UnderwaterVent.Data(new Vector3(1f, 2.5f, 0f), new Vector3(1f, 1.5f, 0f), SimHashes.Methane, 373.15f, 1f / 12f, SimHashes.Sulfur, 1000f, 373.15f, 1200f);

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("UnderwaterVent", STRINGS.CREATURES.SPECIES.GEYSER.UNDERWATERVENT.NAME, STRINGS.CREATURES.SPECIES.GEYSER.UNDERWATERVENT.DESC, 2000f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER1, noise: NOISE_POLLUTION.NOISY.TIER5, anim: Assets.GetAnim("underwater_vent_kanim"), initialAnim: "idle", sceneLayer: Grid.SceneLayer.BuildingBack, width: 4, height: 4, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.GeyserFeature });
		gameObject.GetComponent<OccupyArea>().objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
		gameObject.AddOrGet<EntombVulnerable>();
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Katairite);
		component.Temperature = 363.15f;
		gameObject.AddOrGet<Submergable>().GetStatusItem = GetSubmergableStatusItem;
		gameObject.AddOrGetDef<UnderwaterVent.Def>().data = Data;
		gameObject.AddOrGet<BuildingAttachPoint>().points = new BuildingAttachPoint.HardPoint[1]
		{
			new BuildingAttachPoint.HardPoint(new CellOffset(0, 0), GameTags.UnderwaterVentDrill, null)
		};
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		inst.AddOrGet<Submergable>().GetStatusItem = GetSubmergableStatusItem;
	}

	public void OnSpawn(GameObject inst)
	{
	}

	private static StatusItem GetSubmergableStatusItem()
	{
		return Db.Get().CreatureStatusItems.NotSubmerged;
	}
}
