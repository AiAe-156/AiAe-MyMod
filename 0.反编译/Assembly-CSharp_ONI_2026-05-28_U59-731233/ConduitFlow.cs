#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using KSerialization;
using Klei;
using UnityEngine;
using UnityEngine.Assertions;

[SerializationConfig(MemberSerialization.OptIn)]
[DebuggerDisplay("{conduitType}")]
public class ConduitFlow : IConduitFlow
{
	[DebuggerDisplay("{NumEntries}")]
	public class SOAInfo
	{
		private abstract class ConduitTask : DivisibleTask<SOAInfo>
		{
			public ConduitFlow manager;

			public ConduitTask(string name)
				: base(name)
			{
			}
		}

		private class ConduitTaskDivision<Task> : TaskDivision<Task, SOAInfo> where Task : ConduitTask, new()
		{
			public void Initialize(int conduitCount, ConduitFlow manager)
			{
				Initialize(conduitCount);
				Task[] array = tasks;
				foreach (Task val in array)
				{
					val.manager = manager;
				}
			}
		}

		private class ConduitJob : WorkItemCollection<ConduitTask, SOAInfo>
		{
			public void Add<Task>(ConduitTaskDivision<Task> taskDivision) where Task : ConduitTask, new()
			{
				Task[] tasks = taskDivision.tasks;
				foreach (Task work_item in tasks)
				{
					Add(work_item);
				}
			}
		}

		private class ClearPermanentDiseaseContainer : ConduitTask
		{
			public ClearPermanentDiseaseContainer()
				: base("ClearPermanentDiseaseContainer")
			{
			}

			protected override void RunDivision(SOAInfo soaInfo)
			{
				for (int i = start; i != end; i++)
				{
					soaInfo.ForcePermanentDiseaseContainer(i, force_on: false);
				}
			}
		}

		private class PublishTemperatureToSim : ConduitTask
		{
			public PublishTemperatureToSim()
				: base("PublishTemperatureToSim")
			{
			}

			protected override void RunDivision(SOAInfo soaInfo)
			{
				for (int i = start; i != end; i++)
				{
					HandleVector<int>.Handle handle = soaInfo.temperatureHandles[i];
					if (handle.IsValid())
					{
						float temperature = Game.Instance.conduitTemperatureManager.GetTemperature(handle);
						manager.grid[soaInfo.cells[i]].contents.temperature = temperature;
					}
				}
			}
		}

		private class PublishDiseaseToSim : ConduitTask
		{
			public PublishDiseaseToSim()
				: base("PublishDiseaseToSim")
			{
			}

			protected override void RunDivision(SOAInfo soaInfo)
			{
				for (int i = start; i != end; i++)
				{
					HandleVector<int>.Handle handle = soaInfo.diseaseHandles[i];
					if (handle.IsValid())
					{
						ConduitDiseaseManager.Data data = Game.Instance.conduitDiseaseManager.GetData(handle);
						int num = soaInfo.cells[i];
						manager.grid[num].contents.diseaseIdx = data.diseaseIdx;
						manager.grid[num].contents.diseaseCount = data.diseaseCount;
					}
				}
			}
		}

		private class ResetConduit : ConduitTask
		{
			public ResetConduit()
				: base("ResetConduitTask")
			{
			}

			protected override void RunDivision(SOAInfo soaInfo)
			{
				for (int i = start; i != end; i++)
				{
					manager.grid[soaInfo.cells[i]].conduitIdx = -1;
				}
			}
		}

		private class InitializeContentsTask : ConduitTask
		{
			public InitializeContentsTask()
				: base("SetInitialContents")
			{
			}

			protected override void RunDivision(SOAInfo soaInfo)
			{
				for (int i = start; i != end; i++)
				{
					int num = soaInfo.cells[i];
					ConduitContents conduitContents = soaInfo.conduits[i].GetContents(manager);
					if (conduitContents.mass <= 0f)
					{
						conduitContents = ConduitContents.Empty;
					}
					soaInfo.initialContents[i] = conduitContents;
					manager.grid[num].contents = conduitContents;
				}
			}
		}

		private class InvalidateLastFlow : ConduitTask
		{
			public InvalidateLastFlow()
				: base("InvalidateLastFlow")
			{
			}

			protected override void RunDivision(SOAInfo soaInfo)
			{
				for (int i = start; i != end; i++)
				{
					soaInfo.lastFlowInfo[i] = ConduitFlowInfo.DEFAULT;
				}
			}
		}

		private class PublishTemperatureToGame : ConduitTask
		{
			public PublishTemperatureToGame()
				: base("PublishTemperatureToGame")
			{
			}

			protected override void RunDivision(SOAInfo soaInfo)
			{
				for (int i = start; i != end; i++)
				{
					Game.Instance.conduitTemperatureManager.SetData(soaInfo.temperatureHandles[i], ref manager.grid[soaInfo.cells[i]].contents);
				}
			}
		}

		private class PublishDiseaseToGame : ConduitTask
		{
			public PublishDiseaseToGame()
				: base("PublishDiseaseToGame")
			{
			}

			protected override void RunDivision(SOAInfo soaInfo)
			{
				for (int i = start; i != end; i++)
				{
					Game.Instance.conduitDiseaseManager.SetData(soaInfo.diseaseHandles[i], ref manager.grid[soaInfo.cells[i]].contents);
				}
			}
		}

		private class FlowThroughVacuum : ConduitTask
		{
			public FlowThroughVacuum()
				: base("FlowThroughVacuum")
			{
			}

			protected override void RunDivision(SOAInfo soaInfo)
			{
				for (int i = start; i != end; i++)
				{
					Conduit conduit = soaInfo.conduits[i];
					int cell = conduit.GetCell(manager);
					if (manager.grid[cell].contents.element == SimHashes.Vacuum)
					{
						soaInfo.srcFlowDirections[conduit.idx] = FlowDirections.None;
					}
				}
			}
		}

		private List<Conduit> conduits = new List<Conduit>();

		private List<ConduitConnections> conduitConnections = new List<ConduitConnections>();

		private List<ConduitFlowInfo> lastFlowInfo = new List<ConduitFlowInfo>();

		private List<ConduitContents> initialContents = new List<ConduitContents>();

		private List<GameObject> conduitGOs = new List<GameObject>();

		private List<bool> diseaseContentsVisible = new List<bool>();

		private List<int> cells = new List<int>();

		private List<FlowDirections> permittedFlowDirections = new List<FlowDirections>();

		private List<FlowDirections> srcFlowDirections = new List<FlowDirections>();

		private List<FlowDirections> pullDirections = new List<FlowDirections>();

		private List<FlowDirections> targetFlowDirections = new List<FlowDirections>();

		private List<HandleVector<int>.Handle> structureTemperatureHandles = new List<HandleVector<int>.Handle>();

		private List<HandleVector<int>.Handle> temperatureHandles = new List<HandleVector<int>.Handle>();

		private List<HandleVector<int>.Handle> diseaseHandles = new List<HandleVector<int>.Handle>();

		private ConduitTaskDivision<ClearPermanentDiseaseContainer> clearPermanentDiseaseContainer = new ConduitTaskDivision<ClearPermanentDiseaseContainer>();

		private ConduitTaskDivision<PublishTemperatureToSim> publishTemperatureToSim = new ConduitTaskDivision<PublishTemperatureToSim>();

		private ConduitTaskDivision<PublishDiseaseToSim> publishDiseaseToSim = new ConduitTaskDivision<PublishDiseaseToSim>();

		private ConduitTaskDivision<ResetConduit> resetConduit = new ConduitTaskDivision<ResetConduit>();

		private ConduitJob clearJob = new ConduitJob();

		private ConduitTaskDivision<InitializeContentsTask> initializeContents = new ConduitTaskDivision<InitializeContentsTask>();

		private ConduitTaskDivision<InvalidateLastFlow> invalidateLastFlow = new ConduitTaskDivision<InvalidateLastFlow>();

		private ConduitJob beginFrameJob = new ConduitJob();

		private ConduitTaskDivision<PublishTemperatureToGame> publishTemperatureToGame = new ConduitTaskDivision<PublishTemperatureToGame>();

		private ConduitTaskDivision<PublishDiseaseToGame> publishDiseaseToGame = new ConduitTaskDivision<PublishDiseaseToGame>();

		private ConduitJob endFrameJob = new ConduitJob();

		private ConduitTaskDivision<FlowThroughVacuum> flowThroughVacuum = new ConduitTaskDivision<FlowThroughVacuum>();

		private ConduitJob updateFlowDirectionJob = new ConduitJob();

		public int NumEntries => conduits.Count;

		public int AddConduit(ConduitFlow manager, GameObject conduit_go, int cell)
		{
			int count = conduitConnections.Count;
			Conduit item = new Conduit(count);
			conduits.Add(item);
			conduitConnections.Add(new ConduitConnections
			{
				left = -1,
				right = -1,
				up = -1,
				down = -1
			});
			ConduitContents contents = manager.grid[cell].contents;
			initialContents.Add(contents);
			lastFlowInfo.Add(ConduitFlowInfo.DEFAULT);
			HandleVector<int>.Handle handle = GameComps.StructureTemperatures.GetHandle(conduit_go);
			HandleVector<int>.Handle handle2 = Game.Instance.conduitTemperatureManager.Allocate(manager.conduitType, count, handle, ref contents);
			HandleVector<int>.Handle item2 = Game.Instance.conduitDiseaseManager.Allocate(handle2, ref contents);
			cells.Add(cell);
			diseaseContentsVisible.Add(item: false);
			structureTemperatureHandles.Add(handle);
			temperatureHandles.Add(handle2);
			diseaseHandles.Add(item2);
			conduitGOs.Add(conduit_go);
			permittedFlowDirections.Add(FlowDirections.None);
			srcFlowDirections.Add(FlowDirections.None);
			pullDirections.Add(FlowDirections.None);
			targetFlowDirections.Add(FlowDirections.None);
			return count;
		}

