using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class RocketSimpleInfoPanel : SimpleInfoPanel
{
	private Dictionary<string, GameObject> cargoBayLabels = new Dictionary<string, GameObject>();

	private Dictionary<string, GameObject> artifactModuleLabels = new Dictionary<string, GameObject>();

	public RocketSimpleInfoPanel(SimpleInfoScreen simpleInfoScreen)
		: base(simpleInfoScreen)
	{
	}

	public override void Refresh(CollapsibleDetailContentPanel rocketStatusContainer, GameObject selectedTarget)
	{
		if (selectedTarget == null)
		{
			simpleInfoRoot.StoragePanel.gameObject.SetActive(value: false);
			return;
		}
		RocketModuleCluster rocketModuleCluster = null;
		Clustercraft clusterCraft = null;
		CraftModuleInterface craftModuleInterface = null;
		GetRocketStuffFromTarget(selectedTarget, ref rocketModuleCluster, ref clusterCraft, ref craftModuleInterface);
		rocketStatusContainer.gameObject.SetActive(craftModuleInterface != null || rocketModuleCluster != null);
		if (craftModuleInterface != null)
		{
			RocketEngineCluster engine = craftModuleInterface.GetEngine();
			string arg;
			string text;
			if (engine != null && engine.GetComponent<HEPFuelTank>() != null)
			{
				arg = GameUtil.GetFormattedHighEnergyParticles(craftModuleInterface.FuelPerHex);
				text = GameUtil.GetFormattedHighEnergyParticles(craftModuleInterface.FuelRemaining);
			}
			else
			{
				arg = GameUtil.GetFormattedMass(craftModuleInterface.FuelPerHex);
				text = GameUtil.GetFormattedMass(craftModuleInterface.FuelRemaining);
			}
			string text2 = string.Concat(UI.CLUSTERMAP.ROCKETS.RANGE.TOOLTIP, "\n    • ", string.Format(UI.CLUSTERMAP.ROCKETS.FUEL_PER_HEX.NAME, arg), "\n    • ", UI.CLUSTERMAP.ROCKETS.FUEL_REMAINING.NAME, text, "\n    • ", UI.CLUSTERMAP.ROCKETS.OXIDIZER_REMAINING.NAME, GameUtil.GetFormattedMass(craftModuleInterface.OxidizerPowerRemaining));
			bool is_robo_pilot;
			RocketModuleCluster primaryPilotModule = craftModuleInterface.GetPrimaryPilotModule(out is_robo_pilot);
			if (is_robo_pilot)
			{
				RoboPilotModule component = primaryPilotModule.GetComponent<RoboPilotModule>();
				text2 = text2 + "\n" + string.Format(UI.CLUSTERMAP.ROCKETS.RANGE.ROBO_PILOTED_TOOLTIP, component.dataBankConsumption, component.GetDataBanksStored());
			}
			rocketStatusContainer.SetLabel("RangeRemaining", string.Concat(UI.CLUSTERMAP.ROCKETS.RANGE.NAME, GameUtil.GetFormattedRocketRange(craftModuleInterface.RangeInTiles)), text2);
			string text3 = string.Concat(UI.CLUSTERMAP.ROCKETS.SPEED.TOOLTIP, "\n    • ", UI.CLUSTERMAP.ROCKETS.POWER_TOTAL.NAME, craftModuleInterface.EnginePower.ToString(), "\n    • ", UI.CLUSTERMAP.ROCKETS.BURDEN_TOTAL.NAME, craftModuleInterface.TotalBurden.ToString());
			Clustercraft component2 = craftModuleInterface.GetComponent<Clustercraft>();
			if (component2 != null)
			{
				text3 += UI.CLUSTERMAP.ROCKETS.SPEED.PILOT_SPEED_MODIFIER;
				bool flag = craftModuleInterface.GetPassengerModule();
				bool flag2 = craftModuleInterface.GetRobotPilotModule();
				component2.GetPilotedStatus(out var dupe_piloted, out var robo_piloted);
				if (dupe_piloted)
				{
					text3 = text3 + "\n    • " + UI.CLUSTERMAP.ROCKETS.SPEED.DUPEPILOT_SPEED_TOOLTIP.Replace("{speed_boost}", GameUtil.GetFormattedPercent(component2.PilotSkillMultiplier - 1f));
				}
				if (dupe_piloted && robo_piloted)
				{
					text3 = text3 + "\n    • " + UI.CLUSTERMAP.ROCKETS.SPEED.SUPERPILOTED_SPEED_TOOLTIP.Replace("{speed_boost}", GameUtil.GetFormattedPercent(50f));
				}
				else if (flag && !dupe_piloted)
				{
					text3 = text3 + "\n    • " + UI.CLUSTERMAP.ROCKETS.SPEED.UNPILOTED_SPEED_TOOLTIP.Replace("{speed_boost}", GameUtil.GetFormattedPercent(50f));
				}
				else if (robo_piloted)
				{
					text3 = text3 + "\n    • " + UI.CLUSTERMAP.ROCKETS.SPEED.ROBO_PILOT_ONLY_SPEED_TOOLTIP;
				}
				else if (flag2 && !flag)
				{
					text3 = text3 + "\n    • " + UI.CLUSTERMAP.ROCKETS.SPEED.DEAD_ROBO_PILOT_ONLY_SPEED_TOOLTIP;
				}
			}
			rocketStatusContainer.SetLabel("Speed", string.Concat(UI.CLUSTERMAP.ROCKETS.SPEED.NAME, GameUtil.GetFormattedRocketRangePerCycle(craftModuleInterface.Speed)), text3);
			if (craftModuleInterface.GetEngine() != null)
			{
				string tooltip = string.Format(UI.CLUSTERMAP.ROCKETS.MAX_HEIGHT.TOOLTIP, craftModuleInterface.GetEngine().GetProperName(), craftModuleInterface.MaxHeight.ToString());
				rocketStatusContainer.SetLabel("MaxHeight", string.Format(UI.CLUSTERMAP.ROCKETS.MAX_HEIGHT.NAME, craftModuleInterface.RocketHeight.ToString(), craftModuleInterface.MaxHeight.ToString()), tooltip);
			}
			rocketStatusContainer.SetLabel("RocketSpacer2", "", "");
			if (clusterCraft != null)
			{
				foreach (KeyValuePair<string, GameObject> artifactModuleLabel in artifactModuleLabels)
				{
					artifactModuleLabel.Value.SetActive(value: false);
				}
				int num = 0;
				foreach (Ref<RocketModuleCluster> clusterModule in clusterCraft.ModuleInterface.ClusterModules)
				{
					ArtifactModule component3 = clusterModule.Get().GetComponent<ArtifactModule>();
					if (component3 != null)
					{
						string text4 = "";
						rocketStatusContainer.SetLabel(text: (!(component3.Occupant != null)) ? $"{component3.GetProperName()}: {UI.CLUSTERMAP.ROCKETS.ARTIFACT_MODULE.EMPTY}" : (component3.GetProperName() + ": " + component3.Occupant.GetProperName()), id: "artifactModule_" + num, tooltip: "");
						num++;
					}
				}
				List<CargoBayCluster> allCargoBays = clusterCraft.GetAllCargoBays();
				bool flag3 = allCargoBays != null && allCargoBays.Count > 0;
				foreach (KeyValuePair<string, GameObject> cargoBayLabel in cargoBayLabels)
				{
					cargoBayLabel.Value.SetActive(value: false);
				}
				if (flag3)
				{
					ListPool<Tuple<string, TextStyleSetting>, SimpleInfoScreen>.PooledList pooledList = ListPool<Tuple<string, TextStyleSetting>, SimpleInfoScreen>.Allocate();
					int num2 = 0;
					foreach (CargoBayCluster item in allCargoBays)
					{
						pooledList.Clear();
						Storage storage = item.storage;
						string text5 = $"{item.GetComponent<KPrefabID>().GetProperName()}: {GameUtil.GetFormattedMass(storage.MassStored())}/{GameUtil.GetFormattedMass(storage.capacityKg)}";
						foreach (GameObject item2 in storage.GetItems())
						{
							KPrefabID component4 = item2.GetComponent<KPrefabID>();
							PrimaryElement component5 = item2.GetComponent<PrimaryElement>();
							string a = $"{component4.GetProperName()} : {GameUtil.GetFormattedMass(component5.Mass)}";
							pooledList.Add(new Tuple<string, TextStyleSetting>(a, PluginAssets.Instance.defaultTextStyleSetting));
						}
						string text6 = "";
						for (int i = 0; i < pooledList.Count; i++)
						{
							text6 += pooledList[i].first;
							if (i != pooledList.Count - 1)
							{
								text6 += "\n";
							}
						}
						rocketStatusContainer.SetLabel("cargoBay_" + num2, text5, text6);
						num2++;
					}
					pooledList.Recycle();
				}
			}
		}
		if (rocketModuleCluster != null)
		{
			rocketStatusContainer.SetLabel("ModuleStats", string.Concat(UI.CLUSTERMAP.ROCKETS.MODULE_STATS.NAME, selectedTarget.GetProperName()), UI.CLUSTERMAP.ROCKETS.MODULE_STATS.TOOLTIP);
			float burden = rocketModuleCluster.performanceStats.Burden;
			float enginePower = rocketModuleCluster.performanceStats.EnginePower;
			if (burden != 0f)
			{
				rocketStatusContainer.SetLabel("LocalBurden", string.Concat("    • ", UI.CLUSTERMAP.ROCKETS.BURDEN_MODULE.NAME, burden.ToString()), string.Format(UI.CLUSTERMAP.ROCKETS.BURDEN_MODULE.TOOLTIP, burden));
			}
			if (enginePower != 0f)
			{
				rocketStatusContainer.SetLabel("LocalPower", string.Concat("    • ", UI.CLUSTERMAP.ROCKETS.POWER_MODULE.NAME, enginePower.ToString()), string.Format(UI.CLUSTERMAP.ROCKETS.POWER_MODULE.TOOLTIP, enginePower));
			}
		}
		rocketStatusContainer.Commit();
	}

	public static void GetRocketStuffFromTarget(GameObject selectedTarget, ref RocketModuleCluster rocketModuleCluster, ref Clustercraft clusterCraft, ref CraftModuleInterface craftModuleInterface)
	{
		rocketModuleCluster = selectedTarget.GetComponent<RocketModuleCluster>();
		clusterCraft = selectedTarget.GetComponent<Clustercraft>();
		craftModuleInterface = null;
		if (rocketModuleCluster != null)
		{
			craftModuleInterface = rocketModuleCluster.CraftInterface;
			if (clusterCraft == null)
			{
				clusterCraft = craftModuleInterface.GetComponent<Clustercraft>();
			}
		}
		else if (clusterCraft != null)
		{
			craftModuleInterface = clusterCraft.ModuleInterface;
		}
	}
}
