using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class KCollider2D : KMonoBehaviour, IRenderEveryTick
{
	[SerializeField]
	public Vector2 _offset;

	private Extents cachedExtents;

	private HandleVector<int>.Handle partitionerEntry;

	private ulong movementStateChangedHandlerId;

	private static Action<Transform, bool, object> OnMovementStateChangedDispatcher = delegate(Transform transform, bool is_moving, object context)
	{
		Unsafe.As<KCollider2D>(context).OnMovementStateChanged(is_moving);
	};

	public Vector2 offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = value;
			MarkDirty();
		}
	}

	public abstract Bounds bounds { get; }

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		autoRegisterSimRender = false;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		movementStateChangedHandlerId = Singleton<CellChangeMonitor>.Instance.RegisterMovementStateChanged(base.transform, OnMovementStateChangedDispatcher, this);
		MarkDirty(force: true);
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		Singleton<CellChangeMonitor>.Instance.UnregisterMovementStateChanged(ref movementStateChangedHandlerId);
		GameScenePartitioner.Instance.Free(ref partitionerEntry);
	}

	public void MarkDirty(bool force = false)
	{
		bool flag = force || partitionerEntry.IsValid();
		if (!flag)
		{
			return;
		}
		Extents extents = GetExtents();
		if (force || cachedExtents.x != extents.x || cachedExtents.y != extents.y || cachedExtents.width != extents.width || cachedExtents.height != extents.height)
		{
			cachedExtents = extents;
			GameScenePartitioner.Instance.Free(ref partitionerEntry);
			if (flag)
			{
				partitionerEntry = GameScenePartitioner.Instance.Add(null, this, cachedExtents, GameScenePartitioner.Instance.collisionLayer, null);
			}
		}
	}

	private void OnMovementStateChanged(bool is_moving)
	{
		if (is_moving)
		{
			MarkDirty();
			SimAndRenderScheduler.instance.Add(this);
		}
		else
		{
			SimAndRenderScheduler.instance.Remove(this);
		}
	}

	public void RenderEveryTick(float dt)
	{
		MarkDirty();
	}

	public abstract bool Intersects(Vector2 pos);

	public abstract Extents GetExtents();
}
