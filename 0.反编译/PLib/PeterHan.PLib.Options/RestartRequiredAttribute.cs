using System;

namespace PeterHan.PLib.Options;

/// <summary>
/// An empty marker attribute. If applied to an options class, PLib will notify the user
/// that the game must be restarted to apply the options.
///
/// This attribute now works if it is applied to an individual option; PLib will attempt
/// to compare the old and new value using the Equals method and only prompt if the value
/// appears to be different. If this attribute is applied to a custom type (especially a
/// value type), Equals and preferably all of IEquatable must be implemented for it to work
/// properly.
///
/// Custom dynamic option handlers also need to handle this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class RestartRequiredAttribute : Attribute
{
}