		public void Clear(ConduitFlow manager)
		{
			if (clearJob.Count == 0)
			{
				clearJob.Reset(this);
				clearJob.Add(publishTemperatureToSim);
				clearJob.Add(publishDiseaseToSim);
				clearJob.Add(resetConduit);
			}
			clearPermanentDiseaseContainer.Initialize(conduits.Count, manager);
			publishTemperatureToSim.Initialize(conduits.Count, manager);
			publishDiseaseToSim.Initialize(conduits.Count, manager);
			resetConduit.Initialize(conduits.Count, manager);
			clearPermanentDiseaseContainer.Run(this, 0);
			GlobalJobManager.Run(clearJob);
			for (int i = 0; i != conduits.Count; i++)
			{
				Game.Instance.conduitDiseaseManager.Free(diseaseHandles[i]);
			}
			for (int j = 0; j != conduits.Count; j++)
			{
				Game.Instance.conduitTemperatureManager.Free(temperatureHandles[j]);
			}
			cells.Clear();
			diseaseContentsVisible.Clear();
			permittedFlowDirections.Clear();
			srcFlowDirections.Clear();
			pullDirections.Clear();
			targetFlowDirections.Clear();
			conduitGOs.Clear();
			diseaseHandles.Clear();
			temperatureHandles.Clear();
			structureTemperatureHandles.Clear();
			initialContents.Clear();
			lastFlowInfo.Clear();
			conduitConnections.Clear();
			conduits.Clear();
		}

		public Conduit GetConduit(int idx)
		{
			return conduits[idx];
		}

		public ConduitConnections GetConduitConnections(int idx)
		{
			return conduitConnections[idx];
		}

		public void SetConduitConnections(int idx, ConduitConnections data)
		{
			conduitConnections[idx] = data;
		}

		public float GetConduitTemperature(int idx)
		{
			HandleVector<int>.Handle handle = temperatureHandles[idx];
			float temperature = Game.Instance.conduitTemperatureManager.GetTemperature(handle);
			Debug.Assert(!float.IsNaN(temperature));
			return temperature;
		}

		public void SetConduitTemperatureData(int idx, ref ConduitContents contents)
		{
			HandleVector<int>.Handle handle = temperatureHandles[idx];
			Game.Instance.conduitTemperatureManager.SetData(handle, ref contents);
		}

		public ConduitDiseaseManager.Data GetDiseaseData(int idx)
		{
			HandleVector<int>.Handle handle = diseaseHandles[idx];
			return Game.Instance.conduitDiseaseManager.GetData(handle);
		}

		public void SetDiseaseData(int idx, ref ConduitContents contents)
		{
			HandleVector<int>.Handle handle = diseaseHandles[idx];
			Game.Instance.conduitDiseaseManager.SetData(handle, ref contents);
		}

		public GameObject GetConduitGO(int idx)
		{
			return conduitGOs[idx];
		}

		public void ForcePermanentDiseaseContainer(int idx, bool force_on)
		{
			bool flag = diseaseContentsVisible[idx];
			if (flag != force_on)
			{
				diseaseContentsVisible[idx] = force_on;
				GameObject gameObject = conduitGOs[idx];
				if (!(gameObject == null))
				{
					PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
					component.ForcePermanentDiseaseContainer(force_on);
				}
			}
		}

		public Conduit GetConduitFromDirection(int idx, FlowDirections direction)
		{
			ConduitConnections conduitConnections = this.conduitConnections[idx];
			return direction switch
			{
				FlowDirections.Left => (conduitConnections.left != Grid.InvalidCell) ? conduits[conduitConnections.left] : Conduit.Invalid, 
				FlowDirections.Right => (conduitConnections.right != Grid.InvalidCell) ? conduits[conduitConnections.right] : Conduit.Invalid, 
				FlowDirections.Up => (conduitConnections.up != Grid.InvalidCell) ? conduits[conduitConnections.up] : Conduit.Invalid, 
				FlowDirections.Down => (conduitConnections.down != Grid.InvalidCell) ? conduits[conduitConnections.down] : Conduit.Invalid, 
				_ => Conduit.Invalid, 
			};
		}

		public void BeginFrame(ConduitFlow manager)
		{
			if (beginFrameJob.Count == 0)
			{
				beginFrameJob.Reset(this);
				beginFrameJob.Add(initializeContents);
				beginFrameJob.Add(invalidateLastFlow);
			}
			initializeContents.Initialize(conduits.Count, manager);
			invalidateLastFlow.Initialize(conduits.Count, manager);
			GlobalJobManager.Run(beginFrameJob);
		}

		public void EndFrame(ConduitFlow manager)
		{
			if (endFrameJob.Count == 0)
			{
				endFrameJob.Reset(this);
				endFrameJob.Add(publishDiseaseToGame);
			}
			publishTemperatureToGame.Initialize(conduits.Count, manager);
			publishDiseaseToGame.Initialize(conduits.Count, manager);
			publishTemperatureToGame.Run(this, 0);
			GlobalJobManager.Run(endFrameJob);
		}

		public void UpdateFlowDirection(ConduitFlow manager)
		{
			if (updateFlowDirectionJob.Count == 0)
			{
				updateFlowDirectionJob.Reset(this);
				updateFlowDirectionJob.Add(flowThroughVacuum);
			}
			flowThroughVacuum.Initialize(conduits.Count, manager);
			GlobalJobManager.Run(updateFlowDirectionJob);
		}

		public void ResetLastFlowInfo(int idx)
		{
			lastFlowInfo[idx] = ConduitFlowInfo.DEFAULT;
		}

		public void SetLastFlowInfo(int idx, FlowDirections direction, ref ConduitContents contents)
		{
			if (lastFlowInfo[idx].direction == FlowDirections.None)
			{
				lastFlowInfo[idx] = new ConduitFlowInfo
				{
					direction = direction,
					contents = contents
				};
			}
		}

		public ConduitContents GetInitialContents(int idx)
		{
			return initialContents[idx];
		}

		public ConduitFlowInfo GetLastFlowInfo(int idx)
		{
			return lastFlowInfo[idx];
		}

		public FlowDirections GetPermittedFlowDirections(int idx)
		{
			return permittedFlowDirections[idx];
		}

		public void SetPermittedFlowDirections(int idx, FlowDirections permitted)
		{
			permittedFlowDirections[idx] = permitted;
		}

		public FlowDirections AddPermittedFlowDirections(int idx, FlowDirections delta)
		{
			return permittedFlowDirections[idx] |= delta;
		}

		public FlowDirections RemovePermittedFlowDirections(int idx, FlowDirections delta)
		{
			return permittedFlowDirections[idx] &= (FlowDirections)(byte)(~(int)delta);
		}

		public FlowDirections GetTargetFlowDirection(int idx)
		{
			return targetFlowDirections[idx];
		}

		public void SetTargetFlowDirection(int idx, FlowDirections directions)
		{
			targetFlowDirections[idx] = directions;
		}

		public FlowDirections GetSrcFlowDirection(int idx)
		{
			return srcFlowDirections[idx];
		}

		public void SetSrcFlowDirection(int idx, FlowDirections directions)
		{
			srcFlowDirections[idx] = directions;
		}

		public FlowDirections GetPullDirection(int idx)
		{
			return pullDirections[idx];
		}

		public void SetPullDirection(int idx, FlowDirections directions)
		{
			pullDirections[idx] = directions;
		}

		public int GetCell(int idx)
		{
			return cells[idx];
		}

		public void SetCell(int idx, int cell)
		{
			cells[idx] = cell;
		}
	}

	[DebuggerDisplay("{priority} {callback.Target.name} {callback.Target} {callback.Method}")]
	public struct ConduitUpdater
	{
		public ConduitFlowPriority priority;

		public Action<float> callback;
	}

	[DebuggerDisplay("conduit {conduitIdx}:{contents.element}")]
	public struct GridNode
	{
		public int conduitIdx;

		public ConduitContents contents;
	}

	public struct SerializedContents
	{
		public SimHashes element;

		public float mass;

		public float temperature;

		public int diseaseHash;

		public int diseaseCount;

		public SerializedContents(SimHashes element, float mass, float temperature, byte disease_idx, int disease_count)
		{
			this.element = element;
			this.mass = mass;
			this.temperature = temperature;
			diseaseHash = ((disease_idx != byte.MaxValue) ? Db.Get().Diseases[disease_idx].id.GetHashCode() : 0);
			diseaseCount = disease_count;
			if (diseaseCount <= 0)
			{
				diseaseHash = 0;
			}
		}

		public SerializedContents(ConduitContents src)
			: this(src.element, src.mass, src.temperature, src.diseaseIdx, src.diseaseCount)
		{
		}
	}

	[Flags]
	public enum FlowDirections : byte
	{
		None = 0,
		Down = 1,
		Left = 2,
		Right = 4,
		Up = 8,
		All = 0xF
	}

	[DebuggerDisplay("conduits l:{left}, r:{right}, u:{up}, d:{down}")]
	public struct ConduitConnections
	{
		public int left;

		public int right;

