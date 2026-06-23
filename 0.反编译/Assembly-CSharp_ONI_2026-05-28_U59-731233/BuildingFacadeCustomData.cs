using System.Collections.Generic;
using UnityEngine;

public static class BuildingFacadeCustomData
{
	public const string DATA_KEY_LIGHT_COLOR = "LightColor";

	public const string DATA_KEY_LIGHT_OVERLAY_COLOR = "LightOverlayColor";

	public static void ApplyCustomData(Building building, Dictionary<string, string> newData)
	{
		UpdateLightColor(newData, building);
	}

	private static void UpdateLightColor(Dictionary<string, string> newData, Building building)
	{
		GameObject prefab = Assets.GetPrefab(building.PrefabID());
		if (!prefab.TryGetComponent<Light2D>(out var component))
		{
			return;
		}
		Color color = component.Color;
		Color overlayColour = component.overlayColour;
		if (!building.TryGetComponent<Light2D>(out var component2))
		{
			return;
		}
		if (newData == null)
		{
			component2.Color = color;
			component2.overlayColour = overlayColour;
			return;
		}
		if (newData.TryGetValue("LightColor", out var value))
		{
			Color color2 = Util.ColorFromHex(value);
			component2.Color = color2;
		}
		else
		{
			component2.Color = color;
		}
		if (newData.TryGetValue("LightOverlayColor", out var value2))
		{
			Color overlayColour2 = Util.ColorFromHex(value2);
			component2.overlayColour = overlayColour2;
		}
		else
		{
			component2.overlayColour = overlayColour;
		}
	}
}
