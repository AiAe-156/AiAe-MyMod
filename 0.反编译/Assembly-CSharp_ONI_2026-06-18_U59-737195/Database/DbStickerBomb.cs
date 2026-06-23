namespace Database;

public class DbStickerBomb : PermitResource
{
	public string id;

	public string sticker;

	public KAnimFile animFile;

	private const string stickerAnimPrefix = "idle_sticker";

	private const string stickerSymbolPrefix = "sticker";

	public DbStickerBomb(string id, string name, string desc, PermitRarity rarity, string animfilename, string sticker, string[] requiredDlcIds, string[] forbiddenDlcIds)
		: base(id, name, desc, PermitCategory.Artwork, rarity, requiredDlcIds, forbiddenDlcIds)
	{
		this.id = id;
		this.sticker = sticker;
		animFile = Assets.GetAnim(animfilename);
	}

	public override PermitPresentationInfo GetPermitPresentationInfo()
	{
		return new PermitPresentationInfo
		{
			sprite = Def.GetUISpriteFromMultiObjectAnim(animFile, string.Format("{0}_{1}", "idle_sticker", sticker), centered: false, string.Format("{0}_{1}", "sticker", sticker))
		};
	}
}
