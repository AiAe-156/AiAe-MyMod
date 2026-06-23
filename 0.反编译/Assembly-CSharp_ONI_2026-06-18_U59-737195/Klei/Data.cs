using System.Collections.Generic;
using ProcGen;
using ProcGenGame;
using VoronoiTree;

namespace Klei;

public class Data
{
	public int globalWorldSeed;

	public int globalWorldLayoutSeed;

	public int globalTerrainSeed;

	public int globalNoiseSeed;

	public int chunkEdgeSize = 32;

	public WorldLayout worldLayout;

	public List<TerrainCell> terrainCells;

	public List<TerrainCell> overworldCells;

	public List<BiomeSizeData> biomes;

	public List<River> rivers;

	public GameSpawnData gameSpawnData;

	public Chunk world;

	public Tree voronoiTree;

	public AxialI clusterLocation;

	public Data()
	{
		worldLayout = new WorldLayout(null, 0);
		terrainCells = new List<TerrainCell>();
		overworldCells = new List<TerrainCell>();
		biomes = new List<BiomeSizeData>();
		rivers = new List<River>();
		gameSpawnData = new GameSpawnData();
		world = new Chunk();
		voronoiTree = new Tree(0);
	}
}
