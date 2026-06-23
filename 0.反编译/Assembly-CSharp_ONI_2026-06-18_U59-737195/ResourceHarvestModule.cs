using System.Collections.Generic;
using UnityEngine;

public class ResourceHarvestModule : GameStateMachine<ResourceHarvestModule, ResourceHarvestModule.StatesInstance, IStateMachineTarget, ResourceHarvestModule.Def>
{
	public class Def : BaseDef
	{
		public float harvestSpeed;
	}

	public class NotGroundedStates : State
	{
		public State not_drilling;

		public State drilling;
	}

	public class StatesInstance : GameInstance
	{
		private MeterController meter;

		private Storage storage;

		private int onStorageChangeHandle = -1;

		public StatesInstance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			storage = GetComponent<Storage>();
			GetComponent<RocketModule>().AddModuleCondition(ProcessCondition.ProcessConditionType.RocketStorage, new ConditionHasResource(storage, SimHashes.Diamond, 1000f));
			onStorageChangeHandle = Subscribe(-1697596308, UpdateMeter);
			meter = new MeterController(GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer, "meter_target", "meter_fill", "meter_frame", "meter_OL");
			KBatchedAnimTracker component = meter.gameObject.GetComponent<KBatchedAnimTracker>();
			component.matchParentOffset = true;
			component.forceAlwaysAlive = true;
			UpdateMeter();
		}

		protected override void OnCleanUp()
		{
			base.OnCleanUp();
			Unsubscribe(ref onStorageChangeHandle);
		}

		public void UpdateMeter(object data = null)
		{
			meter.SetPositionPercent(storage.MassStored() / storage.Capacity());
		}

		public void HarvestFromPOI(float dt)
		{
			Clustercraft component = GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>();
			if (!CheckIfCanDrill())
			{
				return;
			}
			ClusterGridEntity pOIAtCurrentLocation = component.GetPOIAtCurrentLocation();
			if (pOIAtCurrentLocation == null || pOIAtCurrentLocation.GetComponent<HarvestablePOIClusterGridEntity>() == null)
			{
				return;
			}
			StarmapHexCellInventory starmapHexCellInventory = ClusterGrid.Instance.AddOrGetHexCellInventory(component.Location);
			HarvestablePOIStates.Instance sMI = pOIAtCurrentLocation.GetSMI<HarvestablePOIStates.Instance>();
			Dictionary<SimHashes, float> elementsWithWeights = sMI.configuration.GetElementsWithWeights();
			float num = 0f;
			foreach (KeyValuePair<SimHashes, float> item in elementsWithWeights)
			{
				num += item.Value;
			}
			foreach (KeyValuePair<SimHashes, float> item2 in elementsWithWeights)
			{
				Element element = ElementLoader.FindElementByHash(item2.Key);
				if (!DiscoveredResources.Instance.IsDiscovered(element.tag))
				{
					DiscoveredResources.Instance.Discover(element.tag, element.GetMaterialCategoryTag());
				}
			}
			float num2 = Mathf.Min(GetMaxExtractKGFromDiamondAvailable(), base.def.harvestSpeed * dt);
			float num3 = 0f;
			foreach (KeyValuePair<SimHashes, float> item3 in elementsWithWeights)
			{
				if (num3 >= num2)
				{
					break;
				}
				SimHashes key = item3.Key;
				float num4 = item3.Value / num;
				float num5 = base.def.harvestSpeed * dt * num4;
				Element element2 = ElementLoader.FindElementByHash(key);
				starmapHexCellInventory.AddItem(element2, num5);
				num3 += num5;
			}
			sMI.DeltaPOICapacity(0f - num3);
			ConsumeDiamond(num3 * 0.05f);
			SaveGame.Instance.ColonyAchievementTracker.totalMaterialsHarvestFromPOI += num3;
		}

		public void ConsumeDiamond(float amount)
		{
			GetComponent<Storage>().ConsumeIgnoringDisease(SimHashes.Diamond.CreateTag(), amount);
		}

		public bool HasAnyAmountOfDiamond()
		{
			return GetComponent<Storage>().GetAmountAvailable(SimHashes.Diamond.CreateTag()) > 0f;
		}

