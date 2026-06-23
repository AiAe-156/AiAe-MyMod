using System;
using System.Collections.Generic;
using KSerialization;
using Klei.AI;
using UnityEngine;

[RequireComponent(typeof(Health))]
[AddComponentMenu("KMonoBehaviour/scripts/OxygenBreather")]
public class OxygenBreather : KMonoBehaviour, ISim200ms
{
	public interface IGasProvider
	{
		void OnSetOxygenBreather(OxygenBreather oxygen_breather);

		void OnClearOxygenBreather(OxygenBreather oxygen_breather);

		bool ConsumeGas(OxygenBreather oxygen_breather, float amount);

		bool ShouldEmitCO2();

		bool ShouldStoreCO2();

		bool IsLowOxygen();

		bool HasOxygen();

		bool IsBlocked();
	}

	private static Tag[] cannotBreathTags = new Tag[2]
	{
		GameTags.Dead,
		GameTags.SuffocatingIncapacitated
	};

	public float O2toCO2conversion = 0.5f;

	public Vector2 mouthOffset;

	[Serialize]
	public float accumulatedCO2;

	[SerializeField]
	public float minCO2ToEmit = 0.3f;

	private bool hasAir = true;

	private Timer hasAirTimer = new Timer();

	[MyCmpAdd]
	private Notifier notifier;

	[MyCmpGet]
	private Facing facing;

	[MyCmpGet]
	private KSelectable selectable;

	private HandleVector<int>.Handle o2Accumulator = HandleVector<int>.InvalidHandle;

	private HandleVector<int>.Handle co2Accumulator = HandleVector<int>.InvalidHandle;

	private AmountInstance temperature;

	public float lowOxygenThreshold;

	public float noOxygenThreshold;

	private AttributeInstance airConsumptionRate;

	public Action<SimHashes, float, float, byte, int> onBreathableGasConsumed;

	private static readonly EventSystem.IntraObjectHandler<OxygenBreather> OnDeadTagAddedDelegate = GameUtil.CreateHasTagHandler(GameTags.Dead, delegate(OxygenBreather component, object data)
	{
		component.OnDeath(data);
	});

	private List<IGasProvider> gasProviders = new List<IGasProvider>();

	private Guid o2StatusItem;

	private Guid cO2StatusItem;

	public KPrefabID prefabID { get; private set; }

	public float ConsumptionRate
	{
		get
		{
			if (airConsumptionRate != null)
			{
				return airConsumptionRate.GetTotalValue();
			}
			return 0f;
		}
	}

	public float CO2EmitRate => Game.Instance.accumulators.GetAverageRate(co2Accumulator);

	public HandleVector<int>.Handle O2Accumulator => o2Accumulator;

	public bool HasOxygen => hasAir;

	public bool IsOutOfOxygen => !hasAir;

	public IGasProvider GetCurrentGasProvider()
	{
		if (gasProviders.Count == 0)
		{
			return null;
		}
		IGasProvider result = null;
		for (int num = gasProviders.Count - 1; num >= 0; num--)
		{
			IGasProvider gasProvider = gasProviders[num];
			if (!gasProvider.IsBlocked())
			{
				result = gasProvider;
				if (gasProvider.HasOxygen())
				{
					break;
				}
			}
		}
		return result;
	}

	public bool IsLowOxygen()
	{
		return GetCurrentGasProvider()?.IsLowOxygen() ?? true;
	}

	protected override void OnPrefabInit()
	{
		GameUtil.SubscribeToTags(this, OnDeadTagAddedDelegate, triggerImmediately: true);
		prefabID = GetComponent<KPrefabID>();
	}

	protected override void OnSpawn()
	{
		airConsumptionRate = Db.Get().Attributes.AirConsumptionRate.Lookup(this);
		o2Accumulator = Game.Instance.accumulators.Add("O2", this);
		co2Accumulator = Game.Instance.accumulators.Add("CO2", this);
		bool flag = base.gameObject.PrefabID() == BionicMinionConfig.ID;
		o2StatusItem = selectable.AddStatusItem(flag ? Db.Get().DuplicantStatusItems.BreathingO2Bionic : Db.Get().DuplicantStatusItems.BreathingO2, this);
		cO2StatusItem = selectable.AddStatusItem(Db.Get().DuplicantStatusItems.EmittingCO2, this);
		temperature = Db.Get().Amounts.Temperature.Lookup(this);
		NameDisplayScreen.Instance.RegisterComponent(base.gameObject, this);
	}

	private void BreathableGasConsumed(SimHashes elementConsumed, float massConsumed, float temperature, byte disseaseIDX, int disseaseCount)
	{
		if (!prefabID.HasTag(GameTags.Dead) && !(O2Accumulator == HandleVector<int>.Handle.InvalidHandle))
		{
			if (elementConsumed == SimHashes.ContaminatedOxygen)
			{
				BoxingTrigger(-935848905, massConsumed);
			}
			Game.Instance.accumulators.Accumulate(O2Accumulator, massConsumed);
			float value = 0f - massConsumed;
			ReportManager.Instance.ReportValueWithPrefabInstanceContext(ReportManager.ReportType.OxygenCreated, value, prefabID, selectable.GetProperName());
			if (onBreathableGasConsumed != null)
			{
				onBreathableGasConsumed(elementConsumed, massConsumed, temperature, disseaseIDX, disseaseCount);
			}
		}
	}

