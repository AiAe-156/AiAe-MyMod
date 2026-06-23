using System;
using System.IO;

namespace FUtility.SaveData;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ExternalAttribute : Attribute
{
	public string path;

	public ExternalAttribute()
	{
		path = Path.Combine(Util.RootFolder(), "mods", "settings", "akismods", Log.modName.ToLowerInvariant());
	}

	public ExternalAttribute(params string[] path)
	{
		string path2 = Path.Combine(path);
		this.path = Path.Combine(Util.RootFolder(), path2);
	}
}
