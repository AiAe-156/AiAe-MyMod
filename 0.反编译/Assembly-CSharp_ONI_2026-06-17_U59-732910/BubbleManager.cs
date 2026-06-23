using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/BubbleManager")]
public class BubbleManager : KMonoBehaviour, ISim33ms, IRenderEveryTick
{
	[Serializable]
	public struct Disease
	{
		public static readonly Disease None = new Disease
		{
			Idx = byte.MaxValue,
			Count = 0
		};

		public byte Idx;

		public int Count;
	}

	[Serializable]
	private readonly struct Archetype
	{
		[Serializable]
		public struct Id
		{
			public int hashCode;
		}

		public readonly Vector2 velocity;

		public readonly SimHashes element;

		public readonly float alphaFadeSpeed;

		public Color32 Colour
		{
			get
			{
				DebugUtil.DevAssert(ElementLoader.elementTable != null, "Elements are not loaded yet");
				ushort elementIndex = ElementLoader.GetElementIndex(this.element);
				DebugUtil.DevAssert(ElementLoader.elements != null, "Elements are not loaded yet");
				Element element = ElementLoader.elements[elementIndex];
				Color color = (element.IsMoltenMetal ? WaterCubes.MOLTEN_METAL_COLOR : ((Color)element.substance.colour));
				color.a = 255f;
				return color;
			}
		}

		public Archetype(Vector2 velocity, SimHashes elementId)
		{
			this.velocity = velocity;
			element = elementId;
			alphaFadeSpeed = velocity.magnitude;
		}

		public Id GetId()
		{
			return new Id
			{
				hashCode = GetHashCode()
			};
		}
	}

	[Serializable]
	private struct WorldArchetype
	{
		public int worldIdx;

		public Archetype.Id archetype;
	}

	[SerializationConfig(MemberSerialization.OptIn)]
	private class InstanceData : IEnumerable<InstanceData.Subscript>, IEnumerable
	{
		public readonly struct Subscript
		{
			private readonly InstanceData data;

			private readonly int index;

			public int Index => index;

			public Vector2 Position
			{
				get
				{
					return data.position[index];
				}
				set
				{
					data.position[index] = value;
				}
			}

			public float ElapsedTime
			{
				get
				{
					return data.elapsedTime[index];
				}
				set
				{
					data.elapsedTime[index] = value;
				}
			}

			public int Frame
			{
				get
				{
					return data.frame[index];
				}
				set
				{
					data.frame[index] = value;
				}
			}

			public float Temperature
			{
				get
				{
					return data.temperature[index];
				}
				set
				{
					data.temperature[index] = value;
				}
			}

			public float Mass
			{
				get
				{
					return data.mass[index];
				}
				set
				{
					data.mass[index] = value;
				}
			}

			public byte SizeLevel
			{
				get
				{
					return data.sizeLevel[index];
				}
				set
				{
					data.sizeLevel[index] = value;
				}
			}

			public bool Visible => data.alpha[index] != 0f;

			public bool FadingOut
			{
				get
				{
					return data.alpha[index] != -1f;
				}
				set
				{
					DebugUtil.DevAssert(value, "Cannot set FadingOut to false. Once the fade out is begun, it cannot be stopped");
					if (value && data.alpha[index] == -1f)
					{
						data.alpha[index] = 1f;
					}
				}
			}

			public float Alpha
			{
				get
				{
					float num = data.alpha[index];
					if (num != -1f)
					{
						return num;
					}
					return 1f;
				}
				set
				{
					data.alpha[index] = value;
				}
			}

			public Disease Disease
			{
				get
				{
					return data.disease[index];
				}
				set
				{
					data.disease[index] = value;
				}
			}

			public Subscript(InstanceData data, int index)
			{
				this.data = data;
				this.index = index;
			}
		}

		public struct Enumerator : IEnumerator<Subscript>, IEnumerator, IDisposable
		{
			private int index;

			private readonly InstanceData outer;

			public readonly Subscript Current => new Subscript(outer, index);

			readonly object IEnumerator.Current => Current;

			public Enumerator(InstanceData outer)
			{
				this.outer = outer;
				index = -1;
			}

			public bool MoveNext()
			{
				index = ((index == -1) ? outer.Begin : outer.Next(index));
				return index != outer.End;
			}

