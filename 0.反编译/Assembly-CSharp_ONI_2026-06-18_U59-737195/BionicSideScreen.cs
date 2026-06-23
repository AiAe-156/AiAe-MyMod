using System;
using System.Collections.Generic;
using UnityEngine;

public class BionicSideScreen : SideScreenContent
{
	public OwnablesSecondSideScreen ownableSecondSideScreenPrefab;

	public BionicSideScreenUpgradeSlot originalBionicSlot;

	private BionicUpgradesMonitor.Instance upgradeMonitor;

	private BionicBatteryMonitor.Instance batteryMonitor;

	private BionicBedTimeMonitor.Instance bedTimeMonitor;

	private List<BionicSideScreenUpgradeSlot> bionicSlots = new List<BionicSideScreenUpgradeSlot>();

	private OwnablesSidescreen ownableSidescreen;

	private AssignableSlotInstance lastSlotSelected;

	private int onBionicBecameOnlineHandle = -1;

	private int onBionicBecameOfflineHandle = -1;

	private int onBionicUpgradeChangedHandle = -1;

	private int onBionicUpgradeComponentSlotCountChangedHandle = -1;

	private int onBionicWattageChangedHandle = -1;

	private int onBionicTagsChangedHandle = -1;

	private void OnBionicUpgradeSlotClicked(BionicSideScreenUpgradeSlot slotClicked)
	{
		bool flag = slotClicked == null || lastSlotSelected == slotClicked.upgradeSlot.GetAssignableSlotInstance();
		bool flag2 = !flag && slotClicked.upgradeSlot.IsLocked;
		lastSlotSelected = (flag ? null : slotClicked.upgradeSlot.GetAssignableSlotInstance());
		RefreshSelectedStateInSlots();
		AssignableSlot bionicUpgrade = Db.Get().AssignableSlots.BionicUpgrade;
		AssignableSlotInstance assignableSlotInstance = ((flag || flag2) ? null : slotClicked.upgradeSlot.GetAssignableSlotInstance());
		if (ownableSidescreen != null)
		{
			ownableSidescreen.SetSelectedSlot(assignableSlotInstance);
		}
		else if (flag || flag2)
		{
			DetailsScreen.Instance.ClearSecondarySideScreen();
		}
		else
		{
			((OwnablesSecondSideScreen)DetailsScreen.Instance.SetSecondarySideScreen(ownableSecondSideScreenPrefab, bionicUpgrade.Name)).SetSlot(assignableSlotInstance);
		}
	}

	private void RefreshSelectedStateInSlots()
	{
		for (int i = 0; i < bionicSlots.Count; i++)
		{
			BionicSideScreenUpgradeSlot bionicSideScreenUpgradeSlot = bionicSlots[i];
			bionicSideScreenUpgradeSlot.SetSelected(bionicSideScreenUpgradeSlot.upgradeSlot.GetAssignableSlotInstance() == lastSlotSelected);
		}
	}

	public void RecreateBionicSlots()
	{
		int num = ((upgradeMonitor != null) ? upgradeMonitor.upgradeComponentSlots.Length : 0);
		for (int i = 0; i < Mathf.Max(num, bionicSlots.Count); i++)
		{
			if (i >= bionicSlots.Count)
			{
				BionicSideScreenUpgradeSlot item = CreateBionicSlot();
				bionicSlots.Add(item);
			}
			BionicSideScreenUpgradeSlot bionicSideScreenUpgradeSlot = bionicSlots[i];
			if (i < num)
			{
				BionicUpgradesMonitor.UpgradeComponentSlot upgradeSlot = upgradeMonitor.upgradeComponentSlots[i];
				bionicSideScreenUpgradeSlot.gameObject.SetActive(value: true);
				bionicSideScreenUpgradeSlot.Setup(upgradeSlot);
				bionicSideScreenUpgradeSlot.SetSelected(bionicSideScreenUpgradeSlot.upgradeSlot.GetAssignableSlotInstance() == lastSlotSelected);
			}
			else
			{
				bionicSideScreenUpgradeSlot.Setup(null);
				bionicSideScreenUpgradeSlot.gameObject.SetActive(value: false);
			}
		}
	}

	private BionicSideScreenUpgradeSlot CreateBionicSlot()
	{
		BionicSideScreenUpgradeSlot bionicSideScreenUpgradeSlot = Util.KInstantiateUI<BionicSideScreenUpgradeSlot>(originalBionicSlot.gameObject, originalBionicSlot.transform.parent.gameObject);
		bionicSideScreenUpgradeSlot.OnClick = (Action<BionicSideScreenUpgradeSlot>)Delegate.Combine(bionicSideScreenUpgradeSlot.OnClick, new Action<BionicSideScreenUpgradeSlot>(OnBionicUpgradeSlotClicked));
		return bionicSideScreenUpgradeSlot;
	}

