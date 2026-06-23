using STRINGS;
using UnityEngine;

public class RemoteWorkerConfig : IEntityConfig, IHasDlcRestrictions
{
	public static readonly string ID = "RemoteWorker";

	public const float MASS_KG = 200f;

	public const float DEBRIS_MASS_KG = 42f;

	public static readonly string DOCK_ANIM_OVERRIDES = "anim_interacts_remote_work_dock_kanim";

	public static readonly string IDLE_IN_DOCK_ANIM = "in_dock_idle";

	public static readonly string BUILD_MATERIAL = "Steel";

	public static readonly Tag BUILD_MATERIAL_TAG = new Tag(BUILD_MATERIAL);

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		string name = DUPLICANTS.MODEL.REMOTEWORKER.NAME;
		string description = DUPLICANTS.MODEL.REMOTEWORKER.DESC;
		GameObject gameObject = EntityTemplates.CreateEntity(ID, name);
		InfoDescription infoDescription = gameObject.AddOrGet<InfoDescription>();
		infoDescription.description = description;
		gameObject.AddComponent<Accessorizer>();
		gameObject.AddOrGet<WearableAccessorizer>();
		gameObject.AddComponent<StateMachineController>();
		KBatchedAnimController kBatchedAnimController = gameObject.AddOrGet<KBatchedAnimController>();
		kBatchedAnimController.defaultAnim = "in_dock_idle";
		kBatchedAnimController.initialAnim = "in_dock_idle";
		kBatchedAnimController.isMovable = true;
		kBatchedAnimController.initialMode = KAnim.PlayMode.Loop;
		kBatchedAnimController.AnimFiles = new KAnimFile[4]
		{
			Assets.GetAnim("body_comp_default_kanim"),
			Assets.GetAnim("anim_idles_default_kanim"),
			Assets.GetAnim("anim_loco_new_kanim"),
			Assets.GetAnim(DOCK_ANIM_OVERRIDES)
		};
		gameObject.AddOrGet<AnimEventHandler>();
		SymbolOverrideController symbolOverrideController = SymbolOverrideControllerUtil.AddToPrefab(gameObject);
		symbolOverrideController.applySymbolOverridesEveryFrame = true;
		symbolOverrideController.AddSymbolOverride("snapto_cheek", Assets.GetAnim("head_swap_kanim").GetData().build.GetSymbol("cheek_007"), 1);
		BaseMinionConfig.ConfigureSymbols(gameObject);
		Accessorizer component = gameObject.GetComponent<Accessorizer>();
		component.ApplyBodyData(CreateBodyData());
		component.ApplyAccessories();
		gameObject.AddTag(GameTags.Experimental);
		gameObject.AddTag(GameTags.Robot);
		KBoxCollider2D kBoxCollider2D = gameObject.AddOrGet<KBoxCollider2D>();
		kBoxCollider2D.size = new Vector2f(1f, 2f);
		kBoxCollider2D.offset = new Vector2f(0f, 1f);
		KBoxCollider2D kBoxCollider2D2 = gameObject.AddOrGet<KBoxCollider2D>();
		kBoxCollider2D2.offset = new Vector2(0f, 0.75f);
		kBoxCollider2D2.size = new Vector2(1f, 1.5f);
		Navigator navigator = gameObject.AddOrGet<Navigator>();
		navigator.NavGridName = "WalkerBabyNavGrid";
		navigator.CurrentNavType = NavType.Floor;
		navigator.defaultSpeed = 1f;
		navigator.updateProber = true;
		navigator.maxProbeRadiusX = 12;
		navigator.maxProbeRadiusY = 1;
		navigator.sceneLayer = Grid.SceneLayer.Creatures;
		PrimaryElement primaryElement = gameObject.AddOrGet<PrimaryElement>();
		primaryElement.ElementID = SimHashes.Steel;
		primaryElement.Mass = 200f;
		gameObject.AddComponent<RemoteWorkerExperienceProxy>();
		gameObject.AddComponent<RemoteWorker>();
		gameObject.AddComponent<RemoteWorkerSM>();
		gameObject.AddComponent<ChoreConsumer>();
		gameObject.AddComponent<Pickupable>();
		gameObject.AddComponent<SaveLoadRoot>();
		Storage storage = gameObject.AddComponent<Storage>();
		storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
		Clearable clearable = gameObject.AddOrGet<Clearable>();
		clearable.isClearable = false;
		CreatureFallMonitor.Def def = gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		def.canSwim = false;
		return gameObject;
	}

	public void OnPrefabInit(GameObject go)
	{
		Navigator navigator = go.AddOrGet<Navigator>();
		navigator.SetAbilities(new CreaturePathFinderAbilities(navigator));
	}

	public void OnSpawn(GameObject go)
	{
	}

	public static KCompBuilder.BodyData CreateBodyData()
	{
		KCompBuilder.BodyData result = default(KCompBuilder.BodyData);
		result.eyes = HashCache.Get().Add("eyes_014");
		result.hair = HashCache.Get().Add("hair_051");
		result.headShape = HashCache.Get().Add("headshape_006");
		result.mouth = HashCache.Get().Add("mouth_007");
		result.neck = HashCache.Get().Add("neck");
		result.arms = HashCache.Get().Add("arm_sleeve_006");
		result.armslower = HashCache.Get().Add("arm_lower_sleeve_006");
		result.body = HashCache.Get().Add("torso_006");
		result.hat = HashedString.Invalid;
		result.faceFX = HashedString.Invalid;
		result.armLowerSkin = HashCache.Get().Add("arm_lower_001");
		result.armUpperSkin = HashCache.Get().Add("arm_upper_001");
		result.legSkin = HashCache.Get().Add("leg_skin_001");
		result.neck = HashCache.Get().Add("neck_006");
		result.legs = HashCache.Get().Add("leg_006");
		result.belt = HashCache.Get().Add("belt_006");
		result.pelvis = HashCache.Get().Add("pelvis_006");
		result.foot = HashCache.Get().Add("foot_006");
		result.hand = HashCache.Get().Add("hand_paint_006");
		result.cuff = HashCache.Get().Add("cuff_006");
		return result;
	}
}
