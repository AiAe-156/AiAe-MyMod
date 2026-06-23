using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/ArtifactPOIConfigurator")]
public class ArtifactPOIConfigurator : KMonoBehaviour
{
	public class ArtifactPOIType : IHasDlcRestrictions
	{
		public string id;

		public HashedString idHash;

		public string harvestableArtifactID;

		public bool destroyOnHarvest;

		public float poiRechargeTimeMin;

		public float poiRechargeTimeMax;

		[Obsolete]
		public string dlcID;

		public int initialDatabankCount;

		public string[] requiredDlcIds;

		public string[] forbiddenDlcIds;

		public List<string> orbitalObject = new List<string> { Db.Get().OrbitalTypeCategories.gravitas.Id };

		public string[] GetRequiredDlcIds()
		{
			return requiredDlcIds;
		}

		public string[] GetForbiddenDlcIds()
		{
			return forbiddenDlcIds;
		}

		public ArtifactPOIType(string id, string harvestableArtifactID = null, bool destroyOnHarvest = false, float poiRechargeTimeMin = 30000f, float poiRechargeTimeMax = 60000f, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
			: this(id, 0, harvestableArtifactID, destroyOnHarvest, poiRechargeTimeMin, poiRechargeTimeMax, requiredDlcIds, forbiddenDlcIds)
		{
		}

		public ArtifactPOIType(string id, int databankCount, string harvestableArtifactID = null, bool destroyOnHarvest = false, float poiRechargeTimeMin = 30000f, float poiRechargeTimeMax = 60000f, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
		{
			this.id = id;
			idHash = id;
			initialDatabankCount = databankCount;
			this.harvestableArtifactID = harvestableArtifactID;
			this.destroyOnHarvest = destroyOnHarvest;
			this.poiRechargeTimeMin = poiRechargeTimeMin;
			this.poiRechargeTimeMax = poiRechargeTimeMax;
			this.requiredDlcIds = requiredDlcIds;
			this.forbiddenDlcIds = forbiddenDlcIds;
			if (_poiTypes == null)
			{
				_poiTypes = new List<ArtifactPOIType>();
			}
			_poiTypes.Add(this);
		}

		[Obsolete]
		public ArtifactPOIType(string id, string harvestableArtifactID = null, bool destroyOnHarvest = false, float poiRechargeTimeMin = 30000f, float poiRechargeTimeMax = 60000f, string dlcID = "EXPANSION1_ID")
		{
			this.id = id;
			idHash = id;
			this.harvestableArtifactID = harvestableArtifactID;
			this.destroyOnHarvest = destroyOnHarvest;
			this.poiRechargeTimeMin = poiRechargeTimeMin;
			this.poiRechargeTimeMax = poiRechargeTimeMax;
			this.dlcID = dlcID;
			if (_poiTypes == null)
			{
				_poiTypes = new List<ArtifactPOIType>();
			}
			_poiTypes.Add(this);
		}
	}

	[Serializable]
	public class ArtifactPOIInstanceConfiguration
	{
		public HashedString typeId;

		private bool didInit;

		public float rechargeRoll;

		private float poiRechargeTime;

		public ArtifactPOIType poiType => FindType(typeId);

		private void Init()
		{
			if (!didInit)
			{
				didInit = true;
				poiRechargeTime = MathUtil.ReRange(rechargeRoll, 0f, 1f, poiType.poiRechargeTimeMin, poiType.poiRechargeTimeMax);
			}
		}

		public bool DestroyOnHarvest()
		{
			Init();
			return poiType.destroyOnHarvest;
		}

		public string GetArtifactID()
		{
			Init();
			return poiType.harvestableArtifactID;
		}

		public float GetRechargeTime()
		{
			Init();
			return poiRechargeTime;
		}
	}

	private static List<ArtifactPOIType> _poiTypes;

	public static ArtifactPOIType defaultArtifactPoiType = new ArtifactPOIType("HarvestablePOIArtifacts", null, destroyOnHarvest: false, 30000f, 60000f, DlcManager.EXPANSION1);

	public HashedString presetType;

	public float presetMin;

	public float presetMax = 1f;

	public static ArtifactPOIType FindType(HashedString typeId)
	{
		ArtifactPOIType artifactPOIType = null;
		if (typeId != HashedString.Invalid)
		{
			artifactPOIType = _poiTypes.Find((ArtifactPOIType t) => t.id == typeId);
		}
		if (artifactPOIType == null)
		{
			Debug.LogError($"Tried finding a harvestable poi with id {typeId.ToString()} but it doesn't exist!");
		}
		return artifactPOIType;
	}

	public ArtifactPOIInstanceConfiguration MakeConfiguration()
	{
		return CreateRandomInstance(presetType, presetMin, presetMax);
	}

	private ArtifactPOIInstanceConfiguration CreateRandomInstance(HashedString typeId, float min, float max)
	{
		int globalWorldSeed = SaveLoader.Instance.clusterDetailSave.globalWorldSeed;
		ClusterGridEntity component = GetComponent<ClusterGridEntity>();
		Vector3 position = ClusterGrid.Instance.GetPosition(component);
		KRandom randomSource = new KRandom(globalWorldSeed + (int)position.x + (int)position.y);
		return new ArtifactPOIInstanceConfiguration
		{
			typeId = typeId,
			rechargeRoll = Roll(randomSource, min, max)
		};
	}

	private float Roll(KRandom randomSource, float min, float max)
	{
		return (float)(randomSource.NextDouble() * (double)(max - min)) + min;
	}
}
