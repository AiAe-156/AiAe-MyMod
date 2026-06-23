using KSerialization;
using UnityEngine;

public class PedestalArtifactSpawner : KMonoBehaviour
{
	[MyCmpReq]
	private Storage storage;

	[MyCmpReq]
	private SingleEntityReceptacle receptacle;

	[Serialize]
	private bool artifactSpawned = false;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		foreach (GameObject item in storage.items)
		{
			ArtifactType artifactType = ArtifactSelector.Instance.GetArtifactType(item.name);
			if (artifactType == ArtifactType.Terrestrial)
			{
				item.GetComponent<KPrefabID>().AddTag(GameTags.TerrestrialArtifact, serialize: true);
			}
		}
		if (!artifactSpawned)
		{
			string uniqueArtifactID = ArtifactSelector.Instance.GetUniqueArtifactID(ArtifactType.Terrestrial);
			GameObject gameObject = Util.KInstantiate(Assets.GetPrefab(uniqueArtifactID), base.transform.position);
			gameObject.SetActive(value: true);
			gameObject.GetComponent<KPrefabID>().AddTag(GameTags.TerrestrialArtifact, serialize: true);
			storage.Store(gameObject);
			receptacle.ForceDeposit(gameObject);
			artifactSpawned = true;
		}
	}
}
