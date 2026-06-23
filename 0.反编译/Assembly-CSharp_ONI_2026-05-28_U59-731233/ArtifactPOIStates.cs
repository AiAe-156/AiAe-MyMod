using System.Collections.Generic;
using KSerialization;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/ArtifactPOIStates")]
public class ArtifactPOIStates : GameStateMachine<ArtifactPOIStates, ArtifactPOIStates.Instance, IStateMachineTarget, ArtifactPOIStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance, IGameObjectEffectDescriptor
	{
		[Serialize]
		public ArtifactPOIConfigurator.ArtifactPOIInstanceConfiguration configuration;

		[Serialize]
		private float _poiCharge;

		[Serialize]
		private int numHarvests = 0;

		[Serialize]
		public string artifactToHarvest;

		public StarmapHexCellInventory HexCellInventory => GetHexCellInventory();

		public float poiCharge
		{
			get
			{
				return _poiCharge;
			}
			set
			{
				_poiCharge = value;
				base.smi.sm.poiCharge.Set(value, base.smi);
			}
		}

		public void IncreaseArtifactsSpawnedCount()
		{
			numHarvests++;
		}

		public Instance(IStateMachineTarget target, Def def)
			: base(target, def)
		{
		}

		public override void StartSM()
		{
			HexCellInventory.Subscribe(-1697596308, OnHexCellInventoryChanged);
			base.StartSM();
		}

		protected override void OnCleanUp()
		{
			HexCellInventory.Unsubscribe(-1697596308, OnHexCellInventoryChanged);
			base.OnCleanUp();
		}

		private void OnHexCellInventoryChanged(object o)
		{
			base.sm.OnHexCellInventoryChangedSignal.Trigger(this);
		}

		public StarmapHexCellInventory GetHexCellInventory()
		{
			ClusterGridEntity component = GetComponent<ClusterGridEntity>();
			return ClusterGrid.Instance.AddOrGetHexCellInventory(component.Location);
		}

		public bool HasArtifactAvailableInHexCell()
		{
			StarmapHexCellInventory.SerializedItem serializedItem = HexCellInventory.Items.Find((StarmapHexCellInventory.SerializedItem i) => i.IsEntity && Assets.GetPrefab(i.ID).HasTag(GameTags.Artifact));
			return serializedItem != null;
		}

		public void SpawnArtifactOnHexCell()
		{
			Tag itemID = ((artifactToHarvest != null) ? artifactToHarvest : PickNewArtifactToHarvest());
			artifactToHarvest = null;
			HexCellInventory.AddItem(itemID, 1f, Element.State.Vacuum);
		}

		public string PickNewArtifactToHarvest()
		{
			string text = null;
			if (numHarvests <= 0 && !string.IsNullOrEmpty(configuration.GetArtifactID()))
			{
				text = configuration.GetArtifactID();
				ArtifactSelector.Instance.ReserveArtifactID(text);
			}
			else
			{
				text = ArtifactSelector.Instance.GetUniqueArtifactID(ArtifactType.Space);
			}
			return text;
		}

		public void RechargePOI(float dt)
		{
			float num = dt / configuration.GetRechargeTime();
			poiCharge += num;
			poiCharge = Mathf.Min(1f, poiCharge);
		}

		public float RechargeTimeRemaining()
		{
			float num = configuration.GetRechargeTime() - configuration.GetRechargeTime() * poiCharge;
			int num2 = Mathf.CeilToInt(num / 600f);
			return (float)num2 * 600f;
		}

		public List<Descriptor> GetDescriptors(GameObject go)
		{
			return new List<Descriptor>();
		}
	}

	public State destroyOnArtifactSpawned;

	public State enter;

	public State waitingForPickup;

	public State recharging;

	public State spawnArtifact;

	public Signal OnHexCellInventoryChangedSignal;

	public FloatParameter poiCharge = new FloatParameter(1f);

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = enter;
		root.Enter(delegate(Instance smi)
		{
			if (smi.configuration == null || smi.configuration.typeId == HashedString.Invalid)
			{
				smi.configuration = smi.GetComponent<ArtifactPOIConfigurator>().MakeConfiguration();
				smi.poiCharge = 1f;
			}
		});
		enter.ParamTransition(poiCharge, spawnArtifact, IsFullyCharged).ParamTransition(poiCharge, waitingForPickup, IsNotFullyCharge);
		spawnArtifact.Enter(SpawnArtifactOnHexCellIfFullyCharged).EnterGoTo(waitingForPickup);
		waitingForPickup.OnSignal(OnHexCellInventoryChangedSignal, recharging, ThereIsNoArtifactInHexCell).EnterTransition(destroyOnArtifactSpawned, (Instance smi) => MarkedForDestroyAfterArtifactSpawned(smi) && IsArtifactAvailableInHexCell(smi));
		recharging.OnSignal(OnHexCellInventoryChangedSignal, waitingForPickup, IsArtifactAvailableInHexCell).ParamTransition(poiCharge, spawnArtifact, IsFullyCharged).EventHandler(GameHashes.NewDay, (Instance smi) => GameClock.Instance, AddDayWothOfCharge);
		destroyOnArtifactSpawned.Enter(SelfDestroy);
	}

	public static bool IsNotFullyCharge(Instance smi, float f)
	{
		return !IsFullyCharge(smi);
	}

	public static bool IsNotFullyCharge(Instance smi)
	{
		return !IsFullyCharge(smi);
	}

	public static bool IsFullyCharge(Instance smi)
	{
		return smi.sm.poiCharge.Get(smi) >= 1f;
	}

	public static bool IsFullyCharged(Instance smi, float f)
	{
		return smi.sm.poiCharge.Get(smi) >= 1f;
	}

	public static bool ThereIsNoArtifactInHexCell(Instance smi, SignalParameter param)
	{
		return ThereIsNoArtifactInHexCell(smi);
	}

	public static bool ThereIsNoArtifactInHexCell(Instance smi)
	{
		return !smi.HasArtifactAvailableInHexCell();
	}

	public static bool IsArtifactAvailableInHexCell(Instance smi, SignalParameter param)
	{
		return IsArtifactAvailableInHexCell(smi);
	}

	public static bool IsArtifactAvailableInHexCell(Instance smi)
	{
		return smi.HasArtifactAvailableInHexCell();
	}

	public static bool MarkedForDestroyAfterArtifactSpawned(Instance smi)
	{
		return smi.configuration.DestroyOnHarvest();
	}

	public static void ResetRechargeProgress(Instance smi)
	{
		smi.poiCharge = 0f;
	}

	public static void IncreaseArtifactSpawnedCount(Instance smi)
	{
		smi.IncreaseArtifactsSpawnedCount();
	}

	public static void SelfDestroy(Instance smi)
	{
		smi.gameObject.DeleteObject();
	}

	public static void AddDayWothOfCharge(Instance smi)
	{
		smi.RechargePOI(600f);
	}

	public static void SpawnArtifactOnHexCellIfFullyCharged(Instance smi)
	{
		if (IsFullyCharge(smi))
		{
			smi.SpawnArtifactOnHexCell();
			ResetRechargeProgress(smi);
			IncreaseArtifactSpawnedCount(smi);
		}
	}
}
