using System;
using System.Collections.Generic;
using System.Linq;
using KSerialization;
using STRINGS;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/AccessControl")]
public class AccessControl : KMonoBehaviour, ISaveLoadable, IGameObjectEffectDescriptor
{
	public enum Permission
	{
		Both,
		GoLeft,
		GoRight,
		Neither
	}

	[MyCmpGet]
	private Operational operational;

	[MyCmpReq]
	private KSelectable selectable;

	[MyCmpAdd]
	private CopyBuildingSettings copyBuildingSettings;

	private bool isTeleporter;

	private int[] registeredBuildingCells = null;

	[Serialize]
	[Obsolete("Added support for Robots Access Controls, use savedPermissionsById", false)]
	private List<KeyValuePair<Ref<KPrefabID>, Permission>> savedPermissions = new List<KeyValuePair<Ref<KPrefabID>, Permission>>();

	[Serialize]
	[Obsolete("Added support for Robots Access Controls, use defaultPermissionByTag", false)]
	private Permission _defaultPermission = Permission.Both;

	[Serialize]
	private List<KeyValuePair<Tag, Permission>> defaultPermissionByTag = new List<KeyValuePair<Tag, Permission>>();

	[Serialize]
	private List<KeyValuePair<int, Permission>> savedPermissionsById = new List<KeyValuePair<int, Permission>>();

	[Serialize]
	public bool registered = true;

	[Serialize]
	public bool controlEnabled;

	public Door.ControlState overrideAccess = Door.ControlState.Auto;

	private static StatusItem accessControlActive;

	private static readonly EventSystem.IntraObjectHandler<AccessControl> OnControlStateChangedDelegate = new EventSystem.IntraObjectHandler<AccessControl>(delegate(AccessControl component, object data)
	{
		component.OnControlStateChanged(data);
	});

	private static readonly EventSystem.IntraObjectHandler<AccessControl> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<AccessControl>(delegate(AccessControl component, object data)
	{
		component.OnCopySettings(data);
	});

	public bool Online => true;

	private int GetTagId(Tag game_tag)
	{
		return GridRestrictionSerializer.Instance.GetTagId(game_tag);
	}

	public Permission GetDefaultPermission(Tag groupTag)
	{
		foreach (KeyValuePair<Tag, Permission> item in defaultPermissionByTag)
		{
			if (item.Key == groupTag)
			{
				return item.Value;
			}
		}
		return Permission.Both;
	}

