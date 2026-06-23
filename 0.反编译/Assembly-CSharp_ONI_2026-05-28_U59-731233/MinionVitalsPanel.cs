using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Klei.AI;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("KMonoBehaviour/scripts/MinionVitalsPanel")]
public class MinionVitalsPanel : CollapsibleDetailContentPanel
{
	[DebuggerDisplay("{amount.Name}")]
	public struct AmountLine
	{
		public Amount amount;

		public GameObject go;

		public ValueTrendImageToggle imageToggle;

		public LocText locText;

		public ToolTip toolTip;

		public Func<AmountInstance, string> toolTipFunc;

		public Func<AmountInstance, Sprite> spriteFunc;

		public bool TryUpdate(Amounts amounts)
		{
			foreach (AmountInstance modifier in amounts.ModifierList)
			{
				if (amount == modifier.amount && !modifier.hide)
				{
					locText.SetText(amount.GetDescription(modifier));
					toolTip.toolTip = toolTipFunc(modifier);
					imageToggle.SetValue(modifier);
					Sprite sprite = ((spriteFunc == null) ? null : spriteFunc(modifier));
					if (sprite != null)
					{
						go.GetComponentInChildren<Image>().sprite = sprite;
					}
					return true;
				}
			}
			return false;
		}
	}

	[DebuggerDisplay("{attribute.Name}")]
	public struct AttributeLine
	{
		public Klei.AI.Attribute attribute;

		public GameObject go;

		public LocText locText;

		public ToolTip toolTip;

		public Func<AttributeInstance, string> toolTipFunc;

		public bool TryUpdate(Attributes attributes)
		{
			foreach (AttributeInstance attribute in attributes)
			{
				if (this.attribute == attribute.modifier && !attribute.hide)
				{
					locText.SetText(this.attribute.GetDescription(attribute));
					toolTip.toolTip = toolTipFunc(attribute);
					return true;
				}
			}
			return false;
		}
	}

	public struct CheckboxLine
	{
		public Amount amount;

		public GameObject go;

		public LocText locText;

		public Func<GameObject, string> tooltip;

		public Func<GameObject, bool> get_value;

		public Func<GameObject, CheckboxLineDisplayType> display_condition;

		public Func<GameObject, string> label_text_func;

		public Transform parentContainer;
	}

	public enum CheckboxLineDisplayType
	{
		Normal,
		Diminished,
		Hidden
	}

	public GameObject LineItemPrefab;

	public GameObject CheckboxLinePrefab;

	private GameObject lastSelectedEntity = null;

	public List<AmountLine> amountsLines = new List<AmountLine>();

	public List<AttributeLine> attributesLines = new List<AttributeLine>();

	public List<CheckboxLine> checkboxLines = new List<CheckboxLine>();

	public Transform conditionsContainerNormal;

	public Transform conditionsContainerAdditional;

	private string unpollinatedTooltip;

	public string UnpollinatedTooltip
	{
		get
		{
			if (string.IsNullOrEmpty(unpollinatedTooltip))
			{
				StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
				List<GameObject> prefabsWithTag = Assets.GetPrefabsWithTag(GameTags.Creatures.Pollinator);
				foreach (GameObject item in prefabsWithTag)
				{
					KPrefabID component = item.GetComponent<KPrefabID>();
					if (!(component == null) && Game.IsCorrectDlcActiveForCurrentSave(component))
					{
						stringBuilder.AppendFormat("\n{0}{1}", "    • ", item.GetProperName());
					}
				}
				unpollinatedTooltip = string.Format(UI.TOOLTIPS.VITALS_CHECKBOX_UNPOLLINATED, GlobalStringBuilderPool.ReturnAndFree(stringBuilder));
			}
			return unpollinatedTooltip;
		}
	}