		public int up;

		public int down;

		public static readonly ConduitConnections DEFAULT = new ConduitConnections
		{
			left = -1,
			right = -1,
			up = -1,
			down = -1
		};

		public int GetConnection(FlowDirections dir)
		{
			return dir switch
			{
				FlowDirections.Left => left, 
				FlowDirections.Right => right, 
				FlowDirections.Up => up, 
				FlowDirections.Down => down, 
				_ => -1, 
			};
		}
	}

	[DebuggerDisplay("{direction}:{contents.element}")]
	public struct ConduitFlowInfo
	{
		public FlowDirections direction;

		public ConduitContents contents;

		public static readonly ConduitFlowInfo DEFAULT = new ConduitFlowInfo
		{
			direction = FlowDirections.None,
			contents = ConduitContents.Empty
		};
	}

	[Serializable]
	[DebuggerDisplay("conduit {idx}")]
	public struct Conduit : IEquatable<Conduit>
	{
		public static readonly Conduit Invalid = new Conduit(-1);

		public readonly int idx;

		public Conduit(int idx)
		{
			this.idx = idx;
		}

		public FlowDirections GetPermittedFlowDirections(ConduitFlow manager)
		{
			return manager.soaInfo.GetPermittedFlowDirections(idx);
		}

		public void SetPermittedFlowDirections(FlowDirections permitted, ConduitFlow manager)
		{
			manager.soaInfo.SetPermittedFlowDirections(idx, permitted);
		}

		public FlowDirections GetTargetFlowDirection(ConduitFlow manager)
		{
			return manager.soaInfo.GetTargetFlowDirection(idx);
		}

		public void SetTargetFlowDirection(FlowDirections directions, ConduitFlow manager)
		{
			manager.soaInfo.SetTargetFlowDirection(idx, directions);
		}

		public ConduitContents GetContents(ConduitFlow manager)
		{
			int cell = manager.soaInfo.GetCell(idx);
			ConduitContents contents = manager.grid[cell].contents;
			SOAInfo soaInfo = manager.soaInfo;
			contents.temperature = soaInfo.GetConduitTemperature(idx);
			ConduitDiseaseManager.Data diseaseData = soaInfo.GetDiseaseData(idx);
			contents.diseaseIdx = diseaseData.diseaseIdx;
			contents.diseaseCount = diseaseData.diseaseCount;
			return contents;
		}

		public void SetContents(ConduitFlow manager, ConduitContents contents)
		{
			int cell = manager.soaInfo.GetCell(idx);
			manager.grid[cell].contents = contents;
			SOAInfo soaInfo = manager.soaInfo;
			soaInfo.SetConduitTemperatureData(idx, ref contents);
			soaInfo.ForcePermanentDiseaseContainer(idx, contents.diseaseIdx != byte.MaxValue);
			soaInfo.SetDiseaseData(idx, ref contents);
		}

		public ConduitFlowInfo GetLastFlowInfo(ConduitFlow manager)
		{
			return manager.soaInfo.GetLastFlowInfo(idx);
		}

		public ConduitContents GetInitialContents(ConduitFlow manager)
		{
			return manager.soaInfo.GetInitialContents(idx);
		}

		public int GetCell(ConduitFlow manager)
		{
			return manager.soaInfo.GetCell(idx);
		}

		public bool Equals(Conduit other)
		{
			return idx == other.idx;
		}
	}

	[DebuggerDisplay("{element} M:{mass} T:{temperature}")]
	public struct ConduitContents
	{
		public SimHashes element;

		private float initial_mass;

		private float added_mass;

		private float removed_mass;

		public float temperature;

		public byte diseaseIdx;

		public int diseaseCount;

		public static readonly ConduitContents Empty = new ConduitContents
		{
			element = SimHashes.Vacuum,
			initial_mass = 0f,
			added_mass = 0f,
			removed_mass = 0f,
			temperature = 0f,
			diseaseIdx = byte.MaxValue,
			diseaseCount = 0
		};

		public float mass => initial_mass + added_mass - removed_mass;

		public float movable_mass => initial_mass - removed_mass;

		public ConduitContents(SimHashes element, float mass, float temperature, byte disease_idx, int disease_count)
		{
			Debug.Assert(!float.IsNaN(temperature));
			this.element = element;
			initial_mass = mass;
			added_mass = 0f;
			removed_mass = 0f;
			this.temperature = temperature;
			diseaseIdx = disease_idx;
			diseaseCount = disease_count;
		}

		public void ConsolidateMass()
		{
			initial_mass += added_mass;
			added_mass = 0f;
			initial_mass -= removed_mass;
			removed_mass = 0f;
		}

		public float GetEffectiveCapacity(float maximum_capacity)
		{
			float num = mass;
			return Mathf.Max(0f, maximum_capacity - num);
		}

		public void AddMass(float amount)
		{
			Debug.Assert(0f <= amount);
			added_mass += amount;
		}

		public float RemoveMass(float amount)
		{
			Debug.Assert(0f <= amount);
			float result = 0f;
			float num = mass - amount;
			if (num < 0f)
			{
				amount += num;
				result = 0f - num;
				Debug.Assert(condition: false);
			}
			removed_mass += amount;
			return result;
		}
	}

	[DebuggerDisplay("{network.ConduitType}:{cells.Count}")]
	private struct Network
	{
		public List<int> cells;

		public FlowUtilityNetwork network;
	}

	private struct BuildNetworkTask : IWorkItem<ConduitFlow>
	{
		[DebuggerDisplay("cell {cell}:{distance}")]
		private struct OrderNode
		{
			public int idx;

			public FlowDirections direction;

			public FlowDirections permited;
		}

		[DebuggerDisplay("vertices:{vertex_cells.Count}, edges:{edges.Count}")]
		private struct Graph
		{
			[DebuggerDisplay("{cell}:{direction}")]
			public struct Vertex : IEquatable<Vertex>
			{
				public FlowDirections direction;

				public int cell;

				public static Vertex INVALID = new Vertex
				{
					direction = FlowDirections.None,
					cell = -1
				};

				public bool is_valid => cell != -1;

				public bool Equals(Vertex rhs)
				{
					return direction == rhs.direction && cell == rhs.cell;
				}
			}

			[DebuggerDisplay("{vertices[0].cell}:{vertices[0].direction} -> {vertices[1].cell}:{vertices[1].direction}")]
			public struct Edge : IEquatable<Edge>
			{
				[DebuggerDisplay("{cell}:{direction}")]
				public struct VertexIterator
				{
					public int cell;

					public FlowDirections direction;

					private ConduitFlow conduit_flow;

					private Edge edge;

					public VertexIterator(ConduitFlow conduit_flow, Edge edge)
					{
						this.conduit_flow = conduit_flow;
						this.edge = edge;
						cell = edge.vertices[0].cell;
						direction = edge.vertices[0].direction;
					}

					public void Next()
					{
						int conduitIdx = conduit_flow.grid[cell].conduitIdx;
						Conduit conduitFromDirection = conduit_flow.soaInfo.GetConduitFromDirection(conduitIdx, direction);
						Debug.Assert(conduitFromDirection.idx != -1);
						cell = conduitFromDirection.GetCell(conduit_flow);
						if (cell == edge.vertices[1].cell)
						{
							return;
						}
						direction = Opposite(direction);
						bool flag = false;
						for (int i = 0; i != 3; i++)
						{
							direction = ComputeNextFlowDirection(direction);
							if (conduit_flow.soaInfo.GetConduitFromDirection(conduitFromDirection.idx, direction).idx != -1)
							{
								flag = true;
								break;
							}
						}
						Debug.Assert(flag);
						if (!flag)
						{
							cell = edge.vertices[1].cell;
						}
					}

					public bool IsValid()
					{
						return cell != edge.vertices[1].cell;
					}
				}

				public Vertex[] vertices;

				public static readonly Edge INVALID = new Edge
				{
					vertices = null
				};

				public bool is_valid => vertices != null;

				public bool Equals(Edge rhs)
				{
					if (vertices == null)
					{
						return rhs.vertices == null;
					}
					if (rhs.vertices == null)
					{
						return false;
					}
					return vertices.Length == rhs.vertices.Length && vertices.Length == 2 && vertices[0].Equals(rhs.vertices[0]) && vertices[1].Equals(rhs.vertices[1]);
				}

				public Edge Invert()
				{
					return new Edge
					{
						vertices = new Vertex[2]
						{
							new Vertex
							{
								cell = vertices[1].cell,
								direction = Opposite(vertices[1].direction)
							},
							new Vertex
							{
								cell = vertices[0].cell,
								direction = Opposite(vertices[0].direction)
							}
						}
					};
				}

				public VertexIterator Iter(ConduitFlow conduit_flow)
				{
					return new VertexIterator(conduit_flow, this);
				}
			}

			[DebuggerDisplay("cell:{cell}, parent:{parent == null ? -1 : parent.cell}")]
			private class DFSNode
			{
				public int cell;

				public DFSNode parent;
			}

			private ConduitFlow conduit_flow;

			private HashSetPool<int, ConduitFlow>.PooledHashSet vertex_cells;

			private ListPool<Edge, ConduitFlow>.PooledList edges;

			private ListPool<Edge, ConduitFlow>.PooledList cycles;

			private QueuePool<Vertex, ConduitFlow>.PooledQueue bfs_traversal;

			private HashSetPool<int, ConduitFlow>.PooledHashSet visited;

			private ListPool<Vertex, ConduitFlow>.PooledList pseudo_sources;

