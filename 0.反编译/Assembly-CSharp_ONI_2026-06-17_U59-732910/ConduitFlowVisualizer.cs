using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class ConduitFlowVisualizer
{
	[Serializable]
	public class Tuning
	{
		public bool renderMesh;

		public float size;

		public float spriteCount;

		public float framesPerSecond;

		public Texture2D backgroundTexture;

		public Texture2D foregroundTexture;
	}

	private class ConduitFlowMesh
	{
		private Mesh mesh;

		private Material material;

		private List<Vector3> positions = new List<Vector3>();

		private List<Vector4> uvs = new List<Vector4>();

		private List<int> triangles = new List<int>();

		private List<Color32> colors = new List<Color32>();

		private int quadIndex;

		public ConduitFlowMesh()
		{
			mesh = new Mesh();
			mesh.name = "ConduitMesh";
			material = new Material(Shader.Find("Klei/ConduitBall"));
		}

		public void AddQuad(Vector2 pos, Color32 color, float size, float is_foreground, float highlight, Vector2I uvbl, Vector2I uvtl, Vector2I uvbr, Vector2I uvtr)
		{
			float num = size * 0.5f;
			positions.Add(new Vector3(pos.x - num, pos.y - num, 0f));
			positions.Add(new Vector3(pos.x - num, pos.y + num, 0f));
			positions.Add(new Vector3(pos.x + num, pos.y - num, 0f));
			positions.Add(new Vector3(pos.x + num, pos.y + num, 0f));
			uvs.Add(new Vector4(uvbl.x, uvbl.y, is_foreground, highlight));
			uvs.Add(new Vector4(uvtl.x, uvtl.y, is_foreground, highlight));
			uvs.Add(new Vector4(uvbr.x, uvbr.y, is_foreground, highlight));
			uvs.Add(new Vector4(uvtr.x, uvtr.y, is_foreground, highlight));
			colors.Add(color);
			colors.Add(color);
			colors.Add(color);
			colors.Add(color);
			triangles.Add(quadIndex * 4);
			triangles.Add(quadIndex * 4 + 1);
			triangles.Add(quadIndex * 4 + 2);
			triangles.Add(quadIndex * 4 + 2);
			triangles.Add(quadIndex * 4 + 1);
			triangles.Add(quadIndex * 4 + 3);
			quadIndex++;
		}

		public void SetTexture(string id, Texture2D texture)
		{
			material.SetTexture(id, texture);
		}

		public void SetVector(string id, Vector4 data)
		{
			material.SetVector(id, data);
		}

		public void Begin()
		{
			positions.Clear();
			uvs.Clear();
			triangles.Clear();
			colors.Clear();
			quadIndex = 0;
		}

		public void End(float z, int layer)
		{
			mesh.Clear();
			mesh.SetVertices(positions);
			mesh.SetUVs(0, uvs);
			mesh.SetColors(colors);
			mesh.SetTriangles(triangles, 0, calculateBounds: false);
			Graphics.DrawMesh(mesh, new Vector3(GRID_OFFSET.x, GRID_OFFSET.y, z - 0.1f), Quaternion.identity, material, layer);
		}

		public void Cleanup()
		{
			UnityEngine.Object.Destroy(mesh);
			mesh = null;
			UnityEngine.Object.Destroy(material);
			material = null;
		}
	}

	private struct AudioInfo
	{
		public int networkID;

		public int blobCount;

		public float distance;

		public Vector3 position;
	}

	private struct RenderMeshContext
	{
		public static RenderMeshContext EmptyContext;

		public ListPool<int, ConduitFlowVisualizer>.PooledList visible_conduits;

		public ConduitFlowVisualizer outer;

		public float lerp_percent;

		public RenderMeshContext(ConduitFlowVisualizer outer, float lerp_percent, Vector2I min, Vector2I max)
		{
			this.outer = outer;
			this.lerp_percent = lerp_percent;
			visible_conduits = ListPool<int, ConduitFlowVisualizer>.Allocate();
			visible_conduits.Capacity = Math.Max(outer.flowManager.soaInfo.NumEntries, visible_conduits.Capacity);
			for (int i = 0; i != outer.flowManager.soaInfo.NumEntries; i++)
			{
				Vector2I vector2I = Grid.CellToXY(outer.flowManager.soaInfo.GetCell(i));
				if (min <= vector2I && vector2I <= max)
				{
					visible_conduits.Add(i);
				}
			}
		}

		public void Finish()
		{
			visible_conduits.Recycle();
		}
	}

	private class RenderMeshPerThreadData
	{
		public struct Ball
		{
			private class UVPack
			{
				public Vector2I bl;

				public Vector2I tl;

				public Vector2I br;

				public Vector2I tr;
			}

			private Vector2 pos;

			private float size;

			private Color32 color;

			private ConduitFlow.FlowDirections direction;

			private bool foreground;

			private bool highlight;

			private static Dictionary<ConduitFlow.FlowDirections, UVPack> uv_packs = new Dictionary<ConduitFlow.FlowDirections, UVPack>();

			public Ball(ConduitFlow.FlowDirections direction, Vector2 pos, Color32 color, float size, bool foreground, bool highlight)
			{
				this.pos = pos;
				this.size = size;
				this.color = color;
				this.direction = direction;
				this.foreground = foreground;
				this.highlight = highlight;
			}

			public static void InitializeResources()
			{
				uv_packs[ConduitFlow.FlowDirections.None] = new UVPack
				{
					bl = new Vector2I(0, 0),
					tl = new Vector2I(0, 1),
					br = new Vector2I(1, 0),
					tr = new Vector2I(1, 1)
				};
				uv_packs[ConduitFlow.FlowDirections.Left] = new UVPack
				{
					bl = new Vector2I(0, 0),
					tl = new Vector2I(0, 1),
					br = new Vector2I(1, 0),
					tr = new Vector2I(1, 1)
				};
				uv_packs[ConduitFlow.FlowDirections.Right] = uv_packs[ConduitFlow.FlowDirections.Left];
				uv_packs[ConduitFlow.FlowDirections.Up] = new UVPack
				{
					bl = new Vector2I(1, 0),
					tl = new Vector2I(0, 0),
					br = new Vector2I(1, 1),
					tr = new Vector2I(0, 1)
				};
				uv_packs[ConduitFlow.FlowDirections.Down] = uv_packs[ConduitFlow.FlowDirections.Up];
			}

			private static UVPack GetUVPack(ConduitFlow.FlowDirections direction)
			{
				return uv_packs[direction];
			}

			public void Consume(ConduitFlowMesh mesh)
			{
				UVPack uVPack = GetUVPack(direction);
				mesh.AddQuad(pos, color, size, foreground ? 1 : 0, highlight ? 1 : 0, uVPack.bl, uVPack.tl, uVPack.br, uVPack.tr);
			}
		}

		public List<Ball> moving_balls = new List<Ball>();

		public List<Ball> static_balls = new List<Ball>();

		public List<ConduitFlow.Conduit> moving_conduits = new List<ConduitFlow.Conduit>();

		public void Finish(ConduitFlowMesh moving_ball_mesh, ConduitFlowMesh static_ball_mesh, Vector3 camera_pos, ConduitFlowVisualizer visualizer)
		{
			for (int i = 0; i != moving_balls.Count; i++)
			{
				moving_balls[i].Consume(moving_ball_mesh);
			}
			moving_balls.Clear();
			for (int j = 0; j != static_balls.Count; j++)
			{
				static_balls[j].Consume(static_ball_mesh);
			}
			static_balls.Clear();
			if (visualizer != null)
			{
				foreach (ConduitFlow.Conduit moving_conduit in moving_conduits)
				{
					visualizer.AddAudioSource(moving_conduit, camera_pos);
				}
			}
			moving_conduits.Clear();
		}
	}

	private class RenderMeshBatchJob : WorkItemCollectionWithThreadContex<RenderMeshContext, RenderMeshPerThreadData>
	{
		private const int kBatchSize = 32;

		private static RenderMeshBatchJob instance;

		public static RenderMeshBatchJob Instance
		{
			get
			{
				if (instance == null || instance.threadContexts.Count != GlobalJobManager.ThreadCount)
				{
					instance = new RenderMeshBatchJob();
				}
				return instance;
			}
		}

		public RenderMeshBatchJob()
		{
			threadContexts = new List<RenderMeshPerThreadData>();
			for (int i = 0; i < GlobalJobManager.ThreadCount; i++)
			{
				threadContexts.Add(new RenderMeshPerThreadData());
			}
		}

		public void Reset(RenderMeshContext context)
		{
			sharedData = context;
			if (context.visible_conduits == null)
			{
				count = 0;
			}
			else
			{
				count = (context.visible_conduits.Count + 32 - 1) / 32;
			}
		}

		public override void RunItem(int item, ref RenderMeshContext shared_data, RenderMeshPerThreadData thread_context, int threadIndex)
		{
			Element element = null;
			int num = item * 32;
			int num2 = Math.Min(shared_data.visible_conduits.Count, num + 32);
			for (int i = num; i < num2; i++)
			{
				ConduitFlow.Conduit conduit = shared_data.outer.flowManager.soaInfo.GetConduit(shared_data.visible_conduits[i]);
				ConduitFlow.ConduitFlowInfo lastFlowInfo = conduit.GetLastFlowInfo(shared_data.outer.flowManager);
				ConduitFlow.ConduitContents initialContents = conduit.GetInitialContents(shared_data.outer.flowManager);
				if (lastFlowInfo.contents.mass > 0f)
				{
					int cell = conduit.GetCell(shared_data.outer.flowManager);
					int cellFromDirection = ConduitFlow.GetCellFromDirection(cell, lastFlowInfo.direction);
					Vector2I vector2I = Grid.CellToXY(cell);
					Vector2I vector2I2 = Grid.CellToXY(cellFromDirection);
					Vector2 pos = ((cell == -1) ? ((Vector2)vector2I) : Vector2.Lerp(new Vector2(vector2I.x, vector2I.y), new Vector2(vector2I2.x, vector2I2.y), shared_data.lerp_percent));
					Color32 cellTintColour = shared_data.outer.GetCellTintColour(cell);
					Color32 cellTintColour2 = shared_data.outer.GetCellTintColour(cellFromDirection);
					Color32 color = Color32.Lerp(cellTintColour, cellTintColour2, shared_data.lerp_percent);
					bool highlight = false;
					if (shared_data.outer.showContents)
					{
						if (lastFlowInfo.contents.mass >= initialContents.mass)
						{
							thread_context.moving_balls.Add(new RenderMeshPerThreadData.Ball(lastFlowInfo.direction, pos, color, shared_data.outer.tuning.size, foreground: false, highlight: false));
						}
						if (element == null || lastFlowInfo.contents.element != element.id)
						{
							element = ElementLoader.FindElementByHash(lastFlowInfo.contents.element);
						}
					}
					else
					{
						element = null;
						highlight = Grid.PosToCell(new Vector3(pos.x + GRID_OFFSET.x, pos.y + GRID_OFFSET.y, 0f)) == shared_data.outer.highlightedCell;
					}
					Color32 contentsColor = shared_data.outer.GetContentsColor(element, color);
					float num3 = 1f;
					if (shared_data.outer.showContents || lastFlowInfo.contents.mass < initialContents.mass)
					{
						num3 = shared_data.outer.CalculateMassScale(lastFlowInfo.contents.mass);
					}
					thread_context.moving_balls.Add(new RenderMeshPerThreadData.Ball(lastFlowInfo.direction, pos, contentsColor, shared_data.outer.tuning.size * num3, foreground: true, highlight));
					thread_context.moving_conduits.Add(conduit);
				}
				if (!(initialContents.mass > lastFlowInfo.contents.mass) || !(initialContents.mass > 0f))
				{
					continue;
				}
				int cell2 = conduit.GetCell(shared_data.outer.flowManager);
				Vector2 pos2 = Grid.CellToXY(cell2);
				float mass = initialContents.mass - lastFlowInfo.contents.mass;
				bool highlight2 = false;
				Color32 cellTintColour3 = shared_data.outer.GetCellTintColour(cell2);
				float num4 = shared_data.outer.CalculateMassScale(mass);
				if (shared_data.outer.showContents)
				{
					thread_context.static_balls.Add(new RenderMeshPerThreadData.Ball(ConduitFlow.FlowDirections.None, pos2, cellTintColour3, shared_data.outer.tuning.size * num4, foreground: false, highlight: false));
					if (element == null || initialContents.element != element.id)
					{
						element = ElementLoader.FindElementByHash(initialContents.element);
					}
				}
				else
				{
					element = null;
					highlight2 = cell2 == shared_data.outer.highlightedCell;
				}
				Color32 contentsColor2 = shared_data.outer.GetContentsColor(element, cellTintColour3);
				thread_context.static_balls.Add(new RenderMeshPerThreadData.Ball(ConduitFlow.FlowDirections.None, pos2, contentsColor2, shared_data.outer.tuning.size * num4, foreground: true, highlight2));
			}
		}

		public void Finish(ConduitFlowMesh moving_ball_mesh, ConduitFlowMesh static_ball_mesh, Vector3 camera_pos, ConduitFlowVisualizer visualizer)
		{
			foreach (RenderMeshPerThreadData threadContext in threadContexts)
			{
				threadContext.Finish(moving_ball_mesh, static_ball_mesh, camera_pos, visualizer);
			}
			sharedData.Finish();
		}
	}

	private ConduitFlow flowManager;

	private EventReference overlaySound;

	private bool showContents;

	private double animTime;

	private int layer;

	private static Vector2 GRID_OFFSET = new Vector2(0.5f, 0.5f);

	private List<AudioInfo> audioInfo;

	private HashSet<int> insulatedCells = new HashSet<int>();

	private HashSet<int> radiantCells = new HashSet<int>();

	private Game.ConduitVisInfo visInfo;

	private ConduitFlowMesh movingBallMesh;

	private ConduitFlowMesh staticBallMesh;

	private int highlightedCell = -1;

	private Color32 highlightColour = new Color(0.2f, 0.2f, 0.2f, 0.2f);

	private Tuning tuning;

	public ConduitFlowVisualizer(ConduitFlow flow_manager, Game.ConduitVisInfo vis_info, EventReference overlay_sound, Tuning tuning)
	{
		flowManager = flow_manager;
		visInfo = vis_info;
		overlaySound = overlay_sound;
		this.tuning = tuning;
		movingBallMesh = new ConduitFlowMesh();
		staticBallMesh = new ConduitFlowMesh();
		RenderMeshPerThreadData.Ball.InitializeResources();
	}

	public void FreeResources()
	{
		movingBallMesh.Cleanup();
		staticBallMesh.Cleanup();
	}

	private float CalculateMassScale(float mass)
	{
		float t = (mass - visInfo.overlayMassScaleRange.x) / (visInfo.overlayMassScaleRange.y - visInfo.overlayMassScaleRange.x);
		return Mathf.Lerp(visInfo.overlayMassScaleValues.x, visInfo.overlayMassScaleValues.y, t);
	}

	private Color32 GetContentsColor(Element element, Color32 default_color)
	{
		if (element != null)
		{
			Color color = (element.IsMoltenMetal ? Color.Lerp((Color)element.substance.conduitColour, WaterCubes.MOLTEN_METAL_COLOR, 0.8f) : ((Color)element.substance.conduitColour));
			color.a = (element.substance.Glows ? 1f : 0.5f);
			return color;
		}
		return default_color;
	}

	private Color32 GetTintColour()
	{
		if (!showContents)
		{
			return visInfo.tint;
		}
		return GlobalAssets.Instance.colorSet.GetColorByName(visInfo.overlayTintName);
	}

	private Color32 GetInsulatedTintColour()
	{
		if (!showContents)
		{
			return visInfo.insulatedTint;
		}
		return GlobalAssets.Instance.colorSet.GetColorByName(visInfo.overlayInsulatedTintName);
	}

	private Color32 GetRadiantTintColour()
	{
		if (!showContents)
		{
			return visInfo.radiantTint;
		}
		return GlobalAssets.Instance.colorSet.GetColorByName(visInfo.overlayRadiantTintName);
	}

	private Color32 GetCellTintColour(int cell)
	{
		if (insulatedCells.Contains(cell))
		{
			return GetInsulatedTintColour();
		}
		if (radiantCells.Contains(cell))
		{
			return GetRadiantTintColour();
		}
		return GetTintColour();
	}

	public void Render(float z, int render_layer, float lerp_percent, bool trigger_audio = false)
	{
		animTime += Time.deltaTime;
		if (trigger_audio)
		{
			if (audioInfo == null)
			{
				audioInfo = new List<AudioInfo>();
			}
			for (int i = 0; i < audioInfo.Count; i++)
			{
				AudioInfo value = audioInfo[i];
				value.distance = float.PositiveInfinity;
				value.position = Vector3.zero;
				value.blobCount = (value.blobCount + 1) % 10;
				audioInfo[i] = value;
			}
		}
		if (tuning.renderMesh)
		{
			RenderMesh(z, render_layer, lerp_percent, trigger_audio);
		}
		if (trigger_audio)
		{
			TriggerAudio();
		}
	}

	private void RenderMesh(float z, int render_layer, float lerp_percent, bool trigger_audio)
	{
		GridArea visibleArea = GridVisibleArea.GetVisibleArea();
		Vector2I min = new Vector2I(Mathf.Max(0, visibleArea.Min.x - 1), Mathf.Max(0, visibleArea.Min.y - 1));
		Vector2I max = new Vector2I(Mathf.Min(Grid.WidthInCells - 1, visibleArea.Max.x + 1), Mathf.Min(Grid.HeightInCells - 1, visibleArea.Max.y + 1));
		RenderMeshContext context = new RenderMeshContext(this, lerp_percent, min, max);
		if (context.visible_conduits.Count == 0)
		{
			context.Finish();
			return;
		}
		RenderMeshBatchJob.Instance.Reset(context);
		GlobalJobManager.Run(RenderMeshBatchJob.Instance);
		float z2 = 0f;
		if (showContents)
		{
			z2 = 1f;
		}
		float w = (float)((int)(animTime / (1.0 / (double)tuning.framesPerSecond)) % (int)tuning.spriteCount) * (1f / tuning.spriteCount);
		movingBallMesh.Begin();
		movingBallMesh.SetTexture("_BackgroundTex", tuning.backgroundTexture);
		movingBallMesh.SetTexture("_ForegroundTex", tuning.foregroundTexture);
		movingBallMesh.SetVector("_SpriteSettings", new Vector4(1f / tuning.spriteCount, 1f, z2, w));
		movingBallMesh.SetVector("_Highlight", new Vector4((float)(int)highlightColour.r / 255f, (float)(int)highlightColour.g / 255f, (float)(int)highlightColour.b / 255f, 0f));
		staticBallMesh.Begin();
		staticBallMesh.SetTexture("_BackgroundTex", tuning.backgroundTexture);
		staticBallMesh.SetTexture("_ForegroundTex", tuning.foregroundTexture);
		staticBallMesh.SetVector("_SpriteSettings", new Vector4(1f / tuning.spriteCount, 1f, z2, 0f));
		staticBallMesh.SetVector("_Highlight", new Vector4((float)(int)highlightColour.r / 255f, (float)(int)highlightColour.g / 255f, (float)(int)highlightColour.b / 255f, 0f));
		Vector3 position = CameraController.Instance.transform.GetPosition();
		ConduitFlowVisualizer visualizer = (trigger_audio ? this : null);
		RenderMeshBatchJob.Instance.Finish(movingBallMesh, staticBallMesh, position, visualizer);
		movingBallMesh.End(z, layer);
		staticBallMesh.End(z, layer);
		RenderMeshBatchJob.Instance.Reset(RenderMeshContext.EmptyContext);
	}

	public void ColourizePipeContents(bool show_contents, bool move_to_overlay_layer)
	{
		showContents = show_contents;
		layer = ((show_contents && move_to_overlay_layer) ? LayerMask.NameToLayer("MaskedOverlay") : 0);
	}

	private void AddAudioSource(ConduitFlow.Conduit conduit, Vector3 camera_pos)
	{
		UtilityNetwork network = flowManager.GetNetwork(conduit);
		if (network == null)
		{
			return;
		}
		Vector3 vector = Grid.CellToPosCCC(conduit.GetCell(flowManager), Grid.SceneLayer.Building);
		float num = Vector3.SqrMagnitude(vector - camera_pos);
		bool flag = false;
		for (int i = 0; i < audioInfo.Count; i++)
		{
			AudioInfo value = audioInfo[i];
			if (value.networkID == network.id)
			{
				if (num < value.distance)
				{
					value.distance = num;
					value.position = vector;
					audioInfo[i] = value;
				}
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			AudioInfo item = new AudioInfo
			{
				networkID = network.id,
				position = vector,
				distance = num,
				blobCount = 0
			};
			audioInfo.Add(item);
		}
	}

	private void TriggerAudio()
	{
		if (SpeedControlScreen.Instance.IsPaused)
		{
			return;
		}
		CameraController instance = CameraController.Instance;
		int num = 0;
		List<AudioInfo> list = new List<AudioInfo>();
		for (int i = 0; i < this.audioInfo.Count; i++)
		{
			if (instance.IsVisiblePos(this.audioInfo[i].position))
			{
				list.Add(this.audioInfo[i]);
				num++;
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			AudioInfo audioInfo = list[j];
			if (audioInfo.distance != float.PositiveInfinity)
			{
				Vector3 position = audioInfo.position;
				position.z = 0f;
				EventInstance instance2 = SoundEvent.BeginOneShot(overlaySound, position);
				instance2.setParameterByName("blobCount", audioInfo.blobCount);
				instance2.setParameterByName("networkCount", num);
				SoundEvent.EndOneShot(instance2);
			}
		}
	}

	public void AddThermalConductivity(int cell, float conductivity)
	{
		if (conductivity < 1f)
		{
			insulatedCells.Add(cell);
		}
		else if (conductivity > 1f)
		{
			radiantCells.Add(cell);
		}
	}

	public void RemoveThermalConductivity(int cell, float conductivity)
	{
		if (conductivity < 1f)
		{
			insulatedCells.Remove(cell);
		}
		else if (conductivity > 1f)
		{
			radiantCells.Remove(cell);
		}
	}

	public void SetHighlightedCell(int cell)
	{
		highlightedCell = cell;
	}
}
