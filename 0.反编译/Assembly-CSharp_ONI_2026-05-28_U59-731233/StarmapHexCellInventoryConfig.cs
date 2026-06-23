using STRINGS;
using UnityEngine;

public class StarmapHexCellInventoryConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "StarmapHexCellInventory";

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
		GameObject gameObject = EntityTemplates.CreateEntity("StarmapHexCellInventory", UI.CLUSTERMAP.HEXCELL_INVENTORY.NAME);
		gameObject.AddOrGet<SaveLoadRoot>();
		gameObject.AddOrGet<StarmapHexCellInventory>();
		gameObject.AddOrGet<StarmapHexCellInventoryVisuals>();
		InfoDescription infoDescription = gameObject.AddOrGet<InfoDescription>();
		infoDescription.description = UI.CLUSTERMAP.HEXCELL_INVENTORY.DESC;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
