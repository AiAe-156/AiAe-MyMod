using System;
using System.Collections.Generic;
using PeterHan.PLib.Core;
using STRINGS;
using TUNING;
using UnityEngine;

namespace PeterHan.PLib.Buildings;

/// <summary>
/// A class used for creating new buildings. Abstracts many of the details to allow them
/// to be used across different game versions.
/// </summary>
/// <summary>
/// Utility methods for creating new buildings.
/// </summary>
public sealed class PBuilding
{
	/// <summary>
	/// Whether the building was added to the plan menu.
	/// </summary>
	private bool addedPlan;

	/// <summary>
	/// Whether the strings were added.
	/// </summary>
	private bool addedStrings;

	/// <summary>
	/// Whether the technology wes added.
	/// </summary>
	private bool addedTech;

	/// <summary>
	/// The default building category.
	/// </summary>
	private static readonly HashedString DEFAULT_CATEGORY = new HashedString("Base");

	/// <summary>
	/// The building ID which should precede this building ID in the plan menu.
	/// </summary>
	public string AddAfter { get; set; }

	/// <summary>
	/// Whether the building is always operational.
	/// </summary>
	public bool AlwaysOperational { get; set; }

	/// <summary>
	/// The building's animation.
	/// </summary>
	public string Animation { get; set; }

	/// <summary>
	/// The audio sounds used when placing/completing the building.
	/// </summary>
	public string AudioCategory { get; set; }

	/// <summary>
	/// The audio volume used when placing/completing the building.
	/// </summary>
	public string AudioSize { get; set; }

	/// <summary>
	/// Whether this building can break down.
	/// </summary>
	public bool Breaks { get; set; }

	/// <summary>
	/// The build menu category.
	/// </summary>
	public HashedString Category { get; set; }

	/// <summary>
	/// The construction time in seconds on x1 speed.
	/// </summary>
	public float ConstructionTime { get; set; }

	/// <summary>
	/// The decor of this building.
	/// </summary>
	public EffectorValues Decor { get; set; }

	/// <summary>
	/// The building description.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Text describing the building's effect.
	/// </summary>
	public string EffectText { get; set; }

	/// <summary>
	/// Whether this building can entomb.
	/// </summary>
	public bool Entombs { get; set; }

	/// <summary>
	/// The heat generation from the exhaust in kDTU/s.
	/// </summary>
	public float ExhaustHeatGeneration { get; set; }

	/// <summary>
	/// Whether this building can flood.
	/// </summary>
	public bool Floods { get; set; }

	/// <summary>
	/// The default priority of this building, with null to not add a priority.
	/// </summary>
	public int? DefaultPriority { get; set; }

	/// <summary>
	/// The self-heating when active in kDTU/s.
	/// </summary>
	public float HeatGeneration { get; set; }

	/// <summary>
	/// The building height.
	/// </summary>
	public int Height { get; set; }

	/// <summary>
	/// The building HP until it breaks down.
	/// </summary>
	public int HP { get; set; }

	/// <summary>
	/// The ingredients required for construction.
	/// </summary>
	public IList<BuildIngredient> Ingredients { get; }

	/// <summary>
	/// The building ID.
	/// </summary>
	public string ID { get; }

	/// <summary>
	/// Whether this building is an industrial machine.
	/// </summary>
	public bool IndustrialMachine { get; set; }

	/// <summary>
	/// The input conduits.
	/// </summary>
	public IList<ConduitConnection> InputConduits { get; }

	/// <summary>
	/// Whether this building is (or can be) a solid tile.
	/// </summary>
	public bool IsSolidTile { get; set; }

	/// <summary>
	/// The logic ports.
	/// </summary>
	public IList<Port> LogicIO { get; }

	/// <summary>
	/// The building name.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// The noise of this building (not used by Klei).
	/// </summary>
	public EffectorValues Noise { get; set; }

	/// <summary>
	/// The layer for this building.
	/// </summary>
	public ObjectLayer ObjectLayer { get; set; }

