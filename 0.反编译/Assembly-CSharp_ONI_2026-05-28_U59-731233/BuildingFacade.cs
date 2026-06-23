using System.Collections.Generic;
using Database;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class BuildingFacade : KMonoBehaviour
{
	[Serialize]
	private string currentFacade;

	public KAnimFile[] animFiles;

	public Dictionary<string, KAnimFile[]> interactAnims = new Dictionary<string, KAnimFile[]>();

	private BuildingFacadeAnimateIn animateIn;

	public string CurrentFacade => currentFacade;

	public bool IsOriginal => currentFacade.IsNullOrWhiteSpace();

	protected override void OnPrefabInit()
	{
	}

	protected override void OnSpawn()
	{
		if (!IsOriginal)
		{
			ApplyBuildingFacade(Db.GetBuildingFacades().TryGet(currentFacade));
		}
	}

	public void ApplyDefaultFacade(bool shouldTryAnimate = false)
	{
		currentFacade = "DEFAULT_FACADE";
		ClearFacade(shouldTryAnimate);
	}

	public void ApplyBuildingFacade(BuildingFacadeResource facade, bool shouldTryAnimate = false)
	{
		if (facade == null)
		{
			ClearFacade();
			return;
		}
		currentFacade = facade.Id;
		KAnimFile[] array = new KAnimFile[1] { Assets.GetAnim(facade.AnimFile) };
		ChangeBuilding(array, facade.Name, facade.Description, facade.InteractFile, shouldTryAnimate, facade.Data);
	}

	private void ClearFacade(bool shouldTryAnimate = false)
	{
		Building component = GetComponent<Building>();
		ChangeBuilding(component.Def.AnimFiles, component.Def.Name, component.Def.Desc, null, shouldTryAnimate);
	}

	private void ChangeBuilding(KAnimFile[] animFiles, string displayName, string desc, Dictionary<string, string> interactAnimsNames = null, bool shouldTryAnimate = false, Dictionary<string, string> data = null)
	{
		interactAnims.Clear();
		if (interactAnimsNames != null && interactAnimsNames.Count > 0)
		{
			interactAnims = new Dictionary<string, KAnimFile[]>();
			foreach (KeyValuePair<string, string> interactAnimsName in interactAnimsNames)
			{
				interactAnims.Add(interactAnimsName.Key, new KAnimFile[1] { Assets.GetAnim(interactAnimsName.Value) });
			}
		}
		Dictionary<string, string> newData = ((data == null) ? null : new Dictionary<string, string>(data));
		Building[] components = GetComponents<Building>();
		Building[] array = components;
		foreach (Building building in array)
		{
			KBatchedAnimController component = building.GetComponent<KBatchedAnimController>();
			HashedString batchGroupID = component.batchGroupID;
			component.SwapAnims(animFiles);
			KBatchedAnimController[] componentsInChildren = building.GetComponentsInChildren<KBatchedAnimController>(includeInactive: true);
			KBatchedAnimController[] array2 = componentsInChildren;
			foreach (KBatchedAnimController kBatchedAnimController in array2)
			{
				if (kBatchedAnimController.batchGroupID == batchGroupID)
				{
					kBatchedAnimController.SwapAnims(animFiles);
				}
			}
			if (!animateIn.IsNullOrDestroyed())
			{
				Object.Destroy(animateIn);
				animateIn = null;
			}
			if (shouldTryAnimate)
			{
				animateIn = BuildingFacadeAnimateIn.MakeFor(component);
				string parameter = "Unlocked";
				float parameterValue = 1f;
				KFMOD.PlayUISoundWithParameter(GlobalAssets.GetSound(KleiInventoryScreen.GetFacadeItemSoundName(Db.Get().Permits.TryGet(currentFacade)) + "_Click"), parameter, parameterValue);
			}
			BuildingFacadeCustomData.ApplyCustomData(building, newData);
		}
		UserNameable component2 = GetComponent<UserNameable>();
		if (component2 != null && !string.IsNullOrEmpty(component2.savedName) && component2.savedName != GetComponent<Building>().Def.Name)
		{
			component2.SetName(component2.savedName);
		}
		else
		{
			GetComponent<KSelectable>().SetName(displayName);
			if (DetailsScreen.Instance != null && DetailsScreen.Instance.target == base.gameObject)
			{
				DetailsScreen.Instance.RefreshTitle();
			}
		}
		if (GetComponent<AnimTileable>() != null && components.Length != 0)
		{
			GameScenePartitioner.Instance.TriggerEvent(components[0].GetExtents(), GameScenePartitioner.Instance.objectLayers[1], null);
		}
	}

	public string GetNextFacade()
	{
		BuildingDef def = GetComponent<Building>().Def;
		int num = def.AvailableFacades.FindIndex((string s) => s == currentFacade) + 1;
		if (num >= def.AvailableFacades.Count)
		{
			num = 0;
		}
		return def.AvailableFacades[num];
	}
}
