using STRINGS;

public interface IEmptyableCargo
{
	IStateMachineTarget master { get; }

	bool CanAutoDeploy { get; }

	bool AutoDeploy { get; set; }

	bool ChooseDuplicant { get; }

	bool ModuleDeployed { get; }

	MinionIdentity ChosenDuplicant { get; set; }

	bool CanTargetClusterGridEntities => false;

	string GetButtonText => UI.UISIDESCREENS.MODULEFLIGHTUTILITYSIDESCREEN.DEPLOY_BUTTON;

	string GetButtonToolip => UI.UISIDESCREENS.MODULEFLIGHTUTILITYSIDESCREEN.DEPLOY_BUTTON_TOOLTIP;

	bool CanEmptyCargo();

	void EmptyCargo();
}
