using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/SingleEntityReceptacle")]
public class SingleEntityReceptacle : Workable, IRender1000ms
{
	public struct CustomPositionData
	{
		public Tag tag;

		public Vector3 pos;

		public CustomPositionData(Tag t, Vector3 p)
		{
			tag = t;
			pos = p;
		}
	}

	public enum ReceptacleDirection
	{
		Top,
		Side,
		Bottom,
		Any
	}

	[MyCmpGet]
	public Operational operational;

	[MyCmpReq]
	protected Storage storage;

	[MyCmpGet]
	public Rotatable rotatable;

	protected FetchChore fetchChore;

	public ChoreType choreType = Db.Get().ChoreTypes.Fetch;

	[Serialize]
	public bool autoReplaceEntity;

	[Serialize]
	public Tag requestedEntityTag;

	[Serialize]
	public Tag requestedEntityAdditionalFilterTag;

	[Serialize]
	protected Ref<KSelectable> occupyObjectRef = new Ref<KSelectable>();

	[SerializeField]
	private List<Tag> possibleDepositTagsList = new List<Tag>();

	[SerializeField]
	private List<Func<GameObject, bool>> additionalCriteria = new List<Func<GameObject, bool>>();

	[SerializeField]
	protected bool destroyEntityOnDeposit = false;

	[SerializeField]
	protected ReceptacleDirection direction;

	public static Dictionary<Tag, List<CustomPositionData>> CustomOccupyingObjectRelativePosition = new Dictionary<Tag, List<CustomPositionData>>();

	public Vector3 occupyingObjectRelativePosition = new Vector3(0f, 1f, 3f);

	private Tag selfPrefabID;

	protected StatusItem statusItemAwaitingDelivery;

	protected StatusItem statusItemNeed;

	protected StatusItem statusItemNoneAvailable;

	private static readonly EventSystem.IntraObjectHandler<SingleEntityReceptacle> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<SingleEntityReceptacle>(delegate(SingleEntityReceptacle component, object data)
	{
		component.OnOperationalChanged(data);
	});

	public FetchChore GetActiveRequest => fetchChore;

	protected GameObject occupyingObject
	{
		get
		{
			if (occupyObjectRef.Get() != null)
			{
				return occupyObjectRef.Get().gameObject;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				occupyObjectRef.Set(null);
			}
			else
			{
				occupyObjectRef.Set(value.GetComponent<KSelectable>());
			}
		}
	}

	public GameObject Occupant => occupyingObject;

	public IReadOnlyList<Tag> possibleDepositObjectTags => possibleDepositTagsList;

	public ReceptacleDirection Direction => direction;

	public bool HasDepositTag(Tag tag)
	{
		return possibleDepositTagsList.Contains(tag);
	}

	public bool IsValidEntity(GameObject candidate)
	{
		KPrefabID component = candidate.GetComponent<KPrefabID>();
		if (!Game.IsCorrectDlcActiveForCurrentSave(component))
		{
			return false;
		}
		IReceptacleDirection component2 = candidate.GetComponent<IReceptacleDirection>();
		bool flag = Direction == ReceptacleDirection.Any || rotatable != null || component2 == null || component2.Direction == Direction;
		int num = 0;
		while (flag && num < additionalCriteria.Count)
		{
			flag = additionalCriteria[num](candidate);
			num++;
		}
		return flag;
	}

	protected Vector3 GetOccupyingObjectRelativePosition()
	{
		if (Occupant != null && CustomOccupyingObjectRelativePosition.ContainsKey(selfPrefabID))
		{
			KPrefabID component = Occupant.GetComponent<KPrefabID>();
			List<CustomPositionData> list = CustomOccupyingObjectRelativePosition[selfPrefabID];
			foreach (CustomPositionData item in list)
			{
				if (component.HasTag(item.tag))
				{
					return item.pos;
				}
			}
		}
		return occupyingObjectRelativePosition;
	}

	protected override void OnPrefabInit()
	{
		selfPrefabID = base.gameObject.PrefabID();
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (occupyingObject != null)
		{
			PositionOccupyingObject();
			SubscribeToOccupant();
		}
		UpdateStatusItem();
		if (occupyingObject == null && !requestedEntityTag.IsValid)
		{
			requestedEntityAdditionalFilterTag = null;
		}
		if (occupyingObject == null && requestedEntityTag.IsValid)
		{
			CreateOrder(requestedEntityTag, requestedEntityAdditionalFilterTag);
		}
		Subscribe(-592767678, OnOperationalChangedDelegate);
		TriggerReceptacleOperationalSignal();
	}

