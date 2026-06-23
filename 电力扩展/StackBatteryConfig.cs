﻿using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

namespace PowerExtension
{
    // 堆叠蓄电池：以眼冒金星 DLC「蓄电池舱」为原型，改为可直接落地、可堆叠(下层即上层地基)、
    // 接入普通电网并具备智能电池自动化(充电阈值绿/红信号 + 侧栏激活范围滑块)的建筑。
    public class StackBatteryConfig : BaseBatteryConfig
    {
        public const string ID = "StackBattery";

        // 堆叠用的连接点标签：每个电池顶部暴露此标签，上层电池据此附着到下层之上。
        // 不使用 GameTags.Rocket，避免被当作火箭模块处理。
        public static readonly Tag STACK_TAG = new Tag("PowerExt_StackBattery");

        // 仅在眼冒金星 DLC 下提供(复用 rocket_battery_pack 贴图 + 太空电力科技)
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

            // 容量(千焦)由 ModOptions 配置，默认 100
            float capacityKJ = ModOptions.Instance.StackBatteryCapacity;

            // 发热 = 容量(千焦数) * 2%，单位千复制热/秒(kDTU/s)，即 100kJ -> 2 千复制热/秒
            float self_heat_kilowatts_active = capacityKJ * 0.02f;

            bool hasIO = ElementLoader.GetElement("AIO_HardenedAlloy".ToTag()) != null;
            float[] construction_mass = new float[] { 800f, 200f };
            string[] construction_materials = new string[]
            {
                "RefinedMetal",
                hasIO ? "AIO_HardenedAlloy" : "Steel"
            };

            float melting_point = 800f;
            float exhaust_temperature_active = 0f;

            // 装饰减益 = 智能电池(-15)的 2 倍
            EffectorValues decor = new EffectorValues { amount = -30, radius = 3 };

            BuildingDef def = base.CreateBuildingDef(
                ID, width, height, hitpoints, anim, construction_time,
                construction_mass, construction_materials, melting_point,
                exhaust_temperature_active, self_heat_kilowatts_active,
                decor, TUNING.NOISE_POLLUTION.NOISY.TIER1);

            // 落地 + 堆叠：可建在地板上，也可建在下层电池的连接点上
            def.BuildLocationRule = BuildLocationRule.OnFloorOrBuildingAttachPoint;
            def.AttachmentSlotTag = STACK_TAG;

            // 复用原版蓄电池舱的静态展示状态
            def.DefaultAnimState = "grounded";

            def.Overheatable = true;
            def.OverheatTemperature = 273.15f + 75f;

            // 允许建在水里、被淹不损毁
            def.Floodable = false;
            def.Entombable = false;

            def.AudioCategory = "Metal";

            // 智能电池自动化逻辑输出口，放在正中(0,0)
            def.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.OutputPort(
                    IonBatterySmart.PORT_ID,
                    new CellOffset(0, 0),
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

            // 不强制要求实体地基，使其可落在下层电池的连接点上、并可建在水里
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);

            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
            go.AddOrGet<CopyBuildingSettings>();

            // 顶部连接点：接受带 STACK_TAG 的建筑(即另一块堆叠蓄电池)附着，实现无上限堆叠
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
            // 漏电 = 容量 * 1.5% / 周期；周期=600秒 -> 焦/秒
            battery.joulesLostPerSecond = capacityJ * 0.015f / 600f;
            battery.powerSortOrder = 1000;

            // 调色 + 循环播放静置动画(实例级，见 StackBatteryVisuals)。
            go.AddOrGet<StackBatteryVisuals>();

            // 注意：不调用 base.DoPostConfigureComplete。
            // 基类会塞入 PoweredActiveController(按运转状态播 on/off 动画)，
            // 而 rocket_battery_pack 图里没有 on/off 状态，会把建筑卡在静止帧；
            // 原版「蓄电池舱」同样不带 PoweredActiveController，仅循环播放 grounded。
            // 电池功能由上面的 IonBatterySmart(继承 Battery) 提供，无需基类。
        }
    }

    // 重型智能电池的外观控制：在实例 OnSpawn 时(prefab 期设置会在实例化时丢失)
    // ① 把动画染成亮金属色；② 循环播放 grounded 静置动画，使其像原版火箭电池一样有动画。
    public class StackBatteryVisuals : KMonoBehaviour
    {
        // 微调色：仅略提亮 + 极淡冷色，呈抛光金属的轻微光泽，避免压暗偏黄。
        // 各通道 >1 表示在原贴图上提亮；想更明显可整体上调、想更淡可趋近 1.0。
        private static readonly Color TINT = new Color(1.08f, 1.10f, 1.16f);

        protected override void OnSpawn()
        {
            base.OnSpawn();
            ApplyVisuals();
            // spawn 期 KBatchedAnimController 会按 DefaultAnimState 单次播放，可能覆盖我们设的
            // grounded 循环并停在末帧(表现为静止)。下一帧再确保循环，保证 grounded 持续播放。
            GameScheduler.Instance.Schedule(
                "StackBatteryGroundedLoop", 0f, (_) => ApplyVisuals(), null, null);
        }

        private void ApplyVisuals()
        {
            KBatchedAnimController kbac = GetComponent<KBatchedAnimController>();
            if (kbac != null)
            {
                kbac.TintColour = TINT;
                kbac.Play("grounded", KAnim.PlayMode.Loop);
            }
        }
    }
}
