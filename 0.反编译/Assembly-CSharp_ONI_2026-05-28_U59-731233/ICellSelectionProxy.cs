using UnityEngine;

public interface ICellSelectionProxy
{
	const float CELL_SELECTION_Z_OFFSET = -0.6f;

	const float BACKWALL_SELECTION_Z_OFFSET = -0.5f;

	Element Element { get; }

	void OnObjectSelected(object o);

	static bool IsSelectionProxy(GameObject go)
	{
		return CellSelectionObject.IsSelectionObject(go) || BackwallSelectionObject.IsBackwallSelectionObject(go);
	}
}