	public void AddDepositTag(Tag t)
	{
		possibleDepositTagsList.Add(t);
	}

	public void AddAdditionalCriteria(Func<GameObject, bool> criteria)
	{
		additionalCriteria.Add(criteria);
	}

	public void SetReceptacleDirection(ReceptacleDirection d)
	{
		direction = d;
	}

	public virtual void SetPreview(Tag entityTag, bool solid = false)
	{
	}

	public virtual void CreateOrder(Tag entityTag, Tag additionalFilterTag)
	{
		requestedEntityTag = entityTag;
		requestedEntityAdditionalFilterTag = additionalFilterTag;
		CreateFetchChore(requestedEntityTag, requestedEntityAdditionalFilterTag);
		SetPreview(entityTag, solid: true);
		UpdateStatusItem();
	}

	public virtual void Render1000ms(float dt)
	{
		UpdateStatusItem();
	}

	protected void UpdateStatusItem()
	{
		UpdateStatusItem(GetComponent<KSelectable>());
	}

	protected virtual void UpdateStatusItem(KSelectable selectable)
	{
		if (Occupant != null)
		{
			selectable.SetStatusItem(Db.Get().StatusItemCategories.EntityReceptacle, null);
		}
		else if (fetchChore != null)
		{
			bool flag = fetchChore.fetcher != null;
			WorldContainer myWorld = this.GetMyWorld();
			if (!flag && myWorld != null)
			{
				foreach (Tag tag in fetchChore.tags)
				{
					if (myWorld.worldInventory.GetTotalAmount(tag, includeRelatedWorlds: true) > 0f)
					{
						if (myWorld.worldInventory.GetTotalAmount(requestedEntityAdditionalFilterTag, includeRelatedWorlds: true) > 0f || requestedEntityAdditionalFilterTag == Tag.Invalid)
						{
							flag = true;
						}
						break;
					}
				}
			}
			if (flag)
			{
				selectable.SetStatusItem(Db.Get().StatusItemCategories.EntityReceptacle, statusItemAwaitingDelivery);
			}
			else
			{
				selectable.SetStatusItem(Db.Get().StatusItemCategories.EntityReceptacle, statusItemNoneAvailable);
			}
		}
		else
		{
			selectable.SetStatusItem(Db.Get().StatusItemCategories.EntityReceptacle, statusItemNeed);
		}
	}

	protected void CreateFetchChore(Tag entityTag, Tag additionalRequiredTag)
	{
		if (fetchChore == null && entityTag.IsValid && entityTag != GameTags.Empty)
		{
			fetchChore = new FetchChore(choreType, storage, GetPrefabFetchMass(entityTag), new HashSet<Tag> { entityTag }, FetchChore.MatchCriteria.MatchID, (additionalRequiredTag.IsValid && additionalRequiredTag != GameTags.Empty) ? additionalRequiredTag : Tag.Invalid, null, null, run_until_complete: true, OnFetchComplete, delegate
			{
				UpdateStatusItem();
			}, delegate
			{
				UpdateStatusItem();
			}, Operational.State.Functional);
			MaterialNeeds.UpdateNeed(requestedEntityTag, 1f, base.gameObject.GetMyWorldId());
			UpdateStatusItem();
		}
	}

	private float GetPrefabFetchMass(Tag entityTag)
	{
		GameObject prefab = Assets.GetPrefab(entityTag);
		if (prefab != null)
		{
			PrimaryElement component = prefab.GetComponent<PrimaryElement>();
			if (component != null)
			{
				return component.MassPerUnit;
			}
		}
		KCrashReporter.ReportDevNotification("SingleEntityReceptacle " + base.name + " is requesting " + entityTag.Name + " which is not an entity", Environment.StackTrace);
		return 1f;
	}

	public virtual void OrderRemoveOccupant()
	{
		ClearOccupant();
	}

	protected virtual void ClearOccupant()
	{
		if ((bool)occupyingObject)
		{
			UnsubscribeFromOccupant();
			storage.DropAll();
		}
		occupyingObject = null;
		UpdateActive();
		UpdateStatusItem();
		Trigger(-731304873, (object)occupyingObject);
	}

	public void CancelActiveRequest()
	{
		if (fetchChore != null)
		{
			MaterialNeeds.UpdateNeed(requestedEntityTag, -1f, base.gameObject.GetMyWorldId());
			fetchChore.Cancel("User canceled");
			fetchChore = null;
		}
		requestedEntityTag = Tag.Invalid;
		requestedEntityAdditionalFilterTag = Tag.Invalid;
		UpdateStatusItem();
		SetPreview(Tag.Invalid);
	}

