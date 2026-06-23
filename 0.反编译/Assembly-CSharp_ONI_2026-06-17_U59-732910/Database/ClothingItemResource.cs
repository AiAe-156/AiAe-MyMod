using STRINGS;

namespace Database;

public class ClothingItemResource : PermitResource
{
	public string animFilename { get; private set; }

	public KAnimFile AnimFile { get; private set; }

	public ClothingOutfitUtility.OutfitType outfitType { get; private set; }

	public ClothingItemResource(string id, string name, string desc, ClothingOutfitUtility.OutfitType outfitType, PermitCategory category, PermitRarity rarity, string animFile, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
		: base(id, name, desc, category, rarity, requiredDlcIds, forbiddenDlcIds)
	{
		AnimFile = Assets.GetAnim(animFile);
		animFilename = animFile;
		this.outfitType = outfitType;
	}

	public override PermitPresentationInfo GetPermitPresentationInfo()
	{
		PermitPresentationInfo result = default(PermitPresentationInfo);
		if (AnimFile == null)
		{
			Debug.LogError("Clothing kanim is missing from bundle: " + animFilename);
		}
		result.sprite = Def.GetUISpriteFromMultiObjectAnim(AnimFile);
		result.SetFacadeForText(UI.KLEI_INVENTORY_SCREEN.CLOTHING_ITEM_FACADE_FOR);
		return result;
	}
}
