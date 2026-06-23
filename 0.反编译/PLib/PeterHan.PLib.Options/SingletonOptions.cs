namespace PeterHan.PLib.Options;

/// <summary>
/// A class which can be used by mods to maintain a singleton of their options. This
/// class should be the superclass of the mod options class, and &lt;T&gt; should be
/// the type of the options class to store.
///
/// This class only initializes the mod options once by default. If the settings can
/// be updated without restarting the game, update the Instance manually using
/// IOptions.OnOptionsChanged. If the game has to be restarted anyways, add
/// [RestartRequired].
/// </summary>
/// <typeparam name="T">The mod options class to wrap.</typeparam>
public abstract class SingletonOptions<T> where T : class, new()
{
	/// <summary>
	/// The only instance of the singleton options.
	/// </summary>
	protected static T instance;

	/// <summary>
	/// Retrieves the program options, or lazily initializes them if not yet loaded.
	/// </summary>
	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = POptions.ReadSettings<T>() ?? new T();
			}
			return instance;
		}
		protected set
		{
			if (value != null)
			{
				instance = value;
			}
		}
	}
}
