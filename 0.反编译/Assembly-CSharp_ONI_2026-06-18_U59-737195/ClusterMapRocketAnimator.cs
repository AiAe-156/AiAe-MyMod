using System;
using System.Collections.Generic;
using UnityEngine;

public class ClusterMapRocketAnimator : GameStateMachine<ClusterMapRocketAnimator, ClusterMapRocketAnimator.StatesInstance, ClusterMapVisualizer>
{
	public class TravelingStates : State
	{
		public State regular;

		public State boosted;
	}

	public class MovingStates : State
	{
		public State takeoff;

		public TravelingStates traveling;

		public State landing;
	}

	public class UtilityStates : State
	{
		public class CollectingStates : State
		{
			public State pre;

			public State loop;

			public State pst;
		}

		public CollectingStates collecting;
	}

	public class StatesInstance : GameInstance
	{
		public ClusterGridEntity entity;

		private KBatchedAnimController drillConeSubAnim;

		private int animCompleteHandle = -1;

		private GameObject animCompleteSubscriber;

		public StatesInstance(ClusterMapVisualizer master, ClusterGridEntity entity)
			: base(master)
		{
			this.entity = entity;
			base.sm.entityTarget.Set(entity, this);
		}

		public override void StartSM()
		{
			GetComponent<ClusterMapVisualizer>().GetFirstAnimController();
			base.StartSM();
		}

		public void PlayVisAnim(string animName, KAnim.PlayMode playMode)
		{
			GetComponent<ClusterMapVisualizer>().PlayAnim(animName, playMode);
		}

		public void ToggleVisAnim(bool on)
		{
			ClusterMapVisualizer component = GetComponent<ClusterMapVisualizer>();
			if (!on)
			{
				component.GetFirstAnimController().Play("grounded");
			}
		}

		public void SubscribeOnVisAnimComplete(Action<object> action)
		{
			ClusterMapVisualizer component = GetComponent<ClusterMapVisualizer>();
			UnsubscribeOnVisAnimComplete();
			animCompleteSubscriber = component.GetFirstAnimController().gameObject;
			animCompleteHandle = animCompleteSubscriber.Subscribe(-1061186183, action);
		}

		public void UnsubscribeOnVisAnimComplete()
		{
			if (animCompleteHandle != -1)
			{
				DebugUtil.DevAssert(animCompleteSubscriber != null, "ClusterMapRocketAnimator animCompleteSubscriber GameObject is null. Whatever the previous gameObject in this variable was, it may not have unsubscribed from an event properly");
				animCompleteSubscriber.Unsubscribe(animCompleteHandle);
				animCompleteHandle = -1;
			}
		}

		public void RefreshDrillConeVisibility()
		{
			List<ResourceHarvestModule.StatesInstance> allResourceHarvestModules = ((Clustercraft)base.smi.entity).GetAllResourceHarvestModules();
			bool drillConeVisibility = allResourceHarvestModules != null && allResourceHarvestModules.Count > 0;
			SetDrillConeVisibility(drillConeVisibility);
		}

		private void SetDrillConeVisibility(bool shouldBeVisible)
		{
			if (shouldBeVisible)
			{
				if (drillConeSubAnim == null)
				{
					drillConeSubAnim = CreateSymbolController("nose_target", require_sound: true);
				}
				drillConeSubAnim.gameObject.SetActive(value: true);
			}
			else
			{
				if (drillConeSubAnim != null)
				{
					drillConeSubAnim.gameObject.SetActive(value: false);
				}
				GetComponent<ClusterMapVisualizer>().GetFirstAnimController().SetSymbolVisiblity("nose_target", is_visible: false);
			}
		}

		public void PlayDrillingAnimation()
		{
			if (drillConeSubAnim != null)
			{
				drillConeSubAnim.Play("drilling_loop", KAnim.PlayMode.Loop);
			}
		}

		public void PlayIdleDrillConeAnimation()
		{
			if (drillConeSubAnim != null)
			{
				drillConeSubAnim.Play("drill_cone_idle");
			}
		}

		private void DeleteDrillConeSubAnim()
		{
			if (drillConeSubAnim != null)
			{
				drillConeSubAnim.gameObject.DeleteObject();
				drillConeSubAnim = null;
			}
		}

		protected override void OnCleanUp()
		{
			base.OnCleanUp();
			DeleteDrillConeSubAnim();
			UnsubscribeOnVisAnimComplete();
		}

		private KBatchedAnimController CreateSymbolController(string symbolName, bool require_sound = false)
		{
			KBatchedAnimController firstAnimController = GetComponent<ClusterMapVisualizer>().GetFirstAnimController();
			KBatchedAnimController kBatchedAnimController = CreateEmptyKAnimController(symbolName, firstAnimController);
			kBatchedAnimController.transform.SetParent(firstAnimController.transform, worldPositionStays: false);
			kBatchedAnimController.initialAnim = "drill_cone_idle";
			KBatchedAnimTracker kBatchedAnimTracker = kBatchedAnimController.gameObject.AddComponent<KBatchedAnimTracker>();
			HashedString symbol = new HashedString(symbolName);
			kBatchedAnimTracker.controller = firstAnimController;
			kBatchedAnimTracker.symbol = symbol;
			kBatchedAnimTracker.forceAlwaysVisible = false;
			if (require_sound)
			{
				kBatchedAnimController.gameObject.AddComponent<LoopingSounds>();
			}
			kBatchedAnimController.gameObject.SetActive(value: false);
			firstAnimController.SetSymbolVisiblity(symbolName, is_visible: false);
			return kBatchedAnimController;
		}

