using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/DetailLabelWithButton")]
public class DetailLabelWithButton : KMonoBehaviour
{
	public LocText label;

	public LocText label2;

	public LocText label3;

	public ToolTip toolTip;

	public KButton button;

	public void RefreshLabelsVisibility()
	{
		if (label.gameObject.activeInHierarchy != !string.IsNullOrEmpty(label.text))
		{
			label.gameObject.SetActive(!string.IsNullOrEmpty(label.text));
		}
		if (label2.gameObject.activeInHierarchy != !string.IsNullOrEmpty(label2.text))
		{
			label2.gameObject.SetActive(!string.IsNullOrEmpty(label2.text));
		}
		if (label3.gameObject.activeInHierarchy != !string.IsNullOrEmpty(label3.text))
		{
			label3.gameObject.SetActive(!string.IsNullOrEmpty(label3.text));
		}
	}
}
