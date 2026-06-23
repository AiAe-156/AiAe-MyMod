using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using HarmonyLib;
using KMod;
using KSerialization;
using STRINGS;
using TUNING;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("Stairs")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Stairs")]
[assembly: AssemblyCopyright("Copyright ©  2019")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("1280a1ef-e563-4258-939a-da7d21a6e9ac")]
[assembly: AssemblyFileVersion("2020.1.25.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: AssemblyVersion("2020.1.25.0")]
namespace Stairs;

public class BuidingTemplates
{
	public static BuildingDef CreateStairsDef(string id, string anim, float[] construction_mass, string[] construction_materials)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		int num = 1;
		int num2 = 1;
		int num3 = 10;
		float num4 = 10f;
		float num5 = 2400f;
		BuildLocationRule val = (BuildLocationRule)0;
		EffectorValues nONE = NOISE_POLLUTION.NONE;
		BuildingDef obj = BuildingTemplates.CreateBuildingDef(id, num, num2, anim, num3, num4, construction_mass, construction_materials, num5, val, BONUS.TIER0, nONE, 0.2f);
		obj.Floodable = false;
		obj.Entombable = false;
		obj.Overheatable = false;
		obj.AudioCategory = "Metal";
		obj.AudioSize = "small";
		obj.BaseTimeUntilRepair = -1f;
		obj.DragBuild = true;
		obj.IsFoundation = false;
		obj.BaseDecor = -2f;
		obj.TileLayer = (ObjectLayer)24;
		obj.PermittedRotations = (PermittedRotations)3;
		obj.ReplacementLayer = (ObjectLayer)25;
		List<Tag> replacementTags = new List<Tag> { Patches.tag_Stairs };
		obj.ReplacementTags = replacementTags;
		List<ObjectLayer> equivalentReplacementLayers = new List<ObjectLayer> { (ObjectLayer)11 };
		obj.EquivalentReplacementLayers = equivalentReplacementLayers;
		return obj;
	}

	public static BuildingDef CreateScaffoldingDef(string id, string anim, float[] construction_mass, string[] construction_materials)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		int num = 1;
		int num2 = 1;
		int num3 = 5;
		float num4 = 12f;
		float num5 = 2400f;
		BuildLocationRule val = (BuildLocationRule)7;
		EffectorValues nONE = NOISE_POLLUTION.NONE;
		BuildingDef obj = BuildingTemplates.CreateBuildingDef(id, num, num2, anim, num3, num4, construction_mass, construction_materials, num5, val, BONUS.TIER0, nONE, 1f);
		obj.Floodable = false;
		obj.Entombable = false;
		obj.Overheatable = false;
		obj.AudioCategory = "Metal";
		obj.AudioSize = "small";
		obj.BaseTimeUntilRepair = -1f;
		obj.DragBuild = true;
		obj.IsFoundation = false;
		obj.BaseDecor = -4f;
		obj.ObjectLayer = (ObjectLayer)38;
		obj.SceneLayer = (SceneLayer)30;
		obj.ReplacementLayer = (ObjectLayer)11;
		obj.ReplacementCandidateLayers = new List<ObjectLayer> { (ObjectLayer)38 };
		List<Tag> replacementTags = new List<Tag> { Patches.tag_Scaffolding };
		obj.ReplacementTags = replacementTags;
		return obj;
	}
}
public class ScaffoldingConfig : IBuildingConfig
{
	public const string ID = "urfScaffolding";

	public override BuildingDef CreateBuildingDef()
	{
		float[] construction_mass = new float[1] { CONSTRUCTION_MASS_KG.TIER2[0] };
		string[] construction_materials = new string[1] { "Metal" };
		BuildingDef obj = BuidingTemplates.CreateScaffoldingDef("urfScaffolding", "scaffolding_kanim", construction_mass, construction_materials);
		obj.ConstructionTime = 2f;
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		EntityTemplateExtensions.AddOrGet<AnimTileable>(go).objectLayer = (ObjectLayer)38;
		KPrefabIDExtensions.AddTag(go, Patches.tag_Scaffolding);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		EntityTemplateExtensions.AddOrGet<Scaffolding>(go);
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		((IBuildingConfig)this).DoPostConfigurePreview(def, go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Scaffolding);
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		EntityTemplateExtensions.AddOrGet<AnimTileable>(go).objectLayer = (ObjectLayer)38;
		((IBuildingConfig)this).DoPostConfigureUnderConstruction(go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Scaffolding);
	}
}
public class ScaffoldingAlt1Config : IBuildingConfig
{
	public const string ID = "urfScaffolding_Alt1";

	public override BuildingDef CreateBuildingDef()
	{
		float[] construction_mass = new float[1] { CONSTRUCTION_MASS_KG.TIER2[0] };
		string[] rAW_MINERALS_OR_WOOD = MATERIALS.RAW_MINERALS_OR_WOOD;
		BuildingDef obj = BuidingTemplates.CreateScaffoldingDef("urfScaffolding_Alt1", "scaffolding_alt1_kanim", construction_mass, rAW_MINERALS_OR_WOOD);
		obj.BaseDecor = 0f;
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		EntityTemplateExtensions.AddOrGet<AnimTileable>(go).objectLayer = (ObjectLayer)38;
		KPrefabIDExtensions.AddTag(go, Patches.tag_Scaffolding);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		EntityTemplateExtensions.AddOrGet<Scaffolding>(go);
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		((IBuildingConfig)this).DoPostConfigurePreview(def, go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Scaffolding);
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		EntityTemplateExtensions.AddOrGet<AnimTileable>(go).objectLayer = (ObjectLayer)38;
		((IBuildingConfig)this).DoPostConfigureUnderConstruction(go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Scaffolding);
	}
}
public class ScaffoldingAlt2Config : IBuildingConfig
{
	public const string ID = "urfScaffolding_Alt2";

	public override BuildingDef CreateBuildingDef()
	{
		float[] construction_mass = new float[1] { CONSTRUCTION_MASS_KG.TIER2[0] };
		string[] construction_materials = new string[1] { "RefinedMetal" };
		BuildingDef obj = BuidingTemplates.CreateScaffoldingDef("urfScaffolding_Alt2", "scaffolding_alt2_kanim", construction_mass, construction_materials);
		obj.HitPoints = 50;
		obj.ConstructionTime = 25f;
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		EntityTemplateExtensions.AddOrGet<AnimTileable>(go).objectLayer = (ObjectLayer)38;
		KPrefabIDExtensions.AddTag(go, Patches.tag_Scaffolding);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		EntityTemplateExtensions.AddOrGet<Scaffolding>(go);
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		((IBuildingConfig)this).DoPostConfigurePreview(def, go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Scaffolding);
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		EntityTemplateExtensions.AddOrGet<AnimTileable>(go).objectLayer = (ObjectLayer)38;
		((IBuildingConfig)this).DoPostConfigureUnderConstruction(go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Scaffolding);
	}
}
public class StairsConfig : IBuildingConfig
{
	public const string ID = "Stairs";

