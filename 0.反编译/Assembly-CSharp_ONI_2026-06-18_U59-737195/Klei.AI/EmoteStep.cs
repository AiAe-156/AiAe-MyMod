using System;
using UnityEngine;

namespace Klei.AI;

public class EmoteStep
{
	public struct Callbacks
	{
		public Action<GameObject> StartedCb;

		public Action<GameObject> FinishedCb;
	}

	public HashedString anim = HashedString.Invalid;

	public KAnim.PlayMode mode = KAnim.PlayMode.Once;

	public float timeout = -1f;

	private HandleVector<Callbacks> callbacks = new HandleVector<Callbacks>(64);

	public int Id => anim.HashValue;

	public HandleVector<Callbacks>.Handle RegisterCallbacks(Action<GameObject> startedCb, Action<GameObject> finishedCb)
	{
		if (startedCb == null && finishedCb == null)
		{
			return HandleVector<Callbacks>.InvalidHandle;
		}
		Callbacks item = new Callbacks
		{
			StartedCb = startedCb,
			FinishedCb = finishedCb
		};
		return callbacks.Add(item);
	}

	public void UnregisterCallbacks(HandleVector<Callbacks>.Handle callbackHandle)
	{
		callbacks.Release(callbackHandle);
	}

	public void UnregisterAllCallbacks()
	{
		callbacks = new HandleVector<Callbacks>(64);
	}

	public void OnStepStarted(HandleVector<Callbacks>.Handle callbackHandle, GameObject parameter)
	{
		if (!(callbackHandle == HandleVector<Callbacks>.Handle.InvalidHandle))
		{
			Callbacks item = callbacks.GetItem(callbackHandle);
			if (item.StartedCb != null)
			{
				item.StartedCb(parameter);
			}
		}
	}

	public void OnStepFinished(HandleVector<Callbacks>.Handle callbackHandle, GameObject parameter)
	{
		if (!(callbackHandle == HandleVector<Callbacks>.Handle.InvalidHandle))
		{
			Callbacks item = callbacks.GetItem(callbackHandle);
			if (item.FinishedCb != null)
			{
				item.FinishedCb(parameter);
			}
		}
	}
}
