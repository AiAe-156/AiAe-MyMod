public class DissolvingAlgae : DissolvingElementDiseaseEmitter
{
	private const int LIGHT_THRESHOLD = 500;

	public DissolvingAlgae()
		: base(SimHashes.Oxygen)
	{
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
	}

	protected override void EvaluateEmissionCondition()
	{
		int num = Grid.PosToCell(this);
		bool flag = Grid.IsLiquid(num) && Grid.Element[num].HasTag(GameTags.AnyWater) && Grid.LightIntensity[num] > 500;
		if (enableEmitter != flag)
		{
			SetEnable(flag);
			UpdateStatusItem();
			if (flag)
			{
				SpawnVisualFX();
			}
		}
	}
}