		public float GetMaxExtractKGFromDiamondAvailable()
		{
			return GetComponent<Storage>().GetAmountAvailable(SimHashes.Diamond.CreateTag()) / 0.05f;
		}

		public bool CheckIfCanDrill()
		{
			Clustercraft component = GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>();
			if (component == null)
			{
				base.sm.canHarvest.Set(value: false, this);
				return false;
			}
			if (!HasAnyAmountOfDiamond())
			{
				base.sm.canHarvest.Set(value: false, this);
				return false;
			}
			ClusterGridEntity pOIAtCurrentLocation = component.GetPOIAtCurrentLocation();
			bool flag = false;
			if (pOIAtCurrentLocation != null && (bool)pOIAtCurrentLocation.GetComponent<HarvestablePOIClusterGridEntity>())
			{
				flag = pOIAtCurrentLocation.GetSMI<HarvestablePOIStates.Instance>().POICanBeHarvested();
			}
			base.sm.canHarvest.Set(flag, this);
			return flag;
		}

		public static void AddHarvestStatusItems(GameObject statusTarget, StatesInstance smi)
		{
			statusTarget.GetComponent<KSelectable>().AddStatusItem(Db.Get().BuildingStatusItems.SpacePOIHarvesting, smi);
		}

		public static void RemoveHarvestStatusItems(GameObject statusTarget)
		{
			statusTarget.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().BuildingStatusItems.SpacePOIHarvesting);
		}
	}

	public BoolParameter canHarvest;

	public FloatParameter lastHarvestTime;

	public State grounded;

	public NotGroundedStates not_grounded;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = grounded;
		root.Enter(delegate(StatesInstance smi)
		{
			smi.CheckIfCanDrill();
		});
		grounded.TagTransition(GameTags.RocketNotOnGround, not_grounded).Enter(delegate(StatesInstance smi)
		{
			smi.UpdateMeter();
		});
		not_grounded.DefaultState(not_grounded.not_drilling).EventHandler(GameHashes.ClusterLocationChanged, (StatesInstance smi) => Game.Instance, delegate(StatesInstance smi)
		{
			smi.CheckIfCanDrill();
		}).EventHandler(GameHashes.OnStorageChange, delegate(StatesInstance smi)
		{
			smi.CheckIfCanDrill();
		})
			.TagTransition(GameTags.RocketNotOnGround, grounded, on_remove: true);
		not_grounded.not_drilling.PlayAnim("loaded").ParamTransition(canHarvest, not_grounded.drilling, GameStateMachine<ResourceHarvestModule, StatesInstance, IStateMachineTarget, Def>.IsTrue).Enter(delegate(StatesInstance smi)
		{
			StatesInstance.RemoveHarvestStatusItems(smi.master.gameObject.GetComponent<RocketModuleCluster>().CraftInterface.gameObject);
		})
			.Update(delegate(StatesInstance smi, float dt)
			{
				smi.CheckIfCanDrill();
			}, UpdateRate.SIM_4000ms);
		not_grounded.drilling.PlayAnim("deploying").Exit(delegate(StatesInstance smi)
		{
			smi.master.gameObject.GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>().Trigger(939543986);
			StatesInstance.RemoveHarvestStatusItems(smi.master.gameObject.GetComponent<RocketModuleCluster>().CraftInterface.gameObject);
		}).Enter(delegate(StatesInstance smi)
		{
			Clustercraft component = smi.master.gameObject.GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>();
			component.AddTag(GameTags.RocketDrilling);
			component.Trigger(-1762453998);
			StatesInstance.AddHarvestStatusItems(smi.master.gameObject.GetComponent<RocketModuleCluster>().CraftInterface.gameObject, smi);
		})
			.Exit(delegate(StatesInstance smi)
			{
				smi.master.gameObject.GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>().RemoveTag(GameTags.RocketDrilling);
			})
			.Update(delegate(StatesInstance smi, float dt)
			{
				smi.HarvestFromPOI(dt);
				lastHarvestTime.Set(Time.time, smi);
			}, UpdateRate.SIM_4000ms)
			.ParamTransition(canHarvest, not_grounded.not_drilling, GameStateMachine<ResourceHarvestModule, StatesInstance, IStateMachineTarget, Def>.IsFalse);
	}
}
