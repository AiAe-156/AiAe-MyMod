using System.Collections.Generic;
using UnityEngine;

public class EffectConfigs : IMultiEntityConfig
{
	public struct EffectTemplate
	{
		public string id;

		public string[] animFiles;

		public string initialAnim;

		public KAnim.PlayMode initialMode;

		public bool destroyOnAnimComplete;
	}

	public static string EffectTemplateId = "EffectTemplateFx";

	public static string EffectTemplateOverrideId = "EffectTemplateOverrideFx";

	public static string AttackSplashId = "AttackSplashFx";

	public static string OreAbsorbId = "OreAbsorbFx";

	public static string PlantDeathId = "PlantDeathFx";

	public static string BuildSplashId = "BuildSplashFx";

	public static string DemolishSplashId = "DemolishSplashFx";

	public static string SquidAttackId = "SquidAttackFx";

	public List<GameObject> CreatePrefabs()
	{
		List<GameObject> list = new List<GameObject>();
		List<EffectTemplate> list2 = new List<EffectTemplate>();
		list2.Add(new EffectTemplate
		{
			id = EffectTemplateId,
			animFiles = new string[0],
			initialAnim = "",
			initialMode = KAnim.PlayMode.Once,
			destroyOnAnimComplete = false
		});
		list2.Add(new EffectTemplate
		{
			id = EffectTemplateOverrideId,
			animFiles = new string[0],
			initialAnim = "",
			initialMode = KAnim.PlayMode.Once,
			destroyOnAnimComplete = false
		});
		list2.Add(new EffectTemplate
		{
			id = AttackSplashId,
			animFiles = new string[1] { "attack_beam_contact_fx_kanim" },
			initialAnim = "loop",
			initialMode = KAnim.PlayMode.Loop,
			destroyOnAnimComplete = false
		});
		list2.Add(new EffectTemplate
		{
			id = OreAbsorbId,
			animFiles = new string[1] { "ore_collision_kanim" },
			initialAnim = "idle",
			initialMode = KAnim.PlayMode.Once,
			destroyOnAnimComplete = true
		});
		list2.Add(new EffectTemplate
		{
			id = PlantDeathId,
			animFiles = new string[1] { "plant_death_fx_kanim" },
			initialAnim = "plant_death",
			initialMode = KAnim.PlayMode.Once,
			destroyOnAnimComplete = true
		});
		list2.Add(new EffectTemplate
		{
			id = BuildSplashId,
			animFiles = new string[1] { "sparks_radial_build_kanim" },
			initialAnim = "loop",
			initialMode = KAnim.PlayMode.Loop,
			destroyOnAnimComplete = false
		});
		list2.Add(new EffectTemplate
		{
			id = DemolishSplashId,
			animFiles = new string[1] { "poi_demolish_impact_kanim" },
			initialAnim = "POI_demolish_impact",
			initialMode = KAnim.PlayMode.Loop,
			destroyOnAnimComplete = false
		});
		List<EffectTemplate> list3 = list2;
		if (DlcManager.IsContentSubscribed("DLC5_ID"))
		{
			list3.Add(new EffectTemplate
			{
				id = SquidAttackId,
				animFiles = new string[1] { "squid_ink_fx_kanim" },
				initialAnim = "loop",
				initialMode = KAnim.PlayMode.Once,
				destroyOnAnimComplete = true
			});
		}
		foreach (EffectTemplate item in list3)
		{
			GameObject gameObject = EntityTemplates.CreateEntity(item.id, item.id, is_selectable: false);
			KBatchedAnimController kBatchedAnimController = gameObject.AddOrGet<KBatchedAnimController>();
			kBatchedAnimController.materialType = KAnimBatchGroup.MaterialType.Simple;
			kBatchedAnimController.initialAnim = item.initialAnim;
			kBatchedAnimController.initialMode = item.initialMode;
			kBatchedAnimController.isMovable = true;
			kBatchedAnimController.destroyOnAnimComplete = item.destroyOnAnimComplete;
			if (item.id == EffectTemplateOverrideId)
			{
				SymbolOverrideControllerUtil.AddToPrefab(gameObject);
			}
			if (item.animFiles.Length != 0)
			{
				KAnimFile[] array = new KAnimFile[item.animFiles.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Assets.GetAnim(item.animFiles[i]);
				}
				kBatchedAnimController.AnimFiles = array;
			}
			gameObject.AddOrGet<LoopingSounds>();
			list.Add(gameObject);
		}
		return list;
	}

	public void OnPrefabInit(GameObject go)
	{
	}

	public void OnSpawn(GameObject go)
	{
	}
}