	/// <summary>
	/// The output conduits.
	/// </summary>
	public IList<ConduitConnection> OutputConduits { get; }

	/// <summary>
	/// If null, the building does not overheat; otherwise, it overheats at this
	/// temperature in K.
	/// </summary>
	public float? OverheatTemperature { get; set; }

	/// <summary>
	/// The location where this building may be built.
	/// </summary>
	public BuildLocationRule Placement { get; set; }

	/// <summary>
	/// If null, the building has no power input; otherwise, it uses this much power.
	/// </summary>
	public PowerRequirement PowerInput { get; set; }

	/// <summary>
	/// If null, the building has no power output; otherwise, it provides this much power.
	/// </summary>
	public PowerRequirement PowerOutput { get; set; }

	/// <summary>
	/// The directions this building can face.
	/// </summary>
	public PermittedRotations RotateMode { get; set; }

	/// <summary>
	/// The scene layer for this building.
	/// </summary>
	public SceneLayer SceneLayer { get; set; }

	/// <summary>
	/// The subcategory for this building.
	///
	/// The base game currently defines the following:
	/// Base:
	/// ladders, tiles, printing pods, doors, storage, tubes, default
	/// Oxygen:
	/// producers, scrubbers
	/// Power:
	/// generators, wires, batteries, transformers, switches
	/// Food:
	/// cooking, farming, ranching
	/// Plumbing:
	/// bathroom, pipes, pumps, valves, sensors
	/// HVAC:
	/// pipes, pumps, valves, sensors
	/// Refining:
	/// materials, oil, advanced
	/// Medical:
	/// cleaning, hospital, wellness
	/// Furniture:
	/// bed, lights, dining, recreation, pots, sculpture, electronic decor, moulding,
	/// canvas, dispaly, monument, signs
	/// Equipment:
	/// research, exploration, work stations, suits general, oxygen masks, atmo suits,
	/// jet suits, lead suits
	/// Utilities:
	/// temperature, other utilities, special
	/// Automation:
	/// wires, sensors, logic gates, utilities
	/// Solid Transport:
	/// conduit, valves, utilities
	/// Rocketry:
	/// telescopes, launch pad, railguns, engines, fuel and oxidizer, cargo, utility,
	/// command, fittings
	/// Radiation:
	/// HEP, uranium, radiation
	/// </summary>
	public string SubCategory { get; set; }

	/// <summary>
	/// The technology name required to unlock the building.
	/// </summary>
	public string Tech { get; set; }

	/// <summary>
	/// The view mode used when placing this building.
	/// </summary>
	public HashedString ViewMode { get; set; }

	/// <summary>
	/// The building width.
	/// </summary>
	public int Width { get; set; }

