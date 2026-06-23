using UnityEngine;
using UnityEngine.UI;

namespace FUtility;

public class ModDebug
{
	public static LineRenderer AddSimpleLineRenderer(Transform transform, Color start, Color end, float width = 0.05f)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Beached_DebugLineRenderer");
		val.transform.position = new Vector3(transform.position.x, transform.position.y, Grid.GetLayerZ((SceneLayer)34));
		transform.SetParent(transform);
		val.SetActive(true);
		LineRenderer obj = val.AddComponent<LineRenderer>();
		((Renderer)obj).material = new Material(Shader.Find("Sprites/Default"));
		((Renderer)obj).material.renderQueue = 3501;
		obj.startColor = start;
		obj.endColor = end;
		float startWidth = (obj.endWidth = width);
		obj.startWidth = startWidth;
		obj.positionCount = 2;
		((Renderer)((Component)obj).GetComponent<LineRenderer>()).material.renderQueue = RenderQueues.Liquid;
		return obj;
	}

	public static void Square(LineRenderer debugLineRenderer, Vector3 position, float radius)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		debugLineRenderer.positionCount = 4;
		debugLineRenderer.loop = true;
		float layerZ = Grid.GetLayerZ((SceneLayer)34);
		debugLineRenderer.SetPositions((Vector3[])(object)new Vector3[4]
		{
			new Vector3(position.x - radius, position.y - radius, layerZ),
			new Vector3(position.x + radius, position.y - radius, layerZ),
			new Vector3(position.x + radius, position.y + radius, layerZ),
			new Vector3(position.x - radius, position.y + radius, layerZ)
		});
	}

	public static Text AddText(Vector3 position, Color color, string msg)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject();
		RectTransform val2 = val.AddComponent<RectTransform>();
		((Transform)val2).SetParent((Transform)(object)GameScreenManager.Instance.worldSpaceCanvas.GetComponent<RectTransform>());
		TransformExtensions.SetPosition(val.transform, position);
		((Transform)val2).localScale = new Vector3(0.02f, 0.02f, 1f);
		Text obj = val.AddComponent<Text>();
		obj.font = Assets.DebugFont;
		obj.text = msg;
		((Graphic)obj).color = color;
		obj.horizontalOverflow = (HorizontalWrapMode)1;
		obj.verticalOverflow = (VerticalWrapMode)1;
		obj.alignment = (TextAnchor)4;
		return obj;
	}
}
