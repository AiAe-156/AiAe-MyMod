namespace ProcGenGame;

public struct WorldgenSimData
{
	public Sim.Cell[] cells;

	public Sim.DiseaseCell[] diseaseCells;

	public Sim.SimBackwall[] backwallCells;

	public void Init(int cellCount)
	{
		cells = new Sim.Cell[cellCount];
		diseaseCells = new Sim.DiseaseCell[cellCount];
		backwallCells = new Sim.SimBackwall[cellCount];
		ushort elementIndex = ElementLoader.GetElementIndex(SimHashes.Vacuum);
		for (int i = 0; i < cellCount; i++)
		{
			backwallCells[i].elementIdx = elementIndex;
		}
	}
}