	public void Init()
	{
		AddAmountLine(Db.Get().Amounts.HitPoints);
		AddAmountLine(Db.Get().Amounts.BionicInternalBattery);
		AddAmountLine(Db.Get().Amounts.BionicOil);
		AddAmountLine(Db.Get().Amounts.BionicGunk);
		AddAttributeLine(Db.Get().CritterAttributes.Happiness);
		AddAmountLine(Db.Get().Amounts.Wildness);
		AddAmountLine(Db.Get().Amounts.Incubation);
		AddAmountLine(Db.Get().Amounts.Viability);
		AddAmountLine(Db.Get().Amounts.PowerCharge);
		AddAmountLine(Db.Get().Amounts.Fertility);
		AddAmountLine(Db.Get().Amounts.Beckoning);
		AddAmountLine(Db.Get().Amounts.Age);
		AddAmountLine(Db.Get().Amounts.Stress);
		AddAttributeLine(Db.Get().Attributes.QualityOfLife);
		AddAmountLine(Db.Get().Amounts.Bladder);
		AddAmountLine(Db.Get().Amounts.Breath);
		AddAmountLine(Db.Get().Amounts.BionicOxygenTank);
		AddAmountLine(Db.Get().Amounts.Stamina);
		AddAttributeLine(Db.Get().CritterAttributes.Metabolism);
		AddAmountLine(Db.Get().Amounts.Calories);
		AddAmountLine(Db.Get().Amounts.ScaleGrowth);
		AddAmountLine(Db.Get().Amounts.MilkProduction);
		AddAmountLine(Db.Get().Amounts.ElementGrowth);
		AddAmountLine(Db.Get().Amounts.Moisture);
		AddAmountLine(Db.Get().Amounts.Temperature);
		AddAmountLine(Db.Get().Amounts.CritterTemperature);
		AddAmountLine(Db.Get().Amounts.Decor);
		AddAmountLine(Db.Get().Amounts.InternalBattery);
		AddAmountLine(Db.Get().Amounts.InternalChemicalBattery);
		AddAmountLine(Db.Get().Amounts.InternalBioBattery);
		AddAmountLine(Db.Get().Amounts.InternalElectroBank);
		if (DlcManager.FeatureRadiationEnabled())
		{
			AddAmountLine(Db.Get().Amounts.RadiationBalance);
		}
		AddCheckboxLine(Db.Get().Amounts.AirPressure, conditionsContainerNormal, (GameObject go) => GetAirPressureLabel(go), (GameObject go) => (!(go.GetComponent<PressureVulnerable>() != null) || !go.GetComponent<PressureVulnerable>().pressure_sensitive) ? CheckboxLineDisplayType.Hidden : CheckboxLineDisplayType.Normal, (GameObject go) => check_pressure(go), (GameObject go) => GetAirPressureTooltip(go));
		AddCheckboxLine(null, conditionsContainerNormal, (GameObject go) => GetAtmosphereLabel(go), (GameObject go) => (!(go.GetComponent<PressureVulnerable>() != null) || go.GetComponent<PressureVulnerable>().safe_atmospheres.Count <= 0) ? CheckboxLineDisplayType.Hidden : CheckboxLineDisplayType.Normal, (GameObject go) => check_atmosphere(go), (GameObject go) => GetAtmosphereTooltip(go));
		AddCheckboxLine(Db.Get().Amounts.Temperature, conditionsContainerNormal, (GameObject go) => GetInternalTemperatureLabel(go), (GameObject go) => (!(go.GetComponent<TemperatureVulnerable>() != null)) ? CheckboxLineDisplayType.Hidden : CheckboxLineDisplayType.Normal, (GameObject go) => check_temperature(go), (GameObject go) => GetInternalTemperatureTooltip(go));
		AddCheckboxLine(Db.Get().Amounts.Fertilization, conditionsContainerAdditional, (GameObject go) => GetFertilizationLabel(go), delegate(GameObject go)
		{
			if (go.GetComponent<ReceptacleMonitor>() == null)
			{
				return CheckboxLineDisplayType.Hidden;
			}
			return (!go.GetComponent<ReceptacleMonitor>().Replanted) ? CheckboxLineDisplayType.Diminished : CheckboxLineDisplayType.Normal;
		}, (GameObject go) => check_fertilizer(go), (GameObject go) => GetFertilizationTooltip(go));
		AddCheckboxLine(Db.Get().Amounts.Irrigation, conditionsContainerAdditional, (GameObject go) => GetIrrigationLabel(go), delegate(GameObject go)
		{
			ReceptacleMonitor component = go.GetComponent<ReceptacleMonitor>();
			return (!(component != null) || !component.Replanted) ? CheckboxLineDisplayType.Diminished : CheckboxLineDisplayType.Normal;
		}, (GameObject go) => check_irrigation(go), (GameObject go) => GetIrrigationTooltip(go));
		AddCheckboxLine(Db.Get().Amounts.Illumination, conditionsContainerNormal, (GameObject go) => GetIlluminationLabel(go), (GameObject go) => CheckboxLineDisplayType.Normal, (GameObject go) => check_illumination(go), (GameObject go) => GetIlluminationTooltip(go));
		AddCheckboxLine(null, conditionsContainerNormal, (GameObject go) => GetRadiationLabel(go), delegate(GameObject go)
		{
			AttributeInstance attributeInstance = go.GetAttributes().Get(Db.Get().PlantAttributes.MaxRadiationThreshold);
			return (attributeInstance == null || !(attributeInstance.GetTotalValue() > 0f)) ? CheckboxLineDisplayType.Hidden : CheckboxLineDisplayType.Normal;
		}, (GameObject go) => check_radiation(go), (GameObject go) => GetRadiationTooltip(go));
		AddCheckboxLine(null, conditionsContainerNormal, (GameObject go) => GetEntityConsumptionLabel(go), (GameObject go) => (go.GetComponent<IPlantConsumeEntities>() == null) ? CheckboxLineDisplayType.Hidden : CheckboxLineDisplayType.Normal, (GameObject go) => check_entity_consumed(go), (GameObject go) => GetEntityConsumedTooltip(go));
		AddCheckboxLine(null, conditionsContainerNormal, (GameObject go) => GetPollinationLabel(go), (GameObject go) => (go.GetSMI<PollinationMonitor.StatesInstance>() == null) ? CheckboxLineDisplayType.Hidden : CheckboxLineDisplayType.Normal, (GameObject go) => go.GetComponent<WiltCondition>().IsConditionSatisifed(WiltCondition.Condition.Pollination), (GameObject go) => GetPollinationTooltip(go));
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Init();
	}

