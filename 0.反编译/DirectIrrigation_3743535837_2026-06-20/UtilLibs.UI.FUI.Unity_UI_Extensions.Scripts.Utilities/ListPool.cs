using System.Collections.Generic;
using UnityEngine.Events;

namespace UtilLibs.UI.FUI.Unity_UI_Extensions.Scripts.Utilities;

internal static class ListPool<T>
{
	private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, (UnityAction<List<T>>)(object)new UnityAction<List<List<T>>>(Clear));

	private static void Clear(List<T> l)
	{
		l.Clear();
	}

	public static List<T> Get()
	{
		return s_ListPool.Get();
	}

	public static void Release(List<T> toRelease)
	{
		s_ListPool.Release(toRelease);
	}
}
