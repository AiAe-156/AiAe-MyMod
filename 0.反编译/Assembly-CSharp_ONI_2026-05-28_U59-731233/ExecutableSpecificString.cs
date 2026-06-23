public class ExecutableSpecificString
{
	private string baseString;

	private string soString;

	public ExecutableSpecificString(string baseStr, string soStr)
	{
		baseString = baseStr;
		soString = soStr;
	}

	public static implicit operator string(ExecutableSpecificString dualString)
	{
		return DlcManager.IsExpansion1Active() ? dualString.soString : dualString.baseString;
	}

	public static implicit operator LocString(ExecutableSpecificString dualString)
	{
		return new LocString(dualString);
	}
}
