using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class CaloriesConsumedSecondaryExcretionMonitor : GameStateMachine<CaloriesConsumedSecondaryExcretionMonitor, CaloriesConsumedSecondaryExcretionMonitor.Instance>, IGameObjectEffectDescriptor
{
	public new class Instance : GameInstance
	{
		public CreatureCalorieMonitor.CaloriesConsumedEvent consumptionData;

		public Instance(IStateMachineTarget master)
			: base(master)
		{
		}

		public void OnStartChore(object o)
		{
			Chore chore = (Chore)o;
			if (chore.SatisfiesUrge(Db.Get().Urges.Fart))
			{
				GoTo(base.sm.idle);
			}
		}

		public void OnCaloriesConsumed(object data)
		{
			base.smi.consumptionData = ((Boxed<CreatureCalorieMonitor.CaloriesConsumedEvent>)data).value;
			base.smi.GoTo(base.smi.sm.schedule_fart);
		}
	}

	public State idle;

	public State schedule_fart;

	public State needs_to_fart;

	public SimHashes producedElement;

	public float kgProducedPerKcalConsumed = 1f;

	private float overpressureThreshold = 2f;

	private int handle;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = idle;
		base.serializable = SerializeType.Both_DEPRECATED;
		idle.PlayAnim("idle_loop", KAnim.PlayMode.Loop).Enter(delegate(Instance smi)
		{
			handle = smi.gameObject.Subscribe(-2038961714, smi.OnCaloriesConsumed);
		}).Exit(delegate(Instance smi)
		{
			smi.gameObject.Unsubscribe(handle);
		});
		schedule_fart.ScheduleGoTo((Instance smi) => Random.Range(3f, 6f), needs_to_fart);
		needs_to_fart.Enter(CreateChore).ToggleUrge(Db.Get().Urges.Fart).EventHandler(GameHashes.BeginChore, delegate(Instance smi, object o)
		{
			smi.OnStartChore(o);
		});
	}

	public static void CreateChore(Instance smi)
	{
		CreatureCalorieMonitor.CaloriesConsumedEvent consumptionData = smi.consumptionData;
		new FartChore(smi.GetComponent<ChoreProvider>(), Db.Get().ChoreTypes.Fart, consumptionData.calories * 0.001f * smi.sm.kgProducedPerKcalConsumed, smi.sm.producedElement, byte.MaxValue, 0, smi.sm.overpressureThreshold);
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		list.Add(new Descriptor(UI.BUILDINGEFFECTS.DIET_ADDITIONAL_PRODUCED.Replace("{Items}", ElementLoader.GetElement(producedElement.CreateTag()).name), UI.BUILDINGEFFECTS.TOOLTIPS.DIET_ADDITIONAL_PRODUCED.Replace("{Items}", ElementLoader.GetElement(producedElement.CreateTag()).name)));
		return list;
	}
}
