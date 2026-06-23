using System.Collections.Generic;
using System.Linq;
using ProcGen;

namespace UtilLibs;

public class CGMWorldGenUtils
{
	public static readonly string CGM_Impactor_StoryTrait = "CGM_ImpactorStoryTrait";

	public static readonly string CGM_Heatpump_StoryTrait = "CGM_GeothermalHeatPump";

	private static Dictionary<string, bool> CachedPumpInfo = new Dictionary<string, bool>();

	private static Dictionary<string, bool> CachedImpactorInfo = new Dictionary<string, bool>();

	public static bool ShouldStoryBeInteractable(string storyId, List<WorldPlacement> worlds)
	{
		if (storyId == CGM_Heatpump_StoryTrait)
		{
			return !HasGeothermalPumpInCluster(worlds);
		}
		if (storyId == CGM_Impactor_StoryTrait)
		{
			return !HasImpactorShowerInCluster(worlds);
		}
		return true;
	}

	public static bool IsImpactorTrait(string storyId)
	{
		return storyId == CGM_Impactor_StoryTrait;
	}

	public static bool HasImpactorShower(World world)
	{
		return world != null && world.seasons != null && HasImpactorShower(world.seasons);
	}

	public static bool HasImpactorShower(List<string> seasons)
	{
		return seasons.Contains("LargeImpactor");
	}

	public static bool HasImpactorShowerInCluster(List<WorldPlacement> worldPlacements)
	{
		foreach (WorldPlacement worldPlacement in worldPlacements)
		{
			string world = worldPlacement.world;
			World worldData = SettingsCache.worlds.GetWorldData(world);
			if (worldData == null)
			{
				SgtLogger.warning("world " + world + " not found in world layouts");
			}
			else if (HasImpactorShower(worldData))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasImpactorShowerInCluster(string clusterID)
	{
		if (CachedImpactorInfo.TryGetValue(clusterID, out var value))
		{
			return value;
		}
		ClusterLayout clusterData = SettingsCache.clusterLayouts.GetClusterData(clusterID);
		if (clusterData == null)
		{
			SgtLogger.warning("cluster " + clusterID + " not found in cluster layouts");
			return false;
		}
		value = HasImpactorShowerInCluster(clusterData.worldPlacements);
		SgtLogger.l("cluster " + clusterID + " has largeimpactor shower: " + value);
		CachedImpactorInfo[clusterID] = value;
		return value;
	}

	public static bool HasGeothermalPumpInCluster(List<WorldPlacement> worldPlacements)
	{
		foreach (WorldPlacement worldPlacement in worldPlacements)
		{
			string world = worldPlacement.world;
			World worldData = SettingsCache.worlds.GetWorldData(world);
			if (worldData == null)
			{
				SgtLogger.warning("world " + world + " not found in world layouts");
			}
			else if (HasGeothermalPump(worldData))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasGeothermalPump(World world)
	{
		if (world == null)
		{
			return false;
		}
		foreach (TemplateSpawnRules worldTemplateRule in world.worldTemplateRules)
		{
			if (worldTemplateRule.names == null || !worldTemplateRule.names.Any() || (!worldTemplateRule.names.Contains("dlc2::poi/geothermal/geothermal_controller") && !worldTemplateRule.names.Contains("dlc2::poi/geothermal/shattered_geothermal_controller")))
			{
				continue;
			}
			return true;
		}
		return false;
	}

	public static bool HasGeothermalPumpInCluster(string clusterID)
	{
		if (CachedPumpInfo.TryGetValue(clusterID, out var value))
		{
			return value;
		}
		ClusterLayout clusterData = SettingsCache.clusterLayouts.GetClusterData(clusterID);
		if (clusterData == null)
		{
			SgtLogger.warning("cluster " + clusterID + " not found in cluster layouts");
			return false;
		}
		bool flag = HasGeothermalPumpInCluster(clusterData.worldPlacements);
		SgtLogger.l("cluster " + clusterID + " has geothermal pump: " + flag);
		CachedPumpInfo[clusterID] = flag;
		return flag;
	}
}
