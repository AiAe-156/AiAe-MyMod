using System;
using STRINGS;

namespace Database;

public class BalloonArtistFacadeResource : PermitResource
{
	private BalloonArtistFacadeType balloonFacadeType;

	public readonly string[] balloonOverrideSymbolIDs;

	public int nextSymbolIndex;

	public string animFilename { get; private set; }

	public KAnimFile AnimFile { get; private set; }

	public BalloonArtistFacadeResource(string id, string name, string desc, PermitRarity rarity, string animFile, BalloonArtistFacadeType balloonFacadeType, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
		: base(id, name, desc, PermitCategory.JoyResponse, rarity, requiredDlcIds, forbiddenDlcIds)
	{
		AnimFile = Assets.GetAnim(animFile);
		animFilename = animFile;
		this.balloonFacadeType = balloonFacadeType;
		Db.Get().Accessories.AddAccessories(id, AnimFile);
		balloonOverrideSymbolIDs = GetBalloonOverrideSymbolIDs();
		Debug.Assert(balloonOverrideSymbolIDs.Length != 0);
	}

	public override PermitPresentationInfo GetPermitPresentationInfo()
	{
		PermitPresentationInfo result = default(PermitPresentationInfo);
		result.sprite = Def.GetUISpriteFromMultiObjectAnim(AnimFile);
		result.SetFacadeForText(UI.KLEI_INVENTORY_SCREEN.BALLOON_ARTIST_FACADE_FOR);
		return result;
	}

	public BalloonOverrideSymbol GetNextOverride()
	{
		int num = nextSymbolIndex;
		nextSymbolIndex = (nextSymbolIndex + 1) % balloonOverrideSymbolIDs.Length;
		return new BalloonOverrideSymbol(animFilename, balloonOverrideSymbolIDs[num]);
	}

	public BalloonOverrideSymbolIter GetSymbolIter()
	{
		return new BalloonOverrideSymbolIter(this);
	}

	public BalloonOverrideSymbol GetOverrideAt(int index)
	{
		return new BalloonOverrideSymbol(animFilename, balloonOverrideSymbolIDs[index]);
	}

	private string[] GetBalloonOverrideSymbolIDs()
	{
		_ = AnimFile.GetData().build;
		return balloonFacadeType switch
		{
			BalloonArtistFacadeType.Single => new string[1] { "body" }, 
			BalloonArtistFacadeType.ThreeSet => new string[3] { "body1", "body2", "body3" }, 
			_ => throw new NotImplementedException(), 
		};
	}
}
