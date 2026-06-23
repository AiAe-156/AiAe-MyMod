using Klei.AI;
using UnityEngine;

public class StoredMinionConfig : IEntityConfig
{
	public static string ID = "StoredMinion";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateEntity(ID, ID);
		gameObject.AddOrGet<SaveLoadRoot>();
		gameObject.AddOrGet<KPrefabID>();
		gameObject.AddOrGet<Traits>();
		gameObject.AddOrGet<Schedulable>();
		gameObject.AddOrGet<StoredMinionIdentity>();
		KSelectable kSelectable = gameObject.AddOrGet<KSelectable>();
		kSelectable.IsSelectable = false;
		MinionModifiers minionModifiers = gameObject.AddOrGet<MinionModifiers>();
		minionModifiers.addBaseTraits = false;
		return gameObject;
	}

	public void OnPrefabInit(GameObject go)
	{
		GameObject prefab = Assets.GetPrefab(BionicMinionConfig.ID);
		if (!(prefab != null))
		{
			return;
		}
		StoredMinionIdentity.IStoredMinionExtension[] components = prefab.GetComponents<StoredMinionIdentity.IStoredMinionExtension>();
		if (components != null)
		{
			foreach (StoredMinionIdentity.IStoredMinionExtension storedMinionExtension in components)
			{
				storedMinionExtension.AddStoredMinionGameObjectRequirements(go);
			}
		}
	}

	public void OnSpawn(GameObject go)
	{
		go.Trigger(1589886948, (object)go);
	}
}
