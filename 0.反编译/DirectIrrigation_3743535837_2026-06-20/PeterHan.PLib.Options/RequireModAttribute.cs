using System;
using System.Collections.Generic;
using KMod;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Options;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public sealed class RequireModAttribute : Attribute, IRequireFilter
{
	private static readonly ISet<string> ACTIVE_MODS = new HashSet<string>();

	public string StaticID { get; }

	public bool Required { get; }

	private static bool IsModActive(string staticID)
	{
		ISet<string> aCTIVE_MODS = ACTIVE_MODS;
		lock (aCTIVE_MODS)
		{
			if (aCTIVE_MODS.Count <= 0)
			{
				List<Mod> mods = Global.Instance.modManager.mods;
				int count = mods.Count;
				for (int i = 0; i < count; i++)
				{
					Mod val = mods[i];
					if (val.IsActive())
					{
						aCTIVE_MODS.Add(val.staticID);
					}
				}
			}
			return aCTIVE_MODS.Contains(staticID);
		}
	}

	public RequireModAttribute(string staticID)
	{
		StaticID = staticID ?? "";
		Required = true;
	}

	public RequireModAttribute(string staticID, bool required)
	{
		StaticID = staticID ?? "";
		Required = required;
	}

	public bool Filter()
	{
		return IsModActive(StaticID) == Required;
	}

	public override string ToString()
	{
		return "RequireMod[StaticID={0},require={1}]".F(StaticID, Required);
	}
}
