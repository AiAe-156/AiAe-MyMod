using UnityEngine;

namespace PowerExtension
{
    public class PowerTransformer2kWConfig : IBuildingConfig
    {
        public const string ID = "PowerTransformer2kW";

        public override BuildingDef CreateBuildingDef()
        {
            // [可修改] 变量定义区 - 方便你调整
            int width = 2;
            int height = 2;
            string anim = "transformer_medium_kanim"; // [修改] 你指定的动画 (注意：如果游戏里没这个动画会显示粉色方块)
            int hitpoints = 30;
            float construction_time = 30f;            // [修改] 建造耗时 30秒
            float[] construction_mass = new float[] { 100f, 100f }; // 消耗 100kg + 100kg
            string[] construction_materials = new string[] { "Metal", "RefinedMetal" }; // 金属矿物 + 精炼金属
            float melting_point = 800f;

            BuildingDef def = BuildingTemplates.CreateBuildingDef(
                ID,
                width, height,
                anim,
                hitpoints,
                construction_time, // 这里使用了上面定义的变量
                construction_mass,
                construction_materials,
                melting_point,
                BuildLocationRule.OnFloor,
                TUNING.BUILDINGS.DECOR.PENALTY.TIER1,
                TUNING.NOISE_POLLUTION.NOISY.TIER2
            );

            def.RequiresPowerInput = true;
            def.RequiresPowerOutput = true;
            // def.UseWhitePowerOutputConnectorColour = true; // 移除：恢复默认的绿色输出口

            // [端口位置修改]
            // (0,0)是左下, (0,1)是左上, (1,0)是右下, (1,1)是右上
            def.PowerInputOffset = new CellOffset(0, 1);  // 输入：左上
            def.PowerOutputOffset = new CellOffset(1, 0); // 输出：右下

            def.ViewMode = OverlayModes.Power.ID;
            def.AudioCategory = "Metal";
            def.ExhaustKilowattsWhenActive = 0.5f; // 产热
            def.SelfHeatKilowattsWhenActive = 1f;

            // [核心] 变压器参数
            def.GeneratorWattageRating = 2000f; // 2kW 输出限制
            def.GeneratorBaseCapacity = 2000f;  // 内部电池容量

            def.PermittedRotations = PermittedRotations.FlipH;
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
            go.AddComponent<RequireInputs>();
            BuildingDef def = go.GetComponent<Building>().Def;

            Battery battery = go.AddOrGet<Battery>();
            battery.powerSortOrder = 1000;
            battery.capacity = def.GeneratorWattageRating;
            battery.chargeWattage = def.GeneratorWattageRating;

            PowerTransformer transformer = go.AddComponent<PowerTransformer>();
            transformer.powerDistributionOrder = 9;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Object.DestroyImmediate(go.GetComponent<EnergyConsumer>());
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}