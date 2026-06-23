using System;
using System.Collections.Generic;
using ProcGen;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("KMonoBehaviour/scripts/GroundRenderer")]
public class GroundRenderer : KMonoBehaviour
{
	[Serializable]
	private struct Materials
	{
		public Material opaque;

		public Material alpha;

		public Material backwall;

		public Material backwallAlpha;

		public Materials(Material opaque, Material alpha, Material backwall, Material backwallAlpha)
		{
			this.opaque = opaque;
			this.alpha = alpha;
			this.backwall = backwall;
			this.backwallAlpha = backwallAlpha;
		}
	}

	private class ElementChunk
	{
		private class RenderData
		{
			public Material material;

			public Mesh mesh;

			public List<Vector3> pos;

			public List<Vector2> uv;

			public List<int> indices;

			public RenderData(Material material)
			{
				this.material = material;
				mesh = new Mesh();
				mesh.MarkDynamic();
				mesh.name = "ElementChunk";
				pos = new List<Vector3>();
				uv = new List<Vector2>();
				indices = new List<int>();
			}

			public void ClearMesh()
			{
				if (mesh != null)
				{
					mesh.Clear();
					UnityEngine.Object.DestroyImmediate(mesh);
					mesh = null;
				}
			}

			public void Clear()
			{
				if (mesh != null)
				{
					mesh.Clear();
				}
				if (pos != null)
				{
					pos.Clear();
				}
				if (uv != null)
				{
					uv.Clear();
				}
				if (indices != null)
				{
					indices.Clear();
				}
			}

			public void FreeResources()
			{
				ClearMesh();
				Clear();
				pos = null;
				uv = null;
				indices = null;
				material = null;
			}

			public void Build()
			{
				mesh.SetVertices(pos);
				mesh.SetUVs(0, uv);
				mesh.SetTriangles(indices, 0);
			}

			public void AddQuad(int x, int y, GroundMasks.UVData uvs)
			{
				int count = pos.Count;
				indices.Add(count);
				indices.Add(count + 1);
				indices.Add(count + 3);
				indices.Add(count);
				indices.Add(count + 3);
				indices.Add(count + 2);
				pos.Add(new Vector3((float)x + -0.5f, (float)y + -0.5f, 0f));
				pos.Add(new Vector3((float)x + 1f + -0.5f, (float)y + -0.5f, 0f));
				pos.Add(new Vector3((float)x + -0.5f, (float)y + 1f + -0.5f, 0f));
				pos.Add(new Vector3((float)x + 1f + -0.5f, (float)y + 1f + -0.5f, 0f));
				uv.Add(uvs.bl);
				uv.Add(uvs.br);
				uv.Add(uvs.tl);
				uv.Add(uvs.tr);
			}

			public void Render(Vector3 position, int layer)
			{
				if (pos.Count != 0)
				{
					Graphics.DrawMesh(mesh, position, Quaternion.identity, material, layer, null, 0, null, ShadowCastingMode.Off, receiveShadows: false, null, useLightProbes: false);
				}
			}
		}

		public SimHashes element;

		private RenderData alpha;

		private RenderData opaque;

		private RenderData backwall;

		private RenderData backwallAlpha;

		public int tileCount;

		public ElementChunk(SimHashes element, Dictionary<SimHashes, Materials> materials)
		{
			this.element = element;
			Materials materials2 = materials[element];
			alpha = new RenderData(materials2.alpha);
			opaque = new RenderData(materials2.opaque);
			backwall = new RenderData(materials2.backwall);
			backwallAlpha = new RenderData(materials2.backwallAlpha);
			Clear();
		}

		public void Clear()
		{
			opaque.Clear();
			alpha.Clear();
			backwall.Clear();
			backwallAlpha.Clear();
			tileCount = 0;
		}

		public void AddOpaqueQuad(int x, int y, GroundMasks.UVData uvs)
		{
			opaque.AddQuad(x, y, uvs);
			tileCount++;
		}

		public void AddAlphaQuad(int x, int y, GroundMasks.UVData uvs)
		{
			alpha.AddQuad(x, y, uvs);
			tileCount++;
		}

