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
		if (!DlcManager.IsExpansion1Active())
		{
			return dualString.baseString;
		}
		return dualString.soString;
	}

	public static implicit operator LocString(ExecutableSpecificString dualString)
	{
		return new LocString(dualString);
	}
}
