namespace PeterHan.PLib.Options;

/// <summary>
/// The common parent of all classes that can specify the user visible attributes of an
/// option.
/// </summary>
public interface IOptionSpec
{
	/// <summary>
	/// The option category. Ignored and replaced with the parent option's category if
	/// this option is part of a custom grouped type.
	/// </summary>
	string Category { get; }

	/// <summary>
	/// The format string to use when displaying this option value. Only applicable for
	/// some types of options.
	///
	/// <b>Warning</b>: Attribute may have issues on nested classes that are used as custom
	/// grouped options. To mitigate, try declaring the custom class in a non-nested
	/// context (i.e. not declared inside another class).
	/// </summary>
	string Format { get; }

	/// <summary>
	/// The option title. Ignored for fields which are displayed as custom grouped types
	/// types of other options.
	/// </summary>
	string Title { get; }

	/// <summary>
	/// The option description tooltip. Ignored for fields which are displayed as custom
	/// grouped types of other options.
	/// </summary>
	string Tooltip { get; }
}
