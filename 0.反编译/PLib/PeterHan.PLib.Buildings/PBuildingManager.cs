using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.PatchManager;
using UnityEngine;

namespace PeterHan.PLib.Buildings;

/// <summary>
/// Manages PLib buildings to break down PBuilding into a more reasonable sized class.
/// </summary>
public sealed class PBuildingManager : PForwardedComponent
{
	/// <summary>
	/// A Patch Manager patch which registers all PBuilding technologies.
	/// </summary>
	private sealed class BuildingTechRegistration : IPatchMethodInstance
	{
		public void Run(Harmony instance)
		{
			Instance?.AddAllTechs();
		}
	}

	/// <summary>
	/// The version of this component. Uses the running PLib version.
	/// </summary>
	internal static readonly Version VERSION = new Version("4.19.0.0");

	/// <summary>
	/// The buildings which need to be registered.
	/// </summary>
	private readonly ICollection<PBuilding> buildings;

	/// <summary>
	/// The instantiated copy of this class.
	/// </summary>
	internal static PBuildingManager Instance { get; private set; }

	public override Version Version => VERSION;

	/// <summary>
	/// Immediately adds an <i>existing</i> building ID to an existing technology ID in the
	/// tech tree.
	///
	/// Do <b>not</b> use this method on buildings registered through PBuilding as they
	/// are added automatically.
	///
	/// This method must be used in a Db.Initialize postfix patch or RunAt.AfterDbInit
	/// PPatchManager method/patch.
	/// </summary>
	/// <param name="tech">The technology tree node ID.</param>
	/// <param name="id">The building ID to add to that node.</param>
	public static void AddExistingBuildingToTech(string tech, string id)
	{
		if (string.IsNullOrEmpty(tech))
		{
			throw new ArgumentNullException("tech");
		}
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		(((ResourceSet<Tech>)(object)Db.Get().Techs)?.TryGet(id))?.unlockedItemIDs?.Add(tech);
	}

	private static void CreateBuildingDef_Postfix(BuildingDef __result, string anim, string id)
	{
		KAnimFile[] array = __result?.AnimFiles;
		if (array != null && array.Length != 0 && (Object)(object)array[0] == (Object)null)
		{
			Debug.LogWarningFormat("(when looking for KAnim named {0} on building {1})", new object[2] { anim, id });
		}
	}

	private static void CreateEquipmentDef_Postfix(EquipmentDef __result, string Anim, string Id)
	{
		if ((Object)(object)__result?.Anim == (Object)null)
		{
			Debug.LogWarningFormat("(when looking for KAnim named {0} on equipment {1})", new object[2] { Anim, Id });
		}
	}

	/// <summary>
	/// Logs a message encountered by the PLib building system.
	/// </summary>
	/// <param name="message">The debug message.</param>
	internal static void LogBuildingDebug(string message)
	{
		Debug.LogFormat("[PLibBuildings] {0}", new object[1] { message });
	}

	private static void LoadGeneratedBuildings_Prefix()
	{
		Instance?.AddAllStrings();
	}

	/// <summary>
	/// Creates a building manager to register PLib buildings.
	/// </summary>
	public PBuildingManager()
	{
		buildings = new List<PBuilding>(16);
	}

	/// <summary>
	/// Adds the strings for every registered building in all mods to the database.
	/// </summary>
	private void AddAllStrings()
	{
		InvokeAllProcess(0u, null);
	}

	/// <summary>
	/// Adds the strings for each registered building in this mod to the database.
	/// </summary>
	private void AddStrings()
	{
		int count = buildings.Count;
		if (count <= 0)
		{
			return;
		}
		LogBuildingDebug("Register strings for {0:D} building(s) from {1}".F(count, Assembly.GetExecutingAssembly().GetNameSafe() ?? "?"));
		foreach (PBuilding building in buildings)
		{
			if (building != null)
			{
				building.AddStrings();
				building.AddPlan();
			}
		}
	}

	/// <summary>
	/// Adds the techs for every registered building in all mods to the database.
	/// </summary>
	private void AddAllTechs()
	{
		InvokeAllProcess(1u, null);
	}

	/// <summary>
	/// Adds the techs for each registered building in this mod to the database.
	/// </summary>
	private void AddTechs()
	{
		int count = buildings.Count;
		if (count <= 0)
		{
			return;
		}
		LogBuildingDebug("Register techs for {0:D} building(s) from {1}".F(count, Assembly.GetExecutingAssembly().GetNameSafe() ?? "?"));
		foreach (PBuilding building in buildings)
		{
			building?.AddTech();
		}
	}

	/// <summary>
	/// Registers a building to properly display its name, description, and tech tree
	/// entry. PLib must be initialized using InitLibrary before using this method. Each
	/// building should only be registered once, either in OnLoad or a post-load patch.
	/// </summary>
	/// <param name="building">The building to register.</param>
	public void Register(PBuilding building)
	{
		if (building == null)
		{
			throw new ArgumentNullException("building");
		}
		RegisterForForwarding();
		buildings.Add(building);
	}

	public override void Initialize(Harmony plibInstance)
	{
		Instance = this;
		try
		{
			plibInstance.Patch(typeof(BuildingTemplates), "CreateBuildingDef", null, PatchMethod("CreateBuildingDef_Postfix"));
			plibInstance.Patch(typeof(EquipmentTemplates), "CreateEquipmentDef", null, PatchMethod("CreateEquipmentDef_Postfix"));
		}
		catch (Exception)
		{
		}
		plibInstance.Patch(typeof(GeneratedBuildings), "LoadGeneratedBuildings", PatchMethod("LoadGeneratedBuildings_Prefix"));
		new PPatchManager(plibInstance).RegisterPatch(3u, new BuildingTechRegistration());
	}

	public override void Process(uint operation, object _)
	{
		switch (operation)
		{
		case 0u:
			AddStrings();
			break;
		case 1u:
			AddTechs();
			break;
		}
	}
}
