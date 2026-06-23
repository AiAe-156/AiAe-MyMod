using STRINGS;
using UnityEngine;

[EntityConfigOrder(4)]
public class BabyAlgaeStegoConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "AlgaeStegoBaby";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC4;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = AlgaeStegoConfig.CreateStego("AlgaeStegoBaby", CREATURES.SPECIES.ALGAE_STEGO.BABY.NAME, CREATURES.SPECIES.ALGAE_STEGO.BABY.DESC, "baby_stego_kanim", is_baby: true);
		GameObject gameObject2 = EntityTemplates.ExtendEntityToBeingABaby(gameObject, "AlgaeStego");
		KBatchedAnimController component = gameObject.GetComponent<KBatchedAnimController>();
		component.SetSymbolVisiblity("baby_stego_eye_yellow", is_visible: false);
		component.SetSymbolVisiblity("baby_stego_scale", is_visible: false);
		component.SetSymbolVisiblity("baby_stego_pupil", is_visible: false);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
