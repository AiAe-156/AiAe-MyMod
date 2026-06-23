using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class MinionStorageDataHolder : KMonoBehaviour, StoredMinionIdentity.IStoredMinionExtension
{
	[SerializationConfig(MemberSerialization.OptIn)]
	public class DataPackData
	{
		[Serialize]
		public bool[] Bools;

		[Serialize]
		public Tag[] Tags;
	}

	[SerializationConfig(MemberSerialization.OptIn)]
	public class DataPack
	{
		[Serialize]
		private string id;

		[Serialize]
		private bool isStoringNewData = false;

		[Serialize]
		private DataPackData data;

		public bool IsStoringNewData => isStoringNewData;

		public string ID => id;

		public DataPack(string id)
		{
			this.id = id;
		}

		public void SetData(DataPackData data, bool markAsNewDataToRead)
		{
			this.data = data;
			if (markAsNewDataToRead)
			{
				isStoringNewData = markAsNewDataToRead;
			}
		}

		public DataPackData ReadData()
		{
			isStoringNewData = false;
			return data;
		}

		public DataPackData PeekData()
		{
			return data;
		}
	}

	public Action<StoredMinionIdentity> OnCopyBegins = null;

	[Serialize]
	private List<DataPack> storedDataPacks;

	protected override void OnSpawn()
	{
		base.OnSpawn();
	}

	public DataPack Internal_GetDataPack(string ID)
	{
		if (storedDataPacks != null)
		{
			DataPack dataPack = storedDataPacks.Find((DataPack d) => d.ID == ID);
			if (dataPack != null)
			{
				return dataPack;
			}
		}
		return null;
	}

	public void Internal_UpdateData(string ID, DataPackData data)
	{
		SetData(ID, data, markAsNewDataToRead: false);
	}

	private void SetData(string ID, DataPackData data, bool markAsNewDataToRead)
	{
		if (storedDataPacks == null)
		{
			storedDataPacks = new List<DataPack>();
		}
		DataPack dataPack = storedDataPacks.Find((DataPack d) => d.ID == ID);
		if (dataPack == null)
		{
			dataPack = new DataPack(ID);
			storedDataPacks.Add(dataPack);
		}
		dataPack.SetData(data, markAsNewDataToRead);
	}

	public void PullFrom(StoredMinionIdentity source)
	{
		MinionStorageDataHolder component = source.GetComponent<MinionStorageDataHolder>();
		if (!(component != null) || component.storedDataPacks == null)
		{
			return;
		}
		for (int i = 0; i < component.storedDataPacks.Count; i++)
		{
			DataPack dataPack = component.storedDataPacks[i];
			if (dataPack != null)
			{
				SetData(dataPack.ID, dataPack.ReadData(), markAsNewDataToRead: true);
			}
		}
	}

	public void PushTo(StoredMinionIdentity destination)
	{
		OnCopyBegins?.Invoke(destination);
		AddStoredMinionGameObjectRequirements(destination.gameObject);
		MinionStorageDataHolder component = destination.gameObject.GetComponent<MinionStorageDataHolder>();
		if (storedDataPacks == null)
		{
			return;
		}
		for (int i = 0; i < storedDataPacks.Count; i++)
		{
			DataPack dataPack = storedDataPacks[i];
			if (dataPack != null)
			{
				component.SetData(dataPack.ID, dataPack.ReadData(), markAsNewDataToRead: true);
			}
		}
	}

	public void AddStoredMinionGameObjectRequirements(GameObject storedMinionGameObject)
	{
		storedMinionGameObject.AddOrGet<MinionStorageDataHolder>();
	}
}