		public void AddBackwallQuad(int x, int y, GroundMasks.UVData uvs)
		{
			backwall.AddQuad(x, y, uvs);
			tileCount++;
		}

		public void AddBackwallAlphaQuad(int x, int y, GroundMasks.UVData uvs)
		{
			backwallAlpha.AddQuad(x, y, uvs);
			tileCount++;
		}

		public void Build()
		{
			backwall.Build();
			opaque.Build();
			alpha.Build();
			backwallAlpha.Build();
		}

		public void Render(int layer, int element_idx)
		{
			float layerZ = Grid.GetLayerZ(Grid.SceneLayer.Ground);
			layerZ -= 0.0001f * (float)element_idx;
			float layerZ2 = Grid.GetLayerZ(Grid.SceneLayer.Backwall);
			layerZ2 -= 0.0001f * (float)element_idx;
			backwall.Render(new Vector3(0f, 0f, layerZ2), layer);
			backwallAlpha.Render(new Vector3(0f, 0f, layerZ2), layer);
			opaque.Render(new Vector3(0f, 0f, layerZ), layer);
			alpha.Render(new Vector3(0f, 0f, layerZ), layer);
		}

		public void FreeResources()
		{
			alpha.FreeResources();
			opaque.FreeResources();
			backwall.FreeResources();
			backwallAlpha.FreeResources();
			alpha = null;
			opaque = null;
			backwall = null;
			backwallAlpha = null;
		}
	}

	private struct WorldChunk
	{
		public class BiomeMaskCheck
		{
			public Func<int, Element> elementGet;

			public Func<Element, int, bool> elementCheck;

			public Action<ElementChunk, int, int, GroundMasks.UVData> opaqueAction;

			public Action<ElementChunk, int, int, GroundMasks.UVData> alphaAction;

			public BiomeMaskCheck(Func<int, Element> elementGet, Func<Element, int, bool> checkFn, Action<ElementChunk, int, int, GroundMasks.UVData> opaqueAction, Action<ElementChunk, int, int, GroundMasks.UVData> alphaAction)
			{
				this.elementGet = elementGet;
				elementCheck = checkFn;
				this.opaqueAction = opaqueAction;
				this.alphaAction = alphaAction;
			}
		}

		public readonly int chunkX;

		public readonly int chunkY;

		private List<ElementChunk> elementChunks;

		private static Element[] elements = new Element[4];

		private static Element[] uniqueElements = new Element[4];

		private static int[] substances = new int[4];

		private static BiomeMaskCheck[] biomeChecks = new BiomeMaskCheck[2]
		{
			new BiomeMaskCheck((int cell) => Grid.Element[cell], (Element e, int cell) => (cell == Grid.InvalidCell || Grid.RenderedByWorld[cell]) && e.IsSolid, delegate(ElementChunk ec, int a, int b, GroundMasks.UVData c)
			{
				ec.AddOpaqueQuad(a, b, c);
			}, delegate(ElementChunk ec, int a, int b, GroundMasks.UVData c)
			{
				ec.AddAlphaQuad(a, b, c);
			}),
			new BiomeMaskCheck((int cell) => BackwallManager.At(cell).Element ?? ElementLoader.FindElementByHash(SimHashes.Vacuum), (Element e, int cell) => e.id != SimHashes.Vacuum, delegate(ElementChunk ec, int a, int b, GroundMasks.UVData c)
			{
				ec.AddBackwallQuad(a, b, c);
			}, delegate(ElementChunk ec, int a, int b, GroundMasks.UVData c)
			{
				ec.AddBackwallAlphaQuad(a, b, c);
			})
		};

		private static Vector2 NoiseScale = new Vector3(1f, 1f);

		public WorldChunk(int x, int y)
		{
			chunkX = x;
			chunkY = y;
			elementChunks = new List<ElementChunk>();
		}

		public void Clear()
		{
			elementChunks.Clear();
		}

		private static void InsertSorted(Element element, Element[] array, int size)
		{
			int id = (int)element.id;
			for (int i = 0; i < size; i++)
			{
				Element element2 = array[i];
				if ((int)element2.id > id)
				{
					array[i] = element;
					element = element2;
					id = (int)element2.id;
				}
			}
			array[size] = element;
		}