	public override BuildingDef CreateBuildingDef()
	{
		float[] construction_mass = new float[2]
		{
			CONSTRUCTION_MASS_KG.TIER2[0],
			CONSTRUCTION_MASS_KG.TIER0[0]
		};
		string[] construction_materials = new string[2]
		{
			MATERIALS.RAW_MINERALS_OR_WOOD[0],
			"Metal"
		};
		return BuidingTemplates.CreateStairsDef("Stairs", "stairs_kanim", construction_mass, construction_materials);
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		EntityTemplateExtensions.AddOrGet<Stair>(go);
		EntityTemplateExtensions.AddOrGet<AnimStairs>(go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Stairs);
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		((IBuildingConfig)this).DoPostConfigurePreview(def, go);
		AnimStairs.PrearePreview(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		((IBuildingConfig)this).DoPostConfigureUnderConstruction(go);
		EntityTemplateExtensions.AddOrGet<AnimStairs>(go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Stairs);
	}
}
public class StairsAlt1Config : IBuildingConfig
{
	public const string ID = "Stairs_Alt1";

	public override BuildingDef CreateBuildingDef()
	{
		float[] construction_mass = new float[2]
		{
			CONSTRUCTION_MASS_KG.TIER0[0],
			CONSTRUCTION_MASS_KG.TIER0[0]
		};
		string[] construction_materials = new string[2] { "RefinedMetal", "Glass" };
		BuildingDef obj = BuidingTemplates.CreateStairsDef("Stairs_Alt1", "stairs_alt1_kanim", construction_mass, construction_materials);
		obj.BaseDecor = 8f;
		obj.BaseDecorRadius = 2f;
		obj.AudioCategory = "Glass";
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		EntityTemplateExtensions.AddOrGet<Stair>(go);
		EntityTemplateExtensions.AddOrGet<AnimStairs>(go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Stairs);
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		((IBuildingConfig)this).DoPostConfigurePreview(def, go);
		AnimStairs.PrearePreview(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		((IBuildingConfig)this).DoPostConfigureUnderConstruction(go);
		EntityTemplateExtensions.AddOrGet<AnimStairs>(go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Stairs);
	}
}
public class StairsClassicConfig : IBuildingConfig
{
	public const string ID = "Stairs_Classic";

	public override BuildingDef CreateBuildingDef()
	{
		float[] construction_mass = new float[2]
		{
			CONSTRUCTION_MASS_KG.TIER2[0],
			CONSTRUCTION_MASS_KG.TIER0[0]
		};
		string[] construction_materials = new string[2] { "BuildableRaw", "Metal" };
		return BuidingTemplates.CreateStairsDef("Stairs_Classic", "stairs_classic_kanim", construction_mass, construction_materials);
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		EntityTemplateExtensions.AddOrGet<Stair>(go);
		EntityTemplateExtensions.AddOrGet<AnimStairs>(go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Stairs);
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		((IBuildingConfig)this).DoPostConfigurePreview(def, go);
		AnimStairs.PrearePreview(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		((IBuildingConfig)this).DoPostConfigureUnderConstruction(go);
		EntityTemplateExtensions.AddOrGet<AnimStairs>(go);
		KPrefabIDExtensions.AddTag(go, Patches.tag_Stairs);
	}
}
public class MyTransitionLayer : OverrideLayer
{
	public const float cos45 = 0.7071f;

	public const float upwardsMovementSpeedMultiplier = 0.9f;

	public const float downwardsMovementSpeedMultiplier = 1.5f;

	public const float scaffoldingSpeedMultiplier = 1.5f;

	public bool isMovingOnStaris;

	private float time;

	private Vector3 startPos;

	private Vector3 targetPos;

	public MyTransitionLayer(Navigator navigator)
		: base(navigator)
	{
		time = Time.time;
	}

	public override void BeginTransition(Navigator navigator, ActiveTransition transition)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		((OverrideLayer)this).BeginTransition(navigator, transition);
		if ((int)transition.start != 0 || (int)transition.end != 0)
		{
			return;
		}
		int num = Grid.PosToCell((KMonoBehaviour)(object)navigator);
		if (transition.y == 0 && transition.x != 0)
		{
			if (!MyGrid.IsScaffolding(num))
			{
				return;
			}
			int num2 = Grid.CellBelow(num);
			if (!((BuildFlagsFoundationIndexer)(ref Grid.Foundation))[num2])
			{
				GameObject val = ((ObjectLayerIndexer)(ref Grid.Objects))[num, 38];
				if (!((Object)(object)val == (Object)null) && !(KPrefabIDExtensions.PrefabID(val) != Patches.tag_ScaffoldingAlt2))
				{
					transition.speed *= 1.5f;
					transition.animSpeed *= 1.5f;
				}
			}
		}
		else
		{
			if ((transition.y != 1 && transition.y != -1) || (transition.x != 1 && transition.x != -1))
			{
				return;
			}
			int cell;
			if (transition.y > 0)
			{
				cell = Grid.OffsetCell(num, transition.x, 0);
				if (transition.x > 0)
				{
					if (MyGrid.IsRightSet(cell))
					{
						return;
					}
				}
				else if (!MyGrid.IsRightSet(cell))
				{
					return;
				}
			}
			else
			{
				cell = Grid.OffsetCell(num, 0, transition.y);
				if (transition.x > 0)
				{
					if (!MyGrid.IsRightSet(cell))
					{
						return;
					}
				}
				else if (MyGrid.IsRightSet(cell))
				{
					return;
				}
			}
			if (MyGrid.IsHypotenuse(cell))
			{
				if (transition.y > 0)
				{
					transition.speed *= 0.63639f;
					transition.animSpeed *= 0.63639f;
				}
				else
				{
					transition.speed *= 1.06065f;
					transition.animSpeed *= 1.06065f;
				}
				transition.isLooping = true;
				transition.anim = HashedString.op_Implicit("floor_floor_1_0_loop");
				isMovingOnStaris = true;
				time = Time.time;
				int num3 = Grid.OffsetCell(num, transition.x, transition.y);
				targetPos = Grid.CellToPosCBC(num3, navigator.sceneLayer);
				startPos = TransformExtensions.GetPosition(((KMonoBehaviour)navigator).transform);
			}
		}
	}

	public override void EndTransition(Navigator navigator, ActiveTransition transition)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		((OverrideLayer)this).EndTransition(navigator, transition);
		if (isMovingOnStaris && MyGrid.IsHypotenuse(Grid.PosToCell(TransformExtensions.GetPosition(((KMonoBehaviour)navigator).transform))))
		{
			((KMonoBehaviour)navigator).transform.position = startPos;
		}
		isMovingOnStaris = false;
	}
}
public class MyGrid
{
	public enum Flags : byte
	{
		HasStair = 1,
		RightSet = 2,
		Walkable = 4,
		Hypotenuse = 8,
		HasScaffolding = 0x10
	}

	public static Flags[] Masks;

