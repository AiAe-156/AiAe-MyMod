using System;

namespace Database;

public class BalloonArtistFacades : ResourceSet<BalloonArtistFacadeResource>
{
	public BalloonArtistFacades(ResourceSet parent)
		: base("BalloonArtistFacades", parent)
	{
		foreach (BalloonArtistFacadeInfo balloonArtistFacade in Blueprints.Get().all.balloonArtistFacades)
		{
			Add(balloonArtistFacade.id, balloonArtistFacade.name, balloonArtistFacade.desc, balloonArtistFacade.rarity, balloonArtistFacade.animFile, balloonArtistFacade.balloonFacadeType, balloonArtistFacade.GetRequiredDlcIds(), balloonArtistFacade.GetForbiddenDlcIds());
		}
	}

	[Obsolete("Please use Add(...) with required/forbidden")]
	public void Add(string id, string name, string desc, PermitRarity rarity, string animFile, BalloonArtistFacadeType balloonFacadeType)
	{
		Add(id, name, desc, rarity, animFile, balloonFacadeType, null, null);
	}

	[Obsolete("Please use Add(...) with required/forbidden")]
	public void Add(string id, string name, string desc, PermitRarity rarity, string animFile, BalloonArtistFacadeType balloonFacadeType, string[] dlcIds)
	{
		Add(id, name, desc, rarity, animFile, balloonFacadeType, null, null);
	}

	public void Add(string id, string name, string desc, PermitRarity rarity, string animFile, BalloonArtistFacadeType balloonFacadeType, string[] requiredDlcIds, string[] forbiddenDlcIds)
	{
		BalloonArtistFacadeResource item = new BalloonArtistFacadeResource(id, name, desc, rarity, animFile, balloonFacadeType, requiredDlcIds, forbiddenDlcIds);
		resources.Add(item);
	}
}
