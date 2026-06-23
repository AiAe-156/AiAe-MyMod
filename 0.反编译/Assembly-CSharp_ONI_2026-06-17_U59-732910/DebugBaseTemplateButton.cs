using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TemplateClasses;
using UnityEngine;

public class DebugBaseTemplateButton : KScreen
{
	private bool SaveAllBuildings;

	private bool SaveAllPickups;

	public KButton saveBaseButton;

	public KButton clearButton;

	private TemplateContainer pasteAndSelectAsset;

	public KButton AddSelectionButton;

	public KButton RemoveSelectionButton;

	public KButton clearSelectionButton;

	public KButton DestroyButton;

	public KButton DeconstructButton;

	public KButton MoveButton;

	public TemplateContainer moveAsset;

	public KInputTextField nameField;

	private string SaveName = "enter_template_name";

	public GameObject Placer;

	public Grid.SceneLayer visualizerLayer = Grid.SceneLayer.Move;

	public List<int> SelectedCells = new List<int>();

	public static DebugBaseTemplateButton Instance { get; private set; }

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
		base.gameObject.SetActive(value: false);
		SetupLocText();
		base.ConsumeMouseScroll = true;
		KInputTextField kInputTextField = nameField;
		kInputTextField.onFocus = (System.Action)Delegate.Combine(kInputTextField.onFocus, (System.Action)delegate
		{
			base.isEditing = true;
		});
		nameField.onEndEdit.AddListener(delegate
		{
			base.isEditing = false;
		});
		nameField.onValueChanged.AddListener(delegate
		{
			Util.ScrubInputField(nameField, isPath: true);
		});
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		base.ConsumeMouseScroll = true;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (saveBaseButton != null)
		{
			saveBaseButton.onClick -= OnClickSaveBase;
			saveBaseButton.onClick += OnClickSaveBase;
		}
		if (clearButton != null)
		{
			clearButton.onClick -= OnClickClear;
			clearButton.onClick += OnClickClear;
		}
		if (AddSelectionButton != null)
		{
			AddSelectionButton.onClick -= OnClickAddSelection;
			AddSelectionButton.onClick += OnClickAddSelection;
		}
		if (RemoveSelectionButton != null)
		{
			RemoveSelectionButton.onClick -= OnClickRemoveSelection;
			RemoveSelectionButton.onClick += OnClickRemoveSelection;
		}
		if (clearSelectionButton != null)
		{
			clearSelectionButton.onClick -= OnClickClearSelection;
			clearSelectionButton.onClick += OnClickClearSelection;
		}
		if (MoveButton != null)
		{
			MoveButton.onClick -= OnClickMove;
			MoveButton.onClick += OnClickMove;
		}
		if (DestroyButton != null)
		{
			DestroyButton.onClick -= OnClickDestroySelection;
			DestroyButton.onClick += OnClickDestroySelection;
		}
		if (DeconstructButton != null)
		{
			DeconstructButton.onClick -= OnClickDeconstructSelection;
			DeconstructButton.onClick += OnClickDeconstructSelection;
		}
	}

	private void SetupLocText()
	{
	}

	private void OnClickDestroySelection()
	{
		DebugTool.Instance.Activate(DebugTool.Type.Destroy);
	}

	private void OnClickDeconstructSelection()
	{
		DebugTool.Instance.Activate(DebugTool.Type.Deconstruct);
	}

	private void OnClickMove()
	{
		DebugTool.Instance.DeactivateTool();
		moveAsset = GetSelectionAsAsset();
		StampTool.Instance.Activate(moveAsset);
	}

	private void OnClickAddSelection()
	{
		DebugTool.Instance.Activate(DebugTool.Type.AddSelection);
	}

	private void OnClickRemoveSelection()
	{
		DebugTool.Instance.Activate(DebugTool.Type.RemoveSelection);
	}

	private void OnClickClearSelection()
	{
		ClearSelection();
		nameField.text = "";
	}

	private void OnClickClear()
	{
		DebugTool.Instance.Activate(DebugTool.Type.Clear);
	}

	protected override void OnDeactivate()
	{
		if (DebugTool.Instance != null)
		{
			DebugTool.Instance.DeactivateTool();
		}
		base.OnDeactivate();
	}

	protected override void OnDisable()
	{
		if (DebugTool.Instance != null)
		{
			DebugTool.Instance.DeactivateTool();
		}
	}

	private TemplateContainer GetSelectionAsAsset()
	{
		List<Cell> list = new List<Cell>();
		List<Prefab> list2 = new List<Prefab>();
		List<Prefab> list3 = new List<Prefab>();
		List<Prefab> _primaryElementOres = new List<Prefab>();
		List<Prefab> _otherEntities = new List<Prefab>();
		HashSet<GameObject> _excludeEntities = new HashSet<GameObject>();
		float num = 0f;
		float num2 = 0f;
		foreach (int selectedCell in SelectedCells)
		{
			num += (float)Grid.CellToXY(selectedCell).x;
			num2 += (float)Grid.CellToXY(selectedCell).y;
		}
		float x = num / (float)SelectedCells.Count;
		float y = (num2 /= (float)SelectedCells.Count);
		Grid.CellToXY(Grid.PosToCell(new Vector3(x, y, 0f)), out var rootX, out var rootY);
		for (int i = 0; i < SelectedCells.Count; i++)
		{
			int gameCell = SelectedCells[i];
			Grid.CellToXY(SelectedCells[i], out var x2, out var y2);
			list.Add(new Cell(x2 - rootX, y2 - rootY, gameCell));
		}
		for (int j = 0; j < Components.BuildingCompletes.Count; j++)
		{
			BuildingComplete buildingComplete = Components.BuildingCompletes[j];
			if (_excludeEntities.Contains(buildingComplete.gameObject))
			{
				continue;
			}
			int num3 = Grid.PosToCell(buildingComplete);
			Grid.CellToXY(num3, out var x3, out var y3);
			if (!SaveAllBuildings && !SelectedCells.Contains(num3))
			{
				continue;
			}
			int[] placementCells = buildingComplete.PlacementCells;
			string diseaseName;
			foreach (int num4 in placementCells)
			{
				Grid.CellToXY(num4, out var xplace, out var yplace);
				diseaseName = ((Grid.DiseaseIdx[num4] != byte.MaxValue) ? Db.Get().Diseases[Grid.DiseaseIdx[num4]].Id : null);
				if (list.Find((Cell c) => c.location_x == xplace - rootX && c.location_y == yplace - rootY) == null)
				{
					list.Add(new Cell(xplace - rootX, yplace - rootY, Grid.Element[num4].id, Grid.Temperature[num4], Grid.Mass[num4], diseaseName, Grid.DiseaseCount[num4]));
				}
			}
			Orientation rotation = Orientation.Neutral;
			Rotatable component = buildingComplete.gameObject.GetComponent<Rotatable>();
			if (component != null)
			{
				rotation = component.GetOrientation();
			}
			SimHashes element = SimHashes.Void;
			float value = 280f;
			diseaseName = null;
			int disease_count = 0;
			PrimaryElement component2 = buildingComplete.GetComponent<PrimaryElement>();
			buildingComplete.GetComponent<KPrefabID>();
			if (component2 != null)
			{
				element = component2.ElementID;
				value = component2.Temperature;
				diseaseName = ((component2.DiseaseIdx != byte.MaxValue) ? Db.Get().Diseases[component2.DiseaseIdx].Id : null);
				disease_count = component2.DiseaseCount;
			}
			List<Prefab.template_amount_value> list4 = new List<Prefab.template_amount_value>();
			List<Prefab.template_amount_value> list5 = new List<Prefab.template_amount_value>();
			foreach (AmountInstance modifier in buildingComplete.gameObject.GetAmounts().ModifierList)
			{
				list4.Add(new Prefab.template_amount_value(modifier.amount.Id, modifier.value));
			}
			float num5 = 0f;
			Battery component3 = buildingComplete.GetComponent<Battery>();
			if (component3 != null)
			{
				num5 = component3.JoulesAvailable;
				list5.Add(new Prefab.template_amount_value("joulesAvailable", num5));
			}
			float num6 = 0f;
			Unsealable component4 = buildingComplete.GetComponent<Unsealable>();
			if (component4 != null)
			{
				num6 = (component4.facingRight ? 1 : 0);
				list5.Add(new Prefab.template_amount_value("sealedDoorDirection", num6));
			}
			float num7 = 0f;
			LogicSwitch component5 = buildingComplete.GetComponent<LogicSwitch>();
			if (component5 != null)
			{
				num7 = (component5.IsSwitchedOn ? 1 : 0);
				list5.Add(new Prefab.template_amount_value("switchSetting", num7));
			}
			int connections = 0;
			IHaveUtilityNetworkMgr component6 = buildingComplete.GetComponent<IHaveUtilityNetworkMgr>();
			if (component6 != null)
			{
				connections = (int)component6.GetNetworkManager().GetConnections(num3, is_physical_building: true);
			}
			string facadeIdId = null;
			BuildingFacade component7 = buildingComplete.GetComponent<BuildingFacade>();
			if (component7 != null)
			{
				facadeIdId = component7.CurrentFacade;
			}
			x3 -= rootX;
			y3 -= rootY;
			value = Mathf.Clamp(value, 1f, 99999f);
			Prefab prefab = new Prefab(buildingComplete.PrefabID().Name, Prefab.Type.Building, x3, y3, element, value, 0f, diseaseName, disease_count, rotation, list4.ToArray(), list5.ToArray(), connections, facadeIdId);
			LoreBearer component8 = buildingComplete.GetComponent<LoreBearer>();
			if (component8 != null && !string.IsNullOrEmpty(component8.poiOverrideLoreUnlockId))
			{
				prefab.loreUnlockId = component8.poiOverrideLoreUnlockId;
				prefab.loreDisplayText = component8.poiOverrideLoreDisplayText;
				prefab.loreNextCollectionId = component8.poiOverrideNextCollectionId;
			}
			Storage component9 = buildingComplete.gameObject.GetComponent<Storage>();
			if (component9 != null)
			{
				foreach (GameObject item2 in component9.items)
				{
					float units = 0f;
					SimHashes element2 = SimHashes.Vacuum;
					float temp = 280f;
					string disease = null;
					int disease_count2 = 0;
					bool isOre = false;
					PrimaryElement component10 = item2.GetComponent<PrimaryElement>();
					if (component10 != null)
					{
						units = component10.Units;
						element2 = component10.ElementID;
						temp = component10.Temperature;
						disease = ((component10.DiseaseIdx != byte.MaxValue) ? Db.Get().Diseases[component10.DiseaseIdx].Id : null);
						disease_count2 = component10.DiseaseCount;
					}
					Rottable.Instance sMI = item2.gameObject.GetSMI<Rottable.Instance>();
					if (item2.GetComponent<ElementChunk>() != null)
					{
						isOre = true;
					}
					StorageItem storageItem = new StorageItem(item2.PrefabID().Name, units, temp, element2, disease, disease_count2, isOre);
					if (sMI != null)
					{
						storageItem.rottable.rotAmount = sMI.RotValue;
					}
					prefab.AssignStorage(storageItem);
					_excludeEntities.Add(item2);
				}
			}
			list2.Add(prefab);
			_excludeEntities.Add(buildingComplete.gameObject);
		}
		for (int num8 = 0; num8 < Components.Pickupables.Count; num8++)
		{
			if (!Components.Pickupables[num8].gameObject.activeSelf)
			{
				continue;
			}
			Pickupable pickupable = Components.Pickupables[num8];
			if (_excludeEntities.Contains(pickupable.gameObject))
			{
				continue;
			}
			int num9 = Grid.PosToCell(pickupable);
			if ((SaveAllPickups || SelectedCells.Contains(num9)) && !Components.Pickupables[num8].gameObject.GetComponent<MinionBrain>())
			{
				Grid.CellToXY(num9, out var x4, out var y4);
				x4 -= rootX;
				y4 -= rootY;
				SimHashes element3 = SimHashes.Void;
				float temperature = 280f;
				float units2 = 1f;
				string disease2 = null;
				int disease_count3 = 0;
				float rotAmount = 0f;
				Rottable.Instance sMI2 = pickupable.gameObject.GetSMI<Rottable.Instance>();
				if (sMI2 != null)
				{
					rotAmount = sMI2.RotValue;
				}
				PrimaryElement component11 = pickupable.gameObject.GetComponent<PrimaryElement>();
				if (component11 != null)
				{
					element3 = component11.ElementID;
					units2 = component11.Units;
					temperature = component11.Temperature;
					disease2 = ((component11.DiseaseIdx != byte.MaxValue) ? Db.Get().Diseases[component11.DiseaseIdx].Id : null);
					disease_count3 = component11.DiseaseCount;
				}
				if (pickupable.gameObject.GetComponent<ElementChunk>() != null)
				{
					Prefab item = new Prefab(pickupable.PrefabID().Name, Prefab.Type.Ore, x4, y4, element3, temperature, units2, disease2, disease_count3);
					_primaryElementOres.Add(item);
				}
				else
				{
					Prefab item = new Prefab(pickupable.PrefabID().Name, Prefab.Type.Pickupable, x4, y4, element3, temperature, units2, disease2, disease_count3);
					item.rottable = new TemplateClasses.Rottable();
					item.rottable.rotAmount = rotAmount;
					list3.Add(item);
				}
				_excludeEntities.Add(pickupable.gameObject);
			}
		}
		GetEntities(Components.Crops.Items, rootX, rootY, ref _primaryElementOres, ref _otherEntities, ref _excludeEntities);
		GetEntities(Components.Health.Items, rootX, rootY, ref _primaryElementOres, ref _otherEntities, ref _excludeEntities);
		GetEntities(Components.Harvestables.Items, rootX, rootY, ref _primaryElementOres, ref _otherEntities, ref _excludeEntities);
		GetEntities(Components.Edibles.Items, rootX, rootY, ref _primaryElementOres, ref _otherEntities, ref _excludeEntities);
		GetEntities<Geyser>(rootX, rootY, ref _primaryElementOres, ref _otherEntities, ref _excludeEntities);
		GetEntities<OccupyArea>(rootX, rootY, ref _primaryElementOres, ref _otherEntities, ref _excludeEntities);
		GetEntities<FogOfWarMask>(rootX, rootY, ref _primaryElementOres, ref _otherEntities, ref _excludeEntities);
		_otherEntities.RemoveAll((Prefab prefab2) => Assets.GetPrefab(prefab2.id).HasTag(GameTags.ExcludeFromTemplate));
		TemplateContainer templateContainer = new TemplateContainer();
		templateContainer.Init(list, list2, list3, _primaryElementOres, _otherEntities);
		return templateContainer;
	}

	private void GetEntities<T>(int rootX, int rootY, ref List<Prefab> _primaryElementOres, ref List<Prefab> _otherEntities, ref HashSet<GameObject> _excludeEntities)
	{
		object[] array = UnityEngine.Object.FindObjectsByType(typeof(T), FindObjectsSortMode.InstanceID);
		object[] component_collection = array;
		GetEntities(component_collection, rootX, rootY, ref _primaryElementOres, ref _otherEntities, ref _excludeEntities);
	}

	private void GetEntities<T>(IEnumerable<T> component_collection, int rootX, int rootY, ref List<Prefab> _primaryElementOres, ref List<Prefab> _otherEntities, ref HashSet<GameObject> _excludeEntities)
	{
		foreach (T item2 in component_collection)
		{
			if (_excludeEntities.Contains((item2 as KMonoBehaviour).gameObject) || !(item2 as KMonoBehaviour).gameObject.activeSelf)
			{
				continue;
			}
			int num = Grid.PosToCell(item2 as KMonoBehaviour);
			if (!SelectedCells.Contains(num) || (bool)(item2 as KMonoBehaviour).gameObject.GetComponent<MinionBrain>())
			{
				continue;
			}
			Orientation rotation = Orientation.Neutral;
			Rotatable component = (item2 as KMonoBehaviour).GetComponent<Rotatable>();
			if (component != null)
			{
				rotation = component.Orientation;
			}
			Grid.CellToXY(num, out var x, out var y);
			x -= rootX;
			y -= rootY;
			SimHashes simHashes = SimHashes.Void;
			float num2 = 280f;
			float num3 = 1f;
			string text = null;
			int num4 = 0;
			PrimaryElement component2 = (item2 as KMonoBehaviour).gameObject.GetComponent<PrimaryElement>();
			if (component2 != null)
			{
				simHashes = component2.ElementID;
				num3 = component2.Units;
				num2 = component2.Temperature;
				text = ((component2.DiseaseIdx != byte.MaxValue) ? Db.Get().Diseases[component2.DiseaseIdx].Id : null);
				num4 = component2.DiseaseCount;
			}
			List<Prefab.template_amount_value> list = new List<Prefab.template_amount_value>();
			if ((item2 as KMonoBehaviour).gameObject.GetAmounts() != null)
			{
				foreach (AmountInstance modifier in (item2 as KMonoBehaviour).gameObject.GetAmounts().ModifierList)
				{
					list.Add(new Prefab.template_amount_value(modifier.amount.Id, modifier.value));
				}
			}
			Prefab.template_amount_value[] amount_values;
			Prefab item;
			if ((item2 as KMonoBehaviour).gameObject.GetComponent<ElementChunk>() != null)
			{
				string id = (item2 as KMonoBehaviour).PrefabID().Name;
				int loc_x = x;
				int loc_y = y;
				SimHashes element = simHashes;
				float temperature = num2;
				float units = num3;
				string disease = text;
				int disease_count = num4;
				amount_values = list.ToArray();
				item = new Prefab(id, Prefab.Type.Ore, loc_x, loc_y, element, temperature, units, disease, disease_count, rotation, amount_values);
				_primaryElementOres.Add(item);
				_excludeEntities.Add((item2 as KMonoBehaviour).gameObject);
				continue;
			}
			string id2 = (item2 as KMonoBehaviour).PrefabID().Name;
			int loc_x2 = x;
			int loc_y2 = y;
			SimHashes element2 = simHashes;
			float temperature2 = num2;
			float units2 = num3;
			string disease2 = text;
			int disease_count2 = num4;
			amount_values = list.ToArray();
			item = new Prefab(id2, Prefab.Type.Other, loc_x2, loc_y2, element2, temperature2, units2, disease2, disease_count2, rotation, amount_values);
			LoreBearer component3 = (item2 as KMonoBehaviour).gameObject.GetComponent<LoreBearer>();
			if (component3 != null && !string.IsNullOrEmpty(component3.poiOverrideLoreUnlockId))
			{
				item.loreUnlockId = component3.poiOverrideLoreUnlockId;
				item.loreDisplayText = component3.poiOverrideLoreDisplayText;
				item.loreNextCollectionId = component3.poiOverrideNextCollectionId;
			}
			_otherEntities.Add(item);
			_excludeEntities.Add((item2 as KMonoBehaviour).gameObject);
		}
	}

	private static int CountLoreOverrides(TemplateContainer template)
	{
		int num = 0;
		if (template.buildings != null)
		{
			foreach (Prefab building in template.buildings)
			{
				if (!string.IsNullOrEmpty(building.loreUnlockId))
				{
					num++;
				}
			}
		}
		if (template.otherEntities != null)
		{
			foreach (Prefab otherEntity in template.otherEntities)
			{
				if (!string.IsNullOrEmpty(otherEntity.loreUnlockId))
				{
					num++;
				}
			}
		}
		return num;
	}

	private void OnClickSaveBase()
	{
		TemplateContainer asset = GetSelectionAsAsset();
		if (SelectedCells.Count <= 0)
		{
			Debug.LogWarning("No cells selected. Use buttons above to select the area you want to save.");
			return;
		}
		SaveName = nameField.text;
		if (SaveName == null || SaveName == "")
		{
			Debug.LogWarning("Invalid save name. Please enter a name in the input field.");
			return;
		}
		if (TemplateCache.TemplateExists(SaveName))
		{
			int num = CountLoreOverrides(TemplateCache.GetTemplate(SaveName));
			int num2 = CountLoreOverrides(asset);
			if (num2 < num)
			{
				Util.KInstantiateUI<ConfirmDialogScreen>(ScreenPrefabs.Instance.ConfirmDialogScreen.gameObject, GameScreenManager.Instance.ssOverlayCanvas.gameObject, force_active: true).PopupConfirmDialog(UI.DEBUG_TOOLS.SAVE_BASE_TEMPLATE.LORE_OVERRIDE_WARNING.Replace("{newCount}", num2.ToString()).Replace("{oldCount}", num.ToString()), delegate
				{
					DoSaveTemplate(asset);
				}, delegate
				{
				});
				return;
			}
		}
		DoSaveTemplate(asset);
	}

	private void DoSaveTemplate(TemplateContainer asset)
	{
		asset.SaveToYaml(SaveName);
		TemplateCache.Clear();
		TemplateCache.Init();
		PasteBaseTemplateScreen.Instance.RefreshStampButtons();
	}

	public void ClearSelection()
	{
		for (int num = SelectedCells.Count - 1; num >= 0; num--)
		{
			RemoveFromSelection(SelectedCells[num]);
		}
	}

	public void DestroySelection()
	{
	}

	public void DeconstructSelection()
	{
	}

	public void AddToSelection(int cell)
	{
		if (!SelectedCells.Contains(cell))
		{
			GameObject gameObject = (Grid.Objects[cell, 7] = Util.KInstantiate(Placer));
			Vector3 position = Grid.CellToPosCBC(cell, visualizerLayer);
			float num = -0.15f;
			position.z += num;
			gameObject.transform.SetPosition(position);
			SelectedCells.Add(cell);
		}
	}

	public void RemoveFromSelection(int cell)
	{
		if (SelectedCells.Contains(cell))
		{
			GameObject gameObject = Grid.Objects[cell, 7];
			if (gameObject != null)
			{
				gameObject.DeleteObject();
			}
			SelectedCells.Remove(cell);
		}
	}
}