			public HashSetPool<int, ConduitFlow>.PooledHashSet sources;

			private HashSetPool<int, ConduitFlow>.PooledHashSet sinks;

			private HashSetPool<DFSNode, ConduitFlow>.PooledHashSet dfs_path;

			private ListPool<DFSNode, ConduitFlow>.PooledList dfs_traversal;

			public HashSetPool<int, ConduitFlow>.PooledHashSet dead_ends;

			private ListPool<Vertex, ConduitFlow>.PooledList cycle_vertices;

			public Graph(FlowUtilityNetwork network)
			{
				conduit_flow = null;
				vertex_cells = HashSetPool<int, ConduitFlow>.Allocate();
				edges = ListPool<Edge, ConduitFlow>.Allocate();
				cycles = ListPool<Edge, ConduitFlow>.Allocate();
				bfs_traversal = QueuePool<Vertex, ConduitFlow>.Allocate();
				visited = HashSetPool<int, ConduitFlow>.Allocate();
				pseudo_sources = ListPool<Vertex, ConduitFlow>.Allocate();
				sources = HashSetPool<int, ConduitFlow>.Allocate();
				sinks = HashSetPool<int, ConduitFlow>.Allocate();
				dfs_path = HashSetPool<DFSNode, ConduitFlow>.Allocate();
				dfs_traversal = ListPool<DFSNode, ConduitFlow>.Allocate();
				dead_ends = HashSetPool<int, ConduitFlow>.Allocate();
				cycle_vertices = ListPool<Vertex, ConduitFlow>.Allocate();
			}

			public void Recycle()
			{
				vertex_cells.Recycle();
				edges.Recycle();
				cycles.Recycle();
				bfs_traversal.Recycle();
				visited.Recycle();
				pseudo_sources.Recycle();
				sources.Recycle();
				sinks.Recycle();
				dfs_path.Recycle();
				dfs_traversal.Recycle();
				dead_ends.Recycle();
				cycle_vertices.Recycle();
			}

			public void Build(ConduitFlow conduit_flow, List<FlowUtilityNetwork.IItem> sources, List<FlowUtilityNetwork.IItem> sinks, bool are_dead_ends_pseudo_sources)
			{
				this.conduit_flow = conduit_flow;
				this.sources.Clear();
				for (int i = 0; i < sources.Count; i++)
				{
					int cell = sources[i].Cell;
					if (conduit_flow.grid[cell].conduitIdx != -1)
					{
						this.sources.Add(cell);
					}
				}
				this.sinks.Clear();
				for (int j = 0; j < sinks.Count; j++)
				{
					int cell2 = sinks[j].Cell;
					if (conduit_flow.grid[cell2].conduitIdx != -1)
					{
						this.sinks.Add(cell2);
					}
				}
				Debug.Assert(bfs_traversal.Count == 0);
				visited.Clear();
				foreach (int source in this.sources)
				{
					bfs_traversal.Enqueue(new Vertex
					{
						cell = source,
						direction = FlowDirections.None
					});
					visited.Add(source);
				}
				pseudo_sources.Clear();
				dead_ends.Clear();
				cycles.Clear();
				while (bfs_traversal.Count != 0)
				{
					Vertex vertex = bfs_traversal.Dequeue();
					vertex_cells.Add(vertex.cell);
					FlowDirections flowDirections = FlowDirections.None;
					int num = 4;
					if (vertex.direction != FlowDirections.None)
					{
						flowDirections = Opposite(vertex.direction);
						num = 3;
					}
					int conduitIdx = conduit_flow.grid[vertex.cell].conduitIdx;
					for (int k = 0; k != num; k++)
					{
						flowDirections = ComputeNextFlowDirection(flowDirections);
						Vertex vertex2 = WalkPath(conduitIdx, conduit_flow.soaInfo.GetConduitFromDirection(conduitIdx, flowDirections).idx, flowDirections, are_dead_ends_pseudo_sources);
						if (!vertex2.is_valid)
						{
							continue;
						}
						Edge item = new Edge
						{
							vertices = new Vertex[2]
							{
								new Vertex
								{
									cell = vertex.cell,
									direction = flowDirections
								},
								vertex2
							}
						};
						if (vertex2.cell == vertex.cell)
						{
							cycles.Add(item);
							continue;
						}
						bool flag = false;
						foreach (Edge edge in edges)
						{
							if (edge.vertices[0].cell == vertex2.cell && edge.vertices[1].cell == vertex.cell)
							{
								flag = true;
								break;
							}
						}
						if (flag || edges.Contains(item))
						{
							continue;
						}
						edges.Add(item);
						if (visited.Add(vertex2.cell))
						{
							if (IsSink(vertex2.cell))
							{
								pseudo_sources.Add(vertex2);
							}
							else
							{
								bfs_traversal.Enqueue(vertex2);
							}
						}
					}
					if (bfs_traversal.Count != 0)
					{
						continue;
					}
					foreach (Vertex pseudo_source in pseudo_sources)
					{
						bfs_traversal.Enqueue(pseudo_source);
					}
					pseudo_sources.Clear();
				}
			}

			private bool IsEndpoint(int cell)
			{
				Debug.Assert(cell != -1);
				GridNode gridNode = conduit_flow.grid[cell];
				return gridNode.conduitIdx == -1 || sources.Contains(cell) || sinks.Contains(cell) || dead_ends.Contains(cell);
			}

			private bool IsSink(int cell)
			{
				return sinks.Contains(cell);
			}

			private bool IsJunction(int cell)
			{
				Debug.Assert(cell != -1);
				GridNode gridNode = conduit_flow.grid[cell];
				Debug.Assert(gridNode.conduitIdx != -1);
				ConduitConnections conduitConnections = conduit_flow.soaInfo.GetConduitConnections(gridNode.conduitIdx);
				return 2 < JunctionValue(conduitConnections.down) + JunctionValue(conduitConnections.left) + JunctionValue(conduitConnections.up) + JunctionValue(conduitConnections.right);
			}

			private int JunctionValue(int conduit)
			{
				return (conduit != -1) ? 1 : 0;
			}

			private Vertex WalkPath(int root_conduit, int conduit, FlowDirections direction, bool are_dead_ends_pseudo_sources)
			{
				if (conduit == -1)
				{
					return Vertex.INVALID;
				}
				int cell;
				bool flag;
				do
				{
					cell = conduit_flow.soaInfo.GetCell(conduit);
					if (IsEndpoint(cell) || IsJunction(cell))
					{
						return new Vertex
						{
							cell = cell,
							direction = direction
						};
					}
					direction = Opposite(direction);
					flag = true;
					for (int i = 0; i != 3; i++)
					{
						direction = ComputeNextFlowDirection(direction);
						Conduit conduitFromDirection = conduit_flow.soaInfo.GetConduitFromDirection(conduit, direction);
						if (conduitFromDirection.idx != -1)
						{
							conduit = conduitFromDirection.idx;
							flag = false;
							break;
						}
					}
				}
				while (!flag);
				if (are_dead_ends_pseudo_sources)
				{
					dead_ends.Add(cell);
					pseudo_sources.Add(new Vertex
					{
						cell = cell,
						direction = ComputeNextFlowDirection(direction)
					});
					return Vertex.INVALID;
				}
				return new Vertex
				{
					cell = cell,
					direction = (direction = Opposite(ComputeNextFlowDirection(direction)))
				};
			}

			public void Merge(Graph inverted_graph)
			{
				foreach (Edge edge4 in inverted_graph.edges)
				{
					Edge edge = edge4.Invert();
					bool flag = false;
					foreach (Edge edge5 in edges)
					{
						if (edge5.Equals(edge4) || edge5.Equals(edge))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						edges.Add(edge);
						vertex_cells.Add(edge.vertices[0].cell);
						vertex_cells.Add(edge.vertices[1].cell);
					}
				}
				int num = 1000;
				for (int i = 0; i != num; i++)
				{
					Debug.Assert(i != num - 1);
					bool flag2 = false;
					foreach (int vertex_cell in vertex_cells)
					{
						if (IsSink(vertex_cell))
						{
							continue;
						}
						bool flag3 = false;
						foreach (Edge edge6 in edges)
						{
							if (edge6.vertices[0].cell == vertex_cell)
							{
								flag3 = true;
								break;
							}
						}
						if (flag3)
						{
							continue;
						}
						int num2 = -1;
						for (int j = 0; j < inverted_graph.edges.Count; j++)
						{
							if (inverted_graph.edges[j].vertices[1].cell == vertex_cell)
							{
								num2 = j;
								break;
							}
						}
						if (num2 == -1)
						{
							continue;
						}
						Edge edge2 = inverted_graph.edges[num2];
						for (int k = 0; k != edges.Count; k++)
						{
							Edge edge3 = edges[k];
							if (edge3.vertices[0].cell == edge2.vertices[0].cell && edge3.vertices[1].cell == edge2.vertices[1].cell)
							{
								edges[k] = edge3.Invert();
							}
						}
						flag2 = true;
						break;
					}
					if (!flag2)
					{
						break;
					}
				}
			}

