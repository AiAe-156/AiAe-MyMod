using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace FUtility.FUI;

public class SideScreen
{
	public static void AddClonedSideScreen<T>(string name, string originalName, Type originalType, SidescreenTabTypes targetTab = (SidescreenTabTypes)0)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (GetElements(out var screens, out var tabs))
		{
			GameObject contentBodyForTab = GetContentBodyForTab(targetTab, tabs);
			SideScreenContent prefab = Copy<T>(FindOriginal(originalName, screens), contentBodyForTab, name, originalType);
			screens.Add(NewSideScreen(name, prefab));
		}
	}

	public static void AddClonedSideScreen<T>(string name, Type originalType, SidescreenTabTypes targetTab = (SidescreenTabTypes)0)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (GetElements(out var screens, out var tabs))
		{
			GameObject contentBodyForTab = GetContentBodyForTab(targetTab, tabs);
			SideScreenContent prefab = Copy<T>(FindOriginal(originalType, screens), contentBodyForTab, name, originalType);
			screens.Add(NewSideScreen(name, prefab));
		}
	}

	public static void AddCustomSideScreen<T>(string name, GameObject prefab)
	{
		if (GetElements(out var screens, out var _))
		{
			Component obj = prefab.AddComponent(typeof(T));
			SideScreenContent prefab2 = (SideScreenContent)(object)((obj is SideScreenContent) ? obj : null);
			screens.Add(NewSideScreen(name, prefab2));
		}
	}

	private static bool GetElements(out List<SideScreenRef> screens, out List<SidescreenTab> tabs)
	{
		Traverse val = Traverse.Create((object)DetailsScreen.Instance);
		screens = val.Field("sideScreens").GetValue<List<SideScreenRef>>();
		tabs = val.Field("sidescreenTabs").GetValue<SidescreenTab[]>().ToList();
		if (screens != null)
		{
			return tabs != null;
		}
		return false;
	}

	private static GameObject GetContentBodyForTab(SidescreenTabTypes targetTab, List<SidescreenTab> tabs)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		foreach (SidescreenTab tab in tabs)
		{
			if (tab.type == targetTab)
			{
				return tab.bodyInstance;
			}
		}
		Log.Warning("no targetTab " + ((object)targetTab).GetType().Name);
		return null;
	}

	private static SideScreenContent FindOriginal(string name, List<SideScreenRef> screens)
	{
		foreach (SideScreenRef screen in screens)
		{
			Log.Debuglog(screen.name, ((object)screen?.screenPrefab).GetType());
		}
		SideScreenContent screenPrefab = screens.Find((SideScreenRef s) => s.name == name).screenPrefab;
		if ((Object)(object)screenPrefab == (Object)null)
		{
			Debug.LogWarning((object)("Could not find a sidescreen with the name " + name));
		}
		return screenPrefab;
	}

	private static SideScreenContent FindOriginal(Type type, List<SideScreenRef> screens)
	{
		foreach (SideScreenRef screen in screens)
		{
			Log.Debuglog(screen.name, ((object)screen).GetType());
		}
		SideScreenContent obj = screens.Find((SideScreenRef s) => ((object)s?.screenPrefab).GetType() == type)?.screenPrefab;
		if ((Object)(object)obj == (Object)null)
		{
			Debug.LogWarning((object)("Could not find a sidescreen with the type " + type));
		}
		return obj;
	}

	private static SideScreenContent Copy<T>(SideScreenContent original, GameObject contentBody, string name, Type originalType)
	{
		GameObject gameObject = ((Component)Util.KInstantiateUI<SideScreenContent>(((Component)original).gameObject, contentBody, false)).gameObject;
		Object.Destroy((Object)(object)gameObject.GetComponent(originalType));
		Component obj = gameObject.AddComponent(typeof(T));
		Component obj2 = ((obj is SideScreenContent) ? obj : null);
		((Object)obj2).name = name.Trim();
		gameObject.SetActive(false);
		return (SideScreenContent)(object)obj2;
	}

	private static SideScreenRef NewSideScreen(string name, SideScreenContent prefab)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		return new SideScreenRef
		{
			name = name,
			offset = Vector2.zero,
			screenPrefab = prefab
		};
	}
}
