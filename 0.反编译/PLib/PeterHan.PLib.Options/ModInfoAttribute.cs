using System;

namespace PeterHan.PLib.Options;

/// <summary>
/// Allows mod authors to specify attributes for their mods to be shown in the Options
/// dialog.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ModInfoAttribute : Attribute
{
	/// <summary>
	/// If true, forces all categories in the options screen to begin collapsed (except
	/// the default category).
	/// </summary>
	public bool ForceCollapseCategories { get; }

	/// <summary>
	/// The name of the image file (in the mod's root directory) to display in the options
	/// dialog. If null or empty (or it cannot be loaded), no image is displayed.
	/// </summary>
	public string Image { get; }

	/// <summary>
	/// The URL to use for the mod. If null or empty, the Steam workshop link will be used
	/// if possible, or otherwise the button will not be shown.
	/// </summary>
	public string URL { get; }

	public ModInfoAttribute(string url, string image = null, bool collapse = false)
	{
		ForceCollapseCategories = collapse;
		Image = image;
		URL = url;
	}

	public override string ToString()
	{
		return string.Format("ModInfoAttribute[URL={1},Image={2}]", URL, Image);
	}
}
