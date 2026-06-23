using System;
using System.Collections.Generic;

public static class DevToolCommandPaletteUtil
{
	public static List<DevToolCommandPalette.Command> GenerateDefaultCommandPalette()
	{
		List<DevToolCommandPalette.Command> list = new List<DevToolCommandPalette.Command>();
		List<Type> list2 = ReflectionUtil.CollectTypesThatInheritOrImplement<DevTool>();
		foreach (Type devToolType in list2)
		{
			if (!devToolType.IsAbstract && ReflectionUtil.HasDefaultConstructor(devToolType))
			{
				list.Add(new DevToolCommandPalette.Command("Open DevTool: \"" + DevToolUtil.GenerateDevToolName(devToolType) + "\"", delegate
				{
					DevToolUtil.Open((DevTool)Activator.CreateInstance(devToolType));
				}));
			}
		}
		return list;
	}
}
