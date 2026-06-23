using System;

namespace PeterHan.PLib.Options;

/// <summary>
/// An attribute placed on an options class only (will not function on a member property)
/// which denotes the config file name to use for that mod, and allows save/load options
/// to be set.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ConfigFileAttribute : Attribute
{
	/// <summary>
	/// The configuration file name. If null, the default file name will be used.
	/// </summary>
	public string ConfigFileName { get; }

	/// <summary>
	/// Whether the output should be indented nicely. Defaults to false for smaller
	/// config files.
	/// </summary>
	public bool IndentOutput { get; }

	/// <summary>
	/// If true, the config file will be moved from the mod folder to a folder in the
	/// config directory shared across mods. This change preserves the mod configuration
	/// across updates, but may not be cleared when the mod is uninstalled. Use with
	/// caution.
	/// </summary>
	public bool UseSharedConfigLocation { get; }

	public ConfigFileAttribute(string FileName = "config.json", bool IndentOutput = false, bool SharedConfigLocation = false)
	{
		ConfigFileName = FileName;
		this.IndentOutput = IndentOutput;
		UseSharedConfigLocation = SharedConfigLocation;
	}

	public override string ToString()
	{
		return ConfigFileName;
	}
}
