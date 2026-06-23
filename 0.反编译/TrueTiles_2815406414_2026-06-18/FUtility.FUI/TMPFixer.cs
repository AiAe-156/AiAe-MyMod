using TMPro;
using UnityEngine;

namespace FUtility.FUI;

internal class TMPFixer : KMonoBehaviour
{
	[SerializeField]
	public TextAlignmentOptions alignment;

	[MyCmpReq]
	private LocText text;

	protected override void OnSpawn()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		((KMonoBehaviour)this).OnSpawn();
		((TMP_Text)text).alignment = alignment;
		Object.Destroy((Object)(object)this);
	}
}
