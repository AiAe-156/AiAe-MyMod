using Delaunay.Geo;
using Klei;
using ProcGen;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/SubworldZoneRenderData")]
public class SubworldZoneRenderData : KMonoBehaviour
{
	[SerializeField]
	private Texture2D colourTex = null;

	[SerializeField]
	private Texture2D indexTex = null;

	[HideInInspector]
	public SubWorld.ZoneType[] worldZoneTypes;

	[SerializeField]
	[HideInInspector]
	public Color32[] zoneColours = new Color32[19]
	{
		new Color32(145, 198, 213, 0),
		new Color32(135, 82, 160, 1),
		new Color32(123, 151, 75, 2),
		new Color32(236, 189, 89, 3),
		new Color32(201, 152, 181, 4),
		new Color32(222, 90, 59, 5),
		new Color32(201, 152, 181, 6),
		new Color32(byte.MaxValue, 0, 0, 7),
		new Color32(201, 201, 151, 8),
		new Color32(236, 90, 110, 9),
		new Color32(110, 236, 110, 10),
		new Color32(145, 198, 213, 11),
		new Color32(145, 198, 213, 12),
		new Color32(145, 198, 213, 13),
		new Color32(173, 222, 212, 14),
		new Color32(100, 100, 222, 18),
		new Color32(222, 100, 222, 19),
		new Color32(100, 222, 100, 20),
		new Color32(192, 100, 16, 27)
	};

	private const int NUM_COLOUR_BYTES = 3;

	private int[] zoneTextureArrayIndices = new int[28]
	{
		0, 1, 2, 3, 4, 5, 5, 3, 6, 7,
		8, 9, 10, 11, 12, 7, 3, 13, 0, 0,
		0, 14, 15, 16, 4, 6, 18, 17
	};

	public static Vector4[] zoneTextureParallaxData = new Vector4[28]
	{
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero,
		Vector4.zero
	};

	protected override void OnSpawn()
	{
		base.OnSpawn();
		ShaderReloader.Register(OnShadersReloaded);
		GenerateTexture();
		OnActiveWorldChanged();
		Game.Instance.Subscribe(1983128072, delegate
		{
			OnActiveWorldChanged();
		});
	}