			public void BreakCycles()
			{
				visited.Clear();
				foreach (int vertex_cell in vertex_cells)
				{
					if (visited.Contains(vertex_cell))
					{
						continue;
					}
					dfs_path.Clear();
					dfs_traversal.Clear();
					dfs_traversal.Add(new DFSNode
					{
						cell = vertex_cell,
						parent = null
					});
					while (dfs_traversal.Count != 0)
					{
						DFSNode dFSNode = dfs_traversal[dfs_traversal.Count - 1];
						dfs_traversal.RemoveAt(dfs_traversal.Count - 1);
						bool flag = false;
						for (DFSNode parent = dFSNode.parent; parent != null; parent = parent.parent)
						{
							if (parent.cell == dFSNode.cell)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							for (int num = edges.Count - 1; num != -1; num--)
							{
								Edge item = edges[num];
								if (item.vertices[0].cell == dFSNode.parent.cell && item.vertices[1].cell == dFSNode.cell)
								{
									cycles.Add(item);
									edges.RemoveAt(num);
								}
							}
						}
						else
						{
							if (!visited.Add(dFSNode.cell))
							{
								continue;
							}
							foreach (Edge edge in edges)
							{
								if (edge.vertices[0].cell == dFSNode.cell)
								{
									dfs_traversal.Add(new DFSNode
									{
										cell = edge.vertices[1].cell,
										parent = dFSNode
									});
								}
							}
						}
					}
				}
			}

			public void WriteFlow(bool cycles_only = false)
			{
				if (!cycles_only)
				{
					foreach (Edge edge in edges)
					{
						Edge.VertexIterator vertexIterator = edge.Iter(conduit_flow);
						while (vertexIterator.IsValid())
						{
							conduit_flow.soaInfo.AddPermittedFlowDirections(conduit_flow.grid[vertexIterator.cell].conduitIdx, vertexIterator.direction);
							vertexIterator.Next();
						}
					}
				}
				foreach (Edge cycle in cycles)
				{
					cycle_vertices.Clear();
					Edge.VertexIterator vertexIterator2 = cycle.Iter(conduit_flow);
					vertexIterator2.Next();
					while (vertexIterator2.IsValid())
					{
						cycle_vertices.Add(new Vertex
						{
							cell = vertexIterator2.cell,
							direction = vertexIterator2.direction
						});
						vertexIterator2.Next();
					}
					if (cycle_vertices.Count > 1)
					{
						int num = 0;
						int num2 = cycle_vertices.Count - 1;
						FlowDirections direction = cycle.vertices[0].direction;
						while (num <= num2)
						{
							Vertex vertex = cycle_vertices[num];
							conduit_flow.soaInfo.AddPermittedFlowDirections(conduit_flow.grid[vertex.cell].conduitIdx, Opposite(direction));
							direction = vertex.direction;
							num++;
							Vertex vertex2 = cycle_vertices[num2];
							conduit_flow.soaInfo.AddPermittedFlowDirections(conduit_flow.grid[vertex2.cell].conduitIdx, vertex2.direction);
							num2--;
						}
						dead_ends.Add(cycle_vertices[num].cell);
						dead_ends.Add(cycle_vertices[num2].cell);
					}
				}
			}
		}

		private Network network;

		private StackPool<OrderNode, ConduitFlow>.PooledStack order_dfs_stack;

		private HashSetPool<int, ConduitFlow>.PooledHashSet visited;

		private ListPool<KeyValuePair<int, int>, ConduitFlow>.PooledList from_sources;

		private ListPool<KeyValuePair<int, int>, ConduitFlow>.PooledList from_sinks;

		private Graph from_sources_graph;

		private Graph from_sinks_graph;

		public BuildNetworkTask(Network network, int conduit_count)
		{
			this.network = network;
			order_dfs_stack = StackPool<OrderNode, ConduitFlow>.Allocate();
			visited = HashSetPool<int, ConduitFlow>.Allocate();
			from_sources = ListPool<KeyValuePair<int, int>, ConduitFlow>.Allocate();
			from_sinks = ListPool<KeyValuePair<int, int>, ConduitFlow>.Allocate();
			from_sources_graph = new Graph(network.network);
			from_sinks_graph = new Graph(network.network);
		}

		public void Finish()
		{
			order_dfs_stack.Recycle();
			visited.Recycle();
			from_sources.Recycle();
			from_sinks.Recycle();
			from_sources_graph.Recycle();
			from_sinks_graph.Recycle();
		}

		private void ComputeFlow(ConduitFlow outer)
		{
			from_sources_graph.Build(outer, network.network.sources, network.network.sinks, are_dead_ends_pseudo_sources: true);
			from_sinks_graph.Build(outer, network.network.sinks, network.network.sources, are_dead_ends_pseudo_sources: false);
			from_sources_graph.Merge(from_sinks_graph);
			from_sources_graph.BreakCycles();
			from_sources_graph.WriteFlow();
			from_sinks_graph.WriteFlow(cycles_only: true);
		}

		private void ReverseTopologicalOrderingPush(ConduitFlow outer, List<int> result, int start_cell)
		{
			Debug.Assert(order_dfs_stack.Count == 0);
			OrderNode item = default(OrderNode);
			item.idx = outer.grid[start_cell].conduitIdx;
			item.direction = FlowDirections.Down;
			item.permited = outer.soaInfo.GetPermittedFlowDirections(item.idx);
			if (item.idx == -1 || !visited.Add(item.idx))
			{
				return;
			}
			order_dfs_stack.Push(item);
			while (order_dfs_stack.Count > 0)
			{
				OrderNode item2 = order_dfs_stack.Pop();
				if ((item2.direction & FlowDirections.All) != FlowDirections.None)
				{
					FlowDirections direction = item2.direction;
					item2.direction = (FlowDirections)((uint)direction << 1);
					order_dfs_stack.Push(item2);
					if ((item2.permited & direction) != FlowDirections.None)
					{
						item.idx = outer.soaInfo.GetConduitConnections(item2.idx).GetConnection(direction);
						item.direction = FlowDirections.Down;
						item.permited = outer.soaInfo.GetPermittedFlowDirections(item.idx);
						if (item.idx != -1 && visited.Add(item.idx))
						{
							order_dfs_stack.Push(item);
						}
					}
				}
				else
				{
					result.Add(outer.soaInfo.GetCell(item2.idx));
				}
			}
		}

		private void ReverseTopologicalOrderingPull(ConduitFlow outer, List<int> result, int start_cell)
		{
			Debug.Assert(order_dfs_stack.Count == 0);
			OrderNode item = default(OrderNode);
			item.idx = outer.grid[start_cell].conduitIdx;
			item.direction = FlowDirections.Down;
			item.permited = outer.soaInfo.GetPermittedFlowDirections(item.idx);
			if (item.idx == -1 || !visited.Add(item.idx))
			{
				return;
			}
			order_dfs_stack.Push(item);
			while (order_dfs_stack.Count > 0)
			{
				OrderNode item2 = order_dfs_stack.Pop();
				if ((item2.direction & FlowDirections.All) != FlowDirections.None)
				{
					FlowDirections direction = item2.direction;
					item2.direction = (FlowDirections)((uint)direction << 1);
					order_dfs_stack.Push(item2);
					item.idx = outer.soaInfo.GetConduitConnections(item2.idx).GetConnection(Opposite(direction));
					if (item.idx != -1)
					{
						item.direction = FlowDirections.Down;
						item.permited = outer.soaInfo.GetPermittedFlowDirections(item.idx);
						if ((item.permited & direction) != FlowDirections.None && visited.Add(item.idx))
						{
							order_dfs_stack.Push(item);
						}
					}
				}
				else
				{
					result.Add(outer.soaInfo.GetCell(item2.idx));
				}
			}
		}

		private void ComputeOrder(ConduitFlow outer)
		{
			network.cells.Capacity = Math.Max(network.cells.Capacity, outer.soaInfo.NumEntries);
			foreach (int source in from_sources_graph.sources)
			{
				ReverseTopologicalOrderingPush(outer, network.cells, source);
			}
			foreach (int dead_end in from_sources_graph.dead_ends)
			{
				ReverseTopologicalOrderingPush(outer, network.cells, dead_end);
			}
			int count = network.cells.Count;
			foreach (int source2 in from_sinks_graph.sources)
			{
				ReverseTopologicalOrderingPull(outer, network.cells, source2);
			}
			foreach (int dead_end2 in from_sinks_graph.dead_ends)
			{
				ReverseTopologicalOrderingPull(outer, network.cells, dead_end2);
			}
			if (count != network.cells.Count)
			{
				network.cells.Reverse(count, network.cells.Count - count);
			}
		}

		public void Run(ConduitFlow outer, int threadIndex)
		{
			ComputeFlow(outer);
			ComputeOrder(outer);
		}
	}

	private struct ConnectContext
	{
		public ListPool<int, ConduitFlow>.PooledList cells;

		public ConduitFlow outer;

		public ConnectContext(ConduitFlow outer)
		{
			this.outer = outer;
			cells = ListPool<int, ConduitFlow>.Allocate();
			cells.Capacity = Mathf.Max(cells.Capacity, outer.soaInfo.NumEntries);
		}

		public void Finish()
		{
			cells.Recycle();
		}
	}

	private struct ConnectTask : IWorkItem<ConnectContext>
	{
		private int start;

		private int end;

		public ConnectTask(int start, int end)
		{
			this.start = start;
			this.end = end;
		}

