using System;
using System.Collections.Generic;
using System.Linq;
using Database;

public static class UIMinionOrMannequinITargetExtensions
{
	public static readonly ClothingItemResource[] EMPTY_OUTFIT = new ClothingItemResource[0];

	public static void SetOutfit(this UIMinionOrMannequin.ITarget self, ClothingOutfitResource outfit)
	{
		self.SetOutfit(outfit.outfitType, outfit.itemsInOutfit.Select((string itemId) => Db.Get().Permits.ClothingItems.Get(itemId)));
	}

	public static void SetOutfit(this UIMinionOrMannequin.ITarget self, OutfitDesignerScreen_OutfitState outfit)
	{
		self.SetOutfit(outfit.outfitType, from itemId in outfit.GetItems()
			select Db.Get().Permits.ClothingItems.Get(itemId));
	}

	public static void SetOutfit(this UIMinionOrMannequin.ITarget self, ClothingOutfitTarget outfit)
	{
		self.SetOutfit(outfit.OutfitType, outfit.ReadItemValues());
	}

	public static void SetOutfit(this UIMinionOrMannequin.ITarget self, ClothingOutfitUtility.OutfitType outfitType, Option<ClothingOutfitTarget> outfit)
	{
		if (outfit.HasValue)
		{
			self.SetOutfit(outfit.Value);
		}
		else
		{
			self.ClearOutfit(outfitType);
		}
	}

	public static void ClearOutfit(this UIMinionOrMannequin.ITarget self, ClothingOutfitUtility.OutfitType outfitType)
	{
		self.SetOutfit(outfitType, EMPTY_OUTFIT);
	}

	public static void React(this UIMinionOrMannequin.ITarget self)
	{
		self.React(UIMinionOrMannequinReactSource.None);
	}

	public static void ReactToClothingItemChange(this UIMinionOrMannequin.ITarget self, PermitCategory clothingChangedCategory)
	{
		self.React(GetSource(clothingChangedCategory));
		static UIMinionOrMannequinReactSource GetSource(PermitCategory permitCategory)
		{
			switch (permitCategory)
			{
			case PermitCategory.DupeHats:
			case PermitCategory.AtmoSuitHelmet:
			case PermitCategory.JetSuitHelmet:
				return UIMinionOrMannequinReactSource.OnHatChanged;
			case PermitCategory.DupeTops:
			case PermitCategory.AtmoSuitBody:
			case PermitCategory.AtmoSuitBelt:
			case PermitCategory.JetSuitBody:
				return UIMinionOrMannequinReactSource.OnTopChanged;
			case PermitCategory.DupeBottoms:
				return UIMinionOrMannequinReactSource.OnBottomChanged;
			case PermitCategory.DupeGloves:
			case PermitCategory.AtmoSuitGloves:
			case PermitCategory.JetSuitGloves:
				return UIMinionOrMannequinReactSource.OnGlovesChanged;
			case PermitCategory.DupeShoes:
			case PermitCategory.AtmoSuitShoes:
			case PermitCategory.JetSuitShoes:
				return UIMinionOrMannequinReactSource.OnShoesChanged;
			default:
				DebugUtil.DevAssert(test: false, $"Couldn't find a reaction for \"{permitCategory}\" clothing item category being changed");
				return UIMinionOrMannequinReactSource.None;
			}
		}
	}

	public static void ReactToPersonalityChange(this UIMinionOrMannequin.ITarget self)
	{
		self.React(UIMinionOrMannequinReactSource.OnPersonalityChanged);
	}

	public static void ReactToFullOutfitChange(this UIMinionOrMannequin.ITarget self)
	{
		self.React(UIMinionOrMannequinReactSource.OnWholeOutfitChanged);
	}

