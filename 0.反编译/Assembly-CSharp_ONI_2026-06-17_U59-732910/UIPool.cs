using System;
using System.Collections.Generic;
using UnityEngine;

public class UIPool<T> where T : MonoBehaviour
{
	private T prefab;

	private Stack<T> freeElements;

	private List<T> activeElements;

	public Transform disabledElementParent;

	public int ActiveElementsCount => activeElements.Count;

	public int FreeElementsCount => freeElements.Count;

	public int TotalElementsCount => ActiveElementsCount + FreeElementsCount;

	public UIPool(T prefab)
	{
		this.prefab = prefab;
		freeElements = new Stack<T>();
		activeElements = new List<T>();
	}

	public T GetFreeElement(GameObject instantiateParent = null, bool forceActive = false)
	{
		T val;
		if (freeElements.Count == 0)
		{
			val = Util.KInstantiateUI<T>(prefab.gameObject, instantiateParent);
		}
		else
		{
			val = freeElements.Pop();
			if (val.transform.parent != instantiateParent)
			{
				val.transform.SetParent(instantiateParent?.transform);
			}
		}
		if (val.gameObject.activeInHierarchy != forceActive)
		{
			val.gameObject.SetActive(forceActive);
		}
		activeElements.Add(val);
		return val;
	}

	public void ClearElement(T element)
	{
		if (!activeElements.Contains(element))
		{
			Debug.LogError(freeElements.Contains(element) ? "The element provided is already inactive" : "The element provided does not belong to this pool");
			return;
		}
		if (disabledElementParent != null)
		{
			element.transform.SetParent(disabledElementParent);
		}
		element.gameObject.SetActive(value: false);
		freeElements.Push(element);
		activeElements.Remove(element);
	}

	public void ClearAll()
	{
		for (int num = activeElements.Count - 1; num >= 0; num--)
		{
			T val = activeElements[num];
			val.gameObject.SetActive(value: false);
			if (disabledElementParent != null)
			{
				val.transform.SetParent(disabledElementParent);
			}
			freeElements.Push(val);
		}
		activeElements.Clear();
	}

	public void DestroyAll()
	{
		DestroyAllActive();
		DestroyAllFree();
	}

	public void DestroyAllActive()
	{
		foreach (T activeElement in activeElements)
		{
			UnityEngine.Object.Destroy(activeElement.gameObject);
		}
		activeElements.Clear();
	}

	public void DestroyAllFree()
	{
		foreach (T freeElement in freeElements)
		{
			UnityEngine.Object.Destroy(freeElement.gameObject);
		}
		freeElements.Clear();
	}

	public void ForEachActiveElement(Action<T> predicate)
	{
		for (int i = 0; i < activeElements.Count; i++)
		{
			predicate(activeElements[i]);
		}
	}

	public void ForEachFreeElement(Action<T> predicate)
	{
		foreach (T freeElement in freeElements)
		{
			predicate(freeElement);
		}
	}
}
