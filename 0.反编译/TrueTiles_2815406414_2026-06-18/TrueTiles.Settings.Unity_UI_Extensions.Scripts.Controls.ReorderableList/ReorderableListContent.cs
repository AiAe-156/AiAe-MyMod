using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrueTiles.Settings.Unity_UI_Extensions.Scripts.Controls.ReorderableList;

[DisallowMultipleComponent]
public class ReorderableListContent : MonoBehaviour
{
	private List<Transform> _cachedChildren;

	private List<ReorderableListElement> _cachedListElement;

	private ReorderableListElement _ele;

	private ReorderableList _extList;

	private RectTransform _rect;

	private bool _started;

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)_rect))
		{
			((MonoBehaviour)this).StartCoroutine(RefreshChildren());
		}
	}

	public void OnTransformChildrenChanged()
	{
		if (((Behaviour)this).isActiveAndEnabled)
		{
			((MonoBehaviour)this).StartCoroutine(RefreshChildren());
		}
	}

	public void Init(ReorderableList extList)
	{
		if (_started)
		{
			((MonoBehaviour)this).StopCoroutine(RefreshChildren());
		}
		_extList = extList;
		_rect = ((Component)this).GetComponent<RectTransform>();
		_cachedChildren = new List<Transform>();
		_cachedListElement = new List<ReorderableListElement>();
		((MonoBehaviour)this).StartCoroutine(RefreshChildren());
		_started = true;
	}

	private IEnumerator RefreshChildren()
	{
		for (int i = 0; i < ((Transform)_rect).childCount; i++)
		{
			if (!_cachedChildren.Contains(((Transform)_rect).GetChild(i)))
			{
				_ele = ((Component)((Transform)_rect).GetChild(i)).gameObject.GetComponent<ReorderableListElement>() ?? ((Component)((Transform)_rect).GetChild(i)).gameObject.AddComponent<ReorderableListElement>();
				_ele.Init(_extList);
				_cachedChildren.Add(((Transform)_rect).GetChild(i));
				_cachedListElement.Add(_ele);
			}
		}
		yield return 0;
		for (int num = _cachedChildren.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)_cachedChildren[num] == (Object)null)
			{
				_cachedChildren.RemoveAt(num);
				_cachedListElement.RemoveAt(num);
			}
		}
	}
}