		public void Rebuild(GroundMasks.BiomeMaskData[] biomeMasks, Dictionary<SimHashes, Materials> materials)
		{
			foreach (ElementChunk elementChunk3 in elementChunks)
			{
				elementChunk3.Clear();
			}
			Vector2I vector2I = new Vector2I(chunkX * 16, chunkY * 16);
			Vector2I vector2I2 = new Vector2I(Math.Min(Grid.WidthInCells, (chunkX + 1) * 16), Math.Min(Grid.HeightInCells, (chunkY + 1) * 16));
			for (int i = vector2I.y; i < vector2I2.y; i++)
			{
				int num = Math.Max(0, i - 1);
				int num2 = i;
				for (int j = vector2I.x; j < vector2I2.x; j++)
				{
					int num3 = Math.Max(0, j - 1);
					int num4 = j;
					int num5 = num * Grid.WidthInCells + num3;
					int num6 = num * Grid.WidthInCells + num4;
					int num7 = num2 * Grid.WidthInCells + num3;
					int num8 = num2 * Grid.WidthInCells + num4;
					for (int k = 0; k < biomeChecks.Length; k++)
					{
						BiomeMaskCheck biomeMaskCheck = biomeChecks[k];
						elements[0] = biomeMaskCheck.elementGet(num5);
						elements[1] = biomeMaskCheck.elementGet(num6);
						elements[2] = biomeMaskCheck.elementGet(num7);
						elements[3] = biomeMaskCheck.elementGet(num8);
						substances[0] = (biomeMaskCheck.elementCheck(elements[0], num5) ? elements[0].substance.idx : (-1));
						substances[1] = (biomeMaskCheck.elementCheck(elements[1], num6) ? elements[1].substance.idx : (-1));
						substances[2] = (biomeMaskCheck.elementCheck(elements[2], num7) ? elements[2].substance.idx : (-1));
						substances[3] = (biomeMaskCheck.elementCheck(elements[3], num8) ? elements[3].substance.idx : (-1));
						uniqueElements[0] = elements[0];
						InsertSorted(elements[1], uniqueElements, 1);
						InsertSorted(elements[2], uniqueElements, 2);
						InsertSorted(elements[3], uniqueElements, 3);
						int num9 = -1;
						int cell = i * Grid.WidthInCells + j;
						int biomeIdx = GetBiomeIdx(cell);
						GroundMasks.BiomeMaskData biomeMaskData = biomeMasks[biomeIdx];
						if (biomeMaskData == null)
						{
							biomeMaskData = biomeMasks[3];
						}
						for (int l = 0; l < uniqueElements.Length; l++)
						{
							Element element = uniqueElements[l];
							if (!biomeMaskCheck.elementCheck(element, Grid.InvalidCell))
							{
								continue;
							}
							int idx = element.substance.idx;
							if (idx == num9)
							{
								continue;
							}
							num9 = idx;
							int num10 = (((substances[2] >= idx) ? 1 : 0) << 3) | (((substances[3] >= idx) ? 1 : 0) << 2) | (((substances[0] >= idx) ? 1 : 0) << 1) | ((substances[1] >= idx) ? 1 : 0);
							if (num10 > 0)
							{
								GroundMasks.UVData[] variationUVs = biomeMaskData.tiles[num10].variationUVs;
								float staticRandom = GetStaticRandom(j, i);
								int num11 = Mathf.Min(variationUVs.Length - 1, (int)((float)variationUVs.Length * staticRandom));
								GroundMasks.UVData arg = variationUVs[num11 % variationUVs.Length];
								ElementChunk elementChunk = GetElementChunk(element.id, materials);
								if (num10 == 15)
								{
									biomeMaskCheck.opaqueAction(elementChunk, j, i, arg);
								}
								else
								{
									biomeMaskCheck.alphaAction(elementChunk, j, i, arg);
								}
							}
						}
					}
				}
			}
			foreach (ElementChunk elementChunk4 in elementChunks)
			{
				elementChunk4.Build();
			}
			for (int num12 = elementChunks.Count - 1; num12 >= 0; num12--)
			{
				ElementChunk elementChunk2 = elementChunks[num12];
				if (elementChunk2.tileCount == 0)
				{
					int index = elementChunks.Count - 1;
					elementChunks[num12] = elementChunks[index];
					elementChunks.RemoveAt(index);
				}
			}
		}