	private void OnOccupantDestroyed(object data)
	{
		occupyingObject = null;
		ClearOccupant();
		if (autoReplaceEntity && requestedEntityTag.IsValid && requestedEntityTag != GameTags.Empty)
		{
			CreateOrder(requestedEntityTag, requestedEntityAdditionalFilterTag);
		}
	}

	protected virtual void SubscribeToOccupant()
	{
		if (occupyingObject != null)
		{
			Subscribe(occupyingObject, 1969584890, OnOccupantDestroyed);
		}
	}

	protected virtual void UnsubscribeFromOccupant()
	{
		if (occupyingObject != null)
		{
			Unsubscribe(occupyingObject, 1969584890, OnOccupantDestroyed);
		}
	}

	private void OnFetchComplete(Chore chore)
	{
		if (fetchChore == null)
		{
			Debug.LogWarningFormat(base.gameObject, "{0} OnFetchComplete fetchChore null", base.gameObject);
		}
		else if (fetchChore.fetchTarget == null)
		{
			Debug.LogWarningFormat(base.gameObject, "{0} OnFetchComplete fetchChore.fetchTarget null", base.gameObject);
		}
		else
		{
			OnDepositObject(fetchChore.fetchTarget.gameObject);
		}
	}

	public void ForceDeposit(GameObject depositedObject)
	{
		if (occupyingObject != null)
		{
			ClearOccupant();
		}
		OnDepositObject(depositedObject);
	}

	protected virtual void OnDepositObject(GameObject depositedObject)
	{
		SetPreview(Tag.Invalid);
		MaterialNeeds.UpdateNeed(requestedEntityTag, -1f, base.gameObject.GetMyWorldId());
		KBatchedAnimController component = depositedObject.GetComponent<KBatchedAnimController>();
		if (component != null)
		{
			component.GetBatchInstanceData().ClearOverrideTransformMatrix();
		}
		occupyingObject = SpawnOccupyingObject(depositedObject);
		if (occupyingObject != null)
		{
			ConfigureOccupyingObject(occupyingObject);
			occupyingObject.SetActive(value: true);
			PositionOccupyingObject();
			SubscribeToOccupant();
		}
		else
		{
			Debug.LogWarning(base.gameObject.name + " EntityReceptacle did not spawn occupying entity.");
		}
		if (fetchChore != null)
		{
			fetchChore.Cancel("receptacle filled");
			fetchChore = null;
		}
		if (!autoReplaceEntity)
		{
			requestedEntityTag = Tag.Invalid;
			requestedEntityAdditionalFilterTag = Tag.Invalid;
		}
		UpdateActive();
		UpdateStatusItem();
		if (destroyEntityOnDeposit)
		{
			Util.KDestroyGameObject(depositedObject);
		}
		Trigger(-731304873, (object)occupyingObject);
	}

	protected virtual GameObject SpawnOccupyingObject(GameObject depositedEntity)
	{
		return depositedEntity;
	}

	protected virtual void ConfigureOccupyingObject(GameObject source)
	{
	}

	protected virtual void PositionOccupyingObject()
	{
		Vector3 vector = GetOccupyingObjectRelativePosition();
		if (rotatable != null)
		{
			occupyingObject.transform.SetPosition(base.gameObject.transform.GetPosition() + rotatable.GetRotatedOffset(vector));
		}
		else
		{
			occupyingObject.transform.SetPosition(base.gameObject.transform.GetPosition() + vector);
		}
		KBatchedAnimController component = occupyingObject.GetComponent<KBatchedAnimController>();
		component.enabled = false;
		component.enabled = true;
	}

	protected void UpdateActive()
	{
		if (!Equals(null) && !(this == null) && !base.gameObject.Equals(null) && !(base.gameObject == null) && operational != null)
		{
			operational.SetActive(operational.IsOperational && occupyingObject != null);
		}
	}

	protected override void OnCleanUp()
	{
		CancelActiveRequest();
		UnsubscribeFromOccupant();
		base.OnCleanUp();
	}

	protected virtual void OnOperationalChanged(object data)
	{
		UpdateActive();
		TriggerReceptacleOperationalSignal();
	}

	private void TriggerReceptacleOperationalSignal()
	{
		if (!(operational == null) && (bool)occupyingObject)
		{
			occupyingObject.Trigger(operational.IsOperational ? 1628751838 : 960378201);
		}
	}
}
