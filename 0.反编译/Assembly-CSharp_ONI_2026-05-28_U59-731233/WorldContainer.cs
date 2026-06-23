using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Delaunay.Geo;
using KSerialization;
using Klei;
using ProcGen;
using ProcGenGame;
using TUNING;
using TemplateClasses;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class WorldContainer : KMonoBehaviour
{
	[Serialize]
	public int id = -1;

	[Serialize]
	public Tag prefabTag;

	[Serialize]
	private Vector2I worldOffset;

	[Serialize]
	private Vector2I worldSize;

	[Serialize]
	private bool fullyEnclosedBorder;

	[Serialize]
	private int hiddenYOffset;

	[Serialize]
	private bool isModuleInterior;

	[Serialize]
	private WorldDetailSave.OverworldCell overworldCell;

	[Serialize]
	private bool isDiscovered;

	[Serialize]
	private bool isStartWorld;

	[Serialize]
	private bool isDupeVisited;

	[Serialize]
	private float dupeVisitedTimestamp = -1f;

	[Serialize]
	private float discoveryTimestamp = -1f;

	[Serialize]
	private bool isRoverVisited;

	[Serialize]
	private bool isSurfaceRevealed;

	[Serialize]
	public string worldName;

	[Serialize]
	public string[] nameTables;

	[Serialize]
	public Tag[] worldTags;

	[Serialize]
	public string overrideName;

	[Serialize]
	public string worldType;

	[Serialize]
	public string worldDescription;

	[Serialize]
	public int northernlights = FIXEDTRAITS.NORTHERNLIGHTS.DEFAULT_VALUE;

	[Serialize]
	public int largeImpactorFragments = FIXEDTRAITS.LARGEIMPACTORFRAGMENTS.DEFAULT_VALUE;

	[Serialize]
	public int sunlight = FIXEDTRAITS.SUNLIGHT.DEFAULT_VALUE;

	[Serialize]
	public int cosmicRadiation = FIXEDTRAITS.COSMICRADIATION.DEFAULT_VALUE;

	[Serialize]
	public float currentSunlightIntensity = 0f;

	[Serialize]
	public float currentCosmicIntensity = FIXEDTRAITS.COSMICRADIATION.DEFAULT_VALUE;

	[Serialize]
	public string sunlightFixedTrait = null;

	[Serialize]
	public string cosmicRadiationFixedTrait = null;

	[Serialize]
	public string northernLightFixedTrait = null;

	[Serialize]
	public string largeImpactorFragmentsFixedTrait = null;

	[Serialize]
	public int fixedTraitsUpdateVersion = 1;

	private Dictionary<string, int> sunlightFixedTraits = new Dictionary<string, int>
	{
		{
			FIXEDTRAITS.SUNLIGHT.NAME.NONE,
			FIXEDTRAITS.SUNLIGHT.NONE
		},
		{
			FIXEDTRAITS.SUNLIGHT.NAME.VERY_VERY_LOW,
			FIXEDTRAITS.SUNLIGHT.VERY_VERY_LOW
		},
		{
			FIXEDTRAITS.SUNLIGHT.NAME.VERY_LOW,
			FIXEDTRAITS.SUNLIGHT.VERY_LOW
		},
		{
			FIXEDTRAITS.SUNLIGHT.NAME.LOW,
			FIXEDTRAITS.SUNLIGHT.LOW
		},
		{
			FIXEDTRAITS.SUNLIGHT.NAME.MED_LOW,
			FIXEDTRAITS.SUNLIGHT.MED_LOW
		},
		{
			FIXEDTRAITS.SUNLIGHT.NAME.MED,
			FIXEDTRAITS.SUNLIGHT.MED
		},
		{
			FIXEDTRAITS.SUNLIGHT.NAME.MED_HIGH,
			FIXEDTRAITS.SUNLIGHT.MED_HIGH
		},
		{
			FIXEDTRAITS.SUNLIGHT.NAME.HIGH,
			FIXEDTRAITS.SUNLIGHT.HIGH
		},
		{
			FIXEDTRAITS.SUNLIGHT.NAME.VERY_HIGH,
			FIXEDTRAITS.SUNLIGHT.VERY_HIGH
		},
		{
			FIXEDTRAITS.SUNLIGHT.NAME.VERY_VERY_HIGH,
			FIXEDTRAITS.SUNLIGHT.VERY_VERY_HIGH
		},
		{
			FIXEDTRAITS.SUNLIGHT.NAME.VERY_VERY_VERY_HIGH,
			FIXEDTRAITS.SUNLIGHT.VERY_VERY_VERY_HIGH
		}
	};

	private Dictionary<string, int> northernLightsFixedTraits = new Dictionary<string, int>
	{
		{
			FIXEDTRAITS.NORTHERNLIGHTS.NAME.NONE,
			FIXEDTRAITS.NORTHERNLIGHTS.NONE
		},
		{
			FIXEDTRAITS.NORTHERNLIGHTS.NAME.ENABLED,
			FIXEDTRAITS.NORTHERNLIGHTS.ENABLED
		}
	};

	private Dictionary<string, int> largeImpactorFragmentsFixedTraits = new Dictionary<string, int>
	{
		{
			FIXEDTRAITS.LARGEIMPACTORFRAGMENTS.NAME.NONE,
			FIXEDTRAITS.LARGEIMPACTORFRAGMENTS.NONE
		},
		{
			FIXEDTRAITS.LARGEIMPACTORFRAGMENTS.NAME.ALLOWED,
			FIXEDTRAITS.LARGEIMPACTORFRAGMENTS.ALLOWED
		}
	};

	private Dictionary<string, int> cosmicRadiationFixedTraits = new Dictionary<string, int>
	{
		{
			FIXEDTRAITS.COSMICRADIATION.NAME.NONE,
			FIXEDTRAITS.COSMICRADIATION.NONE
		},
		{
			FIXEDTRAITS.COSMICRADIATION.NAME.VERY_VERY_LOW,
			FIXEDTRAITS.COSMICRADIATION.VERY_VERY_LOW
		},
		{
			FIXEDTRAITS.COSMICRADIATION.NAME.VERY_LOW,
			FIXEDTRAITS.COSMICRADIATION.VERY_LOW
		},
		{
			FIXEDTRAITS.COSMICRADIATION.NAME.LOW,
			FIXEDTRAITS.COSMICRADIATION.LOW
		},
		{
			FIXEDTRAITS.COSMICRADIATION.NAME.MED_LOW,
			FIXEDTRAITS.COSMICRADIATION.MED_LOW
		},
		{
			FIXEDTRAITS.COSMICRADIATION.NAME.MED,
			FIXEDTRAITS.COSMICRADIATION.MED
		},
		{
			FIXEDTRAITS.COSMICRADIATION.NAME.MED_HIGH,
			FIXEDTRAITS.COSMICRADIATION.MED_HIGH
		},
		{
			FIXEDTRAITS.COSMICRADIATION.NAME.HIGH,
			FIXEDTRAITS.COSMICRADIATION.HIGH
		},
		{
			FIXEDTRAITS.COSMICRADIATION.NAME.VERY_HIGH,
			FIXEDTRAITS.COSMICRADIATION.VERY_HIGH
		},
		{
			FIXEDTRAITS.COSMICRADIATION.NAME.VERY_VERY_HIGH,
			FIXEDTRAITS.COSMICRADIATION.VERY_VERY_HIGH
		}
	};

	[Serialize]
	private List<string> m_seasonIds;

	[Serialize]
	private List<string> m_subworldNames;

	[Serialize]
	private List<string> m_worldTraitIds;

	[Serialize]
	private List<string> m_storyTraitIds;

	[Serialize]
	private List<string> m_generatedSubworlds;

	[Serialize]
	private List<BiomeSizeData> m_biomesData;

	[Serialize]
	private Vector4[] m_biomesSize = new Vector4[1] { Vector4.zero };

	private WorldParentChangedEventArgs parentChangeArgs = new WorldParentChangedEventArgs();

	[MySmiReq]
	private AlertStateManager.Instance m_alertManager;

	private List<Prioritizable> yellowAlertTasks = new List<Prioritizable>();

	private List<int> m_childWorlds = new List<int>();

	[Serialize]
	public WorldInventory worldInventory { get; private set; }

	public Dictionary<Tag, float> materialNeeds { get; private set; }

	public bool IsModuleInterior => isModuleInterior;

	public bool IsDiscovered => isDiscovered || DebugHandler.RevealFogOfWar;

	public bool IsStartWorld => isStartWorld;

	public bool IsDupeVisited => isDupeVisited;

	public float DupeVisitedTimestamp => dupeVisitedTimestamp;

	public float DiscoveryTimestamp => discoveryTimestamp;

	public bool IsRoverVisted => isRoverVisited;

	public bool IsSurfaceRevealed => isSurfaceRevealed;

	public Dictionary<string, int> SunlightFixedTraits => sunlightFixedTraits;

	public Dictionary<string, int> NorthernLightsFixedTraits => northernLightsFixedTraits;

	public Dictionary<string, int> LargeImpactorFragmentsFixedTraits => largeImpactorFragmentsFixedTraits;

	public Dictionary<string, int> CosmicRadiationFixedTraits => cosmicRadiationFixedTraits;

	public Vector4[] BiomesOnlySizeData => m_biomesSize;

	public List<BiomeSizeData> BiomesData => m_biomesData;

	public List<string> Biomes => m_subworldNames;

	public List<string> GeneratedBiomes => m_generatedSubworlds;

	public List<string> WorldTraitIds => m_worldTraitIds;

	public List<string> StoryTraitIds => m_storyTraitIds;

	public AlertStateManager.Instance AlertManager
	{
		get
		{
			if (m_alertManager == null)
			{
				StateMachineController component = GetComponent<StateMachineController>();
				m_alertManager = component.GetSMI<AlertStateManager.Instance>();
			}
			Debug.Assert(m_alertManager != null, "AlertStateManager should never be null.");
			return m_alertManager;
		}
	}

	public int ParentWorldId { get; private set; }

	public Vector2 minimumBounds => new Vector2(worldOffset.x, worldOffset.y);

	public Vector2 maximumBounds => new Vector2(worldOffset.x + (worldSize.x - 1), worldOffset.y + (worldSize.y - hiddenYOffset - 1));

	public Vector2I WorldSize => worldSize;

	public Vector2I WorldOffset => worldOffset;

	public int HiddenYOffset => hiddenYOffset;

	public bool FullyEnclosedBorder => fullyEnclosedBorder;

	public int Height => worldSize.y;

	public int Width => worldSize.x;

	public void AddTopPriorityPrioritizable(Prioritizable prioritizable)
	{
		if (!yellowAlertTasks.Contains(prioritizable))
		{
			yellowAlertTasks.Add(prioritizable);
		}
		RefreshHasTopPriorityChore();
	}

	public void RemoveTopPriorityPrioritizable(Prioritizable prioritizable)
	{
		for (int num = yellowAlertTasks.Count - 1; num >= 0; num--)
		{
			if (yellowAlertTasks[num] == prioritizable || yellowAlertTasks[num].Equals(null))
			{
				yellowAlertTasks.RemoveAt(num);
			}
		}
		RefreshHasTopPriorityChore();
	}

	public ICollection<int> GetChildWorldIds()
	{
		return m_childWorlds;
	}

	private void OnWorldRemoved(object data)
	{
		int value = ((Boxed<int>)data).value;
		if (value != 255)
		{
			m_childWorlds.Remove(value);
		}
	}

	private void OnWorldParentChanged(object data)
	{
		if (data is WorldParentChangedEventArgs e)
		{
			if (e.world.ParentWorldId == id)
			{
				m_childWorlds.Add(e.world.id);
			}
			if (e.lastParentId == ParentWorldId)
			{
				m_childWorlds.Remove(e.world.id);
			}
		}
	}

	public Quadrant[] GetQuadrantOfCell(int cell, int depth = 1)
	{
		Vector2 vector = new Vector2((float)WorldSize.x * Grid.CellSizeInMeters, (float)worldSize.y * Grid.CellSizeInMeters);
		Vector2 vector2 = Grid.CellToPos2D(Grid.XYToCell(WorldOffset.x, WorldOffset.y));
		Vector2 vector3 = Grid.CellToPos2D(cell);
		Quadrant[] array = new Quadrant[depth];
		Vector2 vector4 = new Vector2(vector2.x, (float)worldOffset.y + vector.y);
		Vector2 vector5 = new Vector2(vector2.x + vector.x, worldOffset.y);
		for (int i = 0; i < depth; i++)
		{
			float num = vector5.x - vector4.x;
			float num2 = vector4.y - vector5.y;
			float num3 = num * 0.5f;
			float num4 = num2 * 0.5f;
			if (vector3.x >= vector4.x + num3 && vector3.y >= vector5.y + num4)
			{
				array[i] = Quadrant.NE;
			}
			if (vector3.x >= vector4.x + num3 && vector3.y < vector5.y + num4)
			{
				array[i] = Quadrant.SE;
			}
			if (vector3.x < vector4.x + num3 && vector3.y < vector5.y + num4)
			{
				array[i] = Quadrant.SW;
			}
			if (vector3.x < vector4.x + num3 && vector3.y >= vector5.y + num4)
			{
				array[i] = Quadrant.NW;
			}
			switch (array[i])
			{
			case Quadrant.NE:
				vector4.x += num3;
				vector5.y += num4;
				break;
			case Quadrant.SE:
				vector4.x += num3;
				vector4.y -= num4;
				break;
			case Quadrant.SW:
				vector4.y -= num4;
				vector5.x -= num3;
				break;
			case Quadrant.NW:
				vector5.x -= num3;
				vector5.y += num4;
				break;
			}
		}
		return array;
	}

	[OnDeserialized]
	private void OnDeserialized()
	{
		ParentWorldId = id;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		worldInventory = GetComponent<WorldInventory>();
		materialNeeds = new Dictionary<Tag, float>();
		ClusterManager.Instance.RegisterWorldContainer(this);
		Game.Instance.Subscribe(880851192, OnWorldParentChanged);
		ClusterManager.Instance.Subscribe(-1078710002, OnWorldRemoved);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		InfoDescription infoDescription = base.gameObject.AddOrGet<InfoDescription>();
		infoDescription.DescriptionLocString = worldDescription;
		RefreshHasTopPriorityChore();
		UpgradeFixedTraits();
		RefreshFixedTraits();
		if (DlcManager.IsPureVanilla())
		{
			isStartWorld = true;
			isDupeVisited = true;
		}
	}

	protected override void OnCleanUp()
	{
		SaveGame.Instance.materialSelectorSerializer.WipeWorldSelectionData(id);
		Game.Instance.Unsubscribe(880851192, OnWorldParentChanged);
		ClusterManager.Instance.Unsubscribe(-1078710002, OnWorldRemoved);
		base.OnCleanUp();
	}

	private void UpgradeFixedTraits()
	{
		if (sunlightFixedTrait == null || sunlightFixedTrait == "")
		{
			Dictionary<int, string> dictionary = new Dictionary<int, string>
			{
				{
					160000,
					FIXEDTRAITS.SUNLIGHT.NAME.VERY_VERY_HIGH
				},
				{
					0,
					FIXEDTRAITS.SUNLIGHT.NAME.NONE
				},
				{
					10000,
					FIXEDTRAITS.SUNLIGHT.NAME.VERY_VERY_LOW
				},
				{
					20000,
					FIXEDTRAITS.SUNLIGHT.NAME.VERY_LOW
				},
				{
					30000,
					FIXEDTRAITS.SUNLIGHT.NAME.LOW
				},
				{
					35000,
					FIXEDTRAITS.SUNLIGHT.NAME.MED_LOW
				},
				{
					40000,
					FIXEDTRAITS.SUNLIGHT.NAME.MED
				},
				{
					50000,
					FIXEDTRAITS.SUNLIGHT.NAME.MED_HIGH
				},
				{
					60000,
					FIXEDTRAITS.SUNLIGHT.NAME.HIGH
				},
				{
					80000,
					FIXEDTRAITS.SUNLIGHT.NAME.VERY_HIGH
				},
				{
					120000,
					FIXEDTRAITS.SUNLIGHT.NAME.VERY_VERY_HIGH
				}
			};
			dictionary.TryGetValue(sunlight, out sunlightFixedTrait);
		}
		if (cosmicRadiationFixedTrait == null || cosmicRadiationFixedTrait == "")
		{
			Dictionary<int, string> dictionary2 = new Dictionary<int, string>
			{
				{
					0,
					FIXEDTRAITS.COSMICRADIATION.NAME.NONE
				},
				{
					6,
					FIXEDTRAITS.COSMICRADIATION.NAME.VERY_VERY_LOW
				},
				{
					12,
					FIXEDTRAITS.COSMICRADIATION.NAME.VERY_LOW
				},
				{
					18,
					FIXEDTRAITS.COSMICRADIATION.NAME.LOW
				},
				{
					21,
					FIXEDTRAITS.COSMICRADIATION.NAME.MED_LOW
				},
				{
					25,
					FIXEDTRAITS.COSMICRADIATION.NAME.MED
				},
				{
					31,
					FIXEDTRAITS.COSMICRADIATION.NAME.MED_HIGH
				},
				{
					37,
					FIXEDTRAITS.COSMICRADIATION.NAME.HIGH
				},
				{
					50,
					FIXEDTRAITS.COSMICRADIATION.NAME.VERY_HIGH
				},
				{
					75,
					FIXEDTRAITS.COSMICRADIATION.NAME.VERY_VERY_HIGH
				}
			};
			dictionary2.TryGetValue(cosmicRadiation, out cosmicRadiationFixedTrait);
		}
	}

	private void RefreshFixedTraits()
	{
		sunlight = GetSunlightValueFromFixedTrait();
		cosmicRadiation = GetCosmicRadiationValueFromFixedTrait();
		northernlights = GetNorthernlightValueFromFixedTrait();
		largeImpactorFragments = GetLargeImpactorFragmentsValueFromFixedTrait();
	}

	private void RefreshHasTopPriorityChore()
	{
		if (AlertManager != null)
		{
			AlertManager.SetHasTopPriorityChore(yellowAlertTasks.Count > 0);
		}
	}

	public List<string> GetSeasonIds()
	{
		return m_seasonIds;
	}

	public bool IsRedAlert()
	{
		return m_alertManager.IsRedAlert();
	}

	public bool IsYellowAlert()
	{
		return m_alertManager.IsYellowAlert();
	}

	public string GetRandomName()
	{
		if (!overrideName.IsNullOrWhiteSpace())
		{
			return Strings.Get(overrideName);
		}
		return GameUtil.GenerateRandomWorldName(nameTables);
	}

	public void SetID(int id)
	{
		this.id = id;
		ParentWorldId = id;
	}

	public void SetParentIdx(int parentIdx)
	{
		parentChangeArgs.lastParentId = ParentWorldId;
		parentChangeArgs.world = this;
		ParentWorldId = parentIdx;
		Game.Instance.Trigger(880851192, (object)parentChangeArgs);
		parentChangeArgs.lastParentId = 255;
	}

	public void SetDiscovered(bool reveal_surface = false)
	{
		if (!isDiscovered)
		{
			discoveryTimestamp = GameUtil.GetCurrentTimeInCycles();
		}
		isDiscovered = true;
		if (reveal_surface)
		{
			LookAtSurface();
		}
		Game.Instance.Trigger(-521212405, (object)this);
	}

	public void SetDupeVisited()
	{
		if (!isDupeVisited)
		{
			dupeVisitedTimestamp = GameUtil.GetCurrentTimeInCycles();
			isDupeVisited = true;
			Game.Instance.Trigger(-434755240, (object)this);
		}
	}

	public void SetRoverLanded()
	{
		isRoverVisited = true;
	}

	public void SetRocketInteriorWorldDetails(int world_id, Vector2I size, Vector2I offset)
	{
		SetID(world_id);
		fullyEnclosedBorder = true;
		worldOffset = offset;
		worldSize = size;
		isDiscovered = true;
		isModuleInterior = true;
		m_seasonIds = new List<string>();
	}

	private static int IsClockwise(Vector2 first, Vector2 second, Vector2 origin)
	{
		if (first == second)
		{
			return 0;
		}
		Vector2 vector = first - origin;
		Vector2 vector2 = second - origin;
		float num = Mathf.Atan2(vector.x, vector.y);
		float num2 = Mathf.Atan2(vector2.x, vector2.y);
		if (num < num2)
		{
			return 1;
		}
		if (num > num2)
		{
			return -1;
		}
		return (vector.sqrMagnitude < vector2.sqrMagnitude) ? 1 : (-1);
	}

	public void PlaceInteriorTemplate(string template_name, System.Action callback)
	{
		TemplateContainer template = TemplateCache.GetTemplate(template_name);
		Vector2 pos = new Vector2(worldSize.x / 2 + worldOffset.x, worldSize.y / 2 + worldOffset.y);
		TemplateLoader.Stamp(template, pos, callback);
		float val = template.info.size.X / 2f;
		float val2 = template.info.size.Y / 2f;
		float num = Math.Max(val, val2);
		GridVisibility.Reveal((int)pos.x, (int)pos.y, (int)num + 3 + 5, num + 3f);
		WorldDetailSave clusterDetailSave = SaveLoader.Instance.clusterDetailSave;
		overworldCell = new WorldDetailSave.OverworldCell();
		List<Vector2> list = new List<Vector2>(template.cells.Count);
		foreach (Prefab building in template.buildings)
		{
			if (building.id == "RocketWallTile")
			{
				Vector2 item = new Vector2((float)building.location_x + pos.x, (float)building.location_y + pos.y);
				if (item.x > pos.x)
				{
					item.x += 0.5f;
				}
				if (item.y > pos.y)
				{
					item.y += 0.5f;
				}
				list.Add(item);
			}
		}
		list.Sort((Vector2 v1, Vector2 v2) => IsClockwise(v1, v2, pos));
		Polygon polygon = new Polygon(list);
		overworldCell.poly = polygon;
		overworldCell.zoneType = SubWorld.ZoneType.RocketInterior;
		overworldCell.tags = new TagSet { WorldGenTags.RocketInterior };
		clusterDetailSave.overworldCells.Add(overworldCell);
		for (int num2 = 0; num2 < worldSize.y; num2++)
		{
			for (int num3 = 0; num3 < worldSize.x; num3++)
			{
				Vector2I vector2I = new Vector2I(worldOffset.x + num3, worldOffset.y + num2);
				int num4 = Grid.XYToCell(vector2I.x, vector2I.y);
				if (polygon.Contains(new Vector2(vector2I.x, vector2I.y)))
				{
					SimMessages.ModifyCellWorldZone(num4, 14);
					World.Instance.zoneRenderData.worldZoneTypes[num4] = SubWorld.ZoneType.RocketInterior;
				}
				else
				{
					SimMessages.ModifyCellWorldZone(num4, byte.MaxValue);
					World.Instance.zoneRenderData.worldZoneTypes[num4] = SubWorld.ZoneType.Space;
				}
			}
		}
	}

	private int GetDefaultValueForFixedTraitCategory(Dictionary<string, int> traitCategory)
	{
		if (traitCategory == largeImpactorFragmentsFixedTraits)
		{
			return FIXEDTRAITS.LARGEIMPACTORFRAGMENTS.DEFAULT_VALUE;
		}
		if (traitCategory == northernLightsFixedTraits)
		{
			return FIXEDTRAITS.NORTHERNLIGHTS.DEFAULT_VALUE;
		}
		if (traitCategory == sunlightFixedTraits)
		{
			return FIXEDTRAITS.SUNLIGHT.DEFAULT_VALUE;
		}
		if (traitCategory == cosmicRadiationFixedTraits)
		{
			return FIXEDTRAITS.COSMICRADIATION.DEFAULT_VALUE;
		}
		return 0;
	}

	private string GetDefaultFixedTraitFor(Dictionary<string, int> traitCategory)
	{
		if (traitCategory == largeImpactorFragmentsFixedTraits)
		{
			return FIXEDTRAITS.LARGEIMPACTORFRAGMENTS.NAME.DEFAULT;
		}
		if (traitCategory == northernLightsFixedTraits)
		{
			return FIXEDTRAITS.NORTHERNLIGHTS.NAME.DEFAULT;
		}
		if (traitCategory == sunlightFixedTraits)
		{
			return FIXEDTRAITS.SUNLIGHT.NAME.DEFAULT;
		}
		if (traitCategory == cosmicRadiationFixedTraits)
		{
			return FIXEDTRAITS.COSMICRADIATION.NAME.DEFAULT;
		}
		return null;
	}

	private string GetFixedTraitsFor(Dictionary<string, int> traitCategory, WorldGen world)
	{
		foreach (string fixedTrait in world.Settings.world.fixedTraits)
		{
			if (traitCategory.ContainsKey(fixedTrait))
			{
				return fixedTrait;
			}
		}
		return GetDefaultFixedTraitFor(traitCategory);
	}

	private int GetFixedTraitValueForTrait(Dictionary<string, int> traitCategory, ref string trait)
	{
		if (trait == null)
		{
			trait = GetDefaultFixedTraitFor(traitCategory);
		}
		if (traitCategory.ContainsKey(trait))
		{
			return traitCategory[trait];
		}
		return GetDefaultValueForFixedTraitCategory(traitCategory);
	}

	private string GetLargeImpactorFragmentsFixedTraits(WorldGen world)
	{
		return GetFixedTraitsFor(LargeImpactorFragmentsFixedTraits, world);
	}

	private string GetNorthernlightFixedTraits(WorldGen world)
	{
		return GetFixedTraitsFor(northernLightsFixedTraits, world);
	}

	private string GetSunlightFromFixedTraits(WorldGen world)
	{
		return GetFixedTraitsFor(sunlightFixedTraits, world);
	}

	private string GetCosmicRadiationFromFixedTraits(WorldGen world)
	{
		return GetFixedTraitsFor(cosmicRadiationFixedTraits, world);
	}

	private int GetLargeImpactorFragmentsValueFromFixedTrait()
	{
		return GetFixedTraitValueForTrait(largeImpactorFragmentsFixedTraits, ref largeImpactorFragmentsFixedTrait);
	}

	private int GetNorthernlightValueFromFixedTrait()
	{
		return GetFixedTraitValueForTrait(northernLightsFixedTraits, ref northernLightFixedTrait);
	}

	private int GetSunlightValueFromFixedTrait()
	{
		return GetFixedTraitValueForTrait(sunlightFixedTraits, ref sunlightFixedTrait);
	}

	private int GetCosmicRadiationValueFromFixedTrait()
	{
		return GetFixedTraitValueForTrait(cosmicRadiationFixedTraits, ref cosmicRadiationFixedTrait);
	}

	public void SetWorldDetails(WorldGen world)
	{
		if (world != null)
		{
			fullyEnclosedBorder = world.Settings.GetBoolSetting("DrawWorldBorder") && world.Settings.GetBoolSetting("DrawWorldBorderOverVacuum");
			worldOffset = world.GetPosition();
			worldSize = world.GetSize();
			hiddenYOffset = world.HiddenYOffset;
			isDiscovered = world.isStartingWorld;
			isStartWorld = world.isStartingWorld;
			worldName = world.Settings.world.filePath;
			nameTables = world.Settings.world.nameTables;
			worldTags = ((world.Settings.world.worldTags != null) ? world.Settings.world.worldTags.ToArray().ToTagArray() : new Tag[0]);
			worldDescription = world.Settings.world.description;
			worldType = world.Settings.world.name;
			isModuleInterior = world.Settings.world.moduleInterior;
			m_seasonIds = new List<string>(world.Settings.world.seasons);
			m_generatedSubworlds = world.Settings.world.generatedSubworlds;
			largeImpactorFragmentsFixedTrait = GetLargeImpactorFragmentsFixedTraits(world);
			northernLightFixedTrait = GetNorthernlightFixedTraits(world);
			sunlightFixedTrait = GetSunlightFromFixedTraits(world);
			cosmicRadiationFixedTrait = GetCosmicRadiationFromFixedTraits(world);
			sunlight = GetSunlightValueFromFixedTrait();
			northernlights = GetNorthernlightValueFromFixedTrait();
			cosmicRadiation = GetCosmicRadiationValueFromFixedTrait();
			currentCosmicIntensity = cosmicRadiation;
			HashSet<string> hashSet = new HashSet<string>();
			foreach (string generatedSubworld in world.Settings.world.generatedSubworlds)
			{
				string text = generatedSubworld;
				text = text.Substring(0, text.LastIndexOf('/'));
				text = text.Substring(text.LastIndexOf('/') + 1, text.Length - (text.LastIndexOf('/') + 1));
				hashSet.Add(text);
			}
			m_subworldNames = hashSet.ToList();
			m_biomesData = world.data.biomes;
			Vector4[] array = new Vector4[world.data.biomes.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = world.data.biomes[i].size;
			}
			m_biomesSize = array;
			m_worldTraitIds = new List<string>();
			m_worldTraitIds.AddRange(world.Settings.GetWorldTraitIDs());
			m_storyTraitIds = new List<string>();
			m_storyTraitIds.AddRange(world.Settings.GetStoryTraitIDs());
		}
		else
		{
			fullyEnclosedBorder = false;
			worldOffset = Vector2I.zero;
			worldSize = new Vector2I(Grid.WidthInCells, Grid.HeightInCells);
			isDiscovered = true;
			isStartWorld = true;
			isDupeVisited = true;
			m_seasonIds = new List<string> { Db.Get().GameplaySeasons.MeteorShowers.Id };
		}
	}

	public bool ContainsPoint(Vector2 point)
	{
		return point.x >= (float)worldOffset.x && point.y >= (float)worldOffset.y && point.x < (float)(worldOffset.x + worldSize.x) && point.y < (float)(worldOffset.y + worldSize.y);
	}

	public void LookAtSurface()
	{
		if (!IsDupeVisited)
		{
			RevealSurface();
		}
		Vector3? vector = SetSurfaceCameraPos();
		if (ClusterManager.Instance.activeWorldId == id && vector.HasValue)
		{
			CameraController.Instance.SnapTo(vector.Value);
		}
	}

	public void RevealSurface()
	{
		if (isSurfaceRevealed)
		{
			return;
		}
		isSurfaceRevealed = true;
		for (int i = 0; i < worldSize.x; i++)
		{
			int num = worldSize.y - 1;
			while (num >= 0)
			{
				int cell = Grid.XYToCell(i + worldOffset.x, num + worldOffset.y);
				if (Grid.IsValidCell(cell) && !Grid.IsSolidCell(cell) && !Grid.IsLiquid(cell))
				{
					GridVisibility.Reveal(i + worldOffset.X, num + worldOffset.y, 7, 1f);
					num--;
					continue;
				}
				break;
			}
		}
	}

	public void RevealHiddenY()
	{
		hiddenYOffset = 0;
	}

	private Vector3? SetSurfaceCameraPos()
	{
		if (SaveGame.Instance != null)
		{
			int num = (int)maximumBounds.y;
			for (int i = 0; i < worldSize.X; i++)
			{
				for (int num2 = worldSize.y - 1; num2 >= 0; num2--)
				{
					int num3 = num2 + worldOffset.y;
					int num4 = Grid.XYToCell(i + worldOffset.x, num3);
					if (Grid.IsValidCell(num4) && (Grid.Solid[num4] || Grid.IsLiquid(num4)))
					{
						num = Math.Min(num, num3);
						break;
					}
				}
			}
			int num5 = (num + worldOffset.y + worldSize.y) / 2;
			Vector3 vector = new Vector3(WorldOffset.x + Width / 2, num5, 0f);
			SaveGame.Instance.GetComponent<UserNavigation>().SetWorldCameraStartPosition(id, vector);
			return vector;
		}
		return null;
	}

	public void EjectAllDupes(Vector3 spawn_pos)
	{
		List<MinionIdentity> worldItems = Components.MinionIdentities.GetWorldItems(id);
		foreach (MinionIdentity item in worldItems)
		{
			item.transform.SetLocalPosition(spawn_pos);
		}
	}

	public void SpacePodAllDupes(AxialI sourceLocation, SimHashes podElement)
	{
		List<MinionIdentity> worldItems = Components.MinionIdentities.GetWorldItems(id);
		foreach (MinionIdentity item in worldItems)
		{
			if (!item.HasTag(GameTags.Dead))
			{
				GameObject gameObject = Util.KInstantiate(position: new Vector3(-1f, -1f, 0f), original: Assets.GetPrefab("EscapePod"));
				gameObject.GetComponent<PrimaryElement>().SetElement(podElement);
				gameObject.SetActive(value: true);
				MinionStorage component = gameObject.GetComponent<MinionStorage>();
				component.SerializeMinion(item.gameObject);
				TravellingCargoLander.StatesInstance sMI = gameObject.GetSMI<TravellingCargoLander.StatesInstance>();
				sMI.StartSM();
				sMI.Travel(sourceLocation, ClusterUtil.ClosestVisibleAsteroidToLocation(sourceLocation).Location);
			}
		}
	}

	public void DestroyWorldBuildings(out HashSet<int> noRefundTiles)
	{
		TransferBuildingMaterials(out noRefundTiles);
		List<ClustercraftInteriorDoor> worldItems = Components.ClusterCraftInteriorDoors.GetWorldItems(id);
		foreach (ClustercraftInteriorDoor item in worldItems)
		{
			item.DeleteObject();
		}
		ClearWorldZones();
	}

	public void TransferResourcesToParentWorld(Vector3 spawn_pos, HashSet<int> noRefundTiles)
	{
		TransferPickupables(spawn_pos);
		TransferLiquidsSolidsAndGases(spawn_pos, noRefundTiles);
	}

	public void TransferResourcesToDebris(AxialI sourceLocation, HashSet<int> noRefundTiles, SimHashes debrisContainerElement)
	{
		List<Storage> debrisObjects = new List<Storage>();
		TransferPickupablesToDebris(ref debrisObjects, debrisContainerElement);
		TransferLiquidsSolidsAndGasesToDebris(ref debrisObjects, noRefundTiles, debrisContainerElement);
		foreach (Storage item in debrisObjects)
		{
			RailGunPayload.StatesInstance sMI = item.GetSMI<RailGunPayload.StatesInstance>();
			sMI.StartSM();
			sMI.Travel(sourceLocation, ClusterUtil.ClosestVisibleAsteroidToLocation(sourceLocation).Location);
		}
	}

	private void TransferBuildingMaterials(out HashSet<int> noRefundTiles)
	{
		HashSet<int> retTemplateFoundationCells = new HashSet<int>();
		ListPool<ScenePartitionerEntry, ClusterManager>.PooledList pooledList = ListPool<ScenePartitionerEntry, ClusterManager>.Allocate();
		GameScenePartitioner.Instance.GatherEntries((int)minimumBounds.x, (int)minimumBounds.y, Width, Height, GameScenePartitioner.Instance.completeBuildings, pooledList);
		foreach (ScenePartitionerEntry item in pooledList)
		{
			BuildingComplete buildingComplete = item.obj as BuildingComplete;
			if (!(buildingComplete != null))
			{
				continue;
			}
			Deconstructable component = buildingComplete.GetComponent<Deconstructable>();
			if (component != null && !buildingComplete.HasTag(GameTags.NoRocketRefund))
			{
				PrimaryElement component2 = buildingComplete.GetComponent<PrimaryElement>();
				float temperature = component2.Temperature;
				byte diseaseIdx = component2.DiseaseIdx;
				int diseaseCount = component2.DiseaseCount;
				for (int i = 0; i < component.constructionElements.Length && buildingComplete.Def.Mass.Length > i; i++)
				{
					Element element = ElementLoader.GetElement(component.constructionElements[i]);
					if (element != null)
					{
						element.substance.SpawnResource(buildingComplete.transform.GetPosition(), buildingComplete.Def.Mass[i], temperature, diseaseIdx, diseaseCount);
						continue;
					}
					GameObject prefab = Assets.GetPrefab(component.constructionElements[i]);
					for (int j = 0; (float)j < buildingComplete.Def.Mass[i]; j++)
					{
						GameObject gameObject = GameUtil.KInstantiate(prefab, buildingComplete.transform.GetPosition(), Grid.SceneLayer.Ore);
						gameObject.SetActive(value: true);
					}
				}
			}
			SimCellOccupier component3 = buildingComplete.GetComponent<SimCellOccupier>();
			if (component3 != null && component3.doReplaceElement)
			{
				buildingComplete.RunOnArea(delegate(int cell)
				{
					retTemplateFoundationCells.Add(cell);
				});
			}
			Storage component4 = buildingComplete.GetComponent<Storage>();
			if (component4 != null)
			{
				component4.DropAll();
			}
			PlantablePlot component5 = buildingComplete.GetComponent<PlantablePlot>();
			if (component5 != null)
			{
				SeedProducer seedProducer = ((component5.Occupant != null) ? component5.Occupant.GetComponent<SeedProducer>() : null);
				if (seedProducer != null)
				{
					seedProducer.DropSeed();
				}
			}
			buildingComplete.DeleteObject();
		}
		pooledList.Clear();
		noRefundTiles = retTemplateFoundationCells;
	}

	private void TransferPickupables(Vector3 pos)
	{
		int cell = Grid.PosToCell(pos);
		ListPool<ScenePartitionerEntry, ClusterManager>.PooledList pooledList = ListPool<ScenePartitionerEntry, ClusterManager>.Allocate();
		GameScenePartitioner.Instance.GatherEntries((int)minimumBounds.x, (int)minimumBounds.y, Width, Height, GameScenePartitioner.Instance.pickupablesLayer, pooledList);
		foreach (ScenePartitionerEntry item in pooledList)
		{
			if (item.obj != null)
			{
				Pickupable pickupable = item.obj as Pickupable;
				if (pickupable != null)
				{
					pickupable.gameObject.transform.SetLocalPosition(Grid.CellToPosCBC(cell, Grid.SceneLayer.Move));
				}
			}
		}
		pooledList.Recycle();
	}

	private void TransferLiquidsSolidsAndGases(Vector3 pos, HashSet<int> noRefundTiles)
	{
		for (int i = (int)minimumBounds.x; (float)i <= maximumBounds.x; i++)
		{
			for (int j = (int)minimumBounds.y; (float)j <= maximumBounds.y; j++)
			{
				int num = Grid.XYToCell(i, j);
				if (!noRefundTiles.Contains(num))
				{
					Element element = Grid.Element[num];
					if (element != null && !element.IsVacuum)
					{
						element.substance.SpawnResource(pos, Grid.Mass[num], Grid.Temperature[num], Grid.DiseaseIdx[num], Grid.DiseaseCount[num]);
					}
				}
			}
		}
	}

	private void TransferPickupablesToDebris(ref List<Storage> debrisObjects, SimHashes debrisContainerElement)
	{
		ListPool<ScenePartitionerEntry, ClusterManager>.PooledList pooledList = ListPool<ScenePartitionerEntry, ClusterManager>.Allocate();
		GameScenePartitioner.Instance.GatherEntries((int)minimumBounds.x, (int)minimumBounds.y, Width, Height, GameScenePartitioner.Instance.pickupablesLayer, pooledList);
		foreach (ScenePartitionerEntry item in pooledList)
		{
			if (item.obj == null)
			{
				continue;
			}
			Pickupable pickupable = item.obj as Pickupable;
			if (!(pickupable != null))
			{
				continue;
			}
			if (pickupable.KPrefabID.HasTag(GameTags.BaseMinion))
			{
				Util.KDestroyGameObject(pickupable.gameObject);
				continue;
			}
			pickupable.PrimaryElement.Units = Mathf.Max(1, Mathf.RoundToInt(pickupable.PrimaryElement.Units * 0.5f));
			if ((debrisObjects.Count == 0 || debrisObjects[debrisObjects.Count - 1].RemainingCapacity() == 0f) && pickupable.PrimaryElement.Mass > 0f)
			{
				debrisObjects.Add(CraftModuleInterface.SpawnRocketDebris(" from World Objects", debrisContainerElement));
			}
			Storage storage = debrisObjects[debrisObjects.Count - 1];
			while (pickupable.PrimaryElement.Mass > storage.RemainingCapacity())
			{
				int num = Mathf.Max(1, Mathf.RoundToInt(storage.RemainingCapacity() / pickupable.PrimaryElement.MassPerUnit));
				Pickupable pickupable2 = pickupable.Take(num);
				storage.Store(pickupable2.gameObject);
				storage = CraftModuleInterface.SpawnRocketDebris(" from World Objects", debrisContainerElement);
				debrisObjects.Add(storage);
			}
			if (pickupable.PrimaryElement.Mass > 0f)
			{
				storage.Store(pickupable.gameObject);
			}
		}
		pooledList.Recycle();
	}

	private void TransferLiquidsSolidsAndGasesToDebris(ref List<Storage> debrisObjects, HashSet<int> noRefundTiles, SimHashes debrisContainerElement)
	{
		for (int i = (int)minimumBounds.x; (float)i <= maximumBounds.x; i++)
		{
			for (int j = (int)minimumBounds.y; (float)j <= maximumBounds.y; j++)
			{
				int num = Grid.XYToCell(i, j);
				if (noRefundTiles.Contains(num))
				{
					continue;
				}
				Element element = Grid.Element[num];
				if (element == null || element.IsVacuum)
				{
					continue;
				}
				float num2 = Grid.Mass[num];
				num2 *= 0.5f;
				if ((debrisObjects.Count == 0 || debrisObjects[debrisObjects.Count - 1].RemainingCapacity() == 0f) && num2 > 0f)
				{
					debrisObjects.Add(CraftModuleInterface.SpawnRocketDebris(" from World Tiles", debrisContainerElement));
				}
				Storage storage = debrisObjects[debrisObjects.Count - 1];
				while (num2 > 0f)
				{
					float num3 = Mathf.Min(num2, storage.RemainingCapacity());
					num2 -= num3;
					storage.AddOre(element.id, num3, Grid.Temperature[num], Grid.DiseaseIdx[num], Grid.DiseaseCount[num]);
					if (num2 > 0f)
					{
						storage = CraftModuleInterface.SpawnRocketDebris(" from World Tiles", debrisContainerElement);
						debrisObjects.Add(storage);
					}
				}
			}
		}
	}

	public void CancelChores()
	{
		for (int i = 0; i < 45; i++)
		{
			for (int j = (int)minimumBounds.x; (float)j <= maximumBounds.x; j++)
			{
				for (int k = (int)minimumBounds.y; (float)k <= maximumBounds.y; k++)
				{
					int cell = Grid.XYToCell(j, k);
					GameObject gameObject = Grid.Objects[cell, i];
					if (gameObject != null)
					{
						gameObject.Trigger(2127324410, (object)BoxedBools.True);
					}
				}
			}
		}
		GlobalChoreProvider.Instance.choreWorldMap.TryGetValue(id, out var value);
		int num = 0;
		while (value != null && num < value.Count)
		{
			Chore chore = value[num];
			if (chore != null && chore.target != null && !chore.isNull)
			{
				chore.Cancel("World destroyed");
			}
			num++;
		}
		GlobalChoreProvider.Instance.fetchMap.TryGetValue(id, out var value2);
		int num2 = 0;
		while (value2 != null && num2 < value2.Count)
		{
			FetchChore fetchChore = value2[num2];
			if (fetchChore != null && fetchChore.target != null && !fetchChore.isNull)
			{
				fetchChore.Cancel("World destroyed");
			}
			num2++;
		}
	}

	public void ClearWorldZones()
	{
		if (this.overworldCell != null)
		{
			WorldDetailSave clusterDetailSave = SaveLoader.Instance.clusterDetailSave;
			int num = -1;
			for (int i = 0; i < SaveLoader.Instance.clusterDetailSave.overworldCells.Count; i++)
			{
				WorldDetailSave.OverworldCell overworldCell = SaveLoader.Instance.clusterDetailSave.overworldCells[i];
				if (overworldCell.zoneType == this.overworldCell.zoneType && overworldCell.tags != null && this.overworldCell.tags != null && overworldCell.tags.ContainsAll(this.overworldCell.tags) && overworldCell.poly.bounds == this.overworldCell.poly.bounds)
				{
					num = i;
					break;
				}
			}
			if (num >= 0)
			{
				clusterDetailSave.overworldCells.RemoveAt(num);
			}
		}
		for (int j = (int)minimumBounds.y; (float)j <= maximumBounds.y; j++)
		{
			for (int k = (int)minimumBounds.x; (float)k <= maximumBounds.x; k++)
			{
				int cell = Grid.XYToCell(k, j);
				SimMessages.ModifyCellWorldZone(cell, byte.MaxValue);
			}
		}
	}

	public int GetSafeCell()
	{
		if (IsModuleInterior)
		{
			foreach (RocketControlStation item in Components.RocketControlStations.Items)
			{
				if (item.GetMyWorldId() == id)
				{
					return Grid.PosToCell(item);
				}
			}
		}
		else
		{
			foreach (Telepad item2 in Components.Telepads.Items)
			{
				if (item2.GetMyWorldId() == id)
				{
					return Grid.PosToCell(item2);
				}
			}
		}
		return Grid.XYToCell(worldOffset.x + worldSize.x / 2, worldOffset.y + worldSize.y / 2);
	}

	public string GetStatus()
	{
		return ColonyDiagnosticUtility.Instance.GetWorldDiagnosticResultStatus(id);
	}
}
