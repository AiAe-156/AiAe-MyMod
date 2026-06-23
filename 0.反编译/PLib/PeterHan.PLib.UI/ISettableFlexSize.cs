using UnityEngine.UI;

namespace PeterHan.PLib.UI;

/// <summary>
/// Describes a UI component whose flexible size can be mutated.
/// </summary>
internal interface ISettableFlexSize : ILayoutGroup, ILayoutController
{
	/// <summary>
	/// The flexible width of the completed layout group can be set.
	/// </summary>
	float flexibleWidth { get; set; }

	/// <summary>
	/// The flexible height of the completed layout group can be set.
	/// </summary>
	float flexibleHeight { get; set; }
}
