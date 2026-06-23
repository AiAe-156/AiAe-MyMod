using System.Collections.Generic;
using UnityEngine;

public class PopFxGroup : KMonoBehaviour
{
	public static readonly Vector3 INVALID_SPAWN_POSITION = Vector3.one * -1f;

	public const float INVALID_PADDING = -1f;

	public const float SPAWN_COOLDOWN = 0.1f;

	public const float MAX_PADDING_MULTIPLIER = 2f;

	public const int MAX_ITEM_COUNT_PADDING = 3;

	public const float INDIVIDUAL_PADDING = 1f;

	public Queue<PopFX> spawnQueue = new Queue<PopFX>();

	private float padding = -1f;

	private int lastKeyUsed = -1;

	private bool isLive = false;

	private float lastSpawnTimeStamp = float.MinValue;

	private Vector3 spawnPosition = INVALID_SPAWN_POSITION;

	public void WakeUp(int key)
	{
		if (!isLive)
		{
			isLive = true;
			lastSpawnTimeStamp = float.MinValue;
			lastKeyUsed = key;
		}
	}

	public void Enqueue(PopFX effect)
	{
		spawnQueue.Enqueue(effect);
	}

	public void Update()
	{
		if (!isLive || !PopFXManager.Instance.Ready())
		{
			return;
		}
		float num = Time.unscaledTime - lastSpawnTimeStamp;
		if (!(num >= 0.1f))
		{
			return;
		}
		padding = ((padding == -1f) ? ((float)(Mathf.Min(spawnQueue.Count, 3) - 1) * 1f) : Mathf.Max(padding - 1f, 0f));
		PopFX popFX = ((spawnQueue.Count > 0) ? spawnQueue.Dequeue() : null);
		if (popFX != null)
		{
			if (spawnPosition == INVALID_SPAWN_POSITION)
			{
				spawnPosition = popFX.StartPos;
			}
			popFX.Run(spawnPosition, Vector3.up * padding);
			lastSpawnTimeStamp = Time.unscaledTime;
		}
		else
		{
			Recycle();
		}
	}

	public void Recycle()
	{
		isLive = false;
		lastSpawnTimeStamp = float.MinValue;
		spawnPosition = INVALID_SPAWN_POSITION;
		padding = -1f;
		while (spawnQueue.Count > 0)
		{
			PopFX popFX = spawnQueue.Dequeue();
			popFX.Recycle();
		}
		PopFXManager.Instance.RecycleFxGroup(lastKeyUsed, this);
		lastKeyUsed = -1;
		base.gameObject.SetActive(value: false);
	}
}
