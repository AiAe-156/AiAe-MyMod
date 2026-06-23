using KSerialization;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/ResearchCenter")]
public class DataMiner : ComplexFabricator
{
	[MyCmpReq]
	private PrimaryElement pe;

	[Serialize]
	private float minEfficiency = DataMinerConfig.PRODUCTION_RATE_SCALE.max;

	private MeterController meter;

	public float OperatingTemp => pe.Temperature;

	public float TemperatureScaleFactor => 1f - DataMinerConfig.TEMPERATURE_SCALING_RANGE.LerpFactorClamped(OperatingTemp);

	public float EfficiencyRate => DataMinerConfig.PRODUCTION_RATE_SCALE.Lerp(TemperatureScaleFactor);

	protected override float ComputeWorkProgress(float dt, ComplexRecipe recipe)
	{
		float efficiencyRate = EfficiencyRate;
		minEfficiency = Mathf.Min(minEfficiency, efficiencyRate);
		return base.ComputeWorkProgress(dt, recipe) * efficiencyRate;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		meter = new MeterController(this, Meter.Offset.Infront, Grid.SceneLayer.NoLayer);
		GetComponent<KSelectable>().AddStatusItem(Db.Get().BuildingStatusItems.DataMinerEfficiency, this);
	}

	public override void CompleteWorkingOrder()
	{
		if (minEfficiency == DataMinerConfig.PRODUCTION_RATE_SCALE.max)
		{
			SaveGame.Instance.ColonyAchievementTracker.efficientlyGatheredData = true;
		}
		minEfficiency = DataMinerConfig.PRODUCTION_RATE_SCALE.max;
		base.CompleteWorkingOrder();
	}

	public override void Sim1000ms(float dt)
	{
		base.Sim1000ms(dt);
		meter.SetPositionPercent(TemperatureScaleFactor);
	}
}
