public class LongRangeSculpture : Sculpture
{
	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		overrideAnims = null;
		SetOffsetTable(OffsetGroups.InvertedStandardTable);
		multitoolContext = "dig";
		multitoolHitEffectTag = "fx_dig_splash";
	}
}
