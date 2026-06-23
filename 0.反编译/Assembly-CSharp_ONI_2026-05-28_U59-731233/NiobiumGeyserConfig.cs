using STRINGS;
using UnityEngine;

public class NiobiumGeyserConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "NiobiumGeyser";

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
		GeyserConfigurator.GeyserType geyserType = new GeyserConfigurator.GeyserType("molten_niobium", SimHashes.MoltenNiobium, GeyserConfigurator.GeyserShape.Molten, 3500f, 800f, 1600f, 150f, null, null, 6000f, 12000f, 0.005f, 0.01f);
		GameObject gameObject = GeyserGenericConfig.CreateGeyser("NiobiumGeyser", "geyser_molten_niobium_kanim", 3, 3, CREATURES.SPECIES.GEYSER.MOLTEN_NIOBIUM.NAME, CREATURES.SPECIES.GEYSER.MOLTEN_NIOBIUM.DESC, geyserType.idHash, geyserType.geyserTemperature, DlcManager.EXPANSION1, null);
		gameObject.GetComponent<KPrefabID>().AddTag(GameTags.DeprecatedContent);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
