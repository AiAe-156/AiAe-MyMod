using System.Collections.Generic;
using ProcGen;
using ProcGenGame;
using UnityEngine;

namespace Klei;

public class BiomeSizeData
{
	private SubWorld.ZoneType biome;

	public Vector4 size;

	public List<TerrainCell> terrainCells;

	public BiomeSizeData(SubWorld.ZoneType id, Vector4 size, List<TerrainCell> terrainCells)
	{
		biome = id;
		this.size = size;
		this.terrainCells = terrainCells;
	}
}