	private void OnBionicBecameOnline(object o)
	{
		RefreshSlots();
	}

	private void OnBionicBecameOffline(object o)
	{
		RefreshSlots();
	}

	private void OnBionicWattageChanged(object o)
	{
		RefreshSlots();
	}

	private void OnBionicBedTimeChoreStateChanged(object o)
	{
		RefreshSlots();
	}

	private void OnBionicUpgradeComponentSlotCountChanged(object o)
	{
		RefreshSlots();
	}

	private void OnBionicUpgradeChanged(object o)
	{
		RecreateBionicSlots();
	}

	private void OnBionicTagsChanged(object o)
	{
		if (o != null && ((Boxed<TagChangedEventData>)o).value.tag == GameTags.BionicBedTime)
		{
			OnBionicBedTimeChoreStateChanged(o);
		}
	}

	private void RefreshSlots()
	{
		for (int num = bionicSlots.Count - 1; num >= 0; num--)
		{
			BionicSideScreenUpgradeSlot bionicSideScreenUpgradeSlot = bionicSlots[num];
			if (bionicSideScreenUpgradeSlot != null)
			{
				bionicSideScreenUpgradeSlot.Refresh();
				bionicSideScreenUpgradeSlot.gameObject.transform.SetAsFirstSibling();
			}
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		originalBionicSlot.gameObject.SetActive(value: false);
		ownableSidescreen = base.transform.parent.GetComponentInChildren<OwnablesSidescreen>();
		if (ownableSidescreen != null)
		{
			OwnablesSidescreen ownablesSidescreen = ownableSidescreen;
			ownablesSidescreen.OnSlotInstanceSelected = (Action<AssignableSlotInstance>)Delegate.Combine(ownablesSidescreen.OnSlotInstanceSelected, new Action<AssignableSlotInstance>(OnOwnableSidescreenRowSelected));
		}
	}

	private void OnOwnableSidescreenRowSelected(AssignableSlotInstance slot)
	{
		lastSlotSelected = slot;
		RefreshSelectedStateInSlots();
	}

	public override void SetTarget(GameObject target)
	{
		base.SetTarget(target);
		lastSlotSelected = null;
		if (upgradeMonitor != null)
		{
			upgradeMonitor.Unsubscribe(ref onBionicBecameOnlineHandle);
			upgradeMonitor.Unsubscribe(ref onBionicBecameOfflineHandle);
			upgradeMonitor.Unsubscribe(ref onBionicUpgradeChangedHandle);
			upgradeMonitor.Unsubscribe(ref onBionicUpgradeComponentSlotCountChangedHandle);
		}
		if (batteryMonitor != null)
		{
			batteryMonitor.Unsubscribe(ref onBionicWattageChangedHandle);
		}
		if (bedTimeMonitor != null)
		{
			bedTimeMonitor.Unsubscribe(ref onBionicTagsChangedHandle);
		}
		batteryMonitor = target.GetSMI<BionicBatteryMonitor.Instance>();
		upgradeMonitor = target.GetSMI<BionicUpgradesMonitor.Instance>();
		bedTimeMonitor = target.GetSMI<BionicBedTimeMonitor.Instance>();
		onBionicBecameOnlineHandle = upgradeMonitor.Subscribe(160824499, OnBionicBecameOnline);
		onBionicBecameOfflineHandle = upgradeMonitor.Subscribe(-1730800797, OnBionicBecameOffline);
		onBionicUpgradeChangedHandle = upgradeMonitor.Subscribe(2000325176, OnBionicUpgradeChanged);
		onBionicUpgradeComponentSlotCountChangedHandle = batteryMonitor.Subscribe(1095596132, OnBionicUpgradeComponentSlotCountChanged);
		onBionicWattageChangedHandle = batteryMonitor.Subscribe(1361471071, OnBionicWattageChanged);
		onBionicTagsChangedHandle = bedTimeMonitor.Subscribe(-1582839653, OnBionicTagsChanged);
		RecreateBionicSlots();
		RefreshSlots();
	}

	protected override void OnShow(bool show)
	{
		base.OnShow(show);
		if (show)
		{
			RefreshSlots();
		}
	}

	public override void ClearTarget()
	{
		base.ClearTarget();
		if (upgradeMonitor != null)
		{
			upgradeMonitor.Unsubscribe(ref onBionicUpgradeChangedHandle);
		}
		bedTimeMonitor = null;
		upgradeMonitor = null;
		lastSlotSelected = null;
	}

	public override bool IsValidForTarget(GameObject target)
	{
		return target.GetSMI<BionicBatteryMonitor.Instance>() != null;
	}

	public override int GetSideScreenSortOrder()
	{
		return 300;
	}
}
