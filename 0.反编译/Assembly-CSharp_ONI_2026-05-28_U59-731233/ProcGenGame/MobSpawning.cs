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
		Sim.Cell[] cells = simData.cells;
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
					int occupiedCell = Grid.OffsetCell(num2, 0, k * num3);
					int item = MobWidthOffset(occupiedCell, j);
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
					int occupiedCell2 = Grid.OffsetCell(num2, 0, m * num3);
					int item2 = MobWidthOffset(occupiedCell2, l);
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
		int num3 = num2 / 2;
		int num4 = num3 - mob.width - mob.paddingX + 1;
		CellOffset cellOffset = new CellOffset(num4 - 1, (num < 0) ? 1 : mob.height);
		CellOffset offset = cellOffset + new CellOffset(mob.width + 1, -(mob.height + 1));
		if (!Grid.IsCellOffsetValid(cell, cellOffset) || !Grid.IsCellOffsetValid(cell, offset))
		{
			return false;
		}
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < mob.height; j++)
			{
				int occupiedCell = Grid.OffsetCell(cell, 0, j * num);
				int num5 = MobWidthOffset(occupiedCell, i);
				if (!Grid.IsValidCell(num5))
				{
					return false;
				}
				if (alreadyOccupiedCells.Contains(num5))
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
			for (int num20 = 0; num20 < mob.width; num20++)
			{
				for (int num21 = 0; num21 < mob.height; num21++)
				{
					int occupiedCell10 = Grid.OffsetCell(cell, 0, num21 * num);
					int num22 = MobWidthOffset(occupiedCell10, num20);
					if (cavityCells.Contains(num22) || !ElementLoader.elements[simData.cells[num22].elementIdx].IsSolid)
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
					int occupiedCell2 = Grid.OffsetCell(cell, 0, -k);
					occupiedCell2 = MobWidthOffset(occupiedCell2, l);
					Element element3 = ElementLoader.elements[simData.cells[occupiedCell2].elementIdx];
					Element element4 = ElementLoader.elements[simData.cells[Grid.CellAbove(occupiedCell2)].elementIdx];
					if (k == 0)
					{
						flag = flag && element4.IsSolid;
					}
					flag = flag && cavityCells.Contains(occupiedCell2) && !element3.IsSolid && !element3.IsLiquid;
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
			bool flag2 = true;
			for (int m = 0; m < mob.height; m++)
			{
				for (int n = 0; n < num2; n++)
				{
					int occupiedCell3 = Grid.OffsetCell(cell, 0, m);
					occupiedCell3 = MobWidthOffset(occupiedCell3, n);
					Element element5 = ElementLoader.elements[simData.cells[occupiedCell3].elementIdx];
					Element element6 = ElementLoader.elements[simData.cells[Grid.CellAbove(occupiedCell3)].elementIdx];
					Element element7 = ElementLoader.elements[simData.cells[Grid.CellBelow(occupiedCell3)].elementIdx];
					flag2 = flag2 && cavityCells.Contains(occupiedCell3) && !element5.IsSolid && !element5.IsLiquid;
					if (m == 0 && n < mob.width)
					{
						flag2 = flag2 && element7.IsSolid;
					}
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
		case Mob.Location.EntombedFloorPeek:
		{
			bool flag5 = false;
			bool flag6 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'EntombedFloorPeek' and is used for mob " + mob.name);
			for (int num10 = 0; num10 < mob.height; num10++)
			{
				for (int num11 = 0; num11 < mob.width; num11++)
				{
					int occupiedCell6 = Grid.OffsetCell(cell, 0, num10);
					occupiedCell6 = MobWidthOffset(occupiedCell6, num11);
					Element element11 = ElementLoader.elements[simData.cells[occupiedCell6].elementIdx];
					Element element12 = ElementLoader.elements[simData.cells[Grid.CellBelow(occupiedCell6)].elementIdx];
					flag5 = flag5 || !element11.IsSolid;
					if (num10 == 0)
					{
						flag6 = flag6 && element12.IsSolid;
					}
					if (!flag6)
					{
						break;
					}
				}
				if (!flag6)
				{
					break;
				}
			}
			return flag6 && flag5;
		}
		case Mob.Location.LiquidFloor:
		case Mob.Location.LiquidFloorCavityNoRequired:
		{
			bool flag4 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'LiquidFloor' and is used for mob " + mob.name);
			for (int num8 = 0; num8 < mob.height; num8++)
			{
				for (int num9 = 0; num9 < mob.width; num9++)
				{
					int occupiedCell5 = Grid.OffsetCell(cell, 0, num8);
					occupiedCell5 = MobWidthOffset(occupiedCell5, num9);
					Element element9 = ElementLoader.elements[simData.cells[occupiedCell5].elementIdx];
					Element element10 = ElementLoader.elements[simData.cells[Grid.CellBelow(occupiedCell5)].elementIdx];
					flag4 = flag4 && (mob.location == Mob.Location.LiquidFloorCavityNoRequired || cavityCells.Contains(cell)) && !element9.IsSolid;
					if (num8 == 0)
					{
						flag4 = flag4 && element9.IsLiquid && element10.IsSolid;
					}
					if (!flag4)
					{
						break;
					}
				}
				if (!flag4)
				{
					break;
				}
			}
			return flag4;
		}
		case Mob.Location.LiquidCeiling:
		{
			bool flag8 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'LiquidCeiling' and is used for mob " + mob.name);
			for (int num14 = 0; num14 < mob.height; num14++)
			{
				for (int num15 = 0; num15 < mob.width; num15++)
				{
					int occupiedCell7 = Grid.OffsetCell(cell, 0, -num14);
					occupiedCell7 = MobWidthOffset(occupiedCell7, num15);
					Element element15 = ElementLoader.elements[simData.cells[occupiedCell7].elementIdx];
					Element element16 = ElementLoader.elements[simData.cells[Grid.CellAbove(occupiedCell7)].elementIdx];
					if (num14 == 0)
					{
						flag8 = flag8 && element16.IsSolid;
					}
					flag8 = flag8 && cavityCells.Contains(occupiedCell7) && element15.IsLiquid && !element15.IsSolid;
					if (!flag8)
					{
						break;
					}
				}
				if (!flag8)
				{
					break;
				}
			}
			return flag8;
		}
		case Mob.Location.AnyFloor:
		{
			bool flag9 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'AnyFloor' and is used for mob " + mob.name);
			for (int num16 = 0; num16 < mob.height; num16++)
			{
				for (int num17 = 0; num17 < mob.width; num17++)
				{
					int occupiedCell8 = Grid.OffsetCell(cell, 0, num16);
					occupiedCell8 = MobWidthOffset(occupiedCell8, num17);
					Element element17 = ElementLoader.elements[simData.cells[occupiedCell8].elementIdx];
					Element element18 = ElementLoader.elements[simData.cells[Grid.CellAbove(occupiedCell8)].elementIdx];
					Element element19 = ElementLoader.elements[simData.cells[Grid.CellBelow(occupiedCell8)].elementIdx];
					flag9 = flag9 && cavityCells.Contains(cell) && !element17.IsSolid;
					if (num16 == 0)
					{
						flag9 = flag9 && element19.IsSolid;
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
		case Mob.Location.Air:
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'Air' and is used for mob " + mob.name);
			return !element.IsSolid && !element2.IsSolid && !element.IsLiquid;
		case Mob.Location.Water:
			DebugUtil.DevLogError("Mob location rule 'Water' is deprecated, use 'Liquid' instead. Mob: " + mob.name);
			goto case Mob.Location.Liquid;
		case Mob.Location.Liquid:
		{
			bool flag10 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'Liquid' and is used for mob " + mob.name);
			for (int num18 = 0; num18 < mob.height; num18++)
			{
				for (int num19 = 0; num19 < mob.width; num19++)
				{
					int occupiedCell9 = Grid.OffsetCell(cell, 0, num18);
					occupiedCell9 = MobWidthOffset(occupiedCell9, num19);
					Element element20 = ElementLoader.elements[simData.cells[occupiedCell9].elementIdx];
					flag10 = flag10 && element20.IsLiquid;
					if (num18 == mob.height - 1)
					{
						Element element21 = ElementLoader.elements[simData.cells[Grid.CellAbove(occupiedCell9)].elementIdx];
						flag10 = flag10 && element21.IsLiquid;
					}
				}
			}
			return flag10;
		}
		case Mob.Location.Surface:
		{
			bool flag7 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'Surface' and is used for mob " + mob.name);
			for (int num12 = 0; num12 < mob.width; num12++)
			{
				int num13 = MobWidthOffset(cell, num12);
				Element element13 = ElementLoader.elements[simData.cells[num13].elementIdx];
				Element element14 = ElementLoader.elements[simData.cells[Grid.CellBelow(num13)].elementIdx];
				flag7 = flag7 && element13.id == SimHashes.Vacuum && element14.IsSolid;
			}
			return flag7;
		}
		case Mob.Location.AnchoredToBackWall:
		{
			bool flag3 = true;
			Debug.Assert(mob.paddingX == 0, "Mob paddingX not implemented yet for rule 'AnchoredToBackWall' and is used for mob " + mob.name);
			for (int num6 = 0; num6 < mob.height; num6++)
			{
				for (int num7 = 0; num7 < mob.width; num7++)
				{
					int occupiedCell4 = Grid.OffsetCell(cell, 0, num6);
					occupiedCell4 = MobWidthOffset(occupiedCell4, num7);
					Element element8 = ElementLoader.elements[simData.cells[occupiedCell4].elementIdx];
					bool isSolid = ElementLoader.elements[simData.backwallCells[cell].elementIdx].IsSolid;
					flag3 = flag3 && isSolid && !element8.IsSolid;
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
		default:
			return cavityCells.Contains(cell) && !element.IsSolid;
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
			Element element = ElementLoader.elements[cells[cell].elementIdx];
			return element.IsSolid ? FloodFill.BoundaryCheckResult.Halt : FloodFill.BoundaryCheckResult.Continue;
		}
	}
}