	public static void ForceDeconstruction(int cell, bool isStairs = true)
	{
		if (!Grid.IsValidCell(cell) || (isStairs && !IsStair(cell)))
		{
			return;
		}
		int num = (isStairs ? 1 : 38);
		GameObject val = ((ObjectLayerIndexer)(ref Grid.Objects))[cell, num];
		if (!((Object)(object)val == (Object)null) && (isStairs || !((Object)(object)val.GetComponent<Scaffolding>() == (Object)null)))
		{
			Deconstructable component = val.GetComponent<Deconstructable>();
			if (!((Object)(object)component == (Object)null) && component.IsMarkedForDeconstruction())
			{
				((Workable)component).CompleteWork((WorkerBase)null);
			}
		}
	}

	public static bool IsStair(int cell)
	{
		if ((Masks[cell] & Flags.HasStair) == 0)
		{
			return false;
		}
		return true;
	}

	public static bool IsScaffolding(int cell)
	{
		if ((Masks[cell] & Flags.HasScaffolding) == 0)
		{
			return false;
		}
		return true;
	}

	public static bool IsWalkable(int cell)
	{
		if ((Masks[cell] & Flags.Walkable) == 0)
		{
			return false;
		}
		return true;
	}

	public static bool IsRightSet(int cell)
	{
		if ((Masks[cell] & Flags.RightSet) == 0)
		{
			return false;
		}
		return true;
	}

	public static bool IsHypotenuse(int cell)
	{
		if ((Masks[cell] & Flags.Hypotenuse) == 0)
		{
			return false;
		}
		return true;
	}
}
public class Patches : UserMod2
{
	[HarmonyPatch(typeof(GridSettings))]
	[HarmonyPatch("Reset")]
	public static class GridSettings_Reset_Patch
	{
		public static void Postfix()
		{
			MyGrid.Masks = new MyGrid.Flags[Grid.CellCount];
			ChainedDeconstruction = false;
			foreach (Mod mod in Global.Instance.modManager.mods)
			{
				if (mod.IsActive() && mod.title == "ChainedDeconstruction")
				{
					ChainedDeconstruction = true;
					Debug.Log((object)"[MOD][Stairs] ChainedDeconstruction Enable");
					break;
				}
			}
		}
	}

	[HarmonyPatch(typeof(GridSettings))]
	[HarmonyPatch("ClearGrid")]
	public static class GridSettings_ClearGrid_Patch
	{
		public static void Postfix()
		{
			MyGrid.Masks = null;
		}
	}

	[HarmonyPatch(typeof(GeneratedBuildings))]
	[HarmonyPatch("LoadGeneratedBuildings")]
	public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
	{
		public static void Postfix()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			ModUtil.AddBuildingToPlanScreen(HashedString.op_Implicit("Base"), "Stairs", "ladders", "FirePole", (BuildingOrdering)1);
			ModUtil.AddBuildingToPlanScreen(HashedString.op_Implicit("Base"), "Stairs_Classic", "ladders", "Stairs", (BuildingOrdering)1);
			ModUtil.AddBuildingToPlanScreen(HashedString.op_Implicit("Base"), "urfScaffolding_Alt1", "ladders", "Stairs_Classic", (BuildingOrdering)1);
			ModUtil.AddBuildingToPlanScreen(HashedString.op_Implicit("Base"), "urfScaffolding", "ladders", "urfScaffolding_Alt1", (BuildingOrdering)1);
			ModUtil.AddBuildingToPlanScreen(HashedString.op_Implicit("Base"), "urfScaffolding_Alt2", "ladders", "urfScaffolding", (BuildingOrdering)1);
			ModUtil.AddBuildingToPlanScreen(HashedString.op_Implicit("Base"), "Stairs_Alt1", "ladders", "urfScaffolding", (BuildingOrdering)1);
		}
	}

	[HarmonyPatch(typeof(Db))]
	[HarmonyPatch("Initialize")]
	public static class Db_Initialize_Patch
	{
		public static void Prefix()
		{
			if (Localization.GetLocale() != null)
			{
				LoadStrings(Path.Combine(sPath, "loc", Localization.GetLocale().Code + ".po"));
			}
		}

		public static void Postfix(ref Db __instance)
		{
			AddBuildingToTechnology(__instance, "Luxury", "Stairs_Alt1");
			AddBuildingToTechnology(__instance, "RefinedObjects", "urfScaffolding");
			AddBuildingToTechnology(__instance, "InteriorDecor", "urfScaffolding_Alt1");
			AddBuildingToTechnology(__instance, "InteriorDecor", "Stairs_Classic");
			AddBuildingToTechnology(__instance, "Smelting", "urfScaffolding_Alt2");
		}
	}

	public static class MinionConfig_Patch
	{
		public static void Postfix(GameObject go)
		{
			Navigator component = go.GetComponent<Navigator>();
			component.transitionDriver.overrideLayers.Add((OverrideLayer)(object)new MyTransitionLayer(component));
		}

		public static void ApplyPatchas(Harmony harmony)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			harmony.Patch((MethodBase)typeof(MinionConfig).GetMethod("OnSpawn"), new HarmonyMethod(typeof(MinionConfig_Patch).GetMethod("Postfix")), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			harmony.Patch((MethodBase)typeof(BionicMinionConfig).GetMethod("OnSpawn"), new HarmonyMethod(typeof(MinionConfig_Patch).GetMethod("Postfix")), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}
	}

	[HarmonyPatch(typeof(ScoutRoverConfig))]
	[HarmonyPatch("OnSpawn")]
	public static class ScoutRoverConfig_Patch
	{
		public static void Postfix(GameObject inst)
		{
			Navigator component = inst.GetComponent<Navigator>();
			component.transitionDriver.overrideLayers.Add((OverrideLayer)(object)new MyTransitionLayer(component));
		}
	}

	[HarmonyPatch(typeof(SafeCellSensor))]
	[HarmonyPatch("Update")]
	public class SafeCellSensor_Patch
	{
		public static bool Prefix(Navigator ___navigator)
		{
			if ((Object)(object)___navigator == (Object)null)
			{
				return true;
			}
			int num = Grid.PosToCell((KMonoBehaviour)(object)___navigator);
			if (!Grid.IsValidCell(num))
			{
				return true;
			}
			if (!MyGrid.IsHypotenuse(num))
			{
				return true;
			}
			MyTransitionLayer myTransitionLayer = (MyTransitionLayer)(object)___navigator.transitionDriver.overrideLayers.Find((OverrideLayer x) => ((object)x).GetType() == typeof(MyTransitionLayer));
			if (myTransitionLayer == null || !myTransitionLayer.isMovingOnStaris)
			{
				return true;
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(Manager))]
	[HarmonyPatch("NextTask")]
	public class PathProbeTask_Patch
	{
		public static bool Prefix(List<WorkOrder> ___workQueue, ref bool __result, Manager __instance)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			lock (__instance)
			{
				if (___workQueue.Count == 0)
				{
					return true;
				}
				Navigator navigator = ___workQueue[0].navigator;
				if ((Object)(object)navigator == (Object)null)
				{
					return true;
				}
				int num = Grid.PosToCell((KMonoBehaviour)(object)navigator);
				if (!Grid.IsValidCell(num))
				{
					return true;
				}
				if (!MyGrid.IsHypotenuse(num))
				{
					return true;
				}
				MyTransitionLayer myTransitionLayer = (MyTransitionLayer)(object)navigator.transitionDriver.overrideLayers.Find((OverrideLayer x) => ((object)x).GetType() == typeof(MyTransitionLayer));
				if (myTransitionLayer == null || !myTransitionLayer.isMovingOnStaris)
				{
					return true;
				}
				___workQueue.RemoveAt(0);
				__result = false;
				return false;
			}
		}
	}

