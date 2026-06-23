﻿using UnityEngine;

namespace PowerExtension
{
    public class MediumWireConfig : BaseWireConfig
    {
        public const string ID = "MediumWire";

        public override BuildingDef CreateBuildingDef()
        {
            string anim = "MediumWire_kanim";
            float construction_time = 3f;
            // 配方 = 原版绝缘导线桥的 2 倍：50kg 精炼金属 + 20kg 橡胶/塑料。
            float[] construction_mass = new float[2]
            {
                50f,
                20f
            };
            string[] construction_materials = new string[2]
            {
                "RefinedMetal",
                // “Rubber&Plastic” = 原版 4kW 橡胶导线同款合并类别，橡胶或塑料均可建造。
                TUNING.MATERIALS.RUBBER_OR_PLASTIC
            };
            float insulation = 0.05f;

            EffectorValues decor = new EffectorValues { amount = -5, radius = 1 };

            BuildingDef def = CreateBuildingDef(
                ID,
                anim,
                construction_time,
                construction_mass,
                insulation,
                noise: TUNING.NOISE_POLLUTION.NONE,
                decor: decor
            );

            def.MaterialCategory = construction_materials;
            def.BuildLocationRule = BuildLocationRule.Anywhere;
            return def;
        }

        public override string[] GetRequiredDlcIds()
        {
            // 需要 Aquatic DLC (DLC5) 提供橡胶原材料
            return DlcManager.DLC5;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            // [2026-05-28] U59 官方 4kW Max4000=5，本 mod 错位取 Max8kW=6（8kW）
            // 提供比官方高一档的中压导线。neededSize=Max8kW+1=7，
            // 由 PowerExtensionPatches 中的 wireGroups/bridgeGroups 扩展 patch 保证不越界。
            DoPostConfigureComplete((Wire.WattageRating)PowerExtensionPatches.ExtendedWattageRating.Max8kW, go);
        }
    }
}