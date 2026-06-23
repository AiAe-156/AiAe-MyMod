using System.Collections.Generic;
using UnityEngine;

public class KAnimLayering
{
	public enum InstanceType
	{
		Invalid = -1,
		Base,
		Foreground,
		ShineMask
	}

	private InstanceType instanceType = InstanceType.Base;

	private KAnimLink link;

	private KAnimControllerBase controller;

	private Grid.SceneLayer foregroundSceneLayer = Grid.SceneLayer.BuildingFront;

	private Dictionary<InstanceType, KAnimControllerBase> animControllers = new Dictionary<InstanceType, KAnimControllerBase>();

	private static readonly Dictionary<InstanceType, KAnim.SymbolFlags> instanceTypeToAnimFlag = new Dictionary<InstanceType, KAnim.SymbolFlags>
	{
		{
			InstanceType.Base,
			(KAnim.SymbolFlags)0
		},
		{
			InstanceType.Foreground,
			KAnim.SymbolFlags.FG
		},
		{
			InstanceType.ShineMask,
			KAnim.SymbolFlags.SH
		}
	};

	public static readonly KAnimHashedString UI = new KAnimHashedString("ui");

	private static readonly int allLayerFlags = 24;

	public InstanceType Type => instanceType;

	public bool UsesLayers => instanceType != InstanceType.Invalid;

	public bool IsBaseLayer => instanceType == InstanceType.Base;

	public KAnimLink Link => link;

	public KAnimLayering(KAnimControllerBase controller, Grid.SceneLayer foregroundSceneLayer)
	{
		this.controller = controller;
		this.foregroundSceneLayer = foregroundSceneLayer;
	}

	public void SetInstanceType(InstanceType type)
	{
		instanceType = type;
	}

	public void SetForegroundSceneLayer(Grid.SceneLayer layer)
	{
		foregroundSceneLayer = layer;
		KAnimControllerBase value = null;
		if (animControllers.TryGetValue(InstanceType.Foreground, out value) && value != null)
		{
			TransformExtensions.SetLocalPosition(position: new Vector3(0f, 0f, Grid.GetLayerZ(layer) - controller.gameObject.transform.GetPosition().z - 0.1f), transform: value.transform);
		}
	}

	private bool DoesSymbolBelongToLayer(int symbolFlags)
	{
		if (instanceType == InstanceType.Base || instanceType == InstanceType.Invalid)
		{
			return (symbolFlags & allLayerFlags) == 0;
		}
		return ((uint)symbolFlags & (uint)instanceTypeToAnimFlag[instanceType]) != 0;
	}

	private void HideSymbolsInternal()
	{
		KAnimFile[] animFiles = controller.AnimFiles;
		foreach (KAnimFile kAnimFile in animFiles)
		{
			if (kAnimFile == null)
			{
				continue;
			}
			KAnimFileData data = kAnimFile.GetData();
			if (data.build == null)
			{
				continue;
			}
			KAnim.Build.Symbol[] symbols = data.build.symbols;
			for (int j = 0; j < symbols.Length; j++)
			{
				if (!(symbols[j].hash == UI) && !DoesSymbolBelongToLayer(symbols[j].flags))
				{
					controller.SetSymbolVisiblity(symbols[j].hash, is_visible: false);
				}
			}
		}
		IReadOnlyList<KAnimControllerBase.OverrideAnimFileData> overrideAnimFiles = controller.OverrideAnimFiles;
		for (int k = 0; k < overrideAnimFiles.Count; k++)
		{
			KAnimFile file = overrideAnimFiles[k].file;
			if (file == null)
			{
				continue;
			}
			KAnimFileData data2 = file.GetData();
			if (data2.build == null)
			{
				continue;
			}
			KAnim.Build.Symbol[] symbols2 = data2.build.symbols;
			for (int l = 0; l < symbols2.Length; l++)
			{
				if (!(symbols2[l].hash == UI) && !DoesSymbolBelongToLayer(symbols2[l].flags))
				{
					controller.SetSymbolVisiblity(symbols2[l].hash, is_visible: false);
				}
			}
		}
	}

	public void Refresh()
	{
		if (EntityPrefabs.Instance == null)
		{
			return;
		}
		switch (instanceType)
		{
		case InstanceType.Base:
			if (false || CreateOrDestroyLayer(controller, InstanceType.Foreground) || CreateOrDestroyLayer(controller, InstanceType.ShineMask, customMaterial: true, KAnimBatchGroup.MaterialType.Shine))
			{
				HideSymbolsInternal();
			}
			break;
		case InstanceType.Invalid:
		case InstanceType.Foreground:
		case InstanceType.ShineMask:
			break;
		}
	}