	[HarmonyPatch(typeof(BuildingDef))]
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	public static class BuildingDef_IsAreaClear_Patch
	{
		private static bool IsScaffolding(GameObject go)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)go == (Object)null)
			{
				return false;
			}
			if (!KPrefabIDExtensions.HasTag(go, tag_Scaffolding))
			{
				return false;
			}
			return true;
		}

		private static bool IsSolid(int cell)
		{
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = ((ObjectLayerIndexer)(ref Grid.Objects))[cell, 1];
			if ((Object)(object)val == (Object)null)
			{
				return false;
			}
			Building component = val.GetComponent<Building>();
			if ((Object)(object)component == (Object)null)
			{
				component = (Building)(object)val.GetComponent<BuildingUnderConstruction>();
				if ((Object)(object)component == (Object)null)
				{
					return false;
				}
			}
			GameObject buildingComplete = component.Def.BuildingComplete;
			if ((Object)(object)buildingComplete.GetComponent<Door>() != (Object)null)
			{
				return true;
			}
			Def def = StateMachineControllerExtensions.GetDef<Def>(buildingComplete);
			if (def == null)
			{
				return false;
			}
			int num = Grid.PosToCell(val);
			for (int i = 0; i < def.solidOffsets.Length; i++)
			{
				CellOffset rotatedCellOffset = Rotatable.GetRotatedCellOffset(def.solidOffsets[i], component.Orientation);
				if (Grid.OffsetCell(num, rotatedCellOffset) == cell)
				{
					return true;
				}
			}
			return false;
		}

		private static bool CheckAll(int cell, CellOffset[] offsets, Orientation orientation)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < offsets.Length; i++)
			{
				CellOffset rotatedCellOffset = Rotatable.GetRotatedCellOffset(offsets[i], orientation);
				int num = Grid.OffsetCell(cell, rotatedCellOffset);
				if (IsScaffolding(((ObjectLayerIndexer)(ref Grid.Objects))[num, 38]))
				{
					return true;
				}
			}
			return false;
		}

		public static void Postfix(BuildingDef __instance, ref bool __result, GameObject source_go, int cell, Orientation orientation, ObjectLayer layer, ObjectLayer tile_layer, bool replace_tile, bool restrictToActiveWorld, ref string fail_reason, bool permitUproots)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Invalid comparison between Unknown and I4
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			if (!__result)
			{
				return;
			}
			if ((int)layer == 39 || (int)__instance.BuildLocationRule == 6 || (int)__instance.BuildLocationRule == 11)
			{
				if (CheckAll(cell, __instance.PlacementOffsets, orientation))
				{
					__result = false;
				}
			}
			else
			{
				if (!IsScaffolding(source_go) || !Grid.IsWorldValidCell(cell))
				{
					return;
				}
				if ((Object)(object)((ObjectLayerIndexer)(ref Grid.Objects))[cell, 39] != (Object)null)
				{
					__result = false;
				}
				else if ((Object)(object)((ObjectLayerIndexer)(ref Grid.Objects))[cell, 1] != (Object)null)
				{
					Building component = ((ObjectLayerIndexer)(ref Grid.Objects))[cell, 1].GetComponent<Building>();
					if ((Object)(object)component != (Object)null && (Object)(object)component.Def.BuildingComplete.GetComponent<Door>() != (Object)null)
					{
						__result = false;
					}
				}
			}
			if (!__result)
			{
				fail_reason = LocString.op_Implicit(TOOLTIPS.HELP_BUILDLOCATION_OCCUPIED);
			}
		}
	}

	[HarmonyPatch(typeof(FloorValidator))]
	[HarmonyPatch("IsWalkableCell")]
	public static class FloorValidator_IsWalkableCell_Patch
	{
		public static void Postfix(ref bool __result, int cell, int anchor_cell, bool is_dupe)
		{
			if (!__result && is_dupe && Grid.IsWorldValidCell(cell))
			{
				if (MyGrid.IsScaffolding(cell))
				{
					__result = true;
				}
				else if (Grid.IsWorldValidCell(anchor_cell) && MyGrid.IsWalkable(anchor_cell))
				{
					__result = true;
				}
			}
		}
	}

	[HarmonyPatch(typeof(CreaturePathFinderAbilities))]
	[HarmonyPatch("TraversePath")]
	public static class CreatureTraversePath_Patch
	{
		public static bool Prefix(Navigator ___navigator, ref bool __result, ref PotentialPath path, int from_cell, NavType from_nav_type, int cost, int transition_id, bool submerged)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (___navigator.NavGridName != "RobotNavGrid")
			{
				return true;
			}
			__result = false;
			return PathFilter(ref path, from_cell, from_nav_type, ___navigator);
		}
	}

	[HarmonyPatch(typeof(MinionPathFinderAbilities))]
	[HarmonyPatch("TraversePath")]
	public static class TraversePath_Patch
	{
		public static bool Prefix(Navigator ___navigator, ref bool __result, ref PotentialPath path, int from_cell, NavType from_nav_type, int cost, int transition_id, bool submerged)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			__result = false;
			return PathFilter(ref path, from_cell, from_nav_type, ___navigator);
		}
	}

	[HarmonyPatch(typeof(Comet))]
	[HarmonyPatch("DamageThings")]
	public static class Comet_Patch
	{
		public static void Prefix(Vector3 pos, int cell, int damage)
		{
			if (Grid.IsValidCell(cell))
			{
				DoDamage2Scaffolding(cell, Mathf.RoundToInt((float)damage), LocString.op_Implicit(DAMAGESOURCES.COMET), LocString.op_Implicit(DAMAGE_POPS.COMET));
			}
		}
	}

	[HarmonyPatch(typeof(States))]
	[HarmonyPatch("DoWorldDamage")]
	public static class LaunchableRocket_Patch
	{
		public static void Postfix(GameObject part, Vector3 apparentPosition)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			CellOffset[] occupiedCellsOffsets = part.GetComponent<OccupyArea>().OccupiedCellsOffsets;
			foreach (CellOffset val in occupiedCellsOffsets)
			{
				int num = Grid.OffsetCell(Grid.PosToCell(apparentPosition), val);
				if (Grid.IsValidCell(num))
				{
					DoDamage2Scaffolding(num, -1, LocString.op_Implicit(DAMAGESOURCES.ROCKET), LocString.op_Implicit(DAMAGE_POPS.ROCKET));
				}
			}
		}
	}

	[HarmonyPatch(typeof(States))]
	[HarmonyPatch("DoWorldDamage")]
	public class LaunchableRocketCluster_Patch
	{
		public static void Postfix(GameObject part, Vector3 apparentPosition, int actualWorld)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			CellOffset[] occupiedCellsOffsets = part.GetComponent<OccupyArea>().OccupiedCellsOffsets;
			foreach (CellOffset val in occupiedCellsOffsets)
			{
				int num = Grid.OffsetCell(Grid.PosToCell(apparentPosition), val);
				if (Grid.IsValidCell(num) && Grid.WorldIdx[num] == Grid.WorldIdx[actualWorld])
				{
					DoDamage2Scaffolding(num, -1, LocString.op_Implicit(DAMAGESOURCES.ROCKET), LocString.op_Implicit(DAMAGE_POPS.ROCKET));
				}
			}
		}
	}

	[HarmonyPatch(typeof(DeconstructTool))]
	[HarmonyPatch("DeconstructCell")]
	public static class DeconstructTool_Patch
	{
		public static void Postfix(DeconstructTool __instance, int cell)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			if (!((FilteredDragTool)__instance).IsActiveLayer(FILTERLAYERS.BACKWALL))
			{
				return;
			}
			GameObject val = ((ObjectLayerIndexer)(ref Grid.Objects))[cell, 38];
			if ((Object)(object)val != (Object)null && (Object)(object)val.GetComponent<Scaffolding>() != (Object)null)
			{
				EventExtensions.Trigger(val, -790448070, (object)null);
				Prioritizable component = val.GetComponent<Prioritizable>();
				if ((Object)(object)component != (Object)null)
				{
					component.SetMasterPriority(ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority());
				}
			}
		}
	}

	public static readonly Tag tag_Stairs = TagManager.Create("Stairs");

	public static readonly Tag tag_Scaffolding = TagManager.Create("Scaffolding");

	public static readonly Tag tag_ScaffoldingAlt2 = TagManager.Create("urfScaffolding_Alt2");

	public static bool ChainedDeconstruction = false;

	public static string sPath;

	public static void LoadStrings(string file, bool isTemplate = false)
	{
		if (!File.Exists(file))
		{
			return;
		}
		foreach (KeyValuePair<string, string> item in Localization.LoadStringsFile(file, isTemplate))
		{
			Strings.Add(new string[2] { item.Key, item.Value });
		}
		Debug.Log((object)("[MOD][Stairs] Locfile loaded : " + file));
	}

	public override void OnLoad(Harmony harmony)
	{
		((UserMod2)this).OnLoad(harmony);
		sPath = ((UserMod2)this).path;
		LoadStrings(Path.Combine(((UserMod2)this).path, "loc/stairs_template.pot"), isTemplate: true);
		MinionConfig_Patch.ApplyPatchas(harmony);
	}

	private static void AddBuildingToTechnology(Db db, string tech, string buildingId)
	{
		((ResourceSet<Tech>)(object)db.Techs).TryGet(tech)?.unlockedItemIDs.Add(buildingId);
	}

	public static bool PathFilter(ref PotentialPath path, int from_cell, NavType from_nav_type, Navigator navigator)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		if ((int)path.navType != 0 || (int)from_nav_type != 0)
		{
			return true;
		}
		int cell = path.cell;
		CellOffset offset = Grid.GetOffset(from_cell, cell);
		if (offset.y == 0)
		{
			return true;
		}
		int cell2 = Grid.CellBelow(cell);
		int cell3 = Grid.CellBelow(from_cell);
		if (offset.y == 1)
		{
			if (MyGrid.IsHypotenuse(cell2))
			{
				if (offset.x == 1)
				{
					if (!MyGrid.IsRightSet(cell2))
					{
						return true;
					}
				}
				else if (offset.x == -1 && MyGrid.IsRightSet(cell2))
				{
					return true;
				}
			}
		}
		else if (offset.y == -1 && MyGrid.IsHypotenuse(cell3))
		{
			if (offset.x == 1)
			{
				if (MyGrid.IsRightSet(cell3))
				{
					return true;
				}
			}
			else if (offset.x == -1 && !MyGrid.IsRightSet(cell3))
			{
				return true;
			}
		}
		if (offset.y > 0)
		{
			int num = Grid.OffsetCell(from_cell, 0, 1);
			if (MyGrid.IsScaffolding(num))
			{
				return false;
			}
			if (offset.x != 0)
			{
				if (MyGrid.IsWalkable(from_cell))
				{
					return false;
				}
				if (offset.x > 0)
				{
					if (MyGrid.IsHypotenuse(num) && MyGrid.IsRightSet(num))
					{
						return false;
					}
					if (offset.x > 1)
					{
						int cell4 = Grid.CellRight(num);
						if (MyGrid.IsHypotenuse(cell4) && MyGrid.IsRightSet(cell4))
						{
							return false;
						}
					}
				}
				else if (offset.x < 0)
				{
					if (MyGrid.IsHypotenuse(num) && !MyGrid.IsRightSet(num))
					{
						return false;
					}
					if (offset.x < -1)
					{
						int cell5 = Grid.CellLeft(num);
						if (MyGrid.IsHypotenuse(cell5) && !MyGrid.IsRightSet(cell5))
						{
							return false;
						}
					}
				}
			}
			else if (MyGrid.IsHypotenuse(from_cell))
			{
				return false;
			}
			if (offset.y > 1)
			{
				if (MyGrid.IsWalkable(num))
				{
					return false;
				}
				num = Grid.OffsetCell(from_cell, 0, 2);
				if (MyGrid.IsScaffolding(num))
				{
					return false;
				}
				if (MyGrid.IsHypotenuse(cell2) && MyGrid.IsHypotenuse(num) && MyGrid.IsRightSet(cell2) == MyGrid.IsRightSet(num))
				{
					return false;
				}
			}
		}
		else if (offset.x > 0)
		{
			if (MyGrid.IsWalkable(cell))
			{
				return false;
			}
			int num2 = Grid.CellRight(from_cell);
			if (MyGrid.IsScaffolding(num2))
			{
				return false;
			}
			if (MyGrid.IsHypotenuse(cell3) && MyGrid.IsHypotenuse(num2) && !MyGrid.IsRightSet(num2))
			{
				return false;
			}
			if (offset.x > 1)
			{
				num2 = Grid.CellRight(num2);
				if (MyGrid.IsScaffolding(num2))
				{
					return false;
				}
				if (MyGrid.IsWalkable(num2) && !MyGrid.IsRightSet(num2))
				{
					return false;
				}
			}
			else if (offset.y < -1)
			{
				num2 = Grid.CellDownRight(from_cell);
				if (MyGrid.IsScaffolding(num2))
				{
					return false;
				}
				if (MyGrid.IsWalkable(num2))
				{
					return false;
				}
				num2 = Grid.CellBelow(num2);
				if (MyGrid.IsWalkable(num2))
				{
					return false;
				}
			}
		}
		else if (offset.x < 0)
		{
			if (MyGrid.IsWalkable(cell))
			{
				return false;
			}
			int num3 = Grid.CellLeft(from_cell);
			if (MyGrid.IsScaffolding(num3))
			{
				return false;
			}
			if (MyGrid.IsHypotenuse(cell3) && MyGrid.IsHypotenuse(num3) && MyGrid.IsRightSet(num3))
			{
				return false;
			}
			if (offset.x < -1)
			{
				num3 = Grid.CellLeft(num3);
				if (MyGrid.IsScaffolding(num3))
				{
					return false;
				}
				if (MyGrid.IsWalkable(num3) && MyGrid.IsRightSet(num3))
				{
					return false;
				}
			}
			else if (offset.y < -1)
			{
				num3 = Grid.CellDownLeft(from_cell);
				if (MyGrid.IsScaffolding(num3))
				{
					return false;
				}
				if (MyGrid.IsWalkable(num3))
				{
					return false;
				}
				num3 = Grid.CellBelow(num3);
				if (MyGrid.IsWalkable(num3))
				{
					return false;
				}
			}
		}
		else
		{
			if (MyGrid.IsScaffolding(from_cell))
			{
				return false;
			}
			int cell6 = Grid.CellBelow(from_cell);
			if (MyGrid.IsHypotenuse(cell6))
			{
				return false;
			}
			if (offset.y < -1)
			{
				if (MyGrid.IsScaffolding(cell6))
				{
					return false;
				}
				cell6 = Grid.OffsetCell(from_cell, 0, -2);
				if (MyGrid.IsWalkable(cell6))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static void DoDamage2Scaffolding(int cell, int damage, string source, string popString)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = ((ObjectLayerIndexer)(ref Grid.Objects))[cell, 38];
		if (!((Object)(object)val != (Object)null) || !KPrefabIDExtensions.HasTag(val, tag_Scaffolding))
		{
			return;
		}
		BuildingHP component = val.GetComponent<BuildingHP>();
		if ((Object)(object)component != (Object)null)
		{
			if (damage < 0)
			{
				damage = component.MaxHitPoints;
			}
			EventExtensions.BoxingTrigger<DamageSourceInfo>(((Component)component).gameObject, -794517298, new DamageSourceInfo
			{
				damage = damage,
				source = source,
				popString = popString
			});
		}
	}
}
internal class Scaffolding : KMonoBehaviour, IGameObjectEffectDescriptor
{
	private static readonly IntraObjectHandler<Scaffolding> OnBuildingBrokenDelegate = new IntraObjectHandler<Scaffolding>((Action<Scaffolding, object>)delegate(Scaffolding component, object data)
	{
		component.OnBuildingBroken(data);
	});

