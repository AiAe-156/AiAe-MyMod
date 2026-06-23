using Database;
using UnityEngine;

public class KleiPermitDioramaVis_BuildingOnFloor : KMonoBehaviour, IKleiPermitDioramaVisTarget
{
	[SerializeField]
	private KBatchedAnimController buildingKAnim;

	private Vector2 defaultScale;

	private RectTransform rectTransform;

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public void ConfigureSetup()
	{
		rectTransform = buildingKAnim.rectTransform();
		defaultScale = rectTransform.localScale;
	}

	public void ConfigureWith(PermitResource permit)
	{
		BuildingFacadeResource buildingFacadeResource = (BuildingFacadeResource)permit;
		string place_anim = "place";
		buildingKAnim.SetSymbolVisiblity("sweep", is_visible: false);
		if (buildingFacadeResource.PrefabID == "LiquidPumpingStation")
		{
			rectTransform.localScale = Vector3.one * 0.7f;
			buildingKAnim.SetSymbolVisiblity("pipe2", is_visible: false);
			buildingKAnim.SetSymbolVisiblity("pipe3", is_visible: false);
			buildingKAnim.SetSymbolVisiblity("pipe4", is_visible: false);
			place_anim = "place_alt";
		}
		else
		{
			rectTransform.localScale = defaultScale;
		}
		KleiPermitVisUtil.ConfigureToRenderBuilding(buildingKAnim, buildingFacadeResource);
		KleiPermitVisUtil.AnimateIn(buildingKAnim, default(Updater), place_anim);
	}
}
