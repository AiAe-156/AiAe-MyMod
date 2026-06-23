using System.Collections.Generic;

public interface IDevQuickAction
{
	public enum CommonMenusNames
	{
		Storage
	}

	List<DevQuickActionInstruction> GetDevInstructions();
}
