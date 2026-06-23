using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ClustercraftInteriorDoorConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "ClustercraftInteriorDoor";

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(ID, STRINGS.BUILDINGS.PREFABS.CLUSTERCRAFTINTERIORDOOR.NAME, STRINGS.BUILDINGS.PREFABS.CLUSTERCRAFTINTERIORDOOR.DESC, 400f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("rocket_hatch_door_kanim"), initialAnim: "closed", sceneLayer: Grid.SceneLayer.TileMain, width: 1, height: 2, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		gameObject.AddTag(GameTags.NotRoomAssignable);
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Unobtanium);
		component.Temperature = 294.15f;
		gameObject.AddOrGet<Operational>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<Prioritizable>();
		KBatchedAnimController kBatchedAnimController = gameObject.AddOrGet<KBatchedAnimController>();
		kBatchedAnimController.fgLayer = Grid.SceneLayer.InteriorWall;
		gameObject.AddOrGet<ClustercraftInteriorDoor>();
		AssignmentGroupController assignmentGroupController = gameObject.AddOrGet<AssignmentGroupController>();
		assignmentGroupController.generateGroupOnStart = false;
		NavTeleporter navTeleporter = gameObject.AddOrGet<NavTeleporter>();
		navTeleporter.offset = new CellOffset(1, 0);
		gameObject.AddOrGet<AccessControl>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		OccupyArea component = inst.GetComponent<OccupyArea>();
		component.objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
	}

	public void OnSpawn(GameObject inst)
	{
		PrimaryElement component = inst.GetComponent<PrimaryElement>();
		OccupyArea component2 = inst.GetComponent<OccupyArea>();
		int cell = Grid.PosToCell(inst);
		CellOffset[] occupiedCellsOffsets = component2.OccupiedCellsOffsets;
		int[] array = new int[occupiedCellsOffsets.Length];
		for (int i = 0; i < occupiedCellsOffsets.Length; i++)
		{
			CellOffset offset = occupiedCellsOffsets[i];
			int num = Grid.OffsetCell(cell, offset);
			array[i] = num;
		}
		foreach (int num2 in array)
		{
			Grid.HasDoor[num2] = true;
			SimMessages.SetCellProperties(num2, 8);
			Grid.RenderedByWorld[num2] = false;
			World.Instance.groundRenderer.MarkDirty(num2);
			SimMessages.ReplaceAndDisplaceElement(num2, component.ElementID, CellEventLogger.Instance.DoorClose, component.Mass / 2f, component.Temperature);
			SimMessages.SetCellProperties(num2, 4);
		}
	}
}
