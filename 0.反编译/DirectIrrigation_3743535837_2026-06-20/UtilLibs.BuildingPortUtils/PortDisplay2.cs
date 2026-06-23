using UnityEngine;

namespace UtilLibs.BuildingPortUtils;

[SkipSaveFileSerialization]
public class PortDisplay2 : KMonoBehaviour
{
	private GameObject portObject;

	[SerializeField]
	private int lastUtilityCell = -1;

	[SerializeField]
	private Color lastColor = Color.black;

	[SerializeField]
	internal ConduitType type;

	[SerializeField]
	internal CellOffset offset;

	[SerializeField]
	internal CellOffset offsetFlipped;

	[SerializeField]
	internal bool input;

	[SerializeField]
	internal Color32 color;

	[SerializeField]
	internal Sprite sprite;

	public bool Input => input;

	public Sprite Sprite => sprite;

	public ConduitType Type => type;

	internal void AssignPort(DisplayConduitPortInfo port)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		type = port.type;
		offset = port.offset;
		offsetFlipped = port.offsetFlipped;
		input = port.input;
		color = Color32.op_Implicit(port.color);
		sprite = SharedConduitUtils.GetSprite(input, type);
	}

	internal void Draw(GameObject obj, BuildingCellVisualizer visualizer, bool force)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Building building = visualizer.building;
		int utilityCell = GetUtilityCell(building);
		if (force || utilityCell != lastUtilityCell || Color32.op_Implicit(color) != lastColor)
		{
			lastColor = Color32.op_Implicit(color);
			lastUtilityCell = utilityCell;
			((EntityCellVisualizer)visualizer).DrawUtilityIcon(utilityCell, sprite, ref portObject, Color32.op_Implicit(color), 1.5f, false);
		}
	}

	private void AttachTooltip(string tooltip)
	{
		if (!((Object)(object)portObject == (Object)null))
		{
		}
	}

	public int GetUtilityCell(Building building)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return building.GetCellWithOffset(((int)building.Orientation == 0) ? offset : offsetFlipped);
	}

	public CellOffset GetUtilityCellOffset(Building building)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return ((int)building.Orientation == 0) ? offset : offsetFlipped;
	}

	internal void DisableIcons()
	{
		if ((Object)(object)portObject != (Object)null && (Object)(object)portObject != (Object)null && portObject.activeInHierarchy)
		{
			portObject.SetActive(false);
		}
	}

	public override void OnCleanUp()
	{
		((KMonoBehaviour)this).OnCleanUp();
		if ((Object)(object)portObject != (Object)null)
		{
			Object.Destroy((Object)(object)portObject);
		}
	}
}