	private static readonly IntraObjectHandler<Scaffolding> OnBuildingFullyRepairedDelegate = new IntraObjectHandler<Scaffolding>((Action<Scaffolding, object>)delegate(Scaffolding component, object data)
	{
		component.OnBuildingFullyRepaired(data);
	});

	private static readonly IntraObjectHandler<Scaffolding> OnRefreshUserMenuDelegate = new IntraObjectHandler<Scaffolding>((Action<Scaffolding, object>)delegate(Scaffolding component, object data)
	{
		component.OnRefreshUserMenu(data);
	});

	[Serialize]
	private bool buildingEnabled = true;

	public bool IsEnabled
	{
		get
		{
			return buildingEnabled;
		}
		set
		{
			buildingEnabled = value;
			Game.Instance.userMenu.Refresh(((Component)this).gameObject);
			((KMonoBehaviour)this).BoxingTrigger(1088293757, buildingEnabled);
			UpdateState();
		}
	}

	protected override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		((KMonoBehaviour)this).Subscribe<Scaffolding>(493375141, OnRefreshUserMenuDelegate);
	}

	private void UpdateState()
	{
		int num = Grid.PosToCell((KMonoBehaviour)(object)this);
		((Component)this).GetComponent<KSelectable>().ToggleStatusItem(Db.Get().BuildingStatusItems.BuildingDisabled, !IsEnabled, (object)null);
		if (IsEnabled && !((Component)this).GetComponent<BuildingHP>().IsBroken)
		{
			MyGrid.Masks[num] |= MyGrid.Flags.HasScaffolding;
		}
		else
		{
			MyGrid.Masks[num] &= (MyGrid.Flags)239;
		}
		Pathfinding.Instance.AddDirtyNavGridCell(num);
	}

	private void OnRefreshUserMenu(object data)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		ButtonInfo val = ((!IsEnabled) ? new ButtonInfo("action_building_disabled", LocString.op_Implicit(ENABLEBUILDING.NAME_OFF), (Action)OnMenuToggle, (Action)167, (Action<GameObject>)null, (Action<ButtonInfo>)null, (Texture)null, LocString.op_Implicit(ENABLEBUILDING.TOOLTIP_OFF), true) : new ButtonInfo("action_building_disabled", LocString.op_Implicit(ENABLEBUILDING.NAME), (Action)OnMenuToggle, (Action)167, (Action<GameObject>)null, (Action<ButtonInfo>)null, (Texture)null, LocString.op_Implicit(ENABLEBUILDING.TOOLTIP), true));
		Game.Instance.userMenu.AddButton(((Component)this).gameObject, val, 1f);
	}

	private void OnMenuToggle()
	{
		IsEnabled = !IsEnabled;
	}

	protected override void OnSpawn()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		((KMonoBehaviour)this).OnSpawn();
		((Component)this).GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.Normal, (object)null);
		IsEnabled = buildingEnabled;
		((KMonoBehaviour)this).Subscribe<Scaffolding>(774203113, OnBuildingBrokenDelegate);
		((KMonoBehaviour)this).Subscribe<Scaffolding>(-1735440190, OnBuildingFullyRepairedDelegate);
		if ((int)((Component)this).GetComponent<PrimaryElement>().ElementID == -899253461 && KPrefabIDExtensions.PrefabID((Component)(object)this) == Patches.tag_ScaffoldingAlt2)
		{
			((Component)this).GetComponent<BuildingHP>().invincible = true;
		}
	}

	protected override void OnCleanUp()
	{
		((KMonoBehaviour)this).Unsubscribe<Scaffolding>(774203113, OnBuildingBrokenDelegate, false);
		((KMonoBehaviour)this).Unsubscribe<Scaffolding>(-1735440190, OnBuildingFullyRepairedDelegate, false);
		((KMonoBehaviour)this).OnCleanUp();
		int num = Grid.PosToCell((KMonoBehaviour)(object)this);
		MyGrid.Masks[num] &= (MyGrid.Flags)239;
		Pathfinding.Instance.AddDirtyNavGridCell(num);
		if (Patches.ChainedDeconstruction)
		{
			Deconstructable component = ((Component)this).GetComponent<Deconstructable>();
			if ((Object)(object)component != (Object)null && component.IsMarkedForDeconstruction())
			{
				MyGrid.ForceDeconstruction(Grid.CellLeft(num), isStairs: false);
				MyGrid.ForceDeconstruction(Grid.CellRight(num), isStairs: false);
			}
		}
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		List<Descriptor> list = null;
		if (KPrefabIDExtensions.PrefabID(go) == Patches.tag_ScaffoldingAlt2)
		{
			list = new List<Descriptor>();
			Descriptor item = default(Descriptor);
			((Descriptor)(ref item)).SetupDescriptor(string.Format(LocString.op_Implicit(BUILDINGEFFECTS.DUPLICANTMOVEMENTBOOST), GameUtil.AddPositiveSign(GameUtil.GetFormattedPercent(50f, (TimeSlice)0), true)), string.Format(LocString.op_Implicit(TOOLTIPS.DUPLICANTMOVEMENTBOOST), GameUtil.GetFormattedPercent(50f, (TimeSlice)0)), (DescriptorType)1);
			list.Add(item);
		}
		return list;
	}

	private void OnBuildingBroken(object data)
	{
		UpdateState();
	}

	private void OnBuildingFullyRepaired(object data)
	{
		UpdateState();
	}
}
[SkipSaveFileSerialization]
public class AnimStairs : KMonoBehaviour
{
	private Handle<int> partitionerEntry;

