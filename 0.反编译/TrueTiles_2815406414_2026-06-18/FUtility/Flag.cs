namespace FUtility;

public class Flag
{
	private int value;

	public Flag(int value = 0)
	{
		this.value = value;
	}

	public bool Has(int flag)
	{
		return (value & flag) == flag;
	}

	public bool SetValue(int flag)
	{
		return value == flag;
	}

	public bool Clear()
	{
		return value == 0;
	}

	public void Set(int flag)
	{
		value |= flag;
	}

	public void UnSet(int flag)
	{
		value &= ~flag;
	}

	public void Toggle(int flag)
	{
		value ^= flag;
	}

	public static implicit operator int(Flag flag)
	{
		return flag.value;
	}

	public static implicit operator Flag(int intValue)
	{
		return new Flag(intValue);
	}
}
