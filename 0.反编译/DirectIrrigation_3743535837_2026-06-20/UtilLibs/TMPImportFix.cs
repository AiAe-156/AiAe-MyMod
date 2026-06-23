using TMPro;
using UnityEngine;

namespace UtilLibs;

internal class TMPImportFix : KMonoBehaviour
{
	[SerializeField]
	public TextOverflowModes textOverflow;

	[SerializeField]
	public TextAlignmentOptions alignment;

	[SerializeField]
	public float fontSizeMin;

	[SerializeField]
	public float fontSizeMax;

	[SerializeField]
	public bool autoResize;

	[MyCmpReq]
	private LocText text;

	public override void OnSpawn()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		((KMonoBehaviour)this).OnSpawn();
		((TMP_Text)text).alignment = alignment;
		((TMP_Text)text).overflowMode = textOverflow;
		((TMP_Text)text).fontSizeMax = fontSizeMax;
		((TMP_Text)text).fontSizeMin = fontSizeMin;
		((TMP_Text)text).enableAutoSizing = autoResize;
		Object.Destroy((Object)(object)this);
	}
}
