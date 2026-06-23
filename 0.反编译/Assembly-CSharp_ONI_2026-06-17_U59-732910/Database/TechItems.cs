using System;
using STRINGS;
using UnityEngine;

namespace Database;

public class TechItems : ResourceSet<TechItem>
{
	public const string AUTOMATION_OVERLAY_ID = "AutomationOverlay";

	public TechItem automationOverlay;

	public const string SUITS_OVERLAY_ID = "SuitsOverlay";

	public TechItem suitsOverlay;

	public const string JET_SUIT_ID = "JetSuit";

	public TechItem jetSuit;

	public const string ATMO_SUIT_ID = "AtmoSuit";

	public TechItem atmoSuit;

	public const string OXYGEN_MASK_ID = "OxygenMask";

	public TechItem oxygenMask;

	public const string LEAD_SUIT_ID = "LeadSuit";

	public TechItem leadSuit;

	public TechItem disposableElectrobankMetalOre;

	public TechItem lubricationStick;

	public TechItem gasket;

	public TechItem rubberBoots;

	public TechItem drySuit;

	public TechItem disposableElectrobankUraniumOre;

	public TechItem electrobank;

	public TechItem fetchDrone;

	public TechItem selfChargingElectrobank;

	public TechItem superLiquids;

	public const string BETA_RESEARCH_POINT_ID = "BetaResearchPoint";

	public TechItem betaResearchPoint;

	public const string GAMMA_RESEARCH_POINT_ID = "GammaResearchPoint";

	public TechItem gammaResearchPoint;

	public const string DELTA_RESEARCH_POINT_ID = "DeltaResearchPoint";

	public TechItem deltaResearchPoint;

	public const string ORBITAL_RESEARCH_POINT_ID = "OrbitalResearchPoint";

	public TechItem orbitalResearchPoint;

	public const string CONVEYOR_OVERLAY_ID = "ConveyorOverlay";

	public TechItem conveyorOverlay;

	public TechItems(ResourceSet parent)
		: base("TechItems", parent)
	{
	}