		private KBatchedAnimController CreateEmptyKAnimController(string name, KBatchedAnimController animController)
		{
			GameObject obj = new GameObject(base.gameObject.name + "-" + name);
			obj.SetActive(value: false);
			KBatchedAnimController kBatchedAnimController = obj.AddComponent<KBatchedAnimController>();
			kBatchedAnimController.AnimFiles = new KAnimFile[1] { Assets.GetAnim("rocket01_kanim") };
			kBatchedAnimController.materialType = KAnimBatchGroup.MaterialType.UI;
			kBatchedAnimController.animScale = ((animController == null) ? 0.08f : animController.animScale);
			kBatchedAnimController.fgLayer = Grid.SceneLayer.NoLayer;
			kBatchedAnimController.sceneLayer = Grid.SceneLayer.NoLayer;
			kBatchedAnimController.forceUseGameTime = true;
			return kBatchedAnimController;
		}
	}

	public TargetParameter entityTarget;

	public State idle;

	public State grounded;

	public MovingStates moving;

	public UtilityStates utility;

	public State exploding;

	public State exploding_pst;

	public const string DRILLCONE_METER_TARGET_NAME = "nose_target";

	public const string DRILLCONE_DEFAULT_ANIM_NAME = "drill_cone_idle";

	public const string DRILLCONE_DRILL_ANIM_NAME = "drilling_loop";

