using KSerialization;

public class WorldgenOnlyLoreBearer : KMonoBehaviour
{
	[MyCmpReq]
	private LoreBearer loreBearer;

	[Serialize]
	public bool hasLore;

	protected override void OnPrefabInit()
	{
		Subscribe(1119167081, OnNewGameSpawn);
	}

	private void UpdateLore()
	{
		loreBearer.hideLore = !hasLore;
	}

	protected override void OnSpawn()
	{
		UpdateLore();
	}

	private void OnNewGameSpawn(object obj)
	{
		hasLore = true;
		UpdateLore();
	}
}