	/// <summary>
	/// Creates a new building. All buildings thus created must be registered using
	/// PBuilding.Register and have an appropriate IBuildingConfig class.
	///
	/// Building should be created in OnLoad or a post-load patch (not in static
	/// initializers) to give the localization framework time to patch the LocString
	/// containing the building name and description.
	/// </summary>
	/// <param name="id">The building ID.</param>
	/// <param name="name">The building name.</param>
	public PBuilding(string id, string name)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		AddAfter = null;
		AlwaysOperational = false;
		Animation = "";
		AudioCategory = "Metal";
		AudioSize = "medium";
		Breaks = true;
		Category = DEFAULT_CATEGORY;
		ConstructionTime = 10f;
		Decor = DECOR.NONE;
		DefaultPriority = null;
		Description = "Default Building Description";
		EffectText = "Default Building Effect";
		Entombs = true;
		ExhaustHeatGeneration = 0f;
		Floods = true;
		HeatGeneration = 0f;
		Height = 1;
		Ingredients = new List<BuildIngredient>(4);
		IndustrialMachine = false;
		InputConduits = new List<ConduitConnection>(4);
		HP = 100;
		ID = id;
		LogicIO = new List<Port>(4);
		Name = name;
		Noise = NOISE_POLLUTION.NONE;
		ObjectLayer = PGameUtils.GetObjectLayer("Building", (ObjectLayer)1);
		OutputConduits = new List<ConduitConnection>(4);
		OverheatTemperature = null;
		Placement = (BuildLocationRule)1;
		PowerInput = null;
		PowerOutput = null;
		RotateMode = (PermittedRotations)0;
		SceneLayer = (SceneLayer)19;
		SubCategory = "default";
		Tech = null;
		ViewMode = None.ID;
		Width = 1;
		addedPlan = false;
		addedStrings = false;
		addedTech = false;
	}

	/// <summary>
	/// Creates the building def from this class.
	/// </summary>
	/// <returns>The Klei building def.</returns>
	public BuildingDef CreateDef()
	{
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		if (Width < 1)
		{
			throw new InvalidOperationException("Building width: " + Width);
		}
		if (Height < 1)
		{
			throw new InvalidOperationException("Building height: " + Height);
		}
		if (HP < 1)
		{
			throw new InvalidOperationException("Building HP: " + HP);
		}
		if (ConstructionTime.IsNaNOrInfinity())
		{
			throw new InvalidOperationException("Construction time: " + ConstructionTime);
		}
		int count = Ingredients.Count;
		if (count < 1)
		{
			throw new InvalidOperationException("No ingredients for build");
		}
		float[] array = new float[count];
		string[] array2 = new string[count];
		for (int i = 0; i < count; i++)
		{
			BuildIngredient buildIngredient = Ingredients[i];
			if (buildIngredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			array[i] = buildIngredient.Quantity;
			array2[i] = buildIngredient.Material;
		}
		BuildingDef val = BuildingTemplates.CreateBuildingDef(ID, Width, Height, Animation, HP, Math.Max(0.1f, ConstructionTime), array, array2, 2400f, Placement, Decor, Noise, 0.2f);
		if (IsSolidTile)
		{
			val.BaseTimeUntilRepair = -1f;
			val.UseStructureTemperature = false;
			BuildingTemplates.CreateFoundationTileDef(val);
		}
		val.AudioCategory = AudioCategory;
		val.AudioSize = AudioSize;
		if (OverheatTemperature.HasValue)
		{
			val.Overheatable = true;
			val.OverheatTemperature = OverheatTemperature ?? 348.15f;
		}
		else
		{
			val.Overheatable = false;
		}
		if (PowerInput != null)
		{
			val.RequiresPowerInput = true;
			val.EnergyConsumptionWhenActive = PowerInput.MaxWattage;
			val.PowerInputOffset = PowerInput.PlugLocation;
		}
		if (PowerOutput != null)
		{
			val.RequiresPowerOutput = true;
			val.GeneratorWattageRating = PowerOutput.MaxWattage;
			val.PowerOutputOffset = PowerOutput.PlugLocation;
		}
		val.Breakable = Breaks;
		val.PermittedRotations = RotateMode;
		val.ExhaustKilowattsWhenActive = ExhaustHeatGeneration;
		val.SelfHeatKilowattsWhenActive = HeatGeneration;
		val.Floodable = Floods;
		val.Entombable = Entombs;
		val.ObjectLayer = ObjectLayer;
		val.SceneLayer = SceneLayer;
		val.ViewMode = ViewMode;
		if (InputConduits.Count > 1)
		{
			throw new InvalidOperationException("Only supports one input conduit");
		}
		foreach (ConduitConnection inputConduit in InputConduits)
		{
			val.UtilityInputOffset = inputConduit.Location;
			val.InputConduitType = inputConduit.Type;
		}
		if (OutputConduits.Count > 1)
		{
			throw new InvalidOperationException("Only supports one output conduit");
		}
		foreach (ConduitConnection outputConduit in OutputConduits)
		{
			val.UtilityOutputOffset = outputConduit.Location;
			val.OutputConduitType = outputConduit.Type;
		}
		BUILDINGS.PLANSUBCATEGORYSORTING[ID] = SubCategory;
		return val;
	}

	/// <summary>
	/// Configures the building template of this building. Should be called in
	/// ConfigureBuildingTemplate.
	/// </summary>
	/// <param name="go">The game object to configure.</param>
	public void ConfigureBuildingTemplate(GameObject go)
	{
		if (AlwaysOperational)
		{
			ApplyAlwaysOperational(go);
		}
	}

	/// <summary>
	/// Populates the logic ports of this building. Must be used <b>after</b> the
	/// PBuilding.DoPostConfigureComplete method if logic ports are required.
	///
	/// Should be called in DoPostConfigureComplete, DoPostConfigurePreview, and
	/// DoPostConfigureUnderConstruction.
	/// </summary>
	/// <param name="go">The game object to configure.</param>
	public void CreateLogicPorts(GameObject go)
	{
		SplitLogicPorts(go);
	}

	/// <summary>
	/// Performs the post-configure complete steps that this building object can do.
	/// Not exhaustive! Other components must likely be added.
	///
	/// This method does NOT add the logic ports. Use CreateLogicPorts to do so,
	/// <b>after</b> this method has been invoked.
	/// </summary>
	/// <param name="go">The game object to configure.</param>
	public void DoPostConfigureComplete(GameObject go)
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		if (InputConduits.Count == 1)
		{
			ConduitConsumer val = EntityTemplateExtensions.AddOrGet<ConduitConsumer>(go);
			foreach (ConduitConnection inputConduit in InputConduits)
			{
				val.alwaysConsume = true;
				val.conduitType = inputConduit.Type;
				val.wrongElementResult = (WrongElementResult)2;
			}
		}
		if (OutputConduits.Count == 1)
		{
			ConduitDispenser val2 = EntityTemplateExtensions.AddOrGet<ConduitDispenser>(go);
			foreach (ConduitConnection outputConduit in OutputConduits)
			{
				val2.alwaysDispense = true;
				val2.conduitType = outputConduit.Type;
				val2.elementFilter = null;
			}
		}
		KPrefabID val3 = default(KPrefabID);
		if (IndustrialMachine && go.TryGetComponent<KPrefabID>(ref val3))
		{
			val3.AddTag(ConstraintTags.IndustrialMachinery, false);
		}
		if (PowerInput != null)
		{
			EntityTemplateExtensions.AddOrGet<EnergyConsumer>(go);
		}
		if (PowerOutput != null)
		{
			EntityTemplateExtensions.AddOrGet<EnergyGenerator>(go);
		}
		Prioritizable val4 = default(Prioritizable);
		if (DefaultPriority.HasValue && go.TryGetComponent<Prioritizable>(ref val4))
		{
			Prioritizable.AddRef(go);
			val4.SetMasterPriority(new PrioritySetting((PriorityClass)0, DefaultPriority ?? 5));
		}
	}

	public override string ToString()
	{
		return "PBuilding[ID={0}]".F(ID);
	}

	/// <summary>
	/// Makes the building always operational.
	/// </summary>
	/// <param name="go">The game object to configure.</param>
	private static void ApplyAlwaysOperational(GameObject go)
	{
		BuildingEnabledButton val = default(BuildingEnabledButton);
		if (go.TryGetComponent<BuildingEnabledButton>(ref val))
		{
			Object.DestroyImmediate((Object)(object)val);
		}
		Operational val2 = default(Operational);
		if (go.TryGetComponent<Operational>(ref val2))
		{
			Object.DestroyImmediate((Object)(object)val2);
		}
		LogicPorts val3 = default(LogicPorts);
		if (go.TryGetComponent<LogicPorts>(ref val3))
		{
			Object.DestroyImmediate((Object)(object)val3);
		}
	}

	public static Port CompatLogicPort(LogicPortSpriteType type, CellOffset offset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return new Port(LogicOperationalController.PORT_ID, offset, LocString.op_Implicit(LOGIC_PORTS.CONTROL_OPERATIONAL), LocString.op_Implicit(LOGIC_PORTS.CONTROL_OPERATIONAL_ACTIVE), LocString.op_Implicit(LOGIC_PORTS.CONTROL_OPERATIONAL_INACTIVE), false, type, false);
	}

	/// <summary>
	/// Adds the building to the plan menu.
	/// </summary>
	public unsafe void AddPlan()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (addedPlan)
		{
			return;
		}
		HashedString category = Category;
		if (!((HashedString)(ref category)).IsValid)
		{
			return;
		}
		bool flag = false;
		foreach (PlanInfo item in BUILDINGS.PLANORDER)
		{
			if (item.category == Category)
			{
				AddPlanToCategory(item);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			category = Category;
			PUtil.LogWarning("Unable to find build menu: " + ((object)(*(HashedString*)(&category))/*cast due to .constrained prefix*/).ToString());
		}
		addedPlan = true;
	}

	private void AddPlanToCategory(PlanInfo menu)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		List<KeyValuePair<string, string>> buildingAndSubcategoryData = menu.buildingAndSubcategoryData;
		if (buildingAndSubcategoryData != null)
		{
			string addAfter = AddAfter;
			bool flag = false;
			if (addAfter != null)
			{
				int count = buildingAndSubcategoryData.Count;
				for (int i = 0; i < count - 1; i++)
				{
					if (flag)
					{
						break;
					}
					if (buildingAndSubcategoryData[i].Key == addAfter)
					{
						buildingAndSubcategoryData.Insert(i + 1, new KeyValuePair<string, string>(ID, SubCategory));
						flag = true;
					}
				}
			}
			if (!flag)
			{
				buildingAndSubcategoryData.Add(new KeyValuePair<string, string>(ID, SubCategory));
			}
		}
		else
		{
			PUtil.LogWarning("Build menu " + ((object)Category/*cast due to .constrained prefix*/).ToString() + " has invalid entries!");
		}
	}

	/// <summary>
	/// Adds the building strings to the strings list.
	/// </summary>
	public void AddStrings()
	{
		if (!addedStrings)
		{
			string text = "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".";
			string text2 = text + "NAME";
			StringEntry val = default(StringEntry);
			if (Strings.TryGet(text2, ref val))
			{
				Name = val.String;
			}
			else
			{
				Strings.Add(new string[2] { text2, Name });
			}
			if (Description != null)
			{
				Strings.Add(new string[2]
				{
					text + "DESC",
					Description
				});
			}
			if (EffectText != null)
			{
				Strings.Add(new string[2]
				{
					text + "EFFECT",
					EffectText
				});
			}
			addedStrings = true;
		}
	}

	/// <summary>
	/// Adds the building tech to the tech tree.
	/// </summary>
	public void AddTech()
	{
		if (!addedTech && Tech != null)
		{
			(((ResourceSet<Tech>)(object)Db.Get().Techs)?.TryGet(Tech))?.unlockedItemIDs?.Add(ID);
			addedTech = true;
		}
	}

	/// <summary>
	/// Splits up logic input/output ports and configures the game object with them.
	/// </summary>
	/// <param name="go">The game object to configure.</param>
	private void SplitLogicPorts(GameObject go)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		int count = LogicIO.Count;
		List<Port> list = new List<Port>(count);
		List<Port> list2 = new List<Port>(count);
		foreach (Port item in LogicIO)
		{
			if ((int)item.spriteType == 1)
			{
				list2.Add(item);
			}
			else
			{
				list.Add(item);
			}
		}
		LogicPorts val = EntityTemplateExtensions.AddOrGet<LogicPorts>(go);
		if (list.Count > 0)
		{
			val.inputPortInfo = list.ToArray();
		}
		if (list2.Count > 0)
		{
			val.outputPortInfo = list2.ToArray();
		}
	}
}
