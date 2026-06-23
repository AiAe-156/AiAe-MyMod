using UnityEngine;

namespace Database;

public class EquippableFacadeResource : PermitResource
{
	public string BuildOverride { get; private set; }

	public string DefID { get; private set; }

	public KAnimFile AnimFile { get; private set; }

	public EquippableFacadeResource(string id, string name, string desc, PermitRarity rarity, string buildOverride, string defID, string animFile, string[] requiredDlcIds, string[] forbiddenDlcIds)
		: base(id, name, desc, PermitCategory.Equipment, rarity, requiredDlcIds, forbiddenDlcIds)
	{
		DefID = defID;
		BuildOverride = buildOverride;
		AnimFile = Assets.GetAnim(animFile);
	}

	public Tuple<Sprite, Color> GetUISprite()
	{
		if (AnimFile == null)
		{
			Debug.LogError("Facade AnimFile is null: " + DefID);
		}
		Sprite uISpriteFromMultiObjectAnim = Def.GetUISpriteFromMultiObjectAnim(AnimFile);
		return new Tuple<Sprite, Color>(uISpriteFromMultiObjectAnim, (uISpriteFromMultiObjectAnim != null) ? Color.white : Color.clear);
	}

	public override PermitPresentationInfo GetPermitPresentationInfo()
	{
		PermitPresentationInfo result = new PermitPresentationInfo
		{
			sprite = GetUISprite().first
		};
		GameObject gameObject = Assets.TryGetPrefab(DefID);
		if (gameObject == null || !gameObject)
		{
			result.SetFacadeForPrefabID(DefID);
		}
		else
		{
			result.SetFacadeForPrefabName(gameObject.GetProperName());
		}
		return result;
	}
}
