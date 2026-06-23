using STRINGS;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/CopyBuildingSettings")]
public class CopyBuildingSettings : KMonoBehaviour
{
	[MyCmpReq]
	private KPrefabID id;

	public Tag copyGroupTag;

	private static readonly EventSystem.IntraObjectHandler<CopyBuildingSettings> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<CopyBuildingSettings>(delegate(CopyBuildingSettings component, object data)
	{
		component.OnRefreshUserMenu(data);
	});

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Subscribe(493375141, OnRefreshUserMenuDelegate);
	}

	private void OnRefreshUserMenu(object data)
	{
		Game.Instance.userMenu.AddButton(base.gameObject, new KIconButtonMenu.ButtonInfo("action_mirror", UI.USERMENUACTIONS.COPY_BUILDING_SETTINGS.NAME, ActivateCopyTool, Action.BuildingUtility1, null, null, null, UI.USERMENUACTIONS.COPY_BUILDING_SETTINGS.TOOLTIP));
	}

	private void ActivateCopyTool()
	{
		CopySettingsTool.Instance.SetSourceObject(base.gameObject);
		PlayerController.Instance.ActivateTool(CopySettingsTool.Instance);
	}

	public static ObjectLayer ResolveLayer(GameObject sourceGameObject)
	{
		ObjectLayer result = ObjectLayer.Building;
		if (sourceGameObject.TryGetComponent<MoverLayerOccupier>(out var _))
		{
			result = ObjectLayer.Mover;
		}
		if (sourceGameObject.TryGetComponent<BuildingComplete>(out var component2))
		{
			result = component2.Def.ObjectLayer;
		}
		return result;
	}

	public static KPrefabID ResolveTarget(ObjectLayer layer, int targetCell)
	{
		GameObject gameObject = Grid.Objects[targetCell, (int)layer];
		if (gameObject == null)
		{
			return null;
		}
		gameObject.TryGetComponent<KPrefabID>(out var component);
		return component;
	}

	public static bool ApplyCopy(KPrefabID other_id, GameObject sourceGameObject, KPrefabID source_id, CopyBuildingSettings source_settings)
	{
		DebugUtil.DevAssert(other_id.gameObject != sourceGameObject, "source and target must not be equal");
		if (other_id.gameObject == sourceGameObject)
		{
			return false;
		}
		if (!other_id.gameObject.TryGetComponent<CopyBuildingSettings>(out var component))
		{
			return false;
		}
		if (source_settings.copyGroupTag != Tag.Invalid)
		{
			if (source_settings.copyGroupTag != component.copyGroupTag)
			{
				return false;
			}
		}
		else if (other_id.PrefabID() != source_id.PrefabID())
		{
			return false;
		}
		other_id.Trigger(-905833192, (object)sourceGameObject);
		PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, UI.COPIED_SETTINGS, other_id.gameObject.transform, new Vector3(0f, 0.5f, 0f));
		return true;
	}
}
