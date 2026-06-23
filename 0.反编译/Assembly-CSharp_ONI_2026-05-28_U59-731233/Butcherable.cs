using System.Collections.Generic;
using STRINGS;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/Butcherable")]
public class Butcherable : Workable, ISaveLoadable
{
	[MyCmpGet]
	private KAnimControllerBase controller;

	[MyCmpGet]
	private Harvestable harvestable;

	private bool readyToButcher = false;

	private bool butchered = false;

	public Dictionary<string, float> drops;

	private Chore chore;

	private static readonly EventSystem.IntraObjectHandler<Butcherable> SetReadyToButcherDelegate = new EventSystem.IntraObjectHandler<Butcherable>(delegate(Butcherable component, object data)
	{
		component.SetReadyToButcher(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Butcherable> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<Butcherable>(delegate(Butcherable component, object data)
	{
		component.OnRefreshUserMenu(data);
	});

	public void SetDrops(string[] drops)
	{
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		for (int i = 0; i < drops.Length; i++)
		{
			if (!dictionary.ContainsKey(drops[i]))
			{
				dictionary.Add(drops[i], 0f);
			}
			dictionary[drops[i]] += 1f;
		}
		SetDrops(dictionary);
	}

	public void SetDrops(Dictionary<string, float> drops)
	{
		this.drops = drops;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Subscribe(1272413801, SetReadyToButcherDelegate);
		Subscribe(493375141, OnRefreshUserMenuDelegate);
		workTime = 3f;
		multitoolContext = "harvest";
		multitoolHitEffectTag = "fx_harvest_splash";
	}

	public void SetReadyToButcher(object param)
	{
		readyToButcher = true;
	}

	public void SetReadyToButcher(bool ready)
	{
		readyToButcher = ready;
	}

	public void ActivateChore(object param)
	{
		if (chore == null)
		{
			chore = new WorkChore<Butcherable>(Db.Get().ChoreTypes.Harvest, this);
			OnRefreshUserMenu(null);
		}
	}

	public void CancelChore(object param)
	{
		if (chore != null)
		{
			chore.Cancel("User cancelled");
			chore = null;
		}
	}

	private void OnClickCancel()
	{
		CancelChore(null);
	}

	private void OnClickButcher()
	{
		if (DebugHandler.InstantBuildMode)
		{
			OnButcherComplete();
		}
		else
		{
			ActivateChore(null);
		}
	}

	private void OnRefreshUserMenu(object data)
	{
		if (readyToButcher)
		{
			KIconButtonMenu.ButtonInfo button = ((chore != null) ? new KIconButtonMenu.ButtonInfo("action_harvest", "Cancel Meatify", OnClickCancel) : new KIconButtonMenu.ButtonInfo("action_harvest", "Meatify", OnClickButcher));
			Game.Instance.userMenu.AddButton(base.gameObject, button);
		}
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		OnButcherComplete();
	}

	public GameObject[] CreateDrops(float multiplier = 1f)
	{
		GameObject[] array = new GameObject[drops.Count];
		int num = 0;
		float temperature = GetComponent<PrimaryElement>().Temperature;
		foreach (KeyValuePair<string, float> drop in drops)
		{
			GameObject gameObject = Scenario.SpawnPrefab(GetDropSpawnLocation(), 0, 0, drop.Key);
			gameObject.SetActive(value: true);
			PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
			component.Mass = component.Mass * multiplier * drop.Value;
			component.Temperature = temperature;
			Edible component2 = gameObject.GetComponent<Edible>();
			if ((bool)component2)
			{
				ReportManager.Instance.ReportValue(ReportManager.ReportType.CaloriesCreated, component2.Calories, StringFormatter.Replace(UI.ENDOFDAYREPORT.NOTES.BUTCHERED, "{0}", gameObject.GetProperName()), UI.ENDOFDAYREPORT.NOTES.BUTCHERED_CONTEXT);
			}
			array[num] = gameObject;
			num++;
		}
		return array;
	}

	public void OnButcherComplete()
	{
		if (butchered)
		{
			return;
		}
		KSelectable component = GetComponent<KSelectable>();
		if ((bool)component && component.IsSelected)
		{
			SelectTool.Instance.Select(null);
		}
		Pickupable component2 = GetComponent<Pickupable>();
		Storage storage = ((component2 != null) ? component2.storage : null);
		GameObject[] array = CreateDrops();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (storage != null && storage.storeDropsFromButcherables)
				{
					storage.Store(array[i]);
				}
			}
		}
		chore = null;
		butchered = true;
		readyToButcher = false;
		Game.Instance.userMenu.Refresh(base.gameObject);
		Trigger(395373363, (object)array);
	}

	private int GetDropSpawnLocation()
	{
		int num = Grid.PosToCell(base.gameObject);
		int num2 = Grid.CellAbove(num);
		if (Grid.IsValidCell(num2) && !Grid.Solid[num2])
		{
			return num2;
		}
		return num;
	}
}
