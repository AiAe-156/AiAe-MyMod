using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class UnderwaterCritterCondoConfig : IBuildingConfig
{
	public const string ID = "UnderwaterCritterCondo";

	public static readonly Operational.Flag Submerged = new Operational.Flag("Submerged", Operational.Flag.Type.Requirement);

	private static string[] AllFGSymbols = new string[3] { "doorway_fg", "condo_fg", "doorway_squid_fg" };

	private static Dictionary<CritterCondo.CreatureFGLayerType, string> AnimFGLayersToSymbolName = new Dictionary<CritterCondo.CreatureFGLayerType, string>
	{
		[CritterCondo.CreatureFGLayerType.SmallCreatureLayer] = AllFGSymbols[0],
		[CritterCondo.CreatureFGLayerType.LargeCreatureLayer] = AllFGSymbols[1],
		[CritterCondo.CreatureFGLayerType.SquidLayer] = AllFGSymbols[2]
	};

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("UnderwaterCritterCondo", 3, 3, "underwater_critter_condo_kanim", 100, 120f, new float[1] { 200f }, MATERIALS.PLASTICS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER3);
		buildingDef.AudioCategory = "Metal";
		buildingDef.PermittedRotations = PermittedRotations.FlipH;
		buildingDef.Floodable = false;
		buildingDef.AddSearchTerms(SEARCH_TERMS.CRITTER);
		buildingDef.AddSearchTerms(SEARCH_TERMS.RANCHING);
		buildingDef.AddSearchTerms(SEARCH_TERMS.WATER);
		return buildingDef;
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
	}

	private static StatusItem GetSubmergableStatusItem()
	{
		return Db.Get().BuildingStatusItems.NotSubmerged;
	}

	private static void DisableAllFGSymbols(KBatchedAnimController animController)
	{
		if (!(animController == null))
		{
			for (int i = 0; i < AllFGSymbols.Length; i++)
			{
				string text = AllFGSymbols[i];
				animController.SetSymbolVisiblity(text, is_visible: false);
			}
		}
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<BuildingSubmergable>();
		Effect effect = new Effect("InteractedWithUnderwaterCondo", STRINGS.CREATURES.MODIFIERS.CRITTERCONDOINTERACTEFFECT.NAME, STRINGS.CREATURES.MODIFIERS.UNDERWATERCRITTERCONDOINTERACTEFFECT.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, 1f, STRINGS.CREATURES.MODIFIERS.CRITTERCONDOINTERACTEFFECT.NAME));
		Db.Get().effects.Add(effect);
		CritterCondo.Def def = go.AddOrGetDef<CritterCondo.Def>();
		def.IsCritterCondoOperationalCb = delegate(CritterCondo.Instance condo_smi)
		{
			Building component = condo_smi.GetComponent<Building>();
			for (int i = 0; i < component.PlacementCells.Length; i++)
			{
				int cell = component.PlacementCells[i];
				if (!Grid.IsLiquid(cell))
				{
					return false;
				}
			}
			return true;
		};
		def.UpdateForegroundVisibilitySymbols = delegate(KBatchedAnimController foreground_controller, CritterCondo.CreatureFGLayerType layer)
		{
			if (foreground_controller != null)
			{
				DisableAllFGSymbols(foreground_controller);
				foreground_controller.SetSymbolVisiblity(AnimFGLayersToSymbolName[layer], is_visible: true);
			}
		};
		def.moveToStatusItem = new StatusItem("UNDERWATERCRITTERCONDO.MOVINGTO", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		def.interactStatusItem = new StatusItem("UNDERWATERCRITTERCONDO.INTERACTING", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		def.condoTag = "UnderwaterCritterCondo";
		def.effectId = effect.Id;
	}

	public override void ConfigurePost(BuildingDef def)
	{
	}
}
