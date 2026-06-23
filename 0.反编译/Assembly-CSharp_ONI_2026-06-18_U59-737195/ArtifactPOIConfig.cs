using System.Collections.Generic;
using UnityEngine;

public class ArtifactPOIConfig : IMultiEntityConfig
{
	public struct ArtifactPOIParams
	{
		public string id;

		public string anim;

		public StringKey nameStringKey;

		public StringKey descStringKey;

		public ArtifactPOIConfigurator.ArtifactPOIType poiType;

		public ArtifactPOIParams(string anim, ArtifactPOIConfigurator.ArtifactPOIType poiType)
		{
			id = "ArtifactSpacePOI_" + poiType.id;
			this.anim = anim;
			nameStringKey = new StringKey("STRINGS.UI.SPACEDESTINATIONS.ARTIFACT_POI." + poiType.id.ToUpper() + ".NAME");
			descStringKey = new StringKey("STRINGS.UI.SPACEDESTINATIONS.ARTIFACT_POI." + poiType.id.ToUpper() + ".DESC");
			this.poiType = poiType;
		}
	}

	public const int DEFAULT_INITIAL_DATABANK_COUNT = 50;

	public const string GravitasSpaceStation1 = "GravitasSpaceStation1";

	public const string GravitasSpaceStation2 = "GravitasSpaceStation2";

	public const string GravitasSpaceStation3 = "GravitasSpaceStation3";

	public const string GravitasSpaceStation4 = "GravitasSpaceStation4";

	public const string GravitasSpaceStation5 = "GravitasSpaceStation5";

	public const string GravitasSpaceStation6 = "GravitasSpaceStation6";

	public const string GravitasSpaceStation7 = "GravitasSpaceStation7";

	public const string GravitasSpaceStation8 = "GravitasSpaceStation8";

	public const string RussellsTeapot = "RussellsTeapot";

	public List<GameObject> CreatePrefabs()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (ArtifactPOIParams item in GenerateConfigs())
		{
			list.Add(CreateArtifactPOI(item.id, item.anim, Strings.Get(item.nameStringKey), Strings.Get(item.descStringKey), item.poiType.idHash, item.poiType.initialDatabankCount));
		}
		return list;
	}

	public static GameObject CreateArtifactPOI(string id, string anim, string name, string desc, HashedString poiType)
	{
		return CreateArtifactPOI(id, anim, name, desc, poiType, 0);
	}

	public static GameObject CreateArtifactPOI(string id, string anim, string name, string desc, HashedString poiType, int initialDatabankCount)
	{
		GameObject gameObject = EntityTemplates.CreateEntity(id, id);
		gameObject.AddOrGet<SaveLoadRoot>();
		gameObject.AddOrGet<ArtifactPOIConfigurator>().presetType = poiType;
		ArtifactPOIClusterGridEntity artifactPOIClusterGridEntity = gameObject.AddOrGet<ArtifactPOIClusterGridEntity>();
		artifactPOIClusterGridEntity.m_name = name;
		artifactPOIClusterGridEntity.m_Anim = anim;
		if (initialDatabankCount > 0)
		{
			gameObject.AddOrGetDef<ClusterGridOneTimeResourceSpawner.Def>().thingsToSpawn = new List<ClusterGridOneTimeResourceSpawner.Data>
			{
				new ClusterGridOneTimeResourceSpawner.Data
				{
					itemID = DatabankHelper.ID,
					mass = 1f * (float)initialDatabankCount
				}
			};
		}
		gameObject.AddOrGetDef<ArtifactPOIStates.Def>();
		gameObject.AddOrGet<InfoDescription>().description = desc;
		LoreBearerUtil.AddLoreTo(gameObject, LoreBearerUtil.UnlockNextSpaceEntry);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}

	private List<ArtifactPOIParams> GenerateConfigs()
	{
		List<ArtifactPOIParams> list = new List<ArtifactPOIParams>();
		if (!DlcManager.IsExpansion1Active())
		{
			return list;
		}
		list.Add(new ArtifactPOIParams("station_1", new ArtifactPOIConfigurator.ArtifactPOIType("GravitasSpaceStation1", 50, null, destroyOnHarvest: false, 30000f, 60000f, DlcManager.EXPANSION1)));
		list.Add(new ArtifactPOIParams("station_2", new ArtifactPOIConfigurator.ArtifactPOIType("GravitasSpaceStation2", 50, null, destroyOnHarvest: false, 30000f, 60000f, DlcManager.EXPANSION1)));
		list.Add(new ArtifactPOIParams("station_3", new ArtifactPOIConfigurator.ArtifactPOIType("GravitasSpaceStation3", 50, null, destroyOnHarvest: false, 30000f, 60000f, DlcManager.EXPANSION1)));
		list.Add(new ArtifactPOIParams("station_4", new ArtifactPOIConfigurator.ArtifactPOIType("GravitasSpaceStation4", 50, null, destroyOnHarvest: false, 30000f, 60000f, DlcManager.EXPANSION1)));
		list.Add(new ArtifactPOIParams("station_5", new ArtifactPOIConfigurator.ArtifactPOIType("GravitasSpaceStation5", 50, null, destroyOnHarvest: false, 30000f, 60000f, DlcManager.EXPANSION1)));
		list.Add(new ArtifactPOIParams("station_6", new ArtifactPOIConfigurator.ArtifactPOIType("GravitasSpaceStation6", 50, null, destroyOnHarvest: false, 30000f, 60000f, DlcManager.EXPANSION1)));
		list.Add(new ArtifactPOIParams("station_7", new ArtifactPOIConfigurator.ArtifactPOIType("GravitasSpaceStation7", 50, null, destroyOnHarvest: false, 30000f, 60000f, DlcManager.EXPANSION1)));
		list.Add(new ArtifactPOIParams("station_8", new ArtifactPOIConfigurator.ArtifactPOIType("GravitasSpaceStation8", 50, null, destroyOnHarvest: false, 30000f, 60000f, DlcManager.EXPANSION1)));
		list.Add(new ArtifactPOIParams("russels_teapot", new ArtifactPOIConfigurator.ArtifactPOIType("RussellsTeapot", "artifact_TeaPot", destroyOnHarvest: true, 30000f, 60000f, DlcManager.EXPANSION1)));
		list.RemoveAll((ArtifactPOIParams poi) => !DlcManager.IsCorrectDlcSubscribed(poi.poiType));
		return list;
	}
}
