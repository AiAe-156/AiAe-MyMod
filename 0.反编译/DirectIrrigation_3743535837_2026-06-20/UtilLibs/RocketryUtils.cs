using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PeterHan.PLib.Core;
using TUNING;
using UnityEngine;

namespace UtilLibs;

public static class RocketryUtils
{
	public enum RocketCategory
	{
		engines = 0,
		habitats = 1,
		nosecones = 2,
		deployables = 3,
		fuel = 4,
		cargo = 5,
		power = 6,
		production = 7,
		utility = 8,
		uncategorized = -1
	}

	public class RocketModuleList
	{
		public bool VanillaModulesCategorized;

		public Dictionary<int, List<string>> CategorizedButtonSortOrder;

		public static Dictionary<int, List<string>> GetRocketModuleList()
		{
			if (!PRegistry.GetData<bool>("Sgt_Imalas_VanillaRocketModulesCategorized"))
			{
				Debug.Log((object)"Rocketry Utils: Initializing global keys");
				PRegistry.PutData("Sgt_Imalas_VanillaRocketModulesCategorized", true);
				Dictionary<int, List<string>> categorizedButtonSortOrder = new RocketModuleList().CategorizedButtonSortOrder;
				SetRocketModuleList(categorizedButtonSortOrder);
				return categorizedButtonSortOrder;
			}
			return PRegistry.GetData<Dictionary<int, List<string>>>("Sgt_Imalas_RocketModuleSortOrder");
		}

		public static void SetRocketModuleList(Dictionary<int, List<string>> list)
		{
			PRegistry.PutData("Sgt_Imalas_RocketModuleSortOrder", list);
		}

		public RocketModuleList()
		{
			CategorizedButtonSortOrder = new Dictionary<int, List<string>>
			{
				{
					0,
					new List<string>()
				},
				{
					1,
					new List<string>()
				},
				{
					2,
					new List<string>()
				},
				{
					3,
					new List<string>()
				},
				{
					4,
					new List<string>()
				},
				{
					5,
					new List<string>()
				},
				{
					6,
					new List<string>()
				},
				{
					7,
					new List<string>()
				},
				{
					8,
					new List<string>()
				},
				{
					-1,
					new List<string>()
				}
			};
		}
	}

	public const string CategoryDataKey = "Sgt_Imalas_RocketModuleSortOrder";

	public const string CategoryInitKey = "Sgt_Imalas_VanillaRocketModulesCategorized";

	public const string AddonModsDataKey = "Sgt_Imalas_ModulesToRearrange";

