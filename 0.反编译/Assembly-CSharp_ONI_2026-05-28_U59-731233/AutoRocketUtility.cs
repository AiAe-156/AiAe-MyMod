using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AutoRocketUtility
{
	public static void StartAutoRocket(LaunchPad selectedPad)
	{
		selectedPad.StartCoroutine(AutoRocketRoutine(selectedPad));
	}

	private static IEnumerator AutoRocketRoutine(LaunchPad selectedPad)
	{
		GameObject engine = AddEngine(selectedPad);
		GameObject oxidizerTank = AddOxidizerTank(engine);
		yield return SequenceUtil.WaitForEndOfFrame;
		AddOxidizer(oxidizerTank);
		GameObject commandModule = AddPassengerModule(oxidizerTank);
		GameObject storageModule = AddSolidStorageModule(commandModule);
		AddDrillCone(storageModule);
		PassengerRocketModule passengerModule = commandModule.GetComponent<PassengerRocketModule>();
		ClustercraftExteriorDoor exteriorDoor = passengerModule.GetComponent<ClustercraftExteriorDoor>();
		int max = 100;
		while (exteriorDoor.GetInteriorDoor() == null && max > 0)
		{
			max--;
			yield return SequenceUtil.WaitForEndOfFrame;
		}
		RocketModuleCluster rocketModuleCluster = passengerModule.GetComponent<RocketModuleCluster>();
		CraftModuleInterface craftModuleInterface = rocketModuleCluster.CraftInterface;
		WorldContainer interiorWorld = craftModuleInterface.GetInteriorWorld();
		RocketControlStation station = Components.RocketControlStations.GetWorldItems(interiorWorld.id)[0];
		GameObject minion = AddPilot(station);
		AddOxygen(station);
		yield return SequenceUtil.WaitForEndOfFrame;
		AssignCrew(minion, passengerModule);
	}

	private static GameObject AddEngine(LaunchPad selectedPad)
	{
		BuildingDef buildingDef = Assets.GetBuildingDef("KeroseneEngineClusterSmall");
		List<Tag> elements = new List<Tag> { SimHashes.Steel.CreateTag() };
		GameObject gameObject = selectedPad.AddBaseModule(buildingDef, elements);
		RocketEngineCluster component = gameObject.GetComponent<RocketEngineCluster>();
		Element element = ElementLoader.GetElement(component.fuelTag);
		Storage component2 = gameObject.GetComponent<Storage>();
		if (element.IsGas)
		{
			component2.AddGasChunk(element.id, component2.Capacity(), element.defaultValues.temperature, byte.MaxValue, 0, keep_zero_mass: false);
		}
		else if (element.IsLiquid)
		{
			component2.AddLiquid(element.id, component2.Capacity(), element.defaultValues.temperature, byte.MaxValue, 0);
		}
		else if (element.IsSolid)
		{
			component2.AddOre(element.id, component2.Capacity(), element.defaultValues.temperature, byte.MaxValue, 0);
		}
		return gameObject;
	}

	private static GameObject AddPassengerModule(GameObject baseModule)
	{
		ReorderableBuilding component = baseModule.GetComponent<ReorderableBuilding>();
		BuildingDef buildingDef = Assets.GetBuildingDef("HabitatModuleMedium");
		List<Tag> buildMaterials = new List<Tag> { SimHashes.Cuprite.CreateTag() };
		return component.AddModule(buildingDef, buildMaterials);
	}

	private static GameObject AddSolidStorageModule(GameObject baseModule)
	{
		ReorderableBuilding component = baseModule.GetComponent<ReorderableBuilding>();
		BuildingDef buildingDef = Assets.GetBuildingDef("SolidCargoBaySmall");
		List<Tag> buildMaterials = new List<Tag> { SimHashes.Steel.CreateTag() };
		return component.AddModule(buildingDef, buildMaterials);
	}

	private static GameObject AddDrillCone(GameObject baseModule)
	{
		ReorderableBuilding component = baseModule.GetComponent<ReorderableBuilding>();
		BuildingDef buildingDef = Assets.GetBuildingDef("NoseconeHarvest");
		List<Tag> buildMaterials = new List<Tag>
		{
			SimHashes.Steel.CreateTag(),
			SimHashes.Polypropylene.CreateTag()
		};
		GameObject gameObject = component.AddModule(buildingDef, buildMaterials);
		gameObject.GetComponent<Storage>().AddOre(SimHashes.Diamond, 1000f, 273f, byte.MaxValue, 0);
		return gameObject;
	}

	private static GameObject AddOxidizerTank(GameObject baseModule)
	{
		ReorderableBuilding component = baseModule.GetComponent<ReorderableBuilding>();
		BuildingDef buildingDef = Assets.GetBuildingDef("SmallOxidizerTank");
		List<Tag> buildMaterials = new List<Tag> { SimHashes.Cuprite.CreateTag() };
		return component.AddModule(buildingDef, buildMaterials);
	}

	private static void AddOxidizer(GameObject oxidizerTank)
	{
		SimHashes simHashes = SimHashes.OxyRock;
		Element element = ElementLoader.FindElementByHash(simHashes);
		DiscoveredResources.Instance.Discover(element.tag, element.GetMaterialCategoryTag());
		oxidizerTank.GetComponent<OxidizerTank>().DEBUG_FillTank(simHashes);
	}

	private static GameObject AddPilot(RocketControlStation station)
	{
		MinionStartingStats minionStartingStats = new MinionStartingStats(is_starter_minion: false, null, null, isDebugMinion: true);
		Vector3 position = station.transform.position;
		GameObject prefab = Assets.GetPrefab(BaseMinionConfig.GetMinionIDForModel(minionStartingStats.personality.model));
		GameObject gameObject = Util.KInstantiate(prefab);
		gameObject.name = prefab.name;
		Immigration.Instance.ApplyDefaultPersonalPriorities(gameObject);
		Vector3 position2 = Grid.CellToPosCBC(Grid.PosToCell(position), Grid.SceneLayer.Move);
		gameObject.transform.SetLocalPosition(position2);
		gameObject.SetActive(value: true);
		minionStartingStats.Apply(gameObject);
		MinionResume component = gameObject.GetComponent<MinionResume>();
		if (DebugHandler.InstantBuildMode && component.AvailableSkillpoints < 1)
		{
			component.ForceAddSkillPoint();
		}
		string id = Db.Get().Skills.RocketPiloting1.Id;
		MinionResume.SkillMasteryConditions[] skillMasteryConditions = component.GetSkillMasteryConditions(id);
		bool flag = component.CanMasterSkill(skillMasteryConditions);
		if (component != null && !component.HasMasteredSkill(id) && flag)
		{
			component.MasterSkill(id);
		}
		return gameObject;
	}

	private static void AddOxygen(RocketControlStation station)
	{
		SimMessages.ReplaceElement(Grid.PosToCell(station.transform.position + Vector3.up * 2f), SimHashes.OxyRock, CellEventLogger.Instance.DebugTool, 1000f, 273f);
	}

	private static void AssignCrew(GameObject minion, PassengerRocketModule passengerModule)
	{
		for (int i = 0; i < Components.MinionAssignablesProxy.Count; i++)
		{
			if (Components.MinionAssignablesProxy[i].GetTargetGameObject() == minion)
			{
				passengerModule.GetComponent<AssignmentGroupController>().SetMember(Components.MinionAssignablesProxy[i], isAllowed: true);
				break;
			}
		}
		passengerModule.RequestCrewBoard(PassengerRocketModule.RequestCrewState.Request);
	}

	private static void SetDestination(CraftModuleInterface craftModuleInterface, PassengerRocketModule passengerModule)
	{
		ClusterDestinationSelector component = craftModuleInterface.GetComponent<ClusterDestinationSelector>();
		component.SetDestination(passengerModule.GetMyWorldLocation() + AxialI.NORTHEAST);
	}
}
