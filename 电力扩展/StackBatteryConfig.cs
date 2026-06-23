using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

namespace PowerExtension
{
    public class StackBatteryConfig : BaseBatteryConfig
    {
        public const string ID = "StackBattery";

        public static readonly Tag STACK_TAG = new Tag("PowerExt_StackBattery");

        public override string[] GetRequiredDlcIds()
        {
            return DlcManager.EXPANSION1;
        }

        public override BuildingDef CreateBuildingDef()
        {
            int width = 3;
            int height = 2;
            int hitpoints = 100;
            string anim = "rocket_battery_pack_kanim";
            float construction_time = 60f;

            float capacityKJ = ModOptions.Instance.StackBatteryCapacity;
            float self_heat_kilowatts_active = capacityKJ * 0.02f;

            bool hasIO = ElementLoader.GetElement("AIO_HardenedAlloy".ToTag()) != null;
            float[] construction_mass = new float[] { 600f, 200f };
            string[] construction_materials = new string[]
            {
                "RefinedMetal",
                hasIO ? "AIO_HardenedAlloy" : "Steel"
            };

            float melting_point = 800f;
            float exhaust_temperature_active = 0f;

            EffectorValues decor = new EffectorValues { amount = -30, radius = 3 };

            BuildingDef def = base.CreateBuildingDef(
                ID, width, height, hitpoints, anim, construction_time,
                construction_mass, construction_materials, melting_point,
                exhaust_temperature_active, self_heat_kilowatts_active,
                decor, TUNING.NOISE_POLLUTION.NOISY.TIER1);

            def.BuildLocationRule = BuildLocationRule.OnFloorOrBuildingAttachPoint;
            def.AttachmentSlotTag = STACK_TAG;
            def.DefaultAnimState = "grounded";
            def.Overheatable = true;
            def.OverheatTemperature = 273.15f + 75f;
            def.Floodable = false;
            def.Entombable = false;
            def.AudioCategory = "Metal";

            def.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.OutputPort(
                    IonBatterySmart.PORT_ID,
                    new CellOffset(0, 1),
                    global::STRINGS.BUILDINGS.PREFABS.BATTERYSMART.LOGIC_PORT,
                    global::STRINGS.BUILDINGS.PREFABS.BATTERYSMART.LOGIC_PORT_ACTIVE,
                    global::STRINGS.BUILDINGS.PREFABS.BATTERYSMART.LOGIC_PORT_INACTIVE,
                    true,
                    false)
            };

            def.AddSearchTerms(SEARCH_TERMS.POWER);
            def.AddSearchTerms(SEARCH_TERMS.BATTERY);
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            base.ConfigureBuildingTemplate(go, prefab_tag);
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
            go.AddOrGet<CopyBuildingSettings>();
            go.AddOrGet<LoopingSounds>();
            go.AddOrGet<BuildingAttachPoint>().points = new BuildingAttachPoint.HardPoint[1]
            {
                new BuildingAttachPoint.HardPoint(new CellOffset(0, 2), STACK_TAG, null)
            };
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            IonBatterySmart battery = go.AddOrGet<IonBatterySmart>();
            float capacityJ = ModOptions.Instance.StackBatteryCapacity * 1000f;
            battery.capacity = capacityJ;
            battery.joulesLostPerSecond = capacityJ * 0.015f / 600f;
            battery.powerSortOrder = 1000;

            // 染色 + 动画：StackBatteryVisuals 负责 TintColour，
            // StackBatteryAnimSM 用 rocket_battery_pack 自带状态做工作动画。
            go.AddOrGet<StackBatteryVisuals>();
            go.AddOrGetDef<StackBatteryAnimSM.Def>();

            // 不调用 base.DoPostConfigureComplete：基类会加 PoweredActiveController
            // 而 rocket_battery_pack 图没有 on/off/working_loop 等状态会卡死。
        }
    }

    // 重型智能电池工作动画状态机：
    // rocket_battery_pack_kanim 没有 on/off 状态(PoweredActiveController 不适用)，
    // 但有 grounded / pre_ready_to_launch / ready_to_launch / pst_ready_to_launch 等。
    // 当电池接入电网且有电(Active)时播放 ready_to_launch 循环(类似原版蓄电池舱准备发射时的运行态)，
    // 无电或断开时回到 grounded 静态。
    public class StackBatteryAnimSM : GameStateMachine<StackBatteryAnimSM, StackBatteryAnimSM.Instance, IStateMachineTarget, StackBatteryAnimSM.Def>
    {
        public class Def : BaseDef { }

        public new class Instance : GameInstance
        {
            public Instance(IStateMachineTarget master, Def def) : base(master, def) { }
        }

        public State idle;
        public State active_pre;
        public State active;
        public State active_pst;

        public override void InitializeStates(out BaseState default_state)
        {
            default_state = idle;

            idle.PlayAnim("grounded", KAnim.PlayMode.Loop)
                .EventTransition(GameHashes.ActiveChanged, active_pre,
                    smi => smi.GetComponent<Operational>().IsActive);

            active_pre.PlayAnim("pre_ready_to_launch")
                .OnAnimQueueComplete(active);

            active.PlayAnim("ready_to_launch", KAnim.PlayMode.Loop)
                .EventTransition(GameHashes.ActiveChanged, active_pst,
                    smi => !smi.GetComponent<Operational>().IsActive);

            active_pst.PlayAnim("pst_ready_to_launch")
                .OnAnimQueueComplete(idle);
        }
    }

    // 染色组件：冷调蓝金属色。
    // TintColour 使用乘法混合 (finalColor = spriteColor * tint)，
    // 值 > 1 可能被渲染管线 clamp，所以使用 < 1 的值压低红绿通道。
    // 用 InvokeRepeating 每秒刷新，防止 PlayAnim 或其他系统重置。
    public class StackBatteryVisuals : KMonoBehaviour
    {
        private static readonly Color TINT = new Color(0.75f, 0.80f, 0.95f, 1f);
        private KBatchedAnimController kbac;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            kbac = GetComponent<KBatchedAnimController>();
            ApplyTint();
            InvokeRepeating(nameof(ApplyTint), 1f, 1f);
        }

        protected override void OnCleanUp()
        {
            CancelInvoke(nameof(ApplyTint));
            base.OnCleanUp();
        }

        private void ApplyTint()
        {
            if (kbac != null) kbac.TintColour = TINT;
        }
    }
}
