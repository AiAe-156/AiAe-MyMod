using System;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class MinionConfig : IEntityConfig
{
	public static Tag MODEL = GameTags.Minions.Models.Standard;

	public static string NAME = DUPLICANTS.MODEL.STANDARD.NAME;

	public static string ID = MODEL.ToString();

	public Func<RationalAi.Instance, StateMachine.Instance>[] RATIONAL_AI_STATE_MACHINES = BaseMinionConfig.BaseRationalAiStateMachines().Append(new Func<RationalAi.Instance, StateMachine.Instance>[9]
	{
		(RationalAi.Instance smi) => new BreathMonitor.Instance(smi.master),
		(RationalAi.Instance smi) => new SteppedInMonitor.Instance(smi.master),
		(RationalAi.Instance smi) => new Dreamer.Instance(smi.master),
		(RationalAi.Instance smi) => new StaminaMonitor.Instance(smi.master),
		(RationalAi.Instance smi) => new RationMonitor.Instance(smi.master),
		(RationalAi.Instance smi) => new CalorieMonitor.Instance(smi.master),
		(RationalAi.Instance smi) => new BladderMonitor.Instance(smi.master),
		(RationalAi.Instance smi) => new HygieneMonitor.Instance(smi.master),
		(RationalAi.Instance smi) => new TiredMonitor.Instance(smi.master)
	});

	public static string[] GetAttributes()
	{
		return BaseMinionConfig.BaseMinionAttributes().Append(new string[2]
		{
			Db.Get().Attributes.FoodExpectation.Id,
			Db.Get().Attributes.ToiletEfficiency.Id
		});
	}

	public static string[] GetAmounts()
	{
		return BaseMinionConfig.BaseMinionAmounts().Append(new string[3]
		{
			Db.Get().Amounts.Bladder.Id,
			Db.Get().Amounts.Stamina.Id,
			Db.Get().Amounts.Calories.Id
		});
	}

	public static AttributeModifier[] GetTraits()
	{
		return BaseMinionConfig.BaseMinionTraits(MODEL).Append(new AttributeModifier[6]
		{
			new AttributeModifier(Db.Get().Attributes.FoodExpectation.Id, DUPLICANTSTATS.GetStatsFor(MODEL).BaseStats.FOOD_QUALITY_EXPECTATION, NAME),
			new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, DUPLICANTSTATS.GetStatsFor(MODEL).BaseStats.MAX_CALORIES, NAME),
			new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, DUPLICANTSTATS.GetStatsFor(MODEL).BaseStats.CALORIES_BURNED_PER_SECOND, NAME),
			new AttributeModifier(Db.Get().Amounts.Stamina.deltaAttribute.Id, DUPLICANTSTATS.GetStatsFor(MODEL).BaseStats.STAMINA_USED_PER_SECOND, NAME),
			new AttributeModifier(Db.Get().Amounts.Bladder.deltaAttribute.Id, DUPLICANTSTATS.GetStatsFor(MODEL).BaseStats.BLADDER_INCREASE_PER_SECOND, NAME),
			new AttributeModifier(Db.Get().Attributes.ToiletEfficiency.Id, DUPLICANTSTATS.GetStatsFor(MODEL).BaseStats.TOILET_EFFICIENCY, NAME)
		});
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = BaseMinionConfig.BaseMinion(MODEL, GetAttributes(), GetAmounts(), GetTraits());
		CodexEntryRedirector codexEntryRedirector = gameObject.AddOrGet<CodexEntryRedirector>();
		codexEntryRedirector.CodexID = "DUPLICANTS";
		return gameObject;
	}

	public void OnPrefabInit(GameObject go)
	{
		BaseMinionConfig.BasePrefabInit(go, MODEL);
		DUPLICANTSTATS statsFor = DUPLICANTSTATS.GetStatsFor(MODEL);
		AmountInstance amountInstance = Db.Get().Amounts.Bladder.Lookup(go);
		amountInstance.value = UnityEngine.Random.Range(0f, 10f);
		AmountInstance amountInstance2 = Db.Get().Amounts.Calories.Lookup(go);
		amountInstance2.value = (statsFor.BaseStats.HUNGRY_THRESHOLD + statsFor.BaseStats.SATISFIED_THRESHOLD) * 0.5f * amountInstance2.GetMax();
		AmountInstance amountInstance3 = Db.Get().Amounts.Stamina.Lookup(go);
		amountInstance3.value = amountInstance3.GetMax();
	}

	public void OnSpawn(GameObject go)
	{
		Sensors component = go.GetComponent<Sensors>();
		component.Add(new ToiletSensor(component));
		BaseMinionConfig.BaseOnSpawn(go, MODEL, RATIONAL_AI_STATE_MACHINES);
		go.GetComponent<OxygenBreather>().AddGasProvider(new GasBreatherFromWorldProvider());
		go.Trigger(1589886948, (object)go);
	}
}
