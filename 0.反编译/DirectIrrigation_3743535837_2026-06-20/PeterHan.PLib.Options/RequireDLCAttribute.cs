using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Options;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public sealed class RequireDLCAttribute : Attribute, IRequireFilter
{
	public string DlcID { get; }

	public bool Required { get; }

	public RequireDLCAttribute(string dlcID)
	{
		DlcID = dlcID ?? "";
		Required = true;
	}

	public RequireDLCAttribute(string dlcID, bool required)
	{
		DlcID = dlcID ?? "";
		Required = required;
	}

	public bool Filter()
	{
		return PGameUtils.IsDLCOwned(DlcID) == Required;
	}

	public override string ToString()
	{
		return "RequireDLC[DLC={0},require={1}]".F(DlcID, Required);
	}
}
