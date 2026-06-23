using Database;
using UnityEngine;

public class KleiPermitDioramaVis_BuildingOnFloorBig : KMonoBehaviour, IKleiPermitDioramaVisTarget
{
	[SerializeField]
	private KBatchedAnimController buildingKAnim;

	private Vector2 defaultAnchoredPosition;

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public void ConfigureSetup()
	{
		defaultAnchoredPosition = buildingKAnim.rectTransform().anchoredPosition;
	}

	public void ConfigureWith(PermitResource permit)
	{
		BuildingFacadeResource buildingFacadeResource = (BuildingFacadeResource)permit;
		buildingKAnim.SetSymbolVisiblity("booster", is_visible: false);
		buildingKAnim.SetSymbolVisiblity("blue_light_bloom", is_visible: false);
		buildingKAnim.rectTransform().anchoredPosition = defaultAnchoredPosition;
		buildingKAnim.rectTransform().localScale = Vector3.one * 0.825f;
		string place_anim = "place";
		if (buildingFacadeResource.PrefabID == "SteamTurbine2")
		{
			buildingKAnim.rectTransform().anchoredPosition += new Vector2(0f, 140f);
			place_anim = "place_alt";
		}
		KleiPermitVisUtil.ConfigureToRenderBuilding(buildingKAnim, buildingFacadeResource);
		KleiPermitVisUtil.AnimateIn(buildingKAnim, default(Updater), place_anim);
	}
}
