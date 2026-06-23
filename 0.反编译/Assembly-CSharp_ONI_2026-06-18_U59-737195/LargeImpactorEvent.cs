using System;
using System.Collections.Generic;
using KSerialization;
using Klei.CustomSettings;
using ProcGen;
using UnityEngine;

public class LargeImpactorEvent : GameplayEvent<LargeImpactorEvent.StatesInstance>
{
	public class States : GameplayEventStateMachine<States, StatesInstance, GameplayEventManager, LargeImpactorEvent>
	{
		public State start;

		public State create;

		public State clusterMap;

		public State killedByPlayer;

		public State impacting;

		public State finished;

		[Serialize]
		public TargetParameter impactorTarget = new TargetParameter();

		public override void InitializeStates(out BaseState default_state)
		{
			base.serializable = SerializeType.ParamsOnly;
			base.InitializeStates(out default_state);
			default_state = start;
			start.ParamTransition(impactorTarget, create, GameStateMachine<States, StatesInstance, GameplayEventManager, object>.IsNull).ParamTransition(impactorTarget, clusterMap, GameStateMachine<States, StatesInstance, GameplayEventManager, object>.IsNotNull);
			create.ParamTransition(impactorTarget, clusterMap, GameStateMachine<States, StatesInstance, GameplayEventManager, object>.IsNotNull).Enter(delegate(StatesInstance smi)
			{
				CreateImpactorInstance(smi);
			});
			clusterMap.Target(impactorTarget).EventTransition(GameHashes.LargeImpactorArrived, impacting).EventTransition(GameHashes.Died, killedByPlayer);
			impacting.EnterTransition(finished, WasWinAchievementAlreadyGranted).Enter(RegisterLandedCycle).Enter(InitializeLandingSequence)
				.Target(impactorTarget)
				.EventTransition(GameHashes.SequenceCompleted, finished);
			killedByPlayer.EnterTransition(finished, WasWinAchievementAlreadyGranted).Enter(PrepareForLargeImpactorDefeatedSequence).Enter(PreventDemoliorFragmentsBGFromPlaying)
				.Enter(UnlockWinAchievement)
				.Enter(RegisterDemoliorSize)
				.Exit(AllowDemoliorFragmentsBGFromPlaying)
				.Exit(SpawnIridiumShowers)
				.Target(impactorTarget)
				.EventHandler(GameHashes.SequenceCompleted, HandleInterception);
			finished.Enter(delegate(StatesInstance smi)
			{
				Util.KDestroyGameObject(smi.sm.impactorTarget.Get(smi));
			}).Enter(DestroyEventInstance).GoTo(null);
		}
	}

	public class StatesInstance : GameplayEventStateMachine<States, StatesInstance, GameplayEventManager, LargeImpactorEvent>.GameplayEventStateMachineInstance
	{
		public GameObject impactorInstance => base.sm.impactorTarget.Get(base.smi);

		public StatesInstance(GameplayEventManager master, GameplayEventInstance eventInstance, LargeImpactorEvent largeImpactorEvent)
			: base(master, eventInstance, largeImpactorEvent)
		{
		}
	}

	public LargeImpactorEvent(string id, string[] requiredDlcIds, string[] forbiddenDlcIds)
		: base(id, 0, 0, requiredDlcIds, forbiddenDlcIds)
	{
	}

	public override StateMachine.Instance GetSMI(GameplayEventManager manager, GameplayEventInstance eventInstance)
	{
		return new StatesInstance(manager, eventInstance, this);
	}

	private static void SpawnIridiumShowers(StatesInstance smi)
	{
		GameplayEventManager.Instance.StartNewEvent(Db.Get().GameplayEvents.IridiumShowerEvent, smi.eventInstance.worldId);
	}

	private static void PreventDemoliorFragmentsBGFromPlaying(StatesInstance smi)
	{
		TerrainBG.preventLargeImpactorFragmentsFromProgressing = true;
	}

	private static void AllowDemoliorFragmentsBGFromPlaying(StatesInstance smi)
	{
		TerrainBG.preventLargeImpactorFragmentsFromProgressing = false;
	}

	private static void DestroyEventInstance(StatesInstance smi)
	{
		smi.eventInstance.smi.StopSM("end");
	}

	private static bool WasWinAchievementAlreadyGranted(StatesInstance smi)
	{
		return SaveGame.Instance.ColonyAchievementTracker.IsAchievementUnlocked(Db.Get().ColonyAchievements.AsteroidDestroyed);
	}

	private static void UnlockWinAchievement(StatesInstance smi)
	{
		SaveGame.Instance.ColonyAchievementTracker.largeImpactorState = ColonyAchievementTracker.LargeImpactorState.Defeated;
	}

	private static void RegisterDemoliorSize(StatesInstance smi)
	{
		ParallaxBackgroundObject component = smi.impactorInstance.GetComponent<ParallaxBackgroundObject>();
		SaveGame.Instance.ColonyAchievementTracker.LargeImpactorBackgroundScale = component.lastScaleUsed;
	}