			public void Reset()
			{
				index = outer.Begin;
			}

			readonly void IDisposable.Dispose()
			{
			}
		}

		private const int INVALID_ENTRY = -1;

		private const float FULLY_OPAQUE = -1f;

		public static readonly float[] MassTresholds = new float[2] { 0.1f, 0.3f };

		[Serialize]
		private readonly List<Vector2> position = new List<Vector2>();

		[Serialize]
		private readonly List<float> elapsedTime = new List<float>();

		[Serialize]
		private readonly List<int> frame = new List<int>();

		[Serialize]
		private readonly List<float> temperature = new List<float>();

		[Serialize]
		private readonly List<float> mass = new List<float>();

		[Serialize]
		private readonly List<byte> sizeLevel = new List<byte>();

		[Serialize]
		private readonly List<float> alpha = new List<float>();

		[Serialize]
		private readonly List<Disease> disease = new List<Disease>();

		[Serialize]
		private readonly List<int> freeList = new List<int>();

		public int Begin => Next(-1);

		public int End => position.Count;

		public int Count => position.Count - freeList.Count;

		public Subscript this[int index] => new Subscript(this, index);

		public int Add(Vector2 position, float mass, float temperature, int frame, Disease disease)
		{
			int num = ManifestIndex();
			Subscript subscript = this[num];
			subscript.Position = position;
			subscript.ElapsedTime = 0f;
			subscript.Frame = frame;
			subscript.Mass = mass;
			subscript.SizeLevel = CalculateAndGetSizeLevel(mass);
			subscript.Temperature = temperature;
			subscript.Alpha = -1f;
			subscript.Disease = disease;
			return num;
		}

		private byte CalculateAndGetSizeLevel(float mass)
		{
			DebugUtil.DevAssert(MassTresholds != null, "MassTresholds should be statically initialized");
			DebugUtil.DevAssert(MassTresholds.Length != 0, "MassTresholds should be statically initialized");
			for (int i = 0; i < MassTresholds.Length; i++)
			{
				if (MassTresholds[i] - mass >= 0f)
				{
					return (byte)i;
				}
			}
			return (byte)MassTresholds.Length;
		}

		public void Destroy(int index)
		{
			freeList.Add(index);
			frame[index] = -1;
		}

		public void Destroy(List<int> indices)
		{
			freeList.AddRange(indices);
			foreach (int index in indices)
			{
				frame[index] = -1;
			}
		}

		private int ManifestIndex()
		{
			if (freeList.Count > 0)
			{
				List<int> list = freeList;
				int result = list[list.Count - 1];
				freeList.RemoveAt(freeList.Count - 1);
				return result;
			}
			int count = position.Count;
			position.Add(default(Vector2));
			elapsedTime.Add(0f);
			frame.Add(0);
			temperature.Add(0f);
			mass.Add(0f);
			sizeLevel.Add(0);
			alpha.Add(-1f);
			disease.Add(Disease.None);
			return count;
		}

		public int Next(int index)
		{
			if (index == End)
			{
				return End;
			}
			do
			{
				index++;
				if (index == End)
				{
					return End;
				}
			}
			while (frame[index] == -1);
			return index;
		}

		[OnDeserialized]
		public void OnDeserialized()
		{
			if (disease.Count < position.Count)
			{
				disease.Capacity = Math.Max(disease.Capacity, position.Count);
				for (int i = disease.Count; i != position.Count; i++)
				{
					disease.Add(Disease.None);
				}
			}
			if (alpha.Count < position.Count)
			{
				alpha.Capacity = Math.Max(alpha.Capacity, position.Count);
				for (int j = alpha.Count; j != position.Count; j++)
				{
					alpha.Add(-1f);
				}
			}
		}

