using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/LiquidCooledFanWorkable")]
public class LiquidCooledFanWorkable : Workable
{
	[MyCmpGet]
	private Operational operational;

	private LiquidCooledFanWorkable()
	{
		showProgressBar = false;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		workerStatusItem = null;
	}

	protected override void OnSpawn()
	{
		GameScheduler.Instance.Schedule("InsulationTutorial", 2f, delegate
		{
			Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_Insulation);
		});
		base.OnSpawn();
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		operational.SetActive(value: true);
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		operational.SetActive(value: false);
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		operational.SetActive(value: false);
	}
}
