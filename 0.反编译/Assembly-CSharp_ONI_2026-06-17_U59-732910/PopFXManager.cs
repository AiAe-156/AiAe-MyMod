using System.Collections.Generic;
using Klei;
using UnityEngine;

public class PopFXManager : KScreen
{
	public static PopFXManager Instance;

	private GameObject Prefab_PopFxGroup;

	public GameObject Prefab_PopFX;

	public List<PopFX> Pool = new List<PopFX>();

	public List<PopFxGroup> GroupPool = new List<PopFxGroup>();

	public Dictionary<int, PopFxGroup> AliveGroups = new Dictionary<int, PopFxGroup>();

	public Sprite sprite_Plus;

	public Sprite sprite_Negative;

	public Sprite sprite_Resource;

	public Sprite sprite_Building;

	public Sprite sprite_Research;

	private bool ready;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
		Prefab_PopFxGroup = new GameObject("Prefab_PopFxGroup");
		Prefab_PopFxGroup.AddComponent<PopFxGroup>();
		Prefab_PopFxGroup.transform.SetParent(Prefab_PopFX.transform.parent);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		ready = true;
		if (!GenericGameSettings.instance.disablePopFx)
		{
			for (int i = 0; i < 20; i++)
			{
				PopFxGroup item = CreatePopFxGroup();
				PopFX item2 = CreatePopFX();
				Pool.Add(item2);
				GroupPool.Add(item);
			}
		}
	}

	public bool Ready()
	{
		return ready;
	}

	public PopFX SpawnFX(Sprite mainIcon, string text, Transform target_transform, float lifetime = 1.5f, bool track_target = false)
	{
		return SpawnFX(mainIcon, text, target_transform, Vector3.zero, lifetime, track_target);
	}

	public PopFX SpawnFX(Sprite mainIcon, string text, Transform target_transform, Vector3 offset, float lifetime = 1.5f, bool track_target = false, bool force_spawn = false)
	{
		return SpawnFX(mainIcon, null, text, target_transform, offset, lifetime, selfAdjustPositionIfInGroup: true, track_target, force_spawn);
	}

	public PopFX SpawnFX(Sprite mainIcon, Sprite secondaryIcon, string text, Transform target_transform, Vector3 offset, float lifetime = 1.5f, bool selfAdjustPositionIfInGroup = true, bool track_target = false, bool force_spawn = false)
	{
		if (GenericGameSettings.instance.disablePopFx)
		{
			return null;
		}
		if (Game.IsQuitting())
		{
			return null;
		}
		Vector3 pos = offset;
		if (target_transform != null)
		{
			pos += target_transform.GetPosition();
		}
		int num = Grid.PosToCell(pos);
		if (!force_spawn && (!Grid.IsValidCell(num) || !Grid.IsVisible(num) || (CameraController.Instance != null && !CameraController.Instance.IsVisiblePosExtended(pos))))
		{
			return null;
		}
		PopFX orCreatePopFX = GetOrCreatePopFX(mainIcon, secondaryIcon, text, target_transform, offset, selfAdjustPositionIfInGroup, lifetime, track_target);
		if (!AliveGroups.TryGetValue(num, out var value) || value == null)
		{
			if (GroupPool.Count > 0)
			{
				value = GroupPool[0];
				GroupPool[0].gameObject.SetActive(value: true);
				GroupPool.RemoveAt(0);
			}
			else
			{
				value = CreatePopFxGroup();
				value.gameObject.SetActive(value: true);
			}
			AliveGroups.Add(num, value);
		}
		value.Enqueue(orCreatePopFX);
		value.WakeUp(num);
		return orCreatePopFX;
	}

	private PopFX GetOrCreatePopFX(Sprite mainIcon, Sprite secondaryIcon, string text, Transform target_transform, Vector3 offset, bool selfAdjustPositionIfInGroup = true, float lifetime = 1.5f, bool track_target = false)
	{
		PopFX popFX = null;
		if (Pool.Count > 0)
		{
			popFX = Pool[0];
			Pool[0].Setup(mainIcon, secondaryIcon, text, target_transform, offset, selfAdjustPositionIfInGroup, lifetime, track_target);
			Pool.RemoveAt(0);
		}
		else
		{
			popFX = CreatePopFX();
			popFX.Setup(mainIcon, secondaryIcon, text, target_transform, offset, selfAdjustPositionIfInGroup, lifetime, track_target);
		}
		return popFX;
	}

	private PopFX CreatePopFX()
	{
		_ = Prefab_PopFX.gameObject.activeInHierarchy;
		GameObject obj = Util.KInstantiate(Prefab_PopFX, base.gameObject, "Pooled_PopFX");
		obj.transform.localScale = Vector3.one;
		return obj.GetComponent<PopFX>();
	}

	private PopFxGroup CreatePopFxGroup()
	{
		GameObject obj = Util.KInstantiate(Prefab_PopFxGroup, base.gameObject, "Pooled_PopFxGroup");
		obj.transform.localScale = Vector3.one;
		return obj.GetComponent<PopFxGroup>();
	}

	public void RecycleFX(PopFX fx)
	{
		Pool.Add(fx);
	}

	public void RecycleFxGroup(int key, PopFxGroup fx)
	{
		AliveGroups.Remove(key);
		GroupPool.Add(fx);
	}
}
