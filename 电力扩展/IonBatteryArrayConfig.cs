using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

namespace PowerExtension
{
    public class IonBatteryArrayConfig : BaseBatteryConfig
    {
        public const string ID = "IonBatteryArray";

        public override BuildingDef CreateBuildingDef()
        {

            int width = 3;
            int height = 4;

            int hitpoints = 30;
            string anim = "batterylg_kanim"; 
            float construction_time = 120f;

            bool hasIO = ElementLoader.GetElement("AIO_HardenedAlloy".ToTag()) != null;

            float[] construction_mass = new float[] { 1200f, 25f, 200f };
            string[] construction_materials = new string[] {
                "PowerExt_IonMetal",
                "Fullerene",
                hasIO ? "AIO_HardenedAlloy" : "Steel"
            };

            float melting_point = 800f;
            float exhaust_temperature_active = 0f;
            float self_heat_kilowatts_active = 32f;

            BuildingDef def = base.CreateBuildingDef(ID, width, height, hitpoints, anim, construction_time, construction_mass, construction_materials, melting_point, exhaust_temperature_active, self_heat_kilowatts_active, TUNING.BUILDINGS.DECOR.PENALTY.TIER2, TUNING.NOISE_POLLUTION.NOISY.TIER2);

            def.Overheatable = true;
            def.OverheatTemperature = 273.15f + 75f;

            def.RequiresPowerInput = true;
            def.RequiresPowerOutput = true;
            // def.UseWhitePowerOutputConnectorColour = true; // 移除：恢复默认的绿色输出口

            // 端口位置保持适配 3x4
            def.PowerInputOffset = new CellOffset(0, 0);
            def.PowerOutputOffset = new CellOffset(1, 2);

            def.GeneratorWattageRating = 20000f;
            def.GeneratorBaseCapacity = 20000f;

            // 逻辑端口
            def.LogicInputPorts = new List<LogicPorts.Port> {
                LogicPorts.Port.InputPort(
                    LogicOperationalController.PORT_ID,
                    new CellOffset(0, 2),
                    UI.LOGIC_PORTS.CONTROL_OPERATIONAL,
                    UI.LOGIC_PORTS.CONTROL_OPERATIONAL_ACTIVE,
                    UI.LOGIC_PORTS.CONTROL_OPERATIONAL_INACTIVE,
                    false,
                    false
                )
            };

            def.LogicOutputPorts = new List<LogicPorts.Port> {
                LogicPorts.Port.OutputPort(
                    IonBatterySmart.PORT_ID,
                    new CellOffset(1, 0),
                    STRINGS.BUILDINGS.PREFABS.BATTERYSMART.LOGIC_PORT,
                    STRINGS.BUILDINGS.PREFABS.BATTERYSMART.LOGIC_PORT_ACTIVE,
                    STRINGS.BUILDINGS.PREFABS.BATTERYSMART.LOGIC_PORT_INACTIVE,
                    true,
                    false
                )
            };

            def.AddSearchTerms(SEARCH_TERMS.POWER);
            def.AddSearchTerms(SEARCH_TERMS.BATTERY);
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            base.ConfigureBuildingTemplate(go, prefab_tag);
            go.AddOrGet<CopyBuildingSettings>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            IonBatterySmart battery = go.AddOrGet<IonBatterySmart>();
            battery.capacity = ModOptions.Instance.IonBatteryCapacity * 1000f;
            battery.joulesLostPerSecond = 3.333f;
            battery.powerSortOrder = 1000;
            // 先进离子电池列：建造完成即满电(即建即用)
            battery.refillOnNewBuilding = true;

            // 拆除工时 360 秒(默认 = 建造时间×0.5；customWorkTime>0 时覆盖)
            go.AddOrGet<Deconstructable>().customWorkTime = 360f;

            go.AddOrGet<IonBatteryTransformer>();
            go.AddOrGet<LogicOperationalController>();

            // [修改] 删除了 kbac.animScale *= 1.5f; 
            // 现在贴图不会被强制拉伸了
            // 2. [新增] 强制调整动画尺寸
            // ONI 的标准缩放是 0.005f。如果太大，试着改小，比如 0.0025f (一半大小)
            // 你需要根据实际视觉效果微调这个数字
            //KBatchedAnimController kbac = go.GetComponent<KBatchedAnimController>();
            //kbac.animScale *= 1.5f;
            base.DoPostConfigureComplete(go);
        }
    }
}