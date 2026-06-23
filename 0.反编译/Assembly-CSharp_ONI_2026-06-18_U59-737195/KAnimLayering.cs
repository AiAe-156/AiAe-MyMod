using System.Collections.Generic;
using UnityEngine;

public class KAnimLayering
{
	public static readonly KAnimHashedString UI = new KAnimHashedString("ui");

	private static Dictionary<KAnim.SymbolFlags, KAnimBatchGroup.MaterialType> layerSettings = new Dictionary<KAnim.SymbolFlags, KAnimBatchGroup.MaterialType>
	{
		{
			KAnim.SymbolFlags.FG,
			KAnimBatchGroup.MaterialType.Default
		},
		{
			KAnim.SymbolFlags.SH,
			KAnimBatchGroup.MaterialType.Shine
		}
	};

	private bool isLayer;

	private KAnimControllerBase controller;

	private Dictionary<KAnim.SymbolFlags, KAnimControllerBase> layerControllers;

	private Dictionary<KAnim.SymbolFlags, KAnimLink> links;

	private Grid.SceneLayer layer = Grid.SceneLayer.BuildingFront;

	public KAnimLayering(KAnimControllerBase controller, Grid.SceneLayer layer)
	{
		this.controller = controller;
		this.layer = layer;
	}

	public void SetLayer(Grid.SceneLayer layer)
	{
		this.layer = layer;
		if (layerControllers == null)
		{
			return;
		}
		foreach (KAnimControllerBase value in layerControllers.Values)
		{
			TransformExtensions.SetLocalPosition(position: new Vector3(0f, 0f, Grid.GetLayerZ(layer) - controller.gameObject.transform.GetPosition().z - 0.1f), transform: value.transform);
		}
	}

	public void SetIsLayer(bool is_layer)
	{
		isLayer = is_layer;
	}

	public bool GetIsLayer()
	{
		return isLayer;
	}

	public void SetSyncLayeringTint(bool sync)
	{
		if (links == null)
		{
			return;
		}
		foreach (KeyValuePair<KAnim.SymbolFlags, KAnimLink> link in links)
		{
			link.Value.syncTint = sync;
		}
	}