		public int CountVisible()
		{
			int num = 0;
			using IEnumerator<Subscript> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Visible)
				{
					num++;
				}
			}
			return num;
		}

		public IEnumerator<Subscript> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public struct CellBubbleInfo
	{
		public SimHashes element;

		public float totalMass;

		public float averageTemperature;
	}

	public static BubbleManager instance;

	[Serialize]
	private readonly Dictionary<WorldArchetype, InstanceData> bubbles = new Dictionary<WorldArchetype, InstanceData>();

	[Serialize]
	private readonly Dictionary<Archetype.Id, Archetype> archetypes = new Dictionary<Archetype.Id, Archetype>();

	private Mesh mesh;

	private MaterialPropertyBlock propertyBlock;

	[SerializeField]
	private Texture2D texture;

	[SerializeField]
	private int numFrames;

	[SerializeField]
	private Material material;

	[SerializeField]
	private Grid.SceneLayer sceneLayer;

	[SerializeField]
	private Vector2 particleSize;

	private bool isInfraredON;

	private static Vector2 DEFAULT_VELOCITY = new Vector2(0f, 1f);

	public static void DestroyInstance()
	{
		instance = null;
	}

	protected override void OnPrefabInit()
	{
		instance = this;
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		mesh = new Mesh();
		mesh.MarkDynamic();
		mesh.name = "BubbleManager Mesh";
		propertyBlock = new MaterialPropertyBlock();
		propertyBlock.SetTexture("_MainTex", texture);
		Game.Instance.Subscribe(-880408538, OnTemperatureOverlayInfraredUpdate);
		Game.Instance.Subscribe(972756592, OnTemperatureOverlayInfraredClear);
	}

	private Archetype.Id RegisterArchetype(Archetype archetype)
	{
		Archetype.Id id = archetype.GetId();
		archetypes.TryAdd(id, archetype);
		return id;
	}

	private void OnTemperatureOverlayInfraredClear(object obj)
	{
		isInfraredON = false;
	}

	private void OnTemperatureOverlayInfraredUpdate(object obj)
	{
		isInfraredON = true;
	}

	[OnDeserialized]
	public void OnDeserialized()
	{
		ListPool<Archetype.Id, BubbleManager>.PooledList pooledList = ListPool<Archetype.Id, BubbleManager>.Allocate();
		foreach (Archetype.Id key2 in archetypes.Keys)
		{
			int num = 0;
			foreach (KeyValuePair<WorldArchetype, InstanceData> bubble in bubbles)
			{
				bubble.Deconstruct(out var key, out var value);
				WorldArchetype worldArchetype = key;
				InstanceData instanceData = value;
				if (worldArchetype.archetype.hashCode == key2.hashCode)
				{
					num += instanceData.Count;
				}
			}
			if (num == 0)
			{
				pooledList.Add(key2);
			}
		}
		foreach (Archetype.Id item in pooledList)
		{
			archetypes.Remove(item);
		}
		pooledList.Recycle();
		ListPool<WorldArchetype, BubbleManager>.PooledList pooledList2 = ListPool<WorldArchetype, BubbleManager>.Allocate();
		foreach (WorldArchetype key3 in bubbles.Keys)
		{
			if (!archetypes.ContainsKey(key3.archetype))
			{
				pooledList2.Add(key3);
			}
		}
		if (pooledList2.Count != 0)
		{
			DebugUtil.LogWarningArgs("BubbleManager.OnDeserialized is deleting bubbles");
		}
		foreach (WorldArchetype item2 in pooledList2)
		{
			bubbles.Remove(item2);
		}
		pooledList2.Recycle();
	}

	public void SpawnBubble(SimHashes element, Vector2 position, float mass, float temperature, Disease disease, Vector2? velocity = null)
	{
		if (mass < 1E-09f)
		{
			Debug.LogFormat("BubbleManager.SpawnBubble: Attempted to spawn a bubble with mass {0} which is below the sim's minimum mass threshold of {1}. Bubble will not be spawned.", mass, 1E-09f);
			return;
		}
		int frame = UnityEngine.Random.Range(0, numFrames);
		int num = Grid.PosToCell(position);
		byte worldIdx = Grid.WorldIdx[num];
		WorldArchetype worldArchetype = new WorldArchetype
		{
			worldIdx = worldIdx,
			archetype = RegisterArchetype(new Archetype(velocity ?? DEFAULT_VELOCITY, element))
		};
		ManifestBucket(worldArchetype).Add(position, mass, temperature, frame, disease);
	}

	private InstanceData ManifestBucket(WorldArchetype worldArchetype)
	{
		if (!bubbles.TryGetValue(worldArchetype, out var value))
		{
			value = new InstanceData();
			bubbles[worldArchetype] = value;
		}
		return value;
	}

	private static bool ShouldPop(Vector2 position, SimHashes bubbleElement, out int cell)
	{
		cell = Grid.PosToCell(position);
		if (Grid.Element[cell].id != bubbleElement)
		{
			return !UnderwaterSoundEvent.IsVisiblyInLiquid(position);
		}
		return true;
	}

	public void Sim33ms(float dt)
	{
		ListPool<int, BubbleManager>.PooledList pooledList = ListPool<int, BubbleManager>.Allocate();
		ListPool<WorldArchetype, BubbleManager>.PooledList pooledList2 = ListPool<WorldArchetype, BubbleManager>.Allocate();
		foreach (var (item, instanceData2) in bubbles)
		{
			if (!archetypes.TryGetValue(item.archetype, out var value))
			{
				DebugUtil.LogWarningArgs("BubbleManager.Sim33ms: Unknown archetype id. Skipping this bubble type for this world");
				pooledList2.Add(item);
				continue;
			}
			Vector2 velocity = value.velocity;
			velocity.Normalize();
			velocity *= Grid.HalfCellSizeInMeters;
			foreach (InstanceData.Subscript item2 in instanceData2)
			{
				InstanceData.Subscript current = item2;
				current.Position += value.velocity * dt;
				Vector2 vector = current.Position + velocity;
				int cell;
				bool flag = ShouldPop(vector, value.element, out cell);
				if (!current.Visible || flag)
				{
					SimMessages.AddRemoveSubstance((Grid.Solid[cell] && Grid.Element[cell].IsSolid) ? Grid.PosToCell(current.Position) : cell, value.element, CellEventLogger.Instance.FallingWaterAddToSim, current.Mass, current.Temperature, current.Disease.Idx, current.Disease.Count);
					pooledList.Add(current.Index);
				}
				if (!current.FadingOut)
				{
					if (ShouldPop(vector + velocity * 2f, value.element, out var _))
					{
						current.FadingOut = true;
					}
				}
				else
				{
					current.Alpha = Mathf.Max(0f, current.Alpha - value.alphaFadeSpeed * dt);
				}
				current.ElapsedTime += dt;
			}
			instanceData2.Destroy(pooledList);
			pooledList.Clear();
		}
		pooledList.Recycle();
		foreach (WorldArchetype item3 in pooledList2)
		{
			bubbles.Remove(item3);
		}
		pooledList2.Recycle();
	}

	public void GetBubblesInCell(int cell, List<CellBubbleInfo> results)
	{
		results.Clear();
		int num = Grid.WorldIdx[cell];
		foreach (var (worldArchetype2, instanceData2) in bubbles)
		{
			if (worldArchetype2.worldIdx != num || !archetypes.TryGetValue(worldArchetype2.archetype, out var value))
			{
				continue;
			}
			float num2 = 0f;
			float num3 = 0f;
			foreach (InstanceData.Subscript item in instanceData2)
			{
				if (Grid.PosToCell(item.Position) == cell)
				{
					num2 += item.Mass;
					num3 += item.Mass * item.Temperature;
				}
			}
			if (num2 <= 0f)
			{
				continue;
			}
			bool flag = false;
			for (int i = 0; i < results.Count; i++)
			{
				if (results[i].element == value.element)
				{
					CellBubbleInfo value2 = results[i];
					float num4 = value2.totalMass + num2;
					value2.averageTemperature = (value2.averageTemperature * value2.totalMass + num3) / num4;
					value2.totalMass = num4;
					results[i] = value2;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				results.Add(new CellBubbleInfo
				{
					element = value.element,
					totalMass = num2,
					averageTemperature = num3 / num2
				});
			}
		}
	}

	public void RenderEveryTick(float dt)
	{
		List<Vector3> vertices = MeshUtil.vertices;
		List<Color32> colours = MeshUtil.colours32;
		List<Vector2> uvs = MeshUtil.uvs;
		List<Vector2> uv2s = MeshUtil.uv2s;
		List<Vector4> uv4s = MeshUtil.uv4s;
		List<int> indices = MeshUtil.indices;
		float num = particleSize.x * 0.5f;
		float num2 = particleSize.y * 0.5f;
		Vector2 vector = new Vector2(0f - num, 0f - num2);
		Vector2 vector2 = new Vector2(num, 0f - num2);
		Vector2 vector3 = new Vector2(num, num2);
		Vector2 vector4 = new Vector2(0f - num, num2);
		uvs.Clear();
		uv2s.Clear();
		uv4s.Clear();
		vertices.Clear();
		indices.Clear();
		colours.Clear();
		int num3 = 0;
		foreach (var (worldArchetype2, instanceData2) in bubbles)
		{
			if (worldArchetype2.worldIdx != ClusterManager.Instance.activeWorldId)
			{
				continue;
			}
			if (!archetypes.TryGetValue(worldArchetype2.archetype, out var value))
			{
				DebugUtil.LogWarningArgs("BubbleManager.RenderEveryTick: Unknown archetype id, likely dynamically registered");
				continue;
			}
			int num4 = instanceData2.CountVisible();
			if (num4 == 0)
			{
				continue;
			}
			int num5 = 16249 - num3;
			if (num5 <= 0)
			{
				DebugUtil.LogWarningArgs("BubbleManager.RenderEveryTick: Particle capacity reached, skipping remaining archetypes");
				break;
			}
			int num6 = Mathf.Min(num4, num5);
			bool flag = num6 == num4;
			if (!flag)
			{
				DebugUtil.LogWarningArgs("Too many bubbles to render. Wanted", num4, "but truncating to", num6);
			}
			int num7 = 0;
			foreach (InstanceData.Subscript item3 in instanceData2)
			{
				if (item3.Visible)
				{
					vertices.Add(item3.Position + vector);
					vertices.Add(item3.Position + vector2);
					vertices.Add(item3.Position + vector3);
					vertices.Add(item3.Position + vector4);
					uvs.Add(new Vector2(0f, 0f));
					uvs.Add(new Vector2(1f, 0f));
					uvs.Add(new Vector2(1f, 1f));
					uvs.Add(new Vector2(0f, 1f));
					Color32 colour = value.Colour;
					colour.a = (byte)((float)(int)colour.a * item3.Alpha);
					Vector2 item = new Vector2((int)item3.SizeLevel, item3.Frame);
					uv2s.Add(item);
					uv2s.Add(item);
					uv2s.Add(item);
					uv2s.Add(item);
					Vector4 item2 = (isInfraredON ? ((Vector4)SimDebugView.Instance.NormalizedTemperature(item3.Temperature)) : Vector4.zero);
					uv4s.Add(item2);
					uv4s.Add(item2);
					uv4s.Add(item2);
					uv4s.Add(item2);
					colours.Add(colour);
					colours.Add(colour);
					colours.Add(colour);
					colours.Add(colour);
					int num8 = (num3 + num7) * 4;
					indices.Add(num8);
					indices.Add(num8 + 1);
					indices.Add(num8 + 2);
					indices.Add(num8);
					indices.Add(num8 + 2);
					indices.Add(num8 + 3);
					num7++;
					if (!flag && num7 == num6)
					{
						break;
					}
				}
			}
			DebugUtil.DevAssert(num7 == num6, "Rendered bubble count does not match expected");
			num3 += num7;
		}
		if (num3 > 0)
		{
			mesh.Clear();
			mesh.SetVertices(vertices);
			mesh.SetUVs(0, uvs);
			mesh.SetUVs(1, uv2s);
			mesh.SetUVs(2, uv4s);
			mesh.SetColors(colours);
			mesh.SetTriangles(indices, 0);
			int layer = LayerMask.NameToLayer("Default");
			Vector4 value2 = PropertyTextures.CalculateClusterWorldSize();
			material.SetVector("_ClusterWorldSizeInfo", value2);
			Graphics.DrawMesh(mesh, new Vector3(0f, 0f, Grid.GetLayerZ(sceneLayer)), Quaternion.identity, material, layer, null, 0, propertyBlock);
		}
	}

	protected override void OnCleanUp()
	{
		Game.Instance.Unsubscribe(-880408538, OnTemperatureOverlayInfraredUpdate);
		Game.Instance.Unsubscribe(972756592, OnTemperatureOverlayInfraredClear);
		base.OnCleanUp();
	}
}
