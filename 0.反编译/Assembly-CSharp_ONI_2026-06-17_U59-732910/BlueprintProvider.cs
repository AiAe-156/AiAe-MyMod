using System;
using System.Collections.Generic;
using Database;

public abstract class BlueprintProvider : IHasDlcRestrictions
{
	public enum ArtableType
	{
		Painting,
		PaintingTall,
		PaintingWide,
		Sculpture,
		SculptureSmall,
		SculptureIce,
		SculptureMetal,
		SculptureMarble,
		SculptureWood,
		FossilSculpture,
		CeilingFossilSculpture
	}

	public enum ArtableQuality
	{
		LookingGreat,
		LookingOkay,
		LookingUgly
	}

	public enum ClothingType
	{
		DupeTops = 1,
		DupeBottoms = 2,
		DupeGloves = 3,
		DupeShoes = 4,
		DupeHats = 5,
		DupeAccessories = 6,
		AtmoSuitHelmet = 7,
		AtmoSuitBody = 8,
		AtmoSuitGloves = 9,
		AtmoSuitBelt = 10,
		AtmoSuitShoes = 11,
		JetSuitHelmet = 18,
		JetSuitBody = 19,
		JetSuitGloves = 20,
		JetSuitShoes = 21
	}

	public enum OutfitType
	{
		Clothing = 0,
		AtmoSuit = 2,
		JetSuit = 3
	}

	public enum JoyResponseType
	{
		BallonSet
	}

	public enum MonumentPart
	{
		Bottom = 0,
		Top = 2,
		Middle = 1
	}

	protected readonly ref struct ArtableInfoAuthoringHelper
	{
		private readonly ArtableType artableType;

		private readonly ArtableInfo artableInfo;

		public ArtableInfoAuthoringHelper(ArtableType artableType, ArtableInfo artableInfo)
		{
			this.artableType = artableType;
			this.artableInfo = artableInfo;
		}

		public void Quality(ArtableQuality artableQuality)
		{
			if (artableInfo != null)
			{
				int num;
				int num2;
				int num3;
				if (artableType == ArtableType.SculptureWood)
				{
					num = 4;
					num2 = 8;
					num3 = 12;
				}
				else
				{
					num = 5;
					num2 = 10;
					num3 = 15;
				}
				int decor_value;
				bool cheer_on_complete;
				string status_id;
				switch (artableQuality)
				{
				case ArtableQuality.LookingGreat:
					decor_value = num3;
					cheer_on_complete = true;
					status_id = "LookingGreat";
					break;
				case ArtableQuality.LookingOkay:
					decor_value = num2;
					cheer_on_complete = false;
					status_id = "LookingOkay";
					break;
				case ArtableQuality.LookingUgly:
					decor_value = num;
					cheer_on_complete = false;
					status_id = "LookingUgly";
					break;
				default:
					throw new ArgumentException();
				}
				artableInfo.decor_value = decor_value;
				artableInfo.cheer_on_complete = cheer_on_complete;
				artableInfo.status_id = status_id;
			}
		}
	}

	public BlueprintCollection blueprintCollection;

	private string[] requiredDlcIds;

	private string[] forbiddenDlcIds;

