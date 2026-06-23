using System.Linq;
using UnityEngine;

namespace Database;

public class ClothingOutfitResource : Resource, IHasDlcRestrictions
{
	public ClothingOutfitUtility.OutfitType outfitType;

	public string[] requiredDlcIds;

	public string[] forbiddenDlcIds;

	public string[] itemsInOutfit { get; private set; }

	public ClothingOutfitResource(string id, string[] items_in_outfit, string name, ClothingOutfitUtility.OutfitType outfitType, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
		: base(id, name)
	{
		itemsInOutfit = items_in_outfit;
		this.outfitType = outfitType;
		this.requiredDlcIds = requiredDlcIds;
		this.forbiddenDlcIds = forbiddenDlcIds;
	}

	public Tuple<Sprite, Color> GetUISprite()
	{
		Sprite sprite = Assets.GetSprite("unknown");
		return new Tuple<Sprite, Color>(sprite, (sprite != null) ? Color.white : Color.clear);
	}

	public string GetDlcIdFrom()
	{
		if (requiredDlcIds == null)
		{
			return null;
		}
		return requiredDlcIds.Last();
	}

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}
}
