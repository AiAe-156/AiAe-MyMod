using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/EntityConfigManager")]
public class EntityConfigManager : KMonoBehaviour
{
	private struct ConfigEntry
	{
		public Type type;

		public int sortOrder;
	}

	public static EntityConfigManager Instance;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		Instance = this;
	}

	private static int GetSortOrder(Type type)
	{
		object[] customAttributes = type.GetCustomAttributes(inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			Attribute attribute = (Attribute)customAttributes[i];
			if (attribute.GetType() == typeof(EntityConfigOrder))
			{
				return (attribute as EntityConfigOrder).sortOrder;
			}
		}
		return 0;
	}

	public void LoadGeneratedEntities(List<Type> types)
	{
		Type typeFromHandle = typeof(IEntityConfig);
		Type typeFromHandle2 = typeof(IMultiEntityConfig);
		List<ConfigEntry> list = new List<ConfigEntry>();
		foreach (Type type in types)
		{
			if ((typeFromHandle.IsAssignableFrom(type) || typeFromHandle2.IsAssignableFrom(type)) && !type.IsAbstract && !type.IsInterface)
			{
				int sortOrder = GetSortOrder(type);
				ConfigEntry item = new ConfigEntry
				{
					type = type,
					sortOrder = sortOrder
				};
				list.Add(item);
			}
		}
		list.Sort((ConfigEntry x, ConfigEntry y) => x.sortOrder.CompareTo(y.sortOrder));
		foreach (ConfigEntry item2 in list)
		{
			object obj = Activator.CreateInstance(item2.type);
			if (obj is IEntityConfig)
			{
				IEntityConfig entityConfig = obj as IEntityConfig;
				string[] requiredDlcIds = null;
				string[] forbiddenDlcIds = null;
				if (entityConfig.GetDlcIds() != null)
				{
					DlcManager.ConvertAvailableToRequireAndForbidden(entityConfig.GetDlcIds(), out requiredDlcIds, out forbiddenDlcIds);
					DebugUtil.DevLogError($"{item2.type} implements GetDlcIds, which is obsolete.");
				}
				else if (obj is IHasDlcRestrictions hasDlcRestrictions)
				{
					requiredDlcIds = hasDlcRestrictions.GetRequiredDlcIds();
					forbiddenDlcIds = hasDlcRestrictions.GetForbiddenDlcIds();
				}
				if (DlcManager.IsCorrectDlcSubscribed(requiredDlcIds, forbiddenDlcIds))
				{
					RegisterEntity(entityConfig, requiredDlcIds, forbiddenDlcIds);
				}
			}
			if (obj is IMultiEntityConfig config)
			{
				DebugUtil.Assert(!(obj is IHasDlcRestrictions), "IMultiEntityConfig cannot implement IHasDlcRestrictions, wrap the individual config instead.");
				RegisterEntities(config);
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void ValidateEntityConfig(IEntityConfig entityConfig)
	{
		if (entityConfig == null)
		{
			throw new ArgumentNullException("entityConfig");
		}
		Type type = entityConfig.GetType();
		Type typeFromHandle = typeof(IHasDlcRestrictions);
		bool num = type.GetMethod("GetRequiredDlcIds", Type.EmptyTypes) != null;
		bool flag = type.GetMethod("GetForbiddenDlcIds", Type.EmptyTypes) != null;
		bool flag2 = typeFromHandle.IsAssignableFrom(type);
		if ((num || flag) && !flag2)
		{
			DebugUtil.LogErrorArgs(type.Name + " is an IEntityConfig and has GetRequiredDlcIds or GetForbiddenDlcIds but does not implement IHasDlcRestrictions.");
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void ValidateMultiEntityConfig(IMultiEntityConfig entityConfig)
	{
		if (entityConfig == null)
		{
			throw new ArgumentNullException("entityConfig");
		}
		Type type = entityConfig.GetType();
		bool num = type.GetMethod("GetRequiredDlcIds", Type.EmptyTypes) != null;
		bool flag = type.GetMethod("GetForbiddenDlcIds", Type.EmptyTypes) != null;
		if (num || flag)
		{
			DebugUtil.LogErrorArgs(type.Name + " is an IMultiEntityConfig and you shouldn't be specifying GetRequiredDlcIds or GetForbiddenDlcIds. Wrap each config in a DLC check instead.");
		}
	}

	public void RegisterEntity(IEntityConfig config, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
	{
		GameObject gameObject = config.CreatePrefab();
		if (!(gameObject == null))
		{
			KPrefabID component = gameObject.GetComponent<KPrefabID>();
			component.requiredDlcIds = requiredDlcIds;
			component.forbiddenDlcIds = forbiddenDlcIds;
			component.prefabInitFn += config.OnPrefabInit;
			component.prefabSpawnFn += config.OnSpawn;
			Assets.AddPrefab(component);
		}
	}

	public void RegisterEntities(IMultiEntityConfig config)
	{
		foreach (GameObject item in config.CreatePrefabs())
		{
			KPrefabID component = item.GetComponent<KPrefabID>();
			component.prefabInitFn += config.OnPrefabInit;
			component.prefabSpawnFn += config.OnSpawn;
			Assets.AddPrefab(component);
		}
	}
}
