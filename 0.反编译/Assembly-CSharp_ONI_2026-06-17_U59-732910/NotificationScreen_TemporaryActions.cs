using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class NotificationScreen_TemporaryActions : KMonoBehaviour
{
	public TemporaryActionRow originalRow;

	private List<TemporaryActionRow> rows = new List<TemporaryActionRow>();

	private TemporaryActionRow cameraReturnRow;

	private Vector3 cameraPositionToReturnTo = Vector3.zero;

	private const float CAMERA_RETURN_BUTTON_LIFETIME = 10f;

	public static NotificationScreen_TemporaryActions Instance { get; private set; }

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
		originalRow.gameObject.SetActive(value: false);
	}

	private TemporaryActionRow CreateActionRow()
	{
		TemporaryActionRow temporaryActionRow = Util.KInstantiateUI<TemporaryActionRow>(originalRow.gameObject, originalRow.transform.parent.gameObject);
		temporaryActionRow.gameObject.SetActive(value: true);
		temporaryActionRow.transform.SetAsLastSibling();
		rows.Add(temporaryActionRow);
		return temporaryActionRow;
	}

	private void RemoveRow(TemporaryActionRow row)
	{
		if (rows.Contains(row))
		{
			rows.Remove(row);
		}
		row.OnRowHidden = null;
		row.gameObject.DeleteObject();
	}

	protected override void OnCleanUp()
	{
		if (rows != null)
		{
			TemporaryActionRow[] array = rows.ToArray();
			foreach (TemporaryActionRow temporaryActionRow in array)
			{
				if (temporaryActionRow != null)
				{
					RemoveRow(temporaryActionRow);
				}
			}
			rows.Clear();
		}
		base.OnCleanUp();
	}

	public void CreateCameraReturnActionButton(Vector3 positionToReturnTo)
	{
		if (cameraReturnRow == null)
		{
			cameraReturnRow = CreateActionRow();
			cameraReturnRow.Setup(UI.TEMPORARY_ACTIONS.CAMERA_RETURN.NAME, UI.TEMPORARY_ACTIONS.CAMERA_RETURN.TOOLTIP, Assets.GetSprite("action_follow_cam"));
			cameraReturnRow.gameObject.name = "TemporaryActionRow_CameraReturn";
			cameraPositionToReturnTo = positionToReturnTo;
			cameraReturnRow.OnRowHidden = RemoveRow;
			cameraReturnRow.OnRowClicked = OnCameraReturnActionButtonClicked;
		}
		cameraReturnRow.SetLifetime(10f);
	}

	private void OnCameraReturnActionButtonClicked(TemporaryActionRow row)
	{
		if (cameraPositionToReturnTo != Vector3.zero)
		{
			GameUtil.FocusCamera(cameraPositionToReturnTo, 2f, playSound: true, show_back_button: false);
		}
	}
}
