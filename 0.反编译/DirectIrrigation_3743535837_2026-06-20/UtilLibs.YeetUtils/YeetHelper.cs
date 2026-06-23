using System;
using UnityEngine;

namespace UtilLibs.YeetUtils;

public class YeetHelper
{
	public static GameObject Spawn(Tag tag, Vector3 position, SceneLayer sceneLayer = (SceneLayer)24, bool setActive = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		GameObject prefab = Assets.GetPrefab(tag);
		if ((Object)(object)prefab == (Object)null)
		{
			return null;
		}
		GameObject val = GameUtil.KInstantiate(Assets.GetPrefab(tag), position, sceneLayer, (string)null, 0);
		val.SetActive(setActive);
		return val;
	}

	public static GameObject Spawn(Tag tag, GameObject atGO, SceneLayer sceneLayer = (SceneLayer)24, bool setActive = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return Spawn(tag, atGO.transform.position, sceneLayer, setActive);
	}

	public static void YeetRandomly(GameObject go, bool onlyUp, float minDistance, float maxDistance, bool rotate, bool stopOnLand = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		Vector2 normalized = ((Vector2)(ref insideUnitCircle)).normalized;
		if (onlyUp)
		{
			normalized.y = Mathf.Abs(normalized.y);
		}
		normalized += new Vector2(0f, Random.Range(0f, 1f));
		normalized *= Random.Range(minDistance, maxDistance);
		Yeet(go, minDistance, rotate, normalized, stopOnLand);
	}

	public static void YeetAtAngle(GameObject go, float angle, float distance, bool rotate, bool stopOnLand = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vector2 vec = DegreeToVector2(angle) * distance;
		Yeet(go, distance, rotate, vec, stopOnLand);
	}

	private static void Yeet(GameObject go, float distance, bool rotate, Vector2 vec, bool stopOnLand = true)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (((KComponentManager<FallerComponent>)(object)GameComps.Fallers).Has((object)go))
		{
			((KGameObjectComponentManager<FallerComponent>)(object)GameComps.Fallers).Remove(go);
		}
		GameComps.Fallers.Add(go, vec);
		if (rotate)
		{
			Rotator rotator = EntityTemplateExtensions.AddOrGet<Rotator>(go);
			rotator.minDistance = distance;
			rotator.SetVec(vec);
			rotator.stopOnLand = stopOnLand;
		}
	}

	public static Vector2 RadianToVector2(float radian)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
	}

	public static Vector2 DegreeToVector2(float degree)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return RadianToVector2(degree * (MathF.PI / 180f));
	}
}
