using System.Collections.Generic;
using Database;
using UnityEngine;
using UnityEngine.UI;

public class KleiPermitDioramaVis_AutomationGates : KMonoBehaviour, IKleiPermitDioramaVisTarget
{
	[SerializeField]
	private Image itemSprite;

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
		itemSprite.gameObject.SetActive(value: false);
		BuildingFacadeResource buildingPermit = (BuildingFacadeResource)permit;
		KleiPermitVisUtil.ConfigureToRenderBuilding(buildingKAnim, buildingPermit);
		BuildingDef buildingDef = KleiPermitVisUtil.GetBuildingDef(permit);
		Dictionary<int, float> dictionary = new Dictionary<int, float>
		{
			{ 3, 0.7f },
			{ 2, 0.9f },
			{ 1, 0.85f }
		};
		Dictionary<int, float> dictionary2 = new Dictionary<int, float>
		{
			{ 4, 32f },
			{ 3, 32f },
			{ 2, 32f },
			{ 1, 96f }
		};
		buildingKAnimPosition.SetOn(buildingKAnim);
		buildingKAnim.rectTransform().localScale = Vector3.one * dictionary[buildingDef.WidthInCells];
		buildingKAnim.rectTransform().anchoredPosition += new Vector2(0f, dictionary2[buildingDef.HeightInCells]);
		KleiPermitVisUtil.AnimateIn(buildingKAnim);
	}
}
