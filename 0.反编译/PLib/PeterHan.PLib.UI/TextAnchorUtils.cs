using UnityEngine;

namespace PeterHan.PLib.UI;

/// <summary>
/// Extension methods to deal with TextAnchor alignments.
/// </summary>
public static class TextAnchorUtils
{
	/// <summary>
	/// Returns true if this text alignment is at the left.
	/// </summary>
	/// <param name="anchor">The anchoring to check.</param>
	/// <returns>true if it denotes a Left alignment, or false otherwise.</returns>
	public static bool IsLeft(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		if ((int)anchor != 0 && (int)anchor != 3)
		{
			return (int)anchor == 6;
		}
		return true;
	}

	/// <summary>
	/// Returns true if this text alignment is at the bottom.
	/// </summary>
	/// <param name="anchor">The anchoring to check.</param>
	/// <returns>true if it denotes a Lower alignment, or false otherwise.</returns>
	public static bool IsLower(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		if ((int)anchor != 7 && (int)anchor != 6)
		{
			return (int)anchor == 8;
		}
		return true;
	}

	/// <summary>
	/// Returns true if this text alignment is at the right.
	/// </summary>
	/// <param name="anchor">The anchoring to check.</param>
	/// <returns>true if it denotes a Right alignment, or false otherwise.</returns>
	public static bool IsRight(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		if ((int)anchor != 2 && (int)anchor != 5)
		{
			return (int)anchor == 8;
		}
		return true;
	}

	/// <summary>
	/// Returns true if this text alignment is at the top.
	/// </summary>
	/// <param name="anchor">The anchoring to check.</param>
	/// <returns>true if it denotes an Upper alignment, or false otherwise.</returns>
	public static bool IsUpper(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		if ((int)anchor != 1 && (int)anchor != 0)
		{
			return (int)anchor == 2;
		}
		return true;
	}

	/// <summary>
	/// Mirrors a text alignment horizontally. UpperLeft becomes UpperRight, MiddleLeft
	/// becomes MiddleRight, and so forth.
	/// </summary>
	/// <param name="anchor">The anchoring to mirror.</param>
	/// <returns>The horizontally reflected version of that mirror.</returns>
	public static TextAnchor MirrorHorizontal(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		int num = (int)anchor;
		num = 3 * (num / 3) + 2 - num % 3;
		return (TextAnchor)num;
	}

	/// <summary>
	/// Mirrors a text alignment vertically. UpperLeft becomes LowerLeft, LowerCenter
	/// becomes UpperCenter, and so forth.
	/// </summary>
	/// <param name="anchor">The anchoring to mirror.</param>
	/// <returns>The vertically reflected version of that mirror.</returns>
	public static TextAnchor MirrorVertical(this TextAnchor anchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		int num = (int)anchor;
		num = 6 - 3 * (num / 3) + num % 3;
		return (TextAnchor)num;
	}
}
