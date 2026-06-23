using System;
using System.Runtime.CompilerServices;

internal class OilFloaterMovementSound : KMonoBehaviour
{
	public string sound;

	public bool isPlayingSound;

	public bool isMoving;

	private ulong cellChangedHandlerID = 0uL;

	private static readonly EventSystem.IntraObjectHandler<OilFloaterMovementSound> OnObjectMovementStateChangedDelegate = new EventSystem.IntraObjectHandler<OilFloaterMovementSound>(delegate(OilFloaterMovementSound component, object data)
	{
		component.OnObjectMovementStateChanged(data);
	});

	private static readonly Action<object> UpdateSoundDispatcher = delegate(object obj)
	{
		Unsafe.As<OilFloaterMovementSound>(obj).UpdateSound();
	};

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		sound = GlobalAssets.GetSound(sound);
		Subscribe(1027377649, OnObjectMovementStateChangedDelegate);
		cellChangedHandlerID = Singleton<CellChangeMonitor>.Instance.RegisterCellChangedHandler(base.transform, UpdateSoundDispatcher, this, "OilFloaterMovementSound");
	}

	private void OnObjectMovementStateChanged(object data)
	{
		GameHashes gameHashes = Boxed<GameHashes>.Unbox(data);
		isMoving = gameHashes == GameHashes.ObjectMovementWakeUp;
		UpdateSound();
	}

	private void UpdateSound()
	{
		bool flag = isMoving && GetComponent<Navigator>().CurrentNavType != NavType.Swim;
		if (flag != isPlayingSound)
		{
			LoopingSounds component = GetComponent<LoopingSounds>();
			if (flag)
			{
				component.StartSound(sound);
			}
			else
			{
				component.StopSound(sound);
			}
			isPlayingSound = flag;
		}
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		Singleton<CellChangeMonitor>.Instance.UnregisterCellChangedHandler(ref cellChangedHandlerID);
	}
}