	public static IEnumerable<ClothingItemResource> GetOutfitWithDefaultItems(ClothingOutfitUtility.OutfitType outfitType, IEnumerable<ClothingItemResource> outfit)
	{
		switch (outfitType)
		{
		case ClothingOutfitUtility.OutfitType.JoyResponse:
			throw new NotSupportedException();
		case ClothingOutfitUtility.OutfitType.Clothing:
			return outfit;
		case ClothingOutfitUtility.OutfitType.AtmoSuit:
		{
			using DictionaryPool<PermitCategory, ClothingItemResource, UIMinionOrMannequin.ITarget>.PooledDictionary pooledDictionary2 = PoolsFor<UIMinionOrMannequin.ITarget>.AllocateDict<PermitCategory, ClothingItemResource>();
			foreach (ClothingItemResource item in outfit)
			{
				DebugUtil.DevAssert(!pooledDictionary2.ContainsKey(item.Category), "Duplicate item for category");
				pooledDictionary2[item.Category] = item;
			}
			if (!pooledDictionary2.ContainsKey(PermitCategory.AtmoSuitHelmet))
			{
				pooledDictionary2[PermitCategory.AtmoSuitHelmet] = Db.Get().Permits.ClothingItems.Get("visonly_AtmoHelmetClear");
			}
			if (!pooledDictionary2.ContainsKey(PermitCategory.AtmoSuitBody))
			{
				pooledDictionary2[PermitCategory.AtmoSuitBody] = Db.Get().Permits.ClothingItems.Get("visonly_AtmoSuitBasicBlue");
			}
			if (!pooledDictionary2.ContainsKey(PermitCategory.AtmoSuitGloves))
			{
				pooledDictionary2[PermitCategory.AtmoSuitGloves] = Db.Get().Permits.ClothingItems.Get("visonly_AtmoGlovesBasicBlue");
			}
			if (!pooledDictionary2.ContainsKey(PermitCategory.AtmoSuitBelt))
			{
				pooledDictionary2[PermitCategory.AtmoSuitBelt] = Db.Get().Permits.ClothingItems.Get("visonly_AtmoBeltBasicBlue");
			}
			if (!pooledDictionary2.ContainsKey(PermitCategory.AtmoSuitShoes))
			{
				pooledDictionary2[PermitCategory.AtmoSuitShoes] = Db.Get().Permits.ClothingItems.Get("visonly_AtmoShoesBasicBlack");
			}
			return pooledDictionary2.Values.ToArray();
		}
		case ClothingOutfitUtility.OutfitType.JetSuit:
		{
			using DictionaryPool<PermitCategory, ClothingItemResource, UIMinionOrMannequin.ITarget>.PooledDictionary pooledDictionary = PoolsFor<UIMinionOrMannequin.ITarget>.AllocateDict<PermitCategory, ClothingItemResource>();
			foreach (ClothingItemResource item2 in outfit)
			{
				DebugUtil.DevAssert(!pooledDictionary.ContainsKey(item2.Category), "Duplicate item for category");
				pooledDictionary[item2.Category] = item2;
			}
			if (!pooledDictionary.ContainsKey(PermitCategory.JetSuitHelmet))
			{
				pooledDictionary[PermitCategory.JetSuitHelmet] = Db.Get().Permits.ClothingItems.Get("visonly_JetHelmetClear");
			}
			if (!pooledDictionary.ContainsKey(PermitCategory.JetSuitBody))
			{
				pooledDictionary[PermitCategory.JetSuitBody] = Db.Get().Permits.ClothingItems.Get("visonly_JetSuitBasic");
			}
			if (!pooledDictionary.ContainsKey(PermitCategory.JetSuitGloves))
			{
				pooledDictionary[PermitCategory.JetSuitGloves] = Db.Get().Permits.ClothingItems.Get("visonly_JetGlovesBasic");
			}
			if (!pooledDictionary.ContainsKey(PermitCategory.JetSuitShoes))
			{
				pooledDictionary[PermitCategory.JetSuitShoes] = Db.Get().Permits.ClothingItems.Get("visonly_JetShoesBasic");
			}
			return pooledDictionary.Values.ToArray();
		}
		default:
			throw new NotImplementedException();
		}
	}
}
