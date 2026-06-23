using System;
using UnityEngine;

public class LargeImpactorVanillaConfig : IEntityConfig, IHasDlcRestrictions
{
	public class BackgroundMotion : ParallaxBackgroundObject.IMotion
	{
		private LargeImpactorStatus.Instance statusMonitor;

		private LargeImpactorStatus.Instance StatusMonitor
		{
			get
			{
				if (statusMonitor == null)
				{
					statusMonitor = GetStatusMonitor();
				}
				return statusMonitor;
			}
		}

		public float GetETA()
		{
			return StatusMonitor.IsRunning() ? StatusMonitor.TimeRemainingBeforeCollision : GetDuration();
		}

		public float GetDuration()
		{
			return LargeImpactorEvent.GetImpactTime();
		}

		public void OnNormalizedDistanceChanged(float normalizedDistance)
		{
			AmbienceManager component = Game.Instance.GetComponent<AmbienceManager>();
			AmbienceManager.Quadrant[] quadrants = component.quadrants;
			foreach (AmbienceManager.Quadrant quadrant in quadrants)
			{
				quadrant.spaceLayer.SetCustomParameter("distanceToMeteor", normalizedDistance);
			}
		}
	}

	public static string ID = "LargeImpactorVanilla";

	public static string NAME = "LargestPotaytoeVanilla";

	public string[] GetRequiredDlcIds()
	{
		return new string[2] { "", "DLC4_ID" };
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	GameObject IEntityConfig.CreatePrefab()
	{
		return ConfigCommon(ID, NAME);
	}

	public static GameObject ConfigCommon(string id, string name)
	{
		GameObject gameObject = EntityTemplates.CreateEntity(id, name);
		gameObject.AddOrGet<SaveLoadRoot>();
		gameObject.AddOrGet<StateMachineController>();
		gameObject.AddOrGet<Notifier>();
		gameObject.AddOrGet<LoopingSounds>();
		LargeImpactorStatus.Def def = gameObject.AddOrGetDef<LargeImpactorStatus.Def>();
		def.MAX_HEALTH = 1000;
		def.EventID = "LargeImpactor";
		gameObject.AddOrGet<LargeImpactorVisualizer>();
		LargeImpactorCrashStamp largeImpactorCrashStamp = gameObject.AddOrGet<LargeImpactorCrashStamp>();
		largeImpactorCrashStamp.largeStampTemplate = "dlc4::poi/asteroid_impacts/potato_large";
		gameObject.AddOrGetDef<LargeImpactorNotificationMonitor.Def>();
		gameObject.AddOrGet<ParallaxBackgroundObject>().Initialize("Demolior_final_whole");
		return gameObject;
	}

	void IEntityConfig.OnPrefabInit(GameObject inst)
	{
	}

	private static LargeImpactorStatus.Instance GetStatusMonitor()
	{
		GameplayEventInstance gameplayEventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.Id);
		LargeImpactorEvent.StatesInstance statesInstance = (LargeImpactorEvent.StatesInstance)gameplayEventInstance.smi;
		return statesInstance.impactorInstance.GetSMI<LargeImpactorStatus.Instance>();
	}

	public static void SpawnCommon(GameObject inst)
	{
		ParallaxBackgroundObject component = inst.GetComponent<ParallaxBackgroundObject>();
		component.motion = new BackgroundMotion();
		LargeImpactorStatus.Instance statusMonitor = GetStatusMonitor();
		if (statusMonitor != null)
		{
			statusMonitor.OnDamaged = (Action<int>)Delegate.Combine(statusMonitor.OnDamaged, new Action<int>(component.TriggerShaderDamagedEffect));
		}
	}

	void IEntityConfig.OnSpawn(GameObject inst)
	{
		SpawnCommon(inst);
	}
}