		public void Run(ConnectContext context, int threadIndex)
		{
			for (int i = start; i != end; i++)
			{
				int num = context.cells[i];
				int conduitIdx = context.outer.grid[num].conduitIdx;
				if (conduitIdx == -1)
				{
					continue;
				}
				UtilityConnections connections = context.outer.networkMgr.GetConnections(num, is_physical_building: true);
				if (connections != 0)
				{
					ConduitConnections dEFAULT = ConduitConnections.DEFAULT;
					int num2 = Grid.CellLeft(num);
					if (Grid.IsValidCell(num2) && (connections & UtilityConnections.Left) != 0)
					{
						dEFAULT.left = context.outer.grid[num2].conduitIdx;
					}
					num2 = Grid.CellRight(num);
					if (Grid.IsValidCell(num2) && (connections & UtilityConnections.Right) != 0)
					{
						dEFAULT.right = context.outer.grid[num2].conduitIdx;
					}
					num2 = Grid.CellBelow(num);
					if (Grid.IsValidCell(num2) && (connections & UtilityConnections.Down) != 0)
					{
						dEFAULT.down = context.outer.grid[num2].conduitIdx;
					}
					num2 = Grid.CellAbove(num);
					if (Grid.IsValidCell(num2) && (connections & UtilityConnections.Up) != 0)
					{
						dEFAULT.up = context.outer.grid[num2].conduitIdx;
					}
					context.outer.soaInfo.SetConduitConnections(conduitIdx, dEFAULT);
				}
			}
		}
	}

	private struct UpdateNetworkTask : IWorkItem<ConduitFlow>
	{
		private Network network;

		public bool continue_updating { get; private set; }

		public UpdateNetworkTask(Network network)
		{
			continue_updating = true;
			this.network = network;
		}

		public void Run(ConduitFlow conduit_flow, int threadIndex)
		{
			Debug.Assert(continue_updating);
			continue_updating = false;
			foreach (int cell in network.cells)
			{
				int conduitIdx = conduit_flow.grid[cell].conduitIdx;
				if (conduit_flow.UpdateConduit(conduit_flow.soaInfo.GetConduit(conduitIdx)))
				{
					continue_updating = true;
				}
			}
		}

		public void Finish(ConduitFlow conduit_flow)
		{
			foreach (int cell in network.cells)
			{
				ConduitContents contents = conduit_flow.grid[cell].contents;
				contents.ConsolidateMass();
				conduit_flow.grid[cell].contents = contents;
			}
		}
	}

	public const float MAX_LIQUID_MASS = 10f;

	public const float MAX_GAS_MASS = 1f;

	public ConduitType conduitType;

	private float MaxMass = 10f;

	private const float PERCENT_MAX_MASS_FOR_STATE_CHANGE_DAMAGE = 0.1f;

	public const float TickRate = 1f;

	public const float WaitTime = 1f;

	private float elapsedTime = 0f;

	private float lastUpdateTime = float.NegativeInfinity;

	public SOAInfo soaInfo = new SOAInfo();

	private bool dirtyConduitUpdaters = false;

	private List<ConduitUpdater> conduitUpdaters = new List<ConduitUpdater>();

	private GridNode[] grid;

	[Serialize]
	public int[] serializedIdx;

	[Serialize]
	public ConduitContents[] serializedContents;

	[Serialize]
	public SerializedContents[] versionedSerializedContents;

	private IUtilityNetworkMgr networkMgr;

	private HashSet<int> replacements = new HashSet<int>();

	private const int FLOW_DIRECTION_COUNT = 4;

	private List<Network> networks = new List<Network>();

	private WorkItemCollection<BuildNetworkTask, ConduitFlow> build_network_job = new WorkItemCollection<BuildNetworkTask, ConduitFlow>();

	private WorkItemCollection<ConnectTask, ConnectContext> connect_job = new WorkItemCollection<ConnectTask, ConnectContext>();

	private WorkItemCollection<UpdateNetworkTask, ConduitFlow> update_networks_job = new WorkItemCollection<UpdateNetworkTask, ConduitFlow>();

	public float ContinuousLerpPercent => Mathf.Clamp01((Time.time - lastUpdateTime) / 1f);

	public float DiscreteLerpPercent => Mathf.Clamp01(elapsedTime / 1f);

	public event System.Action onConduitsRebuilt;

	public void AddConduitUpdater(Action<float> callback, ConduitFlowPriority priority = ConduitFlowPriority.Default)
	{
		conduitUpdaters.Add(new ConduitUpdater
		{
			priority = priority,
			callback = callback
		});
		dirtyConduitUpdaters = true;
	}

	public void RemoveConduitUpdater(Action<float> callback)
	{
		for (int i = 0; i < conduitUpdaters.Count; i++)
		{
			if (conduitUpdaters[i].callback == callback)
			{
				conduitUpdaters.RemoveAt(i);
				dirtyConduitUpdaters = true;
				break;
			}
		}
	}

	private static FlowDirections ComputeNextFlowDirection(FlowDirections current)
	{
		switch (current)
		{
		case FlowDirections.Down:
			return FlowDirections.Left;
		case FlowDirections.Left:
			return FlowDirections.Right;
		case FlowDirections.Right:
			return FlowDirections.Up;
		case FlowDirections.None:
		case FlowDirections.Up:
			return FlowDirections.Down;
		default:
			Debug.Assert(condition: false, "multiple bits are set in 'FlowDirections'...can't compute next direction");
			return FlowDirections.None;
		}
	}

	public static FlowDirections Invert(FlowDirections directions)
	{
		return (FlowDirections)(0xF & (byte)(~(int)directions));
	}

	public static FlowDirections Opposite(FlowDirections directions)
	{
		FlowDirections result = FlowDirections.None;
		if ((directions & FlowDirections.Left) != FlowDirections.None)
		{
			result = FlowDirections.Right;
		}
		else if ((directions & FlowDirections.Right) != FlowDirections.None)
		{
			result = FlowDirections.Left;
		}
		else if ((directions & FlowDirections.Up) != FlowDirections.None)
		{
			result = FlowDirections.Down;
		}
		else if ((directions & FlowDirections.Down) != FlowDirections.None)
		{
			result = FlowDirections.Up;
		}
		return result;
	}

	public ConduitFlow(ConduitType conduit_type, int num_cells, IUtilityNetworkMgr network_mgr, float max_conduit_mass, float initial_elapsed_time)
	{
		elapsedTime = initial_elapsed_time;
		conduitType = conduit_type;
		networkMgr = network_mgr;
		MaxMass = max_conduit_mass;
		Initialize(num_cells);
		network_mgr.AddNetworksRebuiltListener(OnUtilityNetworksRebuilt);
	}

	public void Initialize(int num_cells)
	{
		grid = new GridNode[num_cells];
		for (int i = 0; i < num_cells; i++)
		{
			grid[i].conduitIdx = -1;
			grid[i].contents.element = SimHashes.Vacuum;
			grid[i].contents.diseaseIdx = byte.MaxValue;
		}
	}

	private void OnUtilityNetworksRebuilt(IList<UtilityNetwork> networks, ICollection<int> root_nodes)
	{
		RebuildConnections(root_nodes);
		int count = this.networks.Count - networks.Count;
		if (0 < this.networks.Count - networks.Count)
		{
			this.networks.RemoveRange(networks.Count, count);
		}
		Debug.Assert(this.networks.Count <= networks.Count);
		for (int i = 0; i != networks.Count; i++)
		{
			if (i < this.networks.Count)
			{
				this.networks[i] = new Network
				{
					network = (FlowUtilityNetwork)networks[i],
					cells = this.networks[i].cells
				};
				this.networks[i].cells.Clear();
			}
			else
			{
				this.networks.Add(new Network
				{
					network = (FlowUtilityNetwork)networks[i],
					cells = new List<int>()
				});
			}
		}
		build_network_job.Reset(this);
		foreach (Network network in this.networks)
		{
			build_network_job.Add(new BuildNetworkTask(network, soaInfo.NumEntries));
		}
		GlobalJobManager.Run(build_network_job);
		for (int j = 0; j != build_network_job.Count; j++)
		{
			build_network_job.GetWorkItem(j).Finish();
		}
	}

	private void RebuildConnections(IEnumerable<int> root_nodes)
	{
		ConnectContext shared_data = new ConnectContext(this);
		soaInfo.Clear(this);
		replacements.ExceptWith(root_nodes);
		ObjectLayer layer = ((conduitType == ConduitType.Gas) ? ObjectLayer.GasConduit : ObjectLayer.LiquidConduit);
		foreach (int root_node in root_nodes)
		{
			GameObject gameObject = Grid.Objects[root_node, (int)layer];
			if (!(gameObject == null))
			{
				global::Conduit component = gameObject.GetComponent<global::Conduit>();
				if (!(component != null) || !component.IsDisconnected())
				{
					int conduitIdx = soaInfo.AddConduit(this, gameObject, root_node);
					grid[root_node].conduitIdx = conduitIdx;
					shared_data.cells.Add(root_node);
				}
			}
		}
		Game.Instance.conduitTemperatureManager.Sim200ms(0f);
		connect_job.Reset(shared_data);
		int num = 256;
		for (int i = 0; i < shared_data.cells.Count; i += num)
		{
			connect_job.Add(new ConnectTask(i, Mathf.Min(i + num, shared_data.cells.Count)));
		}
		GlobalJobManager.Run(connect_job);
		shared_data.Finish();
		if (this.onConduitsRebuilt != null)
		{
			this.onConduitsRebuilt();
		}
	}

	private FlowDirections GetDirection(Conduit conduit, Conduit target_conduit)
	{
		Debug.Assert(conduit.idx != -1);
		Debug.Assert(target_conduit.idx != -1);
		ConduitConnections conduitConnections = soaInfo.GetConduitConnections(conduit.idx);
		if (conduitConnections.up == target_conduit.idx)
		{
			return FlowDirections.Up;
		}
		if (conduitConnections.down == target_conduit.idx)
		{
			return FlowDirections.Down;
		}
		if (conduitConnections.left == target_conduit.idx)
		{
			return FlowDirections.Left;
		}
		if (conduitConnections.right == target_conduit.idx)
		{
			return FlowDirections.Right;
		}
		return FlowDirections.None;
	}

