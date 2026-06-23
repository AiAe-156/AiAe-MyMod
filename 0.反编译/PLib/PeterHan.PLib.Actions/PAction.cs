using System;

namespace PeterHan.PLib.Actions;

/// <summary>
/// An Action managed by PLib. Actions have key bindings assigned to them.
/// </summary>
public sealed class PAction
{
	/// <summary>
	/// The action's ID. This ID is assigned internally upon register and used for PLib
	/// indexing. Even if you somehow obtain it in your mod, it is not to be used!
	/// </summary>
	private readonly int id;

	/// <summary>
	/// The maximum action value (typically used to mean "no action") used in the currently
	/// running instance of the game.
	///
	/// Since Action is compiled to a const int when a mod is built, any changes to the
	/// Action enum will break direct references to Action.NumActions. Use this property
	/// instead to always use the intended "no action" value.
	/// </summary>
	public static Action MaxAction { get; }

	/// <summary>
	/// The default key binding for this action. Not necessarily the current key binding.
	/// </summary>
	internal PKeyBinding DefaultBinding { get; }

	/// <summary>
	/// The action's non-localized identifier. Something like YOURMOD.CATEGORY.ACTIONNAME.
	/// </summary>
	public string Identifier { get; }

	/// <summary>
	/// The action's title.
	/// </summary>
	public LocString Title { get; }

	static PAction()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!Enum.TryParse<Action>("NumActions", out Action result))
		{
			result = (Action)280;
		}
		MaxAction = result;
	}

	internal PAction(int id, string identifier, LocString title, PKeyBinding binding)
	{
		if (id <= 0)
		{
			throw new ArgumentOutOfRangeException("id");
		}
		DefaultBinding = binding;
		Identifier = identifier;
		this.id = id;
		Title = title;
	}

	public override bool Equals(object obj)
	{
		if (obj is PAction pAction)
		{
			return pAction.id == id;
		}
		return false;
	}

	/// <summary>
	/// Retrieves the Klei action for this PAction.
	/// </summary>
	/// <returns>The Klei action for use in game functions.</returns>
	public Action GetKAction()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return (Action)(MaxAction + id);
	}

	public override int GetHashCode()
	{
		return id;
	}

	public override string ToString()
	{
		return "PAction[" + Identifier + "]: " + LocString.op_Implicit(Title);
	}
}
