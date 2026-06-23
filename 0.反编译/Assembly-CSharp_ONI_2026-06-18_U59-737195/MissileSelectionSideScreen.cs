using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class MissileSelectionSideScreen : SideScreenContent
{
	private IMissileSelectionInterface targetMissileLauncher;

	[SerializeField]
	private GameObject rowPrefab;

	[SerializeField]
	private GameObject listContainer;

	[SerializeField]
	private LocText headerLabel;

	private List<Tag> ammunitiontags = new List<Tag> { "MissileBasic" };

	private Dictionary<Tag, GameObject> rows = new Dictionary<Tag, GameObject>();

	public override int GetSideScreenSortOrder()
	{
		return 500;
	}

	public override bool IsValidForTarget(GameObject target)
	{
		if (target.GetComponent<IMissileSelectionInterface>() == null)
		{
			return target.GetSMI<IMissileSelectionInterface>() != null;
		}
		return true;
	}

	public override void SetTarget(GameObject target)
	{
		base.SetTarget(target);
		targetMissileLauncher = target.GetComponent<IMissileSelectionInterface>();
		if (targetMissileLauncher == null)
		{
			targetMissileLauncher = target.GetSMI<IMissileSelectionInterface>();
		}
		Build();
	}

	private void Build()
	{
		foreach (KeyValuePair<Tag, GameObject> row in rows)
		{
			Util.KDestroyGameObject(row.Value);
		}
		rows.Clear();
		ammunitiontags = targetMissileLauncher.GetValidAmmunitionTags();
		UpdateLongRangeMissiles();
		foreach (Tag ammunitiontag in ammunitiontags)
		{
			GameObject gameObject = Util.KInstantiateUI(rowPrefab, listContainer);
			gameObject.gameObject.name = ammunitiontag.ProperName();
			rows.Add(ammunitiontag, gameObject);
		}
		Refresh();
	}

	private void UpdateLongRangeMissiles()
	{
		if (DlcManager.IsExpansion1Active())
		{
			foreach (Tag cosmicBlastShotType in MissileLauncherConfig.CosmicBlastShotTypes)
			{
				if (!ammunitiontags.Contains(cosmicBlastShotType))
				{
					ammunitiontags.Add(cosmicBlastShotType);
				}
			}
			return;
		}
		if (GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.IdHash) == null)
		{
			ammunitiontags.Remove("MissileLongRange");
		}
		else if (!ammunitiontags.Contains("MissileLongRange"))
		{
			ammunitiontags.Add("MissileLongRange");
		}
	}

	private void Refresh()
	{
		foreach (KeyValuePair<Tag, GameObject> kvp in rows)
		{
			kvp.Value.GetComponent<HierarchyReferences>().GetReference<LocText>("Label").SetText(kvp.Key.ProperNameStripLink());
			kvp.Value.GetComponent<HierarchyReferences>().GetReference<Image>("Icon").sprite = Def.GetUISprite(kvp.Key).first;
			kvp.Value.GetComponent<HierarchyReferences>().GetReference<Image>("Icon").color = Def.GetUISprite(kvp.Key).second;
			kvp.Value.GetComponent<HierarchyReferences>().GetReference<MultiToggle>("Toggle").onClick = delegate
			{
				targetMissileLauncher.ChangeAmmunition(kvp.Key, !targetMissileLauncher.AmmunitionIsAllowed(kvp.Key));
				targetMissileLauncher.OnRowToggleClick();
				DetailsScreen.Instance.Refresh(SelectTool.Instance.selected.gameObject);
				Refresh();
			};
			kvp.Value.GetComponent<HierarchyReferences>().GetReference<MultiToggle>("Toggle").ChangeState(targetMissileLauncher.AmmunitionIsAllowed(kvp.Key) ? 1 : 0);
			kvp.Value.SetActive(value: true);
		}
	}

	public override string GetTitle()
	{
		return UI.UISIDESCREENS.MISSILESELECTIONSIDESCREEN.TITLE;
	}
}
