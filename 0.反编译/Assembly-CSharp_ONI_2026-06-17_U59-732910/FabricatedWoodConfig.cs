using UnityEngine;

public class FabricatedWoodConfig : IOreConfig
{
	public const string ID = "FabricatedWood";

	public static readonly Tag TAG = TagManager.Create("FabricatedWood");

	public SimHashes ElementID => SimHashes.FabricatedWood;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateSolidOreEntity(ElementID);
		gameObject.GetComponent<KPrefabID>().RemoveTag(GameTags.HideFromSpawnTool);
		return gameObject;
	}
}
