using System;
using System.Collections.Generic;
using System.Reflection;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;
using UnityEngine;

namespace PeterHan.PLib.Buildings;

/// <summary>
/// A visualizer that colors cells with an overlay when a building is selected or being
/// previewed.
/// </summary>
public abstract class ColoredRangeVisualizer : KMonoBehaviour
{
	/// <summary>
	/// TODO Remove when versions prior to U57-699077 no longer need to be supported
	/// </summary>
	private delegate ulong RegisterCellChangedHandlerNew(CellChangeMonitor instance, Transform transform, Action<object> callback);

	private delegate int RegisterCellChangedHandlerOld(CellChangeMonitor instance, Transform transform, Action callback, string handler);

	private delegate void UnregisterCellChangedHandlerNew(CellChangeMonitor instance, ref ulong id);

	private delegate void UnregisterCellChangedHandlerOld(CellChangeMonitor instance, Transform transform, Action callback);

	/// <summary>
	/// Stores the data about a particular cell, including its anim controller and tint
	/// color.
	/// </summary>
	protected sealed class VisCellData : IComparable<VisCellData>
	{
		/// <summary>
		/// The target cell.
		/// </summary>
		public int Cell { get; }

		/// <summary>
		/// The anim controller for this cell.
		/// </summary>
		public KBatchedAnimController Controller { get; private set; }

		/// <summary>
		/// The tint used for this cell.
		/// </summary>
		public Color Tint { get; }

		/// <summary>
		/// Creates a visualized cell.
		/// </summary>
		/// <param name="cell">The cell to visualize.</param>
		public VisCellData(int cell)
			: this(cell, Color.white)
		{
		}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


		/// <summary>
		/// Creates a visualized cell.
		/// </summary>
		/// <param name="cell">The cell to visualize.</param>
		/// <param name="tint">The color to tint it.</param>
		public VisCellData(int cell, Color tint)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Cell = cell;
			Controller = null;
			Tint = tint;
		}

		public int CompareTo(VisCellData other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			return Cell.CompareTo(other.Cell);
		}