	private Extents extents;

	public ObjectLayer objectLayer = (ObjectLayer)1;

	private static readonly KAnimHashedString[] leftSymbols = (KAnimHashedString[])(object)new KAnimHashedString[2]
	{
		new KAnimHashedString("cap_left"),
		new KAnimHashedString("cap_left_place")
	};

	private static readonly KAnimHashedString[] rightSymbols = (KAnimHashedString[])(object)new KAnimHashedString[2]
	{
		new KAnimHashedString("cap_right"),
		new KAnimHashedString("cap_right_place")
	};

	private static readonly KAnimHashedString[] rightHalfSymbols = (KAnimHashedString[])(object)new KAnimHashedString[2]
	{
		new KAnimHashedString("cap_right_half"),
		new KAnimHashedString("cap_right_half_place")
	};

	private static readonly KAnimHashedString[] topSymbols = (KAnimHashedString[])(object)new KAnimHashedString[2]
	{
		new KAnimHashedString("cap_top"),
		new KAnimHashedString("cap_top_place")
	};

	private static readonly KAnimHashedString[] topHalfSymbols = (KAnimHashedString[])(object)new KAnimHashedString[2]
	{
		new KAnimHashedString("cap_top_half"),
		new KAnimHashedString("cap_top_half_place")
	};