	private static bool IsAnimLayered(KAnimFile[] anims, KAnim.SymbolFlags layer_flag)
	{
		for (int i = 0; i < anims.Length; i++)
		{
			if (IsAnimFileLayered(anims[i], layer_flag))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsAnimFileLayered(KAnimFile anim_file, KAnim.SymbolFlags layer_flag)
	{
		if (anim_file == null)
		{
			return false;
		}
		KAnimFileData data = anim_file.GetData();
		if (data.build == null)
		{
			return false;
		}
		KAnim.Build.Symbol[] symbols = data.build.symbols;
		for (int i = 0; i < symbols.Length; i++)
		{
			if (((uint)symbols[i].flags & (uint)layer_flag) != 0)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsOverrideAnimLayered(IReadOnlyList<KAnimControllerBase.OverrideAnimFileData> override_anims, KAnim.SymbolFlags layer_flag)
	{
		foreach (KAnimControllerBase.OverrideAnimFileData override_anim in override_anims)
		{
			if (IsAnimFileLayered(override_anim.file, layer_flag))
			{
				return true;
			}
		}
		return false;
	}

	private void HideSymbolsInternal(KAnim.SymbolFlags symbol_flag_to_hide)
	{
		KAnimFile[] animFiles = controller.AnimFiles;
		foreach (KAnimFile anim_file in animFiles)
		{
			SetAnimVisibility(anim_file, symbol_flag_to_hide);
		}
		IReadOnlyList<KAnimControllerBase.OverrideAnimFileData> overrideAnimFiles = controller.OverrideAnimFiles;
		for (int j = 0; j < overrideAnimFiles.Count; j++)
		{
			KAnimFile file = overrideAnimFiles[j].file;
			SetAnimVisibility(file, symbol_flag_to_hide);
		}
	}

	private void SetAnimVisibility(KAnimFile anim_file, KAnim.SymbolFlags symbol_flag)
	{
		if (anim_file == null)
		{
			return;
		}
		KAnimFileData data = anim_file.GetData();
		if (data.build == null)
		{
			return;
		}
		KAnim.Build.Symbol[] symbols = data.build.symbols;
		for (int i = 0; i < symbols.Length; i++)
		{
			if (((uint)symbols[i].flags & (uint)symbol_flag) != 0 != isLayer && !(symbols[i].hash == UI))
			{
				controller.SetSymbolVisiblity(symbols[i].hash, is_visible: false);
			}
		}
	}

	public void HideSymbols()
	{
		if (EntityPrefabs.Instance == null || isLayer)
		{
			return;
		}
		foreach (KAnim.SymbolFlags key in layerSettings.Keys)
		{
			bool flag = IsAnimLayered(controller.AnimFiles, key);
			bool flag2 = IsOverrideAnimLayered(controller.OverrideAnimFiles, key);
			flag = flag || flag2;
			int num;
			if (flag && layer != Grid.SceneLayer.NoLayer)
			{
				if (layerControllers != null)
				{
					num = ((!layerControllers.ContainsKey(key)) ? 1 : 0);
					if (num == 0)
					{
						goto IL_0152;
					}
				}
				else
				{
					num = 1;
				}
				if (layerControllers == null)
				{
					layerControllers = new Dictionary<KAnim.SymbolFlags, KAnimControllerBase>();
				}
				if (links == null)
				{
					links = new Dictionary<KAnim.SymbolFlags, KAnimLink>();
				}
				GameObject gameObject = Util.KInstantiate(EntityPrefabs.Instance.ForegroundLayer, controller.gameObject);
				gameObject.name = controller.name + "_" + key.ToString().ToLower();
				KAnimControllerBase component = gameObject.GetComponent<KAnimControllerBase>();
				if (flag2)
				{
					SymbolOverrideControllerUtil.AddToPrefab(gameObject).applySymbolOverridesEveryFrame = true;
				}
				layerControllers.Add(key, component);
				links.Add(key, new KAnimLink(controller, component));
				component.materialType = layerSettings[key];
				goto IL_0152;
			}
			if (!flag && layerControllers != null && layerControllers.Count != 0 && layerControllers.TryGetValue(key, out var value))
			{
				controller.GetSynchronizer().Remove(value);
				value.gameObject.DeleteObject();
				layerControllers.Remove(key);
				if (links != null)
				{
					links[key].Unregister();
					links.Remove(key);
				}
			}
			continue;
			IL_0152:
			KAnimControllerBase kAnimControllerBase = layerControllers[key];
			kAnimControllerBase.AnimFiles = controller.AnimFiles;
			kAnimControllerBase.GetLayering().SetIsLayer(is_layer: true);
			kAnimControllerBase.initialAnim = controller.initialAnim;
			Dirty();
			KAnimSynchronizer synchronizer = controller.GetSynchronizer();
			if (num != 0)
			{
				synchronizer.Add(kAnimControllerBase);
			}
			else
			{
				RefreshForegroundBatchGroup();
			}
			synchronizer.Sync(kAnimControllerBase);
			TransformExtensions.SetLocalPosition(position: new Vector3(0f, 0f, Grid.GetLayerZ(layer) - controller.gameObject.transform.GetPosition().z - 0.1f), transform: kAnimControllerBase.gameObject.transform);
			kAnimControllerBase.gameObject.SetActive(value: true);
			if (!flag2)
			{
				continue;
			}
			foreach (KAnimControllerBase.OverrideAnimFileData overrideAnimFile in controller.OverrideAnimFiles)
			{
				kAnimControllerBase.AddAnimOverrides(overrideAnimFile.file, overrideAnimFile.priority);
			}
		}
		if (layerControllers == null)
		{
			return;
		}
		foreach (KeyValuePair<KAnim.SymbolFlags, KAnimControllerBase> layerController in layerControllers)
		{
			HideSymbolsInternal(layerController.Key);
			layerController.Value.GetLayering()?.HideSymbolsInternal(layerController.Key);
		}
	}

	private void RefreshForegroundBatchGroup()
	{
		if (layerControllers == null)
		{
			return;
		}
		foreach (KeyValuePair<KAnim.SymbolFlags, KAnimControllerBase> layerController in layerControllers)
		{
			foreach (KAnimControllerBase.OverrideAnimFileData item in new List<KAnimControllerBase.OverrideAnimFileData>(layerController.Value.OverrideAnimFiles))
			{
				layerController.Value.RemoveAnimOverrides(item.file);
			}
			layerController.Value.GetComponent<KBatchedAnimController>().SwapAnims(layerController.Value.AnimFiles);
		}
	}

	public void Dirty()
	{
		if (layerControllers == null)
		{
			return;
		}
		foreach (KeyValuePair<KAnim.SymbolFlags, KAnimControllerBase> layerController in layerControllers)
		{
			layerController.Value.Offset = controller.Offset;
			layerController.Value.Pivot = controller.Pivot;
			layerController.Value.Rotation = controller.Rotation;
			layerController.Value.FlipX = controller.FlipX;
			layerController.Value.FlipY = controller.FlipY;
		}
	}
}