		public void CreateController(SceneLayer sceneLayer)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			Controller = FXHelpers.CreateEffect("transferarmgrid_kanim", Grid.CellToPosCCC(Cell, sceneLayer), (Transform)null, false, sceneLayer, true);
			((KAnimControllerBase)Controller).destroyOnAnimComplete = false;
			((KAnimControllerBase)Controller).visibilityType = (VisibilityType)2;
			((Component)Controller).gameObject.SetActive(true);
			((KAnimControllerBase)Controller).Play(PRE_ANIMS, (PlayMode)0);
			((KAnimControllerBase)Controller).TintColour = Color32.op_Implicit(Tint);
		}

		/// <summary>
		/// Destroys the anim controller for this cell.
		/// </summary>
		public void Destroy()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Controller != (Object)null)
			{
				((KAnimControllerBase)Controller).destroyOnAnimComplete = true;
				((KAnimControllerBase)Controller).Play(POST_ANIM, (PlayMode)1, 1f, 0f);
				Controller = null;
			}
		}

		public override bool Equals(object obj)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (obj is VisCellData visCellData && visCellData.Cell == Cell)
			{
				Color tint = Tint;
				return ((Color)(ref tint)).Equals(visCellData.Tint);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Cell;
		}

		public override string ToString()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			return "CellData[cell={0:D},color={1}]".F(Cell, Tint);
		}
	}

	private static readonly RegisterCellChangedHandlerNew REGISTER_NEW;

	private static readonly DetouredMethod<RegisterCellChangedHandlerOld> REGISTER_OLD;

	private static readonly UnregisterCellChangedHandlerNew UNREGISTER_NEW;

	private static readonly DetouredMethod<UnregisterCellChangedHandlerOld> UNREGISTER_OLD;

	/// <summary>
	/// The anim name to use when visualizing.
	/// </summary>
	private const string ANIM_NAME = "transferarmgrid_kanim";

	/// <summary>
	/// The animations to play when the visualization is created.
	/// </summary>
	private static readonly HashedString[] PRE_ANIMS;

	/// <summary>
	/// The animation to play when the visualization is destroyed.
	/// </summary>
	private static readonly HashedString POST_ANIM;

	[MyCmpGet]
	protected BuildingPreview preview;

	[MyCmpGet]
	protected Rotatable rotatable;

	/// <summary>
	/// The cells where animations are being displayed.
	/// </summary>
	private readonly HashSet<VisCellData> cells;

	/// <summary>
	/// More efficiently unregister handles to events.
	/// </summary>
	private ulong handlerID;

	/// <summary>
	/// The layer on which to display the visualizer.
	/// </summary>
	public SceneLayer Layer { get; set; }

	static ColoredRangeVisualizer()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		REGISTER_OLD = typeof(CellChangeMonitor).DetourLazy<RegisterCellChangedHandlerOld>("RegisterCellChangedHandler");
		UNREGISTER_OLD = typeof(CellChangeMonitor).DetourLazy<UnregisterCellChangedHandlerOld>("UnregisterCellChangedHandler");
		PRE_ANIMS = (HashedString[])(object)new HashedString[2]
		{
			HashedString.op_Implicit("grid_pre"),
			HashedString.op_Implicit("grid_loop")
		};
		POST_ANIM = HashedString.op_Implicit("grid_pst");
		MethodInfo methodSafe = typeof(CellChangeMonitor).GetMethodSafe("RegisterCellChangedHandler", false, typeof(Transform), typeof(Action<object>));
		if (methodSafe != null)
		{
			REGISTER_NEW = methodSafe.Detour<RegisterCellChangedHandlerNew>();
			UNREGISTER_NEW = typeof(CellChangeMonitor).Detour<UnregisterCellChangedHandlerNew>("UnregisterCellChangedHandler");
		}
		else
		{
			REGISTER_NEW = null;
			UNREGISTER_NEW = null;
		}
	}

	protected ColoredRangeVisualizer()
	{
		cells = new HashSet<VisCellData>();
		handlerID = 0uL;
		Layer = (SceneLayer)32;
	}

	/// <summary>
	/// Creates or updates the visualizers as necessary.
	/// </summary>
	private void CreateVisualizers()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		PooledHashSet<VisCellData, ColoredRangeVisualizer> val = HashSetPool<VisCellData, ColoredRangeVisualizer>.Allocate();
		PooledList<VisCellData, ColoredRangeVisualizer> val2 = ListPool<VisCellData, ColoredRangeVisualizer>.Allocate();
		try
		{
			if ((Object)(object)((Component)this).gameObject != (Object)null)
			{
				VisualizeCells((ICollection<VisCellData>)val);
			}
			foreach (VisCellData cell in cells)
			{
				if (((HashSet<VisCellData>)(object)val).Remove(cell))
				{
					((List<VisCellData>)(object)val2).Add(cell);
				}
				else
				{
					cell.Destroy();
				}
			}
			foreach (VisCellData item in (HashSet<VisCellData>)(object)val)
			{
				item.CreateController(Layer);
				((List<VisCellData>)(object)val2).Add(item);
			}
			cells.Clear();
			foreach (VisCellData item2 in (List<VisCellData>)(object)val2)
			{
				cells.Add(item2);
			}
		}
		finally
		{
			val.Recycle();
			val2.Recycle();
		}
	}

	/// <summary>
	/// Called when cells are changed in the building radius.
	/// </summary>
	private void OnCellChange(object _)
	{
		CreateVisualizers();
	}

	protected override void OnCleanUp()
	{
		((KMonoBehaviour)this).Unsubscribe(-1503271301);
		if ((Object)(object)preview != (Object)null)
		{
			UnregisterCellChangedHandler();
			if ((Object)(object)rotatable != (Object)null)
			{
				((KMonoBehaviour)this).Unsubscribe(-1643076535);
			}
		}
		RemoveVisualizers();
		((KMonoBehaviour)this).OnCleanUp();
	}

	/// <summary>
	/// Called when the object is rotated.
	/// </summary>
	private void OnRotated(object _)
	{
		CreateVisualizers();
	}

	/// <summary>
	/// Called when the object is selected.
	/// </summary>
	/// <param name="data">true if selected, or false if deselected.</param>
	private void OnSelect(object data)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (data is bool flag)
		{
			Vector3 position = ((KMonoBehaviour)this).transform.position;
			if (flag)
			{
				PGameUtils.PlaySound("RadialGrid_form", position);
				CreateVisualizers();
			}
			else
			{
				PGameUtils.PlaySound("RadialGrid_disappear", position);
				RemoveVisualizers();
			}
		}
	}

	protected override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		((KMonoBehaviour)this).Subscribe(-1503271301, (Action<object>)OnSelect);
		if ((Object)(object)preview != (Object)null)
		{
			RegisterCellChangedHandler();
			if ((Object)(object)rotatable != (Object)null)
			{
				((KMonoBehaviour)this).Subscribe(-1643076535, (Action<object>)OnRotated);
			}
		}
	}

	/// <summary>
	/// Adds the cell changed handler.
	/// </summary>
	private void RegisterCellChangedHandler()
	{
		if (REGISTER_NEW != null)
		{
			handlerID = REGISTER_NEW(Singleton<CellChangeMonitor>.Instance, ((KMonoBehaviour)this).transform, OnCellChange);
		}
		else
		{
			REGISTER_OLD.Invoke(Singleton<CellChangeMonitor>.Instance, ((KMonoBehaviour)this).transform, CreateVisualizers, "ColoredRangeVisualizer.OnSpawn");
		}
	}

	/// <summary>
	/// Removes all of the visualizers.
	/// </summary>
	private void RemoveVisualizers()
	{
		foreach (VisCellData cell in cells)
		{
			cell.Destroy();
		}
		cells.Clear();
	}

	protected int RotateOffsetCell(int baseCell, CellOffset offset)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rotatable != (Object)null)
		{
			offset = rotatable.GetRotatedCellOffset(offset);
		}
		return Grid.OffsetCell(baseCell, offset);
	}

	/// <summary>
	/// Removes the cell changed handler.
	/// </summary>
	private void UnregisterCellChangedHandler()
	{
		if (UNREGISTER_NEW != null)
		{
			UNREGISTER_NEW(Singleton<CellChangeMonitor>.Instance, ref handlerID);
		}
		else
		{
			UNREGISTER_OLD.Invoke(Singleton<CellChangeMonitor>.Instance, ((KMonoBehaviour)this).transform, CreateVisualizers);
		}
	}

	/// <summary>
	/// Called when cell visualizations need to be updated. Visualized cells should be
	/// added to the collection supplied as an argument.
	/// </summary>
	/// <param name="newCells">The cells which should be visualized.</param>
	protected abstract void VisualizeCells(ICollection<VisCellData> newCells);
}
