using System.Collections.Generic;

public class KAnimSynchronizer
{
	public delegate string TranslateAnimName(string masterAnimName);

	private string idle_anim = "idle_default";

	private KAnimControllerBase masterController;

	private List<KAnimControllerBase> Targets = new List<KAnimControllerBase>();

	private readonly Dictionary<KAnimControllerBase, TranslateAnimName> targetTranslators = new Dictionary<KAnimControllerBase, TranslateAnimName>();

	private List<KAnimSynchronizedController> SyncedControllers = new List<KAnimSynchronizedController>();

	public string IdleAnim
	{
		get
		{
			return idle_anim;
		}
		set
		{
			idle_anim = value;
		}
	}

	public KAnimSynchronizer(KAnimControllerBase master_controller)
	{
		masterController = master_controller;
	}

	private void Clear(KAnimControllerBase controller)
	{
		controller.Play(IdleAnim, KAnim.PlayMode.Loop);
	}

	public void Add(KAnimControllerBase controller, TranslateAnimName translate = null)
	{
		Targets.Add(controller);
		if (translate != null)
		{
			targetTranslators[controller] = translate;
		}
	}

	public void Remove(KAnimControllerBase controller)
	{
		Clear(controller);
		Targets.Remove(controller);
		targetTranslators.Remove(controller);
	}

	public void RemoveWithoutIdleAnim(KAnimControllerBase controller)
	{
		Targets.Remove(controller);
		targetTranslators.Remove(controller);
	}

	private void Clear(KAnimSynchronizedController controller)
	{
		controller.Play(IdleAnim, KAnim.PlayMode.Loop);
	}

	public void Add(KAnimSynchronizedController controller)
	{
		SyncedControllers.Add(controller);
	}

	public void Remove(KAnimSynchronizedController controller)
	{
		Clear(controller);
		SyncedControllers.Remove(controller);
	}

	public void Clear()
	{
		foreach (KAnimControllerBase target in Targets)
		{
			if (!(target == null) && target.AnimFiles != null)
			{
				Clear(target);
			}
		}
		Targets.Clear();
		targetTranslators.Clear();
		foreach (KAnimSynchronizedController syncedController in SyncedControllers)
		{
			if (!(syncedController.synchronizedController == null) && syncedController.synchronizedController.AnimFiles != null)
			{
				Clear(syncedController);
			}
		}
		SyncedControllers.Clear();
	}

	public void Sync(KAnimControllerBase controller)
	{
		if (masterController == null || controller == null)
		{
			return;
		}
		KAnim.Anim currentAnim = masterController.GetCurrentAnim();
		if (currentAnim == null)
		{
			return;
		}
		targetTranslators.TryGetValue(controller, out var value);
		string text = ((value != null) ? value(currentAnim.name) : currentAnim.name);
		if (!string.IsNullOrEmpty(controller.defaultAnim) && !controller.HasAnimation(text))
		{
			controller.Play(controller.defaultAnim, KAnim.PlayMode.Loop);
			return;
		}
		KAnim.PlayMode mode = masterController.GetMode();
		float playSpeed = masterController.GetPlaySpeed();
		float elapsedTime = masterController.GetElapsedTime();
		controller.Play(text, mode, playSpeed, elapsedTime);
		Facing component = controller.GetComponent<Facing>();
		if (component != null)
		{
			float x = component.transform.GetPosition().x;
			x += (masterController.FlipX ? (-0.5f) : 0.5f);
			component.Face(x);
		}
		else
		{
			controller.FlipX = masterController.FlipX;
			controller.FlipY = masterController.FlipY;
		}
	}

	public void SyncController(KAnimSynchronizedController controller)
	{
		if (masterController == null || controller == null)
		{
			return;
		}
		KAnim.Anim currentAnim = masterController.GetCurrentAnim();
		string text = ((currentAnim != null) ? (currentAnim.name + controller.Postfix) : string.Empty);
		if (!string.IsNullOrEmpty(controller.synchronizedController.defaultAnim) && !controller.synchronizedController.HasAnimation(text))
		{
			controller.Play(controller.synchronizedController.defaultAnim, KAnim.PlayMode.Loop);
		}
		else if (currentAnim != null)
		{
			KAnim.PlayMode mode = masterController.GetMode();
			float playSpeed = masterController.GetPlaySpeed();
			float elapsedTime = masterController.GetElapsedTime();
			controller.Play(text, mode, playSpeed, elapsedTime);
			Facing component = controller.synchronizedController.GetComponent<Facing>();
			if (component != null)
			{
				float x = component.transform.GetPosition().x;
				x += (masterController.FlipX ? (-0.5f) : 0.5f);
				component.Face(x);
			}
			else
			{
				controller.synchronizedController.FlipX = masterController.FlipX;
				controller.synchronizedController.FlipY = masterController.FlipY;
			}
		}
	}

	public void Sync()
	{
		for (int i = 0; i < Targets.Count; i++)
		{
			KAnimControllerBase controller = Targets[i];
			Sync(controller);
		}
		for (int j = 0; j < SyncedControllers.Count; j++)
		{
			KAnimSynchronizedController controller2 = SyncedControllers[j];
			SyncController(controller2);
		}
	}

	public void SyncTime()
	{
		float elapsedTime = masterController.GetElapsedTime();
		for (int i = 0; i < Targets.Count; i++)
		{
			KAnimControllerBase kAnimControllerBase = Targets[i];
			kAnimControllerBase.SetElapsedTime(elapsedTime);
			KBatchedAnimController kBatchedAnimController = kAnimControllerBase as KBatchedAnimController;
			if (kBatchedAnimController != null)
			{
				kBatchedAnimController.UpdateFromSync();
			}
		}
		for (int j = 0; j < SyncedControllers.Count; j++)
		{
			SyncedControllers[j].synchronizedController.SetElapsedTime(elapsedTime);
		}
	}
}
