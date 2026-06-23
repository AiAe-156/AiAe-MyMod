using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilLibs.UI.FUI.Unity_UI_Extensions.Scripts.Controls.ReorderableList;

[DisallowMultipleComponent]
public class ReorderableListContent : MonoBehaviour
{
	private List<Transform> _cachedChildren;

	private List<ReorderableListElement> _cachedListElement;

	private ReorderableListElement _ele;

	private ReorderableList _extList;

	private RectTransform _rect;

	private bool _started = false;

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
		for (int i2 = _cachedChildren.Count - 1; i2 >= 0; i2--)
		{
			if ((Object)(object)_cachedChildren[i2] == (Object)null)
			{
				_cachedChildren.RemoveAt(i2);
				_cachedListElement.RemoveAt(i2);
			}
		}
	}
}
