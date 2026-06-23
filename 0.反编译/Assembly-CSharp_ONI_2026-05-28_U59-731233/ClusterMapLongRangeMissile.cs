using System.Collections.Generic;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class ClusterMapLongRangeMissile : GameStateMachine<ClusterMapLongRangeMissile, ClusterMapLongRangeMissile.StatesInstance, IStateMachineTarget, ClusterMapLongRangeMissile.Def>
{
	public class Def : BaseDef
	{
	}

	public class TravellingStates : State
	{
		public State moving;

		public State idle;
	}

	public class StatesInstance : GameInstance
	{
		[Serialize]
		public bool exploded = false;

		public KBatchedAnimController animController;

		public StatesInstance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			animController = GetComponent<KBatchedAnimController>();
		}

		public void Setup(AxialI source, ClusterGridEntity target, MissileLongRangeProjectile.Def projectile_def)
		{
			BallisticClusterGridEntity component = GetComponent<BallisticClusterGridEntity>();
			component.nameKey = new StringKey(projectile_def.missileName);
			InfoDescription component2 = GetComponent<InfoDescription>();
			component2.description = Strings.Get(projectile_def.missileDesc);
			component.SwapSymbolFromSameAnim("payload", projectile_def.starmapOverrideSymbol);
			KAnim.Build.Symbol symbol = animController.AnimFiles[0].GetData().build.GetSymbol(projectile_def.starmapOverrideSymbol);
			animController.GetComponent<SymbolOverrideController>().AddSymbolOverride("payload", symbol);
			base.sm.targetObject.Set(target.gameObject, this);
			Travel(source, FindInterceptPoint(source, target, GetComponent<ClusterDestinationSelector>()));
		}

		public static AxialI FindInterceptPoint(AxialI source, ClusterGridEntity target, ClusterDestinationSelector selector, int maxGridRange = 99999)
		{
			ClusterTraveler component = target.GetComponent<ClusterTraveler>();
			if (component != null)
			{
				List<AxialI> currentPath = component.CurrentPath;
				AxialI result = target.Location;
				foreach (AxialI item in currentPath)
				{
					float num = component.TravelETA(item);
					List<AxialI> path = ClusterGrid.Instance.GetPath(source, item, selector);
					if (path != null && path.Count != 0 && path.Count <= maxGridRange)
					{
						float num2 = (float)path.Count * 600f / 10f;
						if (num2 < num)
						{
							return result;
						}
					}
					result = item;
				}
			}
			return target.Location;
		}

		public float InterceptETA()
		{
			ClusterTraveler component = GetComponent<ClusterTraveler>();
			float a = 0f;
			float b = component.TravelETA();
			GameObject gameObject = base.sm.targetObject.Get(this);
			if (gameObject != null)
			{
				ClusterTraveler component2 = gameObject.GetComponent<ClusterTraveler>();
				if (component2 != null && component.CurrentPath != null)
				{
					a = component2.TravelETA(component.Destination);
				}
			}
			return Mathf.Max(a, b);
		}

		public void Travel(AxialI source, AxialI destination)
		{
			GetComponent<BallisticClusterGridEntity>().Configure(source, destination);
			base.sm.destinationHex.Set(destination, this);
			GoTo(base.sm.travelling.moving);
		}

		public bool IsTraveling()
		{
			ClusterTraveler component = GetComponent<ClusterTraveler>();
			return component.CurrentPath != null && component.CurrentPath.Count != 0;
		}
	}

	public TargetParameter targetObject;

	public AxialIParameter destinationHex;

	public State initialization;

	public TravellingStates travelling;

	public State contact;

	public State exploding_with_visual;

	public State cleanup;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = initialization;
		base.serializable = SerializeType.ParamsOnly;
		root.ToggleTag(GameTags.EntityInSpace);
		initialization.Enter(delegate(StatesInstance smi)
		{
			if (smi.exploded)
			{
				smi.GoTo(smi.sm.cleanup);
			}
			else if (targetObject.Get(smi) != null)
			{
				smi.GoTo(smi.sm.travelling.moving);
			}
			else
			{
				smi.GoTo(smi.sm.contact);
			}
		});
		travelling.ToggleStatusItem(Db.Get().MiscStatusItems.LongRangeMissileTTI).OnTargetLost(targetObject, contact).Target(targetObject)
			.EventHandler(GameHashes.ClusterLocationChanged, UpdatePath)
			.Target(masterTarget);
		travelling.moving.ToggleTag(GameTags.LongRangeMissileMoving).EnterTransition(travelling.idle, (StatesInstance smi) => !smi.IsTraveling()).EventTransition(GameHashes.ClusterDestinationReached, travelling.idle);
		travelling.idle.ToggleTag(GameTags.LongRangeMissileIdle).Transition(contact, HitTarget, UpdateRate.SIM_1000ms).Transition(contact, GameStateMachine<ClusterMapLongRangeMissile, StatesInstance, IStateMachineTarget, Def>.Not(CanHitTarget), UpdateRate.SIM_1000ms);
		contact.Enter(TriggerDamage).EnterTransition(exploding_with_visual, HasVisualizer).EnterTransition(cleanup, GameStateMachine<ClusterMapLongRangeMissile, StatesInstance, IStateMachineTarget, Def>.Not(HasVisualizer));
		exploding_with_visual.ToggleTag(GameTags.LongRangeMissileExploding).EventTransition(GameHashes.RocketExploded, cleanup);
		cleanup.Enter(delegate(StatesInstance smi)
		{
			smi.gameObject.DeleteObject();
		}).GoTo(null);
	}

	private static bool HasVisualizer(StatesInstance smi)
	{
		if (smi == null)
		{
			return false;
		}
		return ClusterMapScreen.Instance.GetEntityVisAnim(smi.GetComponent<ClusterGridEntity>()) != null;
	}

	public static void TriggerDamage(StatesInstance smi)
	{
		GameObject gameObject = smi.sm.targetObject.Get(smi);
		if (gameObject != null && CanHitTarget(smi))
		{
			gameObject.Trigger(-2056344675, (object)MissileLongRangeConfig.DamageEventPayload.sharedInstance);
		}
		smi.exploded = true;
	}

	public static bool HitTarget(StatesInstance smi)
	{
		ClusterGridEntity clusterGridEntity = smi.sm.targetObject.Get<ClusterGridEntity>(smi);
		if (clusterGridEntity == null)
		{
			return false;
		}
		return clusterGridEntity.Location == smi.sm.destinationHex.Get(smi);
	}

	public static bool CanHitTarget(StatesInstance smi)
	{
		return smi.sm.targetObject.Get(smi) != null;
	}

	private static void UpdatePath(StatesInstance smi)
	{
		ClusterDestinationSelector component = smi.GetComponent<ClusterDestinationSelector>();
		if (component == null)
		{
			return;
		}
		ClusterGridEntity clusterGridEntity = smi.sm.targetObject.Get<ClusterGridEntity>(smi);
		if (!(clusterGridEntity == null))
		{
			ClusterGridEntity component2 = smi.GetComponent<ClusterGridEntity>();
			AxialI axialI = StatesInstance.FindInterceptPoint(component2.Location, clusterGridEntity, component);
			if (axialI != smi.sm.destinationHex.Get(smi))
			{
				smi.Travel(component2.Location, axialI);
			}
		}
	}
}
