using System.Collections.Generic;
using UnityEngine;

public class AlgaeConfig : IOreConfig
{
	public SimHashes ElementID => SimHashes.Algae;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateSolidOreEntity(ElementID, new List<Tag> { GameTags.Life });
		DissolvingAlgae dissolvingAlgae = gameObject.AddOrGet<DissolvingAlgae>();
		dissolvingAlgae.emitRange = 1;
		dissolvingAlgae.emitCount = 1000;
		return gameObject;
	}
}
