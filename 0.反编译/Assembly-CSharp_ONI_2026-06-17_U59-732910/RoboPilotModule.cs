using System;

public class RoboPilotModule : KMonoBehaviour
{
	private MeterController meter;

	private Storage databankStorage;

	private ManualDeliveryKG manualDeliveryChore;

	public int dataBankConsumption = 2;

	public bool consumeDataBanksOnLand;

	private static CellOffset[] dataDeliveryOffsets = new CellOffset[7]
	{
		new CellOffset(0, 0),
		new CellOffset(1, 0),
		new CellOffset(2, 0),
		new CellOffset(3, 0),
		new CellOffset(-1, 0),
		new CellOffset(-2, 0),
		new CellOffset(-3, 0)
	};

	protected override void OnSpawn()
	{
		databankStorage = GetComponent<Storage>();
		manualDeliveryChore = GetComponent<ManualDeliveryKG>();
		meter = new MeterController(GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer, "meter_target", "meter_fill", "meter_frame");
		meter.gameObject.GetComponent<KBatchedAnimTracker>().matchParentOffset = true;
		UpdateMeter();
		databankStorage.SetOffsets(dataDeliveryOffsets);
		Subscribe(-1697596308, UpdateMeter);
		Subscribe(-778359855, PlayDeliveryAnimation);
		Subscribe(-887025858, OnRocketLanded);
		RocketModuleCluster component = GetComponent<RocketModuleCluster>();
		if (component != null)
		{
			component.CraftInterface.Subscribe(1655598572, OnLaunchConditionChanged);
			component.CraftInterface.Subscribe(543433792, RequestDataBanksForDestination);
		}
		else
		{
			Subscribe(705820818, OnRocketLaunched);
			GetComponent<RocketModule>().FindLaunchConditionManager().Subscribe(929158128, RequestDataBanksForDestination);
		}
		RequestDataBanksForDestination();
	}

	private void RequestDataBanksForDestination(object _ = null)
	{
		int num = -1;
		RocketModuleCluster component = GetComponent<RocketModuleCluster>();
		if (component != null)
		{
			ClusterTraveler component2 = component.CraftInterface.GetComponent<ClusterTraveler>();
			if (component2 != null && component2.CurrentPath != null)
			{
				num = component2.RemainingTravelNodes() * 2;
			}
		}
		else
		{
			LaunchConditionManager launchConditionManager = GetComponent<RocketModule>().FindLaunchConditionManager();
			if (launchConditionManager != null)
			{
				SpaceDestination spacecraftDestination = SpacecraftManager.instance.GetSpacecraftDestination(launchConditionManager);
				if (spacecraftDestination != null)
				{
					num = spacecraftDestination.OneBasedDistance * 2;
				}
			}
		}
		if (num > 0 && !HasResourcesToMove(num))
		{
			manualDeliveryChore.refillMass = MathF.Min(ResourcesRequiredToMove(num), databankStorage.Capacity() - databankStorage.UnitsStored());
		}
	}

	protected override void OnCleanUp()
	{
		Unsubscribe(-1697596308, UpdateMeter);
		Unsubscribe(-887025858, OnRocketLanded);
		Unsubscribe(-778359855, PlayDeliveryAnimation);
		RocketModuleCluster component = GetComponent<RocketModuleCluster>();
		if (component != null)
		{
			component.CraftInterface.Unsubscribe(1655598572, OnLaunchConditionChanged);
			component.CraftInterface.Unsubscribe(543433792, RequestDataBanksForDestination);
		}
		else
		{
			Unsubscribe(705820818, OnRocketLaunched);
			GetComponent<RocketModule>().FindLaunchConditionManager().Unsubscribe(929158128, RequestDataBanksForDestination);
		}
		base.OnCleanUp();
	}

	private void OnLaunchConditionChanged(object data)
	{
		RocketModuleCluster component = GetComponent<RocketModuleCluster>();
		if (component != null && component.CraftInterface.IsLaunchRequested())
		{
			component.CraftInterface.GetComponent<Clustercraft>().Launch();
		}
	}

	private void OnRocketLanded(object o)
	{
		if (consumeDataBanksOnLand)
		{
			LaunchConditionManager lcm = GetComponent<RocketModule>().FindLaunchConditionManager();
			Spacecraft spacecraftFromLaunchConditionManager = SpacecraftManager.instance.GetSpacecraftFromLaunchConditionManager(lcm);
			float amount = Math.Min(SpacecraftManager.instance.GetSpacecraftDestination(spacecraftFromLaunchConditionManager.id).OneBasedDistance * dataBankConsumption * 2, databankStorage.MassStored());
			databankStorage.ConsumeIgnoringDisease(DatabankHelper.TAG, amount);
		}
		RequestDataBanksForDestination();
	}

	private void OnRocketLaunched(object o)
	{
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		component.Play("launch_pre");
		component.Queue("launch");
		component.Queue("launch_pst");
	}

	public void ConsumeDataBanksInFlight()
	{
		if (databankStorage != null)
		{
			databankStorage.ConsumeIgnoringDisease(DatabankHelper.TAG, dataBankConsumption);
		}
	}

	private void PlayDeliveryAnimation(object data = null)
	{
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		HashedString currentAnim = component.currentAnim;
		component.Play("databank_delivery_reaction");
		component.Queue(currentAnim);
	}

	private void UpdateMeter(object data = null)
	{
		meter.SetPositionPercent(databankStorage.MassStored() / databankStorage.Capacity());
	}

	public bool HasResourcesToMove(int distance)
	{
		return databankStorage.UnitsStored() >= (float)(distance * dataBankConsumption);
	}

	public float ResourcesRequiredToMove(int distance)
	{
		return distance * dataBankConsumption;
	}

	public bool IsFull()
	{
		return databankStorage.MassStored() >= databankStorage.Capacity();
	}

	public float GetDataBanksStored()
	{
		if (!(databankStorage != null))
		{
			return 0f;
		}
		return databankStorage.UnitsStored();
	}

	public float GetDataBankRange()
	{
		if (databankStorage == null)
		{
			return 0f;
		}
		if (consumeDataBanksOnLand)
		{
			return databankStorage.UnitsStored() / (float)dataBankConsumption * RoboPilotCommandModuleConfig.DATABANKRANGE;
		}
		return databankStorage.UnitsStored() / (float)dataBankConsumption * 600f;
	}

	public float GetMaxDataBankRange()
	{
		if (databankStorage == null)
		{
			return 0f;
		}
		if (consumeDataBanksOnLand)
		{
			return databankStorage.Capacity() / (float)dataBankConsumption * RoboPilotCommandModuleConfig.DATABANKRANGE;
		}
		return databankStorage.Capacity() / (float)dataBankConsumption * 600f;
	}
}