	private static readonly KAnimHashedString[] jointSymbols = (KAnimHashedString[])(object)new KAnimHashedString[2]
	{
		new KAnimHashedString("joint"),
		new KAnimHashedString("joint_place")
	};

	protected override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		OccupyArea component = ((Component)this).GetComponent<OccupyArea>();
		if ((Object)(object)component != (Object)null)
		{
			extents = component.GetExtents();
		}
		else
		{
			Building component2 = ((Component)this).GetComponent<Building>();
			extents = component2.GetExtents();
		}
		Extents val = default(Extents);
		((Extents)(ref val))..ctor(extents.x - 1, extents.y - 1, extents.width + 2, extents.height + 2);
		partitionerEntry = GameScenePartitioner.Instance.Add("AnimStairs.OnSpawn", (object)((Component)this).gameObject, val, GameScenePartitioner.Instance.objectLayers[objectLayer], (Action<object>)OnNeighbourCellsUpdated);
		UpdateEndCaps();
	}

	protected override void OnCleanUp()
	{
		GameScenePartitioner.Instance.Free(ref partitionerEntry);
		((KMonoBehaviour)this).OnCleanUp();
	}

	public static void PrearePreview(GameObject go)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		KBatchedAnimController[] componentsInChildren = go.GetComponentsInChildren<KBatchedAnimController>();
		foreach (KBatchedAnimController val in componentsInChildren)
		{
			KAnimHashedString[] array = leftSymbols;
			foreach (KAnimHashedString val2 in array)
			{
				((KAnimControllerBase)val).SetSymbolVisiblity(val2, true);
			}
			array = rightSymbols;
			foreach (KAnimHashedString val3 in array)
			{
				((KAnimControllerBase)val).SetSymbolVisiblity(val3, false);
			}
			array = rightHalfSymbols;
			foreach (KAnimHashedString val4 in array)
			{
				((KAnimControllerBase)val).SetSymbolVisiblity(val4, false);
			}
			array = jointSymbols;
			foreach (KAnimHashedString val5 in array)
			{
				((KAnimControllerBase)val).SetSymbolVisiblity(val5, false);
			}
			array = topSymbols;
			foreach (KAnimHashedString val6 in array)
			{
				((KAnimControllerBase)val).SetSymbolVisiblity(val6, false);
			}
			array = topHalfSymbols;
			foreach (KAnimHashedString val7 in array)
			{
				((KAnimControllerBase)val).SetSymbolVisiblity(val7, true);
			}
		}
	}

	private void UpdateEndCaps()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected I4, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Invalid comparison between Unknown and I4
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		int num = Grid.PosToCell(((Component)this).gameObject);
		bool flag = true;
		bool flag2 = false;
		bool flag3 = true;
		bool flag4 = false;
		bool flag5 = (int)((Component)this).GetComponent<Rotatable>().GetOrientation() == 5;
		int num2 = Grid.CellBelow(num);
		int num3 = Grid.CellAbove(num);
		if (Grid.IsValidCell(num3))
		{
			flag3 = HasTileableNeighbour(num3);
		}
		if (!flag3)
		{
			int num4 = (flag5 ? Grid.CellRight(num) : Grid.CellLeft(num));
			if (Grid.IsValidCell(num4))
			{
				flag = !HasTileableNeighbour(num4);
			}
		}
		else
		{
			flag = false;
		}
		if (flag && !flag3)
		{
			int num5 = (flag5 ? Grid.CellLeft(num3) : Grid.CellRight(num3));
			if (Grid.IsValidCell(num5))
			{
				GameObject val = ((ObjectLayerIndexer)(ref Grid.Objects))[num5, (int)objectLayer];
				if ((Object)(object)val != (Object)null && KPrefabIDExtensions.HasTag(val, Patches.tag_Stairs) && (int)val.GetComponent<Rotatable>().GetOrientation() == 5 == flag5)
				{
					flag4 = true;
				}
			}
		}
		if (Grid.IsValidCell(num2))
		{
			flag2 = flag3 || !flag || HasTileableNeighbour(num2, check_tile: false);
			if (!flag2 && flag && IsSolid(num2))
			{
				int num6 = (flag5 ? Grid.CellLeft(num) : Grid.CellRight(num));
				if (Grid.IsValidCell(num6))
				{
					flag2 = HasTileableNeighbour(num6, check_tile: false);
				}
			}
		}
		if (MyGrid.IsStair(num))
		{
			if (flag)
			{
				MyGrid.Masks[num] |= MyGrid.Flags.Hypotenuse;
			}
			else
			{
				MyGrid.Masks[num] &= (MyGrid.Flags)247;
			}
			if (flag || !flag3)
			{
				MyGrid.Masks[num] |= MyGrid.Flags.Walkable;
			}
			else
			{
				MyGrid.Masks[num] &= (MyGrid.Flags)251;
			}
		}
		KBatchedAnimController[] componentsInChildren = ((Component)this).GetComponentsInChildren<KBatchedAnimController>();
		foreach (KBatchedAnimController val2 in componentsInChildren)
		{
			KAnimHashedString[] array = leftSymbols;
			foreach (KAnimHashedString val3 in array)
			{
				((KAnimControllerBase)val2).SetSymbolVisiblity(val3, flag);
			}
			bool flag6 = flag2 && !flag;
			array = rightSymbols;
			foreach (KAnimHashedString val4 in array)
			{
				((KAnimControllerBase)val2).SetSymbolVisiblity(val4, flag6);
			}
			array = jointSymbols;
			foreach (KAnimHashedString val5 in array)
			{
				((KAnimControllerBase)val2).SetSymbolVisiblity(val5, flag6);
			}
			flag6 = flag2 && flag;
			array = rightHalfSymbols;
			foreach (KAnimHashedString val6 in array)
			{
				((KAnimControllerBase)val2).SetSymbolVisiblity(val6, flag6);
			}
			flag6 = !flag3 && !flag4 && !flag;
			array = topSymbols;
			foreach (KAnimHashedString val7 in array)
			{
				((KAnimControllerBase)val2).SetSymbolVisiblity(val7, flag6);
			}
			flag6 = flag && !flag3 && !flag4;
			array = topHalfSymbols;
			foreach (KAnimHashedString val8 in array)
			{
				((KAnimControllerBase)val2).SetSymbolVisiblity(val8, flag6);
			}
		}
	}

	private bool IsSolid(int cell)
	{
		GameObject val = ((ObjectLayerIndexer)(ref Grid.Objects))[cell, 1];
		if ((Object)(object)val != (Object)null && (Object)(object)val.GetComponent<SimCellOccupier>() != (Object)null)
		{
			return true;
		}
		return false;
	}

	private bool HasTileableNeighbour(int neighbour_cell, bool check_tile = true)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected I4, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = ((ObjectLayerIndexer)(ref Grid.Objects))[neighbour_cell, (int)objectLayer];
		if ((Object)(object)val != (Object)null)
		{
			if (check_tile && (Object)(object)val.GetComponent<SimCellOccupier>() != (Object)null)
			{
				return true;
			}
			if (KPrefabIDExtensions.HasTag(val, Patches.tag_Stairs))
			{
				return true;
			}
		}
		return false;
	}

	private void OnNeighbourCellsUpdated(object data)
	{
		if (!((Object)(object)this == (Object)null) && !((Object)(object)((Component)this).gameObject == (Object)null) && partitionerEntry.IsValid())
		{
			UpdateEndCaps();
		}
	}
}
public class Stair : KMonoBehaviour, IGameObjectEffectDescriptor
{
	private static readonly IntraObjectHandler<Stair> OnRefreshUserMenuDelegate = new IntraObjectHandler<Stair>((Action<Stair, object>)delegate(Stair component, object data)
	{
		component.OnRefreshUserMenu(data);
	});

