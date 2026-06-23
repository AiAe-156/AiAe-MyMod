public class EntitySizeVisualizer : KMonoBehaviour
{
	public OreSizeVisualizerComponents.TiersSetType TierSetType;

	protected override void OnPrefabInit()
	{
		OreSizeVisualizerData cmp = new OreSizeVisualizerData(base.gameObject);
		cmp.tierSetType = TierSetType;
		GameComps.OreSizeVisualizers.Add(base.gameObject, cmp);
		base.OnPrefabInit();
	}

	protected override void OnCleanUp()
	{
		GameComps.OreSizeVisualizers.Remove(base.gameObject);
		base.OnCleanUp();
	}
}