	public int ComputeUpdateOrder(int cell)
	{
		foreach (Network network in networks)
		{
			int num = network.cells.IndexOf(cell);
			if (num != -1)
			{
				return num;
			}
		}
		return -1;
	}

	public ConduitContents GetContents(int cell)
	{
		ConduitContents contents = grid[cell].contents;
		GridNode gridNode = grid[cell];
		if (gridNode.conduitIdx != -1)
		{
			contents = soaInfo.GetConduit(gridNode.conduitIdx).GetContents(this);
		}
		if (contents.mass > 0f && contents.temperature <= 0f)
		{
			Debug.LogError($"unexpected temperature {contents.temperature}");
		}
		return contents;
	}

	public void SetContents(int cell, ConduitContents contents)
	{
		GridNode gridNode = grid[cell];
		if (gridNode.conduitIdx != -1)
		{
			soaInfo.GetConduit(gridNode.conduitIdx).SetContents(this, contents);
		}
		else
		{
			grid[cell].contents = contents;
		}
	}

	public static int GetCellFromDirection(int cell, FlowDirections direction)
	{
		return direction switch
		{
			FlowDirections.Left => Grid.CellLeft(cell), 
			FlowDirections.Right => Grid.CellRight(cell), 
			FlowDirections.Up => Grid.CellAbove(cell), 
			FlowDirections.Down => Grid.CellBelow(cell), 
			_ => -1, 
		};
	}

	public void Sim200ms(float dt)
	{
		if (dt <= 0f)
		{
			return;
		}
		elapsedTime += dt;
		if (elapsedTime < 1f)
		{
			return;
		}
		elapsedTime -= 1f;
		float obj = 1f;
		lastUpdateTime = Time.time;
		soaInfo.BeginFrame(this);
		ListPool<UpdateNetworkTask, ConduitFlow>.PooledList pooledList = ListPool<UpdateNetworkTask, ConduitFlow>.Allocate();
		pooledList.Capacity = Mathf.Max(pooledList.Capacity, networks.Count);
		foreach (Network network in networks)
		{
			pooledList.Add(new UpdateNetworkTask(network));
		}
		for (int i = 0; i != 4; i++)
		{
			if (pooledList.Count == 0)
			{
				break;
			}
			update_networks_job.Reset(this);
			foreach (UpdateNetworkTask item in pooledList)
			{
				update_networks_job.Add(item);
			}
			GlobalJobManager.Run(update_networks_job);
			pooledList.Clear();
			for (int j = 0; j != update_networks_job.Count; j++)
			{
				UpdateNetworkTask workItem = update_networks_job.GetWorkItem(j);
				if (workItem.continue_updating && i != 3)
				{
					pooledList.Add(workItem);
				}
				else
				{
					workItem.Finish(this);
				}
			}
		}
		pooledList.Recycle();
		if (dirtyConduitUpdaters)
		{
			conduitUpdaters.Sort((ConduitUpdater a, ConduitUpdater b) => a.priority - b.priority);
		}
		soaInfo.EndFrame(this);
		for (int num = 0; num < conduitUpdaters.Count; num++)
		{
			conduitUpdaters[num].callback(obj);
		}
	}

	private float ComputeMovableMass(GridNode grid_node)
	{
		ConduitContents contents = grid_node.contents;
		if (contents.element == SimHashes.Vacuum)
		{
			return 0f;
		}
		return contents.movable_mass;
	}

	private bool UpdateConduit(Conduit conduit)
	{
		bool result = false;
		int cell = soaInfo.GetCell(conduit.idx);
		GridNode grid_node = grid[cell];
		float num = ComputeMovableMass(grid_node);
		FlowDirections permittedFlowDirections = soaInfo.GetPermittedFlowDirections(conduit.idx);
		FlowDirections flowDirections = soaInfo.GetTargetFlowDirection(conduit.idx);
		if (num <= 0f)
		{
			for (int i = 0; i != 4; i++)
			{
				flowDirections = ComputeNextFlowDirection(flowDirections);
				if ((permittedFlowDirections & flowDirections) != FlowDirections.None)
				{
					Conduit conduitFromDirection = soaInfo.GetConduitFromDirection(conduit.idx, flowDirections);
					Debug.Assert(conduitFromDirection.idx != -1);
					FlowDirections srcFlowDirection = soaInfo.GetSrcFlowDirection(conduitFromDirection.idx);
					if ((srcFlowDirection & Opposite(flowDirections)) != FlowDirections.None)
					{
						soaInfo.SetPullDirection(conduitFromDirection.idx, flowDirections);
					}
				}
			}
		}
		else
		{
			for (int j = 0; j != 4; j++)
			{
				flowDirections = ComputeNextFlowDirection(flowDirections);
				if ((permittedFlowDirections & flowDirections) == 0)
				{
					continue;
				}
				Conduit conduitFromDirection2 = soaInfo.GetConduitFromDirection(conduit.idx, flowDirections);
				Debug.Assert(conduitFromDirection2.idx != -1);
				FlowDirections srcFlowDirection2 = soaInfo.GetSrcFlowDirection(conduitFromDirection2.idx);
				bool flag = (srcFlowDirection2 & Opposite(flowDirections)) != 0;
				if (srcFlowDirection2 != FlowDirections.None && !flag)
				{
					result = true;
					continue;
				}
				int cell2 = soaInfo.GetCell(conduitFromDirection2.idx);
				Debug.Assert(cell2 != -1);
				ConduitContents contents = grid[cell2].contents;
				bool flag2 = contents.element == SimHashes.Vacuum || contents.element == grid_node.contents.element;
				float effectiveCapacity = contents.GetEffectiveCapacity(MaxMass);
				bool flag3 = flag2 && effectiveCapacity > 0f;
				float num2 = Mathf.Min(num, effectiveCapacity);
				if (flag && flag3)
				{
					soaInfo.SetPullDirection(conduitFromDirection2.idx, flowDirections);
				}
				if (num2 <= 0f || !flag3)
				{
					continue;
				}
				soaInfo.SetTargetFlowDirection(conduit.idx, flowDirections);
				Debug.Assert(grid_node.contents.temperature > 0f);
				contents.temperature = GameUtil.GetFinalTemperature(grid_node.contents.temperature, num2, contents.temperature, contents.mass);
				contents.AddMass(num2);
				contents.element = grid_node.contents.element;
				float num3 = num2 / grid_node.contents.mass;
				int num4 = (int)(num3 * (float)grid_node.contents.diseaseCount);
				if (num4 != 0)
				{
					SimUtil.DiseaseInfo diseaseInfo = SimUtil.CalculateFinalDiseaseInfo(grid_node.contents.diseaseIdx, num4, contents.diseaseIdx, contents.diseaseCount);
					contents.diseaseIdx = diseaseInfo.idx;
					contents.diseaseCount = diseaseInfo.count;
				}
				grid[cell2].contents = contents;
				Debug.Assert(num2 <= grid_node.contents.mass);
				float num5 = grid_node.contents.mass - num2;
				num -= num2;
				if (num5 <= 0f)
				{
					Debug.Assert(num <= 0f);
					soaInfo.SetLastFlowInfo(conduit.idx, flowDirections, ref grid_node.contents);
					grid_node.contents = ConduitContents.Empty;
				}
				else
				{
					float num6 = num5 / grid_node.contents.mass;
					int num7 = (int)(num6 * (float)grid_node.contents.diseaseCount);
					Debug.Assert(num7 >= 0);
					ConduitContents contents2 = grid_node.contents;
					contents2.RemoveMass(num5);
					contents2.diseaseCount -= num7;
					grid_node.contents.RemoveMass(num2);
					grid_node.contents.diseaseCount = num7;
					if (num7 == 0)
					{
						grid_node.contents.diseaseIdx = byte.MaxValue;
					}
					soaInfo.SetLastFlowInfo(conduit.idx, flowDirections, ref contents2);
				}
				grid[cell].contents = grid_node.contents;
				result = 0f < ComputeMovableMass(grid_node);
				break;
			}
		}
		FlowDirections srcFlowDirection3 = soaInfo.GetSrcFlowDirection(conduit.idx);
		FlowDirections pullDirection = soaInfo.GetPullDirection(conduit.idx);
		if (srcFlowDirection3 == FlowDirections.None || (Opposite(srcFlowDirection3) & pullDirection) != FlowDirections.None)
		{
			soaInfo.SetPullDirection(conduit.idx, FlowDirections.None);
			soaInfo.SetSrcFlowDirection(conduit.idx, FlowDirections.None);
			for (int k = 0; k != 2; k++)
			{
				FlowDirections flowDirections2 = srcFlowDirection3;
				for (int l = 0; l != 4; l++)
				{
					flowDirections2 = ComputeNextFlowDirection(flowDirections2);
					Conduit conduitFromDirection3 = soaInfo.GetConduitFromDirection(conduit.idx, flowDirections2);
					if (conduitFromDirection3.idx == -1)
					{
						continue;
					}
					FlowDirections permittedFlowDirections2 = soaInfo.GetPermittedFlowDirections(conduitFromDirection3.idx);
					if ((permittedFlowDirections2 & Opposite(flowDirections2)) != FlowDirections.None)
					{
						int cell3 = soaInfo.GetCell(conduitFromDirection3.idx);
						ConduitContents contents3 = grid[cell3].contents;
						float num8 = ((k == 0) ? contents3.movable_mass : contents3.mass);
						if (0f < num8)
						{
							soaInfo.SetSrcFlowDirection(conduit.idx, flowDirections2);
							break;
						}
					}
				}
				if (soaInfo.GetSrcFlowDirection(conduit.idx) != FlowDirections.None)
				{
					break;
				}
			}
		}
		return result;
	}

