using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class LargeImpactorKeepsake : GameStateMachine<LargeImpactorKeepsake, LargeImpactorKeepsake.Instance, IStateMachineTarget, LargeImpactorKeepsake.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Notification notification;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			notification = CreateDeathNotification();
		}

		private Notification CreateDeathNotification()
		{
			string title = MISC.NOTIFICATIONS.LARGE_IMPACTOR_KEEPSAKE.NAME;
			Func<List<Notification>, object, string> tooltip = (List<Notification> notificationList, object data) => MISC.NOTIFICATIONS.LARGE_IMPACTOR_KEEPSAKE.TOOLTIP;
			Transform click_focus = base.gameObject.transform;
			return new Notification(title, NotificationType.Event, tooltip, null, expires: false, 0f, MarkAsAknowledgedAndFocusCamera, this, click_focus, volume_attenuation: true, clear_on_click: true);
		}

		private void MarkAsAknowledgedAndFocusCamera(object data)
		{
			if (data != null)
			{
				Instance instance = (Instance)data;
				instance.sm.HasNotificationBeenAknowledged.Set(value: true, instance);
				GameUtil.FocusCamera(base.gameObject.transform);
			}
		}
	}

	private State notification;

	private State idle;

	private BoolParameter HasNotificationBeenAknowledged;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = notification;
		notification.ParamTransition(HasNotificationBeenAknowledged, idle, GameStateMachine<LargeImpactorKeepsake, Instance, IStateMachineTarget, Def>.IsTrue).ToggleNotification(GetNotification);
		idle.DoNothing();
	}

	public static Notification GetNotification(Instance smi)
	{
		return smi.notification;
	}
}
