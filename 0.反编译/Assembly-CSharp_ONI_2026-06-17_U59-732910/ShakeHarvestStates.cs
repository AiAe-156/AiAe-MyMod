using STRINGS;
using UnityEngine;

public class ShakeHarvestStates : GameStateMachine<ShakeHarvestStates, ShakeHarvestStates.Instance, IStateMachineTarget, ShakeHarvestStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.WantsToHarvest);
			base.sm.harvester.Set(base.gameObject, this);
		}
	}

	private readonly ApproachSubState<IApproachable> approach;

	private readonly State harvest;

	private readonly State complete;

	private readonly State failed;

	private readonly TargetParameter harvester;

	private readonly TargetParameter plant;

	private static StatusItem GoingToHarvestStatus(Instance smi)
	{
		return MakeStatus(smi, CREATURES.STATUSITEMS.GOING_TO_HARVEST.NAME, CREATURES.STATUSITEMS.GOING_TO_HARVEST.TOOLTIP);
	}

	private static StatusItem HarvestingStatus(Instance smi)
	{
		return MakeStatus(smi, CREATURES.STATUSITEMS.HARVESTING.NAME, CREATURES.STATUSITEMS.HARVESTING.TOOLTIP);
	}

	private static StatusItem MakeStatus(Instance smi, string name, string tooltip)
	{
		return new StatusItem(smi.GetCurrentState().longName, name, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString));
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.Never;
		default_state = approach;
		root.Enter(delegate(Instance smi)
		{
			ShakeHarvestMonitor.Instance sMI = smi.GetSMI<ShakeHarvestMonitor.Instance>();
			plant.Set(sMI.sm.plant.Get(sMI), smi);
		});
		approach.InitializeStates(harvester, plant, delegate(Instance smi)
		{
			ListPool<CellOffset, ShakeHarvestStates>.PooledList pooledList = ListPool<CellOffset, ShakeHarvestStates>.Allocate();
			ShakeHarvestMonitor.Def.GetApproachOffsets(plant.Get(smi), pooledList);
			CellOffset[] result = pooledList.ToArray();
			pooledList.Recycle();
			return result;
		}, harvest, failed).ToggleMainStatusItem(GoingToHarvestStatus).OnTargetLost(plant, failed)
			.Target(plant)
			.EventTransition(GameHashes.Harvest, failed)
			.EventTransition(GameHashes.Uprooted, failed)
			.EventTransition(GameHashes.QueueDestroyObject, failed);
		harvest.PlayAnim("shake", KAnim.PlayMode.Once).ToggleMainStatusItem(HarvestingStatus).OnAnimQueueComplete(complete)
			.OnTargetLost(plant, failed);
		complete.Enter(delegate(Instance smi)
		{
			GameObject gameObject = plant.Get(smi);
			if (!gameObject.IsNullOrDestroyed())
			{
				Harvestable component = gameObject.GetComponent<Harvestable>();
				if (component != null && component.CanBeHarvested)
				{
					component.Trigger(2127324410, (object)BoxedBools.True);
					component.Harvest();
				}
			}
		}).BehaviourComplete(GameTags.Creatures.WantsToHarvest);
		failed.Enter(delegate(Instance smi)
		{
			ShakeHarvestMonitor.Instance sMI = smi.GetSMI<ShakeHarvestMonitor.Instance>();
			sMI?.sm.failed.Trigger(sMI);
		}).EnterGoTo(null);
	}
}
