using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace UtilLibs.UIcmp;

public class SideScreen
{
	public static void AddClonedSideScreen<T>(string name, string originalName, Type originalType)
	{
		if (GetElements(out var screens, out var contentBody))
		{
			SideScreenContent original = FindOriginal(originalName, screens);
			SideScreenContent prefab = Copy<T>(original, contentBody, name, originalType);
			screens.Add(NewSideScreen(name, prefab));
		}
	}

	public static void AddClonedSideScreen<T>(string name, Type originalType)
	{
		if (GetElements(out var screens, out var contentBody))
		{
			SideScreenContent original = FindOriginal(originalType, screens);
			SideScreenContent prefab = Copy<T>(original, contentBody, name, originalType);
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

	private static bool GetElements(out List<SideScreenRef> screens, out GameObject contentBody)
	{
		Traverse val = Traverse.Create((object)DetailsScreen.Instance);
		screens = val.Field("sideScreens").GetValue<List<SideScreenRef>>();
		contentBody = val.Field("sideScreenContentBody").GetValue<GameObject>();
		return screens != null && (Object)(object)contentBody != (Object)null;
	}

	private static SideScreenContent FindOriginal(string name, List<SideScreenRef> screens)
	{
		foreach (SideScreenRef screen in screens)
		{
			SgtLogger.debuglog(screen.name + ((object)screen?.screenPrefab).GetType());
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
			SgtLogger.debuglog(screen.name + ((object)screen).GetType());
		}
		SideScreenContent val = screens.Find((SideScreenRef s) => ((object)s?.screenPrefab).GetType() == type)?.screenPrefab;
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogWarning((object)("Could not find a sidescreen with the type " + type));
		}
		return val;
	}

	private static SideScreenContent Copy<T>(SideScreenContent original, GameObject contentBody, string name, Type originalType)
	{
		GameObject gameObject = ((Component)Util.KInstantiateUI<SideScreenContent>(((Component)original).gameObject, contentBody, false)).gameObject;
		Object.Destroy((Object)(object)gameObject.GetComponent(originalType));
		Component obj = gameObject.AddComponent(typeof(T));
		SideScreenContent val = (SideScreenContent)(object)((obj is SideScreenContent) ? obj : null);
		((Object)val).name = name.Trim();
		gameObject.SetActive(false);
		return val;
	}

	private static SideScreenRef NewSideScreen(string name, SideScreenContent prefab)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		return new SideScreenRef
		{
			name = name,
			offset = Vector2.zero,
			screenPrefab = prefab
		};
	}
}
