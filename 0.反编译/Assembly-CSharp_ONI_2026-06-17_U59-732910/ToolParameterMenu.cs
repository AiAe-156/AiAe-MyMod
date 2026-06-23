using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("KMonoBehaviour/scripts/ToolParameterMenu")]
public class ToolParameterMenu : KMonoBehaviour
{
	public class FILTERLAYERS
	{
		public static string BUILDINGS = "BUILDINGS";

		public static string TILES = "TILES";

		public static string WIRES = "WIRES";

		public static string LIQUIDCONDUIT = "LIQUIDPIPES";

		public static string GASCONDUIT = "GASPIPES";

		public static string SOLIDCONDUIT = "SOLIDCONDUITS";

		public static string CLEANANDCLEAR = "CLEANANDCLEAR";

		public static string DIGPLACER = "DIGPLACER";

		public static string LOGIC = "LOGIC";

		public static string BACKWALL = "BACKWALL";

		public static string NATURALBACKWALL = "NATURALBACKWALL";

		public static string UPROOTPLANTS = "UPROOTPLANTS";

		public static string CONSTRUCTION = "CONSTRUCTION";

		public static string DIG = "DIG";

		public static string CLEAN = "CLEAN";

		public static string OPERATE = "OPERATE";

		public static string METAL = "METAL";

		public static string BUILDABLE = "BUILDABLE";

		public static string FILTER = "FILTER";

		public static string LIQUIFIABLE = "LIQUIFIABLE";

		public static string LIQUID = "LIQUID";

		public static string CONSUMABLEORE = "CONSUMABLEORE";

		public static string ORGANICS = "ORGANICS";

		public static string FARMABLE = "FARMABLE";

		public static string GAS = "GAS";

		public static string MISC = "MISC";

		public static string HEATFLOW = "HEATFLOW";

		public static string ABSOLUTETEMPERATURE = "ABSOLUTETEMPERATURE";

		public static string RELATIVETEMPERATURE = "RELATIVETEMPERATURE";

		public static string ADAPTIVETEMPERATURE = "ADAPTIVETEMPERATURE";

		public static string STATECHANGE = "STATECHANGE";

		public static string ALL = "ALL";
	}

	public class ToggleData
	{
		public string name;

		public bool isToggleInclusive;

		public ToggleState state;

		public bool IsOn => state == ToggleState.On;

		public ToggleData()
		{
		}

		public ToggleData(string name, ToggleState state, bool isToggleInclusive = false)
		{
			this.name = name;
			this.state = state;
			this.isToggleInclusive = isToggleInclusive;
		}
	}

	private class Widget
	{
		public GameObject gameObject;

		public ToggleData data;
	}

	public enum ToggleState
	{
		On,
		Off,
		Disabled
	}

	public GameObject content;

	public GameObject widgetContainer;

	public GameObject widgetPrefab;

	private Dictionary<string, Widget> widgets = new Dictionary<string, Widget>();

	private ToggleData[] currentTogglesData;

	private string lastEnabledFilter;

	public event System.Action onParametersChanged;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		ClearMenu();
	}

	private int ToggleStateToMultiToggleInt(ToggleData data)
	{
		switch (data.state)
		{
		case ToggleState.On:
			if (!data.isToggleInclusive)
			{
				return 1;
			}
			return 3;
		case ToggleState.Off:
			return 0;
		case ToggleState.Disabled:
			return 2;
		default:
			return 0;
		}
	}

	public void PopulateMenu(ToggleData[] togglesData)
	{
		ClearMenu();
		currentTogglesData = togglesData;
		bool flag = true;
		for (int i = 0; i < togglesData.Length; i++)
		{
			if (togglesData[i].isToggleInclusive)
			{
				flag = false;
				break;
			}
		}
		widgetContainer.GetComponent<ToggleGroup>().enabled = flag;
		foreach (ToggleData toggleData in togglesData)
		{
			GameObject gameObject = CreateToggleGameObject(toggleData);
			widgets.Add(toggleData.name, new Widget
			{
				gameObject = gameObject,
				data = toggleData
			});
		}
		content.SetActive(value: true);
	}

	private GameObject CreateToggleGameObject(ToggleData data)
	{
		GameObject newWidget = Util.KInstantiateUI(widgetPrefab, widgetContainer, force_active: true);
		LocText componentInChildren = newWidget.GetComponentInChildren<LocText>();
		ToolTip componentInChildren2 = newWidget.GetComponentInChildren<ToolTip>();
		MultiToggle componentInChildren3 = newWidget.GetComponentInChildren<MultiToggle>();
		_ = data.state;
		componentInChildren.text = Strings.Get("STRINGS.UI.TOOLS.FILTERLAYERS." + data.name + ".NAME");
		if (componentInChildren2 != null)
		{
			componentInChildren2.SetSimpleTooltip(Strings.Get("STRINGS.UI.TOOLS.FILTERLAYERS." + data.name + ".TOOLTIP"));
		}
		componentInChildren3.ChangeState(ToggleStateToMultiToggleInt(data));
		componentInChildren3.onClick = (System.Action)Delegate.Combine(componentInChildren3.onClick, (System.Action)delegate
		{
			foreach (KeyValuePair<string, Widget> widget in widgets)
			{
				Widget value = widget.Value;
				ToggleData data2 = value.data;
				if (value.gameObject == newWidget)
				{
					if (data2.state != ToggleState.Disabled)
					{
						ChangeToSetting(value);
						OnChange();
					}
					break;
				}
			}
		});
		return newWidget;
	}

	public void ClearMenu()
	{
		content.SetActive(value: false);
		foreach (KeyValuePair<string, Widget> widget in widgets)
		{
			Util.KDestroyGameObject(widget.Value.gameObject);
		}
		widgets.Clear();
	}

	private void ChangeToSetting(Widget clickedWidget)
	{
		ToggleData data = clickedWidget.data;
		if (data.isToggleInclusive)
		{
			data.state = ((data.state != ToggleState.Off) ? ToggleState.Off : ToggleState.On);
			{
				foreach (KeyValuePair<string, Widget> widget in widgets)
				{
					ToggleData data2 = widget.Value.data;
					if (data2.state != ToggleState.Disabled && !data.isToggleInclusive)
					{
						data2.state = ToggleState.Off;
					}
				}
				return;
			}
		}
		foreach (KeyValuePair<string, Widget> widget2 in widgets)
		{
			ToggleData data3 = widget2.Value.data;
			if (data3.state != ToggleState.Disabled)
			{
				data3.state = ToggleState.Off;
			}
		}
		data.state = ToggleState.On;
	}

	private void OnChange()
	{
		foreach (KeyValuePair<string, Widget> widget in widgets)
		{
			Widget value = widget.Value;
			ToggleData data = value.data;
			GameObject obj = value.gameObject;
			int new_state_index = ToggleStateToMultiToggleInt(data);
			obj.GetComponentInChildren<MultiToggle>().ChangeState(new_state_index);
		}
		if (this.onParametersChanged != null)
		{
			this.onParametersChanged();
		}
	}

	public string GetLastEnabledFilter()
	{
		return lastEnabledFilter;
	}
}
