using UnityEngine;

public struct OreSizeVisualizerData
{
	public PrimaryElement primaryElement;

	public OreSizeVisualizerComponents.TiersSetType tierSetType;

	public int absorbHandle;

	public int splitFromChunkHandle;

	public OreSizeVisualizerData(GameObject go)
	{
		primaryElement = go.GetComponent<PrimaryElement>();
		tierSetType = OreSizeVisualizerComponents.TiersSetType.Ores;
		absorbHandle = -1;
		splitFromChunkHandle = -1;
	}
}