		private ElementChunk GetElementChunk(SimHashes elementID, Dictionary<SimHashes, Materials> materials)
		{
			ElementChunk elementChunk = null;
			for (int i = 0; i < elementChunks.Count; i++)
			{
				if (elementChunks[i].element == elementID)
				{
					elementChunk = elementChunks[i];
					break;
				}
			}
			if (elementChunk == null)
			{
				elementChunk = new ElementChunk(elementID, materials);
				elementChunks.Add(elementChunk);
			}
			return elementChunk;
		}

		private static int GetBiomeIdx(int cell)
		{
			if (!Grid.IsValidCell(cell))
			{
				return 0;
			}
			SubWorld.ZoneType result = SubWorld.ZoneType.Sandstone;
			if (World.Instance != null && World.Instance.zoneRenderData != null)
			{
				result = World.Instance.zoneRenderData.GetSubWorldZoneType(cell);
			}
			return (int)result;
		}

		private static float GetStaticRandom(int x, int y)
		{
			return PerlinSimplexNoise.noise((float)x * NoiseScale.x, (float)y * NoiseScale.y);
		}

		public void Render(int layer)
		{
			for (int i = 0; i < elementChunks.Count; i++)
			{
				ElementChunk elementChunk = elementChunks[i];
				elementChunk.Render(layer, ElementLoader.FindElementByHash(elementChunk.element).substance.idx);
			}
		}

		public void FreeResources()
		{
			foreach (ElementChunk elementChunk in elementChunks)
			{
				elementChunk.FreeResources();
			}
			elementChunks.Clear();
			elementChunks = null;
		}
	}

	[SerializeField]
	private GroundMasks masks;

	private GroundMasks.BiomeMaskData[] biomeMasks;

	private Dictionary<SimHashes, Materials> elementMaterials = new Dictionary<SimHashes, Materials>();

	private bool[,] dirtyChunks;

	private WorldChunk[,] worldChunks;

	private const int ChunkEdgeSize = 16;

	private Vector2I size;

	private static int SHINE_COLOR = Shader.PropertyToID("_ShineColour");

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		ShaderReloader.Register(OnShadersReloaded);
		OnShadersReloaded();
		masks.Initialize();
		SubWorld.ZoneType[] array = (SubWorld.ZoneType[])Enum.GetValues(typeof(SubWorld.ZoneType));
		biomeMasks = new GroundMasks.BiomeMaskData[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			SubWorld.ZoneType zone_type = array[i];
			biomeMasks[i] = GetBiomeMask(zone_type);
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		size = new Vector2I((Grid.WidthInCells + 16 - 1) / 16, (Grid.HeightInCells + 16 - 1) / 16);
		dirtyChunks = new bool[size.x, size.y];
		worldChunks = new WorldChunk[size.x, size.y];
		for (int i = 0; i < size.y; i++)
		{
			for (int j = 0; j < size.x; j++)
			{
				worldChunks[j, i] = new WorldChunk(j, i);
				dirtyChunks[j, i] = true;
			}
		}
	}

	public void Render(Vector2I vis_min, Vector2I vis_max, bool forceVisibleRebuild = false)
	{
		if (!base.enabled)
		{
			return;
		}
		int layer = LayerMask.NameToLayer("World");
		Vector2I vector2I = new Vector2I(vis_min.x / 16, vis_min.y / 16);
		Vector2I vector2I2 = new Vector2I((vis_max.x + 16 - 1) / 16, (vis_max.y + 16 - 1) / 16);
		for (int i = vector2I.y; i < vector2I2.y; i++)
		{
			for (int j = vector2I.x; j < vector2I2.x; j++)
			{
				WorldChunk worldChunk = worldChunks[j, i];
				if (dirtyChunks[j, i] || forceVisibleRebuild)
				{
					dirtyChunks[j, i] = false;
					worldChunk.Rebuild(biomeMasks, elementMaterials);
				}
				worldChunk.Render(layer);
			}
		}
		RebuildDirtyChunks();
	}

