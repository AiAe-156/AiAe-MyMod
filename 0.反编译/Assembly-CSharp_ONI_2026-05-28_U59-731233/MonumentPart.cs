using System.Collections.Generic;
using System.Runtime.Serialization;
using Database;
using KSerialization;
using TUNING;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/MonumentPart")]
public class MonumentPart : KMonoBehaviour
{
	public MonumentPartResource.Part part;

	public string stateUISymbol;

	[Serialize]
	private string chosenState;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Components.MonumentParts.Add(this);
		if (!string.IsNullOrEmpty(chosenState))
		{
			SetState(chosenState);
		}
		UpdateMonumentDecor();
	}

	[OnDeserialized]
	private void OnDeserializedMethod()
	{
		MonumentPartResource monumentPartResource = Db.GetMonumentParts().TryGet(chosenState);
		if (monumentPartResource == null)
		{
			string id = "";
			if (part == MonumentPartResource.Part.Bottom)
			{
				id = "bottom_" + chosenState;
			}
			else if (part == MonumentPartResource.Part.Middle)
			{
				id = "mid_" + chosenState;
			}
			else if (part == MonumentPartResource.Part.Top)
			{
				id = "top_" + chosenState;
			}
			monumentPartResource = Db.GetMonumentParts().TryGet(id);
			if (monumentPartResource != null)
			{
				chosenState = id;
			}
		}
	}

	protected override void OnCleanUp()
	{
		Components.MonumentParts.Remove(this);
		RemoveMonumentPiece();
		base.OnCleanUp();
	}

	public void SetState(string state)
	{
		MonumentPartResource monumentPartResource = Db.GetMonumentParts().TryGet(state);
		if (monumentPartResource != null)
		{
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			component.SwapAnims(new KAnimFile[1] { monumentPartResource.AnimFile });
			component.Play(monumentPartResource.State);
			chosenState = state;
		}
	}

	public bool IsMonumentCompleted()
	{
		bool flag = GetMonumentPart(MonumentPartResource.Part.Top) != null;
		bool flag2 = GetMonumentPart(MonumentPartResource.Part.Middle) != null;
		bool flag3 = GetMonumentPart(MonumentPartResource.Part.Bottom) != null;
		return flag && flag3 && flag2;
	}

	public void UpdateMonumentDecor()
	{
		GameObject monumentPart = GetMonumentPart(MonumentPartResource.Part.Middle);
		if (!IsMonumentCompleted())
		{
			return;
		}
		DecorProvider component = monumentPart.GetComponent<DecorProvider>();
		component.SetValues(BUILDINGS.DECOR.BONUS.MONUMENT.COMPLETE);
		List<GameObject> attachedNetwork = AttachableBuilding.GetAttachedNetwork(GetComponent<AttachableBuilding>());
		foreach (GameObject item in attachedNetwork)
		{
			if (item != monumentPart)
			{
				DecorProvider component2 = item.GetComponent<DecorProvider>();
				component2.SetValues(BUILDINGS.DECOR.NONE);
			}
		}
	}

	public void RemoveMonumentPiece()
	{
		if (!IsMonumentCompleted())
		{
			return;
		}
		List<GameObject> attachedNetwork = AttachableBuilding.GetAttachedNetwork(GetComponent<AttachableBuilding>());
		foreach (GameObject item in attachedNetwork)
		{
			if (item.GetComponent<MonumentPart>() != this)
			{
				DecorProvider component = item.GetComponent<DecorProvider>();
				component.SetValues(BUILDINGS.DECOR.BONUS.MONUMENT.INCOMPLETE);
			}
		}
	}

	private GameObject GetMonumentPart(MonumentPartResource.Part requestPart)
	{
		List<GameObject> attachedNetwork = AttachableBuilding.GetAttachedNetwork(GetComponent<AttachableBuilding>());
		foreach (GameObject item in attachedNetwork)
		{
			MonumentPart component = item.GetComponent<MonumentPart>();
			if (component == null || component.part != requestPart)
			{
				continue;
			}
			return item;
		}
		return null;
	}
}
