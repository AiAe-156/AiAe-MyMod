using STRINGS;

namespace Database;

public class MonumentPartResource : PermitResource
{
	public enum Part
	{
		Bottom,
		Middle,
		Top
	}

	public Part part;

	public KAnimFile AnimFile { get; private set; }

	public string SymbolName { get; private set; }

	public string State { get; private set; }

	public MonumentPartResource(string id, string name, string desc, PermitRarity rarity, string animFilename, string state, string symbolName, Part part, string[] requiredDlcIds, string[] forbiddenDlcIds)
		: base(id, name, desc, PermitCategory.Artwork, rarity, requiredDlcIds, forbiddenDlcIds)
	{
		AnimFile = Assets.GetAnim(animFilename);
		SymbolName = symbolName;
		State = state;
		this.part = part;
	}

	public override PermitPresentationInfo GetPermitPresentationInfo()
	{
		PermitPresentationInfo result = default(PermitPresentationInfo);
		result.sprite = Def.GetUISpriteFromMultiObjectAnim(AnimFile);
		result.SetFacadeForText(UI.KLEI_INVENTORY_SCREEN.MONUMENT_PART_FACADE_FOR);
		return result;
	}
}