	public void RenderAll()
	{
		Render(new Vector2I(0, 0), new Vector2I(worldChunks.GetLength(0) * 16, worldChunks.GetLength(1) * 16), forceVisibleRebuild: true);
	}

	private void RebuildDirtyChunks()
	{
		for (int i = 0; i < dirtyChunks.GetLength(1); i++)
		{
			for (int j = 0; j < dirtyChunks.GetLength(0); j++)
			{
				if (dirtyChunks[j, i])
				{
					dirtyChunks[j, i] = false;
					worldChunks[j, i].Rebuild(biomeMasks, elementMaterials);
				}
			}
		}
	}

	public void MarkDirty(int cell)
	{
		Vector2I vector2I = Grid.CellToXY(cell);
		Vector2I vector2I2 = new Vector2I(vector2I.x / 16, vector2I.y / 16);
		dirtyChunks[vector2I2.x, vector2I2.y] = true;
		bool flag = vector2I.x % 16 == 0 && vector2I2.x > 0;
		bool flag2 = vector2I.x % 16 == 15 && vector2I2.x < size.x - 1;
		bool flag3 = vector2I.y % 16 == 0 && vector2I2.y > 0;
		bool flag4 = vector2I.y % 16 == 15 && vector2I2.y < size.y - 1;
		if (flag)
		{
			dirtyChunks[vector2I2.x - 1, vector2I2.y] = true;
			if (flag3)
			{
				dirtyChunks[vector2I2.x - 1, vector2I2.y - 1] = true;
			}
			if (flag4)
			{
				dirtyChunks[vector2I2.x - 1, vector2I2.y + 1] = true;
			}
		}
		if (flag3)
		{
			dirtyChunks[vector2I2.x, vector2I2.y - 1] = true;
		}
		if (flag4)
		{
			dirtyChunks[vector2I2.x, vector2I2.y + 1] = true;
		}
		if (flag2)
		{
			dirtyChunks[vector2I2.x + 1, vector2I2.y] = true;
			if (flag3)
			{
				dirtyChunks[vector2I2.x + 1, vector2I2.y - 1] = true;
			}
			if (flag4)
			{
				dirtyChunks[vector2I2.x + 1, vector2I2.y + 1] = true;
			}
		}
	}

	private Vector2I GetChunkIdx(int cell)
	{
		Vector2I vector2I = Grid.CellToXY(cell);
		return new Vector2I(vector2I.x / 16, vector2I.y / 16);
	}

	private GroundMasks.BiomeMaskData GetBiomeMask(SubWorld.ZoneType zone_type)
	{
		GroundMasks.BiomeMaskData value = null;
		string key = zone_type.ToString().ToLower();
		masks.biomeMasks.TryGetValue(key, out value);
		return value;
	}

	private void InitOpaqueMaterial(Material material, Element element)
	{
		material.name = element.id.ToString() + "_opaque";
		material.renderQueue = RenderQueues.WorldOpaque;
		material.EnableKeyword("OPAQUE");
		material.DisableKeyword("ALPHA");
		ConfigureMaterialShine(material);
		material.SetTexture("_AlphaTestMap", Texture2D.whiteTexture);
		material.SetInt("_IsBackwallEdge", 0);
		material.SetInt("_SrcAlpha", 1);
		material.SetInt("_DstAlpha", 0);
		material.SetInt("_ZWrite", 1);
	}

	private void InitAlphaMaterial(Material material, Element element)
	{
		material.name = element.id.ToString() + "_alpha";
		material.renderQueue = RenderQueues.WorldTransparent;
		material.EnableKeyword("ALPHA");
		material.DisableKeyword("OPAQUE");
		ConfigureMaterialShine(material);
		material.SetTexture("_AlphaTestMap", masks.maskAtlas.texture);
		material.SetInt("_IsBackwallEdge", 0);
		material.SetInt("_SrcAlpha", 5);
		material.SetInt("_DstAlpha", 10);
		material.SetInt("_ZWrite", 0);
	}

