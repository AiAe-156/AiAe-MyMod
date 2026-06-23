using System;
using UnityEngine;

public struct FallerComponent
{
	public Transform transform;

	public int transformInstanceId;

	public bool isFalling;

	public float offset;

	public Vector2 initialVelocity;

	public HandleVector<int>.Handle partitionerEntry;

	public Action<object> solidChangedCB;

	public Action<object> cellChangedCB;

	public ulong cellChangedHandlerID;

	public FallerComponent(Transform transform, Vector2 initial_velocity)
	{
		this.transform = transform;
		transformInstanceId = transform.GetInstanceID();
		isFalling = false;
		initialVelocity = initial_velocity;
		partitionerEntry = default(HandleVector<int>.Handle);
		solidChangedCB = null;
		cellChangedCB = null;
		cellChangedHandlerID = 0uL;
		KCircleCollider2D component = transform.GetComponent<KCircleCollider2D>();
		if (component != null)
		{
			offset = component.radius;
			return;
		}
		KCollider2D component2 = transform.GetComponent<KCollider2D>();
		if (component2 != null)
		{
			offset = transform.GetPosition().y - component2.bounds.min.y;
		}
		else
		{
			offset = 0f;
		}
	}
}