	protected override void OnCmpEnable()
	{
		base.OnCmpEnable();
		SimAndRenderScheduler.instance.Add(this);
	}

	protected override void OnCmpDisable()
	{
		base.OnCmpDisable();
		SimAndRenderScheduler.instance.Remove(this);
	}

	private void AddAmountLine(Amount amount, Func<AmountInstance, string> tooltip_func = null)
	{
		GameObject gameObject = Util.KInstantiateUI(LineItemPrefab, Content.gameObject);
		gameObject.GetComponent<ToolTip>().refreshWhileHovering = true;
		gameObject.SetActive(value: true);
		AmountLine item = new AmountLine
		{
			amount = amount,
			go = gameObject,
			locText = gameObject.GetComponentInChildren<LocText>(),
			toolTip = gameObject.GetComponentInChildren<ToolTip>(),
			imageToggle = gameObject.GetComponentInChildren<ValueTrendImageToggle>(),
			toolTipFunc = ((tooltip_func != null) ? tooltip_func : new Func<AmountInstance, string>(amount.GetTooltip))
		};
		if (!amount.CanDisplayerDisplayCustomIcons)
		{
			gameObject.GetComponentInChildren<Image>().sprite = Assets.GetSprite(amount.uiSprite);
		}
		else
		{
			item.spriteFunc = amount.GetSprite;
		}
		amountsLines.Add(item);
	}

	private void AddAttributeLine(Klei.AI.Attribute attribute, Func<AttributeInstance, string> tooltip_func = null)
	{
		GameObject gameObject = Util.KInstantiateUI(LineItemPrefab, Content.gameObject);
		gameObject.GetComponentInChildren<Image>().sprite = Assets.GetSprite(attribute.uiSprite);
		gameObject.GetComponent<ToolTip>().refreshWhileHovering = true;
		gameObject.SetActive(value: true);
		AttributeLine item = new AttributeLine
		{
			attribute = attribute,
			go = gameObject,
			locText = gameObject.GetComponentInChildren<LocText>(),
			toolTip = gameObject.GetComponentInChildren<ToolTip>()
		};
		gameObject.GetComponentInChildren<ValueTrendImageToggle>().gameObject.SetActive(value: false);
		item.toolTipFunc = ((tooltip_func != null) ? tooltip_func : new Func<AttributeInstance, string>(attribute.GetTooltip));
		attributesLines.Add(item);
	}