	public void OnActiveWorldChanged()
	{
		byte[] rawTextureData = colourTex.GetRawTextureData();
		byte[] rawTextureData2 = indexTex.GetRawTextureData();
		WorldDetailSave clusterDetailSave = SaveLoader.Instance.clusterDetailSave;
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < clusterDetailSave.overworldCells.Count; i++)
		{
			WorldDetailSave.OverworldCell overworldCell = clusterDetailSave.overworldCells[i];
			Polygon poly = overworldCell.poly;
			zero.y = (int)Mathf.Floor(poly.bounds.yMin);
			while (zero.y < Mathf.Ceil(poly.bounds.yMax))
			{
				zero.x = (int)Mathf.Floor(poly.bounds.xMin);
				while (zero.x < Mathf.Ceil(poly.bounds.xMax))
				{
					if (poly.Contains(zero))
					{
						int num = Grid.XYToCell((int)zero.x, (int)zero.y);
						if (Grid.IsValidCell(num))
						{
							if (Grid.IsActiveWorld(num))
							{
								rawTextureData2[num * 3] = ((overworldCell.zoneType == SubWorld.ZoneType.Space) ? byte.MaxValue : ((byte)zoneTextureArrayIndices[(int)overworldCell.zoneType]));
								rawTextureData2[num * 3 + 1] = overworldCell.biomeIdx;
								rawTextureData2[num * 3 + 2] = (byte)overworldCell.zoneType;
								Color32 color = zoneColours[(int)overworldCell.zoneType];
								rawTextureData[num * 3] = color.r;
								rawTextureData[num * 3 + 1] = color.g;
								rawTextureData[num * 3 + 2] = color.b;
							}
							else
							{
								rawTextureData2[num * 3] = byte.MaxValue;
								rawTextureData2[num * 3 + 1] = byte.MaxValue;
								rawTextureData2[num * 3 + 2] = byte.MaxValue;
								Color32 color2 = zoneColours[7];
								rawTextureData[num * 3] = color2.r;
								rawTextureData[num * 3 + 1] = color2.g;
								rawTextureData[num * 3 + 2] = color2.b;
							}
						}
					}
					zero.x += 1f;
				}
				zero.y += 1f;
			}
		}
		colourTex.LoadRawTextureData(rawTextureData);
		indexTex.LoadRawTextureData(rawTextureData2);
		colourTex.Apply();
		indexTex.Apply();
		OnShadersReloaded();
	}

	public void GenerateTexture()
	{
		byte[] array = new byte[Grid.WidthInCells * Grid.HeightInCells];
		byte[] array2 = new byte[Grid.WidthInCells * Grid.HeightInCells * 3];
		byte[] array3 = new byte[Grid.WidthInCells * Grid.HeightInCells * 3];
		worldZoneTypes = new SubWorld.ZoneType[Grid.CellCount];
		colourTex = new Texture2D(Grid.WidthInCells, Grid.HeightInCells, TextureFormat.RGB24, mipChain: false);
		colourTex.name = "SubworldRegionColourData";
		colourTex.filterMode = FilterMode.Bilinear;
		colourTex.wrapMode = TextureWrapMode.Clamp;
		colourTex.anisoLevel = 0;
		indexTex = new Texture2D(Grid.WidthInCells, Grid.HeightInCells, TextureFormat.RGB24, mipChain: false);
		indexTex.name = "SubworldRegionIndexData";
		indexTex.filterMode = FilterMode.Point;
		indexTex.wrapMode = TextureWrapMode.Clamp;
		indexTex.anisoLevel = 0;
		for (int i = 0; i < Grid.CellCount; i++)
		{
			array[i] = byte.MaxValue;
			array2[i * 3] = byte.MaxValue;
			array2[i * 3 + 1] = byte.MaxValue;
			array2[i * 3 + 2] = byte.MaxValue;
			Color32 color = zoneColours[7];
			array3[i * 3] = color.r;
			array3[i * 3 + 1] = color.g;
			array3[i * 3 + 2] = color.b;
			worldZoneTypes[i] = SubWorld.ZoneType.Space;
		}
		colourTex.LoadRawTextureData(array3);
		indexTex.LoadRawTextureData(array2);
		colourTex.Apply();
		indexTex.Apply();
		WorldDetailSave clusterDetailSave = SaveLoader.Instance.clusterDetailSave;
		Vector2 zero = Vector2.zero;
		for (int j = 0; j < clusterDetailSave.overworldCells.Count; j++)
		{
			WorldDetailSave.OverworldCell overworldCell = clusterDetailSave.overworldCells[j];
			Polygon poly = overworldCell.poly;
			zero.y = (int)Mathf.Floor(poly.bounds.yMin);
			while (zero.y < Mathf.Ceil(poly.bounds.yMax))
			{
				zero.x = (int)Mathf.Floor(poly.bounds.xMin);
				while (zero.x < Mathf.Ceil(poly.bounds.xMax))
				{
					if (poly.Contains(zero))
					{
						int num = Grid.XYToCell((int)zero.x, (int)zero.y);
						if (Grid.IsValidCell(num))
						{
							array[num] = ((overworldCell.zoneType == SubWorld.ZoneType.Space) ? byte.MaxValue : ((byte)overworldCell.zoneType));
							array2[num * 3] = ((overworldCell.zoneType == SubWorld.ZoneType.Space) ? byte.MaxValue : ((byte)overworldCell.zoneType));
							array2[num * 3 + 1] = overworldCell.biomeIdx;
							worldZoneTypes[num] = overworldCell.zoneType;
						}
					}
					zero.x += 1f;
				}
				zero.y += 1f;
			}
		}
		InitSimZones(array);
	}

	private void OnShadersReloaded()
	{
		Shader.SetGlobalTexture("_WorldZoneTex", colourTex);
		Shader.SetGlobalTexture("_WorldZoneIndexTex", indexTex);
	}

	public SubWorld.ZoneType GetSubWorldZoneType(int cell)
	{
		if (cell >= 0 && cell < worldZoneTypes.Length)
		{
			return worldZoneTypes[cell];
		}
		return SubWorld.ZoneType.Sandstone;
	}

	private unsafe void InitSimZones(byte[] bytes)
	{
		fixed (byte* msg = bytes)
		{
			Sim.SIM_HandleMessage(-457308393, bytes.Length, msg);
		}
	}
}