	public void Init()
	{
		automationOverlay = AddTechItem("AutomationOverlay", RESEARCH.OTHER_TECH_ITEMS.AUTOMATION_OVERLAY.NAME, RESEARCH.OTHER_TECH_ITEMS.AUTOMATION_OVERLAY.DESC, GetSpriteFnBuilder("overlay_logic"));
		suitsOverlay = AddTechItem("SuitsOverlay", RESEARCH.OTHER_TECH_ITEMS.SUITS_OVERLAY.NAME, RESEARCH.OTHER_TECH_ITEMS.SUITS_OVERLAY.DESC, GetSpriteFnBuilder("overlay_suit"));
		betaResearchPoint = AddTechItem("BetaResearchPoint", RESEARCH.OTHER_TECH_ITEMS.BETA_RESEARCH_POINT.NAME, RESEARCH.OTHER_TECH_ITEMS.BETA_RESEARCH_POINT.DESC, GetSpriteFnBuilder("research_type_beta_icon"));
		gammaResearchPoint = AddTechItem("GammaResearchPoint", RESEARCH.OTHER_TECH_ITEMS.GAMMA_RESEARCH_POINT.NAME, RESEARCH.OTHER_TECH_ITEMS.GAMMA_RESEARCH_POINT.DESC, GetSpriteFnBuilder("research_type_gamma_icon"));
		orbitalResearchPoint = AddTechItem("OrbitalResearchPoint", RESEARCH.OTHER_TECH_ITEMS.ORBITAL_RESEARCH_POINT.NAME, RESEARCH.OTHER_TECH_ITEMS.ORBITAL_RESEARCH_POINT.DESC, GetSpriteFnBuilder("research_type_orbital_icon"));
		conveyorOverlay = AddTechItem("ConveyorOverlay", RESEARCH.OTHER_TECH_ITEMS.CONVEYOR_OVERLAY.NAME, RESEARCH.OTHER_TECH_ITEMS.CONVEYOR_OVERLAY.DESC, GetSpriteFnBuilder("overlay_conveyor"));
		jetSuit = AddTechItem("JetSuit", RESEARCH.OTHER_TECH_ITEMS.JET_SUIT.NAME, RESEARCH.OTHER_TECH_ITEMS.JET_SUIT.DESC, GetPrefabSpriteFnBuilder("Jet_Suit".ToTag()));
		if (jetSuit != null)
		{
			jetSuit.AddSearchTerms(SEARCH_TERMS.ATMOSUIT);
		}
		atmoSuit = AddTechItem("AtmoSuit", RESEARCH.OTHER_TECH_ITEMS.ATMO_SUIT.NAME, RESEARCH.OTHER_TECH_ITEMS.ATMO_SUIT.DESC, GetPrefabSpriteFnBuilder("Atmo_Suit".ToTag()));
		if (atmoSuit != null)
		{
			atmoSuit.AddSearchTerms(SEARCH_TERMS.ATMOSUIT);
		}
		oxygenMask = AddTechItem("OxygenMask", RESEARCH.OTHER_TECH_ITEMS.OXYGEN_MASK.NAME, RESEARCH.OTHER_TECH_ITEMS.OXYGEN_MASK.DESC, GetPrefabSpriteFnBuilder("Oxygen_Mask".ToTag()));
		if (oxygenMask != null)
		{
			oxygenMask.AddSearchTerms(SEARCH_TERMS.OXYGEN);
		}
		superLiquids = AddTechItem("SUPER_LIQUIDS", RESEARCH.OTHER_TECH_ITEMS.SUPER_LIQUIDS.NAME, RESEARCH.OTHER_TECH_ITEMS.SUPER_LIQUIDS.DESC, GetPrefabSpriteFnBuilder(SimHashes.ViscoGel.CreateTag()));
		deltaResearchPoint = AddTechItem("DeltaResearchPoint", RESEARCH.OTHER_TECH_ITEMS.DELTA_RESEARCH_POINT.NAME, RESEARCH.OTHER_TECH_ITEMS.DELTA_RESEARCH_POINT.DESC, GetSpriteFnBuilder("research_type_delta_icon"), DlcManager.EXPANSION1);
		leadSuit = AddTechItem("LeadSuit", RESEARCH.OTHER_TECH_ITEMS.LEAD_SUIT.NAME, RESEARCH.OTHER_TECH_ITEMS.LEAD_SUIT.DESC, GetPrefabSpriteFnBuilder("Lead_Suit".ToTag()), DlcManager.EXPANSION1);
		disposableElectrobankMetalOre = AddTechItem("DisposableElectrobank_RawMetal", RESEARCH.OTHER_TECH_ITEMS.DISPOSABLE_ELECTROBANK_METAL_ORE.NAME, RESEARCH.OTHER_TECH_ITEMS.DISPOSABLE_ELECTROBANK_METAL_ORE.DESC, GetPrefabSpriteFnBuilder("DisposableElectrobank_RawMetal".ToTag()), DlcManager.DLC3);
		if (disposableElectrobankMetalOre != null)
		{
			disposableElectrobankMetalOre.AddSearchTerms(SEARCH_TERMS.BATTERY);
		}
		lubricationStick = AddTechItem("LubricationStick", RESEARCH.OTHER_TECH_ITEMS.LUBRICATION_STICK.NAME, RESEARCH.OTHER_TECH_ITEMS.LUBRICATION_STICK.DESC, GetPrefabSpriteFnBuilder("LubricationStick".ToTag()), DlcManager.DLC3);
		if (lubricationStick != null)
		{
			lubricationStick.AddSearchTerms(SEARCH_TERMS.MEDICINE);
			lubricationStick.AddSearchTerms(SEARCH_TERMS.BIONIC);
		}
		gasket = AddTechItem("PlasticGasket", RESEARCH.OTHER_TECH_ITEMS.GASKET.NAME, RESEARCH.OTHER_TECH_ITEMS.GASKET.DESC, GetPrefabSpriteFnBuilder("PlasticGasket".ToTag()), null, null, poi_unlock: true);
		rubberBoots = AddTechItem(RubberBootsConfig.ID, RESEARCH.OTHER_TECH_ITEMS.RUBBER_BOOTS.NAME, RESEARCH.OTHER_TECH_ITEMS.RUBBER_BOOTS.DESC, GetPrefabSpriteFnBuilder(RubberBootsConfig.ID.ToTag()), DlcManager.DLC5);
		drySuit = AddTechItem("DrySuit", RESEARCH.OTHER_TECH_ITEMS.DRY_SUIT.NAME, RESEARCH.OTHER_TECH_ITEMS.DRY_SUIT.DESC, GetPrefabSpriteFnBuilder("DrySuit".ToTag()), DlcManager.DLC5);
		disposableElectrobankUraniumOre = AddTechItem("DisposableElectrobank_UraniumOre", RESEARCH.OTHER_TECH_ITEMS.DISPOSABLE_ELECTROBANK_URANIUM_ORE.NAME, RESEARCH.OTHER_TECH_ITEMS.DISPOSABLE_ELECTROBANK_URANIUM_ORE.DESC, GetPrefabSpriteFnBuilder("DisposableElectrobank_UraniumOre".ToTag()), new string[2] { "EXPANSION1_ID", "DLC3_ID" });
		if (disposableElectrobankUraniumOre != null)
		{
			disposableElectrobankUraniumOre.AddSearchTerms(SEARCH_TERMS.BATTERY);
		}
		electrobank = AddTechItem("Electrobank", RESEARCH.OTHER_TECH_ITEMS.ELECTROBANK.NAME, RESEARCH.OTHER_TECH_ITEMS.ELECTROBANK.DESC, GetPrefabSpriteFnBuilder("Electrobank".ToTag()), DlcManager.DLC3);
		if (electrobank != null)
		{
			electrobank.AddSearchTerms(SEARCH_TERMS.BATTERY);
		}
		fetchDrone = AddTechItem("FetchDrone", RESEARCH.OTHER_TECH_ITEMS.FETCHDRONE.NAME, RESEARCH.OTHER_TECH_ITEMS.FETCHDRONE.DESC, GetPrefabSpriteFnBuilder("FetchDrone".ToTag()), DlcManager.DLC3);
		if (fetchDrone != null)
		{
			fetchDrone.AddSearchTerms(SEARCH_TERMS.ROBOT);
		}
		selfChargingElectrobank = AddTechItem("SelfChargingElectrobank", RESEARCH.OTHER_TECH_ITEMS.SELFCHARGINGELECTROBANK.NAME, RESEARCH.OTHER_TECH_ITEMS.SELFCHARGINGELECTROBANK.DESC, GetPrefabSpriteFnBuilder("SelfChargingElectrobank".ToTag()), new string[2] { "EXPANSION1_ID", "DLC3_ID" });
		if (selfChargingElectrobank != null)
		{
			selfChargingElectrobank.AddSearchTerms(SEARCH_TERMS.BATTERY);
		}
	}

