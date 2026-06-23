using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace UtilLibs.UIcmp;

public class Helper
{
	public struct ButtonInfo
	{
		public LocString text;

		public Action action;

		public int fontSize;

		public ButtonInfo(LocString text, Action action, int font_size)
		{
			this.text = text;
			this.action = action;
			fontSize = font_size;
		}
	}

	public static void ListComponents(GameObject obj)
	{
		Debug.Log((object)("object name: " + ((Object)obj).name));
		Debug.Log((object)"Components:");
		Component[] components = obj.GetComponents<Component>();
		foreach (Component val in components)
		{
			Debug.Log((object)((object)val).GetType());
			ListFieldsAndProps(val, ((object)val).GetType());
		}
	}

	private static void ListFieldsAndProps(object obj, Type T)
	{
		FieldInfo[] fields = T.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			Debug.Log((object)("Field[" + fieldInfo.Name + "] " + fieldInfo.GetValue(obj)));
		}
		PropertyInfo[] properties = T.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			Debug.Log((object)("Property[" + propertyInfo.Name + "] " + propertyInfo.GetValue(obj)));
		}
	}

	public static void ListChildren(Transform parent, int level = 0, int maxDepth = 10)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (level >= maxDepth)
		{
			return;
		}
		foreach (Transform item in parent)
		{
			Transform val = item;
			SgtLogger.debuglog(string.Concat(string.Concat(Enumerable.Repeat('-', level)), ((Object)val).name));
			ListChildren(val, level + 1);
		}
	}

	public static ToolTip AddSimpleToolTip2(GameObject gameObject, string message, bool alignCenter = false, float wrapWidth = 0f)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)gameObject.GetComponent<ToolTip>() != (Object)null)
		{
			SgtLogger.warning("GO already had a tooltip! skipping");
			return null;
		}
		ToolTip val = gameObject.AddComponent<ToolTip>();
		val.tooltipPivot = (alignCenter ? new Vector2(0.5f, 0f) : new Vector2(1f, 0f));
		val.tooltipPositionOffset = new Vector2(0f, 20f);
		val.parentPositionAnchor = new Vector2(0.5f, 0.5f);
		if (wrapWidth > 0f)
		{
			val.WrapWidth = wrapWidth;
			val.SizingSetting = (ToolTipSizeSetting)0;
		}
		ToolTipScreen.Instance.SetToolTip(val);
		val.SetSimpleTooltip(message);
		return val;
	}

	public static KButton MakeKButton(ButtonInfo info, GameObject buttonPrefab, GameObject parent, int index = 0)
	{
		KButton val = Util.KInstantiateUI<KButton>(buttonPrefab, parent, true);
		val.onClick += info.action;
		((KMonoBehaviour)val).transform.SetSiblingIndex(index);
		LocText componentInChildren = ((Component)val).GetComponentInChildren<LocText>();
		((TMP_Text)componentInChildren).text = LocString.op_Implicit(info.text);
		((TMP_Text)componentInChildren).fontSize = info.fontSize;
		return val;
	}

	public static T CreateFDialog<T>(GameObject prefab, string name = null, bool show = true) where T : FScreen
	{
		if ((Object)(object)prefab == (Object)null)
		{
			SgtLogger.warning("Could not display UI (" + name + "): screen prefab is null.");
			return null;
		}
		if (name == null)
		{
			name = ((Object)prefab).name;
		}
		Transform transform = ((Component)GetACanvas(name)).transform;
		GameObject val = Object.Instantiate<GameObject>(prefab, transform);
		T val2 = val.AddComponent(typeof(T)) as T;
		if (show)
		{
			val2.ShowDialog();
		}
		return val2;
	}

	public static Canvas GetACanvas(string name)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		GameObject val;
		if ((Object)(object)FrontEndManager.Instance != (Object)null)
		{
			val = ((Component)FrontEndManager.Instance).gameObject;
		}
		else if ((Object)(object)GameScreenManager.Instance != (Object)null && (Object)(object)GameScreenManager.Instance.ssOverlayCanvas != (Object)null)
		{
			val = GameScreenManager.Instance.ssOverlayCanvas;
		}
		else
		{
			val = new GameObject
			{
				name = name + "Canvas"
			};
			Object.DontDestroyOnLoad((Object)(object)val);
			Canvas val2 = val.AddComponent<Canvas>();
			val2.renderMode = (RenderMode)0;
			val2.additionalShaderChannels = (AdditionalCanvasShaderChannels)1;
			val2.sortingOrder = 3000;
		}
		return val.GetComponent<Canvas>();
	}
}
