using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShelfDisplay : KMonoBehaviour
{
	[MyCmpReq]
	private SymbolOverrideController symbolOverrideController;

	[MyCmpReq]
	private KBatchedAnimController kbac;

	[MyCmpReq]
	private Storage storage;

	private KBatchedAnimController[] shelfItems;

	public static Vector3[] offsets = new Vector3[4]
	{
		new Vector3(-0.1f, 0.1f, 0.1f),
		new Vector3(-0.1f, 0.4f, 0.2f),
		new Vector3(0.2f, 0.1f, 0.05f),
		new Vector3(0.2f, 0.4f, 0.15f)
	};

	protected override void OnSpawn()
	{
		base.OnSpawn();
		shelfItems = new KBatchedAnimController[4];
		Subscribe(-1697596308, OnStorageChange);
		OnStorageChange(null);
	}

	private KBatchedAnimController CreateShelfItem(int index, KAnimFile animFile, bool show)
	{
		if (animFile == null)
		{
			Debug.LogWarning("Mini-fridge: shelf item anim file is null");
			return null;
		}
		KBatchedAnimController kBatchedAnimController = shelfItems[index];
		if (kBatchedAnimController != null)
		{
			kBatchedAnimController.enabled = show;
			if (show)
			{
				kBatchedAnimController.SwapAnims(new KAnimFile[1] { animFile });
			}
			return kBatchedAnimController;
		}
		Vector3 position = base.transform.position;
		GameObject gameObject = new GameObject("Fridge shelf display");
		gameObject.SetActive(value: false);
		KBatchedAnimController kBatchedAnimController2 = gameObject.AddComponent<KBatchedAnimController>();
		kBatchedAnimController2.AnimFiles = new KAnimFile[1] { animFile };
		kBatchedAnimController2.initialAnim = "ui";
		kBatchedAnimController2.sceneLayer = Grid.SceneLayer.Building;
		kBatchedAnimController2.animScale *= 0.4f;
		kBatchedAnimController2.enabled = show;
		gameObject.transform.parent = base.gameObject.transform;
		gameObject.transform.position = position + offsets[index];
		gameObject.SetActive(value: true);
		shelfItems[index] = kBatchedAnimController2;
		return kBatchedAnimController2;
	}

	private void OnStorageChange(object obj)
	{
		List<GameObject> items = storage.GetItems();
		items.OrderBy((GameObject i) => i.GetComponent<PrimaryElement>().Mass);
		for (int num = 0; num < 4; num++)
		{
			if (items.Count > num)
			{
				GameObject item = items[num];
				ShowItem(num, item);
			}
			else
			{
				HideItem(num);
			}
		}
	}

	private void HideItem(int index)
	{
		if (shelfItems[index] != null)
		{
			shelfItems[index].enabled = false;
		}
	}

	private void ShowItem(int index, GameObject item)
	{
		if (item.TryGetComponent<KBatchedAnimController>(out var component))
		{
			if (component.AnimFiles == null || component.AnimFiles.Length == 0)
			{
				Debug.LogWarning($"ShelfDisplay: anim files null or empty {item.PrefabID()}");
				HideItem(index);
			}
			else
			{
				CreateShelfItem(index, component.AnimFiles[0], show: true);
			}
		}
	}
}
