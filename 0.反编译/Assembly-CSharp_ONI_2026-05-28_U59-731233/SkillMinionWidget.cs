using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("KMonoBehaviour/scripts/SkillMinionWidget")]
public class SkillMinionWidget : KMonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField]
	private SkillsScreen skillsScreen;

	[SerializeField]
	private CrewPortrait portrait;

	[SerializeField]
	private LocText masteryPoints;

	[SerializeField]
	private LocText morale;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Image hat_background;

	[SerializeField]
	private Color selected_color;

	[SerializeField]
	private Color unselected_color;

	[SerializeField]
	private Color hover_color;

	[SerializeField]
	private DropDown hatDropDown;

	[SerializeField]
	private TextStyleSetting TooltipTextStyle_Header;

	[SerializeField]
	private TextStyleSetting TooltipTextStyle_AbilityNegativeModifier;

	public ButtonSoundPlayer soundPlayer;

	public IAssignableIdentity assignableIdentity { get; private set; }

	public void SetMinon(IAssignableIdentity identity)
	{
		assignableIdentity = identity;
		portrait.SetIdentityObject(assignableIdentity);
		GetComponent<NotificationHighlightTarget>().targetKey = identity.GetSoleOwner().gameObject.GetInstanceID().ToString();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ToggleHover(on: true);
		soundPlayer.Play(1);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ToggleHover(on: false);
	}

	private void ToggleHover(bool on)
	{
		if (skillsScreen.CurrentlySelectedMinion != assignableIdentity)
		{
			SetColor(on ? hover_color : unselected_color);
		}
	}

	private void SetColor(Color color)
	{
		background.color = color;
		if (assignableIdentity != null && assignableIdentity as StoredMinionIdentity != null)
		{
			GetComponent<CanvasGroup>().alpha = 0.6f;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			skillsScreen.CurrentlySelectedMinion = assignableIdentity;
			GetComponent<NotificationHighlightTarget>().View();
			KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click"));
		}
	}

	public void Refresh()
	{
		if (assignableIdentity.IsNullOrDestroyed())
		{
			return;
		}
		portrait.SetIdentityObject(assignableIdentity);
		string text = "";
		skillsScreen.GetMinionIdentity(assignableIdentity, out var minionIdentity, out var storedMinionIdentity);
		hatDropDown.gameObject.SetActive(value: true);
		if (minionIdentity != null)
		{
			MinionResume component = minionIdentity.GetComponent<MinionResume>();
			int availableSkillpoints = component.AvailableSkillpoints;
			int totalSkillPointsGained = component.TotalSkillPointsGained;
			masteryPoints.text = ((availableSkillpoints > 0) ? GameUtil.ApplyBoldString(GameUtil.ColourizeString(new Color(0.5f, 1f, 0.5f, 1f), availableSkillpoints.ToString())) : "0");
			AttributeInstance attributeInstance = Db.Get().Attributes.QualityOfLife.Lookup(component);
			AttributeInstance attributeInstance2 = Db.Get().Attributes.QualityOfLifeExpectation.Lookup(component);
			morale.text = $"{attributeInstance.GetTotalValue()}/{attributeInstance2.GetTotalValue()}";
			RefreshToolTip(component);
			List<IListableOption> list = new List<IListableOption>();
			foreach (MinionResume.HatInfo allHat in component.GetAllHats())
			{
				list.Add(new HatListable(allHat.Source, allHat.Hat));
			}
			hatDropDown.Initialize(list, OnHatDropEntryClick, hatDropDownSort, hatDropEntryRefreshAction, displaySelectedValueWhenClosed: false, minionIdentity);
			text = (string.IsNullOrEmpty(component.TargetHat) ? component.CurrentHat : component.TargetHat);
		}
		else
		{
			ToolTip component2 = GetComponent<ToolTip>();
			component2.ClearMultiStringTooltip();
			component2.AddMultiStringTooltip(string.Format(UI.TABLESCREENS.INFORMATION_NOT_AVAILABLE_TOOLTIP, storedMinionIdentity.GetStorageReason(), storedMinionIdentity.GetProperName()), null);
			text = (string.IsNullOrEmpty(storedMinionIdentity.targetHat) ? storedMinionIdentity.currentHat : storedMinionIdentity.targetHat);
			masteryPoints.text = UI.TABLESCREENS.NA;
			morale.text = UI.TABLESCREENS.NA;
		}
		bool flag = skillsScreen.CurrentlySelectedMinion == assignableIdentity;
		if (skillsScreen.CurrentlySelectedMinion != null && assignableIdentity != null)
		{
			flag = flag || skillsScreen.CurrentlySelectedMinion.GetSoleOwner() == assignableIdentity.GetSoleOwner();
		}
		SetColor(flag ? selected_color : unselected_color);
		HierarchyReferences component3 = GetComponent<HierarchyReferences>();
		RefreshHat(text);
		component3.GetReference("openButton").gameObject.SetActive(minionIdentity != null);
	}

	private void RefreshToolTip(MinionResume resume)
	{
		if (!(resume != null))
		{
			return;
		}
		AttributeInstance attributeInstance = Db.Get().Attributes.QualityOfLife.Lookup(resume);
		AttributeInstance attributeInstance2 = Db.Get().Attributes.QualityOfLifeExpectation.Lookup(resume);
		ToolTip component = GetComponent<ToolTip>();
		component.ClearMultiStringTooltip();
		component.AddMultiStringTooltip(assignableIdentity.GetProperName() + "\n\n", TooltipTextStyle_Header);
		component.AddMultiStringTooltip(string.Format(UI.SKILLS_SCREEN.CURRENT_MORALE, attributeInstance.GetTotalValue(), attributeInstance2.GetTotalValue()), null);
		component.AddMultiStringTooltip(string.Concat("\n", UI.DETAILTABS.STATS.NAME, "\n\n"), TooltipTextStyle_Header);
		foreach (AttributeInstance attribute in resume.GetAttributes())
		{
			if (attribute.Attribute.ShowInUI == Attribute.Display.Skill)
			{
				string text = UIConstants.ColorPrefixWhite;
				if (attribute.GetTotalValue() > 0f)
				{
					text = UIConstants.ColorPrefixGreen;
				}
				else if (attribute.GetTotalValue() < 0f)
				{
					text = UIConstants.ColorPrefixRed;
				}
				component.AddMultiStringTooltip("    • " + attribute.Name + ": " + text + attribute.GetTotalValue() + UIConstants.ColorSuffix, null);
			}
		}
	}

	public void RefreshHat(string hat)
	{
		HierarchyReferences component = GetComponent<HierarchyReferences>();
		component.GetReference("selectedHat").GetComponent<Image>().sprite = Assets.GetSprite(string.IsNullOrEmpty(hat) ? "hat_role_none" : hat);
	}

	private void OnHatDropEntryClick(IListableOption hatOption, object data)
	{
		skillsScreen.GetMinionIdentity(assignableIdentity, out var minionIdentity, out var _);
		if (minionIdentity == null)
		{
			return;
		}
		MinionResume component = minionIdentity.GetComponent<MinionResume>();
		if (hatOption != null)
		{
			HierarchyReferences component2 = GetComponent<HierarchyReferences>();
			component2.GetReference("selectedHat").GetComponent<Image>().sprite = Assets.GetSprite((hatOption as HatListable).hat);
			if (component != null)
			{
				string hat = (hatOption as HatListable).hat;
				component.SetHats(component.CurrentHat, hat);
				if (component.OwnsHat(hat))
				{
					component.CreateHatChangeChore();
				}
			}
		}
		else
		{
			HierarchyReferences component3 = GetComponent<HierarchyReferences>();
			component3.GetReference("selectedHat").GetComponent<Image>().sprite = Assets.GetSprite("hat_role_none");
			if (component != null)
			{
				component.SetHats(component.CurrentHat, null);
				component.ApplyTargetHat();
			}
		}
		skillsScreen.RefreshAll();
	}

	private void hatDropEntryRefreshAction(DropDownEntry entry, object targetData)
	{
		if (entry.entryData != null)
		{
			HatListable hatListable = entry.entryData as HatListable;
			entry.image.sprite = Assets.GetSprite(hatListable.hat);
		}
	}

	private int hatDropDownSort(IListableOption a, IListableOption b, object targetData)
	{
		return 0;
	}
}
