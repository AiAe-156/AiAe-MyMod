using UnityEngine;

public class TallowConfig : IOreConfig
{
	public const string ID = "Tallow";

	public static readonly Tag TAG = TagManager.Create("Tallow");

	public SimHashes ElementID => SimHashes.Tallow;

	public GameObject CreatePrefab()
	{
		return EntityTemplates.CreateSolidOreEntity(ElementID);
	}
}