	public override void InitializeStates(out BaseState defaultState)
	{
		defaultState = idle;
		root.Enter(RefreshDrillConeSymbol).Transition(null, entityTarget.IsNull).Target(entityTarget)
			.EventHandlerTransition(GameHashes.RocketSelfDestructRequested, exploding, (StatesInstance smi, object data) => true)
			.TagTransition(GameTags.RocketCollectingResources, utility.collecting)
			.EventHandlerTransition(GameHashes.RocketLaunched, moving.takeoff, (StatesInstance smi, object data) => true);
		idle.Enter(RefreshDillingAnimations).Target(masterTarget).Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("idle_loop", KAnim.PlayMode.Loop);
		})
			.Target(entityTarget)
			.EventHandler(GameHashes.TagsChanged, RefreshDillingAnimations)
			.Transition(moving.traveling, IsTraveling)
			.Transition(grounded, IsGrounded)
			.Transition(moving.landing, IsLanding)
			.Transition(utility.collecting, IsCollectingResourcesFromHexCell);
		grounded.Enter(delegate(StatesInstance smi)
		{
			ToggleSelectable(isSelectable: false, smi);
			smi.ToggleVisAnim(on: false);
		}).Exit(delegate(StatesInstance smi)
		{
			ToggleSelectable(isSelectable: true, smi);
			smi.ToggleVisAnim(on: true);
			RefreshDrillConeSymbol(smi);
		}).Target(entityTarget)
			.EventTransition(GameHashes.RocketLaunched, moving.takeoff);
		moving.takeoff.Transition(idle, GameStateMachine<ClusterMapRocketAnimator, StatesInstance, ClusterMapVisualizer, object>.Not(IsSurfaceTransitioning)).Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("launching", KAnim.PlayMode.Loop);
			ToggleSelectable(isSelectable: false, smi);
		}).Exit(delegate(StatesInstance smi)
		{
			ToggleSelectable(isSelectable: true, smi);
		});
		moving.Enter(RefreshDillingAnimations).Target(entityTarget).EventHandler(GameHashes.TagsChanged, RefreshDillingAnimations);
		moving.landing.Transition(idle, GameStateMachine<ClusterMapRocketAnimator, StatesInstance, ClusterMapVisualizer, object>.Not(IsSurfaceTransitioning)).Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("landing", KAnim.PlayMode.Loop);
			ToggleSelectable(isSelectable: false, smi);
		}).Exit(delegate(StatesInstance smi)
		{
			ToggleSelectable(isSelectable: true, smi);
		});
		moving.traveling.DefaultState(moving.traveling.regular).Target(entityTarget).EventTransition(GameHashes.ClusterLocationChanged, idle, GameStateMachine<ClusterMapRocketAnimator, StatesInstance, ClusterMapVisualizer, object>.Not(IsTraveling))
			.EventTransition(GameHashes.ClusterDestinationChanged, idle, GameStateMachine<ClusterMapRocketAnimator, StatesInstance, ClusterMapVisualizer, object>.Not(IsTraveling));
		moving.traveling.regular.Target(entityTarget).Transition(moving.traveling.boosted, IsBoosted).Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("inflight_loop", KAnim.PlayMode.Loop);
		});
		moving.traveling.boosted.Target(entityTarget).Transition(moving.traveling.regular, GameStateMachine<ClusterMapRocketAnimator, StatesInstance, ClusterMapVisualizer, object>.Not(IsBoosted)).Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("boosted", KAnim.PlayMode.Loop);
		});
		utility.Target(entityTarget).EventTransition(GameHashes.ClusterDestinationChanged, idle, IsTraveling).EventHandler(GameHashes.TagsChanged, RefreshDillingAnimations)
			.Enter(RefreshDillingAnimations);
		utility.collecting.DefaultState(utility.collecting.pre).Target(entityTarget).TagTransition(GameTags.RocketCollectingResources, utility.collecting.pst, on_remove: true)
			.ToggleStatusItem(Db.Get().BuildingStatusItems.CollectingHexCellInventoryItems);
		utility.collecting.pre.Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("mining_pre", KAnim.PlayMode.Once);
			smi.SubscribeOnVisAnimComplete(delegate
			{
				smi.GoTo(utility.collecting.loop);
			});
		});
		utility.collecting.loop.Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("mining_loop", KAnim.PlayMode.Loop);
		});
		utility.collecting.pst.Enter(delegate(StatesInstance smi)
		{
			smi.PlayVisAnim("mining_pst", KAnim.PlayMode.Once);
			smi.SubscribeOnVisAnimComplete(delegate
			{
				smi.GoTo(idle);
			});
		});
		exploding.Enter(delegate(StatesInstance smi)
		{
			smi.GetComponent<ClusterMapVisualizer>().GetFirstAnimController().SwapAnims(new KAnimFile[1] { Assets.GetAnim("rocket_self_destruct_kanim") });
			smi.PlayVisAnim("explode", KAnim.PlayMode.Once);
			smi.SubscribeOnVisAnimComplete(delegate
			{
				smi.GoTo(exploding_pst);
			});
		});
		exploding_pst.Enter(delegate(StatesInstance smi)
		{
			smi.GetComponent<ClusterMapVisualizer>().GetFirstAnimController().Stop();
			smi.entity.gameObject.Trigger(-1311384361);
		});
	}

	private static void RefreshDillingAnimations(StatesInstance smi)
	{
		if (IsDrilling(smi))
		{
			smi.PlayDrillingAnimation();
		}
		else
		{
			smi.PlayIdleDrillConeAnimation();
		}
	}

	private static void RefreshDrillConeSymbol(StatesInstance smi)
	{
		smi.RefreshDrillConeVisibility();
	}

	private bool ClusterChangedAtMyLocation(StatesInstance smi, object data)
	{
		ClusterLocationChangedEvent clusterLocationChangedEvent = (ClusterLocationChangedEvent)data;
		if (!(clusterLocationChangedEvent.oldLocation == smi.entity.Location))
		{
			return clusterLocationChangedEvent.newLocation == smi.entity.Location;
		}
		return true;
	}

	private bool IsTraveling(StatesInstance smi)
	{
		if (smi.entity.GetComponent<ClusterTraveler>().IsTraveling())
		{
			return ((Clustercraft)smi.entity).HasResourcesToMove();
		}
		return false;
	}

	private bool IsBoosted(StatesInstance smi)
	{
		return ((Clustercraft)smi.entity).controlStationBuffTimeRemaining > 0f;
	}

	private bool IsGrounded(StatesInstance smi)
	{
		return ((Clustercraft)smi.entity).Status == Clustercraft.CraftStatus.Grounded;
	}

	private bool IsLanding(StatesInstance smi)
	{
		return ((Clustercraft)smi.entity).Status == Clustercraft.CraftStatus.Landing;
	}

	private static bool IsDrilling(StatesInstance smi)
	{
		return ((Clustercraft)smi.entity).HasTag(GameTags.RocketDrilling);
	}

	private bool IsCollectingResourcesFromHexCell(StatesInstance smi)
	{
		return ((Clustercraft)smi.entity).HasTag(GameTags.RocketCollectingResources);
	}

	private bool IsSurfaceTransitioning(StatesInstance smi)
	{
		Clustercraft clustercraft = smi.entity as Clustercraft;
		if (clustercraft != null)
		{
			if (clustercraft.Status != Clustercraft.CraftStatus.Landing)
			{
				return clustercraft.Status == Clustercraft.CraftStatus.Launching;
			}
			return true;
		}
		return false;
	}

	private void ToggleSelectable(bool isSelectable, StatesInstance smi)
	{
		if (!smi.entity.IsNullOrDestroyed())
		{
			KSelectable component = smi.entity.GetComponent<KSelectable>();
			component.IsSelectable = isSelectable;
			if (!isSelectable && component.IsSelected && ClusterMapScreen.Instance.GetMode() != ClusterMapScreen.Mode.SelectDestination)
			{
				ClusterMapSelectTool.Instance.Select(null, skipSound: true);
				SelectTool.Instance.Select(null, skipSound: true);
			}
		}
	}
}