	private static void RegisterLandedCycle(StatesInstance smi)
	{
		SaveGame.Instance.ColonyAchievementTracker.largeImpactorState = ColonyAchievementTracker.LargeImpactorState.Landed;
		SaveGame.Instance.ColonyAchievementTracker.largeImpactorLandedCycle = GameClock.Instance.GetCycle();
	}

	private static bool IsSuitablePOISpawnLocation(AxialI location)
	{
		if (!ClusterGrid.Instance.IsValidCell(location))
		{
			return false;
		}
		foreach (ClusterGridEntity item in ClusterGrid.Instance.GetEntitiesOnCell(location))
		{
			if (item.Layer == EntityLayer.Asteroid || item.Layer == EntityLayer.POI)
			{
				return false;
			}
		}
		return true;
	}

	private static List<AxialI> FindAvailablePOISpawnLocations(AxialI location)
	{
		List<AxialI> list = new List<AxialI>();
		if (IsSuitablePOISpawnLocation(location))
		{
			list.Add(location);
		}
		for (int i = 1; i <= 2; i++)
		{
			foreach (AxialI dIRECTION in AxialI.DIRECTIONS)
			{
				AxialI axialI = location + dIRECTION * i;
				if (IsSuitablePOISpawnLocation(axialI))
				{
					list.Add(axialI);
				}
			}
		}
		return list;
	}

	private static void SpawnPOI(string id, AxialI location)
	{
		GameObject gameObject = Util.KInstantiate(Assets.GetPrefab(id));
		gameObject.GetComponent<HarvestablePOIClusterGridEntity>().Init(location);
		gameObject.SetActive(value: true);
	}

	private static void HandleInterception(StatesInstance smi)
	{
		if (DlcManager.IsExpansion1Active())
		{
			List<AxialI> list = FindAvailablePOISpawnLocations(smi.impactorInstance.GetSMI<ClusterMapLargeImpactor.Instance>().ClusterGridPosition());
			if (list.Count > 0)
			{
				SpawnPOI("HarvestableSpacePOI_DLC4ImpactorDebrisField1", list[0]);
			}
			if (list.Count > 1)
			{
				SpawnPOI("HarvestableSpacePOI_DLC4ImpactorDebrisField2", list[1]);
			}
			if (list.Count > 2)
			{
				SpawnPOI("HarvestableSpacePOI_DLC4ImpactorDebrisField3", list[2]);
			}
		}
		else
		{
			if (!SpacecraftManager.instance.AddDestination(Db.Get().SpaceDestinationTypes.DLC4PrehistoricDemoliorSpaceDestination.Id, SpacecraftManager.DestinationLocationSelectionType.Nearest))
			{
				SpacecraftManager.instance.AddDestination(Db.Get().SpaceDestinationTypes.DLC4PrehistoricDemoliorSpaceDestination.Id, SpacecraftManager.DestinationLocationSelectionType.Random, 0, int.MaxValue, 5);
			}
			if (!SpacecraftManager.instance.AddDestination(Db.Get().SpaceDestinationTypes.DLC4PrehistoricDemoliorSpaceDestination2.Id, SpacecraftManager.DestinationLocationSelectionType.Random, 1, 5, 5))
			{
				SpacecraftManager.instance.AddDestination(Db.Get().SpaceDestinationTypes.DLC4PrehistoricDemoliorSpaceDestination2.Id, SpacecraftManager.DestinationLocationSelectionType.Random, 0, int.MaxValue, 5);
			}
			if (!SpacecraftManager.instance.AddDestination(Db.Get().SpaceDestinationTypes.DLC4PrehistoricDemoliorSpaceDestination3.Id, SpacecraftManager.DestinationLocationSelectionType.Random, 1, 5, 5))
			{
				SpacecraftManager.instance.AddDestination(Db.Get().SpaceDestinationTypes.DLC4PrehistoricDemoliorSpaceDestination3.Id, SpacecraftManager.DestinationLocationSelectionType.Random, 0, int.MaxValue, 5);
			}
		}
		smi.GoTo(smi.sm.finished);
	}

	private static bool WasKilled(StatesInstance smi, object _)
	{
		return smi.impactorInstance.GetSMI<LargeImpactorStatus.Instance>().Health <= 0;
	}

	private static void PrepareForLargeImpactorDefeatedSequence(StatesInstance smi)
	{
		smi.impactorInstance.GetComponent<LargeImpactorCrashStamp>();
		ToggleOffLandingZoneVisualizer(smi);
		ClusterManager.Instance.GetWorld(smi.eventInstance.worldId).RevealSurface();
	}