	private Func<string, bool, Sprite> GetSpriteFnBuilder(string spriteName)
	{
		return (string anim, bool centered) => Assets.GetSprite(spriteName);
	}

	private Func<string, bool, Sprite> GetPrefabSpriteFnBuilder(Tag prefabTag)
	{
		return (string anim, bool centered) => Def.GetUISprite(prefabTag).first;
	}

	[Obsolete("Used AddTechItem with requiredDlcIds and forbiddenDlcIds instead.")]
	public TechItem AddTechItem(string id, string name, string description, Func<string, bool, Sprite> getUISprite, string[] DLCIds, bool poi_unlock = false)
	{
		DlcManager.ConvertAvailableToRequireAndForbidden(DLCIds, out var requiredDlcIds, out var forbiddenDlcIds);
		return AddTechItem(id, name, description, getUISprite, requiredDlcIds, forbiddenDlcIds, poi_unlock);
	}

	public TechItem AddTechItem(string id, string name, string description, Func<string, bool, Sprite> getUISprite, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null, bool poi_unlock = false)
	{
		if (!DlcManager.IsCorrectDlcSubscribed(requiredDlcIds, forbiddenDlcIds))
		{
			return null;
		}
		if (TryGet(id) != null)
		{
			DebugUtil.LogWarningArgs("Tried adding a tech item called", id, name, "but it was already added!");
			return Get(id);
		}
		Tech techFromItemID = GetTechFromItemID(id);
		if (techFromItemID == null)
		{
			return null;
		}
		TechItem techItem = new TechItem(id, this, name, description, getUISprite, techFromItemID.Id, requiredDlcIds, forbiddenDlcIds, poi_unlock);
		techFromItemID.unlockedItems.Add(techItem);
		return techItem;
	}

	public bool IsTechItemComplete(string id)
	{
		bool result = true;
		foreach (TechItem resource in resources)
		{
			if (resource.Id == id)
			{
				result = resource.IsComplete();
				break;
			}
		}
		return result;
	}

	public Tech GetTechFromItemID(string itemId)
	{
		return Db.Get().Techs?.TryGetTechForTechItem(itemId);
	}

	public int GetTechTierForItem(string itemId)
	{
		Tech techFromItemID = GetTechFromItemID(itemId);
		if (techFromItemID != null)
		{
			return Techs.GetTier(techFromItemID);
		}
		return 0;
	}
}
