using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Options;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public sealed class RequireVersionAttribute : Attribute, IRequireFilter
{
	public uint Version { get; }

	public bool Minimum { get; }

	public RequireVersionAttribute(uint version)
	{
		Version = version;
		Minimum = true;
	}

	public RequireVersionAttribute(uint version, bool minimum)
	{
		Version = version;
		Minimum = minimum;
	}

	public bool Filter()
	{
		return PUtil.GameVersion >= Version == Minimum;
	}

	public override string ToString()
	{
		return "RequireVersion[version={0},minimum={1}]".F(Version, Minimum);
	}
}
