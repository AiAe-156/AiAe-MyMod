using UnityEngine;

public class FabricatedWoodConfig : IOreConfig
{
	public const string ID = "FabricatedWood";

	public static readonly Tag TAG = TagManager.Create("FabricatedWood");

	public SimHashes ElementID => SimHashes.FabricatedWood;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateSolidOreEntity(ElementID);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.RemoveTag(GameTags.HideFromSpawnTool);
		return gameObject;
	}
}
