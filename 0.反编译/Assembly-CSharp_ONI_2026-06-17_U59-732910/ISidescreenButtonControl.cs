public interface ISidescreenButtonControl
{
	string SidescreenButtonText { get; }

	string SidescreenButtonTooltip { get; }

	string SidescreenTitle => null;

	void SetButtonTextOverride(ButtonMenuTextOverride textOverride);

	bool SidescreenEnabled();

	bool SidescreenButtonInteractable();

	void OnSidescreenButtonPressed();

	int HorizontalGroupID();

	int ButtonSideScreenSortOrder();
}
