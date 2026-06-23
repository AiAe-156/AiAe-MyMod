using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class StarmapHexCellInventoryVisuals : ClusterGridEntity
{
	public const int MAX_VISUAL_ITEMS = 6;

	public const string ANIM_FILE_NAME = "harvestable_elements_kanim";

	public const string DEFAULT_ANIM_STATE_NAME = "idle_6";

	public const string ANIM_STATE_NAME_PREFIX = "idle_";

	public const string SYMBOL_SWAP_NAME_PREFIX = "swap0";

	private static readonly HashedString GLOW_SYMBOL = "glow";

	public StarmapHexCellInventory inventory;

	private KBatchedAnimController animController;

	private KBatchedAnimController[] symbolAnimControllers;

	public override string Name => UI.CLUSTERMAP.HEXCELL_INVENTORY.NAME;

	public override EntityLayer Layer => EntityLayer.Debri;

	public override List<AnimConfig> AnimConfigs => new List<AnimConfig>
	{
		new AnimConfig
		{
			animFile = Assets.GetAnim("harvestable_elements_kanim"),
			initialAnim = "idle_6",
			playMode = KAnim.PlayMode.Loop,
			additionalInfo = this
		}
	};

	public override bool IsVisible => true;

	public override ClusterRevealLevel IsVisibleInFOW => ClusterRevealLevel.Hidden;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		inventory = GetComponent<StarmapHexCellInventory>();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (!inventory.RegisterInventory(base.Location))
		{
			StarmapHexCellInventory.AllInventories[base.Location].TransferAllItemsFromExternalInventory(inventory);
			base.gameObject.DeleteObject();
		}
		else
		{
			Subscribe(-1697596308, RefreshVisuals);
			Subscribe(-1503271301, OnSelectObject);
			RefreshVisuals(null);
		}
	}

	private void OnSelectObject(object data)
	{
		ToggleSelectionGlow(((Boxed<bool>)data).value);
	}

	public void RefreshVisuals(object o)
	{
		RefreshVisuals();
	}

	public void RefreshVisuals()
	{
		if (ClusterMapScreen.Instance == null || !ClusterMapScreen.Instance.isActiveAndEnabled)
		{
			return;
		}
		bool flag = inventory.ItemCount > 0;
		if (animController != null)
		{
			int num = Mathf.Min(6, inventory.ItemCount);
			string text = "idle_" + num;
			animController.Play(text, KAnim.PlayMode.Loop);
			for (int i = 0; i < symbolAnimControllers.Length; i++)
			{
				KBatchedAnimController kBatchedAnimController = symbolAnimControllers[i];
				KBatchedAnimTracker component = kBatchedAnimController.GetComponent<KBatchedAnimTracker>();
				if (i < num)
				{
					GameObject prefab = Assets.GetPrefab(inventory.Items[i].ID);
					Element element = ElementLoader.GetElement(prefab.PrefabID());
					KBatchedAnimController component2 = prefab.GetComponent<KBatchedAnimController>();
					string text2 = ((element != null && element.IsLiquid) ? "idle2" : (string.IsNullOrEmpty(component2.initialAnim) ? "object" : component2.initialAnim));
					string animName;
					KAnimFile animFileFromPrefabWithTag = Def.GetAnimFileFromPrefabWithTag(prefab, text2, out animName);
					kBatchedAnimController.SwapAnims(new KAnimFile[1] { animFileFromPrefabWithTag });
					kBatchedAnimController.Play(text2);
					if (element != null)
					{
						Color color = element.substance.colour;
						color.a = 1f;
						if (!element.IsSolid)
						{
							kBatchedAnimController.SetSymbolTint(new KAnimHashedString("substance_tinter"), color);
						}
						if (element.IsGas)
						{
							kBatchedAnimController.SetSymbolTint(new KAnimHashedString("substance_tinter_cap"), color);
						}
					}
					kBatchedAnimController.gameObject.SetActive(value: true);
					component.forceAlwaysVisible = true;
				}
				else
				{
					component.forceAlwaysVisible = false;
					kBatchedAnimController.gameObject.SetActive(value: false);
				}
			}
		}
		if (flag != m_selectable.IsSelectable)
		{
			m_selectable.IsSelectable = flag;
		}
	}

	public override void onClustermapVisualizerAnimCreated(KBatchedAnimController controller, AnimConfig config)
	{
		if (config.additionalInfo == this)
		{
			animController = controller;
			SetupAnimControllerAndSymbols();
			RefreshVisuals(null);
		}
	}

	private void ToggleSelectionGlow(bool glow)
	{
		animController.SetSymbolVisiblity(GLOW_SYMBOL, glow);
	}

	private void SetupAnimControllerAndSymbols()
	{
		DeleteSymbolControllers();
		if (animController != null)
		{
			animController.SetSymbolVisiblity(GLOW_SYMBOL, is_visible: false);
			symbolAnimControllers = new KBatchedAnimController[6];
			for (int i = 0; i < symbolAnimControllers.Length; i++)
			{
				string symbolName = "swap0" + (i + 1);
				KBatchedAnimController kBatchedAnimController = CreateSymbolController(symbolName);
				symbolAnimControllers[i] = kBatchedAnimController;
			}
		}
	}

	private KBatchedAnimController CreateSymbolController(string symbolName)
	{
		KBatchedAnimController kBatchedAnimController = CreateEmptyKAnimController(symbolName);
		bool symbolVisible;
		Matrix4x4 symbolTransform = animController.GetSymbolTransform(symbolName, out symbolVisible);
		bool symbolVisible2;
		Matrix2x3 symbolLocalTransform = animController.GetSymbolLocalTransform(symbolName, out symbolVisible2);
		Vector3 position = symbolTransform.GetColumn(3);
		Vector3 localScale = Vector3.one * symbolLocalTransform.m00;
		kBatchedAnimController.transform.SetParent(animController.transform, worldPositionStays: false);
		kBatchedAnimController.transform.SetPosition(position);
		Vector3 localPosition = kBatchedAnimController.transform.localPosition;
		localPosition.z = -0.0025f;
		kBatchedAnimController.transform.localPosition = localPosition;
		kBatchedAnimController.transform.localScale = localScale;
		KBatchedAnimTracker kBatchedAnimTracker = kBatchedAnimController.gameObject.AddComponent<KBatchedAnimTracker>();
		kBatchedAnimTracker.controller = animController;
		kBatchedAnimTracker.symbol = new HashedString(symbolName);
		kBatchedAnimTracker.forceAlwaysVisible = false;
		kBatchedAnimController.gameObject.SetActive(value: false);
		animController.SetSymbolVisiblity(symbolName, is_visible: false);
		return kBatchedAnimController;
	}

	private KBatchedAnimController CreateEmptyKAnimController(string name)
	{
		GameObject obj = new GameObject(base.gameObject.name + "-" + name);
		obj.SetActive(value: false);
		KBatchedAnimController kBatchedAnimController = obj.AddComponent<KBatchedAnimController>();
		kBatchedAnimController.AnimFiles = new KAnimFile[1] { Assets.GetAnim("harvestable_elements_kanim") };
		kBatchedAnimController.materialType = KAnimBatchGroup.MaterialType.UI;
		kBatchedAnimController.animScale = ((animController == null) ? 0.08f : animController.animScale);
		kBatchedAnimController.fgLayer = Grid.SceneLayer.NoLayer;
		kBatchedAnimController.sceneLayer = Grid.SceneLayer.NoLayer;
		return kBatchedAnimController;
	}

	private void DeleteSymbolControllers()
	{
		if (symbolAnimControllers == null)
		{
			return;
		}
		for (int i = 0; i < symbolAnimControllers.Length; i++)
		{
			KBatchedAnimController kBatchedAnimController = symbolAnimControllers[i];
			if (kBatchedAnimController != null)
			{
				kBatchedAnimController.gameObject.DeleteObject();
			}
		}
		symbolAnimControllers = null;
	}
}