	private void InitBackwallMaterial(Material material, Element element)
	{
		material.name = element.id.ToString() + "_backwall";
		material.renderQueue = RenderQueues.NaturalBackwall;
		material.EnableKeyword("OPAQUE");
		material.DisableKeyword("ALPHA");
		ConfigureMaterialShine(material);
		material.SetTexture("_AlphaTestMap", Texture2D.grayTexture);
		material.SetInt("_IsBackwallEdge", 0);
		material.SetInt("_StencilPass", 0);
		material.SetInt("_SrcAlpha", 5);
		material.SetInt("_DstAlpha", 10);
		material.SetInt("_ZWrite", 0);
	}

	private void InitAlphaBackwallMaterial(Material material, Element element)
	{
		material.name = element.id.ToString() + "_backwall_alpha";
		material.renderQueue = RenderQueues.BackwallTransparent;
		material.EnableKeyword("ALPHA");
		material.DisableKeyword("OPAQUE");
		ConfigureMaterialShine(material);
		material.SetTexture("_AlphaTestMap", masks.maskAtlas.texture);
		material.SetInt("_IsBackwallEdge", 1);
		material.SetInt("_StencilPass", 0);
		material.SetInt("_SrcAlpha", 5);
		material.SetInt("_DstAlpha", 10);
		material.SetInt("_ZWrite", 0);
	}

	private void ConfigureMaterialShine(Material material)
	{
		Texture texture = material.GetTexture("_ShineMask");
		if (texture != null)
		{
			material.DisableKeyword("MATTE");
			material.EnableKeyword("SHINY");
		}
		else
		{
			material.EnableKeyword("MATTE");
			material.DisableKeyword("SHINY");
		}
	}

	[ContextMenu("Reload Shaders")]
	public void OnShadersReloaded()
	{
		FreeMaterials();
		foreach (Element element in ElementLoader.elements)
		{
			if (element.IsSolid)
			{
				if (element.substance.material == null)
				{
					DebugUtil.LogErrorArgs(element.name, "must have material associated with it in the substance table");
				}
				Material material = new Material(element.substance.material);
				InitOpaqueMaterial(material, element);
				Material material2 = new Material(material);
				InitAlphaMaterial(material2, element);
				Material material3 = new Material(material);
				InitBackwallMaterial(material3, element);
				Material material4 = new Material(material);
				InitAlphaBackwallMaterial(material4, element);
				Materials value = new Materials(material, material2, material3, material4);
				elementMaterials[element.id] = value;
			}
		}
		if (worldChunks == null)
		{
			return;
		}
		for (int i = 0; i < dirtyChunks.GetLength(1); i++)
		{
			for (int j = 0; j < dirtyChunks.GetLength(0); j++)
			{
				dirtyChunks[j, i] = true;
			}
		}
		WorldChunk[,] array = worldChunks;
		int upperBound = array.GetUpperBound(0);
		int upperBound2 = array.GetUpperBound(1);
		for (int k = array.GetLowerBound(0); k <= upperBound; k++)
		{
			for (int l = array.GetLowerBound(1); l <= upperBound2; l++)
			{
				WorldChunk worldChunk = array[k, l];
				worldChunk.Clear();
				worldChunk.Rebuild(biomeMasks, elementMaterials);
			}
		}
	}

	public void SetShineColors(SimHashes element, Color centerColor, Color edgeColor)
	{
		if (elementMaterials.TryGetValue(element, out var value))
		{
			value.alpha.SetColor(SHINE_COLOR, edgeColor);
			value.opaque.SetColor(SHINE_COLOR, centerColor);
		}
	}

	public void FreeResources()
	{
		FreeMaterials();
		elementMaterials.Clear();
		if (worldChunks == null)
		{
			return;
		}
		WorldChunk[,] array = worldChunks;
		int upperBound = array.GetUpperBound(0);
		int upperBound2 = array.GetUpperBound(1);
		for (int i = array.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = array.GetLowerBound(1); j <= upperBound2; j++)
			{
				WorldChunk worldChunk = array[i, j];
				worldChunk.FreeResources();
			}
		}
		worldChunks = null;
	}

	private void FreeMaterials()
	{
		foreach (Materials value in elementMaterials.Values)
		{
			UnityEngine.Object.Destroy(value.opaque);
			UnityEngine.Object.Destroy(value.alpha);
			UnityEngine.Object.Destroy(value.backwall);
			UnityEngine.Object.Destroy(value.backwallAlpha);
		}
		elementMaterials.Clear();
	}
}