	public void SetDefaultPermission(Tag groupTag, Permission permission)
	{
		bool flag = false;
		KeyValuePair<Tag, Permission> keyValuePair = new KeyValuePair<Tag, Permission>(groupTag, permission);
		for (int i = 0; i < defaultPermissionByTag.Count; i++)
		{
			if (defaultPermissionByTag[i].Key == groupTag)
			{
				defaultPermissionByTag[i] = keyValuePair;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			defaultPermissionByTag.Add(keyValuePair);
		}
		SetStatusItem();
		SetGridRestrictions(GetTagId(groupTag), permission);
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		if (accessControlActive == null)
		{
			accessControlActive = new StatusItem("accessControlActive", BUILDING.STATUSITEMS.ACCESS_CONTROL.ACTIVE.NAME, BUILDING.STATUSITEMS.ACCESS_CONTROL.ACTIVE.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		}
		Subscribe(279163026, OnControlStateChangedDelegate);
		Subscribe(-905833192, OnCopySettingsDelegate);
	}

	[Obsolete("Added support for Robots Access Controls")]
	private void CheckForBadData()
	{
		List<KeyValuePair<Ref<KPrefabID>, Permission>> list = new List<KeyValuePair<Ref<KPrefabID>, Permission>>();
		foreach (KeyValuePair<Ref<KPrefabID>, Permission> savedPermission in savedPermissions)
		{
			if (savedPermission.Key.Get() == null)
			{
				list.Add(savedPermission);
			}
		}
		foreach (KeyValuePair<Ref<KPrefabID>, Permission> item in list)
		{
			savedPermissions.Remove(item);
		}
	}

	private void UpgradeSavePreRobotDoorPermission()
	{
		ListPool<Tuple<MinionAssignablesProxy, Permission>, AccessControl>.PooledList pooledList = ListPool<Tuple<MinionAssignablesProxy, Permission>, AccessControl>.Allocate();
		for (int num = savedPermissions.Count - 1; num >= 0; num--)
		{
			KPrefabID kPrefabID = savedPermissions[num].Key.Get();
			if (kPrefabID != null)
			{
				MinionIdentity component = kPrefabID.GetComponent<MinionIdentity>();
				if (component != null)
				{
					pooledList.Add(new Tuple<MinionAssignablesProxy, Permission>(component.assignableProxy.Get(), savedPermissions[num].Value));
					savedPermissions.RemoveAt(num);
					ClearGridRestrictions(kPrefabID);
				}
			}
		}
		foreach (Tuple<MinionAssignablesProxy, Permission> item in pooledList)
		{
			SetPermission(item.first, item.second);
		}
		pooledList.Recycle();
	}

	private void UpgradeSavesToPostRobotDoorPermissions()
	{
		if (_defaultPermission != Permission.Both)
		{
			SetDefaultPermission(GameTags.Minions.Models.Standard, _defaultPermission);
			SetDefaultPermission(GameTags.Minions.Models.Bionic, _defaultPermission);
			_defaultPermission = Permission.Both;
		}
		foreach (KeyValuePair<Ref<KPrefabID>, Permission> savedPermission in savedPermissions)
		{
			SetPermission(savedPermission.Key.Get().GetComponent<MinionAssignablesProxy>(), savedPermission.Value);
		}
		savedPermissions.Clear();
	}

	protected override void OnSpawn()
	{
		isTeleporter = GetComponent<NavTeleporter>() != null;
		base.OnSpawn();
		if (savedPermissions.Count > 0)
		{
			CheckForBadData();
		}
		if (registered)
		{
			RegisterInGrid(register: true);
			RestorePermissions();
		}
		UpgradeSavePreRobotDoorPermission();
		UpgradeSavesToPostRobotDoorPermissions();
		SetStatusItem();
	}

	protected override void OnCleanUp()
	{
		RegisterInGrid(register: false);
		base.OnCleanUp();
	}

	private void OnControlStateChanged(object data)
	{
		overrideAccess = ((Boxed<Door.ControlState>)data).value;
		SetStatusItem();
	}

	private void OnCopySettings(object data)
	{
		GameObject gameObject = (GameObject)data;
		AccessControl component = gameObject.GetComponent<AccessControl>();
		if (!(component != null))
		{
			return;
		}
		savedPermissionsById.Clear();
		foreach (KeyValuePair<int, Permission> item in component.savedPermissionsById)
		{
			SetPermission(item.Key, item.Value);
		}
		defaultPermissionByTag = new List<KeyValuePair<Tag, Permission>>(component.defaultPermissionByTag);
		foreach (KeyValuePair<Tag, Permission> item2 in defaultPermissionByTag)
		{
			SetGridRestrictions(GetTagId(item2.Key), item2.Value);
		}
	}

	public void SetRegistered(bool newRegistered)
	{
		if (newRegistered && !registered)
		{
			RegisterInGrid(register: true);
			RestorePermissions();
		}
		else if (!newRegistered && registered)
		{
			RegisterInGrid(register: false);
		}
	}

	private void SetPermission(int id, Permission permission)
	{
		bool flag = false;
		for (int i = 0; i < savedPermissionsById.Count; i++)
		{
			if (savedPermissionsById[i].Key == id)
			{
				flag = true;
				KeyValuePair<int, Permission> keyValuePair = savedPermissionsById[i];
				savedPermissionsById[i] = new KeyValuePair<int, Permission>(keyValuePair.Key, permission);
				break;
			}
		}
		if (!flag)
		{
			savedPermissionsById.Add(new KeyValuePair<int, Permission>(id, permission));
		}
		SetStatusItem();
		SetGridRestrictions(id, permission);
	}

	public void SetPermission(MinionAssignablesProxy key, Permission permission)
	{
		SetPermission(key.GetComponent<KPrefabID>().InstanceID, permission);
	}

	public void SetPermission(Tag gameTag, Permission permission)
	{
		SetPermission(GetTagId(gameTag), permission);
	}

	private void RestorePermissions()
	{
		foreach (KeyValuePair<Tag, Permission> item in defaultPermissionByTag)
		{
			SetGridRestrictions(GetTagId(item.Key), item.Value);
		}
		foreach (KeyValuePair<int, Permission> item2 in savedPermissionsById)
		{
			SetGridRestrictions(item2.Key, item2.Value);
		}
	}

	private void RegisterInGrid(bool register)
	{
		Building component = GetComponent<Building>();
		OccupyArea component2 = GetComponent<OccupyArea>();
		if (component2 == null && component == null)
		{
			return;
		}
		if (register)
		{
			Rotatable component3 = GetComponent<Rotatable>();
			Grid.Restriction.Orientation orientation = (isTeleporter ? Grid.Restriction.Orientation.SingleCell : ((!(component3 == null) && component3.GetOrientation() != Orientation.Neutral) ? Grid.Restriction.Orientation.Horizontal : Grid.Restriction.Orientation.Vertical));
			if (component != null)
			{
				registeredBuildingCells = component.PlacementCells;
				int[] array = registeredBuildingCells;
				foreach (int cell in array)
				{
					Grid.RegisterRestriction(cell, orientation);
				}
			}
			else
			{
				CellOffset[] occupiedCellsOffsets = component2.OccupiedCellsOffsets;
				foreach (CellOffset offset in occupiedCellsOffsets)
				{
					int cell2 = Grid.OffsetCell(Grid.PosToCell(component2), offset);
					Grid.RegisterRestriction(cell2, orientation);
				}
			}
			if (isTeleporter)
			{
				int cell3 = GetComponent<NavTeleporter>().GetCell();
				Grid.RegisterRestriction(cell3, orientation);
			}
		}
		else
		{
			if (component != null)
			{
				if (component.GetMyWorldId() != 255 && registeredBuildingCells != null)
				{
					int[] array2 = registeredBuildingCells;
					foreach (int cell4 in array2)
					{
						Grid.UnregisterRestriction(cell4);
					}
					registeredBuildingCells = null;
				}
			}
			else
			{
				CellOffset[] occupiedCellsOffsets2 = component2.OccupiedCellsOffsets;
				foreach (CellOffset offset2 in occupiedCellsOffsets2)
				{
					int cell5 = Grid.OffsetCell(Grid.PosToCell(component2), offset2);
					Grid.UnregisterRestriction(cell5);
				}
			}
			if (isTeleporter)
			{
				int cell6 = GetComponent<NavTeleporter>().GetCell();
				if (cell6 != Grid.InvalidCell)
				{
					Grid.UnregisterRestriction(cell6);
				}
			}
		}
		registered = register;
	}

	private void SetGridRestrictions(int id, Permission permission)
	{
		if (!registered || !base.isSpawned)
		{
			return;
		}
		Building component = GetComponent<Building>();
		OccupyArea component2 = GetComponent<OccupyArea>();
		if (component2 == null && component == null)
		{
			return;
		}
		Grid.Restriction.Directions directions = (Grid.Restriction.Directions)0;
		switch (permission)
		{
		case Permission.Both:
			directions = (Grid.Restriction.Directions)0;
			break;
		case Permission.GoLeft:
			directions = Grid.Restriction.Directions.Right;
			break;
		case Permission.GoRight:
			directions = Grid.Restriction.Directions.Left;
			break;
		case Permission.Neither:
			directions = Grid.Restriction.Directions.Left | Grid.Restriction.Directions.Right;
			break;
		}
		if (isTeleporter)
		{
			directions = ((directions != 0) ? Grid.Restriction.Directions.Teleport : ((Grid.Restriction.Directions)0));
		}
		if (component != null)
		{
			int[] array = registeredBuildingCells;
			foreach (int cell in array)
			{
				Grid.SetRestriction(cell, id, directions);
			}
		}
		else
		{
			CellOffset[] occupiedCellsOffsets = component2.OccupiedCellsOffsets;
			foreach (CellOffset offset in occupiedCellsOffsets)
			{
				int cell2 = Grid.OffsetCell(Grid.PosToCell(component2), offset);
				Grid.SetRestriction(cell2, id, directions);
			}
		}
		if (isTeleporter)
		{
			int cell3 = GetComponent<NavTeleporter>().GetCell();
			Grid.SetRestriction(cell3, id, directions);
		}
	}

	private void ClearGridRestrictions(KPrefabID kpid)
	{
		if (kpid == null)
		{
			return;
		}
		Building component = GetComponent<Building>();
		OccupyArea component2 = GetComponent<OccupyArea>();
		if (component2 == null && component == null)
		{
			return;
		}
		int instanceID = kpid.InstanceID;
		if (component != null)
		{
			int[] array = registeredBuildingCells;
			foreach (int cell in array)
			{
				Grid.ClearRestriction(cell, instanceID);
			}
			return;
		}
		CellOffset[] occupiedCellsOffsets = component2.OccupiedCellsOffsets;
		foreach (CellOffset offset in occupiedCellsOffsets)
		{
			int cell2 = Grid.OffsetCell(Grid.PosToCell(component2), offset);
			Grid.ClearRestriction(cell2, instanceID);
		}
	}

	private void ClearGridRestrictions(int id, Tag default_id)
	{
		Building component = GetComponent<Building>();
		OccupyArea component2 = GetComponent<OccupyArea>();
		if (component2 == null && component == null)
		{
			return;
		}
		int minionInstanceID = GetTagId(default_id);
		if (id != Tag.Invalid.GetHash())
		{
			minionInstanceID = id;
		}
		if (component != null)
		{
			int[] array = registeredBuildingCells;
			foreach (int cell in array)
			{
				Grid.ClearRestriction(cell, minionInstanceID);
			}
			return;
		}
		CellOffset[] occupiedCellsOffsets = component2.OccupiedCellsOffsets;
		foreach (CellOffset offset in occupiedCellsOffsets)
		{
			int cell2 = Grid.OffsetCell(Grid.PosToCell(component2), offset);
			Grid.ClearRestriction(cell2, minionInstanceID);
		}
	}

	public Permission GetSetPermission(MinionAssignablesProxy key)
	{
		return GetSetPermission(key.GetComponent<KPrefabID>().InstanceID, key.GetMinionModel());
	}

	public Permission GetSetPermission(Tag robotTag)
	{
		return GetSetPermission(GetTagId(robotTag), GameTags.Robot);
	}

	public Permission GetSetPermission(int primary_id, Tag secondary_id)
	{
		Permission result = GetDefaultPermission(secondary_id);
		for (int i = 0; i < savedPermissionsById.Count; i++)
		{
			if (savedPermissionsById[i].Key == primary_id)
			{
				result = savedPermissionsById[i].Value;
				break;
			}
		}
		return result;
	}

	public void ClearPermission(MinionAssignablesProxy key)
	{
		KPrefabID component = key.GetComponent<KPrefabID>();
		if (component != null)
		{
			ClearPermission(component.InstanceID, key.GetMinionModel());
		}
		SetStatusItem();
		ClearGridRestrictions(component.InstanceID, key.GetMinionModel());
	}

	public void ClearPermission(Tag tag, Tag default_key)
	{
		int tagId = GetTagId(tag);
		ClearPermission(tagId, default_key);
	}

	private void ClearPermission(int key, Tag default_key)
	{
		for (int i = 0; i < savedPermissionsById.Count; i++)
		{
			if (savedPermissionsById[i].Key == key)
			{
				savedPermissionsById.RemoveAt(i);
				break;
			}
		}
		SetStatusItem();
		ClearGridRestrictions(key, default_key);
	}

	public bool IsDefaultPermission(MinionAssignablesProxy key)
	{
		KPrefabID component = key.GetComponent<KPrefabID>();
		if (component != null)
		{
			return IsDefaultPermission(component.InstanceID);
		}
		return true;
	}

	public bool IsDefaultPermission(Tag robotTag)
	{
		return IsDefaultPermission(GetTagId(robotTag));
	}

	private bool IsDefaultPermission(int id)
	{
		bool flag = false;
		for (int i = 0; i < savedPermissionsById.Count; i++)
		{
			if (savedPermissionsById[i].Key == id)
			{
				flag = true;
				break;
			}
		}
		return !flag;
	}

	private void SetStatusItem()
	{
		if (overrideAccess == Door.ControlState.Locked)
		{
			selectable.SetStatusItem(Db.Get().StatusItemCategories.AccessControl, null);
		}
		else if (defaultPermissionByTag.Any((KeyValuePair<Tag, Permission> default_permission) => default_permission.Value != Permission.Both) || savedPermissionsById.Count > 0)
		{
			selectable.SetStatusItem(Db.Get().StatusItemCategories.AccessControl, accessControlActive);
		}
		else
		{
			selectable.SetStatusItem(Db.Get().StatusItemCategories.AccessControl, null);
		}
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		Descriptor item = default(Descriptor);
		item.SetupDescriptor(UI.BUILDINGEFFECTS.ACCESS_CONTROL, UI.BUILDINGEFFECTS.TOOLTIPS.ACCESS_CONTROL);
		list.Add(item);
		return list;
	}
}