	protected void AddBuilding(string prefabConfigId, PermitRarity rarity, string permitId, string animFile)
	{
		blueprintCollection.buildingFacades.Add(new BuildingFacadeInfo(permitId, Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".NAME"), Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".DESC"), rarity, prefabConfigId, animFile, null, requiredDlcIds, forbiddenDlcIds));
	}

	protected void AddBuildingWithData(string prefabConfigId, PermitRarity rarity, string permitId, string animFile, Dictionary<string, string> data)
	{
		blueprintCollection.buildingFacades.Add(new BuildingFacadeInfo(permitId, Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".NAME"), Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".DESC"), rarity, prefabConfigId, animFile, null, requiredDlcIds, forbiddenDlcIds, data));
	}

	protected void AddBuildingWithInteract(string prefabConfigId, PermitRarity rarity, string permitId, string animFile, Dictionary<string, string> interact_anim)
	{
		blueprintCollection.buildingFacades.Add(new BuildingFacadeInfo(permitId, Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".NAME"), Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".DESC"), rarity, prefabConfigId, animFile, interact_anim, requiredDlcIds, forbiddenDlcIds));
	}

	protected void AddClothing(ClothingType clothingType, PermitRarity rarity, string permitId, string animFile)
	{
		blueprintCollection.clothingItems.Add(new ClothingItemInfo(permitId, Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".NAME"), Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".DESC"), (PermitCategory)clothingType, rarity, animFile, requiredDlcIds, forbiddenDlcIds));
	}

	protected ArtableInfoAuthoringHelper AddArtable(ArtableType artableType, PermitRarity rarity, string permitId, string animFile)
	{
		string text = artableType switch
		{
			ArtableType.Painting => "Canvas", 
			ArtableType.PaintingTall => "CanvasTall", 
			ArtableType.PaintingWide => "CanvasWide", 
			ArtableType.Sculpture => "Sculpture", 
			ArtableType.SculptureSmall => "SmallSculpture", 
			ArtableType.SculptureIce => "IceSculpture", 
			ArtableType.SculptureMetal => "MetalSculpture", 
			ArtableType.SculptureMarble => "MarbleSculpture", 
			ArtableType.SculptureWood => "WoodSculpture", 
			ArtableType.FossilSculpture => "FossilSculpture", 
			ArtableType.CeilingFossilSculpture => "CeilingFossilSculpture", 
			_ => null, 
		};
		bool flag = true;
		if (text == null)
		{
			DebugUtil.DevAssert(test: false, "Failed to get buildingConfigId from " + artableType);
			flag = false;
		}
		ArtableInfoAuthoringHelper result;
		if (flag)
		{
			KAnimFile anim;
			ArtableInfo artableInfo = new ArtableInfo(permitId, Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".NAME"), Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".DESC"), rarity, animFile, (!Assets.TryGetAnim(animFile, out anim)) ? null : anim.GetData().GetAnim(0).name, 0, cheer_on_complete: false, "error", text, "", requiredDlcIds, forbiddenDlcIds);
			result = new ArtableInfoAuthoringHelper(artableType, artableInfo);
			result.Quality(ArtableQuality.LookingGreat);
			blueprintCollection.artables.Add(artableInfo);
		}
		else
		{
			ArtableInfo artableInfo = null;
			result = default(ArtableInfoAuthoringHelper);
		}
		return result;
	}

	protected void AddJoyResponse(JoyResponseType joyResponseType, PermitRarity rarity, string permitId, string animFile)
	{
		if (joyResponseType == JoyResponseType.BallonSet)
		{
			blueprintCollection.balloonArtistFacades.Add(new BalloonArtistFacadeInfo(permitId, Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".NAME"), Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".DESC"), rarity, animFile, BalloonArtistFacadeType.ThreeSet, requiredDlcIds, forbiddenDlcIds));
			return;
		}
		throw new NotImplementedException("Missing case for " + joyResponseType);
	}

	protected void AddOutfit(OutfitType outfitType, string outfitId, string[] permitIdList)
	{
		blueprintCollection.outfits.Add(new ClothingOutfitResource(outfitId, permitIdList, Strings.Get("STRINGS.BLUEPRINTS." + outfitId.ToUpper() + ".NAME"), (ClothingOutfitUtility.OutfitType)outfitType, requiredDlcIds, forbiddenDlcIds));
	}

	protected void AddMonumentPart(MonumentPart part, PermitRarity rarity, string permitId, string animFile)
	{
		string symbolName = "";
		switch (part)
		{
		case MonumentPart.Bottom:
			symbolName = "base";
			break;
		case MonumentPart.Middle:
			symbolName = "mid";
			break;
		case MonumentPart.Top:
			symbolName = "top";
			break;
		}
		blueprintCollection.monumentParts.Add(new MonumentPartInfo(permitId, Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".NAME"), Strings.Get("STRINGS.BLUEPRINTS." + permitId.ToUpper() + ".DESC"), rarity, animFile, permitId.Replace("permit_", ""), symbolName, (MonumentPartResource.Part)part, requiredDlcIds, forbiddenDlcIds));
	}

	public virtual string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public virtual string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}

	public abstract void SetupBlueprints();

	public void Internal_PreSetupBlueprints()
	{
		requiredDlcIds = GetRequiredDlcIds();
		forbiddenDlcIds = GetForbiddenDlcIds();
	}
}
