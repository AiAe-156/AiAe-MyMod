using System;

namespace FUtility.FLocalization;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class NoteAttribute : Attribute
{
	public string message;

	public NoteAttribute(string message)
	{
		this.message = message;
	}
}