	private void AddCheckboxLine(Amount amount, Transform parentContainer, Func<GameObject, string> label_text_func, Func<GameObject, CheckboxLineDisplayType> display_condition, Func<GameObject, bool> checkbox_value_func, Func<GameObject, string> tooltip_func = null)
	{
		GameObject gameObject = Util.KInstantiateUI(CheckboxLinePrefab, Content.gameObject);
		HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
		gameObject.GetComponent<ToolTip>().refreshWhileHovering = true;
		gameObject.SetActive(value: true);
		CheckboxLine item = default(CheckboxLine);
		item.go = gameObject;
		item.parentContainer = parentContainer;
		item.amount = amount;
		item.locText = component.GetReference("Label") as LocText;
		item.get_value = checkbox_value_func;
		item.display_condition = display_condition;
		item.label_text_func = label_text_func;
		item.go.name = "Checkbox_";
		if (amount != null)
		{
			item.go.name += amount.Name;
		}
		else
		{
			item.go.name += "Unnamed";
		}
		if (tooltip_func != null)
		{
			item.tooltip = tooltip_func;
			ToolTip tt = item.go.GetComponent<ToolTip>();
			tt.refreshWhileHovering = true;
			tt.OnToolTip = delegate
			{
				tt.ClearMultiStringTooltip();
				tt.AddMultiStringTooltip(tooltip_func(lastSelectedEntity), null);
				return "";
			};
		}
		checkboxLines.Add(item);
	}

	private void ShouldShowVitalsPanel(GameObject selectedEntity)
	{
	}