	public static void AddPowerPlugToModule(BuildingDef def)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		AddPowerPlugToModule(def, CellOffset.none);
	}

	public static void AddPowerPlugToModule(BuildingDef def, CellOffset offset)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		def.RequiresPowerOutput = true;
		def.PowerInputOffset = offset;
		def.PowerOutputOffset = offset;
		def.UseWhitePowerOutputConnectorColour = true;
	}

	public static bool IsRocketTraveling(Clustercraft craft)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		AxialI location = ((ClusterGridEntity)craft).Location;
		AxialI destination = ((ClusterDestinationSelector)craft.ModuleInterface.GetClusterDestinationSelector()).GetDestination();
		return location != destination;
	}

	public static void CategorizeRocketModule(string moduleId, Dictionary<int, List<string>> sortedModules)
	{
		foreach (List<string> value in sortedModules.Values)
		{
			if (value.Contains(moduleId))
			{
				Debug.Log((object)(moduleId + " already in category"));
				return;
			}
		}
		bool flag = false;
		if (moduleId.Contains("Engine"))
		{
			AddIfNotExists(sortedModules[0], moduleId);
			flag = true;
			Debug.Log((object)("Added " + moduleId + " to category engines"));
		}
		if (moduleId.Contains("HabitatModule") || moduleId.Contains("RoboPilotModule"))
		{
			AddIfNotExists(sortedModules[1], moduleId);
			flag = true;
			Debug.Log((object)("Added " + moduleId + " to category habitats"));
		}
		if (moduleId.Contains("Nosecone") || moduleId == "HabitatModuleSmall")
		{
			AddIfNotExists(sortedModules[2], moduleId);
			flag = true;
			Debug.Log((object)("Added " + moduleId + " to category nosecones"));
		}
		if (moduleId == "OrbitalCargoModule" || moduleId == "ScoutModule" || moduleId == "PioneerModule")
		{
			AddIfNotExists(sortedModules[3], moduleId);
			flag = true;
			Debug.Log((object)("Added " + moduleId + " to category deployables"));
		}
		if (moduleId.Contains("Tank"))
		{
			AddIfNotExists(sortedModules[4], moduleId);
			flag = true;
			Debug.Log((object)("Added " + moduleId + " to category fuel"));
		}
		if (moduleId.Contains("CargoBay") || moduleId == "ResearchClusterModule")
		{
			AddIfNotExists(sortedModules[5], moduleId);
			flag = true;
			Debug.Log((object)("Added " + moduleId + " to category cargo"));
		}
		if (moduleId.Contains("Battery") || moduleId.Contains("SolarPanel"))
		{
			AddIfNotExists(sortedModules[6], moduleId);
			flag = true;
			Debug.Log((object)("Added " + moduleId + " to category power"));
		}
		if (moduleId == "ScannerModule" || moduleId.Contains("Research"))
		{
			AddIfNotExists(sortedModules[8], moduleId);
			flag = true;
			Debug.Log((object)("Added " + moduleId + " to category util"));
		}
		if (!flag)
		{
			SgtLogger.logwarning("No Category found for " + moduleId);
			AddIfNotExists(sortedModules[-1], moduleId);
		}
	}

	public static bool AddIfNotExists<T>(List<T> list, T value, int index = -1)
	{
		if (!list.Contains(value))
		{
			if (index == -1)
			{
				list.Add(value);
			}
			else
			{
				list.Insert(index, value);
			}
			return true;
		}
		return false;
	}

	public static void CategorizeVanillaModules()
	{
		List<string> moduleButtonSortOrder = SelectModuleSideScreen.moduleButtonSortOrder;
		Dictionary<int, List<string>> rocketModuleList = RocketModuleList.GetRocketModuleList();
		foreach (string item in moduleButtonSortOrder)
		{
			CategorizeRocketModule(item, rocketModuleList);
		}
		Debug.Log((object)"Vanilla rocket parts categorized");
		Dictionary<int, List<Tuple<string, string>>> moduleToReshuffleData = GetModuleToReshuffleData();
		foreach (KeyValuePair<int, List<Tuple<string, string>>> item2 in moduleToReshuffleData)
		{
			foreach (Tuple<string, string> item3 in item2.Value)
			{
				Debug.Log((object)"Removing {0} from List {1}".F(item3.first, (RocketCategory)item2.Key));
				rocketModuleList[item2.Key].Remove(item3.first);
			}
			foreach (Tuple<string, string> item4 in item2.Value)
			{
				Debug.Log((object)"Readding {0} to List {1} behind {2}".F(item4.first, (RocketCategory)item2.Key, item4.second));
				rocketModuleList[item2.Key].Insert(GetInsertionIndex(rocketModuleList[item2.Key]), item4.first);
			}
		}
		Debug.Log((object)"Rocketry Expanded: Addon mod reordering done, putting data to PRegistry");
		RocketModuleList.SetRocketModuleList(rocketModuleList);
	}

	public static void AddRocketModuleToBuildList(string moduleId, RocketCategory category = RocketCategory.uncategorized, string placebehind = "", bool placebefore = false)
	{
		InsertRocketModuleToCategory(moduleId, category, placebehind, placebefore);
	}

	public static void AddRocketModuleToBuildList(string moduleId, RocketCategory[] categories, string placebehind = "", bool placebefore = false)
	{
		InsertRocketModuleToCategory(moduleId, categories, placebehind, placebefore);
	}

	public static void PutModuleToReshuffleData(Dictionary<int, List<Tuple<string, string>>> list)
	{
		PRegistry.PutData("Sgt_Imalas_ModulesToRearrange", list);
	}

	public static Dictionary<int, List<Tuple<string, string>>> GetModuleToReshuffleData()
	{
		Dictionary<int, List<Tuple<string, string>>> dictionary = PRegistry.GetData<Dictionary<int, List<Tuple<string, string>>>>("Sgt_Imalas_ModulesToRearrange");
		if (dictionary == null)
		{
			dictionary = new Dictionary<int, List<Tuple<string, string>>>
			{
				{
					0,
					new List<Tuple<string, string>>()
				},
				{
					1,
					new List<Tuple<string, string>>()
				},
				{
					2,
					new List<Tuple<string, string>>()
				},
				{
					3,
					new List<Tuple<string, string>>()
				},
				{
					4,
					new List<Tuple<string, string>>()
				},
				{
					5,
					new List<Tuple<string, string>>()
				},
				{
					6,
					new List<Tuple<string, string>>()
				},
				{
					7,
					new List<Tuple<string, string>>()
				},
				{
					8,
					new List<Tuple<string, string>>()
				},
				{
					-1,
					new List<Tuple<string, string>>()
				}
			};
			PutModuleToReshuffleData(dictionary);
		}
		return dictionary;
	}

	public static void AddModuleToReshuffleData(RocketCategory category, string moduleId, string placeBehind)
	{
		Debug.Log((object)(moduleId + " scheduled for relocation"));
		Dictionary<int, List<Tuple<string, string>>> moduleToReshuffleData = GetModuleToReshuffleData();
		moduleToReshuffleData[(int)category].Add(new Tuple<string, string>(moduleId, placeBehind));
		PutModuleToReshuffleData(moduleToReshuffleData);
	}

	public static void InsertRocketModuleToCategory(string moduleId, RocketCategory category = RocketCategory.uncategorized, string placeBehindId = "", bool placebefore = false)
	{
		InsertRocketModuleToCategory(moduleId, new RocketCategory[1] { category }, placeBehindId, placebefore);
	}

	public static void InsertRocketModuleToCategory(string moduleId, RocketCategory[] categories, string placeBehindId = "", bool placebefore = false)
	{
		Dictionary<int, List<string>> rocketModuleList = RocketModuleList.GetRocketModuleList();
		foreach (RocketCategory rocketCategory in categories)
		{
			if (placeBehindId != "")
			{
				int num = rocketModuleList[(int)rocketCategory].IndexOf(placeBehindId);
				if (num == -1)
				{
					AddIfNotExists(rocketModuleList[(int)rocketCategory], moduleId);
					AddModuleToReshuffleData(rocketCategory, moduleId, placeBehindId);
				}
				else
				{
					AddIfNotExists(rocketModuleList[(int)rocketCategory], moduleId, placebefore ? num : (++num));
				}
			}
			else
			{
				AddIfNotExists(rocketModuleList[(int)rocketCategory], moduleId);
			}
		}
		if (!SelectModuleSideScreen.moduleButtonSortOrder.Contains(moduleId))
		{
			int insertionIndex = GetInsertionIndex(SelectModuleSideScreen.moduleButtonSortOrder, placeBehindId, placebefore);
			SelectModuleSideScreen.moduleButtonSortOrder.Insert(insertionIndex, moduleId);
		}
	}

	public static int GetInsertionIndex(List<string> list, string indexID = "", bool placebefore = false)
	{
		int num = ((indexID != "") ? list.IndexOf(indexID) : (-1));
		return (num == -1) ? list.Count : (placebefore ? num : (++num));
	}

	public static Vector2I GetCustomInteriorSize(string templateString)
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		Regex regex = new Regex("\\(([0-9]*?)[,]([0-9]*?)\\)");
		MatchCollection matchCollection = regex.Matches(templateString);
		if (matchCollection.Count == 1)
		{
			Debug.Log((object)(matchCollection[0]?.ToString() + " " + matchCollection[0].Groups.Count + " " + matchCollection[0].Groups[0].Value + " " + matchCollection[0].Groups[1].Value));
			if (matchCollection[0].Groups.Count == 3)
			{
				Debug.Log((object)"reachedGroups");
				int num = int.Parse(matchCollection[0].Groups[1].Value);
				int num2 = int.Parse(matchCollection[0].Groups[2].Value);
				return new Vector2I(num, num2);
			}
		}
		return ROCKETRY.ROCKET_INTERIOR_SIZE;
	}

	public static void RemoveModuleCondition(this RocketModule module, ProcessConditionType type, ProcessCondition condition)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)module == (Object)null) && condition != null && module.moduleConditions.TryGetValue(type, out var value) && value.Contains(condition))
		{
			value.Remove(condition);
		}
	}

	public static void RemoveModuleCondition(this RocketModule module, ProcessConditionType type, Func<ProcessCondition, bool> shouldRemove)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)module == (Object)null || shouldRemove == null || !module.moduleConditions.TryGetValue(type, out var value))
		{
			return;
		}
		HashSet<ProcessCondition> hashSet = new HashSet<ProcessCondition>();
		foreach (ProcessCondition item in value)
		{
			if (shouldRemove(item))
			{
				hashSet.Add(item);
			}
		}
		value.RemoveAll(hashSet.Contains);
	}

	public static void SelectStarmapEntity(ClusterGridEntity target)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ClusterMapScreen.Instance != (Object)null && !((Component)ClusterMapScreen.Instance).gameObject.activeSelf && (Object)(object)ManagementMenu.Instance != (Object)null)
		{
			ManagementMenu.Instance.ToggleClusterMap();
		}
		ClusterMapScreen.Instance.SetTargetFocusPosition(target.Location, 0f);
	}
}
