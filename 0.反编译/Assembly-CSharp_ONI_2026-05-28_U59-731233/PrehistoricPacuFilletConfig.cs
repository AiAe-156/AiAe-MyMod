using STRINGS;
using TUNING;
using UnityEngine;

public class PrehistoricPacuFilletConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "PrehistoricPacuFillet";

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
		GameObject gameObject = EntityTemplates.CreateLooseEntity("PrehistoricPacuFillet", STRINGS.ITEMS.FOOD.PREHISTORICPACUFILLET.NAME, STRINGS.ITEMS.FOOD.PREHISTORICPACUFILLET.DESC, 1f, unitMass: false, Assets.GetAnim("jawboFillet_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		EntityTemplates.ExtendEntityToFood(gameObject, FOOD.FOOD_TYPES.JAWBOFILLET);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
