using System;
using System.Collections.Generic;
using System.Linq;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class PrinterceptorScreen : KModalScreen
{
	public static PrinterceptorScreen Instance;

	[SerializeField]
	private RectTransform optionGridContainer;

	[SerializeField]
	private GameObject optionButtonPrefab;

	[SerializeField]
	private LocText selectedTitleText;

	[SerializeField]
	private Image selectedIcon;

	[SerializeField]
	private Image selectedIconAlt;

	[SerializeField]
	private LocText selectedEffectsText;

	[SerializeField]
	private LocText selectedFlavourText;

	[SerializeField]
	private KButton printButton;

	[SerializeField]
	private KButton closeButton;

	[SerializeField]
	private LocText dataWalletLabel;

	[SerializeField]
	private Image[] dataWalletIcon;

	[SerializeField]
	private LocText selectedCostLabel;

	[SerializeField]
	private Image selectedCostIcon;

	private const string LOCKER_MENU_MUSIC = "Music_SupplyCloset";

	private const string MUSIC_PARAMETER = "SupplyClosetView";

	[SerializeField]
	private Material desatUIMaterial;

	private HijackedHeadquarters.Instance target;

	private Dictionary<Tag, MultiToggle> optionButtons = new Dictionary<Tag, MultiToggle>();

	public Tag selectedEntityTag { get; private set; }

	protected override void OnActivate()
	{
		Instance = this;
		Show(show: false);
		closeButton.ClearOnClick();
		closeButton.onClick += delegate
		{
			Show(show: false);
		};
	}

	public void SetTarget(HijackedHeadquarters.Instance target)
	{
		this.target = target;
		printButton.ClearOnClick();
		printButton.onClick += delegate
		{
			target.Trigger(1816718186);
			Show(show: false);
		};
	}

	public override float GetSortKey()
	{
		return 40f;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
	}

	public override void Show(bool show = true)
	{
		base.Show(show);
		if (show)
		{
			AudioMixer.instance.Start(AudioMixerSnapshots.Get().FrontEndSupplyClosetSnapshot);
			MusicManager.instance.OnSupplyClosetMenu(paused: true, 0.5f);
			MusicManager.instance.PlaySong("Music_SupplyCloset");
			Image[] array = dataWalletIcon;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].sprite = Def.GetUISprite(DatabankHelper.ID).first;
			}
			dataWalletLabel.SetText(GameUtil.SafeStringFormat(UI.PRINTERCEPTORSCREEN.DATABANKS_AVAILABLE, target.GetComponent<Storage>().GetAmountAvailable(DatabankHelper.ID).ToString()));
			SelectEntity(selectedEntityTag);
			{
				foreach (KeyValuePair<Tag, MultiToggle> optionButton in optionButtons)
				{
					RefreshOptionButton(optionButton.Key);
				}
				return;
			}
		}
		AudioMixer.instance.Stop(AudioMixerSnapshots.Get().FrontEndSupplyClosetSnapshot);
		MusicManager.instance.OnSupplyClosetMenu(paused: false, 1f);
		if (MusicManager.instance.SongIsPlaying("Music_SupplyCloset"))
		{
			MusicManager.instance.StopSong("Music_SupplyCloset");
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		SpawnOptionButtons();
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (e.TryConsume(Action.Escape) || e.TryConsume(Action.MouseRight))
		{
			Show(show: false);
		}
		else
		{
			base.OnKeyDown(e);
		}
	}

	public override void Deactivate()
	{
		Show(show: false);
	}

	private void SpawnOptionButtons()
	{
		foreach (KeyValuePair<Tag, List<EggCrackerConfig.EggData>> eggsBySpecy in EggCrackerConfig.EggsBySpecies)
		{
			foreach (EggCrackerConfig.EggData item in eggsBySpecy.Value)
			{
				if (item.isBaseMorph)
				{
					SpawnOptionButton(item.id);
				}
			}
		}
		List<Tag> list = new List<Tag>();
		list.AddRange(from x in Assets.GetPrefabsWithTag(GameTags.Seed)
			select x.GetComponent<KPrefabID>().PrefabTag);
		list.AddRange(from x in Assets.GetPrefabsWithTag(GameTags.CropSeed)
			select x.GetComponent<KPrefabID>().PrefabTag);
		foreach (Tag item2 in list)
		{
			SpawnOptionButton(item2);
		}
		SelectEntity("SquirrelEgg");
		SpawnOptionButton("BeeBaby");
	}

	private void SpawnOptionButton(Tag id)
	{
		if (optionButtons.ContainsKey(id))
		{
			return;
		}
		GameObject gameObject = Assets.TryGetPrefab(id);
		if (gameObject == null || !Game.IsCorrectDlcActiveForCurrentSave(gameObject.GetComponent<KPrefabID>()) || gameObject.HasTag(GameTags.DeprecatedContent))
		{
			return;
		}
		PlantableSeed component = gameObject.GetComponent<PlantableSeed>();
		if (component != null)
		{
			GameObject prefab = Assets.GetPrefab(component.PlantID);
			if (prefab != null && prefab.HasTag(GameTags.DeprecatedContent))
			{
				return;
			}
		}
		GameObject obj = Util.KInstantiateUI(optionButtonPrefab, optionGridContainer.gameObject, force_active: true);
		MultiToggle component2 = obj.GetComponent<MultiToggle>();
		optionButtons.Add(id, component2);
		HierarchyReferences component3 = obj.GetComponent<HierarchyReferences>();
		component2.onClick = (System.Action)Delegate.Combine(component2.onClick, (System.Action)delegate
		{
			SelectEntity(id);
		});
		component3.GetReference<Image>("FGIcon").sprite = Def.GetUISprite(id).first;
		component3.GetReference<LocText>("NameLabel").text = id.ProperName();
		component3.GetReference<Image>("ProgressOverlay").fillAmount = 0f;
		component3.GetReference<LocText>("CostLabel").text = HijackedHeadquartersConfig.GetDataBankCost(id, GetPrintCount(id)).ToString();
		component3.GetReference<Image>("CostIcon").sprite = Def.GetUISprite(DatabankHelper.ID).first;
	}

	private void RefreshOptionButton(Tag id)
	{
		optionButtons[id].GetComponent<HierarchyReferences>().GetReference<LocText>("CostLabel").text = HijackedHeadquartersConfig.GetDataBankCost(id, GetPrintCount(id)).ToString();
	}

	private void SelectEntity(Tag id)
	{
		selectedEntityTag = id;
		GameObject prefab = Assets.GetPrefab(selectedEntityTag);
		selectedEffectsText.text = prefab.GetComponent<InfoDescription>().description;
		selectedTitleText.text = prefab.GetProperName();
		selectedIcon.sprite = Def.GetUISprite(selectedEntityTag).first;
		if (prefab.HasTag(GameTags.Egg))
		{
			Tag spawnedCreature = prefab.GetDef<IncubationMonitor.Def>().spawnedCreature;
			selectedIconAlt.sprite = Def.GetUISprite(spawnedCreature).first;
		}
		else if (prefab.HasTag(GameTags.Seed))
		{
			selectedIconAlt.sprite = Def.GetUISprite(prefab.GetComponent<PlantableSeed>().PlantID).first;
		}
		else if (prefab.HasTag(GameTags.CropSeed))
		{
			selectedIconAlt.sprite = Def.GetUISprite(prefab.GetComponent<PlantableSeed>().PlantID).first;
		}
		else if (prefab.HasTag(GameTags.Creature))
		{
			CreatureBrain component = prefab.GetComponent<CreatureBrain>();
			selectedIconAlt.sprite = Def.GetUISprite(component.species).first;
		}
		else
		{
			selectedIconAlt.sprite = null;
		}
		foreach (KeyValuePair<Tag, MultiToggle> optionButton in optionButtons)
		{
			optionButton.Value.GetComponent<MultiToggle>().ChangeState((selectedEntityTag == optionButton.Key) ? 1 : 0);
		}
		selectedCostIcon.sprite = Def.GetUISprite(DatabankHelper.ID).first;
		selectedCostLabel.SetText(GameUtil.SafeStringFormat(UI.PRINTERCEPTORSCREEN.DATABANKS_COST, HijackedHeadquartersConfig.GetDataBankCost(selectedEntityTag, GetPrintCount(selectedEntityTag)).ToString()));
		printButton.isInteractable = target != null && target.GetComponent<Storage>().GetAmountAvailable(DatabankHelper.ID) >= (float)HijackedHeadquartersConfig.GetDataBankCost(selectedEntityTag, GetPrintCount(selectedEntityTag));
		printButton.GetComponent<ToolTip>().SetSimpleTooltip(printButton.isInteractable ? GameUtil.SafeStringFormat(UI.PRINTERCEPTORSCREEN.PRINT_TOOLTIP, 25) : ((string)UI.PRINTERCEPTORSCREEN.PRINT_TOOLTIP_DISABLED));
	}

	private int GetPrintCount(Tag id)
	{
		if (target == null || !target.printCounts.ContainsKey(id))
		{
			return 0;
		}
		return target.printCounts[id];
	}
}