	public static void BreathableGasConsumed(OxygenBreather breather, SimHashes elementConsumed, float massConsumed, float temperature, byte disseaseIDX, int disseaseCount)
	{
		if (breather != null)
		{
			breather.BreathableGasConsumed(elementConsumed, massConsumed, temperature, disseaseIDX, disseaseCount);
		}
	}

	public void Sim200ms(float dt)
	{
		if (prefabID.HasAnyTags(cannotBreathTags))
		{
			return;
		}
		float num = airConsumptionRate.GetTotalValue() * dt;
		IGasProvider currentGasProvider = GetCurrentGasProvider();
		bool flag = currentGasProvider?.ConsumeGas(this, num) ?? false;
		if (flag)
		{
			if (currentGasProvider.ShouldEmitCO2())
			{
				if (cO2StatusItem != Guid.Empty)
				{
					cO2StatusItem = selectable.AddStatusItem(Db.Get().DuplicantStatusItems.EmittingCO2, this);
				}
				float num2 = num * O2toCO2conversion;
				Game.Instance.accumulators.Accumulate(co2Accumulator, num2);
				accumulatedCO2 += num2;
				if (accumulatedCO2 >= minCO2ToEmit)
				{
					accumulatedCO2 -= minCO2ToEmit;
					Vector3 position = base.transform.GetPosition();
					Vector3 position2 = position;
					position2.x += (facing.GetFacing() ? (0f - mouthOffset.x) : mouthOffset.x);
					position2.y += mouthOffset.y;
					position2.z -= 0.5f;
					if (Mathf.FloorToInt(position2.x) != Mathf.FloorToInt(position.x))
					{
						position2.x = Mathf.Floor(position.x) + (facing.GetFacing() ? 0.01f : 0.99f);
					}
					CO2Manager.instance.SpawnBreath(position2, minCO2ToEmit, temperature.value, facing.GetFacing());
				}
			}
			else if (currentGasProvider.ShouldStoreCO2())
			{
				if (cO2StatusItem != Guid.Empty)
				{
					cO2StatusItem = selectable.AddStatusItem(Db.Get().DuplicantStatusItems.EmittingCO2, this);
				}
				Equippable equippable = GetComponent<SuitEquipper>().IsWearingAirtightSuit();
				if (equippable != null)
				{
					float num3 = num * O2toCO2conversion;
					Game.Instance.accumulators.Accumulate(co2Accumulator, num3);
					accumulatedCO2 += num3;
					if (accumulatedCO2 >= minCO2ToEmit)
					{
						accumulatedCO2 -= minCO2ToEmit;
						equippable.GetComponent<Storage>().AddGasChunk(SimHashes.CarbonDioxide, minCO2ToEmit, temperature.value, byte.MaxValue, 0, keep_zero_mass: false);
					}
				}
			}
			else if (cO2StatusItem != Guid.Empty)
			{
				selectable.RemoveStatusItem(cO2StatusItem);
				cO2StatusItem = Guid.Empty;
			}
		}
		if (flag != hasAir)
		{
			hasAirTimer.Start();
			if (hasAirTimer.TryStop(2f))
			{
				hasAir = flag;
				Trigger(-933153513, (object)BoxedBools.Box(hasAir));
			}
		}
		else
		{
			hasAirTimer.Stop();
		}
	}

	public void AddGasProvider(IGasProvider gas_provider)
	{
		Debug.Assert(gas_provider != null, "Error at OxygenBreather.cs  adding gas provider, the gas provider param is null!");
		Debug.Assert(!gasProviders.Contains(gas_provider), "Error at OxygenBreather.cs adding gas provider, the gas provider was already added to the gas providers list!");
		gasProviders.Add(gas_provider);
		gas_provider.OnSetOxygenBreather(this);
	}

	public bool RemoveGasProvider(IGasProvider provider)
	{
		if (gasProviders.Count > 0 && gasProviders.Contains(provider))
		{
			_ = gasProviders[gasProviders.Count - 1];
			gasProviders.Remove(provider);
			provider.OnClearOxygenBreather(this);
			return true;
		}
		return false;
	}

	private void OnDeath(object data)
	{
		base.enabled = false;
		selectable.RemoveStatusItem(Db.Get().DuplicantStatusItems.BreathingO2);
		selectable.RemoveStatusItem(Db.Get().DuplicantStatusItems.EmittingCO2);
	}

	protected override void OnCleanUp()
	{
		Game.Instance.accumulators.Remove(o2Accumulator);
		Game.Instance.accumulators.Remove(co2Accumulator);
		o2Accumulator = HandleVector<int>.InvalidHandle;
		co2Accumulator = HandleVector<int>.InvalidHandle;
		while (gasProviders.Count > 0)
		{
			IGasProvider provider = gasProviders[gasProviders.Count - 1];
			RemoveGasProvider(provider);
		}
		base.OnCleanUp();
	}
}