	public float GetAmountAllowedForMerging(ConduitContents from, ConduitContents to, float massDesiredtoBeMoved)
	{
		return Mathf.Min(massDesiredtoBeMoved, MaxMass - to.mass);
	}

	public bool CanMergeContents(ConduitContents from, ConduitContents to, float massToMove)
	{
		if (from.element != to.element && to.element != SimHashes.Vacuum && massToMove > 0f)
		{
			return false;
		}
		float amountAllowedForMerging = GetAmountAllowedForMerging(from, to, massToMove);
		if (amountAllowedForMerging <= 0f)
		{
			return false;
		}
		return true;
	}

	public float AddElement(int cell_idx, SimHashes element, float mass, float temperature, byte disease_idx, int disease_count)
	{
		if (grid[cell_idx].conduitIdx == -1)
		{
			return 0f;
		}
		ConduitContents contents = GetConduit(cell_idx).GetContents(this);
		if (contents.element != element && contents.element != SimHashes.Vacuum && mass > 0f)
		{
			return 0f;
		}
		float num = Mathf.Min(mass, MaxMass - contents.mass);
		float num2 = num / mass;
		if (num <= 0f)
		{
			return 0f;
		}
		contents.temperature = GameUtil.GetFinalTemperature(temperature, num, contents.temperature, contents.mass);
		contents.AddMass(num);
		contents.element = element;
		contents.ConsolidateMass();
		int num3 = (int)(num2 * (float)disease_count);
		if (num3 > 0)
		{
			SimUtil.DiseaseInfo diseaseInfo = SimUtil.CalculateFinalDiseaseInfo(disease_idx, num3, contents.diseaseIdx, contents.diseaseCount);
			contents.diseaseIdx = diseaseInfo.idx;
			contents.diseaseCount = diseaseInfo.count;
		}
		SetContents(cell_idx, contents);
		return num;
	}

	public ConduitContents RemoveElement(int cell, float delta)
	{
		Conduit conduit = GetConduit(cell);
		return (conduit.idx != -1) ? RemoveElement(conduit, delta) : ConduitContents.Empty;
	}

	public ConduitContents RemoveElement(Conduit conduit, float delta)
	{
		ConduitContents contents = conduit.GetContents(this);
		float num = Mathf.Min(contents.mass, delta);
		float num2 = contents.mass - num;
		if (num2 <= 0f)
		{
			conduit.SetContents(this, ConduitContents.Empty);
			return contents;
		}
		ConduitContents result = contents;
		result.RemoveMass(num2);
		float num3 = num2 / contents.mass;
		int num4 = (int)(num3 * (float)contents.diseaseCount);
		result.diseaseCount = contents.diseaseCount - num4;
		ConduitContents contents2 = contents;
		contents2.RemoveMass(num);
		contents2.diseaseCount = num4;
		if (num4 <= 0)
		{
			contents2.diseaseIdx = byte.MaxValue;
			contents2.diseaseCount = 0;
		}
		conduit.SetContents(this, contents2);
		return result;
	}

	public FlowDirections GetPermittedFlow(int cell)
	{
		Conduit conduit = GetConduit(cell);
		if (conduit.idx == -1)
		{
			return FlowDirections.None;
		}
		return soaInfo.GetPermittedFlowDirections(conduit.idx);
	}

	public bool HasConduit(int cell)
	{
		return grid[cell].conduitIdx != -1;
	}

	public Conduit GetConduit(int cell)
	{
		int conduitIdx = grid[cell].conduitIdx;
		return (conduitIdx != -1) ? soaInfo.GetConduit(conduitIdx) : Conduit.Invalid;
	}

	private void DumpPipeContents(int cell, ConduitContents contents)
	{
		if (contents.element != SimHashes.Vacuum && contents.mass > 0f)
		{
			SimMessages.AddRemoveSubstance(cell, contents.element, CellEventLogger.Instance.ConduitFlowEmptyConduit, contents.mass, contents.temperature, contents.diseaseIdx, contents.diseaseCount);
			SetContents(cell, ConduitContents.Empty);
		}
	}

	public void EmptyConduit(int cell)
	{
		if (!replacements.Contains(cell))
		{
			DumpPipeContents(cell, grid[cell].contents);
		}
	}

	public void MarkForReplacement(int cell)
	{
		replacements.Add(cell);
	}

	public void DeactivateCell(int cell)
	{
		grid[cell].conduitIdx = -1;
		SetContents(cell, ConduitContents.Empty);
	}

	[Conditional("CHECK_NAN")]
	private void Validate(ConduitContents contents)
	{
		Assert.IsTrue(!float.IsNaN(contents.temperature));
		Assert.IsTrue(!float.IsPositiveInfinity(contents.temperature));
		Assert.IsTrue(!float.IsNegativeInfinity(contents.temperature));
		Assert.IsTrue(contents.mass == 0f || contents.temperature > 0f);
		if (contents.mass > 0f && contents.temperature <= 0f)
		{
			Debug.LogError("zero degree pipe contents");
		}
	}

	[OnSerializing]
	private void OnSerializing()
	{
		int numEntries = soaInfo.NumEntries;
		if (numEntries > 0)
		{
			versionedSerializedContents = new SerializedContents[numEntries];
			serializedIdx = new int[numEntries];
			for (int i = 0; i < numEntries; i++)
			{
				Conduit conduit = soaInfo.GetConduit(i);
				ConduitContents contents = conduit.GetContents(this);
				serializedIdx[i] = soaInfo.GetCell(conduit.idx);
				versionedSerializedContents[i] = new SerializedContents(contents);
			}
		}
		else
		{
			serializedContents = null;
			versionedSerializedContents = null;
			serializedIdx = null;
		}
	}

	[OnSerialized]
	private void OnSerialized()
	{
		versionedSerializedContents = null;
		serializedContents = null;
		serializedIdx = null;
	}

	[OnDeserialized]
	private void OnDeserialized()
	{
		if (this.serializedContents != null)
		{
			versionedSerializedContents = new SerializedContents[this.serializedContents.Length];
			for (int i = 0; i < this.serializedContents.Length; i++)
			{
				versionedSerializedContents[i] = new SerializedContents(this.serializedContents[i]);
			}
			this.serializedContents = null;
		}
		if (versionedSerializedContents == null)
		{
			return;
		}
		for (int j = 0; j < versionedSerializedContents.Length; j++)
		{
			int num = serializedIdx[j];
			SerializedContents serializedContents = versionedSerializedContents[j];
			ConduitContents contents = ((serializedContents.mass <= 0f) ? ConduitContents.Empty : new ConduitContents(serializedContents.element, Math.Min(MaxMass, serializedContents.mass), serializedContents.temperature, byte.MaxValue, 0));
			if (0 < serializedContents.diseaseCount || serializedContents.diseaseHash != 0)
			{
				contents.diseaseIdx = Db.Get().Diseases.GetIndex(serializedContents.diseaseHash);
				contents.diseaseCount = ((contents.diseaseIdx != byte.MaxValue) ? serializedContents.diseaseCount : 0);
			}
			if (float.IsNaN(contents.temperature) || (contents.temperature <= 0f && contents.element != SimHashes.Vacuum) || 10000f < contents.temperature)
			{
				Vector2I vector2I = Grid.CellToXY(num);
				DeserializeWarnings.Instance.PipeContentsTemperatureIsNan.Warn($"Invalid pipe content temperature of {contents.temperature} detected. Resetting temperature. (x={vector2I.x}, y={vector2I.y}, cell={num})");
				contents.temperature = ElementLoader.FindElementByHash(contents.element).defaultValues.temperature;
			}
			SetContents(num, contents);
		}
		versionedSerializedContents = null;
		this.serializedContents = null;
		serializedIdx = null;
	}

	public UtilityNetwork GetNetwork(Conduit conduit)
	{
		int cell = soaInfo.GetCell(conduit.idx);
		return networkMgr.GetNetworkForCell(cell);
	}

	public void ForceRebuildNetworks()
	{
		networkMgr.ForceRebuildNetworks();
	}

	public bool IsConduitFull(int cell_idx)
	{
		ConduitContents contents = grid[cell_idx].contents;
		return MaxMass - contents.mass <= 0f;
	}

	public bool IsConduitEmpty(int cell_idx)
	{
		ConduitContents contents = grid[cell_idx].contents;
		return contents.mass <= 0f;
	}

	public void FreezeConduitContents(int conduit_idx)
	{
		GameObject conduitGO = soaInfo.GetConduitGO(conduit_idx);
		if (conduitGO != null && soaInfo.GetConduit(conduit_idx).GetContents(this).mass > MaxMass * 0.1f)
		{
			conduitGO.Trigger(-700727624);
		}
	}

	public void MeltConduitContents(int conduit_idx)
	{
		GameObject conduitGO = soaInfo.GetConduitGO(conduit_idx);
		if (conduitGO != null && soaInfo.GetConduit(conduit_idx).GetContents(this).mass > MaxMass * 0.1f)
		{
			conduitGO.Trigger(-1152799878);
		}
	}
}
