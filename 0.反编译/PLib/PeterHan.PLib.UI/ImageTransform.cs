using System;

namespace PeterHan.PLib.UI;

/// <summary>
/// An enumeration describing how to transform the image in a label.
///
/// Rotations are counterclockwise from 0 (straight up).
/// </summary>
[Flags]
public enum ImageTransform : uint
{
	None = 0u,
	FlipHorizontal = 1u,
	FlipVertical = 2u,
	Rotate90 = 4u,
	Rotate180 = 8u,
	Rotate270 = 0xCu
}