	private static void InitializeLandingSequence(StatesInstance smi)
	{
		GameObject impactorInstance = smi.impactorInstance;
		LargeImpactorCrashStamp component = impactorInstance.GetComponent<LargeImpactorCrashStamp>();
		ParallaxBackgroundObject component2 = impactorInstance.GetComponent<ParallaxBackgroundObject>();
		ToggleOffLandingZoneVisualizer(smi);
		WorldContainer world = ClusterManager.Instance.GetWorld(smi.eventInstance.worldId);
		world.RevealHiddenY();
		world.RevealSurface();
		component.RevealFogOfWar(7);
		component2.SetVisibilityState(visible: false);
		LargeComet comet = CreateLargeImpactorInWorldFallingAsteroid(smi, component, world);
		LargeImpactorLandingSequence.Start(component, comet, component, world.id);
	}

	private static void ToggleOffLandingZoneVisualizer(StatesInstance smi)
	{
		LargeImpactorVisualizer component = smi.impactorInstance.GetComponent<LargeImpactorVisualizer>();
		if (component.Active)
		{
			component.Active = false;
		}
	}

	private static LargeComet CreateLargeImpactorInWorldFallingAsteroid(StatesInstance smi, LargeImpactorCrashStamp crashStamp, WorldContainer world)
	{
		TemplateContainer asteroidTemplate = crashStamp.asteroidTemplate;
		Vector2I stampLocation = crashStamp.stampLocation;
		float layerZ = Grid.GetLayerZ(Grid.SceneLayer.FXFront);
		GameObject obj = Util.KInstantiate(position: new Vector3(stampLocation.X, world.Height - world.HiddenYOffset - 1, layerZ), original: Assets.GetPrefab(LargeImpactorCometConfig.ID), rotation: Quaternion.identity);
		LargeComet component = obj.GetComponent<LargeComet>();
		obj.SetActive(value: true);
		component.stampLocation = stampLocation;
		component.crashPosition = stampLocation;
		component.crashPosition.y += asteroidTemplate.GetTemplateBounds().yMin;
		component.asteroidTemplate = asteroidTemplate;
		component.bottomCellsOffsetOfTemplate = crashStamp.TemplateBottomCellsOffsets;
		return component;
	}

	private static GameObject CreateSpacedOutImpactorInstance(StatesInstance smi)
	{
		if (!DlcManager.IsExpansion1Active() || ClusterGrid.Instance == null)
		{
			return null;
		}
		GameObject gameObject = Util.KInstantiate(Assets.GetPrefab("LargeImpactor"));
		float arrivalTime = smi.eventInstance.eventStartTime * 600f + GetImpactTime();
		AxialI location = ClusterManager.Instance.GetClusterPOIManager().GetTemporalTear().Location;
		ClusterMapMeteorShowerVisualizer component = gameObject.GetComponent<ClusterMapMeteorShowerVisualizer>();
		component.SetInitialLocation(location);
		component.forceRevealed = true;
		ClusterMapLargeImpactor.Def def = gameObject.AddOrGetDef<ClusterMapLargeImpactor.Def>();
		def.destinationWorldID = 0;
		def.arrivalTime = arrivalTime;
		gameObject.AddOrGet<ParallaxBackgroundObject>().worldId = smi.eventInstance.worldId;
		return gameObject;
	}

	private static GameObject CreateVanillaImpactorInstance(StatesInstance smi)
	{
		if (DlcManager.IsExpansion1Active())
		{
			return null;
		}
		return Util.KInstantiate(Assets.GetPrefab(LargeImpactorVanillaConfig.ID));
	}

	public static void CreateImpactorInstance(StatesInstance smi)
	{
		GameObject gameObject = ((!DlcManager.IsExpansion1Active()) ? CreateVanillaImpactorInstance(smi) : CreateSpacedOutImpactorInstance(smi));
		if (gameObject == null)
		{
			KCrashReporter.ReportDevNotification("Failed to create LargeImpactor Object.", Environment.StackTrace);
			smi.StopSM("No Impactor created");
		}
		else
		{
			gameObject.SetActive(value: true);
			smi.sm.impactorTarget.Set(gameObject.GetComponent<KPrefabID>(), smi);
		}
	}

	public static float GetImpactTime()
	{
		ClusterLayout currentClusterLayout = CustomGameSettings.Instance.GetCurrentClusterLayout();
		if (currentClusterLayout != null && currentClusterLayout.clusterTags.Contains("DemoliorImminentImpact"))
		{
			return 6000f;
		}
		float num = (num = 200f);
		SettingLevel currentQualitySetting = CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.DemoliorDifficulty);
		if (currentQualitySetting.id == "VeryHard")
		{
			num = 100f;
		}
		else if (currentQualitySetting.id == "Hard")
		{
			num = 150f;
		}
		else if (currentQualitySetting.id == "Easy")
		{
			num = 300f;
		}
		else if (currentQualitySetting.id == "VeryEasy")
		{
			num = 500f;
		}
		return num * 600f;
	}
}