	private bool CreateOrDestroyLayer(KAnimControllerBase baseController, InstanceType layerType, bool customMaterial = false, KAnimBatchGroup.MaterialType customMaterialType = KAnimBatchGroup.MaterialType.Default)
	{
		KAnim.SymbolFlags flag = instanceTypeToAnimFlag[layerType];
		KAnimFile[] animFiles = baseController.AnimFiles;
		bool flag2 = animFiles.FilesContainsSymbolFlag(flag);
		bool flag3 = DoesOverrideAnimsContainLayer(flag);
		bool flag4 = flag2 || flag3;
		bool flag5 = layerType == InstanceType.Foreground;
		KAnimControllerBase value = null;
		bool flag6 = animControllers.TryGetValue(layerType, out value) && value != null;
		if (flag4 && (!flag5 || foregroundSceneLayer != Grid.SceneLayer.NoLayer))
		{
			bool flag7 = !flag6;
			if (flag7)
			{
				GameObject gameObject = Util.KInstantiate(EntityPrefabs.Instance.ForegroundLayer, controller.gameObject);
				gameObject.name = controller.name + "_" + flag.ToString().ToLower();
				value = gameObject.GetComponent<KAnimControllerBase>();
				if (flag3)
				{
					SymbolOverrideController symbolOverrideController = SymbolOverrideControllerUtil.AddToPrefab(gameObject);
					symbolOverrideController.applySymbolOverridesEveryFrame = true;
				}
				link = new KAnimLink(controller, value);
				animControllers[layerType] = value;
				value.materialType = (customMaterial ? customMaterialType : controller.materialType);
			}
			value.AnimFiles = animFiles;
			value.GetLayering().SetInstanceType(layerType);
			value.initialAnim = controller.initialAnim;
			Dirty(value);
			KAnimSynchronizer synchronizer = controller.GetSynchronizer();
			if (flag7)
			{
				synchronizer.Add(value);
			}
			else
			{
				value.RemoveAllAnimOverrides();
				value.GetComponent<KBatchedAnimController>().SwapAnims(value.AnimFiles);
			}
			synchronizer.Sync(value);
			Vector3 position = new Vector3(0f, 0f, (flag5 ? (Grid.GetLayerZ(foregroundSceneLayer) - controller.gameObject.transform.GetPosition().z) : 0f) - 0.1f);
			value.gameObject.transform.SetLocalPosition(position);
			value.gameObject.SetActive(value: true);
			if (flag3)
			{
				foreach (KAnimControllerBase.OverrideAnimFileData overrideAnimFile in controller.OverrideAnimFiles)
				{
					value.AddAnimOverrides(overrideAnimFile.file, overrideAnimFile.priority);
				}
			}
		}
		else if (!flag4 && flag6)
		{
			controller.GetSynchronizer().Remove(value);
			animControllers.Remove(layerType);
			value.gameObject.DeleteObject();
			link = null;
		}
		if (value != null)
		{
			value.GetLayering()?.HideSymbolsInternal();
			return true;
		}
		return false;
	}

	private bool DoesOverrideAnimsContainLayer(KAnim.SymbolFlags flag)
	{
		foreach (KAnimControllerBase.OverrideAnimFileData overrideAnimFile in controller.OverrideAnimFiles)
		{
			KAnimFile file = overrideAnimFile.file;
			if (file.FileContainsSymbolFlag(flag))
			{
				return true;
			}
		}
		return false;
	}

	public void Dirty()
	{
		foreach (KeyValuePair<InstanceType, KAnimControllerBase> animController in animControllers)
		{
			if (animController.Key != InstanceType.Base && animController.Key != InstanceType.Invalid)
			{
				KAnimControllerBase value = animController.Value;
				Dirty(value);
			}
		}
	}

	private void Dirty(KAnimControllerBase layerController)
	{
		if (!(layerController == null))
		{
			layerController.Offset = controller.Offset;
			layerController.Pivot = controller.Pivot;
			layerController.Rotation = controller.Rotation;
			layerController.FlipX = controller.FlipX;
			layerController.FlipY = controller.FlipY;
		}
	}
}
