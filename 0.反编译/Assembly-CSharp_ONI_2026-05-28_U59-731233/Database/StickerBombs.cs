namespace Database;

public class StickerBombs : ResourceSet<DbStickerBomb>
{
	public StickerBombs(ResourceSet parent)
		: base("StickerBombs", parent)
	{
		foreach (StickerBombFacadeInfo stickerBombFacade in Blueprints.Get().all.stickerBombFacades)
		{
			Add(stickerBombFacade.id, stickerBombFacade.name, stickerBombFacade.desc, stickerBombFacade.rarity, stickerBombFacade.animFile, stickerBombFacade.sticker, stickerBombFacade.requiredDlcIds, stickerBombFacade.GetForbiddenDlcIds());
		}
	}

	private DbStickerBomb Add(string id, string name, string desc, PermitRarity rarity, string animfilename, string symbolName, string[] requiredDlcIds, string[] forbiddenDlcIds)
	{
		DbStickerBomb dbStickerBomb = new DbStickerBomb(id, name, desc, rarity, animfilename, symbolName, requiredDlcIds, forbiddenDlcIds);
		resources.Add(dbStickerBomb);
		return dbStickerBomb;
	}

	public DbStickerBomb GetRandomSticker()
	{
		return resources.GetRandom();
	}
}
