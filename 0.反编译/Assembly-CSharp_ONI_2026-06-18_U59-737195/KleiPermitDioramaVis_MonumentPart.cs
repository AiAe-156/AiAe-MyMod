using Database;
using UnityEngine;

public class KleiPermitDioramaVis_MonumentPart : KMonoBehaviour, IKleiPermitDioramaVisTarget
{
	[SerializeField]
	private KBatchedAnimController buildingKAnim;

	private PrefabDefinedUIPosition buildingKAnimPosition = new PrefabDefinedUIPosition();

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public void ConfigureSetup()
	{
	}

	public void ConfigureWith(PermitResource permit)
	{
		MonumentPartResource monumentPermit = (MonumentPartResource)permit;
		KleiPermitVisUtil.ConfigureToRenderBuilding(buildingKAnim, monumentPermit);
		BuildingDef buildingDef = KleiPermitVisUtil.GetBuildingDef(permit);
		buildingKAnimPosition.SetOn(buildingKAnim);
		buildingKAnim.rectTransform().anchoredPosition += new Vector2(0f, -176f + (float)(buildingDef.HeightInCells * 6));
		buildingKAnim.rectTransform().localScale = Vector3.one * 0.55f;
		KleiPermitVisUtil.AnimateIn(buildingKAnim);
	}
}
