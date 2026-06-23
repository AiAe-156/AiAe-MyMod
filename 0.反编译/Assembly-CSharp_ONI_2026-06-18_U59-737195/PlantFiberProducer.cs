using UnityEngine;

public class PlantFiberProducer : KMonoBehaviour
{
	public float amount;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Subscribe(1272413801, OnHarvest);
	}

	protected override void OnCleanUp()
	{
		Unsubscribe(1272413801, OnHarvest);
	}

	private void OnHarvest(object obj)
	{
		Harvestable harvestable = (Harvestable)obj;
		if (harvestable != null && harvestable.completed_by != null && harvestable.completed_by.GetComponent<MinionResume>().HasPerk(Db.Get().SkillPerks.CanSalvagePlantFiber))
		{
			SpawnPlantFiber();
		}
	}

	private GameObject SpawnPlantFiber()
	{
		Vector3 position = base.gameObject.transform.GetPosition() + new Vector3(0f, 0.5f, 0f);
		GameObject prefab = Assets.GetPrefab(new Tag("PlantFiber"));
		GameObject gameObject = GameUtil.KInstantiate(prefab, position, Grid.SceneLayer.Ore);
		PrimaryElement component = base.gameObject.GetComponent<PrimaryElement>();
		PrimaryElement component2 = gameObject.GetComponent<PrimaryElement>();
		component2.Temperature = component.Temperature;
		component2.Mass = amount;
		gameObject.SetActive(value: true);
		string properName = gameObject.GetProperName();
		PopFXManager.Instance.SpawnFX(Def.GetUISprite(prefab).first, PopFXManager.Instance.sprite_Plus, properName, gameObject.transform, Vector3.zero);
		return gameObject;
	}
}
