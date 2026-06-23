namespace UtilLibs;

public class ModHashes
{
	private readonly int value;

	private readonly string name;

	private readonly GameHashes hash;

	public ModHashes(string name)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		this.name = name;
		value = Hash.SDBMLower(name);
		hash = (GameHashes)value;
	}

	public static implicit operator GameHashes(ModHashes modHashes)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return modHashes.hash;
	}

	public static implicit operator int(ModHashes modHashes)
	{
		return modHashes.value;
	}

	public static implicit operator string(ModHashes modHashes)
	{
		return modHashes.name;
	}

	public override string ToString()
	{
		return name;
	}
}