	public void Refresh(GameObject selectedEntity)
	{
		if (selectedEntity == null || selectedEntity.gameObject == null)
		{
			return;
		}
		lastSelectedEntity = selectedEntity;
		WiltCondition component = selectedEntity.GetComponent<WiltCondition>();
		MinionIdentity component2 = selectedEntity.GetComponent<MinionIdentity>();
		CreatureBrain component3 = selectedEntity.GetComponent<CreatureBrain>();
		IncubationMonitor.Instance sMI = selectedEntity.GetSMI<IncubationMonitor.Instance>();
		object[] array = new object[4] { component, component2, component3, sMI };
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			SetActive(active: false);
			return;
		}
		SetActive(active: true);
		SetTitle((component == null) ? UI.DETAILTABS.SIMPLEINFO.GROUPNAME_CONDITION : UI.DETAILTABS.SIMPLEINFO.GROUPNAME_REQUIREMENTS);
		Amounts amounts = selectedEntity.GetAmounts();
		Attributes attributes = selectedEntity.GetAttributes();
		if (amounts == null || attributes == null)
		{
			return;
		}
		if (component == null)
		{
			conditionsContainerNormal.gameObject.SetActive(value: false);
			conditionsContainerAdditional.gameObject.SetActive(value: false);
			foreach (AmountLine amountsLine in amountsLines)
			{
				bool flag2 = amountsLine.TryUpdate(amounts);
				if (amountsLine.go.activeSelf != flag2)
				{
					amountsLine.go.SetActive(flag2);
				}
			}
			foreach (AttributeLine attributesLine in attributesLines)
			{
				bool flag3 = attributesLine.TryUpdate(attributes);
				if (attributesLine.go.activeSelf != flag3)
				{
					attributesLine.go.SetActive(flag3);
				}
			}
		}
		bool flag4 = false;
		for (int j = 0; j < checkboxLines.Count; j++)
		{
			CheckboxLine checkboxLine = checkboxLines[j];
			CheckboxLineDisplayType checkboxLineDisplayType = CheckboxLineDisplayType.Hidden;
			if (checkboxLines[j].amount != null)
			{
				for (int k = 0; k < amounts.Count; k++)
				{
					AmountInstance amountInstance = amounts[k];
					if (checkboxLine.amount == amountInstance.amount)
					{
						checkboxLineDisplayType = checkboxLine.display_condition(selectedEntity.gameObject);
						break;
					}
				}
			}
			else
			{
				checkboxLineDisplayType = checkboxLine.display_condition(selectedEntity.gameObject);
			}
			if (checkboxLineDisplayType != CheckboxLineDisplayType.Hidden)
			{
				checkboxLine.locText.SetText(checkboxLine.label_text_func(selectedEntity.gameObject));
				if (!checkboxLine.go.activeSelf)
				{
					checkboxLine.go.SetActive(value: true);
				}
				GameObject gameObject = checkboxLine.go.GetComponent<HierarchyReferences>().GetReference("Check").gameObject;
				gameObject.SetActive(checkboxLine.get_value(selectedEntity.gameObject));
				if (checkboxLine.go.transform.parent != checkboxLine.parentContainer)
				{
					checkboxLine.go.transform.SetParent(checkboxLine.parentContainer);
					checkboxLine.go.transform.localScale = Vector3.one;
				}
				if (checkboxLine.parentContainer == conditionsContainerAdditional)
				{
					flag4 = true;
				}
				if (checkboxLineDisplayType == CheckboxLineDisplayType.Normal)
				{
					if (checkboxLine.get_value(selectedEntity.gameObject))
					{
						checkboxLine.locText.color = Color.black;
						gameObject.transform.parent.GetComponent<Image>().color = Color.black;
					}
					else
					{
						Color color = new Color(0.99215686f, 0f, 0.101960786f);
						checkboxLine.locText.color = color;
						gameObject.transform.parent.GetComponent<Image>().color = color;
					}
				}
				else
				{
					checkboxLine.locText.color = Color.grey;
					gameObject.transform.parent.GetComponent<Image>().color = Color.grey;
				}
			}
			else if (checkboxLine.go.activeSelf)
			{
				checkboxLine.go.SetActive(value: false);
			}
		}
		if (!(component != null))
		{
			return;
		}
		IManageGrowingStates component4 = component.GetComponent<IManageGrowingStates>();
		component4 = ((component4 != null) ? component4 : component.GetSMI<IManageGrowingStates>());
		bool flag5 = component.HasTag(GameTags.Decoration);
		conditionsContainerNormal.gameObject.SetActive(value: true);
		conditionsContainerAdditional.gameObject.SetActive(!flag5);
		if (component4 == null)
		{
			float num = 1f;
			LocText reference = conditionsContainerNormal.GetComponent<HierarchyReferences>().GetReference<LocText>("Label");
			reference.text = "";
			reference.text = (flag5 ? string.Format(UI.VITALSSCREEN.CONDITIONS_GROWING.WILD_DECOR.BASE) : string.Format(UI.VITALSSCREEN.CONDITIONS_GROWING.WILD_INSTANT.BASE, Util.FormatTwoDecimalPlace(num * 0.25f * 100f)));
			reference.GetComponent<ToolTip>().SetSimpleTooltip(string.Format(UI.VITALSSCREEN.CONDITIONS_GROWING.WILD_INSTANT.TOOLTIP));
			reference = conditionsContainerAdditional.GetComponent<HierarchyReferences>().GetReference<LocText>("Label");
			ReceptacleMonitor component5 = selectedEntity.GetComponent<ReceptacleMonitor>();
			reference.color = ((component5 == null || component5.Replanted) ? Color.black : Color.grey);
			reference.text = string.Format(UI.VITALSSCREEN.CONDITIONS_GROWING.ADDITIONAL_DOMESTIC_INSTANT.BASE, Util.FormatTwoDecimalPlace(num * 100f));
			reference.GetComponent<ToolTip>().SetSimpleTooltip(string.Format(UI.VITALSSCREEN.CONDITIONS_GROWING.ADDITIONAL_DOMESTIC_INSTANT.TOOLTIP));
		}
		else
		{
			LocText reference = conditionsContainerNormal.GetComponent<HierarchyReferences>().GetReference<LocText>("Label");
			reference.text = "";
			reference.text = string.Format(UI.VITALSSCREEN.CONDITIONS_GROWING.WILD.BASE, GameUtil.GetFormattedCycles(component4.WildGrowthTime()));
			reference.GetComponent<ToolTip>().SetSimpleTooltip(string.Format(UI.VITALSSCREEN.CONDITIONS_GROWING.WILD.TOOLTIP, GameUtil.GetFormattedCycles(component4.WildGrowthTime())));
			reference = conditionsContainerAdditional.GetComponent<HierarchyReferences>().GetReference<LocText>("Label");
			reference.color = (component4.IsWildPlanted() ? Color.grey : Color.black);
			reference.text = "";
			reference.text = (flag4 ? string.Format(UI.VITALSSCREEN.CONDITIONS_GROWING.ADDITIONAL_DOMESTIC.BASE, GameUtil.GetFormattedCycles(component4.DomesticGrowthTime())) : string.Format(UI.VITALSSCREEN.CONDITIONS_GROWING.DOMESTIC.BASE, GameUtil.GetFormattedCycles(component4.DomesticGrowthTime())));
			reference.GetComponent<ToolTip>().SetSimpleTooltip(string.Format(UI.VITALSSCREEN.CONDITIONS_GROWING.ADDITIONAL_DOMESTIC.TOOLTIP, GameUtil.GetFormattedCycles(component4.DomesticGrowthTime())));
		}
		foreach (AmountLine amountsLine2 in amountsLines)
		{
			amountsLine2.go.SetActive(value: false);
		}
		foreach (AttributeLine attributesLine2 in attributesLines)
		{
			attributesLine2.go.SetActive(value: false);
		}
	}

	private string GetAirPressureTooltip(GameObject go)
	{
		PressureVulnerable component = go.GetComponent<PressureVulnerable>();
		if (component == null)
		{
			return "";
		}
		return UI.TOOLTIPS.VITALS_CHECKBOX_PRESSURE.text.Replace("{pressure}", GameUtil.GetFormattedMass(component.GetExternalPressure()));
	}

	private string GetInternalTemperatureTooltip(GameObject go)
	{
		TemperatureVulnerable component = go.GetComponent<TemperatureVulnerable>();
		if (component == null)
		{
			return "";
		}
		return UI.TOOLTIPS.VITALS_CHECKBOX_TEMPERATURE.text.Replace("{temperature}", GameUtil.GetFormattedTemperature(component.InternalTemperature));
	}

	private string GetFertilizationTooltip(GameObject go)
	{
		FertilizationMonitor.Instance sMI = go.GetSMI<FertilizationMonitor.Instance>();
		if (sMI == null)
		{
			return "";
		}
		return UI.TOOLTIPS.VITALS_CHECKBOX_FERTILIZER.text.Replace("{mass}", GameUtil.GetFormattedMass(sMI.total_fertilizer_available));
	}

	private string GetIrrigationTooltip(GameObject go)
	{
		IrrigationMonitor.Instance sMI = go.GetSMI<IrrigationMonitor.Instance>();
		if (sMI == null)
		{
			return "";
		}
		return UI.TOOLTIPS.VITALS_CHECKBOX_IRRIGATION.text.Replace("{mass}", GameUtil.GetFormattedMass(sMI.total_fertilizer_available));
	}

	private string GetIlluminationTooltip(GameObject go)
	{
		IIlluminationTracker illuminationTracker = go.GetComponent<IIlluminationTracker>();
		if (illuminationTracker == null)
		{
			illuminationTracker = go.GetSMI<IIlluminationTracker>();
		}
		if (illuminationTracker == null)
		{
			return "";
		}
		return illuminationTracker.GetIlluminationUITooltip();
	}

	private string GetRadiationTooltip(GameObject go)
	{
		int num = Grid.PosToCell(go);
		float rads = (Grid.IsValidCell(num) ? Grid.Radiation[num] : 0f);
		AttributeInstance attributeInstance = go.GetAttributes().Get(Db.Get().PlantAttributes.MinRadiationThreshold);
		AttributeInstance attributeInstance2 = go.GetAttributes().Get(Db.Get().PlantAttributes.MaxRadiationThreshold);
		MutantPlant component = go.GetComponent<MutantPlant>();
		bool flag = component != null && component.IsOriginal;
		string text = ((attributeInstance.GetTotalValue() != 0f) ? UI.TOOLTIPS.VITALS_CHECKBOX_RADIATION.Replace("{rads}", GameUtil.GetFormattedRads(rads)).Replace("{minRads}", attributeInstance.GetFormattedValue()).Replace("{maxRads}", attributeInstance2.GetFormattedValue()) : UI.TOOLTIPS.VITALS_CHECKBOX_RADIATION_NO_MIN.Replace("{rads}", GameUtil.GetFormattedRads(rads)).Replace("{maxRads}", attributeInstance2.GetFormattedValue()));
		if (flag)
		{
			text += UI.GAMEOBJECTEFFECTS.TOOLTIPS.MUTANT_SEED_TOOLTIP;
		}
		return text;
	}

	private string GetReceptacleTooltip(GameObject go)
	{
		ReceptacleMonitor component = go.GetComponent<ReceptacleMonitor>();
		if (component == null)
		{
			return "";
		}
		if (component.HasOperationalReceptacle())
		{
			return UI.TOOLTIPS.VITALS_CHECKBOX_RECEPTACLE_OPERATIONAL;
		}
		return UI.TOOLTIPS.VITALS_CHECKBOX_RECEPTACLE_INOPERATIONAL;
	}

	private string GetEntityConsumedTooltip(GameObject go)
	{
		IPlantConsumeEntities component = go.GetComponent<IPlantConsumeEntities>();
		bool flag = component.AreEntitiesConsumptionRequirementsSatisfied();
		return GameUtil.SafeStringFormat(UI.TOOLTIPS.VITALS_CHECKBOX_ENTITY_CONSUMER_REQUIREMENTS, component.GetConsumableEntitiesCategoryName());
	}

	private string GetPollinationTooltip(GameObject go)
	{
		WiltCondition component = go.GetComponent<WiltCondition>();
		return component.IsConditionSatisifed(WiltCondition.Condition.Pollination) ? ((string)UI.TOOLTIPS.VITALS_CHECKBOX_POLLINATED) : UnpollinatedTooltip;
	}

	private string GetAtmosphereTooltip(GameObject go)
	{
		PressureVulnerable component = go.GetComponent<PressureVulnerable>();
		if (component != null && component.currentAtmoElement != null)
		{
			return UI.TOOLTIPS.VITALS_CHECKBOX_ATMOSPHERE.text.Replace("{element}", component.currentAtmoElement.name);
		}
		return UI.TOOLTIPS.VITALS_CHECKBOX_ATMOSPHERE;
	}

	private string GetAirPressureLabel(GameObject go)
	{
		PressureVulnerable component = go.GetComponent<PressureVulnerable>();
		return Db.Get().Amounts.AirPressure.Name + "\n    • " + GameUtil.GetFormattedMass(component.pressureWarning_Low, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Gram, includeSuffix: false) + " - " + GameUtil.GetFormattedMass(component.pressureWarning_High, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Gram);
	}

	private string GetInternalTemperatureLabel(GameObject go)
	{
		TemperatureVulnerable component = go.GetComponent<TemperatureVulnerable>();
		return Db.Get().Amounts.Temperature.Name + "\n    • " + GameUtil.GetFormattedTemperature(component.TemperatureWarningLow, GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Absolute, displayUnits: false) + " - " + GameUtil.GetFormattedTemperature(component.TemperatureWarningHigh);
	}

	private string GetFertilizationLabel(GameObject go)
	{
		FertilizationMonitor.Instance sMI = go.GetSMI<FertilizationMonitor.Instance>();
		string text = Db.Get().Amounts.Fertilization.Name;
		AttributeInstance attributeInstance = go.GetAttributes().Get(Db.Get().PlantAttributes.FertilizerUsageMod);
		float totalValue = attributeInstance.GetTotalValue();
		PlantElementAbsorber.ConsumeInfo[] consumedElements = sMI.def.consumedElements;
		for (int i = 0; i < consumedElements.Length; i++)
		{
			PlantElementAbsorber.ConsumeInfo consumeInfo = consumedElements[i];
			text = text + "\n    • " + ElementLoader.GetElement(consumeInfo.tag).name + " " + GameUtil.GetFormattedMass(consumeInfo.massConsumptionRate * totalValue, GameUtil.TimeSlice.PerCycle);
		}
		return text;
	}

	private string GetIrrigationLabel(GameObject go)
	{
		IrrigationMonitor.Instance sMI = go.GetSMI<IrrigationMonitor.Instance>();
		string text = Db.Get().Amounts.Irrigation.Name;
		AttributeInstance attributeInstance = go.GetAttributes().Get(Db.Get().PlantAttributes.FertilizerUsageMod);
		float totalValue = attributeInstance.GetTotalValue();
		PlantElementAbsorber.ConsumeInfo[] consumedElements = sMI.def.consumedElements;
		for (int i = 0; i < consumedElements.Length; i++)
		{
			PlantElementAbsorber.ConsumeInfo consumeInfo = consumedElements[i];
			text = text + "\n    • " + ElementLoader.GetElement(consumeInfo.tag).name + ": " + GameUtil.GetFormattedMass(consumeInfo.massConsumptionRate * totalValue, GameUtil.TimeSlice.PerCycle);
		}
		return text;
	}

	private string GetIlluminationLabel(GameObject go)
	{
		IIlluminationTracker illuminationTracker = go.GetComponent<IIlluminationTracker>();
		if (illuminationTracker == null)
		{
			illuminationTracker = go.GetSMI<IIlluminationTracker>();
		}
		return illuminationTracker.GetIlluminationUILabel();
	}

	private string GetEntityConsumptionLabel(GameObject go)
	{
		IPlantConsumeEntities component = go.GetComponent<IPlantConsumeEntities>();
		string requirementText = component.GetRequirementText();
		return requirementText + "\n    • " + (check_entity_consumed(go) ? GameUtil.SafeStringFormat(UI.TOOLTIPS.VITALS_CHECKBOX_ENTITY_CONSUMER_SATISFIED, component.GetConsumedEntityName()) : ((string)UI.TOOLTIPS.VITALS_CHECKBOX_ENTITY_CONSUMER_UNSATISFIED));
	}

	private string GetPollinationLabel(GameObject go)
	{
		return UI.VITALSSCREEN.POLLINATION;
	}

	private string GetAtmosphereLabel(GameObject go)
	{
		PressureVulnerable component = go.GetComponent<PressureVulnerable>();
		string text = UI.VITALSSCREEN.ATMOSPHERE_CONDITION;
		foreach (Element safe_atmosphere in component.safe_atmospheres)
		{
			text = text + "\n    • " + safe_atmosphere.name;
		}
		return text;
	}

	private string GetRadiationLabel(GameObject go)
	{
		AttributeInstance attributeInstance = go.GetAttributes().Get(Db.Get().PlantAttributes.MinRadiationThreshold);
		AttributeInstance attributeInstance2 = go.GetAttributes().Get(Db.Get().PlantAttributes.MaxRadiationThreshold);
		if (attributeInstance.GetTotalValue() == 0f)
		{
			return string.Concat(UI.GAMEOBJECTEFFECTS.AMBIENT_RADIATION, "\n    • ", UI.GAMEOBJECTEFFECTS.AMBIENT_NO_MIN_RADIATION_FMT.Replace("{maxRads}", attributeInstance2.GetFormattedValue()));
		}
		return string.Concat(UI.GAMEOBJECTEFFECTS.AMBIENT_RADIATION, "\n    • ", UI.GAMEOBJECTEFFECTS.AMBIENT_RADIATION_FMT.Replace("{minRads}", attributeInstance.GetFormattedValue()).Replace("{maxRads}", attributeInstance2.GetFormattedValue()));
	}

	private bool check_pressure(GameObject go)
	{
		PressureVulnerable component = go.GetComponent<PressureVulnerable>();
		if (component != null)
		{
			return component.ExternalPressureState == PressureVulnerable.PressureState.Normal;
		}
		return true;
	}

	private bool check_temperature(GameObject go)
	{
		TemperatureVulnerable component = go.GetComponent<TemperatureVulnerable>();
		if (component != null)
		{
			return component.GetInternalTemperatureState == TemperatureVulnerable.TemperatureState.Normal;
		}
		return true;
	}

	private bool check_irrigation(GameObject go)
	{
		IrrigationMonitor.Instance sMI = go.GetSMI<IrrigationMonitor.Instance>();
		if (sMI != null)
		{
			return !sMI.IsInsideState(sMI.sm.replanted.starved) && !sMI.IsInsideState(sMI.sm.wild);
		}
		return true;
	}

	private bool check_illumination(GameObject go)
	{
		IIlluminationTracker illuminationTracker = go.GetComponent<IIlluminationTracker>();
		if (illuminationTracker == null)
		{
			illuminationTracker = go.GetSMI<IIlluminationTracker>();
		}
		return illuminationTracker?.ShouldIlluminationUICheckboxBeChecked() ?? true;
	}

	private bool check_radiation(GameObject go)
	{
		AttributeInstance attributeInstance = go.GetAttributes().Get(Db.Get().PlantAttributes.MinRadiationThreshold);
		if (attributeInstance != null && attributeInstance.GetTotalValue() != 0f)
		{
			int num = Grid.PosToCell(go);
			float num2 = (Grid.IsValidCell(num) ? Grid.Radiation[num] : 0f);
			return num2 >= attributeInstance.GetTotalValue();
		}
		return true;
	}

	private bool check_receptacle(GameObject go)
	{
		ReceptacleMonitor component = go.GetComponent<ReceptacleMonitor>();
		if (component == null)
		{
			return false;
		}
		return component.HasOperationalReceptacle();
	}

	private bool check_fertilizer(GameObject go)
	{
		FertilizationMonitor.Instance sMI = go.GetSMI<FertilizationMonitor.Instance>();
		return sMI?.sm.isFertilized.Get(sMI) ?? false;
	}

	private bool check_atmosphere(GameObject go)
	{
		PressureVulnerable component = go.GetComponent<PressureVulnerable>();
		if (component != null)
		{
			return component.testAreaElementSafe;
		}
		return true;
	}

	private bool check_entity_consumed(GameObject go)
	{
		IPlantConsumeEntities component = go.GetComponent<IPlantConsumeEntities>();
		return component.AreEntitiesConsumptionRequirementsSatisfied();
	}
}
