using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TrueTiles.Settings.Unity_UI_Extensions.Scripts.Controls.ReorderableList;

public class ReorderableListDebug : MonoBehaviour
{
	public Text DebugLabel;

	private void Awake()
	{
		ReorderableList[] array = Object.FindObjectsOfType<ReorderableList>();
		for (int i = 0; i < array.Length; i++)
		{
			((UnityEvent<ReorderableList.ReorderableListEventStruct>)array[i].OnElementDropped).AddListener((UnityAction<ReorderableList.ReorderableListEventStruct>)ElementDropped);
		}
	}

	private void ElementDropped(ReorderableList.ReorderableListEventStruct droppedStruct)
	{
		DebugLabel.text = "";
		Text debugLabel = DebugLabel;
		debugLabel.text = debugLabel.text + "Dropped Object: " + ((Object)droppedStruct.DroppedObject).name + "\n";
		Text debugLabel2 = DebugLabel;
		debugLabel2.text = debugLabel2.text + "Is Clone ?: " + droppedStruct.IsAClone + "\n";
		if (droppedStruct.IsAClone)
		{
			Text debugLabel3 = DebugLabel;
			debugLabel3.text = debugLabel3.text + "Source Object: " + ((Object)droppedStruct.SourceObject).name + "\n";
		}
		Text debugLabel4 = DebugLabel;
		debugLabel4.text += $"From {((Object)droppedStruct.FromList).name} at Index {droppedStruct.FromIndex} \n";
		Text debugLabel5 = DebugLabel;
		debugLabel5.text += string.Format("To {0} at Index {1} \n", ((Object)(object)droppedStruct.ToList == (Object)null) ? "Empty space" : ((Object)droppedStruct.ToList).name, droppedStruct.ToIndex);
	}
}