	protected override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		((KMonoBehaviour)this).OnSpawn();
		((Component)this).GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.Normal, (object)null);
		((KMonoBehaviour)this).Subscribe<Stair>(493375141, OnRefreshUserMenuDelegate);
		((KMonoBehaviour)this).Subscribe<Stair>(-111137758, OnRefreshUserMenuDelegate);
		Rotatable component = ((Component)this).GetComponent<Rotatable>();
		int num = Grid.PosToCell((KMonoBehaviour)(object)this);
		MyGrid.Masks[num] |= MyGrid.Flags.HasStair;
		if ((int)component.GetOrientation() == 5)
		{
			MyGrid.Masks[num] |= MyGrid.Flags.RightSet;
		}
		Pathfinding.Instance.AddDirtyNavGridCell(Grid.CellAbove(num));
	}

	protected override void OnCleanUp()
	{
		((KMonoBehaviour)this).OnCleanUp();
		int num = Grid.PosToCell((KMonoBehaviour)(object)this);
		MyGrid.Masks[num] = (MyGrid.IsScaffolding(num) ? MyGrid.Flags.HasScaffolding : ((MyGrid.Flags)0));
		Pathfinding.Instance.AddDirtyNavGridCell(Grid.CellAbove(num));
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		List<Descriptor> list = new List<Descriptor>();
		Descriptor item = default(Descriptor);
		((Descriptor)(ref item)).SetupDescriptor(string.Format(LocString.op_Implicit(BUILDINGEFFECTS.DUPLICANTMOVEMENTBOOST), GameUtil.GetFormattedPercent(-10f, (TimeSlice)0)), string.Format(LocString.op_Implicit(TOOLTIPS.DUPLICANTMOVEMENTBOOST), GameUtil.GetFormattedPercent(-10f, (TimeSlice)0)), (DescriptorType)1);
		list.Add(item);
		return list;
	}

	private void OnMenuToggle()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		Rotatable component = ((Component)this).GetComponent<Rotatable>();
		int num = Grid.PosToCell((KMonoBehaviour)(object)this);
		if ((int)component.GetOrientation() == 5)
		{
			component.SetOrientation((Orientation)0);
			MyGrid.Masks[num] &= (MyGrid.Flags)253;
		}
		else
		{
			component.SetOrientation((Orientation)5);
			MyGrid.Masks[num] |= MyGrid.Flags.RightSet;
		}
		GameScenePartitioner.Instance.TriggerEvent(num, GameScenePartitioner.Instance.objectLayers[1], (object)null);
	}

	private void OnRefreshUserMenu(object data)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		ButtonInfo val = (((int)((Component)this).GetComponent<Rotatable>().GetOrientation() == 5) ? new ButtonInfo("action_direction_left", StringEntry.op_Implicit(Strings.Get("STRINGS.UI.USERMENUACTIONS.STAIRSBUTTON.NAME")), (Action)OnMenuToggle, (Action)280, (Action<GameObject>)null, (Action<ButtonInfo>)null, (Texture)null, StringEntry.op_Implicit(Strings.Get("STRINGS.UI.USERMENUACTIONS.STAIRSBUTTON.TOOLTIP")), true) : new ButtonInfo("action_direction_right", StringEntry.op_Implicit(Strings.Get("STRINGS.UI.USERMENUACTIONS.STAIRSBUTTON.NAME")), (Action)OnMenuToggle, (Action)280, (Action<GameObject>)null, (Action<ButtonInfo>)null, (Texture)null, StringEntry.op_Implicit(Strings.Get("STRINGS.UI.USERMENUACTIONS.STAIRSBUTTON.TOOLTIP")), true));
		Game.Instance.userMenu.AddButton(((Component)this).gameObject, val, 1f);
	}
}
