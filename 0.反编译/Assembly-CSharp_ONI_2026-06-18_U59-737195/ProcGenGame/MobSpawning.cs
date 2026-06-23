using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProcGen;
using ProcGen.Map;
using STRINGS;
using UnityEngine;

namespace ProcGenGame;

public static class MobSpawning
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct AllocatorTag
	{
	}

	public static Dictionary<int, string> PlaceFeatureAmbientMobs(WorldGenSettings settings, TerrainCell tc, SeededRandom rnd, ref WorldgenSimData simData, HashSet<int> cavityCells, HashSet<int> avoidCells, bool isDebug, ref HashSet<int> alreadyOccupiedCells)
	{
		Dictionary<int, string> spawnedMobs = new Dictionary<int, string>();
		Cell node = tc.node;
		FeatureSettings featureSettings = null;
		foreach (Tag featureSpecificTag in node.featureSpecificTags)
		{
			if (settings.HasFeature(featureSpecificTag.Name))
			{
				featureSettings = settings.GetFeature(featureSpecificTag.Name);
				break;
			}
		}
		if (featureSettings == null)
		{
			return spawnedMobs;
		}
		if (featureSettings.internalMobs == null || featureSettings.internalMobs.Count == 0)
		{
			return spawnedMobs;
		}
		List<int> availableSpawnCellsFeature = tc.GetAvailableSpawnCellsFeature();
		tc.LogInfo("PlaceFeatureAmbientMobs", "possibleSpawnPoints", availableSpawnCellsFeature.Count);
		for (int num = availableSpawnCellsFeature.Count - 1; num > 0; num--)
		{
			int num2 = availableSpawnCellsFeature[num];
			if (ElementLoader.elements[simData.cells[num2].elementIdx].id == SimHashes.Katairite || ElementLoader.elements[simData.cells[num2].elementIdx].id == SimHashes.Unobtanium || avoidCells.Contains(num2))
			{
				availableSpawnCellsFeature.RemoveAt(num);
			}
		}
		tc.LogInfo("mob spawns", "Id:" + node.NodeId + " possible cells", availableSpawnCellsFeature.Count);
		if (availableSpawnCellsFeature.Count == 0)
		{
			if (isDebug)
			{
				Debug.LogWarning("No where to put mobs possibleSpawnPoints [" + tc.node.NodeId + "]");
			}
			return null;
		}
		foreach (MobReference internalMob in featureSettings.internalMobs)
		{
			Mob mob = settings.GetMob(internalMob.type);
			if (mob == null)
			{
				Debug.LogError("Missing mob description for internal mob [" + internalMob.type + "]");
				continue;
			}
			List<int> mobPossibleSpawnPoints = GetMobPossibleSpawnPoints(mob, availableSpawnCellsFeature, ref simData, tc, cavityCells, alreadyOccupiedCells, rnd);
			if (mobPossibleSpawnPoints.Count == 0)
			{
				if (!isDebug)
				{
				}
			}
			else
			{
				tc.LogInfo("\t\tpossible", internalMob.type + " mps: " + mobPossibleSpawnPoints.Count + " ps:", availableSpawnCellsFeature.Count);
				int num3 = Mathf.RoundToInt(internalMob.count.GetRandomValueWithinRange(rnd));
				tc.LogInfo("\t\tcount", internalMob.type, num3);
				Tag mobPrefab = ((mob.prefabName == null) ? new Tag(internalMob.type) : new Tag(mob.prefabName));
				SpawnCountMobs(mob, mobPrefab, num3, mobPossibleSpawnPoints, tc, ref spawnedMobs, ref alreadyOccupiedCells);
			}
		}
		return spawnedMobs;
	}

	public static Dictionary<int, string> PlaceBiomeAmbientMobs(WorldGenSettings settings, TerrainCell tc, SeededRandom rnd, ref WorldgenSimData simData, HashSet<int> cavityCells, HashSet<int> avoidCells, bool isDebug, ref HashSet<int> alreadyOccupiedCells)
	{
		Dictionary<int, string> spawnedMobs = new Dictionary<int, string>();
		Cell node = tc.node;
		List<Tag> list = new List<Tag>();
		if (node.biomeSpecificTags == null)
		{
			tc.LogInfo("PlaceBiomeAmbientMobs", "No tags", node.NodeId);
			return null;
		}
		foreach (Tag biomeSpecificTag in node.biomeSpecificTags)
		{
			if (settings.HasMob(biomeSpecificTag.Name) && settings.GetMob(biomeSpecificTag.Name) != null)
			{
				list.Add(biomeSpecificTag);
			}
		}
		if (list.Count <= 0)
		{
			tc.LogInfo("PlaceBiomeAmbientMobs", "No biome MOBS", node.NodeId);
			return null;
		}
		List<int> list2 = (node.tags.Contains(WorldGenTags.PreventAmbientMobsInFeature) ? tc.GetAvailableSpawnCellsBiome() : tc.GetAvailableSpawnCellsAll());
		tc.LogInfo("PlaceBiomAmbientMobs", "possibleSpawnPoints", list2.Count);
		for (int num = list2.Count - 1; num > 0; num--)
		{
			int num2 = list2[num];
			if (ElementLoader.elements[simData.cells[num2].elementIdx].id == SimHashes.Katairite || ElementLoader.elements[simData.cells[num2].elementIdx].id == SimHashes.Unobtanium || avoidCells.Contains(num2))
			{
				list2.RemoveAt(num);
			}
		}
		tc.LogInfo("mob spawns", "Id:" + node.NodeId + " possible cells", list2.Count);
		if (list2.Count == 0)
		{
			if (isDebug)
			{
				Debug.LogWarning("No where to put mobs possibleSpawnPoints [" + tc.node.NodeId + "]");
			}
			return null;
		}
		list.ShuffleSeeded(rnd.RandomSource());
		for (int i = 0; i < list.Count; i++)
		{
			Mob mob = settings.GetMob(list[i].Name);
			if (mob == null)
			{
				Debug.LogError("Missing sample description for tag [" + list[i].Name + "]");
				continue;
			}
			List<int> mobPossibleSpawnPoints = GetMobPossibleSpawnPoints(mob, list2, ref simData, tc, cavityCells, alreadyOccupiedCells, rnd);
			if (mobPossibleSpawnPoints.Count == 0)
			{
				if (!isDebug)
				{
				}
				continue;
			}
			tc.LogInfo("\t\tpossible", list[i].ToString() + " mps: " + mobPossibleSpawnPoints.Count + " ps:", list2.Count);
			float num3 = mob.density.GetRandomValueWithinRange(rnd) * MobSettings.AmbientMobDensity;
			if (num3 > 1f)
			{
				if (isDebug)
				{
					Debug.LogWarning("Got a mob density greater than 1.0 for " + list[i].Name + ". Probably using density as spacing!");
				}
				num3 = 1f;
			}
			tc.LogInfo("\t\tdensity:", "", num3);
			int num4 = Mathf.RoundToInt((float)mobPossibleSpawnPoints.Count * num3);
			tc.LogInfo("\t\tcount", list[i].ToString(), num4);
			Tag mobPrefab = ((mob.prefabName == null) ? list[i] : new Tag(mob.prefabName));
			SpawnCountMobs(mob, mobPrefab, num4, mobPossibleSpawnPoints, tc, ref spawnedMobs, ref alreadyOccupiedCells);
		}
		return spawnedMobs;
	}

	private static List<int> GetMobPossibleSpawnPoints(Mob mob, List<int> possibleSpawnPoints, ref WorldgenSimData simData, TerrainCell tc, HashSet<int> cavityCells, HashSet<int> alreadyOccupiedCells, SeededRandom rnd)
	{
		List<int> list = new List<int>();
		foreach (int possibleSpawnPoint in possibleSpawnPoints)
		{
			if (IsSuitableMobSpawnPoint(possibleSpawnPoint, mob, ref simData, cavityCells, ref alreadyOccupiedCells, tc))
			{
				list.Add(possibleSpawnPoint);
			}
		}
		list.ShuffleSeeded(rnd.RandomSource());
		return list;
	}

	public static void SpawnCountMobs(Mob mobData, Tag mobPrefab, int count, List<int> mobPossibleSpawnPoints, TerrainCell tc, ref Dictionary<int, string> spawnedMobs, ref HashSet<int> alreadyOccupiedCells)
	{
		int num = 0;
		for (int i = 0; i - num < count && i < mobPossibleSpawnPoints.Count; i++)
		{
			int num2 = mobPossibleSpawnPoints[i];
			int num3 = ((mobData.location != Mob.Location.Ceiling) ? 1 : (-1));
			bool flag = false;
			for (int j = 0; j < mobData.width + mobData.paddingX * 2; j++)
			{
				for (int k = 0; k < mobData.height; k++)
				{
					int item = MobWidthOffset(Grid.OffsetCell(num2, 0, k * num3), j);
					if (alreadyOccupiedCells.Contains(item))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				num++;
				continue;
			}
			for (int l = 0; l < mobData.width + mobData.paddingX * 2; l++)
			{
				for (int m = 0; m < mobData.height; m++)
				{
					int item2 = MobWidthOffset(Grid.OffsetCell(num2, 0, m * num3), l);
					alreadyOccupiedCells.Add(item2);
				}
			}
			tc.AddMob(new KeyValuePair<int, Tag>(num2, mobPrefab));
			spawnedMobs.Add(num2, mobPrefab.Name);
		}
	}

	public static int MobWidthOffset(int occupiedCell, int widthIterator)
	{
		return Grid.OffsetCell(occupiedCell, (widthIterator % 2 == 0) ? (-(widthIterator / 2)) : (widthIterator / 2 + widthIterator % 2), 0);
	}

	private static bool IsSuitableMobSpawnPoint(int cell, Mob mob, ref WorldgenSimData simData, HashSet<int> cavityCells, ref HashSet<int> alreadyOccupiedCells, TerrainCell tc)
	{
		int num = ((mob.location != Mob.Location.Ceiling && mob.location != Mob.Location.LiquidCeiling) ? 1 : (-1));
		if (!Grid.IsValidCell(cell))
		{
			return false;
		}
		int num2 = mob.width + mob.paddingX * 2;
		int num3 = num2 / 2 - mob.width - mob.paddingX + 1;
		CellOffset cellOffset = new CellOffset(num3 - 1, (num < 0) ? 1 : mob.height);
		CellOffset offset = cellOffset + new CellOffset(mob.width + 1, -(mob.height + 1));
		if (!Grid.IsCellOffsetValid(cell, cellOffset) || !Grid.IsCellOffsetValid(cell, offset))
		{
			return false;
		}
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < mob.height; j++)
			{
				int num4 = MobWidthOffset(Grid.OffsetCell(cell, 0, j * num), i);
				if (!Grid.IsValidCell(num4))
				{
					return false;
				}
				if (alreadyOccupiedCells.Contains(num4))
				{
					return false;
				}
			}
		}
		Element element = ElementLoader.elements[simData.cells[cell].elementIdx];
		Element element2 = ElementLoader.elements[simData.cells[Grid.CellAbove(cell)].elementIdx];
		switch (mob.location)
		{
		case Mob.Location.Solid:
		{
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'Solid' and is used for mob " + mob.name);
			for (int num13 = 0; num13 < mob.width; num13++)
			{
				for (int num14 = 0; num14 < mob.height; num14++)
				{
					int num15 = MobWidthOffset(Grid.OffsetCell(cell, 0, num14 * num), num13);
					if (cavityCells.Contains(num15) || !ElementLoader.elements[simData.cells[num15].elementIdx].IsSolid)
					{
						return false;
					}
				}
			}
			return true;
		}
		case Mob.Location.Ceiling:
		{
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'Ceiling' and is used for mob " + mob.name);
			bool flag = true;
			for (int k = 0; k < mob.height; k++)
			{
				for (int l = 0; l < mob.width; l++)
				{
					int occupiedCell = Grid.OffsetCell(cell, 0, -k);
					occupiedCell = MobWidthOffset(occupiedCell, l);
					Element element3 = ElementLoader.elements[simData.cells[occupiedCell].elementIdx];
					Element element4 = ElementLoader.elements[simData.cells[Grid.CellAbove(occupiedCell)].elementIdx];
					if (k == 0)
					{
						flag = flag && element4.IsSolid;
					}
					flag = flag && cavityCells.Contains(occupiedCell) && !element3.IsSolid && !element3.IsLiquid;
					if (!flag)
					{
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			return flag;
		}
		case Mob.Location.Floor:
		{
			bool flag9 = true;
			for (int num18 = 0; num18 < mob.height; num18++)
			{
				for (int num19 = 0; num19 < num2; num19++)
				{
					int occupiedCell7 = Grid.OffsetCell(cell, 0, num18);
					occupiedCell7 = MobWidthOffset(occupiedCell7, num19);
					Element element16 = ElementLoader.elements[simData.cells[occupiedCell7].elementIdx];
					_ = ElementLoader.elements[simData.cells[Grid.CellAbove(occupiedCell7)].elementIdx];
					Element element17 = ElementLoader.elements[simData.cells[Grid.CellBelow(occupiedCell7)].elementIdx];
					flag9 = flag9 && cavityCells.Contains(occupiedCell7) && !element16.IsSolid && !element16.IsLiquid;
					if (num18 == 0 && num19 < mob.width)
					{
						flag9 = flag9 && element17.IsSolid;
					}
					if (!flag9)
					{
						break;
					}
				}
				if (!flag9)
				{
					break;
				}
			}
			return flag9;
		}
		case Mob.Location.EntombedFloorPeek:
		{
			bool flag4 = false;
			bool flag5 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'EntombedFloorPeek' and is used for mob " + mob.name);
			for (int num7 = 0; num7 < mob.height; num7++)
			{
				for (int num8 = 0; num8 < mob.width; num8++)
				{
					int occupiedCell4 = Grid.OffsetCell(cell, 0, num7);
					occupiedCell4 = MobWidthOffset(occupiedCell4, num8);
					Element element8 = ElementLoader.elements[simData.cells[occupiedCell4].elementIdx];
					Element element9 = ElementLoader.elements[simData.cells[Grid.CellBelow(occupiedCell4)].elementIdx];
					flag4 = flag4 || !element8.IsSolid;
					if (num7 == 0)
					{
						flag5 = flag5 && element9.IsSolid;
					}
					if (!flag5)
					{
						break;
					}
				}
				if (!flag5)
				{
					break;
				}
			}
			return flag5 && flag4;
		}
		case Mob.Location.LiquidFloor:
		case Mob.Location.LiquidFloorCavityNoRequired:
		{
			bool flag3 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'LiquidFloor' and is used for mob " + mob.name);
			for (int num5 = 0; num5 < mob.height; num5++)
			{
				for (int num6 = 0; num6 < mob.width; num6++)
				{
					int occupiedCell3 = Grid.OffsetCell(cell, 0, num5);
					occupiedCell3 = MobWidthOffset(occupiedCell3, num6);
					Element element6 = ElementLoader.elements[simData.cells[occupiedCell3].elementIdx];
					Element element7 = ElementLoader.elements[simData.cells[Grid.CellBelow(occupiedCell3)].elementIdx];
					flag3 = flag3 && (mob.location == Mob.Location.LiquidFloorCavityNoRequired || cavityCells.Contains(cell)) && !element6.IsSolid;
					if (num5 == 0)
					{
						flag3 = flag3 && element6.IsLiquid && element7.IsSolid;
					}
					if (!flag3)
					{
						break;
					}
				}
				if (!flag3)
				{
					break;
				}
			}
			return flag3;
		}
		case Mob.Location.LiquidCeiling:
		{
			bool flag10 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'LiquidCeiling' and is used for mob " + mob.name);
			for (int num20 = 0; num20 < mob.height; num20++)
			{
				for (int num21 = 0; num21 < mob.width; num21++)
				{
					int occupiedCell8 = Grid.OffsetCell(cell, 0, -num20);
					occupiedCell8 = MobWidthOffset(occupiedCell8, num21);
					Element element18 = ElementLoader.elements[simData.cells[occupiedCell8].elementIdx];
					Element element19 = ElementLoader.elements[simData.cells[Grid.CellAbove(occupiedCell8)].elementIdx];
					if (num20 == 0)
					{
						flag10 = flag10 && element19.IsSolid;
					}
					flag10 = flag10 && cavityCells.Contains(occupiedCell8) && element18.IsLiquid && !element18.IsSolid;
					if (!flag10)
					{
						break;
					}
				}
				if (!flag10)
				{
					break;
				}
			}
			return flag10;
		}
		case Mob.Location.AnyFloor:
		{
			bool flag7 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'AnyFloor' and is used for mob " + mob.name);
			for (int num11 = 0; num11 < mob.height; num11++)
			{
				for (int num12 = 0; num12 < mob.width; num12++)
				{
					int occupiedCell5 = Grid.OffsetCell(cell, 0, num11);
					occupiedCell5 = MobWidthOffset(occupiedCell5, num12);
					Element element12 = ElementLoader.elements[simData.cells[occupiedCell5].elementIdx];
					_ = ElementLoader.elements[simData.cells[Grid.CellAbove(occupiedCell5)].elementIdx];
					Element element13 = ElementLoader.elements[simData.cells[Grid.CellBelow(occupiedCell5)].elementIdx];
					flag7 = flag7 && cavityCells.Contains(cell) && !element12.IsSolid;
					if (num11 == 0)
					{
						flag7 = flag7 && element13.IsSolid;
					}
					if (!flag7)
					{
						break;
					}
				}
				if (!flag7)
				{
					break;
				}
			}
			return flag7;
		}
		case Mob.Location.Air:
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'Air' and is used for mob " + mob.name);
			if (!element.IsSolid && !element2.IsSolid)
			{
				return !element.IsLiquid;
			}
			return false;
		case Mob.Location.Water:
			DebugUtil.DevLogError("Mob location rule 'Water' is deprecated, use 'Liquid' instead. Mob: " + mob.name);
			goto case Mob.Location.Liquid;
		case Mob.Location.Liquid:
		{
			bool flag8 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'Liquid' and is used for mob " + mob.name);
			for (int num16 = 0; num16 < mob.height; num16++)
			{
				for (int num17 = 0; num17 < mob.width; num17++)
				{
					int occupiedCell6 = Grid.OffsetCell(cell, 0, num16);
					occupiedCell6 = MobWidthOffset(occupiedCell6, num17);
					Element element14 = ElementLoader.elements[simData.cells[occupiedCell6].elementIdx];
					flag8 = flag8 && element14.IsLiquid;
					if (num16 == mob.height - 1)
					{
						Element element15 = ElementLoader.elements[simData.cells[Grid.CellAbove(occupiedCell6)].elementIdx];
						flag8 = flag8 && element15.IsLiquid;
					}
				}
			}
			return flag8;
		}
		case Mob.Location.Surface:
		{
			bool flag6 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'Surface' and is used for mob " + mob.name);
			for (int num9 = 0; num9 < mob.width; num9++)
			{
				int num10 = MobWidthOffset(cell, num9);
				Element element10 = ElementLoader.elements[simData.cells[num10].elementIdx];
				Element element11 = ElementLoader.elements[simData.cells[Grid.CellBelow(num10)].elementIdx];
				flag6 = flag6 && element10.id == SimHashes.Vacuum && element11.IsSolid;
			}
			return flag6;
		}
		case Mob.Location.AnchoredToBackWall:
		{
			bool flag2 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'AnchoredToBackWall' and is used for mob " + mob.name);
			for (int m = 0; m < mob.height; m++)
			{
				for (int n = 0; n < mob.width; n++)
				{
					int occupiedCell2 = Grid.OffsetCell(cell, 0, m);
					occupiedCell2 = MobWidthOffset(occupiedCell2, n);
					Element element5 = ElementLoader.elements[simData.cells[occupiedCell2].elementIdx];
					bool isSolid = ElementLoader.elements[simData.backwallCells[cell].elementIdx].IsSolid;
					flag2 = flag2 && isSolid && !element5.IsSolid;
					if (!flag2)
					{
						break;
					}
				}
				if (!flag2)
				{
					break;
				}
			}
			return flag2;
		}
		default:
			if (cavityCells.Contains(cell))
			{
				return !element.IsSolid;
			}
			return false;
		}
	}

	public static void DetectNaturalCavities(int world, Sim.Cell[] cells, HashSet<int> cavityCells, WorldGen.OfflineCallbackFunction updateProgressFn)
	{
		updateProgressFn(UI.WORLDGEN.ANALYZINGWORLD.key, 0f, WorldGenProgressStages.Stages.DetectNaturalCavities);
		Func<int, FloodFill.BoundaryCheckResult> boundaryCondition = BoundaryCondition;
		HashSetPool<int, AllocatorTag>.PooledHashSet pooledHashSet = HashSetPool<int, AllocatorTag>.Allocate();
		ListPool<int, AllocatorTag>.PooledList pooledList = ListPool<int, AllocatorTag>.Allocate();
		for (int i = 0; i < cells.Length; i++)
		{
			float completePercent = (float)pooledHashSet.Count / (float)cells.Length;
			updateProgressFn(UI.WORLDGEN.ANALYZINGWORLD.key, completePercent, WorldGenProgressStages.Stages.DetectNaturalCavities);
			pooledList.Clear();
			FloodFill.DepthCollect(i, boundaryCondition, pooledHashSet, pooledList);
			if (pooledList.Count != 0 && pooledList.Count <= 300)
			{
				cavityCells.UnionWith(pooledList);
			}
		}
		pooledList.Recycle();
		updateProgressFn(UI.WORLDGEN.ANALYZINGWORLDCOMPLETE.key, 1f, WorldGenProgressStages.Stages.DetectNaturalCavities);
		pooledHashSet.Recycle();
		FloodFill.BoundaryCheckResult BoundaryCondition(int cell)
		{
			if (!ElementLoader.elements[cells[cell].elementIdx].IsSolid)
			{
				return FloodFill.BoundaryCheckResult.Continue;
			}
			return FloodFill.BoundaryCheckResult.Halt;
		}
	}
}
