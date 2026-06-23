using System;

namespace PeterHan.PLib.Detours;

/// <summary>
/// An exception thrown when constructing a detour.
/// </summary>
public class DetourException : ArgumentException
{
	public DetourException(string message)
		: base(message)
	{
	}
}
